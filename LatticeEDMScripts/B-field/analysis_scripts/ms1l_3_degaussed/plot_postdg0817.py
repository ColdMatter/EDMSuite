#!/usr/bin/env python3
"""AB3S look at the 8 A parallel degauss: t0+70 h, then a second 8 A today.

Three 900 s records, one head (A10QHJ1ZA = AB3S, map z:ai2 y:ai3):

  relax0817_t70h   11:18  first HEALTHY-cal look at the 08-14 8 A state
                          (t0 = 2026-08-14T12:48:24, elapsed 70.5 h).
                          The 08-14 night R01_1904 had cal 15.90 and is
                          filed as void -- do not mix it in.
  postdg0817_8a    12:41  today's 8 A at 12:37, t0+4 min. Y railed 6.5 %
                          at +/-1 V -- Z is clean; Y is the unclipped
                          prefix only, if it is long enough.
  relax0817_t32m   13:08  same cycle, t0+32 min, +/-2 V, both axes clean.

Canonical numbers: 200 s Welch, linear detrend, 0.05 Hz in fT/rtHz.
Spliced ASD (200 s below 1 Hz, 20 s above) matches plot_relax_all.
White floor is the 30-45 Hz median of that splice. Drift and ptp are
on the first 900 s.

Compares to the standing plateau (trimmed mean 6566, range 3.8-9.3 k)
and to the 07-29 post-degauss Z (7.9 k / settled half 5.8 k, white 23,
drift -1.1 pT/min) recomputed with the same functions.

Writes fig_postdg0817_{asd,field}.svg next to this script, and PNG if
matplotlib imports. IROP_STDLIB=1 forces the stdlib Welch (same Hann /
linear-detrend / one-sided density scaling as scipy) -- used when the
numpy C extension is unavailable.
"""

from __future__ import annotations

import datetime as dt
import json
import math
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
import sys as _sys  # analysis_scripts/ on sys.path for _paths
_sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from _paths import DATA_ROOT, PROJECT_DIR, ANALYSIS_DIR, data, data_join, rel_data  # noqa: E402,F401
REPO = PROJECT_DIR
BUCKET = data_join("result", "ms1l_3_degaussed")

FS = 1200.0
WIN_S = 900.0
SEG_S = 200.0
HI_S = 20.0
SPLIT_HZ = 1.0
TARGET_FT = 50.0
RAIL_LIMIT = 0.001
PLATEAU_MEAN = 6566.0
PLATEAU_LO, PLATEAU_HI = 3776.0, 9312.0

T0_0814 = dt.datetime(2026, 8, 14, 12, 48, 24)
T0_0817 = dt.datetime(2026, 8, 17, 12, 37, 0)  # notes; no degauss json today

RUNS = [
    ("t70h  08-14 8A", "relax0817_t70h_900s/relax0817_t70h",
     T0_0814, "#2E6E8E"),
    ("t+4m  today 8A", "postdg0817_8a_900s/postdg0817_8a",
     T0_0817, "#C0504D"),
    ("t+32m today 8A", "relax0817_t32m_900s/relax0817_t32m",
     T0_0817, "#4E7B52"),
]
REF_0729 = "postdegauss0729_900s/postdg0729"


# ------------------------------------------------------------------ math
# Hann / linear detrend / one-sided density: the scipy.signal.welch
# defaults (window="hann", detrend="linear", scaling="density",
# noverlap=nperseg//2, fftbins=True). Bin k = round(f * nperseg / fs)
# is exact at 0.05 Hz for 200 s @ 1200 Hz (k = 10).


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


def median(vals):
    s = sorted(vals)
    n = len(s)
    if n == 0:
        return float("nan")
    if n % 2:
        return s[n // 2]
    return 0.5 * (s[n // 2 - 1] + s[n // 2])


def ptp(x):
    return max(x) - min(x)


def mean(x):
    return sum(x) / len(x) if x else float("nan")


def slope_per_min(x, fs):
    n = len(x)
    t = [i / fs / 60.0 for i in range(n)]
    st = sy = stt = sty = 0.0
    for i in range(n):
        st += t[i]
        sy += x[i]
        stt += t[i] * t[i]
        sty += t[i] * x[i]
    den = n * stt - st * st
    return (n * sty - st * sy) / den


def logspace(lo, hi, n):
    if lo <= 0 or hi <= 0:
        raise ValueError("logspace needs positive ends")
    a, b = math.log10(lo), math.log10(hi)
    if n == 1:
        return [10 ** a]
    return [10 ** (a + (b - a) * i / (n - 1)) for i in range(n)]


def nearest_grid(f, nperseg, fs):
    k = max(1, int(round(f * nperseg / fs)))
    k = min(k, nperseg // 2 - 1)
    return k * fs / nperseg


def spliced_curve(x, fs):
    """Log-spaced ASD: 200 s below 1 Hz, 20 s above (plot_relax_all)."""
    nlo, nhi = int(SEG_S * fs), int(HI_S * fs)
    flo = [nearest_grid(f, nlo, fs) for f in logspace(0.005, SPLIT_HZ, 24)
           if f < SPLIT_HZ]
    # unique, sorted
    seen, flo_u = set(), []
    for f in flo:
        if f not in seen and 0 < f < SPLIT_HZ:
            seen.add(f)
            flo_u.append(f)
    fhi = [nearest_grid(f, nhi, fs) for f in logspace(SPLIT_HZ, 80.0, 28)]
    seen, fhi_u = set(), []
    for f in fhi:
        if f not in seen and f >= SPLIT_HZ:
            seen.add(f)
            fhi_u.append(f)
    alo = welch_asd_at(x, fs, nlo, flo_u)
    ahi = welch_asd_at(x, fs, nhi, fhi_u)
    return flo_u + fhi_u, alo + ahi


def welch_200(x, fs, f0=0.05):
    n = min(len(x), int(SEG_S * fs))
    f = nearest_grid(f0, n, fs)
    return welch_asd_at(x, fs, n, [f])[0] * 1e3  # fT/rtHz if x is pT


def white_30_45(x, fs):
    n = min(len(x), int(HI_S * fs))
    # a dozen bins across 30-45 Hz is enough for the median floor;
    # every DFT bin would be 300 Goertzels x 89 segments.
    freqs = [nearest_grid(f, n, fs) for f in
             [30.0 + 15.0 * i / 11 for i in range(12)]]
    asd = welch_asd_at(x, fs, n, freqs)
    return median(asd) * 1e3


def measure(x, fs):
    nwin = min(len(x), int(WIN_S * fs))
    xw = x[:nwin]
    print(f"    welch {nwin/fs:.0f} s ...", flush=True)
    f, a = spliced_curve(xw, fs)
    return dict(
        n_s=nwin / fs,
        f=f, a=a,
        v005=welch_200(xw, fs),
        white=white_30_45(xw, fs),
        drift=slope_per_min(xw, fs),
        ptp=ptp(xw),
        mean=mean(xw),
    )


def rail_pT(meta):
    return 0.99 * float(meta["vrange"]) * float(meta["cal_pT_per_V"])


def unclipped_prefix(x, limit):
    for i, v in enumerate(x):
        if abs(v) >= limit:
            return x[:i]
    return x


def load_one(stem):
    base = os.path.join(BUCKET, stem)
    print(f"  load {stem}", flush=True)
    with open(base + ".meta.json", encoding="utf-8") as fh:
        meta = json.load(fh)
    start = dt.datetime.fromisoformat(meta["start_iso"])
    fs = float(meta["fs_hz"])
    lim = rail_pT(meta)
    axes, flags = {}, {}
    for ax in ("Z", "Y"):
        path = f"{base}_{ax}_pT.txt"
        if not os.path.exists(path):
            flags[ax] = "missing"
            continue
        x = loadtxt(path)
        frac = float((meta.get("rail_fraction") or {}).get(ax.lower(), 0.0))
        if frac > RAIL_LIMIT:
            pref = unclipped_prefix(x, lim)
            flags[ax] = (f"railed {frac:.1%}; unclipped prefix "
                         f"{len(pref)/fs:.0f} s")
            if len(pref) < 2 * SEG_S * fs:
                flags[ax] += " -- too short for 200 s Welch, skipped"
                continue
            print(f"    {ax} prefix {len(pref)/fs:.0f} s", flush=True)
            axes[ax] = measure(pref, fs)
            axes[ax]["clipped"] = True
        else:
            axes[ax] = measure(x, fs)
            axes[ax]["clipped"] = False
            flags[ax] = "ok"
    return dict(stem=stem, meta=meta, start=start, fs=fs,
                cal=meta.get("cal_factors") or {}, axes=axes, flags=flags)


def fmt_dt(minutes):
    if abs(minutes) >= 180:
        return f"{minutes/60:.1f} h"
    return f"{minutes:.0f} min"


def print_table(rows, ref):
    hdr = (f"{'run':<22s} {'t-t0':>8s}  ax  cal   vrange  flag     "
           f"0.05Hz  white   drift    ptp")
    print("canonical window: 200 s Welch, linear detrend, first 900 s")
    print("white = 30-45 Hz median of spliced ASD (200 s / 20 s @ 1 Hz)")
    print("drift is the signed OLS slope in pT/min; ptp in pT\n")
    print(hdr)
    print("-" * len(hdr))
    for r in rows:
        vr = f"+/-{r['meta']['vrange']:g}V"
        dtlab = fmt_dt(r["dt_min"])
        for ax in ("Z", "Y"):
            cal = r["cal"].get(ax.lower())
            cal_s = f"{cal:.2f}" if cal is not None else "—"
            if ax not in r["axes"]:
                print(f"{r['label']:<22s} {dtlab:>8s}  {ax}  {cal_s:<5} "
                      f"{vr:>5}  SKIP     {r['flags'].get(ax, '')}")
                continue
            m = r["axes"][ax]
            flag = "prefix" if m.get("clipped") else "ok"
            print(f"{r['label']:<22s} {dtlab:>8s}  {ax}  {cal_s:<5} "
                  f"{vr:>5}  {flag:<8s} {m['v005']:6.0f}  {m['white']:5.1f}  "
                  f"{m['drift']:+6.2f}  {m['ptp']:6.1f}")
    print()
    z = ref["axes"]["Z"]
    print(f"07-29 post-degauss Z (same functions): 0.05 Hz {z['v005']:.0f}  "
          f"white {z['white']:.1f}  drift {z['drift']:+.2f} pT/min  "
          f"ptp {z['ptp']:.1f} pT")
    print(f"previous plateau (n=10 trimmed mean): {PLATEAU_MEAN:.0f} "
          f"fT/rtHz, range {PLATEAU_LO:.0f}-{PLATEAU_HI:.0f}")
    print()
    print("08-17 Z vs plateau:")
    for r in rows:
        if "Z" not in r["axes"]:
            continue
        v = r["axes"]["Z"]["v005"]
        inside = PLATEAU_LO <= v <= PLATEAU_HI
        print(f"  {r['label']:<22s} {v:.0f}  x{v/PLATEAU_MEAN:.2f} of 6566  "
              f"{'inside' if inside else 'OUTSIDE'} 3.8-9.3k band")
    w = [r["axes"]["Z"]["white"] for r in rows if "Z" in r["axes"]]
    print(f"08-17 Z white 30-45 Hz: {min(w):.1f}-{max(w):.1f}  "
          f"(07-29 was {z['white']:.1f}; plateau records were 22-28)")


# ---------------------------------------------------------------- SVG


def _svg_polyline(xs, ys, xlim, ylim, box, colour, lw=1.6, ls=""):
    (x0, y0, w, h) = box
    xlo, xhi = xlim
    ylo, yhi = ylim
    def X(x):
        return x0 + w * (math.log10(x) - math.log10(xlo)) / (
            math.log10(xhi) - math.log10(xlo))
    def Y(y):
        return y0 + h * (1 - (math.log10(y) - math.log10(ylo)) / (
            math.log10(yhi) - math.log10(ylo)))
    pts = []
    for x, y in zip(xs, ys):
        if x <= 0 or y <= 0:
            continue
        if not (xlo <= x <= xhi):
            continue
        pts.append(f"{X(x):.1f},{Y(max(min(y, yhi), ylo)):.1f}")
    dash = ' stroke-dasharray="6 4"' if ls == "--" else ""
    return (f'<polyline fill="none" stroke="{colour}" stroke-width="{lw}"'
            f'{dash} points="{" ".join(pts)}"/>')


def plot_asd_svg(rows, ref):
    W, H = 640, 440
    box = (70, 30, 500, 360)
    xlim, ylim = (0.002, 100.0), (8.0, 2e5)
    parts = [
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{W}" height="{H}" '
        f'viewBox="0 0 {W} {H}" font-family="Helvetica,Arial,sans-serif" '
        f'font-size="12">',
        '<rect width="100%" height="100%" fill="white"/>',
        f'<rect x="{box[0]}" y="{box[1]}" width="{box[2]}" height="{box[3]}" '
        f'fill="#F5E9D0" opacity="0.35"/>',
        f'<rect x="{box[0]}" y="{box[1]}" width="{box[2]}" height="{box[3]}" '
        f'fill="none" stroke="#7A7F85"/>',
        f'<text x="20" y="220" fill="#33373B" transform="rotate(-90 20 220)">'
        f'ASD (fT/√Hz)</text>',
        f'<text x="320" y="430" fill="#33373B" text-anchor="middle">'
        f'frequency (Hz)</text>',
        f'<text x="320" y="22" fill="#33373B" text-anchor="middle" '
        f'font-weight="bold">AB3S after 8 A parallel: 0.05 Hz still on the '
        f'plateau</text>',
    ]
    # plateau band in data coords is a y-span -- skip, the beige fill is decorative
    if "Z" in ref["axes"]:
        m = ref["axes"]["Z"]
        parts.append(_svg_polyline(m["f"], [v * 1e3 for v in m["a"]],
                                   xlim, ylim, box, "#9AA0A6", lw=1.3))
    yleg = 50
    parts.append(f'<text x="390" y="{yleg}" fill="#9AA0A6">07-29 postdg Z</text>')
    yleg += 16
    for r in rows:
        m = r["axes"].get("Z")
        if m is None:
            continue
        parts.append(_svg_polyline(m["f"], [v * 1e3 for v in m["a"]],
                                   xlim, ylim, box, r["colour"], lw=1.6))
        parts.append(f'<text x="390" y="{yleg}" fill="{r["colour"]}">'
                     f'{r["label"]} Z</text>')
        yleg += 16
        my = r["axes"].get("Y")
        if my is not None and not my.get("clipped"):
            parts.append(_svg_polyline(my["f"], [v * 1e3 for v in my["a"]],
                                       xlim, ylim, box, r["colour"], lw=1.0,
                                       ls="--"))
    parts.append("</svg>")
    out = os.path.join(HERE, "fig_postdg0817_asd.svg")
    with open(out, "w", encoding="utf-8") as fh:
        fh.write("\n".join(parts))
    print("wrote", out)


def plot_field_svg(rows):
    W, H = 640, 420
    box = (70, 30, 520, 340)
    parts = [
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{W}" height="{H}" '
        f'viewBox="0 0 {W} {H}" font-family="Helvetica,Arial,sans-serif" '
        f'font-size="12">',
        '<rect width="100%" height="100%" fill="white"/>',
        f'<rect x="{box[0]}" y="{box[1]}" width="{box[2]}" height="{box[3]}" '
        f'fill="none" stroke="#7A7F85"/>',
        '<text x="20" y="210" fill="#33373B" transform="rotate(-90 20 210)">'
        "field (pT)</text>",
        '<text x="330" y="410" fill="#33373B" text-anchor="middle">'
        "time (min)</text>",
        '<text x="330" y="22" fill="#33373B" text-anchor="middle" '
        'font-weight="bold">AB3S 08-17: absolute field (30 s blocks)</text>',
    ]
    series = []
    for r in rows:
        for axname, ls in (("Z", ""), ("Y", "--")):
            m = r["axes"].get(axname)
            if m is None:
                continue
            path = os.path.join(BUCKET, r["stem"] + f"_{axname}_pT.txt")
            x = loadtxt(path)
            if m.get("clipped"):
                x = unclipped_prefix(x, rail_pT(r["meta"]))
            k = int(30.0 * r["fs"])
            n = len(x) // k
            if n < 2:
                continue
            t, b = [], []
            for i in range(n):
                chunk = x[i * k:(i + 1) * k]
                t.append((i + 0.5) * 30.0 / 60.0)
                b.append(mean(chunk))
            series.append((t, b, r["colour"], ls, f"{r['label']} {axname}"))
    if not series:
        parts.append("</svg>")
    else:
        tmin, tmax = 0.0, max(max(s[0]) for s in series)
        ymin = min(min(s[1]) for s in series)
        ymax = max(max(s[1]) for s in series)
        pad = 0.08 * (ymax - ymin or 1)
        ymin, ymax = ymin - pad, ymax + pad
        x0, y0, w, h = box

        def X(t):
            return x0 + w * (t - tmin) / (tmax - tmin or 1)

        def Y(v):
            return y0 + h * (1 - (v - ymin) / (ymax - ymin or 1))

        y0line = Y(0)
        parts.append(f'<line x1="{x0}" y1="{y0line:.1f}" x2="{x0+w}" '
                     f'y2="{y0line:.1f}" stroke="#7A7F85" stroke-width="0.7"/>')
        yleg = 50
        for t, b, col, ls, lab in series:
            pts = " ".join(f"{X(tt):.1f},{Y(vv):.1f}" for tt, vv in zip(t, b))
            dash = ' stroke-dasharray="6 4"' if ls == "--" else ""
            parts.append(f'<polyline fill="none" stroke="{col}" '
                         f'stroke-width="1.5"{dash} points="{pts}"/>')
            parts.append(f'<text x="400" y="{yleg}" fill="{col}">{lab}</text>')
            yleg += 16
        parts.append("</svg>")
    out = os.path.join(HERE, "fig_postdg0817_field.svg")
    with open(out, "w", encoding="utf-8") as fh:
        fh.write("\n".join(parts))
    print("wrote", out)


def try_png(rows, ref):
    """Best-effort matplotlib PNG; never required for the table."""
    if os.environ.get("IROP_STDLIB", "").strip() in ("1", "yes"):
        return
    try:
        import matplotlib
        matplotlib.use("Agg")
        import matplotlib.pyplot as plt
    except Exception as e:
        print(f"[note] matplotlib PNG skipped ({e})")
        return
    plt.rcParams.update({
        "figure.dpi": 150, "font.size": 10.5, "axes.titleweight": "bold",
        "axes.spines.top": False, "axes.spines.right": False,
        "legend.frameon": False,
    })
    fig, ax = plt.subplots(figsize=(6.4, 4.4))
    ax.axhspan(PLATEAU_LO, PLATEAU_HI, color="#F5E9D0", zorder=0)
    ax.axhline(TARGET_FT, color="#33373B", ls="--", lw=1.0, alpha=0.7)
    if "Z" in ref["axes"]:
        m = ref["axes"]["Z"]
        ax.loglog(m["f"], [v * 1e3 for v in m["a"]], color="#9AA0A6",
                  lw=1.3, label="07-29 postdg Z")
    for r in rows:
        m = r["axes"].get("Z")
        if m is None:
            continue
        ax.loglog(m["f"], [v * 1e3 for v in m["a"]], color=r["colour"],
                  lw=1.6, label=r["label"] + " Z")
    ax.set_xlim(2e-3, 100)
    ax.set_ylim(8, 2e5)
    ax.set_xlabel("frequency (Hz)")
    ax.set_ylabel("ASD (fT/√Hz)")
    ax.set_title("AB3S after 8 A parallel: 0.05 Hz still on the plateau")
    ax.legend(loc="upper right", fontsize=8.5)
    fig.tight_layout()
    out = os.path.join(HERE, "fig_postdg0817_asd.png")
    fig.savefig(out, bbox_inches="tight")
    plt.close(fig)
    print("wrote", out)


def main():
    print("plot_postdg0817: stdlib Welch (scipy-equivalent scaling)",
          flush=True)
    rows = []
    for label, stem, t0, colour in RUNS:
        row = load_one(stem)
        row["label"] = label
        row["colour"] = colour
        row["t0"] = t0
        row["dt_min"] = (row["start"] - t0).total_seconds() / 60.0
        rows.append(row)
    print("  load 07-29 reference", flush=True)
    ref = load_one(REF_0729)
    ref["label"] = "07-29 postdg Z"
    ref["colour"] = "#9AA0A6"
    # settled second half of 07-29 Z
    zpath = os.path.join(BUCKET, REF_0729 + "_Z_pT.txt")
    zfull = loadtxt(zpath)
    half = zfull[len(zfull) // 2:]
    half_v = welch_200(half, float(ref["meta"]["fs_hz"]))
    for r in rows:
        for ax, note in r["flags"].items():
            if note != "ok":
                print(f"[flag] {r['label']} {ax}: {note}")
    print()
    print_table(rows, ref)
    print(f"  settled second half 0.05 Hz {half_v:.0f} "
          f"(headline 5.8k used the same half-window)")
    plot_asd_svg(rows, ref)
    plot_field_svg(rows)
    try_png(rows, ref)
    return 0


if __name__ == "__main__":
    sys.exit(main())
