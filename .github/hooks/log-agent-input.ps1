param(
    [Parameter(Mandatory = $true)]
    [string]$EventName,

    [Parameter(Mandatory = $false)]
    [string]$LogPath = "lab-4\agent_log.txt"
)

$ErrorActionPreference = "Stop"

$logPath = [System.IO.Path]::GetFullPath($LogPath)
$logDir = Split-Path -Parent $logPath

if (-not (Test-Path -Path $logDir)) {
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
}

$payload = [Console]::In.ReadToEnd()

$entry = @{
    timestamp = (Get-Date).ToString("o")
    event = $EventName
    payload = $payload
}

$entryJson = $entry | ConvertTo-Json -Compress -Depth 8
Add-Content -Path $logPath -Value $entryJson

exit 0