param(
    [string]$LogPath = "C:\Users\Luka Vincelj\source\repos\.NET_MVC\lab-1\agent_log.txt"
)

$payload = [Console]::In.ReadToEnd()
if ([string]::IsNullOrWhiteSpace($payload)) {
    exit 0
}

$logDir = Split-Path -Parent $LogPath
if ($logDir -and -not (Test-Path -LiteralPath $logDir)) {
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
}

Add-Content -Path $LogPath -Value $payload -Encoding utf8
param(
    [string]$prompt = $null,
    [string]$LogPath = "C:\Users\Luka Vincelj\source\repos\.NET_MVC\lab-1\agent_log.txt",
    [switch]$AutoConfirm
)

# If -prompt not provided, read stdin
if (-not $prompt) {
    $prompt = [Console]::In.ReadToEnd().Trim()
    if ([string]::IsNullOrWhiteSpace($prompt)) { exit 0 }
}

# If not auto-confirmed, ask user (keeps interactive use possible)
if (-not $AutoConfirm) {
    Write-Host "Prepared payload to append:" -ForegroundColor Cyan
    Write-Host $prompt -ForegroundColor Yellow
    $ans = Read-Host "Append this to '$LogPath'? (Y/N)"
    if ($ans -notmatch '^[Yy]') {
        Write-Host "Aborted. Entry not appended." -ForegroundColor Red
        exit 1
    }
}

$logDir = Split-Path -Parent $LogPath
if ($logDir -and -not (Test-Path -LiteralPath $logDir)) {
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
}

# Append the raw payload (one or multiple lines) as-is
Add-Content -Path $LogPath -Value $prompt -Encoding utf8
Write-Host "Appended to $LogPath" -ForegroundColor Green
