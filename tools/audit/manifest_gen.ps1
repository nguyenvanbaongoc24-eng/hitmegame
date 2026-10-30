<#
Generate: pwsh -File tools/audit/manifest_gen.ps1 -RepoRoot D:\HITME GAME\unity-client -BuildDir D:\HITME GAME\unity-client\Builds\WebGL_new
Check:    pwsh -File tools/audit/manifest_gen.ps1 -RepoRoot D:\HITME GAME\unity-client -BuildDir D:\HITME GAME\unity-client\Builds\WebGL_new -Check
Overwrite the handoff manifest only after reviewing MANIFEST.generated.csv: add -Overwrite.
#>
[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$RepoRoot, [string]$BuildDir, [switch]$Check, [switch]$Overwrite)
$ErrorActionPreference='Stop'; $RepoRoot=(Resolve-Path $RepoRoot).Path
$evidence=Join-Path $RepoRoot 'Evidence'; $target=Join-Path $evidence 'MANIFEST.csv'; $generated=Join-Path $evidence 'MANIFEST.generated.csv'
function Rel([string]$p){ if($p.StartsWith($RepoRoot,[StringComparison]::OrdinalIgnoreCase)){return $p.Substring($RepoRoot.Length).TrimStart('\','/') -replace '\\','/'}; return "Builds/$(Split-Path $p -Leaf)" }
function LastCommit([string]$relative){ $v=(git -C $RepoRoot log -1 --format=%h -- $relative 2>$null).Trim(); if($v){$v}else{'UNCOMMITTED'} }
$files=@(Get-ChildItem $evidence -Recurse -File | Where-Object {$_.Name -notin @('MANIFEST.csv','MANIFEST.generated.csv')})
if($BuildDir){ $b=(Resolve-Path $BuildDir).Path; $files+=@(Get-ChildItem $b -Recurse -File) }
$rows=foreach($file in $files){
  $relative=Rel $file.FullName; $json="$($file.FullName).json"; $p=$null; if(Test-Path $json){try{$p=Get-Content -Raw $json|ConvertFrom-Json}catch{}}
  [pscustomobject][ordered]@{'tệp'=$relative;sha256=(Get-FileHash -Algorithm SHA256 $file.FullName).Hash.ToLowerInvariant();commit=(LastCommit $relative);scene=$(if($p){$p.scene}else{''});'bản build (id/ngày)'=$(if($p){$p.build_id}else{''});'độ phân giải'=$(if($p){$p.resolution}else{''});'lệnh/AutoCapture tạo ra'=$(if($p){$p.command}else{'NO_PROVENANCE'});'người/công cụ'=$(if($p){$p.tool}else{'NO_PROVENANCE'});'kích thước'=$file.Length;'giờ ghi'=$file.LastWriteTimeUtc.ToString('O')}
}
if($Check){
  $old=if(Test-Path $target){@(Import-Csv $target)}else{@()}; $map=@{};foreach($r in $old){$map[$r.'tệp']=$r};$new=@{};foreach($r in $rows){$new[$r.'tệp']=$r}
  $missing=@($new.Keys|Where-Object{-not $map.ContainsKey($_)}).Count;$stale=@($map.Keys|Where-Object{-not $new.ContainsKey($_)}).Count;$badHash=@($new.Keys|Where-Object{$map.ContainsKey($_) -and $map[$_].sha256 -ne $new[$_].sha256}).Count
  "missing=$missing badHash=$badHash stale=$stale";if($missing -or $badHash -or $stale){exit 1};exit 0
}
$rows|Sort-Object 'tệp'|Export-Csv -NoTypeInformation -Encoding utf8 $generated
if($Overwrite){Copy-Item -Force $generated $target}
"Generated $generated ($($rows.Count) rows)"
