"""Starts the LatticeEDM analysis GUI without a console window.
Double-click it, or use the desktop shortcut made by
    python -m analysis_gui.shortcut
(run with the Python you analyse data with, e.g. Spyder's)."""

import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from analysis_gui import env
env.setup()

from analysis_gui.app import main

main()
