# publish_web_dist.ps1 — Wrapper to run publish_web_dist.py
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
python "$ScriptDir\publish_web_dist.py"
