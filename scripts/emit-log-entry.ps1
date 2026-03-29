param(
  [Parameter(Mandatory=$false, ValueFromPipeline=$true)] [string] $prompt,
  [Parameter(Mandatory=$true)] [string] $hook_event_name
)

# Read from stdin if no prompt argument was provided
if (-not $prompt) {
  $prompt = [Console]::In.ReadToEnd()
}

$scriptRoot = $PSScriptRoot
$logPath = Join-Path -Path $scriptRoot -ChildPath '..\lab-1\agent-log.txt'
$logPath = (Resolve-Path -Path $logPath).Path

$sessionFile = Join-Path -Path $scriptRoot -ChildPath '..\lab-1\session-id.txt'
if (Test-Path $sessionFile) {
  $sessionId = (Get-Content $sessionFile -ErrorAction SilentlyContinue).Trim()
  if (-not $sessionId) { $sessionId = [guid]::NewGuid().ToString(); Set-Content -Path $sessionFile -Value $sessionId }
} else {
  $sessionId = [guid]::NewGuid().ToString()
  Set-Content -Path $sessionFile -Value $sessionId
}

$cwd = (Get-Location).Path
$transcriptsDir = Join-Path $env:APPDATA 'Code\User\workspaceStorage'
$transcriptPath = ''

$entry = @{ 
    timestamp = (Get-Date).ToUniversalTime().ToString('o')
    hook_event_name = $hook_event_name
    session_id = $sessionId
    transcript_path = $transcriptPath
    prompt = $prompt.TrimEnd()
    cwd = $cwd
}

$json = ($entry | ConvertTo-Json -Compress)
Add-Content -Path $logPath -Value $json
