"""Main window: data folder at the top, one tab per kind of analysis, and the
log of everything printed at the bottom."""

import os
import sys
import traceback

from qtpy import QtCore, QtGui, QtWidgets

ORG, APP = "LatticeEDM", "AnalysisGUI"


class MainWindow(QtWidgets.QMainWindow):

    def __init__(self, log, qsettings=None):
        super().__init__()
        # Imported here: the library loads the .NET SharedCode, which takes a moment
        import LatticeEDM_analysis_library as EDM
        from analysis_gui import data
        from analysis_gui.modes import TABS
        self.EDM = EDM
        self.data = data

        self.setWindowTitle("LatticeEDM analysis")
        # Settings are kept in the registry (Windows) unless given, e.g. an .ini for tests
        self.qsettings = qsettings if qsettings is not None else QtCore.QSettings(ORG, APP)

        # ---- data folder
        bar = QtWidgets.QWidget()
        bar_layout = QtWidgets.QHBoxLayout(bar)
        bar_layout.setContentsMargins(6, 6, 6, 0)
        bar_layout.addWidget(QtWidgets.QLabel("Data folder"))
        self.folder = QtWidgets.QLineEdit()
        self.folder.setToolTip("Folder with the ScanMaster .zip files, usually a date, e.g. ...\\data\\Oct 2026\\26")
        self.folder.editingFinished.connect(self.folder_changed)
        bar_layout.addWidget(self.folder, 1)
        browse = QtWidgets.QPushButton("Browse...")
        browse.clicked.connect(self.browse)
        bar_layout.addWidget(browse)

        # ---- analysis tabs
        self.tabs = QtWidgets.QTabWidget()
        self.mode_tabs = []
        for cls in TABS:
            tab = cls(self.folder_text, self.qsettings)
            self.mode_tabs.append(tab)
            self.tabs.addTab(tab, cls.NAME)

        central = QtWidgets.QWidget()
        layout = QtWidgets.QVBoxLayout(central)
        layout.setContentsMargins(0, 0, 0, 0)
        layout.addWidget(bar)
        layout.addWidget(self.tabs, 1)
        self.setCentralWidget(central)

        # ---- log
        self.log = log
        dock = QtWidgets.QDockWidget("Log", self)
        dock.setObjectName("log")
        dock.setWidget(log)
        dock.setFeatures(QtWidgets.QDockWidget.DockWidgetMovable | QtWidgets.QDockWidget.DockWidgetFloatable |
                         QtWidgets.QDockWidget.DockWidgetClosable)
        self.addDockWidget(QtCore.Qt.BottomDockWidgetArea, dock)
        self.log_dock = dock

        # ---- menus
        file_menu = self.menuBar().addMenu("&File")
        act = file_menu.addAction("Choose data folder...", self.browse)
        act.setShortcut(QtGui.QKeySequence.Open)
        file_menu.addAction("Forget loaded files (free memory)", self.clear_cache)
        file_menu.addSeparator()
        act = file_menu.addAction("Quit", self.close)
        act.setShortcut(QtGui.QKeySequence.Quit)
        view_menu = self.menuBar().addMenu("&View")
        view_menu.addAction(dock.toggleViewAction())
        view_menu.addAction("Clear log", log.clear)
        help_menu = self.menuBar().addMenu("&Help")
        help_menu.addAction("About", self.about)

        # ---- restore
        folder = self.qsettings.value("folder", "")
        if not folder:
            try:
                folder = EDM.get_datadrive()
            except Exception as e:
                print("No data folder for this computer in data_locations.json: %s" % e)
                folder = ""
        self.folder.setText(str(folder))
        geometry = self.qsettings.value("geometry")
        if geometry is not None:
            self.restoreGeometry(geometry)
        else:
            self.resize(1500, 950)
        state = self.qsettings.value("state")
        if state is not None:
            self.restoreState(state)
        self.tabs.setCurrentIndex(int(self.qsettings.value("tab", 0) or 0))
        self.folder_changed()

    def folder_text(self):
        return os.path.normpath(self.folder.text().strip()) if self.folder.text().strip() else ""

    def browse(self):
        start = self.folder_text()
        while start and not os.path.isdir(start):
            start = os.path.dirname(start)
        folder = QtWidgets.QFileDialog.getExistingDirectory(self, "Data folder", start)
        if folder:
            self.folder.setText(os.path.normpath(folder))
            self.folder_changed()

    def folder_changed(self):
        self.qsettings.setValue("folder", self.folder_text())
        for tab in self.mode_tabs:
            tab.selector.refresh()

    def clear_cache(self):
        n = len(self.data.CACHE)
        self.data.CACHE.clear()
        print("Forgot %d loaded file(s); scans already in the tabs stay until they are reloaded" % n)

    def about(self):
        QtWidgets.QMessageBox.about(
            self, "LatticeEDM analysis",
            "Analysis of ScanMaster data with LatticeEDM_analysis_library.py.\n\n"
            "Duration, setpoint scan, 4-shot slowing and TOF collector analyses follow the "
            "*_SL.py scripts in 'analysis scripts'.\n\nSettings are remembered between sessions.")

    def closeEvent(self, event):
        for tab in self.mode_tabs:
            tab.cancel_load()
            tab.save()
        for tab in self.mode_tabs:
            if tab.worker is not None:
                tab.worker.wait(3000)
        self.qsettings.setValue("geometry", self.saveGeometry())
        self.qsettings.setValue("state", self.saveState())
        self.qsettings.setValue("tab", self.tabs.currentIndex())
        self.qsettings.setValue("folder", self.folder_text())
        super().closeEvent(event)


def _excepthook(exc_type, exc, tb):
    """Errors in Qt callbacks would otherwise vanish under pythonw"""
    text = "".join(traceback.format_exception(exc_type, exc, tb))
    print(text)
    app = QtWidgets.QApplication.instance()
    if app is not None:
        QtWidgets.QMessageBox.critical(None, "Error", "%s: %s\n\nSee the log for details." % (exc_type.__name__, exc))


def main():
    if hasattr(QtCore.Qt, "AA_EnableHighDpiScaling"):
        QtWidgets.QApplication.setAttribute(QtCore.Qt.AA_EnableHighDpiScaling, True)
        QtWidgets.QApplication.setAttribute(QtCore.Qt.AA_UseHighDpiPixmaps, True)
    app = QtWidgets.QApplication(sys.argv)
    app.setOrganizationName(ORG)
    app.setApplicationName(APP)

    from analysis_gui.widgets import LogConsole
    log = LogConsole()
    log.capture_output()
    sys.excepthook = _excepthook

    splash = QtWidgets.QLabel("  Starting LatticeEDM analysis (loading SharedCode)...  ")
    splash.setWindowFlags(QtCore.Qt.SplashScreen | QtCore.Qt.WindowStaysOnTopHint)
    splash.setStyleSheet("font-size: 14px; padding: 20px; background: white; border: 1px solid gray")
    splash.show()
    app.processEvents()
    try:
        window = MainWindow(log)
    except Exception:
        splash.close()
        QtWidgets.QMessageBox.critical(None, "LatticeEDM analysis",
                                       "Could not start:\n\n" + traceback.format_exc())
        sys.exit(1)
    splash.close()
    window.show()
    sys.exit(app.exec_())
