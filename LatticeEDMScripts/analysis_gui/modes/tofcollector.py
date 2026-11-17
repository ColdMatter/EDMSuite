"""TOF collector: nothing scanned, On/Off shots accumulated for statistics,
as TOFcollectorONOFF_SL.py (and TOFcollectorONOFF_4shot_SL.py for 4-shot
patterns, where the YAG Off shot is the background)."""

import numpy as np
from matplotlib import colormaps

import LatticeEDM_analysis_library as EDM
import tools
from analysis_gui import data
from analysis_gui.modes.base import ModeTab, color, gate_lines, show_message


class TOFCollectorTab(ModeTab):
    NAME = "TOF collector"
    KEY = "tofcollector"
    KEYWORD = "TOF"
    DESCRIPTION = ("TOF collector files: averaged background-subtracted On and Off TOFs, their "
                   "difference and the gated On/Off ratio, as TOFcollectorONOFF_SL.py.")
    SETTINGS = [
        ("--", "Gates (ms)"),
        ("SigStart", "Signal start", "float", 22),
        ("SigEnd", "Signal end", "float", 24),
        ("BkgStart", "Background start", "float", 70),
        ("BkgEnd", "Background end", "float", 78),
        ("detector", "Detector", "int", 0),
        ("FourShot", "4-shot (YAG Off background)", ["auto", "yes", "no"], "auto",
         "4-shot data: the YAG Off shot (detector 1) is subtracted from the YAG On shot (detector 0) "
         "instead of the background gate. auto: from the pattern plugin"),
        ("--", "Plots"),
        ("MA", "Moving average (samples)", "int", 100),
        ("Chunk", "Chunk length (shots)", "int", 300, "Shots per chunk on the Chunks page"),
        ("Overlay", "Files to overlay", "text", "", "File IDs for the overlay pages; empty = all"),
    ]

    def build(self):
        self.add_tof_page()
        page = self.add_page("Average TOF", self.render_average)
        page.add_combo("scan", "Scan")
        page = self.add_page("Difference", self.render_difference)
        page.add_combo("scan", "Scan")
        self.add_page("Overlay", self.render_overlay)
        self.add_page("Overlay difference", self.render_overlay_diff)
        page = self.add_page("Gated per point", self.render_gated)
        page.add_combo("scan", "Scan")
        page = self.add_page("Chunks", self.render_chunks)
        page.add_combo("scan", "Scan")
        self.add_table("scans", "Gated ratios")
        self.plot_tabs.setCurrentIndex(1)

    def check_scan(self, arrays):
        p = arrays["settings"]["output plugin"] or ""
        if "NullOutputPlugin" not in p:
            return "has a scanned parameter (%s) - the TOF collector averages over it" % p.split(".")[-1]
        return ""

    def four_shot(self, arrays, s):
        if s["FourShot"] == "auto":
            return "FourShot" in (arrays["settings"]["pattern plugin"] or "")
        return s["FourShot"] == "yes"

    def tof_traces(self, arrays, point, s):
        if self.four_shot(arrays, s) and arrays["on"].shape[0] > 1:
            traces = []
            for shot, label in [("on", "On"), ("off", "Off")]:
                tofs = EDM.DetectorTOFs(arrays, shot, 0) - EDM.DetectorTOFs(arrays, shot, 1)
                traces.append((label + ", YAG On-Off", arrays["time " + shot]*1000, self._point_mean(tofs, point)))
            return traces
        return super().tof_traces(arrays, point, s)

    # ------------------------------------------------------------ analysis
    def analyse(self, s):
        results = {}
        for label, arrays in self.scans.items():
            if arrays["off"] is None:
                print("%s skipped: no Off shots" % label)
                continue
            four = self.four_shot(arrays, s)
            if four and arrays["on"].shape[0] < 2:
                raise ValueError("%s: 4-shot needs 2 detectors (YAG On and Off), it has %d"
                                 % (label, arrays["on"].shape[0]))
            bkg = (None, None) if four else (s["BkgStart"], s["BkgEnd"])
            r = {"four shot": four}
            for shot in ["on", "off"]:
                traces = EDM.TOFTraces(arrays, shot, s["detector"], bkg[0], bkg[1], FourShot=four)
                r[shot + " traces"] = traces
                r[shot], r[shot + " err"] = EDM.MeanAndSEM(traces, axis=0, ddof=1)
                r["time " + shot] = arrays["time " + shot]*1000
            r["diff"] = r["on"] - r["off"]
            r["diff err"] = np.sqrt(r["on err"]**2 + r["off err"]**2)
            if four:
                g = EDM.FourShotGated(arrays, s["SigStart"], s["SigEnd"])
                tconv = EDM.ScanTconv(arrays)
                g = {k: v*tconv for k, v in g.items()}
            else:
                g = EDM.GatedOnOff(arrays, s["SigStart"], s["SigEnd"], s["BkgStart"], s["BkgEnd"], s["detector"])
            ratio = g["on"]/g["off"]
            r["gated"] = g
            r["ratio"] = ratio
            r["ratio mean"], r["ratio sem"] = EDM.MeanAndSEM(ratio)
            r["on mean"], r["on sem"] = EDM.MeanAndSEM(g["on"])
            r["off mean"], r["off sem"] = EDM.MeanAndSEM(g["off"])
            results[label] = r
            print("%s: %d shots, gated On/Off ratio = %.4g +- %.2g" % (
                label, len(r["on traces"]), r["ratio mean"], r["ratio sem"]))
        if not results:
            raise ValueError("None of the loaded scans have On and Off shots")
        return {"scans": results}

    def fill_tables(self):
        rows = []
        for label, r in self.results["scans"].items():
            rows.append({"scan": label, "shots": len(r["on traces"]), "4-shot": r["four shot"],
                         "gated On (ms.V)": r["on mean"], "On SEM": r["on sem"],
                         "gated Off (ms.V)": r["off mean"], "Off SEM": r["off sem"],
                         "On/Off ratio": r["ratio mean"], "ratio SEM": r["ratio sem"]})
        self.tables["scans"].set_rows(rows)

    def overlay_labels(self):
        ids = set(data.parse_ids(self.last_settings["Overlay"]))
        return [l for l in self.results["scans"] if not ids or data.label_id(l) in ids]

    # ------------------------------------------------------------ plots
    def _scan(self, page, fig):
        r = self.results["scans"].get(page.value("scan"))
        if r is None:
            show_message(fig, "No On/Off data for this scan")
        return r

    def _gates(self, ax, r):
        gate_lines(ax, self.last_settings, ("SigStart", "SigEnd") if r["four shot"] else
                   ("SigStart", "SigEnd", "BkgStart", "BkgEnd"))

    def render_average(self, page, fig):
        r = self._scan(page, fig)
        if r is None:
            return
        ax = fig.add_subplot()
        for i, shot in enumerate(["on", "off"]):
            t = r["time " + shot]
            ax.plot(t, r[shot], color=color(i), label=shot.capitalize())
            ax.fill_between(t, r[shot] - r[shot + " err"], r[shot] + r[shot + " err"], color=color(i), alpha=0.3)
        self._gates(ax, r)
        ax.set_title("%s: TOF averaged over %d shots, %s" % (
            page.value("scan"), len(r["on traces"]), "YAG Off subtracted" if r["four shot"] else "background subtracted"))
        ax.set_xlabel("time (ms)")
        ax.set_ylabel("PMT signal (V)")
        ax.legend()

    def render_difference(self, page, fig):
        r = self._scan(page, fig)
        if r is None:
            return
        ax = fig.add_subplot()
        t = r["time on"]
        ax.plot(t, r["diff"], color=color(0), label="On-Off")
        ax.fill_between(t, r["diff"] - r["diff err"], r["diff"] + r["diff err"], color=color(0), alpha=0.3)
        MA = max(1, min(self.last_settings["MA"], len(t)))
        if MA > 1:
            ax.plot(tools.MovingAverage(MA, t), tools.MovingAverage(MA, r["diff"]), color=color(1),
                    label="Moving average of %d" % MA)
        self._gates(ax, r)
        ax.axhline(0, color="0.5", linewidth=0.8)
        ax.set_title("%s: averaged TOF difference (On-Off) over %d shots" % (page.value("scan"), len(r["on traces"])))
        ax.set_xlabel("time (ms)")
        ax.set_ylabel("PMT signal (V)")
        ax.legend()

    def render_overlay(self, page, fig):
        labels = self.overlay_labels()
        if not labels:
            show_message(fig, "No scans to overlay")
            return
        ax = fig.add_subplot()
        for i, l in enumerate(labels):
            r = self.results["scans"][l]
            ax.plot(r["time on"], r["on"], color=color(i), label=l + " On")
            ax.fill_between(r["time on"], r["on"] - r["on err"], r["on"] + r["on err"], color=color(i), alpha=0.3)
            ax.plot(r["time off"], r["off"], "--", color=color(i), label=l + " Off")
            ax.fill_between(r["time off"], r["off"] - r["off err"], r["off"] + r["off err"], color=color(i), alpha=0.15)
        gate_lines(ax, self.last_settings)
        ax.set_title("Averaged TOFs, On solid, Off dashed")
        ax.set_xlabel("time (ms)")
        ax.set_ylabel("PMT signal (V)")
        ax.legend(fontsize=8, ncol=2)

    def render_overlay_diff(self, page, fig):
        labels = self.overlay_labels()
        if not labels:
            show_message(fig, "No scans to overlay")
            return
        MA = max(1, self.last_settings["MA"])
        ax = fig.add_subplot()
        for i, l in enumerate(labels):
            r = self.results["scans"][l]
            m = min(MA, len(r["time on"]))
            ax.plot(tools.MovingAverage(m, r["time on"]), tools.MovingAverage(m, r["diff"]), color=color(i),
                    label=l + " On-Off")
        gate_lines(ax, self.last_settings)
        ax.axhline(0, color="0.5", linewidth=0.8)
        ax.set_title("Averaged TOF difference (On-Off), moving average of %d" % MA)
        ax.set_xlabel("time (ms)")
        ax.set_ylabel("PMT signal (V)")
        ax.legend(fontsize=8, ncol=2)

    def render_gated(self, page, fig):
        r = self._scan(page, fig)
        if r is None:
            return
        g = r["gated"]
        points = np.arange(len(g["on"]))
        ax1, ax2 = fig.subplots(2, 1, sharex=True)
        ax1.errorbar(points, g["on"], yerr=g["on err"], fmt=".", color=color(0), label="On")
        ax1.errorbar(points, g["off"], yerr=g["off err"], fmt=".", color=color(1), label="Off")
        ax1.set_ylabel("Gated LIF (ms.V)")
        ax1.set_title("%s: gated TOF of each scan point, %g-%g ms gate" % (
            page.value("scan"), self.last_settings["SigStart"], self.last_settings["SigEnd"]))
        ax1.legend()
        ax2.plot(points, r["ratio"], ".", color=color(0))
        ax2.axhline(r["ratio mean"], color=color(1),
                    label="mean %.4g ± %.2g" % (r["ratio mean"], r["ratio sem"]))
        ax2.axhspan(r["ratio mean"] - r["ratio sem"], r["ratio mean"] + r["ratio sem"], color=color(1), alpha=0.2)
        ax2.set_xlabel("Scan point (in the order taken)")
        ax2.set_ylabel("On/Off ratio")
        ax2.legend()

    def render_chunks(self, page, fig):
        r = self._scan(page, fig)
        if r is None:
            return
        n = max(1, self.last_settings["Chunk"])
        on_chunks = EDM.ChunkMeans(r["on traces"], n)
        off_chunks = EDM.ChunkMeans(r["off traces"], n)
        cols = colormaps["jet"](np.linspace(0, 1, max(2, len(on_chunks))))
        ax = fig.add_subplot()
        for i, ((a, b, on, on_err), (_, _, off, off_err)) in enumerate(zip(on_chunks, off_chunks)):
            ax.plot(r["time on"], on, color=cols[i], label="On, shots %d to %d" % (a, b))
            ax.fill_between(r["time on"], on - on_err, on + on_err, color=cols[i], alpha=0.2)
            ax.plot(r["time off"], off, "--", color=cols[i], label="Off, shots %d to %d" % (a, b))
        self._gates(ax, r)
        ax.set_title("%s: TOFs averaged over chunks of %d shots" % (page.value("scan"), n))
        ax.set_xlabel("time (ms)")
        ax.set_ylabel("PMT signal (V)")
        ax.legend(fontsize=7, ncol=2)
