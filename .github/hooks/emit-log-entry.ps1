param(
    [string]$prompt = $null,
    [string]$hook_event_name = "UserPromptSubmit",
    [string]$session_id = ([guid]::NewGuid()).ToString(),
    [string]$transcript_path = "",
    [string]$tool_name = "assistant_input",
    [string]$cwd = (Get-Location).ProviderPath,
    [string]$logPath = "C:\Users\Luka Vincelj\source\repos\.NET_MVC\lab-1\agent_log.txt",
    [switch]$AutoConfirm
)

# If no prompt argument provided, read from stdin
if (-not $prompt) {
    $prompt = [Console]::In.ReadToEnd().Trim()
    if ([string]::IsNullOrWhiteSpace($prompt)) {
        Write-Error "No prompt provided."; exit 1
    }
}

# Build tool_input
$tool_input = @{ prompt = $prompt }

# Timestamp in ISO 8601 UTC with milliseconds
$timestamp = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ")

$obj = @{
    timestamp = $timestamp
    hook_event_name = $hook_event_name
    session_id = $session_id
    transcript_path = $transcript_path
    tool_name = $tool_name
    tool_input = $tool_input
    tool_use_id = ([guid]::NewGuid()).ToString()
    cwd = $cwd
}

$jsonLine = $obj | ConvertTo-Json -Depth 10 -Compress

Write-Host "Prepared log entry:" -ForegroundColor Cyan
Write-Host $jsonLine -ForegroundColor Yellow

if (-not $AutoConfirm) {
    $answer = Read-Host "Append this to log file '$logPath'? (Y/N)"
    if ($answer -notmatch '^[Yy]') {
        Write-Host "Aborted. Entry not appended." -ForegroundColor Red
        exit 1
    }
}

$logDir = Split-Path -Parent $logPath
if ($logDir -and -not (Test-Path -LiteralPath $logDir)) {
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
}

Add-Content -Path $logPath -Value $jsonLine -Encoding utf8
Write-Host "Appended to $logPath" -ForegroundColor Green
