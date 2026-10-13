"""Figure style, export to a file, copy to the clipboard.

The style (line width, text size, a full black frame) applies to the plot
on screen, so what you copy or export is what you see. Exports are PNG
at twice the on-screen width (sharp in a report or a slide) or SVG
(vector; edit it in Inkscape / Illustrator); the clipboard gets the PNG
image. Line widths are scaled with the export so a 2 px line on screen
is still 2 px-at-screen-size in the image (pyqtgraph lines are
"cosmetic": left alone they stay 1 px however large the image).
"""

from __future__ import annotations

import os
from contextlib import contextmanager

from PySide6.QtGui import QFont
from PySide6.QtWidgets import (QApplication, QDoubleSpinBox, QFileDialog,
                               QHBoxLayout, QLabel, QMessageBox, QPushButton,
                               QSpinBox, QWidget)

PNG_SCALE = 2
FRAME_PEN = {"color": "k", "width": 2}
DEFAULT_LINE_WIDTH = 2.0
DEFAULT_FONT_PT = 11


def style_plot(plot_widget, line_width=DEFAULT_LINE_WIDTH,
               font_pt=DEFAULT_FONT_PT):
    """Line width, text size and a full black frame (all four sides)."""
    import pyqtgraph as pg
    item = plot_widget.plotItem
    for curve in item.listDataItems():
        pen = curve.opts.get("pen")
        colour = pg.mkPen(pen).color() if pen is not None else "k"
        curve.setPen(pg.mkPen(colour, width=line_width))
    font = QFont()
    font.setPointSizeF(font_pt)
    # the frame: a black border round the plot area. (Not thick axes:
    # pyqtgraph draws the grid with the axis pen, so the grid would go
    # thick too.) Top and right axes carry tick marks only.
    item.getViewBox().setBorder(pg.mkPen(**FRAME_PEN))
    for side in ("left", "bottom", "top", "right"):
        item.showAxis(side)
        ax = item.getAxis(side)
        ax.setPen(pg.mkPen("k", width=1))
        ax.setTextPen(pg.mkPen("k"))
        ax.setStyle(tickFont=font, tickLength=-6)
        if side in ("top", "right"):
            ax.setStyle(showValues=False)
        else:
            label = ax.labelText
            if label:
                ax.setLabel(label, units=ax.labelUnits or None,
                            **{"font-size": f"{font_pt}pt", "color": "k"})
    legend = item.legend
    if legend is not None:
        for sample, label in legend.items:
            label.setText(label.text, size=f"{font_pt}pt", color="k")
    title = item.titleLabel
    if title.text:
        title.setText(title.text, size=f"{font_pt + 2}pt", color="k")
    plot_widget._bf_style = (line_width, font_pt)


def _legend_size(plot_widget, pt):
    legend = plot_widget.plotItem.legend
    if legend is None:
        return
    for sample, label in legend.items:
        label.setText(label.text, size=f"{pt}pt", color="k")


@contextmanager
def _export_lines(plot_widget, scale):
    """Thicken the (cosmetic) lines by the export scale, then restore."""
    import pyqtgraph as pg
    saved = []
    for curve in plot_widget.plotItem.listDataItems():
        pen = pg.mkPen(curve.opts.get("pen"))
        saved.append((curve, pen))
        wide = pg.mkPen(pen)
        wide.setWidthF(max(pen.widthF(), 1.0) * scale)
        curve.setPen(wide)
    axes = []
    for side in ("left", "bottom", "top", "right"):
        ax = plot_widget.plotItem.getAxis(side)
        pen = pg.mkPen(ax.pen())
        axes.append((ax, pen))
        wide = pg.mkPen(pen)
        wide.setWidthF(max(pen.widthF(), 1.0) * scale)
        ax.setPen(wide)
    vb = plot_widget.plotItem.getViewBox()
    border = pg.mkPen(vb.border) if vb.border is not None else None
    if border is not None:
        wide = pg.mkPen(border)
        wide.setWidthF(max(border.widthF(), 1.0) * scale)
        vb.setBorder(wide)
    # legend text is not scaled by the exporter (axis text is): do it
    pt = getattr(plot_widget, "_bf_style", (0, DEFAULT_FONT_PT))[1]
    _legend_size(plot_widget, pt * scale)
    try:
        yield
    finally:
        for curve, pen in saved:
            curve.setPen(pen)
        for ax, pen in axes:
            ax.setPen(pen)
        if border is not None:
            vb.setBorder(border)
        _legend_size(plot_widget, pt)


def _image_exporter(plot_widget):
    from pyqtgraph.exporters import ImageExporter
    ex = ImageExporter(plot_widget.plotItem)
    width = max(int(plot_widget.width() * PNG_SCALE), 800)
    ex.parameters()["width"] = width
    return ex, width / max(plot_widget.plotItem.sceneBoundingRect().width(),
                           1.0)


def export_png(plot_widget, path):
    ex, scale = _image_exporter(plot_widget)
    with _export_lines(plot_widget, scale):
        ex.export(path)


def export_svg(plot_widget, path):
    """Vector SVG through Qt's own SVG painter (pyqtgraph's SVGExporter
    fails on this Qt's path format)."""
    from PySide6.QtCore import QRect
    from PySide6.QtGui import QPainter
    from PySide6.QtSvg import QSvgGenerator
    size = plot_widget.size()
    gen = QSvgGenerator()
    gen.setFileName(path)
    gen.setSize(size)
    gen.setViewBox(QRect(0, 0, size.width(), size.height()))
    gen.setTitle(plot_widget.plotItem.titleLabel.text or "B-field figure")
    painter = QPainter(gen)
    try:
        plot_widget.render(painter)
    finally:
        painter.end()


def copy_image(plot_widget):
    """The plot as an image on the clipboard; returns the QImage."""
    ex, scale = _image_exporter(plot_widget)
    with _export_lines(plot_widget, scale):
        img = ex.export(toBytes=True)
    QApplication.clipboard().setImage(img)
    return img


class PlotActions(QWidget):
    """Line width / text size, 'Export figure...', 'Copy to clipboard'
    (and 'Export data...' when the panel gives an export_data callback).

    name() -> a default file name; folder() -> where the dialog opens.
    """

    def __init__(self, plot_widget, name=lambda: "figure",
                 folder=lambda: "", say=None, export_data=None):
        super().__init__()
        self.plot = plot_widget
        self.name = name
        self.folder = folder
        self.say = say
        lay = QHBoxLayout(self)
        lay.setContentsMargins(0, 0, 0, 0)
        lay.addWidget(QLabel("lines"))
        self.line_width = QDoubleSpinBox()
        self.line_width.setRange(0.5, 8)
        self.line_width.setSingleStep(0.5)
        self.line_width.setValue(DEFAULT_LINE_WIDTH)
        self.line_width.setSuffix(" px")
        self.line_width.setToolTip("Curve width, on screen and in exports")
        lay.addWidget(self.line_width)
        lay.addWidget(QLabel("text"))
        self.font_pt = QSpinBox()
        self.font_pt.setRange(6, 28)
        self.font_pt.setValue(DEFAULT_FONT_PT)
        self.font_pt.setSuffix(" pt")
        self.font_pt.setToolTip("Axis, legend and title text size")
        lay.addWidget(self.font_pt)
        self.line_width.valueChanged.connect(self.restyle)
        self.font_pt.valueChanged.connect(self.restyle)
        self.export_btn = QPushButton("Export figure...")
        self.export_btn.setToolTip("Save the plot as PNG (2x resolution) "
                                   "or SVG (vector)")
        self.export_btn.clicked.connect(self.export)
        self.copy_btn = QPushButton("Copy to clipboard")
        self.copy_btn.setToolTip("Copy the plot as an image")
        self.copy_btn.clicked.connect(self.copy)
        lay.addWidget(self.export_btn)
        lay.addWidget(self.copy_btn)
        self.data_btn = None
        if export_data is not None:
            self.data_btn = QPushButton("Export data...")
            self.data_btn.setToolTip("Save the plotted spectrum as CSV")
            self.data_btn.clicked.connect(export_data)
            lay.addWidget(self.data_btn)
        lay.addStretch(1)
        self.restyle()

    def restyle(self, *_):
        style_plot(self.plot, self.line_width.value(), self.font_pt.value())

    def export(self):
        start = os.path.join(self.folder() or "", f"{self.name()}.png")
        path, chosen = QFileDialog.getSaveFileName(
            self, "Export figure", start,
            "PNG image (*.png);;SVG vector image (*.svg)")
        if not path:
            return
        if chosen.startswith("SVG") and not path.lower().endswith(".svg"):
            path += ".svg"
        if chosen.startswith("PNG") and not path.lower().endswith(".png"):
            path += ".png"
        try:
            (export_svg if path.lower().endswith(".svg")
             else export_png)(self.plot, path)
        except Exception as e:
            QMessageBox.warning(self, "Export failed", f"{path}\n{e}")
            return
        if self.say:
            self.say(f"[figure] saved {path}")

    def copy(self):
        copy_image(self.plot)
        if self.say:
            self.say("[figure] copied to the clipboard")
