# Copilot Agent Instructions

## Automatic Logging

For EVERY response, log to `c:\Users\Luka Vincelj\source\repos\.NET_MVC\lab-2\agent_log.txt` using the terminal (run_in_terminal / Add-Content in PowerShell). Each entry uses the cleaner multi-line block format with a `---` separator.

### Format (use for ALL three event types)
```
[YYYY-MM-DD HH:mm:ss]
{
    "timestamp":  "<ISO8601 UTC>",
    "hook_event_name":  "<event>",
    "session_id":  "<session>",
    "transcript_path":  "",
    ... event-specific fields ...
    "cwd":  "c:\\Users\\Luka Vincelj\\source\\repos\\.NET_MVC"
}
---
```

### 1. Log the user prompt FIRST (before anything else)
Event-specific fields: `"prompt": "<user prompt text>"`

### 2. Log every tool call you make (PreToolUse)
Event-specific fields: `"tool_name": "<tool_name>"`, `"tool_input": { ... }`, `"tool_use_id": "<id>"`

### 3. Log your final text response (AssistantResponse)
Event-specific fields: `"response": "<your response text>"`

All three must happen automatically for every single user message — do not wait to be asked. Use a consistent session_id (e.g. a short UUID) per conversation.

---

## UX Sub-Agent Delegation

The main agent is responsible for generating the Hospital Management web application.
Whenever code that involves a **UI component, Razor view, CSS, or layout** needs to be created or modified, you MUST delegate that work to the UX sub-agent.

### Rules

1. **Mark the delegated prompt** — wrap the portion of the task being sent to the UX sub-agent inside a clearly labelled block:
   ```
   [UX-SUB-AGENT PROMPT]
   <describe exactly what UI/UX work the sub-agent must do>
   [/UX-SUB-AGENT PROMPT]
   ```

2. **Log the invocation** — immediately before or after marking the delegated block, append a `UxSubAgentInvoked` log entry to `lab-2/agent_log.txt` using the same multi-line block format:
   ```
   [YYYY-MM-DD HH:mm:ss]
   {
       "timestamp":  "<ISO8601 UTC>",
       "hook_event_name":  "UxSubAgentInvoked",
       "session_id":  "<session>",
       "transcript_path":  "",
       "reason":  "<brief description of what UI work is being delegated>",
       "cwd":  "c:\\Users\\Luka Vincelj\\source\\repos\\.NET_MVC"
   }
   ---
   ```

3. **Emit the info message** in your response text:
   ```
   [INFO] UX sub-agent invoked for UI generation
   ```

4. The UX sub-agent definition is at `.github/agents/ux-agent.agent.md`. Apply all design rules defined there (card layouts, sidebar nav, breadcrumbs, custom palette) when generating any view.
