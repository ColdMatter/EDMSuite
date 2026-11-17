#!/usr/bin/env python3
"""Noise left by each degauss condition, then the eEDM-band samples.

One representative 900 s Z record per row of the report's degauss table:

  below knee   last pre-degauss after the 0.1 A x 40 cycle (07-28)
  series N=16  22.4 A-turns, t0+46 min (07-29)
  series N=8   20.8-25.6 A-turns, t0+40 min (07-30)
  series N=4   18 A-turns, t0+16 min (08-03)

The parallel 8 A vendor recipe is NOT here, and the omission is a
decision rather than a gap: its only record in the t0+16..46 min family
this figure compares is a relaxation record (relax0817_t32m, 08-17),
and relaxation data is kept out of the report (2026-08-18). The other
08-17 record of that cycle, postdg0817_8a, sits at t0+4 min -- inside
the settling transient and on the other input range -- so it is not a
substitute. Restoring the row needs a post-degauss record of the
parallel cycle taken in the same window as the rest.

Top row: full spliced ASD, one panel per condition, shared axes.
Bottom: the same records sampled at seven frequencies in the eEDM
decade 0.01-1 Hz (design reversal 0.05 Hz, first-lattice reversal
0.25 Hz, both window edges), plotted together as polylines.

Canonical window: first 900 s, 200 s Welch, linear detrend. The seven
band points use that window throughout (they all sit at or below 1 Hz).
The full ASD splice (200 s below 1 Hz, 20 s above) is only for the
shape panels.

Math is the stdlib Hann / linear-detrend / one-sided density used by
plot_postdg0817.py (same scaling as scipy.signal.welch).  The figure is
drawn with gnuplot so the script does not depend on numpy/matplotlib.

Writes fig_degauss_conditions.png next to this script.
"""

from __future__ import annotations

import math
import os
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
import sys as _sys  # analysis_scripts/ on sys.path for _paths
_sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from _paths import DATA_ROOT, PROJECT_DIR, ANALYSIS_DIR, data, data_join, rel_data  # noqa: E402,F401
REPO = PROJECT_DIR

FS = 1200.0
WIN_S = 900.0
SEG_S = 200.0
HI_S = 20.0
SPLIT_HZ = 1.0
TARGET_FT = 50.0

# eEDM decade: reversal and window edge of both lattice generations,
# plus the decade floor and ceiling (Section band of the report).
BAND_HZ = (0.01, 0.02, 0.05, 0.10, 0.25, 0.50, 1.00)

RUNS = [
    dict(key="below",
         title="below knee",
         legend="below knee (4 A·t)",
         path=("result/ms1l_2_degaussfail/swapref_oldhead_900s/"
               "swapref_old_Z_pT.txt"),
         color="#7A7F85"),
    dict(key="n16",
         title="N=16 series",
         legend="N=16  22.4 A·t",
         path=("result/ms1l_3_degaussed/postdegauss0729_900s/"
               "postdg0729_Z_pT.txt"),
         color="#C0504D"),
    dict(key="n8",
         title="N=8 series",
         legend="N=8  20.8–25.6 A·t",
         path=("result/ms1l_3_degaussed/postdg0730_n8_2nd_900s/"
               "postdg_n8_2nd_Z_pT.txt"),
         color="#2E5FA3"),
    dict(key="n4",
         title="N=4 series",
         legend="N=4  18 A·t",
         path=("result/ms1l_3_degaussed/postdg0803_n4_900s/"
               "postdg_n4_Z_pT.txt"),
         color="#4E7B52"),
]


# ------------------------------------------------------------------ math
# Hann / linear detrend / one-sided density: scipy.signal.welch defaults.
# Copied from plot_postdg0817.py so a numpy-less environment still runs.


def loadtxt(path):
    out = []
    with open(path, encoding="utf-8") as fh:
        for line in fh:
            line = line.strip()
            if not line or line.startswith("#"):
                continue
            out.append(float(line.split()[0]))
    return out


def detrend_linear(x):
    n = len(x)
    if n < 2:
        return list(x)
    sx = sy = sxx = sxy = 0.0
    for i, y in enumerate(x):
        sx += i
        sy += y
        sxx += i * i
        sxy += i * y
    den = n * sxx - sx * sx
    slope = (n * sxy - sx * sy) / den
    intercept = (sy - slope * sx) / n
    return [y - (intercept + slope * i) for i, y in enumerate(x)]


def hann_periodic(n):
    twopi = 2.0 * math.pi
    return [0.5 - 0.5 * math.cos(twopi * i / n) for i in range(n)]


def goertzel_abs2(x, k):
    n = len(x)
    w = 2.0 * math.pi * k / n
    coeff = 2.0 * math.cos(w)
    s1 = s2 = 0.0
    for v in x:
        s0 = v + coeff * s1 - s2
        s2 = s1
        s1 = s0
    re = s1 - s2 * math.cos(w)
    im = s2 * math.sin(w)
    return re * re + im * im


def welch_asd_at(x, fs, nperseg, freqs):
    """One-sided ASD (same units as x / sqrt(Hz)) at the listed freqs."""
    n = nperseg
    hop = n // 2
    win = hann_periodic(n)
    wss = sum(w * w for w in win)
    scale = 1.0 / (fs * wss)
    nseg = max(1, 1 + (len(x) - n) // hop)
    acc = [0.0] * len(freqs)
    ks = [int(round(f * n / fs)) for f in freqs]
    used = 0
    for s in range(nseg):
        start = s * hop
        seg = x[start:start + n]
        if len(seg) < n:
            break
        d = detrend_linear(seg)
        wd = [d[i] * win[i] for i in range(n)]
        for j, k in enumerate(ks):
            if k <= 0 or k >= n:
                continue
            p = goertzel_abs2(wd, k) * scale
            if 0 < k < n // 2:
                p *= 2.0
            acc[j] += p
        used += 1
    if used == 0:
        return [float("nan")] * len(freqs)
    return [math.sqrt(a / used) for a in acc]


def logspace(lo, hi, n):
    a, b = math.log10(lo), math.log10(hi)
    if n == 1:
        return [10 ** a]
    return [10 ** (a + (b - a) * i / (n - 1)) for i in range(n)]


def nearest_grid(f, nperseg, fs):
    k = max(1, int(round(f * nperseg / fs)))
    k = min(k, nperseg // 2 - 1)
    return k * fs / nperseg


def unique_keep(seq):
    seen, out = set(), []
    for v in seq:
        if v not in seen:
            seen.add(v)
            out.append(v)
    return out


def spliced_curve(x, fs):
    """Log-spaced ASD: 200 s below 1 Hz, 20 s above (plot_relax_all)."""
    nlo, nhi = int(SEG_S * fs), int(HI_S * fs)
    flo = unique_keep(
        nearest_grid(f, nlo, fs)
        for f in logspace(0.005, SPLIT_HZ, 24)
        if 0 < f < SPLIT_HZ)
    fhi = unique_keep(
        nearest_grid(f, nhi, fs)
        for f in logspace(SPLIT_HZ, 80.0, 28)
        if f >= SPLIT_HZ)
    alo = welch_asd_at(x, fs, nlo, flo)
    ahi = welch_asd_at(x, fs, nhi, fhi)
    return flo + fhi, alo + ahi


def band_samples(x, fs, freqs):
    n = min(len(x), int(SEG_S * fs))
    used = [nearest_grid(f, n, fs) for f in freqs]
    asd = welch_asd_at(x, fs, n, used)
    return used, [v * 1e3 for v in asd]  # pT -> fT


# ------------------------------------------------------------------ I/O


def load_runs():
    rows = []
    nwin = int(WIN_S * FS)
    for spec in RUNS:
        path = data_join(spec["path"])
        print(f"  load {spec['key']}: {os.path.relpath(path, DATA_ROOT)}",
              flush=True)
        x = loadtxt(path)[:nwin]
        f, a = spliced_curve(x, FS)
        used, samples = band_samples(x, FS, BAND_HZ)
        rows.append(dict(spec=spec, f=f, a=[v * 1e3 for v in a],
                         used=used, samples=samples))
    return rows


def print_table(rows):
    hdr = f"{'condition':<28s}" + "".join(f"{f:>10.2f} Hz" for f in BAND_HZ)
    print("canonical 200 s Welch, first 900 s, Z, fT/rtHz", flush=True)
    print(hdr, flush=True)
    print("-" * len(hdr), flush=True)
    for r in rows:
        line = f"{r['spec']['legend']:<28s}"
        for v in r["samples"]:
            line += f"{v:10.0f}"
        print(line, flush=True)
    print("\nWelch bin actually used:", flush=True)
    print("  requested:", ", ".join(f"{f:g}" for f in BAND_HZ), flush=True)
    print("  used:     ", ", ".join(f"{f:g}" for f in rows[0]["used"]),
          flush=True)


def write_dat(rows, asd_path, band_path):
    # asd: one file per run, columns f, asd
    for r in rows:
        path = os.path.join(os.path.dirname(asd_path),
                            f"_asd_{r['spec']['key']}.dat")
        r["asd_dat"] = path
        with open(path, "w", encoding="utf-8") as fh:
            fh.write("# f_Hz  ASD_fT\n")
            for f, a in zip(r["f"], r["a"]):
                fh.write(f"{f:.8g} {a:.8g}\n")
        path = os.path.join(os.path.dirname(band_path),
                            f"_band_{r['spec']['key']}.dat")
        r["band_dat"] = path
        with open(path, "w", encoding="utf-8") as fh:
            fh.write("# f_Hz  ASD_fT\n")
            for f, a in zip(r["used"], r["samples"]):
                fh.write(f"{f:.8g} {a:.8g}\n")
    # combined band table for the polyline panel
    with open(band_path, "w", encoding="utf-8") as fh:
        keys = [r["spec"]["key"] for r in rows]
        fh.write("# f_Hz  " + "  ".join(keys) + "\n")
        n = len(rows[0]["used"])
        for i in range(n):
            fh.write(f"{rows[0]['used'][i]:.8g}")
            for r in rows:
                fh.write(f" {r['samples'][i]:.8g}")
            fh.write("\n")


def gnuplot_script(rows, png_path, band_path):
    # 14.4 in x 7.2 in at 150 dpi
    wpx, hpx = 2160, 1080
    lines = [
        f'set terminal pngcairo size {wpx},{hpx} font "Helvetica,15" '
        "enhanced truecolor",
        f'set output "{png_path}"',
        "unset key",
        "set logscale xy",
        "set grid lw 0.4 lc rgb '#E4E6E8'",
        "set border 3 lw 1 lc rgb '#7A7F85'",
        "set tics nomirror out",
        "set xtics font ',12'",
        "set ytics font ',12'",
        "set xrange [0.005:80]",
        "set yrange [8:2.5e5]",
        "set xlabel 'frequency (Hz)' offset 0,0.4 font ',13'",
        "set format x '10^{%T}'",
        "set format y '10^{%T}'",
        "set style fill solid 1.0 noborder",
        "set multiplot",
        'set label 100 "(a)  full ASD, one panel per condition" '
        'at screen 0.015, 0.965 left font "Helvetica-Bold,16" '
        "tc rgb '#33373B'",
        'set label 101 "Noise left by each degauss condition  '
        '(Z, canonical window)" at screen 0.50, 0.985 centre '
        'font "Helvetica-Bold,18" tc rgb "#33373B"',
    ]

    # one small multiple per condition along the top
    # left margin 0.055, right 0.99, gap 0.012, top 0.93, bottom of row 0.52
    n = len(rows)
    left0, right = 0.055, 0.995
    gap = 0.010
    width = (right - left0 - (n - 1) * gap) / n
    top, bot = 0.93, 0.52
    for i, r in enumerate(rows):
        x0 = left0 + i * (width + gap)
        x1 = x0 + width
        spec = r["spec"]
        lines += [
            f"set origin {x0:.4f}, {bot:.4f}",
            f"set size {width:.4f}, {top - bot:.4f}",
            "unset xlabel" if i != n // 2 else
            "set xlabel 'frequency (Hz)' offset 0,0.6 font ',12'",
            "set ylabel 'ASD (fT/√Hz)' offset 1.2,0 font ',13'"
            if i == 0 else "unset ylabel",
            "set format y '10^{%T}'" if i == 0 else "set format y ''",
            "set ytics" if i == 0 else "unset ytics",
            "set xtics (0.01, 0.1, 1, 10)",
            "set obj 1 rect from 0.01, 8 to 1, 2.5e5 "
            "fc rgb '#F5E9D0' fs solid 0.85 noborder behind",
            "set arrow 1 from 0.05, graph 0 to 0.05, graph 1 nohead "
            "lw 1 dt 2 lc rgb '#B0B4B9' front",
            (f'set label 1 "50 fT" at 0.07, {TARGET_FT * 1.8} '
             "left font ',10' tc rgb '#4E7B52'") if i == 0 else "unset label 1",
            f'set title "{spec["title"]}" font "Helvetica-Bold,13" '
            f'tc rgb "{spec["color"]}" offset 0,-0.3',
            "plot "
            f'"{r["asd_dat"]}" using 1:2 with lines lw 1.8 '
            f'lc rgb "{spec["color"]}" notitle, '
            f'"{r["band_dat"]}" using 1:2 with points pt 7 ps 0.7 '
            f'lc rgb "{spec["color"]}" notitle, '
            f"{TARGET_FT} with lines lw 1 dt 3 lc rgb '#4E7B52' notitle",
            "unset obj 1",
            "unset arrow 1",
            "unset title",
        ]

    # bottom polyline panel
    lines += [
        "unset label 1",
        "unset title",
        "set origin 0.055, 0.07",
        "set size 0.94, 0.38",
        "set xlabel 'frequency in the eEDM decade (Hz)' offset 0,0.4 "
        "font ',13'",
        "set ylabel 'ASD (fT/√Hz)' offset 1.2,0 font ',13'",
        "set format y '10^{%T}'",
        "set ytics",
        "set xrange [0.008:1.35]",
        "set yrange [30:2.2e5]",
        "set xtics (" + ", ".join(f'"{f:g}" {f}' for f in BAND_HZ) + ")",
        "set format x",
        "set obj 2 rect from 0.008, 3e3 to 1.35, 8e3 "
        "fc rgb '#E8E2D4' fs solid 0.55 noborder behind",
        'set label 200 "(b)  eEDM-band samples, every condition on one axis" '
        "at graph 0.0, 1.08 left font 'Helvetica-Bold,15' tc rgb '#33373B'",
        'set label 201 "series plateau 3–8k" at 0.011, 3.5e3 left '
        "font ',11' tc rgb '#8A8064'",
        f'set label 202 "lattice eEDM target  50 fT/√Hz" '
        f"at 1.02, {TARGET_FT} left font ',11' tc rgb '#4E7B52'",
        'set label 203 "0.05 Hz" at 0.053, 1.3e5 left font \',11\' '
        "tc rgb '#7A7F85'",
        "set arrow 2 from 0.05, graph 0 to 0.05, graph 1 nohead "
        "lw 1 dt 2 lc rgb '#B0B4B9' front",
        "set key at graph 0.98, 0.96 right top samplen 1.6 spacing 1.05 "
        "font ',11' maxrows 3",
        "plot "
        + ", ".join(
            f'"{r["band_dat"]}" using 1:2 with linespoints lw 2.2 pt 7 ps 1.15 '
            f'lc rgb "{r["spec"]["color"]}" title "{r["spec"]["legend"]}"'
            for r in rows
        )
        + f", {TARGET_FT} with lines lw 1.2 dt 3 lc rgb '#4E7B52' notitle",
        "unset multiplot",
        "unset output",
    ]
    return "\n".join(lines) + "\n"


def plot(rows):
    png = os.path.join(HERE, "fig_degauss_conditions.png")
    band_path = os.path.join(HERE, "_band_all.dat")
    write_dat(rows, os.path.join(HERE, "_asd.dat"), band_path)
    gp = os.path.join(HERE, "_degauss_conditions.gp")
    with open(gp, "w", encoding="utf-8") as fh:
        fh.write(gnuplot_script(rows, png, band_path))
    print("  gnuplot", gp, flush=True)
    subprocess.check_call(["gnuplot", gp])
    scratch = [gp, band_path]
    for r in rows:
        scratch.extend([r.get("asd_dat"), r.get("band_dat")])
    for p in scratch:
        if p and os.path.isfile(p):
            os.remove(p)
    print(f"saved {os.path.relpath(png, REPO)}", flush=True)


def main():
    print("plot_degauss_conditions: stdlib Welch + gnuplot", flush=True)
    rows = load_runs()
    print_table(rows)
    plot(rows)
    return 0


if __name__ == "__main__":
    sys.exit(main())
