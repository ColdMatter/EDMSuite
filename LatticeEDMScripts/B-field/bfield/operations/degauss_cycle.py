"""Degauss the shield: drive DG cycles and keep the evidence.

Moved from IROP_research/scripts/degauss.py (everything but the tio
adapter, which is bfield.devices.degausser). The DG runs the cycle
itself -- sine drive, soak, linear taper to exactly zero -- so this
gates the trigger on a verified settings readback, saves the streamed
waveform (CSV) and the status-phase timeline, QCs the taper, records t0
for the relaxation series, and writes <outdir>/degauss_session.json on
every exit path.

Modes: status (identity / RPC list / waveform settings); check (stepped
short cycles, |Z| collapse marks wall saturation, stops at the first
unverified taper); run (one full cycle).

Safety: QZFM 19V power switch OFF before any energized mode (sensor may
stay in the bore); amplitude capped at max_amp (default 8 A = vendor
recipe = unit firmware cap); Stop / Ctrl-C sends degauss.abort -- an
interrupted cycle leaves undefined magnetization, redo it; unplug the
banana leads after a cycle (a closed winding loop couples vibration
into the shield).

Open with Twinleaf (mechanism not guessed): 8 A / 300 s faulted 4/4 at
t = 45.9-47.2 s (status -3, ~14 s decay) one session, then ran clean
cold the next day; 8 A / 60 s has always completed; stream CSVs under
degaussingtest/ are the vendor attachment.

Exit codes: 0 verified pass / 30 QC refused, stream missing, or a check
step unverified (walls NOT trusted) / 10 connect / 13 config / 32
readback refused the trigger (nothing driven) / 130 stop / 1 crash.
"""

from __future__ import annotations

import math
import os
from dataclasses import dataclass, field

from bfield import clock
from bfield.constants import (DG_RECIPE_AMP_A, DG_RECIPE_FREQ_HZ,
                              DG_TAPER_FALLBACK_S, dg_cycle_plan_s)
from bfield.devices.degausser import (DG_FREQ_HARD_HZ, DG_OUTPUT_MAX_A,
                                      DG_OUTPUT_MAX_V, DG_PHASE_CODES,
                                      DG_PHASE_NAMES, DG_SOAK_HARD_S,
                                      DG_TAPER_HARD_S, WAVEFORM_KEYS,
                                      fmt_status, rpc_name, status_is_idle)
from bfield.errors import (BFieldError, ConfigError, DgError, ReadbackError,
                           Stopped)
from bfield.io.files import sanitize, stamped_dir, write_json_atomic

DG_FREQ_DEFAULT_HZ = DG_RECIPE_FREQ_HZ  # vendor recipe
DG_FREQ_GOOD_HZ = (3.0, DG_RECIPE_FREQ_HZ)  # firmware floor .. recipe
DG_SOAK_DEFAULT_S = 15.0
DG_TAPER_DEFAULT_S = 300.0
DG_TAPER_GUIDE_S = (100.0, 300.0)   # vendor guide: 100-300 s, longer better
MAX_AMP_DEFAULT_A = DG_RECIPE_AMP_A  # vendor recipe = firmware cap;
                                    # readback gate is the real guard
CHECK_SOAK_S = 2.0
CHECK_TAPER_S = 8.0
DG_WATCHDOG_FACTOR = 1.5
DG_WATCHDOG_PAD_S = 60.0
QC_BINS = 20
QC_TERMINAL_FRAC = 0.02             # residual envelope at the end, vs peak
QC_DC_FRAC = 0.02                   # mean(I) magnitude, vs peak
QC_MONOTONIC_MIN = 0.80             # fraction of decreasing taper bins
QC_PEAK_FRAC = 0.80                 # drive peak below this fraction of the
                                    # request = the DG preflight limited it
READBACK_REL_TOL = 1e-4             # float32 quantization allowance;
                                    # still catches a firmware clamp
ABORT_CONFIRM_S = 10.0              # bounded idle poll after degauss.abort

# a cut taper leaves the walls magnetized by an unmeasured amount;
# true in all three abort states, so said once here
REDO_CYCLE = ("The walls are now in an UNDEFINED magnetization state -- redo "
              "the full degauss cycle before trusting any measurement.")


def require(cond, msg):
    if not cond:
        raise ConfigError(msg)


def parse_amps(text):
    out = []
    for part in str(text).split(","):
        part = part.strip()
        if not part:
            continue
        try:
            out.append(float(part))
        except ValueError:
            raise ConfigError(f"amps entry {part!r} is not a number")
    require(out, "amps is empty; give amplitudes like \"0.1,0.2,0.4\"")
    require(all(math.isfinite(a) and a > 0 for a in out),
            "amps entries must be positive numbers")
    require(out == sorted(out),
            "amps must increase (step the check upward, DG guide)")
    return out


@dataclass
class DegaussSettings:
    mode: str = "run"             # status | check | run
    port: str | None = None
    amp: float | None = None
    amps: list | None = None
    freq: float = DG_FREQ_DEFAULT_HZ
    soak: float = DG_SOAK_DEFAULT_S
    taper: float = DG_TAPER_DEFAULT_S
    max_amp: float = MAX_AMP_DEFAULT_A
    outdir: str | None = None     # parent for dg_<stamp>/; None = default
    name: str | None = None       # dated run folder name; None = Degauss,
                                  # DegaussCheck
    coil: str | None = None
    confirm_checklist: bool = False
    extra: dict = field(default_factory=dict)


def validate_settings(s):
    """Refusals raise; returns notes (deliberate-looking choices)."""
    notes = []
    require(s.mode in ("status", "check", "run"), f"bad mode {s.mode!r}")
    require(s.port, "the degausser port is required (COMx on Windows; "
                    "[degausser] port in config/lab.toml)")
    lo, hi = DG_FREQ_HARD_HZ
    require(math.isfinite(s.freq) and lo <= s.freq <= hi,
            f"freq {s.freq:g} outside {lo:g}-{hi:g} Hz")
    glo, ghi = DG_FREQ_GOOD_HZ
    if not (glo <= s.freq <= ghi):
        notes.append(f"freq {s.freq:g} Hz is outside {glo:g}-{ghi:g} Hz "
                     f"(firmware floor {glo:g}; vendor recipe "
                     f"{DG_FREQ_DEFAULT_HZ:g}); make sure this is deliberate")
    lo, hi = DG_SOAK_HARD_S
    require(lo <= s.soak <= hi, f"soak {s.soak:g} outside {lo:g}-{hi:g} s")
    lo, hi = DG_TAPER_HARD_S
    require(lo <= s.taper <= hi, f"taper {s.taper:g} outside {lo:g}-{hi:g} s")
    require(0 < s.max_amp <= DG_OUTPUT_MAX_A,
            f"max_amp {s.max_amp:g} outside 0-{DG_OUTPUT_MAX_A:g} A")
    if s.mode == "run":
        require(s.amp is not None, "a run needs an amplitude (start at the "
                "check's saturation amplitude)")
        # coil label is unrecoverable later; standing winding is
        # parallel4 since 2026-08-13 (2026-08-19 review, M5)
        require(s.coil, "a run needs the coil label: the winding cannot be "
                "recovered later (standing winding: parallel4)")
        if s.soak < 1.0:
            notes.append(f"soak {s.soak:g} s leaves the QC no rate anchor -- "
                         f"taper length will be unverifiable (use >= 1 s)")
        glo, ghi = DG_TAPER_GUIDE_S
        if s.taper < DG_TAPER_FALLBACK_S:
            notes.append(f"taper {s.taper:g} s is below even the "
                         f"demonstrated {DG_TAPER_FALLBACK_S:g} s fallback "
                         f"-- rehearsal only, not a real degauss")
        elif s.taper < glo:
            notes.append(f"taper {s.taper:g} s is below the vendor guide "
                         f"{glo:g}-{ghi:g} s; {DG_TAPER_FALLBACK_S:g} s is "
                         f"the demonstrated completion at "
                         f"{DG_RECIPE_AMP_A:g} A (the {ghi:g} s recipe cut "
                         f"is with the vendor)")
    if s.mode == "check":
        require(s.amps, "a check needs amplitudes, e.g. \"0.1,0.2,...\"")
    for a in ([s.amp] if s.amp is not None else []) + (s.amps or []):
        require(math.isfinite(a) and 0 < a <= s.max_amp,
                f"amplitude {a:g} A outside 0-{s.max_amp:g} A "
                f"(max_amp raises the cap deliberately)")
    return notes


# ------------------------------------------------------ stream QC

def find_column(columns, words, exact):
    """Index of the first column matching `words` (substring) or `exact`
    (full name or last dot-component) -- by name only, never position.
    The DG names columns by unit: degauss.A / .V / .Ohm (bench 08-12)."""
    for i, name in enumerate(columns or []):
        n = str(name).lower()
        leaf = n.split(".")[-1].strip()
        if any(w in n for w in words) or n in exact or leaf in exact:
            return i
    return None


def find_current_column(columns):
    return find_column(columns, ("current",), ("i", "i_a", "a"))


def find_voltage_column(columns):
    return find_column(columns, ("volt",), ("v", "v_v"))


def find_ohm_column(columns):
    # DG streams |Z| itself; the saturation collapse is measured directly
    return find_column(columns, ("ohm",), ("z",))


def find_status_column(columns):
    return find_column(columns, (), ("status",))


def finite_column(rows, idx):
    """Finite floats in column idx; short rows / non-numeric cells skipped."""
    out = []
    for row in rows:
        try:
            v = float(row[idx])
        except (TypeError, ValueError, IndexError):
            continue
        if math.isfinite(v):
            out.append(v)
    return out


def qc_metrics(rows, columns, expected=None):
    """QC dict, or None when there is nothing to judge.

    Shape checks (monotonic envelope, terminal ~zero, no DC) run on
    drive-phase rows. A bound status column must also show no fault and
    a cooldown; with `expected`, the taper must last the commanded
    duration (soak anchors the sample rate; bench 08-13 sec 1.1).
    Shape alone passes a cut taper (E-1) -- length and fault catch it."""
    if not rows:
        return None
    ci = find_current_column(columns)
    si = find_status_column(columns)

    xs_all, xs_drive, xs_cool, codes = [], [], [], []
    if ci is not None:
        for row in rows:
            code = None
            if si is not None:
                try:
                    code = int(float(row[si]))
                except (TypeError, ValueError, IndexError):
                    code = None
            if code is not None:
                codes.append(code)
            try:
                v = float(row[ci])
            except (TypeError, ValueError, IndexError):
                continue
            if math.isfinite(v):
                xs_all.append(v)
                # 5/6/7/8 = ramp/soak/taper/cooldown; drop idle+fault
                # rows (post-fault idle tail hides a cut taper)
                if code is None or code in (5, 6, 7, 8):
                    xs_drive.append(v)
                if code == DG_PHASE_CODES["cooldown"]:
                    xs_cool.append(v)
    use_drive = si is not None and bool(xs_drive)
    xs = xs_drive if use_drive else xs_all

    def verdict(name, *reasons, **extra):
        return {"column": ci, "status_column": si, "n_rows": len(rows),
                "n_finite": len(xs), "verdict": name,
                "reasons": list(reasons), **extra}

    if ci is None:
        # fail closed: guessing a column could bless a taper that never
        # happened; c0,c1,... means the schema read failed
        return verdict("fail", "current column not identified in the stream "
                               "header -- bind the schema before trusting QC")
    if len(xs) < QC_BINS * 2:
        return verdict("insufficient", "too few finite rows")
    peak = max(abs(v) for v in xs)
    if peak <= 0:
        return verdict("insufficient", "all-zero stream")
    binsz = len(xs) // QC_BINS
    env = [max(abs(v) for v in xs[k * binsz:(k + 1) * binsz])
           for k in range(QC_BINS)]
    start = max(range(QC_BINS), key=lambda k: env[k])
    tail = env[start:]
    downs = sum(1 for a, b in zip(tail, tail[1:]) if b <= a + 0.02 * peak)
    monotonic_frac = downs / max(len(tail) - 1, 1)
    # terminal residual from cooldown rows; the last-bin fallback reaches
    # into the taper on long recipes, so it is only for status-less streams
    terminal_frac = (max(abs(v) for v in xs_cool) / peak if xs_cool
                     else env[-1] / peak)
    dc_frac = abs(sum(xs) / len(xs)) / peak
    reasons = []
    if monotonic_frac < QC_MONOTONIC_MIN:
        reasons.append(f"envelope not monotonic ({monotonic_frac:.2f})")
    if terminal_frac > QC_TERMINAL_FRAC:
        reasons.append(f"terminal residual {terminal_frac * 100:.1f}% of "
                       f"peak (must end at zero)")
    if dc_frac > QC_DC_FRAC:
        reasons.append(f"DC component {dc_frac * 100:.1f}% of peak")

    fault = any(c < 0 for c in codes)
    cooldown_seen = DG_PHASE_CODES["cooldown"] in codes
    taper_s = None
    rate_hz = None
    if si is not None and codes:
        if fault:
            reasons.append(f"device fault (status {min(codes)}) in the "
                           f"record -- the unit cut the cycle itself")
        if not cooldown_seen:
            reasons.append("no cooldown phase in the record -- the cycle "
                           "did not terminate normally")
        exp_soak = (expected or {}).get("soaktime")
        exp_dur = (expected or {}).get("duration")
        n_soak = codes.count(DG_PHASE_CODES["soaking"])
        n_taper = codes.count(DG_PHASE_CODES["running"])
        if exp_soak and exp_soak > 0 and exp_dur and n_soak >= 8:
            rate_hz = n_soak / float(exp_soak)
            taper_s = n_taper / rate_hz
            tol = max(0.05 * float(exp_dur), 1.0)
            # DG cannot extend its own taper: a long measurement means
            # stream loss, not a drive fault (bench 08-14 F-2)
            if taper_s < float(exp_dur) - tol:
                reasons.append(
                    f"taper phase ran {taper_s:.1f} s of the commanded "
                    f"{exp_dur:g} s -- the cycle was cut short")
            elif taper_s > float(exp_dur) + tol:
                reasons.append(
                    f"taper phase MEASURED {taper_s:.1f} s of the "
                    f"commanded {exp_dur:g} s -- the DG cannot extend "
                    f"its own taper, so treat this as a measurement "
                    f"fault (stream sample loss), not a drive fault")
        elif exp_dur:
            # never skip silently: an unusable soak anchor once scored a
            # cut cycle PASS (bench 08-14 F-3)
            reasons.append(
                f"taper length unverifiable -- the commanded soak "
                f"({exp_soak!r} s) gives the QC no usable rate anchor, "
                f"so the cut-short gate did not run")
    elif expected is not None:
        reasons.append("cycle phases absent from the stream -- taper "
                       "length unverifiable")

    return verdict("fail" if reasons else "pass", *reasons,
                   i_peak=peak, monotonic_frac=round(monotonic_frac, 3),
                   terminal_frac=round(terminal_frac, 4),
                   dc_frac=round(dc_frac, 4), fault=fault,
                   cooldown_seen=cooldown_seen,
                   stream_rate_hz=(None if rate_hz is None
                                   else round(rate_hz, 1)),
                   taper_measured_s=(None if taper_s is None
                                     else round(taper_s, 2)))


def phase_timeline(rows, columns, rate_hz=None):
    """Ordered phase segments [{status, phase, n_rows, seconds}, ...]
    from the stream's status column, or None without one."""
    si = find_status_column(columns)
    if si is None:
        return None
    segs = []
    for row in rows:
        try:
            code = int(float(row[si]))
        except (TypeError, ValueError, IndexError):
            continue
        if segs and segs[-1]["status"] == code:
            segs[-1]["n_rows"] += 1
        else:
            segs.append({"status": code, "phase": DG_PHASE_NAMES.get(code),
                         "n_rows": 1})
    for seg in segs:
        seg["seconds"] = (round(seg["n_rows"] / rate_hz, 2)
                          if rate_hz else None)
    return segs or None


def column_rms(rows, idx):
    """RMS of column idx; None with no column or no finite values."""
    if idx is None:
        return None
    xs = finite_column(rows, idx)
    return math.sqrt(sum(v * v for v in xs) / len(xs)) if xs else None


def ohm_stats(rows, columns):
    """(median |Z| over the driven half, min |Z|) from the DG's own Ohm
    column, or (None, None). The minimum is the saturation witness:
    median 0.81-0.83 ohm at 8 A collapses to 0.4333 (-48%) on the
    4-group winding (bench 08-13 sec 4)."""
    ci, zi = find_current_column(columns), find_ohm_column(columns)
    if ci is None or zi is None:
        return None, None
    pairs = []
    for row in rows:
        try:
            i, z = abs(float(row[ci])), float(row[zi])
        except (TypeError, ValueError, IndexError):
            continue
        if math.isfinite(i) and math.isfinite(z) and z > 0:
            pairs.append((i, z))
    if not pairs:
        return None, None
    peak = max(i for i, _ in pairs)
    if peak <= 0:
        return None, None
    zs = sorted(z for i, z in pairs if i >= 0.5 * peak)
    if not zs:
        return None, None
    return zs[len(zs) // 2], zs[0]


def write_stream_csv(outdir, idx, rows, columns):
    if outdir is None or not rows:
        return None
    name = f"stream_c{idx:02d}.csv"
    path = os.path.join(outdir, name)
    width = max(len(r) for r in rows)
    header = list(columns or [])[:width]
    header += [f"c{i}" for i in range(len(header), width)]
    with open(path, "w", newline="\n", encoding="utf-8") as fh:
        fh.write(",".join(header) + "\n")
        for row in rows:
            cells = []
            for i in range(width):
                try:
                    cells.append(f"{float(row[i]):.6g}")
                except (TypeError, ValueError, IndexError):
                    cells.append("")
            fh.write(",".join(cells) + "\n")
    return name


# --------------------------------------------------- the trigger gate

def verified_readback(requested, rb, max_amp):
    """Fail-closed gate between set_waveform and trigger.

    The DG drives from its own stored settings, so only the readback
    says what will be driven: every field must be finite and match the
    request within quantization tolerance, then hard limits re-apply.
    Raises ReadbackError (exit 32); closes the defect where a 9 A
    readback against a 0.6 A request warned and fired anyway."""
    problems = []
    vals = {}
    for key, want in requested.items():
        got = rb.get(key)
        try:
            got = float(got)
        except (TypeError, ValueError):
            got = None
        if got is None or not math.isfinite(got):
            problems.append(f"{key} readback missing or non-finite "
                            f"({rb.get(key)!r})")
            continue
        vals[key] = got
        if abs(got - want) > 1e-6 + READBACK_REL_TOL * abs(want):
            problems.append(f"{key} readback {got:g} != requested {want:g}")
    if not problems:
        cap = min(max_amp, DG_OUTPUT_MAX_A)
        if not (0.0 < vals["current"] <= cap):
            problems.append(f"current readback {vals['current']:g} A "
                            f"outside 0-{cap:g} A")
        for key, (lo, hi), unit in (("frequency", DG_FREQ_HARD_HZ, "Hz"),
                                    ("soaktime", DG_SOAK_HARD_S, "s"),
                                    ("duration", DG_TAPER_HARD_S, "s")):
            if not (lo <= vals[key] <= hi):
                problems.append(f"{key} readback {vals[key]:g} {unit} "
                                f"outside {lo:g}-{hi:g} {unit}")
    if problems:
        raise ReadbackError("refusing to trigger on this readback: "
                            + "; ".join(problems))
    return vals


def confirm_abort(dg, wait_s=ABORT_CONFIRM_S):
    """Poll the unit after degauss.abort until it reports idle: "idle",
    "timeout" (status alive, never idle) or "unknown" (no status)."""
    deadline = clock.monotonic() + wait_s
    saw_status = False
    while clock.monotonic() < deadline:
        try:
            st = dg.status()
        except Exception:
            st = None
        if st is not None:
            saw_status = True
            if status_is_idle(st):
                return "idle"
        clock.sleep(min(0.2, max(dg.poll_sleep_s(), 0.02)))
    return "timeout" if saw_status else "unknown"


def abort_verdict(ab):
    """(short reason, operator sentence) -- is it safe to touch the
    winding yet."""
    confirmed = ab.get("confirmed")
    if ab.get("acked") and confirmed == "idle":
        return ("abort acknowledged, unit reports idle",
                "degauss.abort was acknowledged and the unit reports idle.")
    if ab.get("acked"):
        return (f"abort acknowledged, idle NOT confirmed ({confirmed})",
                f"degauss.abort was acked but idle was NOT confirmed "
                f"({confirmed}) -- treat the DG output as LIVE; do not touch "
                f"or unplug the winding until the unit itself shows idle.")
    return ("ABORT UNCONFIRMED; DG output state unknown",
            "the abort was NOT confirmed and the DG output state is UNKNOWN "
            "-- do not touch or unplug the winding until the unit itself "
            "shows idle.")


# -------------------------------------------------------- the session

class DegaussSession:
    """One degauss program run: its dg_<stamp>/ folder and its json."""

    def __init__(self, ctx, lab, s):
        self.ctx = ctx
        self.lab = lab
        self.s = s
        self.outdir = None
        self.path = None
        self.dg = None
        soak, taper = ((CHECK_SOAK_S, CHECK_TAPER_S) if s.mode == "check"
                       else (s.soak, s.taper))
        self.doc = {
            "program": "degauss", "software": "bfield.operations."
                                              "degauss_cycle",
            "started": clock.iso_now(), "mode": s.mode, "backend": "tio",
            "simulated": lab.sim, "port": s.port, "coil": s.coil,
            "settings_requested": {"frequency": s.freq, "soaktime": soak,
                                   "duration": taper},
            "max_amp": s.max_amp, "device": None, "settings_before": None,
            "cycles": [], "warnings": [], "abort": None,
            "outcome": "crashed", "end_reason": None, "t0": None,
        }

    @property
    def warnings(self):
        return self.doc["warnings"]

    def warn(self, msg):
        self.ctx.warn(msg, self.warnings)

    def write(self):
        if self.path is None:
            return
        # rewritten atomically on every exit path; the one record of
        # what the walls were driven with
        try:
            write_json_atomic(self.path, sanitize(self.doc), indent=2)
        except OSError as e:
            self.ctx.warn(f"session json write failed: {e}")

    # -- entry point ----------------------------------------------------

    def run(self):
        """The whole program: returns the exit code. Never raises a
        BFieldError (they become codes); the json is written always."""
        s = self.s
        try:
            for note in validate_settings(s):
                self.ctx.say("note", note)
            if s.mode in ("check", "run"):
                self.confirm_safety()
            self.outdir = self._make_outdir()
            if self.outdir:
                self.path = os.path.join(self.outdir, "degauss_session.json")
                self.ctx.say("out", self.outdir)
            self.dg = self.lab.degausser(self.ctx, self.warnings, s.port)
            try:
                if s.mode == "status":
                    return self.status()
                if s.mode == "check":
                    return self.check()
                return self.degauss()
            except (KeyboardInterrupt, Stopped):
                # a mid-cycle interrupt already aborted inside run_cycle;
                # reuse its record instead of sending a second abort
                ab = self.doc.get("abort")
                if ab is None:
                    acked = bool(self.dg.abort())
                    confirmed = confirm_abort(self.dg) if acked else None
                    ab = {"attempted": True, "acked": acked,
                          "confirmed": confirmed}
                    self.doc["abort"] = ab
                self.doc["outcome"] = "interrupted"
                reason, msg = abort_verdict(ab)
                self.doc["end_reason"] = f"stop -- {reason}"
                self.ctx.error(f"stop: {msg} {REDO_CYCLE}")
                return 130
            finally:
                self.dg.close()
        except (KeyboardInterrupt, Stopped):
            self.ctx.error("interrupted before driving.")
            return 130
        except BFieldError as e:
            self.doc["outcome"] = "failed"
            self.doc["end_reason"] = f"{type(e).__name__}: {e}"
            self.ctx.error(f"ERROR[{e.code}] {type(e).__name__}: {e}")
            return e.code
        except Exception as e:
            self.doc["end_reason"] = f"{type(e).__name__}: {e}"
            self.ctx.error(f"ERROR[1] {type(e).__name__}: {e}")
            return 1
        finally:
            self.write()

    def confirm_safety(self):
        if self.s.confirm_checklist:
            self.ctx.say("check", "safety checklist asserted by the caller "
                                  "(19V OFF, endcaps closed)")
            return
        self.ctx.ask("Is the QZFM 19V power switch OFF (electronics dead, "
                     "not serial standby)? It may stay in the bore.",
                     "turn the QZFM 19V switch OFF, then rerun -- nothing "
                     "driven")
        self.ctx.ask("Are the endcaps closed (final configuration)?",
                     "safety checklist declined -- nothing driven")

    def _make_outdir(self):
        if self.s.mode == "status" and self.s.outdir is None:
            return None
        try:
            if self.s.outdir is None and self.lab.dated:
                # the dated run folder is the session folder itself
                return self.lab.new_run_dir(
                    self.s.name or ("DegaussCheck" if self.s.mode == "check"
                                    else "Degauss"))
            parent = self.s.outdir or self.lab.bucket("degauss")
            os.makedirs(parent, exist_ok=True)
            return stamped_dir(parent, "dg")
        except (OSError, ValueError) as e:
            raise ConfigError(f"cannot create the session folder: {e}")

    # -- the modes ------------------------------------------------------

    def status(self):
        info = self.dg.connect()
        self.doc["device"] = info
        wave = self.dg.get_waveform()
        self.doc["settings_before"] = wave
        self.ctx.say("status", "device: "
                     + str({k: v for k, v in info.items() if k != "rpcs"}))
        rpcs = info.get("rpcs")
        if rpcs:
            names = [rpc_name(r) for r in rpcs]
            self.ctx.say("status", f"{len(rpcs)} RPCs; degauss.*: "
                         + ", ".join(n for n in names
                                     if n.startswith("degauss")))
        self.ctx.say("status", "waveform settings now (persisted; the "
                               "front-panel button drives them):")
        for key in WAVEFORM_KEYS:
            self.ctx.say("status", f"  degauss.waveform.{key:<10} = "
                                   f"{wave.get(key)}")
        cur = wave.get("current")
        if cur is not None and cur > self.s.max_amp:
            # bench D-7: the unit once arrived holding 5 A against a
            # 3.5 A cap, silently -- say it instead
            self.ctx.say("status", f"NOTE: the stored amplitude {cur:g} A "
                                   f"is above max_amp {self.s.max_amp:g} A "
                                   f"-- the front panel would drive it, this "
                                   f"program would refuse it")
        self.doc["outcome"] = "completed"
        self.doc["end_reason"] = "status dump"
        self.status_result = {"device": info, "waveform": wave}
        return 0

    def _connect_for_drive(self):
        info = self.dg.connect()
        self.doc["device"] = info
        self.doc["settings_before"] = self.dg.get_waveform()
        cap = getattr(self.dg, "output_max_a", None)
        if cap is not None and self.s.max_amp > cap:
            self.warn(f"max_amp {self.s.max_amp:g} A is above the unit's own "
                      f"cap {cap:g} A (degauss.waveform.current.max); the "
                      f"readback gate refuses what the firmware would "
                      f"silently clamp")

    def _note_persisted(self):
        """Say what the unit is left holding (autosave, bench D-7).
        Nothing is written back (a half-restored unit is its own hazard)."""
        before = self.doc.get("settings_before") or {}
        cycles = self.doc.get("cycles") or []
        after = (cycles[-1].get("readback") or {}) if cycles else {}
        if not after:
            return

        def line(w):
            return ", ".join(f"{k} {w.get(k)}" for k in WAVEFORM_KEYS)

        self.ctx.say("note", f"the DG persists its settings: now "
                             f"{line(after)} (was {line(before)}) -- the "
                             f"front-panel button drives these")

    def check(self):
        s = self.s
        self._connect_for_drive()
        self.ctx.say("check", f"stepped saturation probe: amps {s.amps} A, "
                              f"short cycles (soak {CHECK_SOAK_S:g} s, "
                              f"taper {CHECK_TAPER_S:g} s)")
        self.ctx.say("check", "watch |Z|: it COLLAPSES once the walls "
                              "saturate (-48% at 8 A on the 4-group "
                              "winding); streamed as the Ohm column, V/I is "
                              "the cross-check")
        table = []
        self.check_table = table

        def cell(x):
            return f"{'--':>11}" if x is None else f"{x:11.4g}"

        def print_table():
            self.ctx.say("check", "  amp_A     I_rms_A     V_rms_V     "
                                  "V/I_ohm   Z_med_ohm   Z_min_ohm")
            for amp, i_rms, v_rms, ratio, z_med, z_min in table:
                self.ctx.say("check", f"{amp:7.3g} {cell(i_rms)} "
                                      f"{cell(v_rms)} {cell(ratio)} "
                                      f"{cell(z_med)} {cell(z_min)}")

        for idx, amp in enumerate(s.amps, start=1):
            rec, rows = self.run_cycle(idx, amp, s.freq, CHECK_SOAK_S,
                                       CHECK_TAPER_S)
            cols = self.dg.columns
            i_rms = column_rms(rows, find_current_column(cols))
            v_rms = column_rms(rows, find_voltage_column(cols))
            z_med, z_min = ohm_stats(rows, cols)
            ratio = ((v_rms / i_rms)
                     if (i_rms is not None and v_rms is not None
                         and i_rms > 0) else None)
            table.append((amp, i_rms, v_rms, ratio, z_med, z_min))
            rec["check"] = {"i_rms": i_rms, "v_rms": v_rms,
                            "v_over_i": ratio, "z_med_ohm": z_med,
                            "z_min_ohm": z_min}
            if rec["outcome"] != "ok":
                # each probe cycle must end in a verified taper; stepping
                # stronger from an untrusted wall state compounds the damage
                print_table()
                reasons = "; ".join((rec.get("qc") or {}).get("reasons", []))
                self.ctx.say("check", f"cycle {idx} ({amp:g} A) outcome "
                             f"{rec['outcome']}"
                             f"{': ' + reasons if reasons else ''} -- "
                             f"stopping before the next amplitude. Confirm "
                             f"at the DG unit that the output shows "
                             f"idle/zero; redo the check from a verified "
                             f"state.")
                self._note_persisted()
                self.doc["outcome"] = rec["outcome"]
                self.doc["end_reason"] = (f"cycle {idx} of {len(s.amps)} "
                                          f"outcome {rec['outcome']}")
                return 30
        print_table()
        self.ctx.say("check", "the saturation amplitude is where |Z| "
                              "collapses; run the full degauss at or above "
                              "it (operator judgement)")
        self._note_persisted()
        self.doc["outcome"] = "completed"
        self.doc["end_reason"] = f"{len(s.amps)} probe cycles"
        return 0

    def degauss(self):
        s = self.s
        self._connect_for_drive()
        rec, _rows = self.run_cycle(1, s.amp, s.freq, s.soak, s.taper)
        self.doc["end_reason"] = f"cycle outcome {rec['outcome']}"
        self.ctx.say("done", f"cycle outcome: {rec['outcome']}")
        self._note_persisted()
        # t0 and the unplug cue require a verified pass (stream seen,
        # drive ended at zero); never say the winding is safe otherwise
        if rec["outcome"] == "ok":
            self.doc["t0"] = rec["end"]
            self.doc["outcome"] = "completed"
            self.ctx.say("done", f"t0 = {rec['end']}  <- relaxation-series "
                                 f"origin, note it down")
            self.ctx.say("done", "NOW: unplug the banana leads (open the "
                                 "winding circuit); then wake the sensor, "
                                 "Field Zero + Calibrate, record the factors")
            return 0
        self.doc["outcome"] = rec["outcome"]
        if rec["outcome"] == "no-stream":
            self.ctx.error("NO STREAM CAPTURED -- the taper could not be "
                           "verified. Confirm at the DG unit that the output "
                           "shows idle/zero BEFORE unplugging anything; if "
                           "in doubt, redo the cycle.")
        else:
            reasons = "; ".join((rec.get("qc") or {}).get("reasons", []))
            self.ctx.error(f"QC REFUSED this pass ({reasons or 'see qc'}) -- "
                           f"the drive did not verifiably end at zero. The "
                           f"walls are NOT in a trusted state: redo the full "
                           f"degauss cycle before any measurement.")
        return 30

    # -- one cycle ------------------------------------------------------

    def _stop_and_report(self, rec, idx, rows):
        """Try to stop a cycle that is ending badly, and say how far we
        got. Called from an except-BaseException, so it must not raise."""
        dg = self.dg
        acked = False
        try:
            acked = bool(dg.abort())
        except (KeyboardInterrupt, Stopped):
            # a second stop must not skip the abort; retry once
            self.warn(f"cycle {idx}: stop during the abort request -- "
                      f"retrying")
            try:
                acked = bool(dg.abort())
            except Exception as e:
                self.warn(f"cycle {idx}: abort retry raised "
                          f"{type(e).__name__}: {e}")
        except Exception as e:
            self.warn(f"cycle {idx}: abort raised {type(e).__name__}: {e}")
        try:
            confirmed = confirm_abort(dg) if acked else None
        except (KeyboardInterrupt, Stopped):
            confirmed = None        # poll cut short: state honestly unknown
            self.warn(f"cycle {idx}: stop during the idle confirmation -- "
                      f"the DG state is UNKNOWN, check the front panel")
        rec["abort"] = {"attempted": True, "acked": acked,
                        "confirmed": confirmed}
        self.doc["abort"] = rec["abort"]
        try:
            rec["stream_csv"] = write_stream_csv(self.outdir, idx, rows,
                                                 dg.columns)
        except OSError as e:
            self.warn(f"cycle {idx}: stream csv not written ({e})")
        rec["n_rows"] = len(rows)
        rec["status_timeline"] = phase_timeline(rows, dg.columns)
        _, how = abort_verdict(rec["abort"])
        self.warn(f"cycle {idx}: abnormal exit after trigger -- {how} "
                  f"{REDO_CYCLE}")

    def run_cycle(self, idx, amp, freq, soak, taper):
        """One triggered DG cycle. Returns (record dict, stream rows)."""
        dg = self.dg
        rec = {"index": idx, "amp": amp,
               "requested": {"current": amp, "frequency": freq,
                             "soaktime": soak, "duration": taper},
               "readback": None, "verified": None, "trigger": None,
               "end": None, "status_polled": [], "status_timeline": None,
               "stream_csv": None, "n_rows": 0, "qc": None,
               "outcome": "failed"}
        self.doc["cycles"].append(rec)
        dg.set_waveform({"current": amp, "frequency": freq,
                         "soaktime": soak, "duration": taper})
        rb = dg.get_waveform()
        rec["readback"] = rb
        try:
            wave = verified_readback(rec["requested"], rb, self.s.max_amp)
        except ReadbackError:
            rec["outcome"] = "refused-readback"
            raise
        rec["verified"] = wave
        plan = dg_cycle_plan_s(wave["soaktime"], wave["duration"])
        self.ctx.say(f"cycle {idx}", f"amp {wave['current']:g} A, "
                     f"{wave['frequency']:g} Hz, soak {wave['soaktime']:g} s, "
                     f"taper {wave['duration']:g} s (~{plan:.0f} s total, "
                     f"verified readback)")
        # caller-owned rows: whatever was drained before an exception
        # survives, so an aborted cycle keeps its partial waveform (E-3)
        rows = []
        # every exit that is not a confirmed completion must attempt
        # abort; trigger() stays inside the try (a timed-out RPC may fire)
        try:
            rec["trigger"] = clock.iso_now()
            dg.trigger()
            t_start = clock.monotonic()
            deadline = (t_start + plan * DG_WATCHDOG_FACTOR
                        + DG_WATCHDOG_PAD_S)
            last_status = object()
            while True:
                self.ctx.check_stop()
                st = dg.status()
                if st != last_status:
                    rec["status_polled"].append([clock.iso_now(), st])
                    last_status = st
                    if st is not None:
                        self.ctx.say(f"cycle {idx}", f"status: "
                                                     f"{fmt_status(st)}")
                dg.read_rows(rows)
                self.ctx.progress(min((clock.monotonic() - t_start)
                                      / max(plan, 1e-9), 1.0),
                                  f"degauss cycle {idx}")
                if dg.done():
                    break
                if clock.monotonic() > deadline:
                    raise DgError(
                        f"watchdog: cycle {idx} still not finished after "
                        f"{plan * DG_WATCHDOG_FACTOR + DG_WATCHDOG_PAD_S:.0f}"
                        f" s")
                clock.sleep(dg.poll_sleep_s())
            dg.read_rows(rows)
        except BaseException:
            self._stop_and_report(rec, idx, rows)
            raise
        rec["end"] = clock.iso_now()
        rec["n_rows"] = len(rows)
        rec["stream_csv"] = write_stream_csv(self.outdir, idx, rows,
                                             dg.columns)
        rec["qc"] = qc_metrics(rows, dg.columns, expected=wave)
        rec["status_timeline"] = phase_timeline(
            rows, dg.columns,
            rate_hz=(rec["qc"] or {}).get("stream_rate_hz"))
        if rec["qc"] is None:
            rec["outcome"] = "no-stream"
            self.warn(f"cycle {idx}: no waveform stream captured -- QC absent")
        elif rec["qc"]["verdict"] == "pass":
            rec["outcome"] = "ok"
        else:
            rec["outcome"] = "flagged"
            self.warn(f"cycle {idx} QC flagged: "
                      f"{'; '.join(rec['qc']['reasons'])}")
            if rec["qc"].get("fault"):
                self.warn(f"cycle {idx}: the DG cut this cycle itself (fault "
                          f"code in the stream, taper "
                          f"{rec['qc'].get('taper_measured_s')} s of "
                          f"{wave['duration']:g} s) -- keep the stream CSV "
                          f"for the vendor; mechanism deliberately not "
                          f"guessed here")
        i_peak = (rec["qc"] or {}).get("i_peak")
        if i_peak is not None and float(i_peak) < QC_PEAK_FRAC * amp:
            self.warn(f"cycle {idx}: drive peaked at {i_peak:.2f} A vs the "
                      f"requested {amp:g} A -- the DG preflight (I_max = "
                      f"{DG_OUTPUT_MAX_V:g} V / |Z|, state-dependent) "
                      f"probably limited the drive; lower the amplitude or "
                      f"use a 5 Hz pass")
        return rec, rows


def run_degauss(ctx, lab, settings):
    """Convenience: one DegaussSession -> (exit code, session doc, outdir)."""
    sess = DegaussSession(ctx, lab, settings)
    code = sess.run()
    return code, sess.doc, sess.outdir
