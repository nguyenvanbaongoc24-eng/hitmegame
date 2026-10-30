<#
.SYNOPSIS
Read-only Phase 2 release audit. The sole write is Evidence/audit/phase2_audit_<commit>.txt.
.EXAMPLE
pwsh -File tools/audit/phase2_audit.ps1 -RepoRoot D:\HITME GAME\unity-client -BuildDir D:\HITME GAME\unity-client\Builds\WebGL_20261029_942ee08
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$RepoRoot,
    [Parameter(Mandatory = $true)][string]$BuildDir
)

$ErrorActionPreference = 'Stop'
$RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
$BuildDir = (Resolve-Path -LiteralPath $BuildDir).Path
$rows = [System.Collections.Generic.List[object]]::new()
function Add-Result([string]$Check, [bool]$Pass, [string]$Detail) {
    $rows.Add([pscustomobject]@{ Check = $Check; Status = if ($Pass) { 'ĐẠT' } else { 'KHÔNG ĐẠT' }; Detail = $Detail })
}
function Add-Review([string]$Check, [string]$Detail) {
    $rows.Add([pscustomobject]@{ Check = $Check; Status = 'CẦN NGƯỜI XEM'; Detail = $Detail })
}
function Get-Relative([string]$Path) { return $Path.Substring($RepoRoot.Length).TrimStart('\','/') -replace '\\','/' }
function Has-File([string]$Relative) { Test-Path -LiteralPath (Join-Path $RepoRoot $Relative) }

try {
    $buildSettings = Join-Path $RepoRoot 'ProjectSettings/EditorBuildSettings.asset'
    $scenes = @(if (Test-Path $buildSettings) { Select-String -Path $buildSettings -Pattern 'path:.*\.unity' | ForEach-Object { $_.Line.Trim() } })
    $onlyGraybox = $scenes.Count -eq 1 -and $scenes[0] -match 'Graybox\.unity'
    Add-Result 'Build Settings chỉ Graybox' $onlyGraybox ($scenes -join '; ')

    $graybox = Join-Path $RepoRoot 'Assets/Scenes/Graybox.unity'
    $sceneText = if (Test-Path $graybox) { Get-Content -Raw $graybox } else { '' }
    Add-Result 'Graybox có MatchController, không MatchStateMachine' (($sceneText -match 'MatchController') -and ($sceneText -notmatch 'MatchStateMachine')) "scene=$graybox"

    $core = Join-Path $RepoRoot 'Assets/Scripts/Core/Phase1'
    $engineRefs = if (Test-Path $core) { @(Get-ChildItem $core -Recurse -Filter *.cs | Select-String -Pattern '^\s*using\s+UnityEngine') } else { @('core missing') }
    $asmdefs = @(Get-ChildItem (Join-Path $RepoRoot 'Assets') -Recurse -Filter *.asmdef -ErrorAction SilentlyContinue | Where-Object { (Get-Content -Raw $_.FullName) -match 'Phase1' })
    $noEngine = $asmdefs.Count -gt 0 -and (($asmdefs | ForEach-Object { Get-Content -Raw $_.FullName }) -join "`n") -match '"noEngineReferences"\s*:\s*true'
    Add-Result 'Core/Phase1 không UnityEngine và asmdef noEngineReferences' (($engineRefs.Count -eq 0) -and $noEngine) "engineRefs=$($engineRefs.Count); asmdefs=$($asmdefs.Count)"

    $scriptDirs = @('Assets/Scripts/Game','Assets/Scripts/UI') | ForEach-Object { Join-Path $RepoRoot $_ } | Where-Object { Test-Path $_ }
    $forbidden = 'Tint|magenta|OnGUI|Instantiate\(|Destroy\(|Shader\.Find|new\s+Material|\bpet\b|thú cưng'
    $forbiddenHits = @($scriptDirs | ForEach-Object { Get-ChildItem $_ -Recurse -Filter *.cs | Select-String -Pattern $forbidden })
    if ($forbiddenHits.Count) { Add-Review 'Chuỗi cần người xem trong Game/UI' (($forbiddenHits | Select-Object -First 10 | ForEach-Object { "$(Get-Relative $_.Path):$($_.LineNumber)" }) -join '; ') }
    else { Add-Result 'Chuỗi cần người xem trong Game/UI' $true 'Không có chuỗi cần rà soát.' }

    $hard = @($scriptDirs | ForEach-Object { Get-ChildItem $_ -Recurse -Filter *.cs | Select-String -Pattern '0\.45|0\.55|\b5f\b|9\.0|11\.4' })
    if ($hard.Count) { Add-Review 'Số cần rà soát ngoài GameConfig' (($hard | Select-Object -First 15 | ForEach-Object { "$(Get-Relative $_.Path):$($_.LineNumber)" }) -join '; ') }
    else { Add-Result 'Số cần rà soát ngoài GameConfig' $true 'Không có số trong danh sách rà soát.' }

    $evidence = Join-Path $RepoRoot 'Evidence'
    $xmls = @(Get-ChildItem $evidence -Recurse -Filter 'test-results*.xml' -ErrorAction SilentlyContinue)
    $xmlDetail = foreach ($xml in $xmls) { try { [xml]$x = Get-Content $xml; $r = $x.'test-run'; "$(Get-Relative $xml.FullName): total=$($r.total), pass=$($r.passed), fail=$($r.failed), skip=$($r.skipped)" } catch { "$(Get-Relative $xml.FullName): invalid XML" } }
    $badXml = @($xmlDetail | Where-Object { $_ -match 'fail=[1-9]|invalid XML' })
    Add-Result 'Kết quả XML không có fail' ($xmls.Count -gt 0 -and $badXml.Count -eq 0) ($xmlDetail -join '; ')

    $manifestPath = Join-Path $evidence 'MANIFEST.csv'
    $manifest = if (Test-Path $manifestPath) { @(Import-Csv $manifestPath) } else { @() }
    $manifestMap = @{}; foreach ($row in $manifest) { $manifestMap[$row.'tệp'] = $row }
    $files = @(Get-ChildItem $evidence -Recurse -File | Where-Object { $_.FullName -ne $manifestPath -and $_.FullName -notmatch '[\\/]audit[\\/]' })
    $missing = @($files | Where-Object { -not $manifestMap.ContainsKey((Get-Relative $_.FullName)) })
    $badHash = [System.Collections.Generic.List[string]]::new()
    $badCommit = [System.Collections.Generic.List[string]]::new()
    foreach ($file in $files) {
        $relative = Get-Relative $file.FullName
        if (-not $manifestMap.ContainsKey($relative)) { continue }
        $row = $manifestMap[$relative]
        if ($row.sha256 -and (Get-FileHash -Algorithm SHA256 -LiteralPath $file.FullName).Hash.ToLowerInvariant() -ne $row.sha256.ToLowerInvariant()) { $badHash.Add($relative) }
        if ($row.commit) { git -C $RepoRoot cat-file -e "$($row.commit)^{commit}" 2>$null; if ($LASTEXITCODE -ne 0) { $badCommit.Add("$relative=$($row.commit)") } }
    }
    Add-Result 'Manifest bao phủ Evidence, SHA và commit hợp lệ' (($missing.Count -eq 0) -and ($badHash.Count -eq 0) -and ($badCommit.Count -eq 0)) "missing=$($missing.Count); badHash=$($badHash.Count); badCommit=$($badCommit.Count)"

    $requiredBuild = @('index.html','Build/*.data','Build/*.framework.js','Build/*.loader.js','Build/*.wasm')
    $buildMissing = @($requiredBuild | Where-Object { @(Get-ChildItem -Path (Join-Path $BuildDir $_) -ErrorAction SilentlyContinue).Count -eq 0 })
    $buildFiles = @(Get-ChildItem $BuildDir -Recurse -File)
    $ageSpread = if ($buildFiles.Count) { (($buildFiles | Measure-Object LastWriteTime -Minimum -Maximum).Maximum - ($buildFiles | Measure-Object LastWriteTime -Minimum -Maximum).Minimum).TotalMinutes } else { 999 }
    $fingerprint = (Get-Content -Raw (Join-Path $BuildDir 'index.html') -ErrorAction SilentlyContinue) -match '(build|fingerprint|commit)'
    $lockfiles = @(Get-ChildItem $RepoRoot -Recurse -File -Filter 'UnityLockfile' -ErrorAction SilentlyContinue)
    Add-Result 'Build đủ tệp, cùng giờ, có dấu vân tay, không UnityLockfile' (($buildMissing.Count -eq 0) -and ($ageSpread -le 10) -and $fingerprint -and ($lockfiles.Count -eq 0)) "missing=$($buildMissing -join ','); spreadMin=$([math]::Round($ageSpread,2)); fingerprint=$fingerprint; lockfiles=$($lockfiles.Count)"

    $uiNames = @('res_1080x1920','res_1179x2556','res_1080x2400','res_1080x2520','res_1620x2160')
    $uiMissing = @($uiNames | Where-Object { @(Get-ChildItem $evidence -Recurse -File -Filter "$_*" -ErrorAction SilentlyContinue).Count -eq 0 })
    Add-Result 'Ảnh UI đủ năm độ phân giải' ($uiMissing.Count -eq 0) "missing=$($uiMissing -join ',')"

    $perf = @(Get-ChildItem $evidence -Recurse -File -ErrorAction SilentlyContinue | Where-Object { $_.Name -match 'gc|fps|profile' } | ForEach-Object { Get-Content -Raw $_.FullName }) -join "`n"
    $alloc = [regex]::Match($perf, 'ManagedAllocPerFrame\s*[:=]\s*([0-9]+)').Groups[1].Value
    $hasFps = $perf -match '(?i)fps.*(average|avg|minimum|min)'
    $has60 = $perf -match '(?i)(60\s*(seconds|giây)|>\s*60)'
    Add-Result 'GC/FPS WebGL >=60 giây' (($alloc -eq '0') -and $hasFps -and $has60) "ManagedAllocPerFrame=$alloc; fps=$hasFps; duration60=$has60"

    # Compare the release baseline to both committed history and the current worktree.
    $deletions = @(git -C $RepoRoot diff --name-only --diff-filter=D v0.1-graybox -- ASSET SFX Evidence 2>$null)
    Add-Result 'Không xóa ASSET/SFX/Evidence kể từ v0.1-graybox' ($deletions.Count -eq 0) ($deletions -join '; ')
    $docs = @('docs/PHASE2_REPORT.md','docs/01_GAMEPLAY_RULES.md')
    Add-Result 'Tài liệu Phase 2 và luật tồn tại trong repo' (@($docs | Where-Object { -not (Has-File $_) }).Count -eq 0) ($docs -join '; ')
}
catch {
    Add-Result 'Lỗi thực thi audit' $false $_.Exception.Message
}

$commit = (git -C $RepoRoot rev-parse --short HEAD).Trim()
$outDir = Join-Path $RepoRoot 'Evidence/audit'; New-Item -ItemType Directory -Force -Path $outDir | Out-Null
$output = Join-Path $outDir "phase2_audit_$commit.txt"
$rows | Format-Table -AutoSize | Out-String -Width 300 | Tee-Object -FilePath $output
if ($rows.Status | Where-Object { $_ -ne 'ĐẠT' }) { exit 1 }
