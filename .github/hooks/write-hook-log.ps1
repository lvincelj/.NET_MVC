param(
    [string]$HookEventName = "UserPromptSubmit",
    [string]$LogPath = "C:\Users\Luka Vincelj\source\repos\.NET_MVC\lab-1\agent-log.txt"
)

# Read payload from stdin
$payload = [Console]::In.ReadToEnd()
if ([string]::IsNullOrWhiteSpace($payload)) { exit 0 }

# Ensure log directory exists
$logDir = Split-Path -Parent $LogPath
if ($logDir -and -not (Test-Path -LiteralPath $logDir)) { New-Item -ItemType Directory -Path $logDir -Force | Out-Null }

# Maintain or create session id
$sessionFile = Join-Path -Path $logDir -ChildPath "session-id.txt"
if (Test-Path $sessionFile) {
    $sessionId = (Get-Content $sessionFile -ErrorAction SilentlyContinue).Trim()
    if (-not $sessionId) { $sessionId = [guid]::NewGuid().ToString(); Set-Content -Path $sessionFile -Value $sessionId -Encoding utf8 }
} else {
    $sessionId = [guid]::NewGuid().ToString()
    Set-Content -Path $sessionFile -Value $sessionId -Encoding utf8
}

# Compose JSON log entry and append
$entry = @{ 
    timestamp = (Get-Date).ToUniversalTime().ToString('o')
    hook_event_name = $HookEventName
    session_id = $sessionId
    transcript_path = ""
    prompt = $payload.TrimEnd()
    cwd = (Get-Location).Path
}

$json = $entry | ConvertTo-Json -Compress
Add-Content -Path $LogPath -Value $json -Encoding utf8
