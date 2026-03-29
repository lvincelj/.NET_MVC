param(
    [string]$HookEventName = "UserPromptSubmit",
    [string]$LogPath = "lab-1\\agent-log.txt"
)

# Read payload from stdin (the tool should pipe the prompt text)
$payload = [Console]::In.ReadToEnd()
if ([string]::IsNullOrWhiteSpace($payload)) {
    # If no payload, still write an empty JSON line to indicate the event
    $payload = ""
}

# Ensure log directory exists
$logFullPath = Join-Path -Path (Get-Location) -ChildPath $LogPath
$logDir = Split-Path -Parent $logFullPath
if ($logDir -and -not (Test-Path -LiteralPath $logDir)) {
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
}

# Load or create session id
$sessionFile = Join-Path -Path $logDir -ChildPath "session-id.txt"
if (Test-Path $sessionFile) {
    $sessionId = (Get-Content $sessionFile -ErrorAction SilentlyContinue).Trim()
    if (-not $sessionId) { $sessionId = [guid]::NewGuid().ToString(); Set-Content -Path $sessionFile -Value $sessionId -Encoding utf8 }
} else {
    $sessionId = [guid]::NewGuid().ToString()
    Set-Content -Path $sessionFile -Value $sessionId -Encoding utf8
}

# Compose JSON entry
$entry = @{ 
    timestamp = (Get-Date).ToUniversalTime().ToString('o')
    hook_event_name = $HookEventName
    session_id = $sessionId
    transcript_path = ""
    prompt = $payload.TrimEnd()
    cwd = (Get-Location).Path
}

$json = $entry | ConvertTo-Json -Compress
Add-Content -Path $logFullPath -Value $json -Encoding utf8
