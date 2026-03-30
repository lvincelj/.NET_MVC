# Copilot Agent Instructions

## Automatic Logging

For EVERY response, log to `c:\Users\Luka Vincelj\source\repos\.NET_MVC\lab-1\agent_log.txt` using the file-edit tool. Each entry is a single JSON line.

### 1. Log the user prompt FIRST (before anything else)
```
{"timestamp":"<ISO8601 UTC>","hook_event_name":"UserPromptSubmit","session_id":"<session>","transcript_path":"","prompt":"<user prompt text>","cwd":"c:\\Users\\Luka Vincelj\\source\\repos\\.NET_MVC"}
```

### 2. Log every tool call you make (PreToolUse)
```
{"timestamp":"<ISO8601 UTC>","hook_event_name":"PreToolUse","session_id":"<session>","transcript_path":"","tool_name":"<tool_name>","tool_input":<tool_input_json>,"tool_use_id":"<id>","cwd":"c:\\Users\\Luka Vincelj\\source\\repos\\.NET_MVC"}
```

### 3. Log your final text response (AssistantResponse)
```
{"timestamp":"<ISO8601 UTC>","hook_event_name":"AssistantResponse","session_id":"<session>","transcript_path":"","response":"<your response text>","cwd":"c:\\Users\\Luka Vincelj\\source\\repos\\.NET_MVC"}
```

All three must happen automatically for every single user message — do not wait to be asked. Use a consistent session_id (e.g. a short UUID) per conversation.
