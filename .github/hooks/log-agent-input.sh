#!/usr/bin/env bash
# Usage: log-agent-input.sh <EventName> [LogPath]
EVENT_NAME="${1:-UnknownEvent}"
LOG_PATH="${2:-lab-5/agent_log.txt}"

LOG_DIR="$(dirname "$LOG_PATH")"
mkdir -p "$LOG_DIR"

PAYLOAD="$(cat)"
TIMESTAMP="$(date -u +"%Y-%m-%dT%H:%M:%SZ")"

# Escape payload for JSON (replace backslash, then quote, then newlines/tabs)
ESCAPED_PAYLOAD="$(printf '%s' "$PAYLOAD" \
    | sed 's/\\/\\\\/g' \
    | sed 's/"/\\"/g' \
    | tr '\n' ' ' \
    | tr '\t' ' ')"

ENTRY="{\"timestamp\":\"$TIMESTAMP\",\"event\":\"$EVENT_NAME\",\"payload\":\"$ESCAPED_PAYLOAD\"}"

printf '%s\n' "$ENTRY" >> "$LOG_PATH"

exit 0
