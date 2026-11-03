"""4-shot slowing scans: setpoint scans of the angled probe with slowing On/Off
and YAG On/Off shots, as setpointScanONOFF_4shot_batch_SL.py. Gated TOFs
(YAG On - YAG Off) are fitted with Gaussians, normalised to the Off fit, and
the peak shift (On - Off) is plotted against slowing duration."""

import os

import numpy as np

import LatticeEDM_analysis_library as EDM
import tools
from analysis_gui import data
from analysis_gui.modes.base import ModeTab, color, show_message


def parse_subgroups(text):
    """"004+005, 012" -> [["004", "005"], ["012"]]"""
    groups = []
    for part in text.replace(";", ",").split(","):
        ids = [i.strip() for i in part.split("+") if i.strip()]
        if ids:
            groups.append(ids)
    return groups


class SlowingTab(ModeTab):
    NAME = "Slowing (4-shot)"
    KEY = "slowing4shot"
    KEYWORD = "Slowing"
    DESCRIPTION = ("4-shot slowing scans (YAG On/Off, slowing On/Off): Gaussian fits of the gated TOFs "
                   "normalised to the Off fit, peak shift against slowing duration, "
                   "as setpointScanONOFF_4shot_batch_SL.py.")
    SETTINGS = [
        ("--", "Gate (ms), background from the YAG Off shots"),
        ("SigStart", "Signal start", "float", 27),
        ("SigEnd", "Signal end", "float", 37),
        ("--", "X axis"),
        ("UseSetpoint", "Use TCL setpoints (ignore WM)", "bool", True),
        ("fTHz", "WM channel of the probe (THz)", "int", 542),
        ("Resonance", "Rest frame probe frequency (THz)", "float", 542.809124),
        ("angle", "Probe angle to slowing (deg)", "float", 45),
        ("CalTCLconv", "TCL calibration (MHz/V)", "optfloat", -133.9,
         "Converts setpoint peak shifts to m/s. Blank: results stay in volts"),
        ("CalTCLconverr", "TCL calibration error (MHz/V)", "optfloat", 6.5),
        ("CalRestSP", "Zero-velocity setpoint (V)", "optfloat", -1.7,
         "With the TCL calibration, converts the setpoint x axis to velocity. Blank: x stays in volts"),
        ("wavelength", "Probe wavelength (nm)", "float", 552),
        ("--", "Fits"),
        ("WeightedFit", "Weighted fits of averages", "bool", True,
         "Fits of averaged scans are weighted by their standard error (only with more than 2 scans)"),
        ("ManualP0On", "Manual guess On (A, c, FWHM, off)", "floats", "",
         "Tried only if the automatic guess fails, with Manual guess Off. x in m/s, or V for setpoints"),
        ("ManualP0Off", "Manual guess Off (A, c, FWHM, off)", "floats", ""),
        ("Exclude", "Exclude scans", "text", "", "Scans to leave out, e.g. 010_1"),
        ("--", "Summary"),
        ("FitLine", "Fit lines to shift vs duration", "bool", True),
        ("WeightedLineFit", "Weighted line fits", "bool", True),
        ("FixOrigin", "Line through the origin", "bool", True),
        ("FitExcludeTypes", "No line fit for types", "text", "", "Type names, comma separated"),
        ("--", "TOFs in range"),
        ("TOFxMin", "x from", "float", 20, "x range (m/s, or V) of the scan points to average"),
        ("TOFxMax", "x to", "float", 40),
        ("TOFtMin", "Zoom time from (ms)", "optfloat", None),
        ("TOFtMax", "Zoom time to (ms)", "optfloat", None),
    ]
    GROUPS = (("Type", "Files"),
              "Files averaged together joined with +, groups of the type separated by commas, "
              "e.g. 004+005, 012. No groups: every file is its own type.")

    def build(self):
        self.add_tof_page()
        page = self.add_page("Scan fit", self.render_scan)
        page.add_combo("scan", "Scan")
        page = self.add_page("Group fit", self.render_group)
        page.add_combo("group", "Group")
        self.add_page("Summary", self.render_summary)
        self.add_page("Compare", self.render_compare)
        self.add_page("TOFs in range", self.render_tofs)
        self.add_table("scans", "Scan fits")
        self.add_table("groups", "Group fits")
        self.add_table("lines", "Line fits")
        self.add_table("tofs", "Group gated TOFs")
        self.plot_tabs.setCurrentIndex(3)

    def check_scan(self, arrays):
        if "FourShot" not in (arrays["settings"]["pattern plugin"] or ""):
            return "isn't a 4-shot scan (%s)" % (arrays["settings"]["pattern plugin"] or "").split(".")[-1]
        return ""

    def selector_items(self, name):
        if name == "group" and self.results is not None:
            return list(self.results["groups"].keys())
        return super().selector_items(name)

    def tof_traces(self, arrays, point, s):
        if arrays["off"] is None or arrays["on"].shape[0] < 2:
            return super().tof_traces(arrays, point, s)
        traces = []
        for shot, label in [("on", "Slowing On"), ("off", "Slowing Off")]:
            tofs = EDM.DetectorTOFs(arrays, shot, 0) - EDM.DetectorTOFs(arrays, shot, 1)
            traces.append((label + ", YAG On-Off", arrays["time " + shot]*1000, self._point_mean(tofs, point)))
        return traces

    # ------------------------------------------------------------ analysis
    def fit(self, x, on, off, axis, s, on_err=None, off_err=None, weighted=False):
        manual = None
        if s["ManualP0On"] and s["ManualP0Off"]:
            if len(s["ManualP0On"]) != 4 or len(s["ManualP0Off"]) != 4:
                raise ValueError("Manual guesses need 4 numbers: amplitude, centre, FWHM, offset")
            manual = {"On": s["ManualP0On"], "Off": s["ManualP0Off"]}
        return EDM.FitSlowingOnOff(x, on, off, axis["in volts"], axis["from setpoint"],
                                   OnErr=on_err, OffErr=off_err, Weighted=weighted, ManualP0=manual,
                                   CalTCLconv=s["CalTCLconv"], CalTCLconverr=s["CalTCLconverr"],
                                   angle=s["angle"], wavelength=s["wavelength"])

    def analyse(self, s):
        if (s["CalTCLconv"] is None) != (s["CalTCLconverr"] is None):
            raise ValueError("Give both the TCL calibration and its error, or neither")
        exclude = set(data.parse_ids(s["Exclude"]))
        scans = {}
        skipped = []
        for label, arrays in self.scans.items():
            settings = arrays["settings"]
            if arrays["off"] is None or arrays["on"].shape[0] < 2:
                print("%s skipped: not 4-shot On/Off data (%s)" % (label, settings["pattern plugin"]))
                skipped.append(label)
                continue
            tslow = np.nan if settings["slowing time"] is None else settings["slowing time"]/1000
            if settings["slowing time"] is None:
                print("WARNING: no slowing duration saved for %s" % label)
            gated = EDM.FourShotGated(arrays, s["SigStart"], s["SigEnd"])
            axis = EDM.SlowingXAxis(arrays, s["UseSetpoint"], s["fTHz"], s["Resonance"], s["angle"],
                                    s["CalTCLconv"], s["CalRestSP"], s["wavelength"])
            print("%s: %g ms slowing, x from %s" % (label, tslow, axis["source"]))
            res = self.fit(axis["x"], gated["on"], gated["off"], axis, s)
            if res is None:
                print("%s: fit failed" % label)
            else:
                print("%s: peak shift (On-Off) = %.4g +- %.3g %s" % (
                    label, res["peak shift"][0], res["peak shift"][1], res["shift unit"]))
            scans[label] = {"On": gated["on"], "Off": gated["off"], "x": axis["x"],
                            "x unit": "V" if axis["in volts"] else "m/s", "setpoint": arrays["scan params"],
                            "slowing time": tslow, "axis": axis, "fit": res}
        if not scans:
            raise ValueError("No 4-shot scans among the loaded ones")

        # ---- groups: each comma-separated part of a type is averaged and fitted
        if s["groups"]:
            types = [(name, parse_subgroups(text)) for name, text in s["groups"]]
        else:
            ids = []
            for l in scans:
                if data.label_id(l) not in ids:
                    ids.append(data.label_id(l))
            types = [(i, [[i]]) for i in ids]

        groups = {}
        summary = {}
        for tname, subgroups in types:
            X, Y, Yerr, units = [], [], [], set()
            for ids in subgroups:
                members = [l for l in scans if data.label_id(l) in ids]
                for l in [m for m in members if m in exclude]:
                    print("Excluding %s from %s" % (l, tname))
                members = [m for m in members if m not in exclude]
                gname = "%s %s" % (tname, "+".join(ids))
                if not members:
                    print(gname + ": no scans - skipped")
                    continue
                times = np.array([scans[m]["slowing time"] for m in members])
                if np.ptp(times) > 0:
                    print("WARNING: %s has different slowing durations %s" % (gname, times))
                try:
                    avg = EDM.AverageSlowingScans([scans[m] for m in members])
                except ValueError as e:
                    print("WARNING: %s not averaged: %s" % (gname, e))
                    continue
                axis = scans[members[0]]["axis"]
                allow_weighted = len(members) > 2
                if s["WeightedFit"] and not allow_weighted:
                    print("%s: only %d scans averaged - fitted unweighted" % (gname, len(members)))
                res = self.fit(avg["x"], avg["On"], avg["Off"], axis, s, avg["On err"], avg["Off err"],
                               s["WeightedFit"] and allow_weighted)
                groups[gname] = {"type": tname, "ids": "+".join(ids), "members": members, "avg": avg,
                                 "slowing time": np.mean(times), "fit": res, "axis": axis}
                if res is None:
                    print("%s: fit failed" % gname)
                    continue
                print("%s (%s): peak shift = %.4g +- %.3g %s" % (gname, ", ".join(members), res["peak shift"][0],
                                                                res["peak shift"][1], res["shift unit"]))
                X.append(np.mean(times))
                Y.append(res["peak shift"][0])
                Yerr.append(res["peak shift"][1])
                units.add(res["shift unit"])
            if X:
                line = None
                no_line = {t.strip() for t in s["FitExcludeTypes"].split(",")}
                if s["FitLine"] and tname not in no_line:
                    line = EDM.FitShiftVsTime(X, Y, Yerr, s["FixOrigin"], s["WeightedLineFit"])
                summary[tname] = {"X": np.array(X), "Y": np.array(Y), "Yerr": np.abs(np.array(Yerr)),
                                  "unit": "/".join(sorted(units)), "line": line}
                if len(units) > 1:
                    print("WARNING: %s mixes peak shifts in %s" % (tname, " and ".join(sorted(units))))
        return {"scans": scans, "groups": groups, "summary": summary, "skipped": skipped}

    def fill_tables(self):
        rows = []
        for label, r in self.results["scans"].items():
            row = {"scan": label, "slowing time (ms)": r["slowing time"], "x from": r["axis"]["source"]}
            res = r["fit"]
            if res is not None:
                row.update({"peak shift": res["peak shift"][0], "peak shift err": res["peak shift"][1],
                            "shift unit": res["shift unit"],
                            "ON mean": res["fit ON"]["best fit"][1], "ON mean err": res["fit ON"]["error"][1],
                            "OFF mean": res["fit OFF"]["best fit"][1], "OFF mean err": res["fit OFF"]["error"][1],
                            "ON FWHM": res["fit ON"]["best fit"][2], "OFF FWHM": res["fit OFF"]["best fit"][2],
                            "x unit": res["x unit"]})
            else:
                row["peak shift"] = "fit failed"
            rows.append(row)
        self.tables["scans"].set_rows(rows)

        # As the "Export GroupFits" cell of the script
        rows = []
        tof_rows = []
        for gname, g in self.results["groups"].items():
            res = g["fit"]
            row = {"type": g["type"], "file IDs": g["ids"], "scans": "+".join(g["members"]),
                   "slowing time (ms)": g["slowing time"], "x unit": g["avg"]["x unit"]}
            if res is not None:
                for shot in ["ON", "OFF"]:
                    fit = res["fit " + shot]
                    for j, var in enumerate(["amplitude", "mean", "FWHM", "offset"]):
                        row["%s %s" % (shot, var)] = fit["best fit"][j]
                        row["%s %s err" % (shot, var)] = fit["error"][j]
                row["peak shift (%s)" % res["shift unit"]] = res["peak shift"][0]
                row["peak shift err (%s)" % res["shift unit"]] = res["peak shift"][1]
            else:
                row["peak shift"] = "fit failed"
            rows.append(row)
            avg = g["avg"]
            n = len(avg["x"])
            nan = np.full(n, np.nan)
            for k in range(n):
                tof_row = {"type": g["type"], "file IDs": g["ids"], "scans": "+".join(g["members"]),
                           "slowing time (ms)": g["slowing time"], "setpoint (V)": avg["setpoint"][k],
                           "x (%s)" % avg["x unit"]: avg["x"][k],
                           "On gated TOF": avg["On"][k],
                           "On gated TOF err": (nan if avg["On err"] is None else avg["On err"])[k],
                           "Off gated TOF": avg["Off"][k],
                           "Off gated TOF err": (nan if avg["Off err"] is None else avg["Off err"])[k]}
                if res is not None:
                    tof_row.update({"On normalised": res["norm On"][k],
                                    "On normalised err": (nan if res["norm On err"] is None else res["norm On err"])[k],
                                    "Off normalised": res["norm Off"][k],
                                    "Off normalised err": (nan if res["norm Off err"] is None else res["norm Off err"])[k]})
                tof_rows.append(tof_row)
        self.tables["groups"].set_rows(rows)
        self.tables["tofs"].set_rows(tof_rows)

        rows = []
        for tname, p in self.results["summary"].items():
            if p["line"] is not None:
                rows.append({"type": tname, "slope (%s/ms)" % p["unit"]: p["line"]["best fit"][0],
                             "slope err": p["line"]["error"][0], "intercept": p["line"]["best fit"][1],
                             "intercept err": p["line"]["error"][1], "weighted": p["line"]["weighted"],
                             "through origin": self.last_settings["FixOrigin"]})
        self.tables["lines"].set_rows(rows)

    # ------------------------------------------------------------ plots
    def _plot_fit(self, fig, x, on, off, res, axis, title, on_err=None, off_err=None):
        ax = fig.add_subplot()
        xunit, xname, xlabel, _ = EDM.SlowingAxisInfo(axis["in volts"])
        if res is None:
            if off_err is not None:
                ax.fill_between(x, off - off_err, off + off_err, color=color(0), alpha=0.3)
                ax.fill_between(x, on - on_err, on + on_err, color=color(1), alpha=0.3)
            ax.plot(x, off, ".", color=color(0), label="Slowing Off")
            ax.plot(x, on, ".", color=color(1), label="Slowing On")
            ax.set_ylabel("Gated TOF")
            ax.set_title(title + ", fit failed")
        else:
            span = np.linspace(np.min(x), np.max(x), 1000)
            if res["norm Off err"] is not None:
                ax.fill_between(res["x"], res["norm Off"] - res["norm Off err"], res["norm Off"] + res["norm Off err"],
                                color=color(0), alpha=0.3)
            if res["norm On err"] is not None:
                ax.fill_between(res["x"], res["norm On"] - res["norm On err"], res["norm On"] + res["norm On err"],
                                color=color(1), alpha=0.3)
            fon, foff = res["fit ON"], res["fit OFF"]
            ax.plot(res["x"], res["norm Off"], ".", color=color(0), label="Slowing Off")
            ax.plot(span, tools.Gaussian_FWHM(span, *foff["best fit"]), color=color(0),
                    label="Off: %.4g ± %.3g %s, FWHM %.4g" % (foff["best fit"][1], foff["error"][1], xunit, foff["best fit"][2]))
            ax.plot(res["x"], res["norm On"], ".", color=color(1), label="Slowing On")
            ax.plot(span, tools.Gaussian_FWHM(span, *fon["best fit"]), color=color(1),
                    label="On: %.4g ± %.3g %s, FWHM %.4g" % (fon["best fit"][1], fon["error"][1], xunit, fon["best fit"][2]))
            ax.set_ylabel("Normalised gated TOF")
            ax.set_title("%s\n%.3g ± %.2g %s peak shift%s" % (title, res["peak shift"][0], res["peak shift"][1],
                                                             res["shift unit"], ", weighted fit" if res["weighted"] else ""))
        ax.set_xlabel(xlabel)
        ax.legend(fontsize=8)

    def render_scan(self, page, fig):
        label = page.value("scan")
        r = self.results["scans"].get(label)
        if r is None:
            show_message(fig, "%s isn't a 4-shot scan" % label if label else "No scan")
            return
        s = self.last_settings
        self._plot_fit(fig, r["x"], r["On"], r["Off"], r["fit"], r["axis"],
                       "%s, %g ms slowing, %g-%g ms gate" % (label, r["slowing time"], s["SigStart"], s["SigEnd"]))

    def render_group(self, page, fig):
        g = self.results["groups"].get(page.value("group"))
        if g is None:
            show_message(fig, "No groups - check the group file IDs")
            return
        s = self.last_settings
        a = g["avg"]
        self._plot_fit(fig, a["x"], a["On"], a["Off"], g["fit"], g["axis"],
                       "%s: %s, %g ms slowing, %g-%g ms gate" % (
                           page.value("group"), "+".join(g["members"]), g["slowing time"], s["SigStart"], s["SigEnd"]),
                       a["On err"], a["Off err"])

    def render_summary(self, page, fig):
        summary = self.results["summary"]
        if not summary:
            show_message(fig, "No fitted groups")
            return
        ax = fig.add_subplot()
        tmax = max(np.max(p["X"]) for p in summary.values())
        span = np.linspace(0, max(tmax*1.1, 1.), 200)
        units = set()
        for i, (tname, p) in enumerate(summary.items()):
            label = tname
            if p["line"] is not None and np.all(np.isfinite(p["line"]["best fit"])):
                f, e = p["line"]["best fit"], p["line"]["error"]
                label += ", slope = %.3g ± %.2g %s/ms" % (f[0], e[0], p["unit"])
                ax.plot(span, tools.Line(span, *f), color=color(i))
            ax.errorbar(p["X"], p["Y"], yerr=p["Yerr"], fmt=".", markersize=12, capsize=5,
                        elinewidth=2, capthick=2, color=color(i), label=label)
            units.add(p["unit"])
        s = self.last_settings
        ax.set_xlabel("Slowing duration (ms)")
        ax.set_ylabel("Peak shift (%s)" % "/".join(sorted(units)))
        folder = self.folder_getter()
        day = " ".join([os.path.basename(os.path.dirname(folder)), os.path.basename(folder)]).strip()
        ax.set_title("%s slowing summary, gate %g ms to %g ms" % (day, s["SigStart"], s["SigEnd"]))
        ax.legend(fontsize=8)

    def render_compare(self, page, fig):
        groups = {k: g for k, g in self.results["groups"].items() if g["fit"] is not None}
        if not groups:
            show_message(fig, "No fitted groups")
            return
        ax = fig.add_subplot()
        xunits = set()
        for i, (gname, g) in enumerate(groups.items()):
            res = g["fit"]
            span = np.linspace(np.min(res["x"]), np.max(res["x"]), 1000)
            if res["norm On err"] is not None:
                ax.fill_between(res["x"], res["norm On"] - res["norm On err"], res["norm On"] + res["norm On err"],
                                color=color(i), alpha=0.3)
            if res["norm Off err"] is not None:
                ax.fill_between(res["x"], res["norm Off"] - res["norm Off err"], res["norm Off"] + res["norm Off err"],
                                color=color(i), alpha=0.15)
            ax.plot(res["x"], res["norm On"], "o", color=color(i), markersize=4)
            ax.plot(res["x"], res["norm Off"], "o", color=color(i), markerfacecolor="none", markersize=4, alpha=0.7)
            ax.plot(span, tools.Gaussian_FWHM(span, *res["fit ON"]["best fit"]), color=color(i),
                    label="%s, %g ms" % (gname, g["slowing time"]))
            ax.plot(span, tools.Gaussian_FWHM(span, *res["fit OFF"]["best fit"]), "-.", color=color(i))
            xunits.add(res["x unit"])
        ax.set_xlabel(EDM.SlowingAxisInfo(xunits == {"V"})[2])
        ax.set_ylabel("Normalised gated TOF")
        ax.set_title("Slowing comparison: On filled points and solid fits, Off open points and dash-dot fits")
        ax.legend(fontsize=8)

    def render_tofs(self, page, fig):
        s = self.last_settings
        lo, hi = sorted((s["TOFxMin"], s["TOFxMax"]))
        ax = fig.add_subplot()
        visible = []
        drawn = 0
        units = set()
        for i, (gname, g) in enumerate(self.results["groups"].items()):
            ons, offs = [], []
            for label in g["members"]:
                arrays = self.scans[label]
                x = np.asarray(self.results["scans"][label]["x"])   # scan order, same as the points
                inrange = (x >= lo) & (x <= hi)
                if not np.any(inrange):
                    continue
                ons.append(EDM.FourShotPointTOFs(arrays, "on")[inrange].mean(axis=0))
                offs.append(EDM.FourShotPointTOFs(arrays, "off")[inrange].mean(axis=0))
                t_on, t_off = arrays["time on"]*1000, arrays["time off"]*1000
            if not ons:
                continue
            units.add(g["avg"]["x unit"])
            on, on_err = EDM.MeanAndSEM(ons, axis=0, ddof=1)
            off, off_err = EDM.MeanAndSEM(offs, axis=0, ddof=1)
            if len(ons) > 1:
                ax.fill_between(t_on, on - on_err, on + on_err, color=color(i), alpha=0.3)
                ax.fill_between(t_off, off - off_err, off + off_err, color=color(i), alpha=0.15)
            ax.plot(t_on, on, color=color(i), label="%s, slowing On (%d passes)" % (gname, len(ons)))
            ax.plot(t_off, off, "-.", color=color(i), label="%s, slowing Off" % gname)
            drawn += 1
            if s["TOFtMin"] is not None and s["TOFtMax"] is not None:
                for t, y, e in [(t_on, on, on_err), (t_off, off, off_err)]:
                    inT = (t >= s["TOFtMin"]) & (t <= s["TOFtMax"])
                    visible += [(y - e)[inT], (y + e)[inT]]
        if not drawn:
            fig.clear()
            show_message(fig, "No scan points with x between %g and %g" % (lo, hi))
            return
        ax.axvline(s["SigStart"], color="grey", linestyle=":")
        ax.axvline(s["SigEnd"], color="grey", linestyle=":")
        if s["TOFtMin"] is not None and s["TOFtMax"] is not None:
            ax.set_xlim(s["TOFtMin"], s["TOFtMax"])
            visible = np.concatenate(visible) if visible else np.array([])
            if len(visible):
                margin = 0.05*(np.max(visible) - np.min(visible))
                ax.set_ylim(np.min(visible) - margin, np.max(visible) + margin)
        ax.set_xlabel("time (ms)")
        ax.set_ylabel("Signal (V)")
        ax.set_title("YAG On-Off TOFs averaged over x = %g to %g %s" % (lo, hi, "/".join(sorted(units))))
        ax.legend(fontsize=8)
