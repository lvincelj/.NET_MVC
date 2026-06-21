#!/usr/bin/env node
import { createHash } from "node:crypto";
import { existsSync, mkdirSync, readFileSync, readdirSync, appendFileSync } from "node:fs";
import { homedir } from "node:os";
import { dirname, join, resolve } from "node:path";

const repoRoot = resolve(process.env.PROJECT_AGENT_LOG_REPO_ROOT ?? process.cwd());
const logPath = resolve(repoRoot, process.argv[2] ?? "ProjectAgentLogs/agent_log.txt");
const sessionsRoot = resolve(process.argv[3] ?? join(homedir(), ".codex", "sessions"));
const maxFieldChars = Number.parseInt(process.env.PROJECT_AGENT_LOG_MAX_FIELD_CHARS ?? "20000", 10);

function hash(value) {
  return createHash("sha256").update(value).digest("hex");
}

function truncateValue(value) {
  if (typeof value !== "string" || value.length <= maxFieldChars) {
    return value;
  }

  return `${value.slice(0, maxFieldChars)}\n...[truncated ${value.length - maxFieldChars} characters]`;
}

function makeLogKey(sessionId, event, timestamp, content) {
  return `${sessionId || ""}:${event}:${timestamp}:${hash(content)}`;
}

function listJsonlFiles(dir) {
  if (!existsSync(dir)) {
    return [];
  }

  return readdirSync(dir, { withFileTypes: true }).flatMap((entry) => {
    const fullPath = join(dir, entry.name);
    if (entry.isDirectory()) {
      return listJsonlFiles(fullPath);
    }

    return entry.isFile() && entry.name.endsWith(".jsonl") ? [fullPath] : [];
  });
}

function readLoggedKeys() {
  if (!existsSync(logPath)) {
    return new Set();
  }

  const keys = new Set();
  for (const line of readFileSync(logPath, "utf8").split(/\r?\n/).filter(Boolean)) {
    try {
      const entry = JSON.parse(line);
      const payload = typeof entry.payload === "string" ? JSON.parse(entry.payload) : entry.payload;
      if (payload?.source !== "codex") {
        continue;
      }

      if (payload.codex_log_key) {
        keys.add(payload.codex_log_key);
        continue;
      }

      if (entry.event === "UserPromptSubmit" && payload?.prompt) {
        keys.add(makeLogKey(payload.session_id, entry.event, payload.timestamp, payload.prompt));
      }
    } catch {
      continue;
    }
  }

  return keys;
}

function extractTextContent(content) {
  if (!Array.isArray(content)) {
    return "";
  }

  return content
    .filter((item) => item && (item.type === "input_text" || item.type === "text" || item.type === "output_text"))
    .map((item) => item.text ?? "")
    .join("\n")
    .trim();
}

function safeJsonParse(value) {
  if (typeof value !== "string") {
    return value ?? {};
  }

  try {
    return JSON.parse(value);
  } catch {
    return value;
  }
}

function buildEntry({ sessionId, sessionPath, sessionCwd, timestamp, event, payload }) {
  const contentForKey = JSON.stringify(payload);
  const codexLogKey = makeLogKey(sessionId, event, timestamp, contentForKey);
  const fullPayload = {
    timestamp,
    hook_event_name: event,
    session_id: sessionId,
    transcript_path: sessionPath,
    cwd: sessionCwd || repoRoot,
    source: "codex",
    codex_log_key: codexLogKey,
    ...payload,
  };

  return {
    key: codexLogKey,
    line: JSON.stringify({
      timestamp: new Date(timestamp).toISOString().replace(/\.\d{3}Z$/, "Z"),
      event,
      payload: JSON.stringify(fullPayload),
    }),
  };
}

function parseSessionFile(sessionPath) {
  const lines = readFileSync(sessionPath, "utf8").split(/\r?\n/).filter(Boolean);
  let sessionId = "";
  let sessionCwd = "";
  const records = [];
  const seenPrompts = new Set();
  const seenAssistantMessages = new Set();

  for (const line of lines) {
    let record;
    try {
      record = JSON.parse(line);
    } catch {
      continue;
    }

    if (record.type === "session_meta") {
      sessionId = record.payload?.id ?? sessionId;
      sessionCwd = record.payload?.cwd ?? sessionCwd;
      continue;
    }

    if (record.type === "event_msg" && record.payload?.type === "user_message") {
      const message = (record.payload.message ?? "").trim();
      if (!message) {
        continue;
      }

      const promptHash = hash(message);
      if (seenPrompts.has(promptHash)) {
        continue;
      }
      seenPrompts.add(promptHash);

      records.push({
        event: "UserPromptSubmit",
        timestamp: record.timestamp,
        payload: {
          client_id: record.payload.client_id,
          prompt: message,
        },
      });
      continue;
    }

    if (record.type === "event_msg" && record.payload?.type === "agent_message") {
      const message = (record.payload.message ?? "").trim();
      if (!message) {
        continue;
      }

      const messageHash = hash(`${record.payload.phase ?? ""}\n${message}`);
      if (seenAssistantMessages.has(messageHash)) {
        continue;
      }
      seenAssistantMessages.add(messageHash);

      records.push({
        event: "AgentMessage",
        timestamp: record.timestamp,
        payload: {
          phase: record.payload.phase,
          message,
        },
      });
      continue;
    }

    if (record.type === "response_item" && record.payload?.type === "message" && record.payload?.role === "user") {
      const message = extractTextContent(record.payload.content);
      if (!message || message.startsWith("<environment_context>")) {
        continue;
      }

      const promptHash = hash(message);
      if (seenPrompts.has(promptHash)) {
        continue;
      }
      seenPrompts.add(promptHash);

      records.push({
        event: "UserPromptSubmit",
        timestamp: record.timestamp,
        payload: {
          prompt: message,
        },
      });
      continue;
    }

    if (record.type === "response_item" && record.payload?.type === "message" && record.payload?.role === "assistant") {
      const message = extractTextContent(record.payload.content);
      if (!message) {
        continue;
      }

      const messageHash = hash(`${record.payload.phase ?? ""}\n${message}`);
      if (seenAssistantMessages.has(messageHash)) {
        continue;
      }
      seenAssistantMessages.add(messageHash);

      records.push({
        event: "AgentMessage",
        timestamp: record.timestamp,
        payload: {
          phase: record.payload.phase,
          message,
        },
      });
      continue;
    }

    if (record.type === "response_item" && record.payload?.type === "function_call") {
      records.push({
        event: "PreToolUse",
        timestamp: record.timestamp,
        payload: {
          tool_name: record.payload.name,
          tool_use_id: record.payload.call_id,
          tool_input: safeJsonParse(record.payload.arguments),
        },
      });
      continue;
    }

    if (record.type === "response_item" && record.payload?.type === "function_call_output") {
      records.push({
        event: "PostToolUse",
        timestamp: record.timestamp,
        payload: {
          tool_use_id: record.payload.call_id,
          tool_output: truncateValue(record.payload.output ?? ""),
        },
      });
    }
  }

  return { sessionId, sessionCwd, records };
}

mkdirSync(dirname(logPath), { recursive: true });
const loggedKeys = readLoggedKeys();
const entries = [];

for (const sessionPath of listJsonlFiles(sessionsRoot)) {
  const session = parseSessionFile(sessionPath);
  if (session.sessionCwd && resolve(session.sessionCwd) !== repoRoot) {
    continue;
  }

  for (const record of session.records) {
    const entry = buildEntry({
      sessionId: session.sessionId,
      sessionPath,
      sessionCwd: session.sessionCwd,
      timestamp: record.timestamp,
      event: record.event,
      payload: record.payload,
    });

    if (loggedKeys.has(entry.key)) {
      continue;
    }

    entries.push(entry.line);
    loggedKeys.add(entry.key);
  }
}

if (entries.length > 0) {
  appendFileSync(logPath, `${entries.join("\n")}\n`);
}

console.log(`Synced ${entries.length} Codex event(s) to ${logPath}`);
