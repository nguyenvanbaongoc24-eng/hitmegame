<# Disposable positive/negative tests for manifest_gen.ps1. #>
[CmdletBinding()] param([string]$Generator=(Join-Path $PSScriptRoot '..\manifest_gen.ps1'))
$ErrorActionPreference='Stop';$Generator=(Resolve-Path $Generator).Path;$root=Join-Path $env:TEMP ("hitme-manifest-"+[guid]::NewGuid());$ok=0;$total=0
function Put($p,$v='x'){New-Item -ItemType Directory -Force (Split-Path $p)|Out-Null;Set-Content -NoNewline -Path $p -Value $v}
function Check($expected){$script:total++;& pwsh -NoProfile -File $Generator -RepoRoot $root -Check *> $null;$actual=$LASTEXITCODE;if($actual -eq $expected){$script:ok++}else{throw "expected $expected got $actual"}}
try{
  Put "$root\Evidence\a.txt" 'one';Put "$root\Evidence\photo.png" 'png';git -C $root init -q;git -C $root config user.email test@example.invalid;git -C $root config user.name test;git -C $root add .;git -C $root commit -qm initial
  & pwsh -NoProfile -File $Generator -RepoRoot $root -Overwrite *> $null;Check 0
  Put "$root\Evidence\new.txt" 'new';Check 1
  & pwsh -NoProfile -File $Generator -RepoRoot $root -Overwrite *> $null;Put "$root\Evidence\a.txt" 'changed';Check 1
  & pwsh -NoProfile -File $Generator -RepoRoot $root -Overwrite *> $null;Remove-Item "$root\Evidence\a.txt";Check 1
  & pwsh -NoProfile -File $Generator -RepoRoot $root -Overwrite *> $null;$hasNoProvenance=((Get-Content -Raw "$root\Evidence\MANIFEST.csv") -match 'NO_PROVENANCE');$script:total++;if($hasNoProvenance){$script:ok++}
  "Passed! $ok/$total";if($ok -ne $total){exit 1}
}finally{if(Test-Path $root){Remove-Item -Recurse -Force $root}}
