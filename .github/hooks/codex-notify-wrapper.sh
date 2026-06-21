#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="/Users/lukavincelj/Desktop/.NET_MVC"
ORIGINAL_NOTIFY="/Users/lukavincelj/.codex/computer-use/Codex Computer Use.app/Contents/SharedSupport/SkyComputerUseClient.app/Contents/MacOS/SkyComputerUseClient"

if [[ -d "$REPO_ROOT" ]]; then
  (
    cd "$REPO_ROOT"
    PROJECT_AGENT_LOG_REPO_ROOT="$REPO_ROOT" node .github/hooks/sync-codex-user-prompts.mjs ProjectAgentLogs/agent_log.txt >/dev/null 2>&1 || true
  )
fi

exec "$ORIGINAL_NOTIFY" "$@"
