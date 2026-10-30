<# Runs a disposable negative-test matrix for phase2_audit.ps1. No repository is modified. #>
[CmdletBinding()]
param([string]$Audit = (Join-Path $PSScriptRoot '..\phase2_audit.ps1'), [string[]]$Only = @())
$ErrorActionPreference = 'Stop'
$Audit = (Resolve-Path $Audit).Path
$root = Join-Path ([IO.Path]::GetTempPath()) ("hitme-audit-" + [guid]::NewGuid())
$passed = 0; $total = 0
function Put([string]$Path, [string]$Text = 'x') { New-Item -ItemType Directory -Force (Split-Path $Path) | Out-Null; Set-Content -NoNewline -Path $Path -Value $Text }
function Hash([string]$Path) { (Get-FileHash -Algorithm SHA256 -LiteralPath $Path).Hash.ToLowerInvariant() }
function New-Fixture([string]$Name) {
    $r = Join-Path $root $Name; New-Item -ItemType Directory -Force $r | Out-Null
    Put "$r\ProjectSettings\EditorBuildSettings.asset" "path: Assets/Scenes/Graybox.unity"
    Put "$r\Assets\Scenes\Graybox.unity" 'MatchController'
    Put "$r\Assets\Scripts\Core\Phase1\Core.cs" 'namespace HitMe.Core {}'
    Put "$r\Assets\Phase1.asmdef" '{"name":"Phase1","noEngineReferences":true}'
    Put "$r\Assets\Scripts\Game\Safe.cs" 'class Safe {}'
    Put "$r\ASSET\keep.txt" 'keep'; Put "$r\SFX\keep.txt" 'keep'
    Put "$r\docs\PHASE2_REPORT.md" '# report'; Put "$r\docs\01_GAMEPLAY_RULES.md" '# rules'
    Put "$r\Evidence\test-results.xml" '<test-run total="1" passed="1" failed="0" skipped="0" />'
    Put "$r\Evidence\gc_60s_profile.txt" 'ManagedAllocPerFrame=0 FPS average 60 minimum 55 measured 60 seconds'
    foreach($n in 'res_1080x1920','res_1179x2556','res_1080x2400','res_1080x2520','res_1620x2160') { Put "$r\Evidence\$n.png" 'png' }
    $b = "$r\Builds\test"; Put "$b\index.html" '<!-- build fingerprint -->'; Put "$b\Build\game.data" 'data'; Put "$b\Build\game.framework.js" 'js'; Put "$b\Build\game.loader.js" 'loader'; Put "$b\Build\game.wasm" 'wasm'
    git -C $r init -q; git -C $r config user.email audit@example.invalid; git -C $r config user.name audit
    git -C $r add .; git -C $r commit -qm baseline; git -C $r tag v0.1-graybox
    $entries = @(); Get-ChildItem "$r\Evidence" -Recurse -File | ForEach-Object { $entries += [pscustomobject]@{'tệp'=$_.FullName.Substring($r.Length+1) -replace '\\','/';sha256=(Hash $_.FullName);commit=(git -C $r rev-parse --short HEAD);scene='Assets/Scenes/Graybox.unity';'bản build (id/ngày)'='test';'độ phân giải'='';'lệnh/AutoCapture tạo ra'='test';'người/công cụ'='test'} }
    $entries | Export-Csv -NoTypeInformation -Encoding utf8 "$r\Evidence\MANIFEST.csv"
    return $r
}
function Run-Case([string]$Name, [scriptblock]$Mutate, [bool]$ExpectZero, [string]$Needle) {
    if ($Only.Count -and $Name -notin $Only) { return }
    $script:total++; $r = New-Fixture $Name; & $Mutate $r
    & pwsh -NoProfile -File $Audit -RepoRoot $r -BuildDir "$r\Builds\test" *> $null; $code=$LASTEXITCODE
    $out = Get-Content (Get-ChildItem "$r\Evidence\audit" -Filter '*.txt' | Select-Object -Last 1) -Raw
    $ok = (($code -eq 0) -eq $ExpectZero) -and ([string]::IsNullOrEmpty($Needle) -or $out -match [regex]::Escape($Needle))
    if($ok){$script:passed++} else { Write-Output $out; Write-Error "${Name}: exit=$code, required=$Needle" }
}
try {
    Run-Case 'valid' {} $true ''
    Run-Case 'second-scene' {param($r) Add-Content "$r\ProjectSettings\EditorBuildSettings.asset" "`npath: Assets/Scenes/Other.unity"} $false 'Build Settings'
    Run-Case 'engine' {param($r) Add-Content "$r\Assets\Scripts\Core\Phase1\Core.cs" "`nusing UnityEngine;"} $false 'Core/Phase1'
    Run-Case 'forbidden' {param($r) Add-Content "$r\Assets\Scripts\Game\Safe.cs" 'void OnGUI(){}'} $false 'CẦN NGƯỜI XEM'
    Run-Case 'xml-fail' {param($r) Put "$r\Evidence\test-results.xml" '<test-run total="1" passed="0" failed="1" skipped="0" />'} $false 'Kết quả XML'
    Run-Case 'manifest-missing' {param($r) Put "$r\Evidence\unlisted.txt" 'x'} $false 'Manifest'
    Run-Case 'manifest-hash' {param($r) Add-Content "$r\Evidence\gc_60s_profile.txt" 'changed'} $false 'Manifest'
    Run-Case 'manifest-commit' {param($r) $m=Import-Csv "$r\Evidence\MANIFEST.csv"; $m[0].commit='deadbeef'; $m|Export-Csv -NoTypeInformation -Encoding utf8 "$r\Evidence\MANIFEST.csv"} $false 'Manifest'
    Run-Case 'wasm-missing' {param($r) Remove-Item "$r\Builds\test\Build\game.wasm"} $false 'Build đủ'
    Run-Case 'lock' {param($r) Put "$r\UnityLockfile" 'lock'} $false 'Build đủ'
    Run-Case 'ui-missing' {param($r) Remove-Item "$r\Evidence\res_1080x2400.png"} $false 'Ảnh UI'
    Run-Case 'gc' {param($r) Put "$r\Evidence\gc_60s_profile.txt" 'ManagedAllocPerFrame=460 FPS average 60 minimum 55 measured 60 seconds'} $false 'GC/FPS'
    Run-Case 'deleted-asset' {param($r) Remove-Item "$r\ASSET\keep.txt"} $false 'Không xóa'
    Run-Case 'missing-doc' {param($r) Remove-Item "$r\docs\PHASE2_REPORT.md"} $false 'Tài liệu'
    "Passed! $passed/$total"
    if($passed -ne $total){exit 1}
} finally { Remove-Item -Recurse -Force $root -ErrorAction SilentlyContinue }
