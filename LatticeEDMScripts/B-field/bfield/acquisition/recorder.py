"""The recorder: QZFM analog outputs on the NI USB-6008.

Moved from IROP_research/scripts/qzfm_record.py with its behaviour and
its output files unchanged (pT files, raw volts CSV, meta.json, quality
gates, exit codes, the stop-file and rail-abort contracts).

It is still its OWN PROCESS, by the original author's decision: the
sensor session (bfield.operations.record) starts it as a child, so a
recorder hung in a DAQ read can be killed without losing the sensor
session, and a sensor-side crash cannot tear the data files. The
command line is the child's contract and is the same as the old one,
plus --lab-file (which lab -- bench or simulator -- and the config).

Three bench topologies:

  --1head   one head, axes from --axes (default z,y); optional
            raw-volt --aux channels (e.g. a coil-current monitor)
  --2head   two dual-axis heads on four inputs (--map axis:head:aiN);
            z2 = z re-read on the other head, each pair gets a
            block-mean r in meta; --aux mon:ai2 works too (S(f) layout)
  --4head   four single-axis heads (--sensors s1:ai0,...) with signed
            pair gradients (--gradient s1,s2:0.08) in pT/m

Exit codes (also in meta.json): 10 DAQ device, 11 DAQ setup, 12 DAQ
read, 13 config, 30 saturation, 31 no signal / flat, 32 bad data,
40 output or meta could not be written, 130 Ctrl-C.

This process prints: the parent captures its console into
<run_dir>/child.log, and that log is the recorder's operations record.
Direct use (a bare-DAQ record, a probe, a hardware check) is
run/record.py, which calls main() here.
"""

from __future__ import annotations

import argparse
import contextlib
import json
import math
import os
import re
import signal
import sys
import traceback
from dataclasses import dataclass, field

from bfield import clock
from bfield.constants import (
    CAL_FACTOR_MAX, CAL_FACTOR_MIN, CAL_FACTOR_WARN, COMMON_VERSION,
    DAQ_AGGREGATE_MAX, DAQ_DIFF_AI, GAIN_TABLE, HEADS, LF_BAND_HZ,
    MAP_AXIS_RX, PINNED_BLOCK_LIMIT, PINNED_DOMINANT_FRAC, PROBE_HEADROOM,
    QZFM_LINEAR_NT, QZFM_MOD_HZ, QZFM_RAIL_THRESH_V, QZFM_RAIL_V,
    RAIL_LEVEL, VALID_AXES, VALID_VRANGE, WIZARD_GAIN_V_PER_NT,
    mod_alias_hz, smallest_vrange,
)
from bfield.errors import (
    BadDataError, BFieldError as AcqError, ConfigError, DaqDeviceNotFound,
    DaqSetupError, NoSignalError, OutputIOError, SaturationError,
    error_record,
)
from bfield.heads import (DEFAULT_SENSOR_ID, DEFAULT_SENSOR_ID_B,
                          SENSOR_ID_RX, head_is_unknown, same_head)
from bfield.io.files import (recorder_leftovers, rollback_block,
                             unique_outdir, write_json_atomic)
from bfield.ui import robust_console

# the standing two-head wiring (the parent passes config/lab.toml's
# [daq] two_head_map; this is the child's own default)
DEFAULT_TWO_HEAD_MAP = "z:a:ai0,y:a:ai1,z2:b:ai2,y2:b:ai3"
PROBE_SECONDS = 2.0


def parse_cal_factors(spec, names, *, noun="axis", known=None,
                      have_hint=None):
    """'z:1.08,y:0.90' -> {name: factor}: QZFM Calibrate factors.

    Recorded as provenance, never applied to the data. Empty string ==
    flag not passed, returns None (returning {} for "" once made the two
    recorders disagree). `noun`/`known`/`have_hint` word each topology's
    refusals (axes vs map channels).
    """
    spec = (spec or "").strip()
    if not spec:
        return None
    factors = {}
    for pair in (p.strip() for p in spec.split(",")):
        # An empty field is a typo, not an entry
        if not pair:
            raise ConfigError(
                f"--cal-factors {spec!r} has an empty entry (a stray or "
                "trailing comma); write e.g. z:1.08,y:0.90")
        name, sep, val = (s.strip() for s in pair.partition(":"))
        name = name.lower()
        if not sep or not name or (known is not None and name not in known):
            raise ConfigError(
                f"bad --cal-factors entry {pair!r} (want e.g. z:1.08)")
        if name in factors:
            raise ConfigError(f"--cal-factors repeats {noun} {name!r}")
        if name not in names:
            hint = have_hint or f"recorded (have {','.join(names)})"
            raise ConfigError(
                f"--cal-factors names {noun} {name!r} which is not {hint}")
        try:
            v = float(val)
        except ValueError:
            raise ConfigError(
                f"bad --cal-factors value {val!r} for {noun} {name!r}")
        if not (CAL_FACTOR_MIN <= v <= CAL_FACTOR_MAX):
            raise ConfigError(
                f"--cal-factors {name}:{v:g} outside "
                f"{CAL_FACTOR_MIN:g}..{CAL_FACTOR_MAX:g} -- QuSpin factors "
                "sit near 1; check the value")
        factors[name] = v
    return {n: factors[n] for n in names if n in factors}


# --------------------------------------------------- Windows stop path

# Set by the SIGBREAK handler so a stop line can say who stopped it.
# A set, because a signal handler must not do anything interruptible.
BREAK_SEEN = set()


def accept_windows_break():
    """Make CTRL_BREAK raise KeyboardInterrupt on Windows.

    The parent stops a recorder with CTRL_BREAK_EVENT when the stop file
    is not enough (the only console event Windows can aim at one child);
    SIGBREAK's default action kills the process before its finally
    writes meta.json. POSIX needs nothing.
    """
    if os.name != "nt" or not hasattr(signal, "SIGBREAK"):
        return

    def on_break(signum, frame):
        BREAK_SEEN.add(signum)
        raise KeyboardInterrupt

    try:
        signal.signal(signal.SIGBREAK, on_break)
    except (ValueError, OSError) as e:
        print(f"[warn] SIGBREAK handler not installed "
              f"({type(e).__name__}: {e}) -- a graceful stop will end this "
              "process before it writes meta.json", file=sys.stderr)


# ---------------------------------------------------- output lifecycle


def adc_railed(v, vrange):
    # ADC clipping is crisp: RAIL_LEVEL 0.99 of the input range
    import numpy as np
    return np.abs(v) >= RAIL_LEVEL * vrange


def qzfm_output_railed(v):
    # uses the measured soft rail (QZFM_RAIL_THRESH_V); the spec value
    # never fires
    import numpy as np
    return np.abs(v) >= QZFM_RAIL_THRESH_V


def prepare_run_dir(outdir, meta_path):
    """Make outdir and refuse anything a recording could overwrite."""
    try:
        os.makedirs(outdir, exist_ok=True)
    except OSError as e:
        raise OutputIOError(f"cannot create outdir {outdir!r}: {e}")
    if os.path.exists(meta_path):
        # same outdir = same session name; would clobber a finished run
        raise ConfigError(f"refusing to overwrite an existing run "
                          f"({meta_path}); use a fresh --outdir")
    leftovers = recorder_leftovers(outdir)
    if leftovers:
        raise ConfigError(
            f"{outdir} already holds {', '.join(leftovers)} from an "
            f"earlier recording; move or delete them, or use a fresh "
            f"--outdir")


def open_with_headers(specs):
    """Open every (key, path, header) exclusively; all files or none.

    Returns {key: handle} in spec order. On failure every already-open
    handle is closed here (no Recorder.close() exists yet to do it).
    """
    handles = {}
    try:
        for key, path, header in specs:
            fh = open(path, "x", newline="\n", encoding="utf-8")
            handles[key] = fh
            fh.write(header)
    except OSError as e:
        for fh in handles.values():
            with contextlib.suppress(Exception):
                fh.close()
        raise OutputIOError(f"cannot open output file: {e}")
    return handles


def transactional_block(handles, do_writes,
                        what="channel files, raw CSV and meta"):
    """Run do_writes(); any failure rolls every file back to the block boundary.

    Catches BaseException, not just OSError: Ctrl-C inside np.savetxt
    tears a block like a full disk does. OSError becomes OutputIOError;
    anything else re-raises after the rollback. Commit stats only after
    this returns.
    """
    marks = [(fh, fh.tell()) for fh in handles]
    try:
        do_writes()
    except OSError as e:
        failed = rollback_block(marks)
        if failed:
            raise OutputIOError(
                f"write failed mid-block ({e}) AND the rollback failed "
                f"for {'; '.join(failed)} -- data consistency of {what} "
                f"is UNKNOWN, treat this run as suspect")
        raise OutputIOError(
            f"write failed mid-block ({e}) -- all files rolled back to "
            f"the last complete block; {what} stay consistent")
    except BaseException:
        failed = rollback_block(marks)
        if failed:
            print(f"[warn] rollback after an interrupted block failed "
                  f"for {'; '.join(failed)} -- data consistency of "
                  f"{what} is UNKNOWN", file=sys.stderr, flush=True)
        raise


def stage_channel_stats(v, vrange, lsb, *, qzfm=True, codes_room=False):
    """One channel's block quality stats, computed before any file write.

    qzfm=False for aux channels (raw volts, no rail/pinned semantics);
    codes_room=True adds the block's ADC code list (caller bounds it).
    """
    import numpy as np
    s = {"rail": int(np.count_nonzero(adc_railed(v, vrange))),
         "vmin": float(v.min()), "vmax": float(v.max()),
         "qzfm_rail": 0, "pinned": False, "codes": None}
    if qzfm:
        s["qzfm_rail"] = int(np.count_nonzero(qzfm_output_railed(v)))
        # pinned-output signature = per-block dominant-code fraction
        # (the wide-window failure was 99.7% of samples in one code)
        codes, counts = np.unique(np.rint(v / lsb), return_counts=True)
        s["pinned"] = bool(counts.max() >= PINNED_DOMINANT_FRAC * v.size)
        if codes_room:
            s["codes"] = codes.astype(int).tolist()
    return s
SOFTWARE = "bfield.acquisition.recorder"

# default run-dir family per topology (the legacy programs' names);
# keeps result/ sortable by eye
DEFAULT_DATA_DIR = {"1head": "newdata", "2head": "newxyz",
                    "4head": "newgrad"}

# what the old programs wrote into meta["software"]; analyses gate on
# those strings, so meta carries software and software_legacy
LEGACY_SOFTWARE = {"1head": "qzfm_analog.py",
                   "2head": "qzfm_3axis.py",
                   "4head": "qzfm_gradient.py"}


# ------------------------------------------------------------ topologies
# one object per bench shape; the recording path has no topology branches

class Topology:
    key = ""
    label = ""
    map_flag = "--map"

    def default_map(self):
        return None

    def parse(self, args, device):
        """-> dict of Cfg fields this topology defines."""
        raise NotImplementedError

    def validate(self, cfg, is_acquire):
        """Topology-specific refusals and warnings (cfg.warnings)."""

    def derived_specs(self, rec):
        """Extra output files beyond the pT/aux/raw set: (key, path, header)."""
        return []

    def derived_writes(self, rec, pT_by_name, np):
        """Write those files for one block, inside the transaction."""

    def fold_block(self, rec, blk, np):
        """Extra per-block statistics (committed after the write)."""

    def meta(self, rec):
        """Extra meta.json keys."""
        return {}

    def summary(self, cfg, rec):
        """Extra console lines at the end of a run."""
        return []


class OneHead(Topology):
    key, label = "1head", "one head"

    def default_map(self):
        return "z:ai0,y:ai1,x:ai2"

    def parse(self, args, device):
        axes = [a.strip().lower() for a in args.axes.split(",") if a.strip()]
        if not axes or any(a not in VALID_AXES for a in axes):
            raise ConfigError(f"--axes must be a subset of {VALID_AXES}, "
                              f"got {args.axes!r}")
        if len(set(axes)) != len(axes):
            raise ConfigError(f"--axes has duplicate axes: {args.axes!r}")
        sensor_id = str(args.sensor_id).strip()
        if not SENSOR_ID_RX.fullmatch(sensor_id):
            raise ConfigError(
                f"--sensor-id {args.sensor_id!r}: give the serial printed on "
                f"the head, e.g. {DEFAULT_SENSOR_ID} (letters, digits, . _ -)")
        channel_map = parse_axis_map(args.map, axes, device)
        aux = parse_aux(args.aux, axes, device)
        cal_factors = parse_cal_factors(
            args.cal_factors, axes, noun="axis", known=VALID_AXES,
            have_hint=f"being recorded (axes: {','.join(axes)})")
        return {"channel_map": channel_map, "pt_names": axes, "aux": aux,
                "head_of": {ax: "a" for ax in axes},
                "sensor_id": sensor_id, "cal_factors": cal_factors}

    def validate(self, cfg, is_acquire):
        if cfg.cal_factors is not None:
            for ax in cfg.pt_names:
                if ax not in cfg.cal_factors:
                    cfg.warnings.append(f"no cal factor recorded for axis {ax}")
        if (cfg.cal_v_per_nT is not None
                and abs(cfg.cal_v_per_nT - WIZARD_GAIN_V_PER_NT) < 1e-9
                and not same_head(cfg.sensor_id, DEFAULT_SENSOR_ID)):
            if head_is_unknown(cfg.sensor_id):
                # cannot tell if this is a different head: FTDI serial
                # vs the QuSpin name the 2.624 was filed under
                cfg.warnings.append(
                    f"sensor {cfg.sensor_id} with --cal-v-per-nT "
                    f"{WIZARD_GAIN_V_PER_NT:g}: cannot tell if this is the "
                    f"measured head ({DEFAULT_SENSOR_ID}) -- "
                    f"{cfg.sensor_id} is an FTDI-style serial, the gain is "
                    "filed under a QuSpin name. Add the pair to "
                    "HEAD_ALIASES once checked; until then the scale is "
                    f"provisional (nominal for {cfg.gain} is "
                    f"{GAIN_TABLE[cfg.gain]:g})")
            else:
                cfg.warnings.append(
                    f"sensor {cfg.sensor_id} with --cal-v-per-nT "
                    f"{WIZARD_GAIN_V_PER_NT:g}: that gain was measured on "
                    f"{DEFAULT_SENSOR_ID}, not this head -- use the nominal "
                    f"{GAIN_TABLE[cfg.gain]:g} until this head is "
                    "gain-checked")

    def meta(self, rec):
        cfg = rec.cfg
        return {"axes": list(cfg.pt_names),
                "channel_map": dict(cfg.channel_map),
                "aux_map": dict(cfg.aux),
                "sensor_id": cfg.sensor_id}


class TwoHead(Topology):
    key, label = "2head", "two dual-axis heads"

    def default_map(self):
        return DEFAULT_TWO_HEAD_MAP

    def parse(self, args, device):
        entries = parse_head_map(args.map, device)
        names = list(entries)
        channel_map = {n: entries[n][1] for n in names}
        head_of = {n: entries[n][0] for n in names}
        head_ids = parse_head_ids(args.ids)
        # z2 = same axis read on the other head; pairs feed the drift check
        pairs = []
        for name in names:
            if not name.endswith("2"):
                continue
            base = name[:-1]
            if base in names and head_of[base] != head_of[name]:
                pairs.append((base, name))
        cal_factors = parse_cal_factors(
            args.cal_factors, names, noun="channel", known=names,
            have_hint=f"being recorded (channels: {','.join(names)})")
        # --aux allowed here: the S(f) sweep needs the current monitor
        # on the same clocked task
        aux = parse_aux(args.aux, names, device)
        return {"channel_map": channel_map, "pt_names": names, "aux": aux,
                "head_of": head_of, "head_ids": head_ids, "pairs": pairs,
                "cal_factors": cal_factors}

    def validate(self, cfg, is_acquire):
        if not cfg.pairs:
            cfg.warnings.append(
                "no shared-axis pair in --map: the block-mean-r drift check "
                "needs the same axis on both heads, e.g. z on a and z2 on b")
        if len(set(cfg.head_of.values())) < 2:
            cfg.warnings.append(
                "every channel in --map is on one head -- this is a "
                "one-head run wearing a two-head map")

    def fold_block(self, rec, blk, np):
        for a, b in rec.cfg.pairs:
            ma = float(blk[rec.row_of[a]].mean())
            mb = float(blk[rec.row_of[b]].mean())
            rec.update_pair(a, b, ma, mb)

    def meta(self, rec):
        cfg = rec.cfg
        pairs_meta = []
        for a, b in cfg.pairs:
            r = rec.pair_r(a, b)
            pairs_meta.append({
                "axis": a, "second": b,
                "heads": [cfg.head_of[a], cfg.head_of[b]],
                "n_blocks": rec.pair_n(a, b),
                "blockmean_r": round(r, 4) if r is not None else None,
            })
        return {"channels": dict(cfg.channel_map),
                "aux_map": dict(cfg.aux),
                "heads": dict(cfg.head_of),
                "head_ids": dict(cfg.head_ids),
                "pairs": pairs_meta}

    def summary(self, cfg, rec):
        lines = []
        for a, b in cfg.pairs if rec.total else ():
            r = rec.pair_r(a, b)
            lines.append(f"  [pair {a}/{b}] block-mean r="
                         + ("n/a (needs 3 blocks)" if r is None
                            else f"{r:+.3f}"))
        return lines


class FourHead(Topology):
    key, label = "4head", "four single-axis heads"
    map_flag = "--sensors"

    def parse(self, args, device):
        sensors = parse_sensors(args.sensors, device)
        labels = list(sensors)
        head_ids = (parse_sensor_ids(args.ids, labels) if args.ids else {})
        gradients = [parse_gradient(g, labels) for g in (args.gradient or [])]
        seen = {}
        for g in gradients:
            key = tuple(sorted((g.a, g.b)))
            if key in seen:
                raise ConfigError(
                    f"--gradient {g.a},{g.b} given twice (baselines "
                    f"{seen[key]:g} and {g.baseline_m:g} m): both would "
                    f"write the same output file")
            seen[key] = g.baseline_m
        return {"channel_map": dict(sensors), "pt_names": labels, "aux": {},
                "head_of": {lab: lab for lab in labels},
                "head_ids": head_ids, "gradients": gradients}

    def validate(self, cfg, is_acquire):
        if is_acquire and not cfg.head_ids:
            # every hardware run: without serials the record cannot say
            # which physical head produced which column
            raise ConfigError(
                "a hardware run must identify the physical heads: --ids "
                "s1:SERIAL,s2:SERIAL,s3:SERIAL,s4:SERIAL (QuSpin serials)")
        if is_acquire and cfg.gradients:
            # the gradient is signed and only --notes can record which
            # head sat at which end; without it the sign is unrecoverable
            if len(cfg.notes.strip()) < 20:
                raise ConfigError(
                    "a --gradient run must record the bench layout in "
                    "--notes (sensor ends, head axes, baseline direction) "
                    "-- the sign of G = (B[a] - B[b]) / L is unrecoverable "
                    'without it. e.g. --notes "s1 north end, s2 south, '
                    'baseline along +y, all heads on lab z"')
            if not cfg.head_ids:
                raise ConfigError(
                    "a --gradient run must identify the physical heads: "
                    "--ids s1:SERIAL,s2:SERIAL,s3:SERIAL,s4:SERIAL")
            cfg.warnings.append(
                f"all four heads converted with ONE scale "
                f"({cfg.pT_per_V:.4g} pT/V, {cfg.v_per_nT:g} V/nT): a gain "
                f"mismatch becomes a false gradient (2.624 vs 2.7 V/nT in "
                f"a uniform 1 nT field: about -350 pT/m over 8 cm) -- for "
                f"calibrated work redo the conversion per head from the "
                f"raw volts")
        elif is_acquire:
            cfg.warnings.append(
                "no --gradient given: recording the four fields only, no "
                "gradient files")

    def derived_specs(self, rec):
        cfg = rec.cfg
        specs = []
        for pair in cfg.gradients:
            path = os.path.join(
                rec.outdir,
                f"{rec.prefix}_G_{pair.a.upper()}_{pair.b.upper()}"
                f"_pT_per_m.txt")
            rec.gradient_paths[pair.name] = path
            specs.append((
                ("grad", pair.name), path,
                f"# QZFM gradient | pair={pair.a.upper()}-{pair.b.upper()} "
                f"baseline_m={pair.baseline_m:g}\n"
                f"# G = (B[{pair.a.upper()}] - B[{pair.b.upper()}]) / "
                f"{pair.baseline_m:g} m -- the SIGN is only readable\n"
                f"# with the bench layout: which head sat at which end\n"
                f"# of the baseline is in --notes, not in the data\n"
                f"# both heads converted with ONE scale "
                f"({cfg.pT_per_V:.4g} pT/V): a head-to-head gain\n"
                f"# mismatch turns into a false gradient, so\n"
                f"# calibrated work must redo this from the raw volts\n"
                f"# start={rec.start_iso} fs={cfg.fs} Hz "
                f"session={cfg.session}\n"
                "# one value per line, unit=pT/m\n"))
        return specs

    def derived_writes(self, rec, pT_by_name, np):
        # plain difference; the delay-limited CMRR ceiling goes to meta
        for pair in rec.cfg.gradients:
            grad = (pT_by_name[pair.a] - pT_by_name[pair.b]) / pair.baseline_m
            fh = rec.derived_files[pair.name]
            np.savetxt(fh, grad, fmt="%.4g")
            fh.flush()

    def meta(self, rec):
        cfg = rec.cfg
        return {
            "sensors": dict(cfg.channel_map),
            "head_ids": dict(cfg.head_ids),
            "gradients": [
                {"a": p.a, "b": p.b, "baseline_m": p.baseline_m,
                 # one shared V/nT converts both heads; a gain mismatch
                 # appears as a false gradient
                 "single_gain_conversion": True,
                 "expected_cmrr_db_at_30hz": round(
                     timing_cmrr_ceiling_db(pair_skew_s(cfg, p)), 1)}
                for p in cfg.gradients
            ],
        }


TOPOLOGIES = {t.key: t for t in (OneHead(), TwoHead(), FourHead())}


# ------------------------------------------------------- channel parsing

def parse_axis_map(spec, axes, device):
    # 'z:ai0,y:ai1' -> {'z': 'Dev2/ai0', ...}; unselected valid axes
    # are dropped at the end, but a non-axis name is rejected as a typo
    entries = {}
    for pair in (p.strip() for p in spec.split(",")):
        if not pair:
            raise ConfigError(
                f"--map {spec!r} has an empty entry (a stray or trailing "
                "comma); write e.g. z:ai0,y:ai1")
        ax, sep, phys = (s.strip() for s in pair.partition(":"))
        phys, ax = phys.lower(), ax.lower()
        if not sep or not re.fullmatch(DAQ_DIFF_AI, phys):
            raise ConfigError(
                f"bad --map entry {pair!r} (want 'z:ai0'; USB-6008 "
                f"differential inputs are ai0..ai3)")
        if ax not in VALID_AXES:
            raise ConfigError(
                f"--map names {ax!r}, which is not an axis "
                f"({','.join(VALID_AXES)})")
        # a repeated axis is an error, not last-wins
        if ax in entries:
            raise ConfigError(f"--map maps axis {ax!r} twice")
        entries[ax] = f"{device}/{phys}"
    missing = [ax for ax in axes if ax not in entries]
    if missing:
        raise ConfigError(f"--map is missing channels for axes {missing}")
    return {ax: entries[ax] for ax in axes}


def parse_aux(specs, axes, device):
    # extra raw-volt channels (coil monitor etc.), same clock, no calibration
    aux = {}
    for spec in specs or []:
        name, sep, phys = (s.strip() for s in str(spec).partition(":"))
        name, phys = name.lower(), phys.lower()
        if (not sep or not re.fullmatch(DAQ_DIFF_AI, phys)
                or not re.fullmatch(r"[a-z][a-z0-9_]{0,15}", name)):
            raise ConfigError(
                f"bad --aux {spec!r} (want e.g. mon:ai3; USB-6008 "
                f"differential inputs are ai0..ai3)")
        # x2/y2/z2 stay reserved in every topology: an aux named y2
        # would read as a second-head axis in the file listings
        reserved = set(axes) | set(VALID_AXES) | {a + "2" for a in VALID_AXES}
        if name in aux or name in reserved:
            raise ConfigError(f"--aux name {name!r} is already taken (an axis "
                              f"name, or an earlier --aux)")
        aux[name] = f"{device}/{phys}"
    return aux


def parse_head_map(spec, device):
    # 'z:a:ai0,y:a:ai1,z2:b:ai2' -> ordered {name: (head, Dev/aiN)}
    entries = {}
    for item in (p.strip() for p in spec.split(",")):
        # an empty field is an error (silently dropping it once remapped pins)
        if not item:
            raise ConfigError(
                f"--map {spec!r} has an empty entry (a stray or trailing "
                "comma); write e.g. z:a:ai0,y:a:ai1")
        parts = [s.strip().lower() for s in item.split(":")]
        if len(parts) != 3:
            raise ConfigError(
                f"bad --map entry {item!r} (want axis:head:aiN, e.g. z:a:ai0)")
        name, head, phys = parts
        if not re.fullmatch(MAP_AXIS_RX, name):
            raise ConfigError(
                f"bad axis name {name!r} in --map (x,y,z, or x2/y2/z2 for a "
                f"second reading of that axis)")
        if head not in HEADS:
            raise ConfigError(
                f"bad head {head!r} in --map (--2head drives two heads, "
                f"a and b)")
        if not re.fullmatch(DAQ_DIFF_AI, phys):
            raise ConfigError(
                f"bad channel in {item!r}; USB-6008 differential inputs "
                f"are ai0..ai3")
        if name in entries:
            raise ConfigError(f"axis {name!r} mapped twice")
        entries[name] = (head, f"{device}/{phys}")
    return entries


def parse_head_ids(spec):
    # these serials become the head labels in every meta.json and file
    # header, so a typo is permanent provenance
    ids = {"a": DEFAULT_SENSOR_ID, "b": DEFAULT_SENSOR_ID_B}
    seen = set()
    for item in (p.strip() for p in (spec or "").split(",")):
        if not item:
            if spec and spec.strip():
                raise ConfigError(
                    f"--ids {spec!r} has an empty entry (a stray or trailing "
                    f"comma); write e.g. a:{DEFAULT_SENSOR_ID}")
            continue
        head, sep, serial = (s.strip() for s in item.partition(":"))
        head = head.lower()
        if not sep or head not in HEADS or not serial:
            raise ConfigError(
                f"bad --ids entry {item!r} (want a:SERIAL or b:SERIAL)")
        if not SENSOR_ID_RX.fullmatch(serial):
            raise ConfigError(
                f"bad serial {serial!r} in --ids (letters, digits, . _ - "
                f"only, up to 32 characters; e.g. a:{DEFAULT_SENSOR_ID})")
        # a repeated head is an error, not last-wins
        if head in seen:
            raise ConfigError(f"--ids gives head {head!r} twice")
        seen.add(head)
        ids[head] = serial
    # a and b must be two physical sensors; an FTDI serial and its
    # QuSpin name count as one head
    if same_head(ids["a"], ids["b"]):
        raise ConfigError(
            f"--ids names one sensor twice: {ids['a']!r} and {ids['b']!r} "
            "are the same head (FTDI serial and QuSpin name count as one); "
            "give the two different heads")
    return ids


def parse_sensors(spec, device):
    sensors = {}
    for pair in filter(None, (p.strip() for p in spec.split(","))):
        label, sep, phys = (s.strip().lower() for s in pair.partition(":"))
        if not sep or not re.fullmatch(r"s[1-4]", label):
            raise ConfigError(f"bad sensor entry {pair!r} (want s1:ai0)")
        if not re.fullmatch(DAQ_DIFF_AI, phys):
            raise ConfigError(f"bad sensor channel {pair!r}; use ai0..ai3")
        if label in sensors:
            raise ConfigError(f"duplicate sensor label {label!r}")
        sensors[label] = f"{device}/{phys}"
    if len(sensors) != 4 or set(sensors) != {"s1", "s2", "s3", "s4"}:
        raise ConfigError("--sensors must define exactly s1,s2,s3,s4")
    return dict(sorted(sensors.items()))


def parse_sensor_ids(spec, labels):
    """--ids s1:SERIAL,... -> {label: serial}; all four named, all distinct heads.

    s1..s4 are bench positions, not identities; the serials let a
    per-head calibration be reapplied to the data later.
    """
    ids = {}
    for pair in filter(None, (p.strip() for p in str(spec).split(","))):
        label, sep, serial = (s.strip() for s in pair.partition(":"))
        label = label.lower()
        if not sep or label not in labels:
            raise ConfigError(f"bad --ids entry {pair!r} (want s1:AB3Q)")
        if not SENSOR_ID_RX.fullmatch(serial):
            raise ConfigError(f"bad --ids serial in {pair!r}")
        if label in ids:
            raise ConfigError(f"duplicate --ids label {label!r}")
        ids[label] = serial
    missing = sorted(set(labels) - set(ids))
    if missing:
        raise ConfigError(f"--ids must name every sensor; missing: "
                          f"{', '.join(missing)}")
    done = sorted(ids.items())
    for i, (la, sa) in enumerate(done):
        for lb, sb in done[i + 1:]:
            if same_head(sa, sb):
                raise ConfigError(
                    f"--ids gives {la} and {lb} the same physical head "
                    f"({sa!r} / {sb!r}) -- four sensors need four heads")
    return dict(done)


@dataclass
class GradientPair:
    a: str
    b: str
    baseline_m: float

    @property
    def name(self):
        return f"{self.a}_{self.b}"


def parse_gradient(spec, labels):
    pair_part, sep, length_part = spec.partition(":")
    if not sep:
        raise ConfigError(f"bad --gradient {spec!r} (want s1,s2:0.08)")
    a, comma, b = pair_part.partition(",")
    a, b = a.strip().lower(), b.strip().lower()
    if not comma or a not in labels or b not in labels or a == b:
        raise ConfigError(f"bad --gradient pair {pair_part!r}")
    try:
        baseline_m = float(length_part)
    except ValueError:
        baseline_m = -1.0
    # a baseline of a metre or more on one bench is a units typo
    if not (0 < baseline_m < 1.0):
        raise ConfigError(
            "--gradient baseline must be a length in metres, 0 < L < 1 "
            "(typical 0.08)")
    return GradientPair(a, b, baseline_m)


SENSOR_DELAY_SPREAD_S = 3e-4  # head-to-head analog delay spread (Elzenheimer 2025)


def timing_cmrr_ceiling_db(skew_s, f_hz=30.0):
    """Upper bound on common-mode rejection set by timing alone.

    Residual ~ 2*pi*f*tau caps rejection around 25-35 dB in the
    tens-of-Hz band (Elzenheimer 2025). A ceiling, not a prediction:
    gain mismatch, noise and field non-uniformity all push it lower.
    """
    return -20.0 * math.log10(2.0 * math.pi * f_hz * skew_s)


def pair_skew_s(cfg, pair):
    # mux scan skew (USB-6008 samples channels in turn) + analog delay
    # spread; the gap is a distance in scan order, not in ai numbers
    order = cfg.names
    mux = (abs(order.index(pair.a) - order.index(pair.b))
           / (cfg.fs * len(order)))
    return mux + SENSOR_DELAY_SPREAD_S


# ------------------------------------------------------------- config

@dataclass
class Cfg:
    session: str
    topology: str
    channel_map: dict            # name -> Dev/aiN, CLI order = DAQ row order
    pt_names: list               # channels that get a pT file
    aux: dict                    # name -> Dev/aiN, raw volts only
    head_of: dict                # name -> head label
    fs: float
    seconds: float
    vrange: float
    gain: str
    device: str | None
    outdir: str | None
    rail_limit: float
    rail_abort: bool
    no_rail_check: bool
    flat_eps: float
    cal_v_per_nT: float | None
    cal_factors: dict | None
    notes: str
    stop_file: str | None
    sensor_id: str = ""
    head_ids: dict = field(default_factory=dict)
    pairs: list = field(default_factory=list)
    gradients: list = field(default_factory=list)
    warnings: list = field(default_factory=list)
    lab: object = None           # bfield.lab.Lab: bench or simulator
    sensor_info: object = None   # [{head, number, label, usb_serial, ...}]

    @property
    def topo(self):
        return TOPOLOGIES[self.topology]

    @property
    def names(self):
        # magnetic channels first, then aux, in DAQ row order
        return [*self.pt_names, *self.aux]

    @property
    def channels(self):
        return ([self.channel_map[n] for n in self.pt_names]
                + list(self.aux.values()))

    @property
    def source(self):
        # "simulate" is the value the analysis tools refuse without
        # --allow-sim (the old recorder's simulator wrote it too)
        if self.lab is not None and self.lab.sim:
            return "simulate"
        return "analog(USB-6008)"

    @property
    def v_per_nT(self):
        return (self.cal_v_per_nT if self.cal_v_per_nT is not None
                else GAIN_TABLE[self.gain])

    @property
    def pT_per_V(self):
        # the only volts->pT scale; cal_factors are QuSpin Calibrate numbers,
        # provenance only (trace a run to its Field Zero+Calibrate), never applied
        return 1000.0 / self.v_per_nT


def instrument_notes(cfg, requested_device):
    """Instrument-level warnings common to every topology."""
    notes = []
    if cfg.vrange > QZFM_RAIL_V:
        notes.append(
            f"--vrange {cfg.vrange:g} exceeds the QZFM analog output rail "
            f"(+/-{QZFM_RAIL_V:g} V): readings at the rail are saturated "
            f"output, not field -- for large DC use --gain 0.33x")
    for k, label in ((1, ""), (2, " 2nd harmonic")):
        alias = mod_alias_hz(cfg.fs, k)
        if alias <= LF_BAND_HZ:
            notes.append(
                f"QZFM {k * QZFM_MOD_HZ:g} Hz modulation{label} aliases to "
                f"{alias:.0f} Hz at fs={cfg.fs:g}: a spectral line there is "
                f"instrumental, not field")
    if cfg.cal_factors is not None:
        for name, f in cfg.cal_factors.items():
            if f > CAL_FACTOR_WARN:
                notes.append(
                    f"cal factor {name}:{f:g} exceeds {CAL_FACTOR_WARN:g} "
                    "(QuSpin health rule) -- the sensor cell may need service")
    if (cfg.cal_v_per_nT is not None
            and abs(cfg.cal_v_per_nT / GAIN_TABLE[cfg.gain] - 1.0) > 0.2):
        notes.append(
            f"--cal-v-per-nT {cfg.cal_v_per_nT:g} is far from the "
            f"{cfg.gain} table value {GAIN_TABLE[cfg.gain]:g} V/nT -- "
            "gain mode and calibration probably disagree")
    return notes


def parse_sensor_info(text):
    """--sensor-info: the parent's json list of physical sensors."""
    if not text:
        return None
    try:
        info = json.loads(text)
    except ValueError as e:
        raise ConfigError(f"--sensor-info is not valid json ({e})")
    if not isinstance(info, list) or not all(isinstance(r, dict)
                                             for r in info):
        raise ConfigError("--sensor-info must be a json list of objects")
    return info


def build_cfg(args):
    hardware_check = args.hardware_check
    is_acquire = not hardware_check and not args.probe
    topo = TOPOLOGIES[args.topology]

    if args.gain not in GAIN_TABLE:
        raise ConfigError(f"--gain must be one of {list(GAIN_TABLE)}")
    try:
        vrange = float(str(args.vrange).strip())
    except ValueError:
        vrange = -1.0
    if vrange not in VALID_VRANGE:
        raise ConfigError(f"--vrange must be one of {VALID_VRANGE}")
    if not (args.fs >= 1) or not math.isfinite(args.fs):
        raise ConfigError("--fs must be >= 1 (one read block = int(fs) samples)")
    # nan/inf pass plain ">" tests: --seconds nan runs unbounded,
    # --cal-v-per-nT inf writes zeros
    if not math.isfinite(args.seconds):
        raise ConfigError(
            "--seconds must be finite (0 = run until stopped)")
    if args.cal_v_per_nT is not None and not (
            args.cal_v_per_nT > 0 and math.isfinite(args.cal_v_per_nT)):
        raise ConfigError("--cal-v-per-nT must be a finite value > 0 (V/nT)")
    if not (0 <= args.rail_limit <= 1):
        raise ConfigError("--rail-limit is a fraction of samples: 0..1")
    if args.rail_limit == 1.0:
        # the check is "fraction > limit": a limit of 1 can never fire,
        # so a fully railed channel would pass
        raise ConfigError(
            "--rail-limit 1 can never be exceeded (the check is fraction > "
            "limit), so a fully railed channel would pass -- use "
            "--no-rail-check to disable, or 0.999")
    if not (args.flat_eps >= 0):
        raise ConfigError("--flat-eps must be >= 0 (V)")

    # reject flags that belong to another topology; accepted silently
    # they would be ignored and the run would differ from what was typed
    for flag, given, wanted in (
            ("--axes", args.axes != DEFAULT_AXES, {"1head"}),
            ("--aux", bool(args.aux), {"1head", "2head"}),
            ("--sensor-id", args.sensor_id != DEFAULT_SENSOR_ID, {"1head"}),
            ("--sensors", args.sensors is not None, {"4head"}),
            ("--gradient", bool(args.gradient), {"4head"}),
            ("--map", args.map is not None, {"1head", "2head"}),
            ("--ids", args.ids is not None, {"2head", "4head"})):
        if given and args.topology not in wanted:
            belongs = " or ".join(f"--{w}" for w in sorted(wanted))
            raise ConfigError(
                f"{flag} belongs to {belongs} runs; this is a "
                f"--{args.topology} run ({topo.label})")

    device = args.lab.daq().resolve_device(args.device, hardware_check)
    if args.topology == "4head" and args.sensors is None:
        args.sensors = "s1:ai0,s2:ai1,s3:ai2,s4:ai3"
    if args.map is None:
        args.map = topo.default_map()
    fields = topo.parse(args, device)

    n_ch = len(fields["pt_names"]) + len(fields.get("aux") or {})
    if args.fs * n_ch > DAQ_AGGREGATE_MAX:
        raise ConfigError(
            f"aggregate rate {args.fs * n_ch:.0f} S/s exceeds the USB-6008 "
            f"limit of {DAQ_AGGREGATE_MAX} (fs x n_channels); lower --fs or "
            f"drop a channel")
    phys = [*fields["channel_map"].values(), *(fields.get("aux") or {}).values()]
    dupes = sorted({p for p in phys if phys.count(p) > 1})
    if dupes:
        raise ConfigError(
            f"physical channel(s) assigned more than once: {', '.join(dupes)} "
            f"(the USB-6008 has 4 differential inputs, ai0..ai3)")

    outdir = args.outdir
    session = ""
    if is_acquire:
        if outdir is None:
            outdir = unique_outdir(os.path.join(
                args.lab.data_root, DEFAULT_DATA_DIR[args.topology]))
        session = os.path.basename(os.path.normpath(outdir))

    cfg = Cfg(
        session=session,
        topology=args.topology,
        channel_map=fields["channel_map"],
        pt_names=fields["pt_names"],
        aux=fields.get("aux") or {},
        head_of=fields.get("head_of") or {},
        fs=float(args.fs),
        seconds=float(args.seconds),
        vrange=vrange,
        gain=args.gain,
        device=device,
        outdir=outdir,
        rail_limit=args.rail_limit,
        rail_abort=args.rail_abort,
        no_rail_check=args.no_rail_check,
        flat_eps=args.flat_eps,
        cal_v_per_nT=args.cal_v_per_nT,
        cal_factors=fields.get("cal_factors"),
        notes=args.notes or "",
        stop_file=args.stop_file,
        sensor_id=fields.get("sensor_id", ""),
        head_ids=fields.get("head_ids") or {},
        pairs=fields.get("pairs") or [],
        gradients=fields.get("gradients") or [],
        warnings=[],
        lab=args.lab,
        sensor_info=parse_sensor_info(args.sensor_info),
    )
    cfg.warnings = instrument_notes(cfg, args.device)
    topo.validate(cfg, is_acquire)
    return cfg


# ------------------------------------------------------------ data source

def block_source(cfg, block_n, seconds):
    """The generator acquire() and the probe both read blocks from."""
    n_ch = len(cfg.names)
    daq = cfg.lab.daq().open(cfg.channels, cfg.vrange, cfg.fs)
    return daq, daq.blocks(n_ch, block_n)


# --------------------------------------- recorder: files, meta, counters

class Recorder:

    def __init__(self, cfg):
        self.cfg = cfg
        if not cfg.outdir:
            raise ConfigError("internal: Recorder requires outdir")
        self.outdir = cfg.outdir
        self.prefix = cfg.session
        self.meta_path = os.path.join(self.outdir, f"{self.prefix}.meta.json")
        self.names = cfg.names
        self.row_of = {name: i for i, name in enumerate(self.names)}
        # filename: channel = last '_' field before the suffix;
        # operations.sync_check parses it that way -- keep in step
        self.pt_paths = {
            n: os.path.join(self.outdir, f"{self.prefix}_{n.upper()}_pT.txt")
            for n in cfg.pt_names}
        self.aux_paths = {
            n: os.path.join(self.outdir, f"{self.prefix}_{n.upper()}_V.txt")
            for n in cfg.aux}
        self.gradient_paths = {}
        self.raw_path = os.path.join(self.outdir, f"{self.prefix}_raw.csv")
        self.start_iso = clock.iso_now()
        self.total = 0
        self.rail = {n: 0 for n in self.names}
        self.nonfinite = {n: 0 for n in self.names}
        self.qzfm_rail = {n: 0 for n in cfg.pt_names}
        self.vmin = {n: math.inf for n in self.names}
        self.vmax = {n: -math.inf for n in self.names}
        self.codes = {n: set() for n in cfg.pt_names}
        self.pin_blocks = {n: 0 for n in cfg.pt_names}
        self.n_blocks = 0
        self._lsb = cfg.vrange / 2048.0  # 12 bit over +/-vrange
        # running sums for the block-mean Pearson r, one set per pair;
        # every key must sum over the same n blocks
        self._pair = {(a, b): {"n": 0, "sum_a": 0.0, "sum_b": 0.0,
                               "sum_aa": 0.0, "sum_bb": 0.0, "sum_ab": 0.0}
                      for a, b in cfg.pairs}
        self._t_first = self._t_last = None
        self._n_first = self._n_last = 0
        self._pt = {}
        self._aux = {}
        self.derived_files = {}
        self._raw = None
        self._opened = False
        self.meta_write_error = None
        self.data_close_error = None   # a failed flush is a failed run
        self.data_close_path = None

    # ---- counters

    def ptp(self, name):
        return self.vmax[name] - self.vmin[name]

    def rail_fraction(self, name):
        return self.rail[name] / self.total if self.total else 0.0

    def qzfm_rail_fraction(self, name):
        return self.qzfm_rail[name] / self.total if self.total else 0.0

    def adc_railed_names(self, limit):
        # magnetic channels only; an aux channel may sit near the rail
        # on purpose (its stats still go to meta)
        return [n for n in self.cfg.pt_names if self.rail_fraction(n) > limit]

    def qzfm_railed_names(self, limit):
        return [n for n in self.cfg.pt_names
                if self.qzfm_rail_fraction(n) > limit]

    def code_count(self, name):
        return len(self.codes.get(name, ()))

    def pin_fraction(self, name):
        return self.pin_blocks[name] / self.n_blocks if self.n_blocks else 0.0

    def pair_n(self, a, b):
        return self._pair[(a, b)]["n"]

    def update_pair(self, a, b, mean_a, mean_b):
        """Fold one block's pair of means into the running sums.

        All six sums must advance on the same block; pair_r() relies on it.
        """
        st = self._pair[(a, b)]
        st["n"] += 1
        st["sum_a"] += mean_a
        st["sum_b"] += mean_b
        st["sum_aa"] += mean_a * mean_a
        st["sum_bb"] += mean_b * mean_b
        st["sum_ab"] += mean_a * mean_b

    def pair_r(self, a, b):
        # Pearson r of ~1 s block means: drift-band agreement, not
        # broadband (a per-sample r would sit near zero regardless)
        st = self._pair[(a, b)]
        n = st["n"]
        if n < 3:
            return None
        mean_a, mean_b = st["sum_a"] / n, st["sum_b"] / n
        cov = st["sum_ab"] / n - mean_a * mean_b
        var_a = st["sum_aa"] / n - mean_a ** 2
        var_b = st["sum_bb"] / n - mean_b ** 2
        if var_a <= 0.0 or var_b <= 0.0:
            return None
        return cov / math.sqrt(var_a * var_b)

    def tick(self, t):
        # wall-clock ticks for fs_effective; called after the write, so
        # the rate runs end of block 1 to end of the last
        if self._t_first is None:
            self._t_first, self._n_first = t, self.total
        self._t_last, self._n_last = t, self.total

    def clock_elapsed(self):
        if self._t_first is None or self._t_last <= self._t_first:
            return None
        return self._t_last - self._t_first

    def fs_effective(self):
        """Delivered throughput as this process saw it, not the DAQ clock.

        Sags when the disk or console stalls; a real clock check needs
        an external reference. Written to meta as fs_effective_hz,
        which the analyses read.
        """
        elapsed = self.clock_elapsed()
        if elapsed is None or self._n_last <= self._n_first:
            return None
        return (self._n_last - self._n_first) / elapsed

    # ---- files

    def open(self):
        # refusal rules and the all-or-nothing open live in the
        # output-lifecycle helpers up top; this only picks files/headers
        prepare_run_dir(self.outdir, self.meta_path)
        cfg = self.cfg
        specs = []
        for name in cfg.pt_names:
            specs.append((("pt", name), self.pt_paths[name],
                          f"# QZFM acquisition | channel={name.upper()} "
                          f"head={cfg.head_of.get(name, '?')} "
                          f"session={cfg.session}\n"
                          f"# start={self.start_iso} fs={cfg.fs} Hz "
                          f"gain={cfg.gain} cal={cfg.pT_per_V:.4g} pT/V "
                          f"vrange={cfg.vrange} V\n"
                          f"# device={cfg.device} ch={cfg.channel_map[name]} "
                          f"sensor={self.sensor_of(name)} "
                          f"source={cfg.source}\n"
                          "# one value per line, unit=pT\n"))
        for name in cfg.aux:
            specs.append((("aux", name), self.aux_paths[name],
                          f"# QZFM acquisition | aux={name.upper()} "
                          f"session={cfg.session}\n"
                          f"# start={self.start_iso} fs={cfg.fs} Hz "
                          f"vrange={cfg.vrange} V\n"
                          f"# device={cfg.device} ch={cfg.aux[name]} "
                          f"source={cfg.source}\n"
                          "# one value per line, unit=V (raw ADC volts, "
                          "uncalibrated)\n"))
        specs.extend(cfg.topo.derived_specs(self))
        specs.append(("raw", self.raw_path,
                      f"# QZFM raw voltages | session={cfg.session} "
                      f"start={self.start_iso} fs={cfg.fs}\n"
                      "sample," + ",".join(f"V_{n}" for n in self.names)
                      + "\n"))
        handles = open_with_headers(specs)
        for key, fh in handles.items():
            if key == "raw":
                self._raw = fh
                continue
            kind, name = key
            if kind == "pt":
                self._pt[name] = fh
            elif kind == "aux":
                self._aux[name] = fh
            else:
                self.derived_files[name] = fh
        self._opened = True
        return self

    def sensor_of(self, name):
        cfg = self.cfg
        if cfg.sensor_id:
            return cfg.sensor_id
        head = cfg.head_of.get(name)
        return cfg.head_ids.get(head) or "?"

    def write(self, blk):
        import numpy as np
        cfg = self.cfg
        # refuse NaN/inf blocks: they poison the stats and slip past
        # the rail check
        bad = np.count_nonzero(~np.isfinite(blk), axis=1)
        if bad.any():
            for name, k in zip(self.names, bad):
                self.nonfinite[name] += int(k)
            broken = ", ".join(f"{n}: {int(k)}"
                               for n, k in zip(self.names, bad) if k)
            raise BadDataError(
                f"non-finite samples from the DAQ ({broken}); block not "
                f"written -- check wiring and driver, then rerun")
        # stats staged first, committed only after every file accepted
        # the block: a mid-block failure must not advance the counters
        staged, pT_by_name = [], {}
        for i, name in enumerate(self.names):
            v = blk[i]
            is_mag = name in self._pt
            s = stage_channel_stats(
                v, cfg.vrange, self._lsb, qzfm=is_mag,
                codes_room=(is_mag and len(self.codes[name]) < 4096))
            s["name"], s["v"] = name, v
            staged.append(s)
            if is_mag:
                pT_by_name[name] = v * cfg.pT_per_V
        handles = [*self._pt.values(), *self._aux.values(),
                   *self.derived_files.values(), self._raw]

        def writes():
            for name, pT in pT_by_name.items():
                np.savetxt(self._pt[name], pT, fmt="%.2f")
                self._pt[name].flush()
            for name, fh in self._aux.items():
                np.savetxt(fh, blk[self.row_of[name]], fmt="%.6g")
                fh.flush()
            cfg.topo.derived_writes(self, pT_by_name, np)
            index = np.arange(self.total, self.total + blk.shape[1])
            np.savetxt(self._raw, np.column_stack([index, blk.T]),
                       fmt=["%d"] + ["%.6g"] * len(self.names), delimiter=",")
            self._raw.flush()

        transactional_block(handles, writes)
        # count on commit: an interrupt in the folding below must not
        # leave meta's n_samples short of what the files hold
        self.total += blk.shape[1]
        for s in staged:
            name = s["name"]
            self.rail[name] += s["rail"]
            self.vmin[name] = min(self.vmin[name], s["vmin"])
            self.vmax[name] = max(self.vmax[name], s["vmax"])
            if name in self._pt:
                self.qzfm_rail[name] += s["qzfm_rail"]
                if s["pinned"]:
                    self.pin_blocks[name] += 1
                if s["codes"] is not None:
                    self.codes[name].update(s["codes"])
        cfg.topo.fold_block(self, blk, np)
        self.n_blocks += 1

    def _meta(self, status, exit_code, error):
        cfg = self.cfg
        elapsed, fs_eff = self.clock_elapsed(), self.fs_effective()
        meta = {
            "start_iso": self.start_iso,
            "session": cfg.session,
            "topology": cfg.topology,
            "fs_hz": cfg.fs, "n_samples": self.total,
            "duration_s": self.total / cfg.fs,
            # None = not measured; never truthiness-test, a real 0.0
            # got eaten that way (2026-08-09 review, F9)
            "clock_elapsed_s": (None if elapsed is None
                                else round(elapsed, 3)),
            "fs_effective_hz": (None if fs_eff is None
                                else round(fs_eff, 4)),
            # self-describing note: "effective" otherwise reads like a
            # clock measurement
            "fs_effective_hz_note": (
                "host-observed throughput (blocks timed after writing), "
                "not a measurement of the DAQ clock"),
            "gain_mode": cfg.gain, "cal_v_per_nT_used": cfg.v_per_nT,
            "cal_pT_per_V": cfg.pT_per_V,
            "cal_factors": cfg.cal_factors,
            "vrange": cfg.vrange, "device": cfg.device,
            "rail_fraction": {n: self.rail_fraction(n) for n in self.names},
            "n_nonfinite": dict(self.nonfinite),
            "qzfm_rail_fraction": {n: self.qzfm_rail_fraction(n)
                                   for n in cfg.pt_names},
            "flat_ptp_V": {n: self.ptp(n) if self.total else None
                           for n in self.names},
            "adc_unique_codes": {n: self.code_count(n) for n in cfg.pt_names},
            "pinned_block_fraction": {n: self.pin_fraction(n)
                                      for n in cfg.pt_names},
            "mux_skew_note": "USB-6008 multiplexed; <135 Hz negligible",
            "source": cfg.source,
            "software": SOFTWARE,
            # old program name for this topology; analyses gate on it
            "software_legacy": LEGACY_SOFTWARE[cfg.topology],
            "common_version": COMMON_VERSION,
            "status": status, "exit_code": exit_code,
            "error": error, "warnings": cfg.warnings,
            "notes": cfg.notes,
        }
        meta.update(cfg.topo.meta(self))
        if cfg.sensor_info:
            # which physical sensor (number, QuSpin label, USB serial,
            # location, inputs) produced which channel -- an added key,
            # every older key is unchanged
            meta["sensor_info"] = cfg.sensor_info
        return meta

    def close(self, status, exit_code, error):
        if not self._opened:
            return
        # close() flushes buffered samples; a swallowed failure means
        # truncated data with meta saying ok. First failure -> exit 40.
        for fh in (*self._pt.values(), *self._aux.values(),
                   *self.derived_files.values(), self._raw):
            try:
                fh.close()
            except BaseException as e:
                if self.data_close_error is None:
                    self.data_close_error = e
                    self.data_close_path = getattr(fh, "name", "?")
                    print(f"[warn] closing {self.data_close_path} failed: "
                          f"{type(e).__name__}: {e} -- the samples in it "
                          "may be short", file=sys.stderr)
        if self.data_close_error is not None:
            # output integrity outranks the acquisition verdict; the
            # acquisition result stays in meta as context
            err = error_record(self.data_close_error, OutputIOError.code)
            err["path"] = self.data_close_path
            err["acquisition_status"] = status
            err["acquisition_exit_code"] = exit_code
            err["acquisition_error"] = error
            status, exit_code, error = "error", OutputIOError.code, err
        try:
            # atomic write: a plain "w" truncates first, so a Ctrl-C
            # mid-dump could leave an empty meta.json
            write_json_atomic(self.meta_path,
                              self._meta(status, exit_code, error))
        except BaseException as e:
            # remembered so acquire() refuses to exit 0 (no meta = run
            # invisible to every downstream tool)
            self.meta_write_error = e
            print(f"[warn] meta write failed: {e}", file=sys.stderr)


# ------- probe: a short throwaway read to see DC and headroom up front

def probe_volts(cfg, vrange, seconds=PROBE_SECONDS):
    import numpy as np
    block_n = int(cfg.fs)
    n_ch = len(cfg.names)
    n_blocks = max(1, math.ceil(seconds))
    daq = cfg.lab.daq().open(cfg.channels, vrange, cfg.fs)
    try:
        src = daq.blocks(n_ch, block_n)
        return np.concatenate([next(src) for _ in range(n_blocks)], axis=1)
    finally:
        with contextlib.suppress(BaseException):
            daq.close()


def range_probe(cfg):
    import numpy as np
    data = probe_volts(cfg, max(VALID_VRANGE))
    if not np.isfinite(data).all():
        raise BadDataError("non-finite samples in the range probe -- "
                           "check wiring and driver, then rerun")
    peak_v = float(np.abs(data).max())
    railed = [n for n, row in zip(cfg.pt_names, data)
              if qzfm_output_railed(row).any()]
    return {"data": data, "railed": railed, "peak_v": peak_v,
            "chosen_vrange": smallest_vrange(peak_v)}


def run_probe(cfg):
    result = range_probe(cfg)
    print(f"=== probe: {PROBE_SECONDS:.0f} s at +/-{max(VALID_VRANGE):g} V "
          f"({cfg.source}, {cfg.topo.label}) ===")
    for i, name in enumerate(cfg.names):
        v = result["data"][i]
        if name in result["railed"]:
            field_str = "  <-- AT QZFM OUTPUT RAIL (not a field value)"
        elif name in cfg.aux:
            field_str = ""
        else:
            field_str = f" ({v.mean() / cfg.v_per_nT:+.3f} nT)"
        std = (f"std={v.std() * 1e3:.3g} mV" if name in cfg.aux
               else f"std={v.std() * cfg.pT_per_V:.3g} pT")
        print(f"  [{name}] mean={v.mean():+.4g} V{field_str}  {std}  "
              f"min={v.min():+.4g} V  max={v.max():+.4g} V")
    if result["railed"]:
        print(f"  {','.join(result['railed'])}: at the sensor's own "
              f"+/-{QZFM_RAIL_V:g} V rail -- no --vrange helps; "
              f"{REMEDY_SATURATED}.")
    elif result["chosen_vrange"] is None:
        print(f"  peak {result['peak_v']:.3g} V fits no range with "
              f"x{PROBE_HEADROOM:g} headroom -- re-zero first (or check "
              f"wiring).")
    else:
        print(f"  recommended: --vrange {result['chosen_vrange']:g}  "
              f"(peak {result['peak_v']:.3g} V, headroom x{PROBE_HEADROOM:g})")
    return 0


# ---------------------------------------------------------- acquisition

# one remedy sentence, one place
REMEDY_SATURATED = "re-zero, degauss, or use --gain 0.33x"


def qzfm_saturation_error(cfg, rec):
    if cfg.no_rail_check:
        return None
    railed = rec.qzfm_railed_names(cfg.rail_limit)
    if railed:
        return SaturationError(
            f"QZFM analog output rail (|V| >= {QZFM_RAIL_THRESH_V:g} V; "
            f"measured rails +4.27/-4.32 V) reached on: {','.join(railed)} "
            f"-- saturated sensor output, not field; {REMEDY_SATURATED}")
    return None


def adc_saturation_error(cfg, rec):
    if cfg.no_rail_check:
        return None
    railed = rec.adc_railed_names(cfg.rail_limit)
    if railed:
        return SaturationError(
            f"ADC input rail (|V| >= {RAIL_LEVEL:.0%} of the "
            f"+/-{cfg.vrange:g} V range) over {cfg.rail_limit:.0%} of the "
            f"record on: {','.join(railed)} -- the ADC window is too "
            f"small, not the sensor; raise --vrange, or re-zero if "
            f"already at the largest range")
    return None


def saturation_error(cfg, rec):
    return qzfm_saturation_error(cfg, rec) or adc_saturation_error(cfg, rec)


def quality_error(cfg, rec):
    # order matters: a clipped channel is constant, so saturation must
    # be tested before flatness
    if rec.total == 0:
        return NoSignalError("no samples acquired")
    err = saturation_error(cfg, rec)
    if err:
        return err
    flat = [n for n in cfg.pt_names if rec.ptp(n) < cfg.flat_eps]
    if flat:
        return NoSignalError(
            f"flat channel(s) (p-p < {cfg.flat_eps:g} V): {','.join(flat)}")
    pinned = [n for n in cfg.pt_names
              if rec.n_blocks >= 2 and rec.pin_fraction(n) > PINNED_BLOCK_LIMIT]
    if pinned:
        return NoSignalError(
            f"pinned output (one ADC code holds >= {PINNED_DOMINANT_FRAC:.0%} "
            f"of most blocks) on: {','.join(pinned)} -- soft rail, stuck "
            "input, or signal below the ADC resolution of this range")
    return None      # saturation was already ruled out at the top


def acquire(cfg):
    rec = Recorder(cfg)
    for w in cfg.warnings:
        print(f"[note] {w}", file=sys.stderr)
    if cfg.cal_factors is None and cfg.topology != "4head":
        # stderr only, not meta: absent factors are a reminder, not a defect
        print("[note] no --cal-factors given -- record the Field Zero+"
              "Calibrate factors so the run is traceable to its calibration",
              file=sys.stderr)
    task = None
    status, code, error = "ok", 0, None
    stop_reason = "Ctrl-C"
    try:
        BREAK_SEEN.clear()
        rec.open()
        block_n = int(cfg.fs)
        task, source = block_source(cfg, block_n, cfg.seconds)

        # None = run until Ctrl-C; ceil so 0 < seconds < 1/fs still
        # takes one sample instead of truncating to indefinite
        want = (math.ceil(cfg.seconds * cfg.fs)
                if cfg.seconds > 0 else None)
        for blk in source:
            if want is not None and rec.total + blk.shape[1] > want:
                blk = blk[:, : want - rec.total]
                if not blk.shape[1]:
                    break
            rec.write(blk)
            rec.tick(clock.monotonic())
            if cfg.stop_file and os.path.exists(cfg.stop_file):
                # parent stop request; a polled file needs no signal
                # (Windows CTRL_BREAK can kill without running finally)
                stop_reason = "stop file from the parent"
                raise KeyboardInterrupt
            if cfg.rail_abort:
                err = saturation_error(cfg, rec)
                if err:
                    print(f"[abort] {err}; stopping early.", file=sys.stderr)
                    break
            if want is not None and rec.total >= want:
                break

        err = quality_error(cfg, rec)
        if err:
            status, code, error = "flagged", err.code, error_record(err, err.code)
    except KeyboardInterrupt:
        status, code = "interrupted", 130
        note = "partial data saved" if rec.total else "no data yet"
        if BREAK_SEEN and stop_reason == "Ctrl-C":
            # a parent's CTRL_BREAK_EVENT, not an operator's Ctrl-C
            stop_reason = "break from the parent process"
        print(f"\n[stop] {stop_reason}: {note}.", file=sys.stderr)
    except AcqError as e:
        status, code, error = "error", e.code, error_record(e, e.code)
        print(f"ERROR[{e.code}] {type(e).__name__}: {e}", file=sys.stderr)
    except Exception as e:
        status, code, error = "error", 1, error_record(e, 1)
        # unexpected error: print the traceback too, the message alone
        # lacks file and line
        print(f"ERROR[1] {type(e).__name__}: {e}", file=sys.stderr)
        traceback.print_exc()
    finally:
        if task is not None:
            with contextlib.suppress(BaseException):
                task.close()
        rec.close(status, code, error)
    if rec.meta_write_error is not None or rec.data_close_error is not None:
        # unflushed data is not a success; meta keeps the acquisition
        # verdict alongside
        code = OutputIOError.code
    print_summary(cfg, rec, code)
    # 30 and 31 need different hints: a flat channel is usually power
    # or wiring, and re-zeroing for it wastes the 500 s a Field Zero costs
    if code == 30:
        print("[hint] saturated: raise --vrange if the ADC window is the "
              "limit, or re-zero / --gain 0.33x if the sensor output is at "
              "its rail. python run/record.py --probe shows the window in "
              "2 s", file=sys.stderr)
    elif code == 31:
        print("[hint] flat/no signal: check the sensor is powered and "
              "locked, the differential pair (BOTH AI+ and AI- landed), and "
              "that the channel map names the wired pins. python "
              "run/record.py --probe reads 2 s and reports each channel",
              file=sys.stderr)
    return code


def print_summary(cfg, rec, code):
    print(f"\n=== qzfm_record ({cfg.topology}): {cfg.session} ===")
    fs_eff = rec.fs_effective()
    # fs_eff = host throughput, not a clock measurement (see fs_effective)
    extra = f"  fs_eff(host)={fs_eff:.2f} Hz" if fs_eff else ""
    print(f"  samples={rec.total}  duration={rec.total / cfg.fs:.1f}s  "
          f"fs={cfg.fs} Hz{extra}  channels={','.join(cfg.pt_names)}  "
          f"source={cfg.source}")
    if cfg.cal_factors:
        print("  cal-factors=" + ",".join(
            f"{n}:{f:g}" for n, f in cfg.cal_factors.items()))
    for name in rec.names:
        ptp = rec.ptp(name) if rec.total else 0.0
        srail = (f"  qzfm-rail={rec.qzfm_rail_fraction(name):.1%}"
                 if rec.qzfm_rail.get(name) else "")
        # adc-rail = ADC window clipping, qzfm-rail = sensor output
        # clipping; different fixes (meta keys unchanged)
        print(f"  [{name}] p-p={ptp:.4g} V  "
              f"adc-rail={rec.rail_fraction(name):.3%}{srail}")
    for name in cfg.pt_names if rec.total else ():
        mid_nT = (rec.vmin[name] + rec.vmax[name]) / 2.0 / cfg.v_per_nT
        if abs(mid_nT) > QZFM_LINEAR_NT and not rec.qzfm_rail[name]:
            print(f"  [note] {name}: mid-range ~{mid_nT:+.2f} nT is outside "
                  f"the QZFM quasi-linear +/-{QZFM_LINEAR_NT:g} nT regime -- "
                  f"re-zero for precision values")
    for line in cfg.topo.summary(cfg, rec):
        print(line)
    print(f"  outdir={rec.outdir}  exit_code={code}")


# ------------------------------------------------------ hardware check

def run_hardware_check(cfg):
    print(f"=== qzfm_record hardware check ({cfg.topo.label}) ===")
    try:
        daq = cfg.lab.daq()
        devices = daq.list_devices()
        print(f"[daq] devices: {devices or '(none)'}")
        if not devices:
            raise DaqDeviceNotFound("no NI DAQ device found")
        dev = cfg.device or devices[0]
        if dev not in devices:
            raise DaqDeviceNotFound(f"device {dev!r} not in {devices}")

        # check every configured pin at the configured range and rate
        # (a fixed ai0 / +/-1 V / 1 kHz sweep once judged the wrong window)
        pins = [ch.split("/")[-1] for ch in cfg.channels]
        labels = [f"{n.upper()}({p})" for n, p in zip(cfg.names, pins)]
        one = daq.finite_read([f"{dev}/{pins[0]}"], seconds=1.0,
                              vrange=cfg.vrange, fs=cfg.fs)[0]
        print(f"[daq] {dev}/{pins[0]} at +/-{cfg.vrange:g} V: "
              f"mean={one.mean():.4g} std={one.std():.4g} "
              f"min={one.min():.4g} max={one.max():.4g} V")
        rows = daq.finite_read([f"{dev}/{p}" for p in pins],
                               seconds=2.0, vrange=cfg.vrange, fs=cfg.fs)
        flat, railed = [], []
        for row, name in zip(rows, labels):
            ptp = float(row.max() - row.min())
            near_rail = bool(adc_railed(row, cfg.vrange).mean() > 0.01)
            if ptp < 1e-4:
                flat.append(name)
            if near_rail:
                railed.append(name)
            flag = ("  <-- FLAT? check wiring" if ptp < 1e-4
                    else "  <-- near RAIL" if near_rail else "")
            print(f"[chan] {name}: p-p={ptp:.4g} V{flag}")
    except AcqError:
        raise
    except Exception as e:
        raise DaqSetupError(f"hardware check failed unexpectedly: {e}")
    # flat or railed = nonzero exit: automation reads exit 0 as
    # "wired and sane"
    if flat:
        print(f"=== hardware check: FLAT channel(s) {', '.join(flat)} -- "
              f"check the wiring (exit 31) ===")
        return 31
    if railed:
        print(f"=== hardware check: channel(s) near rail "
              f"({', '.join(railed)}) -- raise --vrange or re-zero "
              f"(exit 30) ===")
        return 30
    print("=== hardware check passed ===")
    return 0


# ---------------------------------------------------------------- CLI

DEFAULT_AXES = "z,y"


def build_parser():
    p = argparse.ArgumentParser(
        allow_abbrev=False,
        description=__doc__.split("\n\n")[0],
        formatter_class=argparse.RawDescriptionHelpFormatter)
    bench = p.add_mutually_exclusive_group()
    bench.add_argument("--1head", dest="topology", action="store_const",
                       const="1head",
                       help="one head; --axes picks which axes (default)")
    bench.add_argument("--2head", dest="topology", action="store_const",
                       const="2head",
                       help="two dual-axis heads on four inputs (--map "
                            "axis:head:aiN); shared axes get a block-mean r")
    bench.add_argument("--4head", dest="topology", action="store_const",
                       const="4head",
                       help="four single-axis heads (--sensors) with signed "
                            "--gradient pairs in pT/m")
    p.set_defaults(topology="1head")

    p.add_argument("--axes", default=DEFAULT_AXES,
                   help=f"--1head only: axes to record (default "
                        f"{DEFAULT_AXES})")
    p.add_argument("--map", default=None,
                   help="--1head: axis:aiN list (default z:ai0,y:ai1,x:ai2). "
                        f"--2head: axis:head:aiN list (default "
                        f"{DEFAULT_TWO_HEAD_MAP}, the standing wiring); "
                        f"x2/y2/z2 re-read that axis on the other head")
    p.add_argument("--sensors", default=None,
                   help="--4head only: four channels, e.g. "
                        "s1:ai0,s2:ai1,s3:ai2,s4:ai3")
    p.add_argument("--gradient", action="append", default=[],
                   help="--4head only: pair and baseline in metres, e.g. "
                        "s1,s2:0.08 (repeatable)")
    p.add_argument("--aux", action="append", default=None, metavar="NAME:aiN",
                   help="--1head/--2head: extra raw-volt channel, e.g. "
                        "mon:ai3 (repeatable)")
    p.add_argument("--sensor-id", default=DEFAULT_SENSOR_ID,
                   help=f"--1head only: head serial for provenance (default "
                        f"{DEFAULT_SENSOR_ID})")
    p.add_argument("--ids", default=None,
                   help="--2head: a:SERIAL,b:SERIAL. --4head: "
                        "s1:SERIAL,...,s4:SERIAL (required on hardware)")

    p.add_argument("--fs", type=float, default=1200.0,
                   help="sample rate per channel in Hz (default 1200; "
                        "keeps the 923 Hz carrier alias off 77 Hz)")
    p.add_argument("--seconds", type=float, default=900.0,
                   help="duration in s (default 900); <=0 runs until Ctrl-C")
    p.add_argument("--vrange", default="2",
                   help="input range in V: 1,2,5,10,20 (default 2; use 1 "
                        "only for floor-verification runs)")
    p.add_argument("--gain", default="1x",
                   help="QZFM analog gain: 0.33x, 1x, 3x")
    p.add_argument("--device", default=None,
                   help="NI device name (auto if omitted)")
    p.add_argument("--outdir", default=None,
                   help="output dir (default result/newdata, newdata1, ...)")
    p.add_argument("--rail-limit", type=float, default=0.01,
                   help="max railed fraction before exit 30")
    p.add_argument("--rail-abort", action="store_true",
                   help="stop as soon as the rail limit is exceeded")
    p.add_argument("--no-rail-check", action="store_true",
                   help="disable the ADC and QZFM saturation checks only; "
                        "flat and pinned channels still exit 31")
    p.add_argument("--flat-eps", type=float, default=1e-4,
                   help="min peak-to-peak in V; below that -> exit 31")
    p.add_argument("--cal-v-per-nT", type=float, default=None,
                   help="measured gain in V/nT (overrides the table; applies "
                        "to every channel)")
    p.add_argument("--cal-factors", default=None, metavar="LIST",
                   help="Calibrate-step factors per recorded channel, e.g. "
                        "z:1.08,y:0.90 (recorded in meta, never applied to "
                        "the data)")
    p.add_argument("--notes", default=None,
                   help="free text stored in meta.json")
    p.add_argument("--stop-file", default=None, metavar="PATH",
                   dest="stop_file",
                   help="stop cleanly when this file appears (a parent's "
                        "graceful-stop request; no signal, same on Windows "
                        "and POSIX)")
    p.add_argument("--hardware-check", action="store_true",
                   dest="hardware_check",
                   help="read the wired channels live and report "
                        "rails/flat, no recording (needs the DAQ)")
    p.add_argument("--probe", action="store_true",
                   help="read 2 s, report DC and headroom, exit")
    p.add_argument("--sensor-info", default=None, dest="sensor_info",
                   help="json list of the physical sensors in this run "
                        "(set by the sensor session; stored in meta.json)")
    lab = p.add_argument_group("which lab")
    lab.add_argument("--lab-file", default=None, dest="lab_file",
                     help="json from the parent session: bench or "
                          "simulator, and the config it used")
    lab.add_argument("--sim", action="store_true",
                     help="direct use only: record from the simulated lab")
    lab.add_argument("--speed", type=float, default=1.0,
                     help="direct use with --sim: run the clock faster")
    lab.add_argument("--sim-dir", default=None, dest="sim_dir",
                     help="direct use with --sim: the simulated lab folder")
    lab.add_argument("--config", default=None,
                     help="direct use: a config file instead of "
                          "config/lab.toml")
    return p


def lab_for(args):
    """The Lab this recorder records from."""
    from bfield.lab import lab_from_job, open_lab
    if args.lab_file:
        with open(args.lab_file, encoding="utf-8") as fh:
            return lab_from_job(json.load(fh))
    from bfield.config import load_config
    return open_lab(load_config(args.config), sim=args.sim, speed=args.speed,
                    sim_dir=args.sim_dir)


def main(argv=None):
    # stdout is a pipe under the parent; without line buffering a
    # 900 s run shows nothing until it ends
    with contextlib.suppress(Exception):
        sys.stdout.reconfigure(line_buffering=True)
    robust_console()
    # a parent's CTRL_BREAK_EVENT must reach acquire()'s finally, or
    # the run ends with samples and no meta
    accept_windows_break()
    parser = build_parser()
    try:
        if argv is None and len(sys.argv) == 1:
            # no wizard (2026-08-14); a bare invocation must not start
            # a 900 s default record
            raise ConfigError("no arguments -- pass flags (see --help); "
                              "this recorder is normally spawned by "
                              "the sensor session")
        args = parser.parse_args(argv)
        args.lab = lab_for(args)
        if args.device is None:
            args.device = (args.lab.config.section("daq").get("device")
                           or None)
        if args.hardware_check and args.probe:
            raise ConfigError("pick one mode: --hardware-check or --probe")
        if args.hardware_check:
            return run_hardware_check(build_cfg(args))
        cfg = build_cfg(args)
        if args.probe:
            return run_probe(cfg)
        return acquire(cfg)
    except KeyboardInterrupt:
        print("\n[stop] interrupted before acquisition.", file=sys.stderr)
        return 130
    except AcqError as e:
        print(f"ERROR[{e.code}] {type(e).__name__}: {e}", file=sys.stderr)
        return e.code
    except Exception as e:
        print(f"ERROR[1] {type(e).__name__}: {e}", file=sys.stderr)
        traceback.print_exc()          # see the note in acquire()
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
