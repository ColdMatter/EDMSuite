"""Set-up that has to happen before numpy, scipy, matplotlib or pythonnet are
imported, so the GUI also works when started by double-clicking (pythonw.exe
outside an activated conda environment)."""

import os
import sys

# LatticeEDMScripts, where LatticeEDM_analysis_library.py and tools.py live
SCRIPTS_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))


def setup():
    # A conda environment that isn't activated doesn't have its DLL folders on
    # PATH, and scipy's compiled fits (curve_fit) then crash Python outright
    prefix = sys.prefix
    dll_dirs = [prefix,
                os.path.join(prefix, "Library", "mingw-w64", "bin"),
                os.path.join(prefix, "Library", "usr", "bin"),
                os.path.join(prefix, "Library", "bin"),
                os.path.join(prefix, "Scripts")]
    dll_dirs = [d for d in dll_dirs if os.path.isdir(d)]
    os.environ["PATH"] = os.pathsep.join(dll_dirs + [os.environ.get("PATH", "")])

    # Plots are drawn into Qt widgets; never let pyplot pick Tk
    os.environ.setdefault("MPLBACKEND", "QtAgg")

    if SCRIPTS_DIR not in sys.path:
        sys.path.insert(0, SCRIPTS_DIR)

    # On macOS, pythonnet needs pointing at Mono before anything imports clr
    if sys.platform == "darwin":
        from pythonnet import load
        load("mono", libmono="/Library/Frameworks/Mono.framework/Versions/Current/lib/libmonosgen-2.0.dylib")
