"""Regenerate the Zeeman quick-reference files in this folder from ybf.zeeman.

    python ybf/reference/make_zeeman_reference.py        (from LatticeEDMScripts)

Writes ybf_zeeman_X_N1_A_J12.png and zeeman_levels.csv next to this script. The CSV has one row per state and
field: state, level (zero-field label), mF, B_G, E_MHz (from the zero-field centroid of the manifold), for
X(v=0, N=1) and A2Pi1/2(v'=0) J'=1/2 e(+) and f(-), B = 0-100 G in 0.25 G steps. The A-state model is within
J'=1/2 only: above about 60 G mixing with J'=3/2 shifts levels by more than 0.1 MHz (the plot stops at 60 G).
"""
import csv
import sys
from pathlib import Path

import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE.parents[1]))
from ybf import zeeman  # noqa: E402

B = np.linspace(0, 100, 401)
X = zeeman.X_zeeman_levels(B)
A = {p: zeeman.A12_zeeman_levels(B, p) for p in ("e", "f")}

with open(HERE / "zeeman_levels.csv", "w", newline="") as f:
    w = csv.writer(f)
    w.writerow(["state", "level", "mF", "B_G", "E_MHz"])
    for name, curves in (("X_v0_N1", X), ("A_J1/2_e", A["e"]), ("A_J1/2_f", A["f"])):
        for c in curves:
            for b, e in zip(B, c["E"]):
                w.writerow([name, c["label"], int(c["mF"]), round(float(b), 3), round(float(e), 5)])

import matplotlib  # noqa: E402
matplotlib.use("Agg")
import matplotlib.pyplot as plt  # noqa: E402

ink, ink2, grid, surf = "#0b0b0b", "#52514e", "#e4e3df", "#fcfcfb"
pal = ["#2a78d6", "#eb6834", "#1baf7a", "#eda100"]          # categorical slots 1-4, fixed order
fig, axs = plt.subplots(1, 2, figsize=(12, 6), gridspec_kw=dict(width_ratios=[1.3, 1]), facecolor=surf)
panels = [(axs[0], X, ["F=1h", "F=2", "F=0", "F=1l"], "X²Σ⁺(v=0, N=1)", 100),
          (axs[1], A["e"], ["F'=0", "F'=1"], "A²Π₁/₂(v′=0) J′=½ e(+)", 60)]
for ax, curves, order, title, Bmax in panels:
    ax.set_facecolor(surf)
    col = {lab: pal[i] for i, lab in enumerate(order)}
    sel = B <= Bmax
    for c in curves:
        ax.plot(B[sel], c["E"][sel], color=col[c["label"]], lw=2, solid_capstyle="round")
    for lab in order:
        ax.plot([], [], color=col[lab], lw=2, label=lab)
    ax.set_title(title, color=ink, fontsize=13, loc="left")
    ax.set_xlabel("Magnetic field (G)", color=ink2)
    ax.set_ylabel("Energy from zero-field centroid (MHz)", color=ink2)
    ax.grid(color=grid, lw=0.8)
    ax.set_axisbelow(True)
    for s in ax.spines.values():
        s.set_color(grid)
    ax.tick_params(colors=ink2)
    ax.legend(frameon=False, labelcolor=ink, loc="upper left")
    ax.set_xlim(0, Bmax)
axs[1].text(0.03, 0.03, "F′=1 top to bottom: m′ = −1, 0, +1\nwithin-J′ model; J′=3/2 f(+) mixing\n> 0.1 MHz above ~60 G",
            transform=axs[1].transAxes, ha="left", va="bottom", fontsize=9, color=ink2)
fig.suptitle("174YbF Zeeman levels (ybf.zeeman): ybf X constants, Sauer 1999 A hyperfine, Ma 2009 fit B g-factors",
             color=ink, fontsize=12, x=0.01, ha="left")
fig.tight_layout()
fig.savefig(HERE / "ybf_zeeman_X_N1_A_J12.png", dpi=150, facecolor=surf)
print("wrote", HERE / "zeeman_levels.csv", "and", HERE / "ybf_zeeman_X_N1_A_J12.png")
