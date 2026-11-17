# Put a "B-field GUI" shortcut on this PC's desktop.
#
# Right-click this file -> "Run with PowerShell", or from a terminal:
#   powershell -ExecutionPolicy Bypass -File make_shortcut.ps1
#
# The shortcut runs run\gui.py with this folder's own .venv, so set that
# up first (README.md, "Installing"). Running it again updates the
# shortcut, e.g. after the repository has moved.

param([switch]$NoPause)

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$pythonw = Join-Path $root '.venv\Scripts\pythonw.exe'
$python = Join-Path $root '.venv\Scripts\python.exe'

function Finish($code) {
    if (-not $NoPause) { Read-Host 'Press Enter to close' | Out-Null }
    exit $code
}

if (-not (Test-Path $pythonw)) {
    Write-Host "No .venv in $root -- set it up first:" -ForegroundColor Red
    Write-Host '  py -3.14 -m venv .venv'
    Write-Host '  .venv\Scripts\python -m pip install -e ".[gui]"'
    Finish 1
}
& $python -c "import PySide6, pyqtgraph" 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host 'The .venv has no GUI packages -- install them first:' -ForegroundColor Red
    Write-Host '  .venv\Scripts\python -m pip install -e ".[gui]"'
    Finish 1
}

$link = Join-Path ([Environment]::GetFolderPath('Desktop')) 'B-field GUI.lnk'
$had = Test-Path $link
$s = (New-Object -ComObject WScript.Shell).CreateShortcut($link)
$s.TargetPath = $pythonw          # pythonw: no console window behind the GUI
$s.Arguments = 'run\gui.py'
$s.WorkingDirectory = $root
$s.Description = 'B-field package GUI (LatticeEDMScripts/B-field)'
$s.IconLocation = "$pythonw,0"
$s.Save()

Write-Host ("{0} {1}" -f $(if ($had) { 'Updated' } else { 'Made' }), $link) -ForegroundColor Green
Write-Host "  runs $pythonw run\gui.py in $root"
Write-Host '  if the GUI ever does not appear, run .venv\Scripts\python run\gui.py in a terminal to see why'
Finish 0
