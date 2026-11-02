"""Duration scans (v0Duration): slowing On/Off ratio against slowing duration,
fitted with an exponential decay, as Duration_batch_SL.py."""

import numpy as np

import LatticeEDM_analysis_library as EDM
import tools
from analysis_gui import data
from analysis_gui.modes.base import ModeTab, color, show_message

# Branching ratios back to the cycle (Duration_batch_SL.py)
B = [0.93178, 0.06474, 0.00284, 0.00010]
BR_CHOICES = ["v0 (b0)", "up to v1 (b0+b1)", "up to v2 (b0+b1+b2)", "up to v3 (b0+...+b3)"]


class DurationTab(ModeTab):
    NAME = "Duration"
    KEY = "duration"
    KEYWORD = "Duration"
    DESCRIPTION = ("V0 duration scans: gated On/Off ratio against slowing duration, "
                   "fitted with A exp(-t/tau) + C, as Duration_batch_SL.py.")
    SETTINGS = [
        ("--", "Gates (ms)"),
        ("SigStart", "Signal start", "float", 24),
        ("SigEnd", "Signal end", "float", 30),
        ("BkgStart", "Background start", "float", 70),
        ("BkgEnd", "Background end", "float", 78),
        ("detector", "Detector", "int", 0, "TOF channel (0 = first, usually the PMT)"),
        ("--", "Fits"),
        ("BR", "Branching ratio", BR_CHOICES, BR_CHOICES[0],
         "Branching ratio back into the cycle, for the scattering rate -1/(tau ln BR)"),
        ("Weighted", "Weighted group fits", "bool", True,
         "Group fits are weighted by the standard error of the averaged scans (absolute sigma)"),
        ("UseIndivP0", "Group fit starts from scan fits", "bool", True,
         "Initial guess of a group fit = mean of the good individual fits of its scans"),
        ("p0_default", "Default guess A, tau, C", "floats", "0.5, 2500, 0.5"),
        ("Exclude", "Exclude scans", "text", "",
         "Scans to leave out of the averages, e.g. 003_0, 004_0"),
    ]
    GROUPS = (("Group", "File IDs"),
              "File IDs averaged together, e.g. 002, 003. No groups: all scans with a good fit are averaged.")

    def build(self):
        self.add_tof_page()
        page = self.add_page("Scan", self.render_scan)
        page.add_combo("scan", "Scan")
        self.add_page("All scans", self.render_all)
        self.add_page("Groups", self.render_groups)
        self.add_table("scans", "Scan fits")
        self.add_table("groups", "Group fits")
        self.plot_tabs.setCurrentIndex(3)

    def check_scan(self, arrays):
        p = arrays["settings"]["output plugin"] or ""
        if "PGOutputPlugin" not in p:
            return "doesn't look like a duration scan (%s)" % p.split(".")[-1]
        return ""

    # ------------------------------------------------------------ analysis
    def analyse(self, s):
        BR = sum(B[:BR_CHOICES.index(s["BR"]) + 1])
        exclude = set(data.parse_ids(s["Exclude"]))
        p0_default = s["p0_default"] or [0.5, 2500., 0.5]
        if len(p0_default) != 3:
            raise ValueError("Default guess needs 3 numbers: A, tau, C")

        scans = {}
        for label, arrays in self.scans.items():
            R = EDM.GatedOnOff(arrays, s["SigStart"], s["SigEnd"], s["BkgStart"], s["BkgEnd"],
                               s["detector"])
            x = np.asarray(arrays["scan params"], dtype=float)
            end = arrays["settings"]["end"] or np.max(x)
            # DurationRunSingle: unweighted, p0 = [1, end/3, 0]
            fit = EDM.FitDecay(x, R["ratio"], [1., end/3, 0.])
            rate = EDM.ScatteringRate(fit["best fit"][1], fit["error"][1], BR)
            scans[label] = dict(R, x=x, fit=fit, rate=rate)
            print("%s: tau = %.5g +- %.4g us%s" % (label, fit["best fit"][1], fit["error"][1],
                                                  "" if fit["ok"] else "  (bad fit, not averaged)"))

        if s["groups"]:
            groups = [(name, data.parse_ids(ids)) for name, ids in s["groups"]]
        else:
            groups = [("All good fits", None)]

        group_results = {}
        for name, ids in groups:
            members = [l for l in scans
                       if (ids is None and scans[l]["fit"]["ok"]) or
                       (ids is not None and data.label_id(l) in ids)]
            for l in [m for m in members if m in exclude]:
                print("Excluding %s from %s" % (l, name))
            members = [m for m in members if m not in exclude]
            if not members:
                print(name + ": no scans left - skipped")
                continue
            n_points = {len(scans[m]["x"]) for m in members}
            if len(n_points) > 1:
                print(name + ": scans have different numbers of points %s - skipped" % sorted(n_points))
                continue
            x = scans[members[0]]["x"]
            p0 = p0_default
            if s["UseIndivP0"]:
                good = [m for m in members if scans[m]["fit"]["ok"]] or \
                       [m for m in members if np.all(np.isfinite(scans[m]["fit"]["best fit"]))]
                if good:
                    p0 = np.mean([scans[m]["fit"]["best fit"] for m in good], axis=0)
            avg, sem, fit = EDM.AverageDecayGroup([scans[m]["ratio"] for m in members], x, p0,
                                                  s["Weighted"])
            rate = EDM.ScatteringRate(fit["best fit"][1], fit["error"][1], BR)
            group_results[name] = dict(members=members, x=x, avg=avg, sem=sem, fit=fit, rate=rate)
            print("%s (%s): tau = %.5g +- %.4g us%s" % (name, ", ".join(members), fit["best fit"][1],
                                                       fit["error"][1], "" if fit["ok"] else " (bad fit)"))
        return {"scans": scans, "groups": group_results, "BR": BR}

    def fill_tables(self):
        rows = []
        for label, r in self.results["scans"].items():
            f, e = r["fit"]["best fit"], r["fit"]["error"]
            settings = self.scans[label]["settings"]
            rows.append({"scan": label, "points": len(r["x"]), "shots/point": settings["shotsPerPoint"],
                         "tau (us)": f[1], "tau err": e[1], "A": f[0], "A err": e[0],
                         "C": f[2], "C err": e[2], "good fit": r["fit"]["ok"],
                         "scattering rate (MHz)": r["rate"][0], "rate err": r["rate"][1]})
        self.tables["scans"].set_rows(rows)
        rows = []
        for name, g in self.results["groups"].items():
            f, e = g["fit"]["best fit"], g["fit"]["error"]
            rows.append({"group": name, "scans": " ".join(g["members"]),
                         "tau (us)": f[1], "tau err": e[1], "A": f[0], "A err": e[0],
                         "C": f[2], "C err": e[2], "weighted": g["fit"]["weighted"],
                         "good fit": g["fit"]["ok"], "BR": self.results["BR"],
                         "scattering rate (MHz)": g["rate"][0], "rate err": g["rate"][1]})
        self.tables["groups"].set_rows(rows)

    # ------------------------------------------------------------ plots
    def xlabel(self, label):
        param = self.scans[label]["settings"]["pg param"]
        if param in (None, "v0Duration", "DurationV0"):
            return "V0 slowing duration (μs)"
        return str(param)

    def render_scan(self, page, fig):
        label = page.value("scan")
        if label not in self.results["scans"]:
            show_message(fig, "No scan")
            return
        r = self.results["scans"][label]
        s = self.last_settings
        ax1, ax2 = fig.subplots(2, 1, sharex=True)
        ax1.errorbar(r["x"], r["on"], yerr=r["on err"], fmt=".", color=color(0), label="Slowing On")
        ax1.errorbar(r["x"], r["off"], yerr=r["off err"], fmt=".", color=color(1), label="Slowing Off")
        ax1.set_ylabel("Gated LIF (ms.V)")
        ax1.set_title("%s, %g-%g ms gate, %s shots per point" % (
            label, s["SigStart"], s["SigEnd"], self.scans[label]["settings"]["shotsPerPoint"]))
        ax1.legend()
        ax2.plot(r["x"], r["ratio"], ".", color=color(0), label="On/Off")
        self._fit_line(ax2, r["x"], r["fit"], color(0))
        ax2.set_xlabel(self.xlabel(label))
        ax2.set_ylabel("On/Off ratio")
        ax2.legend()

    @staticmethod
    def _fit_line(ax, x, fit, c, prefix=""):
        f, e = fit["best fit"], fit["error"]
        if not np.all(np.isfinite(f)):
            return
        span = np.linspace(min(0, np.min(x)), np.max(x), 500)
        ax.plot(span, tools.exp_decay(span, *f), color=c, linestyle="-" if fit["ok"] else ":",
                label=prefix + r"$\tau$ = %.5g ± %.3g μs%s" % (f[1], e[1], "" if fit["ok"] else " (bad fit)"))

    def render_all(self, page, fig):
        ax = fig.add_subplot()
        for i, (label, r) in enumerate(self.results["scans"].items()):
            ax.plot(r["x"], r["ratio"], ".", color=color(i), label=label)
            self._fit_line(ax, r["x"], r["fit"], color(i))
        ax.set_xlabel(self.xlabel(next(iter(self.results["scans"]))))
        ax.set_ylabel("On/Off ratio")
        ax.set_title("All scans, %g-%g ms gate" % (self.last_settings["SigStart"], self.last_settings["SigEnd"]))
        ax.legend(fontsize=8, ncol=2)

    def render_groups(self, page, fig):
        groups = self.results["groups"]
        if not groups:
            show_message(fig, "No groups to show - check the group file IDs and exclusions")
            return
        ax = fig.add_subplot()
        for i, (name, g) in enumerate(groups.items()):
            ax.plot(g["x"], g["avg"], ".", color=color(i),
                    label="%s (%d scan%s)" % (name, len(g["members"]), "" if len(g["members"]) == 1 else "s"))
            if len(g["members"]) > 1:
                ax.fill_between(g["x"], g["avg"] - g["sem"], g["avg"] + g["sem"], color=color(i), alpha=0.3)
            self._fit_line(ax, g["x"], g["fit"], color(i))
        ax.set_xlabel(self.xlabel(groups[next(iter(groups))]["members"][0]))
        ax.set_ylabel("On/Off ratio")
        ax.set_title("Averaged pumping curves%s, %g-%g ms gate" % (
            " with weighted fit" if self.last_settings["Weighted"] else "",
            self.last_settings["SigStart"], self.last_settings["SigEnd"]))
        ax.legend(fontsize=8)
