"""Setpoint scans with slowing On/Off shots (TCL setpoint in V, or a WM lock
scan in GHz from an offset), as setpointScan_batch_SL.py."""

import numpy as np

import LatticeEDM_analysis_library as EDM
import tools
from analysis_gui import data
from analysis_gui.modes.base import ModeTab, color, show_message

C_LIGHT = 299792458.


class SetpointTab(ModeTab):
    NAME = "Setpoint scan"
    KEY = "setpoint"
    KEYWORD = "setpoint"
    DESCRIPTION = ("Setpoint scans with slowing On/Off shots: gated On/Off ratio against the "
                   "TCL setpoint, WM lock frequency or WM reading, as setpointScan_batch_SL.py.")
    SETTINGS = [
        ("--", "Gates (ms)"),
        ("SigStart", "Signal start", "float", 27.5),
        ("SigEnd", "Signal end", "float", 40),
        ("BkgStart", "Background start", "float", 70),
        ("BkgEnd", "Background end", "float", 78),
        ("detector", "Detector", "int", 0),
        ("--", "X axis"),
        ("XSource", "Read x axis from", ["auto", "setpoint", "WM lock", "WM"], "auto",
         "auto: as the script - WM reading if it follows the scan (TCL-WM slope >= 800), "
         "WM lock if the scan saved a frequency offset, otherwise the setpoint"),
        ("fTHz", "WM channel of the laser (THz)", "int", 288,
         "Integer THz of the WM reading when the WM is on the scanned laser"),
        ("TCLcal", "Manual TCL calibration (MHz/V)", "float", -235.1,
         "Used for setpoints when the WM isn't on the scanned laser"),
        ("TCLcalerr", "Manual TCL calibration error", "float", 0.83),
        ("ManualTCL", "Always use the manual calibration", "bool", False,
         "Use the manual calibration even when the WM gives one"),
        ("XAxis", "Plot against", ["native", "frequency", "velocity"], "native",
         "native: setpoint (V) / WM lock or WM frequency (MHz); frequency: MHz from SETfreq; "
         "velocity: m/s from the rest frame resonance"),
        ("SET", "SET setpoint (V)", "float", -0.119),
        ("SETfreq", "SET frequency (THz)", "float", 288.734637,
         "Laser frequency at SET; also sets the slowing wavelength for velocities"),
        ("showSET", "Mark SET", "bool", True),
        ("RestFreq", "Rest frame resonance (THz)", "float", 542.808834, "Only for velocity"),
        ("--", "Averaging and fit"),
        ("MA", "Moving average (points)", "int", 3),
        ("AvgSele", "Average passes of files", "text", "",
         "File IDs for the Average plot, e.g. 010, 011; empty = all loaded"),
        ("Exclude", "Exclude scans", "text", "", "Scans left out of the average and the stack, e.g. 002_0"),
        ("BinWidth", "Bin width (x axis units)", "optfloat", None,
         "Width of the bins of the stacked data, in the units of the x axis (V, MHz or m/s). "
         "Blank: the scan step, so each scan point gets its own bin"),
        ("FitMode", "Gaussian fit to", ["binned", "moving average", "none"], "binned",
         "binned: weighted by the SEM of each bin; moving average: unweighted"),
    ]

    def build(self):
        self.add_tof_page()
        page = self.add_page("Scan", self.render_scan)
        page.add_combo("scan", "Scan")
        page = self.add_page("TCL-WM calibration", self.render_cal)
        page.add_combo("scan", "Scan")
        self.add_page("Average", self.render_average)
        self.add_page("Stacked", self.render_stacked)
        self.add_table("scans", "Scans")
        self.add_table("fit", "Gaussian fit")
        self.add_table("binned", "Binned data")
        self.add_table("average", "Average data")
        self.plot_tabs.setCurrentIndex(3)

    def check_scan(self, arrays):
        p = arrays["settings"]["output plugin"] or ""
        if "PGOutputPlugin" in p or "NullOutputPlugin" in p:
            return "doesn't look like a setpoint scan (%s)" % p.split(".")[-1]
        if "FourShot" in (arrays["settings"]["pattern plugin"] or ""):
            return "is a 4-shot scan - use the Slowing tab"
        return ""

    # ------------------------------------------------------------ x axis
    def plot_x(self, r, s, ref):
        """x values to plot of one scan's curves r and the axis label"""
        mode = r["axis"]["mode"]
        if s["XAxis"] == "native":
            if mode == "setpoint":
                return r["X"], "TCL setpoint (V)", "V"
            if mode == "WM lock":
                return r["X"]*1e3, "Relative frequency (MHz) to WM lock setpoint %.10g THz" % r["axis"]["offset"], "MHz"
            return (r["X"] - ref)*1e6, "Relative frequency (MHz) to %.10g THz" % ref, "MHz"
        freq = EDM.SetpointToFreqTHz(r["X"], r["axis"], s["SET"], s["SETfreq"])
        if freq is None:
            raise ValueError("Can't work out frequencies of " + r["label"])
        if s["XAxis"] == "frequency":
            label = "Relative frequency (MHz) to SET %.10g THz" % s["SETfreq"]
            if mode == "setpoint":
                label += "\n(from setpoint, TCL cal. %.4g MHz/V)" % r["axis"]["TCLconv"]
            return (freq - s["SETfreq"])*1e6, label, "MHz"
        wavelength = C_LIGHT/(s["SETfreq"]*1e12)
        return (EDM.FreqToVelocity(freq, s["RestFreq"], wavelength),
                "Velocity (m/s), rest frame at %.10g THz" % s["RestFreq"], "m/s")

    def set_x(self, r, s, ref):
        """x position of the SET line in plotted units, or None"""
        if not s["showSET"]:
            return None
        mode = r["axis"]["mode"]
        if s["XAxis"] == "frequency":
            return 0.
        if s["XAxis"] == "velocity":
            return -(s["SETfreq"] - s["RestFreq"])*1e12*C_LIGHT/(s["SETfreq"]*1e12)
        if mode == "setpoint":
            return s["SET"]
        if mode == "WM lock":
            return s["SET"]*1e3     # as the script
        return (s["SETfreq"] - ref)*1e6

    # ------------------------------------------------------------ analysis
    def analyse(self, s):
        exclude = set(data.parse_ids(s["Exclude"]))
        scans = {}
        for label, arrays in self.scans.items():
            axis = EDM.SetpointScanAxis(arrays, s["fTHz"], s["XSource"], s["TCLcal"], s["TCLcalerr"],
                                        s["ManualTCL"])
            r = EDM.SetpointScanCurves(arrays, axis, s["SigStart"], s["SigEnd"], s["BkgStart"], s["BkgEnd"],
                                       s["detector"])
            r["axis"] = axis
            r["label"] = label
            scans[label] = r
            print("%s: x axis %s%s%s" % (label, axis["mode"],
                  ", TCL cal %.4g MHz/V" % axis["TCLconv"] if axis["mode"] == "setpoint" and axis["TCLconv"] is not None else "",
                  " (" + axis["note"] + ")" if axis["note"] else ""))

        # Reference frequency for WM x axes: first point of the first WM scan
        ref = next((r["X"][0] for r in scans.values() if r["axis"]["mode"] == "WM"), 0.)
        for r in scans.values():
            r["x"], r["xlabel"], r["xunit"] = self.plot_x(r, s, ref)
        mixed = len({r["xunit"] for r in scans.values()}) > 1 or \
            len({r["axis"]["mode"] for r in scans.values()}) > 1
        if mixed:
            print("WARNING: the scans have different x axes (%s) - the average and stack mix them"
                  % ", ".join(sorted({r["axis"]["mode"] for r in scans.values()})))

        # ---- average over passes (point by point)
        avg_ids = set(data.parse_ids(s["AvgSele"]))
        avg_labels = [l for l in scans if (not avg_ids or data.label_id(l) in avg_ids) and l not in exclude]
        average = None
        average_note = ""
        if avg_labels:
            n_points = {len(scans[l]["ratio"]) for l in avg_labels}
            if len(n_points) > 1:
                average_note = "Passes have different numbers of scan points: " + \
                    ", ".join("%s: %d" % (l, len(scans[l]["ratio"])) for l in avg_labels)
            else:
                R = np.array([scans[l]["ratio"] for l in avg_labels])
                Ravg, Rsem = EDM.MeanAndSEM(R, axis=0, ddof=1)
                Xavg = np.mean([scans[l]["x"] for l in avg_labels], axis=0)
                average = {"labels": avg_labels, "x": Xavg, "ratio": Ravg, "sem": Rsem}
        else:
            average_note = "No scans to average"

        # ---- stack all passes, sort, moving average, bin, fit
        stack_labels = [l for l in scans if l not in exclude]
        for l in [l for l in scans if l in exclude]:
            print("Excluding %s from the average and the stack" % l)
        if not stack_labels:
            raise ValueError("Every scan is excluded")
        Xs = np.concatenate([scans[l]["x"] for l in stack_labels])
        Rs = np.concatenate([scans[l]["ratio"] for l in stack_labels])
        order = np.argsort(Xs, kind="stable")
        Xs, Rs = Xs[order], Rs[order]
        MA = max(1, min(s["MA"], len(Xs)))
        stack = {"labels": stack_labels, "x": Xs, "ratio": Rs, "MA": MA,
                 "ma x": tools.MovingAverage(MA, Xs), "ma ratio": tools.MovingAverage(MA, Rs)}
        width = s["BinWidth"]
        if width is None:
            # the scan step of the first scan
            steps = np.abs(np.diff(np.unique(scans[stack_labels[0]]["x"])))
            width = np.median(steps) if len(steps) else 1.
        if width <= 0:
            raise ValueError("Bin width has to be > 0")
        bx, by, berr, bn = EDM.BinXY(Xs, Rs, width)
        stack.update({"bin x": bx, "bin ratio": by, "bin err": berr, "bin n": bn, "bin width": width})

        fit = None
        if s["FitMode"] == "binned":
            mask = np.isfinite(berr) & (berr > 0)
            if np.sum(mask) > 4:
                fit = EDM.GaussianPeakFit(bx[mask], by[mask], berr[mask])
                fit["x"] = bx[mask]
            else:
                print("Too few bins with errors for a weighted fit (need > 4) - try a wider bin")
        elif s["FitMode"] == "moving average":
            if len(stack["ma x"]) > 4:
                fit = EDM.GaussianPeakFit(stack["ma x"], stack["ma ratio"])
                fit["x"] = stack["ma x"]
        if fit is not None:
            f, e = fit["best fit"], fit["error"]
            unit = next(iter(scans.values()))["xunit"]
            print("Gaussian fit to the %s ratio: centre = %.6g +- %.3g %s, FWHM = %.4g +- %.3g %s, "
                  "amplitude = %.4g +- %.3g, offset = %.4g +- %.3g%s" % (
                      s["FitMode"], f[0], e[0], unit, fit["FWHM"], fit["FWHM err"], unit, f[2], e[2], f[3], e[3],
                      ", reduced chi^2 = %.3g" % fit["reduced chi2"] if np.isfinite(fit["reduced chi2"]) else ""))
            if not fit["ok"]:
                print("  fit failed: " + fit["message"])
        return {"scans": scans, "average": average, "average note": average_note, "stack": stack,
                "fit": fit, "ref": ref, "mixed": mixed}

    def fill_tables(self):
        rows = []
        for label, r in self.results["scans"].items():
            a = r["axis"]
            rows.append({"scan": label, "x axis": a["mode"], "points": len(r["X"]),
                         "shots/point": self.scans[label]["settings"]["shotsPerPoint"],
                         "TCL cal (MHz/V)": a["TCLconv"] if a["mode"] == "setpoint" else None,
                         "TCL cal err": a["TCLconverr"] if a["mode"] == "setpoint" else None,
                         "WM channel (THz)": a["WM channel THz"], "WM lock offset (THz)": a["offset"],
                         "note": a["note"]})
        self.tables["scans"].set_rows(rows)

        fit = self.results["fit"]
        unit = next(iter(self.results["scans"].values()))["xunit"]
        rows = []
        if fit is not None:
            f, e = fit["best fit"], fit["error"]
            rows.append({"fit to": self.last_settings["FitMode"], "x unit": unit,
                         "centre": f[0], "centre err": e[0], "FWHM": fit["FWHM"], "FWHM err": fit["FWHM err"],
                         "amplitude": f[2], "amplitude err": e[2], "offset": f[3], "offset err": e[3],
                         "reduced chi2": fit["reduced chi2"], "ok": fit["ok"]})
        self.tables["fit"].set_rows(rows)

        st = self.results["stack"]
        self.tables["binned"].set_rows([{"x (%s)" % unit: x, "ratio": y, "SEM": e, "points": int(n)}
                                        for x, y, e, n in zip(st["bin x"], st["bin ratio"], st["bin err"], st["bin n"])])
        av = self.results["average"]
        self.tables["average"].set_rows([] if av is None else
                                        [{"x (%s)" % unit: x, "ratio": y, "SEM": e}
                                         for x, y, e in zip(av["x"], av["ratio"], av["sem"])])

    # ------------------------------------------------------------ plots
    def _set_line(self, ax, r):
        x = self.set_x(r, self.last_settings, self.results["ref"])
        if x is not None:
            ax.axvline(x, color="r", linestyle="--", linewidth=1, label="SET")

    def _title(self, extra):
        s = self.last_settings
        return "Gated TOF from %g ms to %g ms gate, %s" % (s["SigStart"], s["SigEnd"], extra)

    def render_scan(self, page, fig):
        label = page.value("scan")
        if label not in self.results["scans"]:
            show_message(fig, "No scan")
            return
        r = self.results["scans"][label]
        MA = max(1, min(self.last_settings["MA"], len(r["x"])))
        ax1, ax2 = fig.subplots(2, 1, sharex=True)
        ax1.errorbar(r["x"], r["on"], yerr=r["on err"], fmt=".", color=color(0), label="On")
        ax1.errorbar(r["x"], r["off"], yerr=r["off err"], fmt=".", color=color(1), label="Off")
        ax1.set_ylabel("Gated LIF (ms.V)")
        ax1.set_title(self._title("%s, %s shots per point" % (label, self.scans[label]["settings"]["shotsPerPoint"])))
        ax1.legend()
        ax2.plot(r["x"], r["ratio"], color=color(0), label="On/Off")
        ax2.plot(tools.MovingAverage(MA, r["x"]), tools.MovingAverage(MA, r["ratio"]), color=color(1),
                 label="Moving average of %d" % MA)
        self._set_line(ax2, r)
        ax2.set_xlabel(r["xlabel"])
        ax2.set_ylabel("Slowing On/Off ratio")
        ax2.legend()

    def render_cal(self, page, fig):
        label = page.value("scan")
        if label not in self.results["scans"]:
            show_message(fig, "No scan")
            return
        cal = self.results["scans"][label]["axis"]["cal"]
        arrays = self.scans[label]
        if cal is None:
            wm = arrays["wm THz"]
            show_message(fig, "No TCL-WM calibration for %s:\n%s" % (
                label, "no WM reading" if wm is None else
                "the WM channel is at %.7g THz, not %d THz" % (wm[0], self.last_settings["fTHz"])))
            return
        sp = arrays["scan params"]
        wm = arrays["wm THz"]
        rel = (wm - wm[0])*1e6
        ax = fig.add_subplot()
        ax.plot(sp, rel, ".", label="WM reading")
        span = np.linspace(np.min(sp), np.max(sp), 200)
        if cal["ok"]:
            ax.plot(span, tools.Line(span, *cal["best fit"]),
                    label="%.4g ± %.2g MHz/V" % (cal["best fit"][0], cal["error"][0]))
        ax.set_xlabel("%s %s" % (arrays["settings"]["channel"] or "", arrays["settings"]["param"] or "setpoint"))
        ax.set_ylabel("Relative frequency (MHz) from %.8g THz" % wm[0])
        ax.set_title("TCL-WM calibration of %s: %.4g MHz/V" % (label, cal["best fit"][0]))
        ax.legend()

    def render_average(self, page, fig):
        av = self.results["average"]
        if av is None:
            show_message(fig, self.results["average note"])
            return
        scans = self.results["scans"]
        ax = fig.add_subplot()
        for l in av["labels"]:
            ax.plot(scans[l]["x"], scans[l]["ratio"], color="grey", alpha=0.3, linewidth=0.8)
        ax.plot(av["x"], av["ratio"], ".-", color=color(0), label="Mean of %d passes" % len(av["labels"]))
        ax.fill_between(av["x"], av["ratio"] - av["sem"], av["ratio"] + av["sem"],
                        color=color(0), alpha=0.3, linewidth=0, label="SEM")
        self._set_line(ax, scans[av["labels"][0]])
        ax.set_xlabel(scans[av["labels"][0]]["xlabel"])
        ax.set_ylabel("Slowing On/Off ratio")
        ax.set_title(self._title("averaged over passes of files " +
                                 ", ".join(sorted({data.label_id(l) for l in av["labels"]}))))
        ax.legend()

    def render_stacked(self, page, fig):
        st = self.results["stack"]
        scans = self.results["scans"]
        first = scans[st["labels"][0]]
        ax = fig.add_subplot()
        ax.plot(st["x"], st["ratio"], ".", color="0.7", markersize=3, label="All points")
        ax.plot(st["ma x"], st["ma ratio"], color=color(2), linewidth=1,
                label="Moving average of %d" % st["MA"])
        ax.plot(st["bin x"], st["bin ratio"], ".-", color=color(0),
                label="Bins of %.3g %s" % (st["bin width"], first["xunit"]))
        ax.fill_between(st["bin x"], st["bin ratio"] - st["bin err"], st["bin ratio"] + st["bin err"],
                        color=color(0), alpha=0.3, linewidth=0, label="SEM")
        fit = self.results["fit"]
        if fit is not None and np.all(np.isfinite(fit["best fit"])):
            span = np.linspace(np.min(fit["x"]), np.max(fit["x"]), 1000)
            f, e = fit["best fit"], fit["error"]
            ax.plot(span, tools.Gaussian(span, *f), color=color(1),
                    label="Gaussian fit: centre %.5g ± %.2g, FWHM %.4g ± %.2g %s" % (
                        f[0], e[0], fit["FWHM"], fit["FWHM err"], first["xunit"]))
        self._set_line(ax, first)
        ax.set_xlabel(first["xlabel"])
        ax.set_ylabel("Slowing On/Off ratio")
        ax.set_title(self._title("%d passes stacked, files %s" % (
            len(st["labels"]), ", ".join(sorted({data.label_id(l) for l in st["labels"]})))))
        ax.legend(fontsize=8)
