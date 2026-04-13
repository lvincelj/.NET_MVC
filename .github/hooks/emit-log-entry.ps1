param(
    [string]$LogPath = "C:\Users\Luka Vincelj\source\repos\.NET_MVC\lab-2\agent_log.txt"
)

$payload = $input | Out-String
if ([string]::IsNullOrWhiteSpace($payload)) { exit 0 }

$logDir = Split-Path -Parent $LogPath
if ($logDir -and -not (Test-Path -LiteralPath $logDir)) {
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
}

Add-Content -Path $LogPath -Value $payload -Encoding utf8
