#!/usr/bin/env python3
"""The B-field GUI.

Pick a procedure, set it up, press Start; watch the log and the live
data. Lab "Simulated" (the default) touches no hardware: inject faults
and play the bench in the "Simulated lab" tab. Lab "Bench" (asks first)
drives the real QZFM sensors and degausser from the QZFM and Degauss
tabs, as run/qzfm.py and run/degauss.py do; Relaxation and the S(f)
sweep stay simulator-only in the GUI.

  python run/gui.py
"""

import _path  # noqa: F401

import sys


def main():
    from PySide6.QtWidgets import QApplication

    from bfield.gui.main_window import MainWindow
    app = QApplication(sys.argv)
    win = MainWindow()
    win.show()
    return app.exec()


if __name__ == "__main__":
    raise SystemExit(main())
