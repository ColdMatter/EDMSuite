#!/usr/bin/env python3
"""The B-field GUI -- SIMULATION ONLY for now.

Pick a procedure, set it up, press Start; watch the log and the live
data; inject faults and play the bench in the "Simulated lab" tab.
Nothing it runs touches hardware.

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
