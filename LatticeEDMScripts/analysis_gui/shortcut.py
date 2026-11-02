"""Makes a desktop shortcut that starts the analysis GUI with this Python.

Run it once per computer with the Python you analyse data with, e.g. from
Spyder's console:  !python -m analysis_gui.shortcut
or from a terminal in LatticeEDMScripts:
    <path to python.exe> -m analysis_gui.shortcut
Windows only."""

import os
import subprocess
import sys

from analysis_gui.env import SCRIPTS_DIR

NAME = "LatticeEDM Analysis"


def make_shortcut():
    if sys.platform != "win32":
        print("Desktop shortcuts are only made on Windows. Start the GUI with:\n"
              "  %s -m analysis_gui   (from %s)" % (sys.executable, SCRIPTS_DIR))
        return None
    pythonw = os.path.join(os.path.dirname(sys.executable), "pythonw.exe")
    if not os.path.isfile(pythonw):
        pythonw = sys.executable
    launcher = os.path.join(SCRIPTS_DIR, NAME + ".pyw")
    ps = (
        "$desktop = [Environment]::GetFolderPath('Desktop');"
        "$path = Join-Path $desktop '{name}.lnk';"
        "$s = (New-Object -ComObject WScript.Shell).CreateShortcut($path);"
        "$s.TargetPath = '{target}';"
        "$s.Arguments = '\"{launcher}\"';"
        "$s.WorkingDirectory = '{workdir}';"
        "$s.Description = 'LatticeEDM data analysis';"
        "$s.Save();"
        "Write-Output $path"
    ).format(name=NAME, target=pythonw.replace("'", "''"),
             launcher=launcher.replace("'", "''"), workdir=SCRIPTS_DIR.replace("'", "''"))
    out = subprocess.run(["powershell", "-NoProfile", "-Command", ps],
                         capture_output=True, text=True, check=True)
    path = out.stdout.strip()
    print("Shortcut made: %s\n  runs %s \"%s\"" % (path, pythonw, launcher))
    return path


if __name__ == "__main__":
    make_shortcut()
