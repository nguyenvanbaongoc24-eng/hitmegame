param(
    [int]$HubPort = 8100,
    [int]$ServerPort = 8080,
    [string]$Build,
    [switch]$Open
)
$ErrorActionPreference = 'Stop'
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$builds = 'D:\HITME GAME\unity-client\Builds'
if (-not $Build) {
    $candidate = Get-ChildItem -LiteralPath $builds -Directory -Filter 'WebGL_*' |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if (-not $candidate) { throw "No WebGL build found in $builds" }
    $Build = $candidate.FullName
}
$Build = [IO.Path]::GetFullPath($Build)
if (-not (Test-Path -LiteralPath (Join-Path $Build 'index.html'))) { throw "Build has no index.html: $Build" }
$parts = Get-ChildItem -LiteralPath (Join-Path $Build 'Build') -File
foreach ($suffix in @('.data', '.wasm', '.framework.js', '.loader.js')) {
    if (-not ($parts | Where-Object { $_.Name.EndsWith($suffix) -or $_.Name.EndsWith("$suffix.gz") -or $_.Name.EndsWith("$suffix.br") })) {
        throw "Incomplete WebGL build: missing $suffix"
    }
}
if ($HubPort -lt 1 -or $HubPort -gt 65535 -or $ServerPort -lt 1 -or $ServerPort -gt 65535 -or $HubPort -eq $ServerPort) { throw 'Invalid or duplicate ports.' }
function Test-PortFree([int]$port) {
    $listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, $port)
    try { $listener.Start(); return $true } catch { return $false } finally { $listener.Stop() }
}
if (-not (Test-PortFree $HubPort) -or -not (Test-PortFree $ServerPort)) { throw 'Hub or server port is in use. Try -HubPort and -ServerPort with free ports.' }
$ips = @(Get-NetIPAddress -AddressFamily IPv4 -ErrorAction SilentlyContinue |
    Where-Object { $_.IPAddress -notmatch '^(127\.|169\.254\.|0\.)' } |
    Select-Object -ExpandProperty IPAddress -Unique)
$origins = @("http://localhost:$HubPort", "http://127.0.0.1:$HubPort") + @($ips | ForEach-Object { "http://$_`:$HubPort" })
$env:ALLOWED_ORIGINS = $origins -join ','
$env:PORT = [string]$ServerPort
$env:HITME_HUB_PORT = [string]$HubPort
$env:HITME_SERVER_PORT = [string]$ServerPort
$env:HITME_BUILD_DIR = $Build
$env:HITME_SERVER_COMMIT = (git -C $repo rev-parse --short HEAD).Trim()
$runtime = Join-Path $PSScriptRoot '.runtime'
$published = Join-Path $runtime 'server'
New-Item -ItemType Directory -Path $published -Force | Out-Null
Write-Host 'Publishing .NET server Release...'
& dotnet publish (Join-Path $repo 'server\HitMe.Server.csproj') -c Release -o $published --nologo -v quiet
if ($LASTEXITCODE -ne 0) { throw 'Server publish failed.' }
$serverProcess = $null
$hubProcess = $null
try {
    $serverDll = Join-Path $published 'HitMe.Server.dll'
    $serverProcess = Start-Process -FilePath 'dotnet' -ArgumentList "`"$serverDll`"" -PassThru -WindowStyle Hidden -RedirectStandardOutput (Join-Path $runtime 'server.out.log') -RedirectStandardError (Join-Path $runtime 'server.err.log')
    $healthy = $false
    for ($i = 0; $i -lt 100; $i++) {
        if ($serverProcess.HasExited) { throw 'Server exited during startup; see tools/dev/.runtime/server.err.log.' }
        try { $healthy = (Invoke-WebRequest -Uri "http://127.0.0.1:$ServerPort/healthz" -UseBasicParsing -TimeoutSec 1).StatusCode -eq 200 } catch { }
        if ($healthy) { break }
        Start-Sleep -Milliseconds 150
    }
    if (-not $healthy) { throw 'Server /healthz did not become ready.' }
    $hostScript = Join-Path $PSScriptRoot 'static_host.mjs'
    $hubProcess = Start-Process -FilePath 'node' -ArgumentList "`"$hostScript`"" -PassThru -WindowStyle Hidden -RedirectStandardOutput (Join-Path $runtime 'hub.out.log') -RedirectStandardError (Join-Path $runtime 'hub.err.log')
    $hubReady = $false
    for ($i = 0; $i -lt 100; $i++) {
        if ($hubProcess.HasExited) { throw 'Hub exited during startup; see tools/dev/.runtime/hub.err.log.' }
        try { $hubReady = (Invoke-WebRequest -Uri "http://127.0.0.1:$HubPort/config.json" -UseBasicParsing -TimeoutSec 1).StatusCode -eq 200 } catch { }
        if ($hubReady) { break }
        Start-Sleep -Milliseconds 150
    }
    if (-not $hubReady) { throw 'Hub did not become ready.' }
    Write-Host "Build: $Build"
    Write-Host "Server commit: $env:HITME_SERVER_COMMIT | protocol v1"
    Write-Host "Computer: http://localhost:$HubPort/"
    foreach ($ip in $ips) { Write-Host "Phone on same Wi-Fi: http://$ip`:$HubPort/" }
    if ($ips.Count -gt 1) { Write-Host 'Several IPv4 adapters found; use the IP on the phone Wi-Fi/LAN.' }
    Write-Host "If LAN is blocked, run as Administrator: netsh advfirewall firewall add rule name=`"HITME Local Test`" dir=in action=allow protocol=TCP localport=$HubPort"
    Write-Host 'Remove later: netsh advfirewall firewall delete rule name="HITME Local Test"'
    Write-Host 'Ctrl+C stops both child processes.'
    if ($Open) { Start-Process "http://localhost:$HubPort/" | Out-Null }
    while (-not $serverProcess.HasExited -and -not $hubProcess.HasExited) { Start-Sleep -Milliseconds 500 }
    throw 'A child process exited; inspect tools/dev/.runtime/*.err.log.'
} finally {
    foreach ($process in @($hubProcess, $serverProcess)) {
        if ($process -and -not $process.HasExited) { Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue }
    }
}
