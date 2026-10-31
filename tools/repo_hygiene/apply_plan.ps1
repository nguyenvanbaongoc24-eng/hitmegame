param([switch]$Apply)
# Dry-run by default. -Apply needs the project owner's review first.
# Never removes local asset bytes; git rm --cached only changes Git's index.
$repoRoot = (git rev-parse --show-toplevel).Trim()
if (-not $repoRoot) { throw 'Not inside a Git worktree.' }
$tracked = @(git -C $repoRoot ls-files)
$buildFiles = @($tracked | Where-Object { $_ -match '(^|/)(WebGLBuild|WindowsBuild|Builds)/' })
$reports = @($tracked | Where-Object { $_ -match '(^|/)PHASE2_REPORT\.md$' })
Write-Output "Repository: $repoRoot"
Write-Output "Tracked build files proposed for untracking: $($buildFiles.Count)"
foreach ($path in $buildFiles) { Write-Output "  git rm --cached -- '$path'" }
Write-Output 'Proposed .gitignore entries: **/WebGLBuild/ **/WindowsBuild/ **/Builds/'
Write-Output 'Proposed LFS patterns for NEW commits only: *.png *.jpg *.psd *.wav *.mp3 *.ogg'
Write-Output 'No git lfs migrate and no history rewrite.'
Write-Output 'PHASE2_REPORT.md candidates (compare hashes before selecting a docs copy):'
foreach ($path in $reports) {
    $full = Join-Path $repoRoot $path
    if (Test-Path -LiteralPath $full) {
        $item = Get-Item -LiteralPath $full
        $sha = (Get-FileHash -LiteralPath $full -Algorithm SHA256).Hash
        Write-Output "  $path | $sha | $($item.LastWriteTimeUtc.ToString('o'))"
    }
}
Write-Output 'Do not move MD/ or choose a PHASE2 report automatically.'
if (-not $Apply) { Write-Output 'DRY RUN: no Git index or file changes made.'; exit 0 }
if ((git -C $repoRoot status --porcelain).Count -gt 0) { throw 'Working tree is dirty; apply refused.' }
if (-not (Get-Command git-lfs -ErrorAction SilentlyContinue)) { throw 'Git LFS not installed; apply refused.' }
foreach ($path in $buildFiles) {
    # ls-files supplies repo-relative paths; verify again before mutating index.
    $absolute = [IO.Path]::GetFullPath((Join-Path $repoRoot $path))
    if (-not $absolute.StartsWith($repoRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { throw "Outside repo: $path" }
    git -C $repoRoot rm --cached -- $path
    if ($LASTEXITCODE -ne 0) { throw "git rm --cached failed: $path" }
}
@('**/WebGLBuild/', '**/WindowsBuild/', '**/Builds/') | Add-Content -LiteralPath (Join-Path $repoRoot '.gitignore')
git -C $repoRoot lfs track '*.png' '*.jpg' '*.psd' '*.wav' '*.mp3' '*.ogg'
if ($LASTEXITCODE -ne 0) { throw 'git lfs track failed.' }
Write-Output 'Index and ignore rules updated. Review git diff --cached before committing.'
