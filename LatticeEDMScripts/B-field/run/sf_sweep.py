#!/usr/bin/env python3
"""One S(f) shielding-factor sweep, conducted point by point at the bench.

Replaces IROP_research/scripts/sf_sweep.py; same modes, prompts, files
(<bucket>/sw_<stamp>_<orientation>/ with setup/, null_before/,
pNN_<f>Hz/, null_after/, sf_sweep_session.json, sf_sweep.log) and exit
codes: 0 done, 13 bad command line, 20 sweep stopped, 21 scope
unreadable, 40 a run left no readable meta.

  python run/sf_sweep.py --plan
  python run/sf_sweep.py --precheck
  python run/sf_sweep.py --go --orientation axial
  python run/sf_sweep.py --dc --orientation transverse

The generator is set by hand between records, so --precheck/--go/--dc
need a terminal. Head serials, fs, vrange and the scope come from
config/lab.toml. Degauss after a sweep: the drive magnetizes the walls.
"""

import _path  # noqa: F401

from bfield.cli import add_lab_args, make_parser, open_from_args, run_main
from bfield.errors import ConfigError
from bfield.procedures import sf_sweep as sf


def build_parser():
    p = make_parser(__doc__)
    p.add_argument("--plan", action="store_true",
                   help="print the sweep and the child commands, touch "
                        "nothing")
    p.add_argument("--precheck", action="store_true",
                   help="ONE short run to prove the wiring before the sweep")
    p.add_argument("--go", action="store_true", help="conduct the sweep")
    p.add_argument("--dc", action="store_true",
                   help="measure S at DC: a steady current reversed by hand "
                        f"in {''.join(sf.DC_PATTERN)} blocks")
    p.add_argument("--dc-ma", type=float, default=sf.DEFAULT_DC_MA)
    p.add_argument("--dc-dwell", type=float, default=sf.DEFAULT_DC_DWELL_S)
    p.add_argument("--dc-blocks", type=int, default=sf.DEFAULT_DC_BLOCKS)
    p.add_argument("--freqs", default=sf.DEFAULT_FREQS)
    p.add_argument("--control-hz", type=float, default=sf.DEFAULT_CONTROL_HZ)
    p.add_argument("--seed", type=int, default=None,
                   help="shuffle seed; omit for a fresh order (recorded)")
    p.add_argument("--amp-ma", type=float, default=sf.DEFAULT_AMP_MA,
                   help="TARGET drive amplitude 0-p in mA; S uses what the "
                        "scope read")
    p.add_argument("--rmon-ohm", type=float, default=sf.DEFAULT_RMON_OHM,
                   help="monitor resistor in ohms (the DMM reading)")
    p.add_argument("--min-cycles", type=float, default=sf.DEFAULT_MIN_CYCLES)
    p.add_argument("--min-seconds", type=float,
                   default=sf.DEFAULT_MIN_SECONDS)
    p.add_argument("--precheck-seconds", type=float, default=60.0)
    p.add_argument("--null-seconds", type=float,
                   default=sf.DEFAULT_NULL_SECONDS,
                   help="drive-off record at each end (0 skips them)")
    p.add_argument("--orientation", choices=sorted(sf.ORIENTATION_MAP),
                   default=sf.DEFAULT_ORIENTATION)
    p.add_argument("--map", default=None,
                   help="channel map for the recorder; unset = the one "
                        "--orientation implies")
    p.add_argument("--aux", default="",
                   help="coil-current monitor channel, e.g. mon:ai3 "
                        "(default none: the scope has it)")
    p.add_argument("--no-scope", dest="scope", action="store_false",
                   help="do NOT read the oscilloscope; take typed values")
    p.set_defaults(scope=True)
    p.add_argument("--scope-resource", default=None)
    p.add_argument("--scope-channel", default=None)
    p.add_argument("--ids", default=None,
                   help="the two sensors by serial, e.g. "
                        "A10QHMQMA,A10QHJ1ZA (default: the two "
                        "lowest-numbered sensors in the config)")
    p.add_argument("--sensors", default=None,
                   help="the two sensors by number, e.g. 1,2 (instead of "
                        "--ids); the first is the modulation master")
    p.add_argument("--fs", type=float, default=None)
    p.add_argument("--vrange", type=float, default=None)
    p.add_argument("--gain", default="1x")
    p.add_argument("--cal-v-per-nT", dest="cal_v_per_nt", type=float,
                   default=sf.DEFAULT_CAL_V_PER_NT)
    p.add_argument("--skip-setup", action="store_true",
                   help="the heads are already running and zeroed")
    p.add_argument("--device", default=None, help="NI device name")
    p.add_argument("--outdir", default=None,
                   help="the sweep folder (default: the next dated run "
                        "folder, e.g. <data_root>/Oct 2026/15/"
                        "004_SfSweepAxial)")
    p.add_argument("--name", default=None,
                   help="the run's name in its dated folder (default: "
                        "SfSweep / SfPrecheck / SfDC + the orientation)")
    add_lab_args(p)
    return p


def ids_from(args, config):
    if args.sensors and args.ids:
        raise ConfigError("give --sensors or --ids, not both")
    if not args.sensors:
        return args.ids
    from bfield.sensors import Selection, parse_numbers
    sel = Selection(config, parse_numbers(args.sensors), axes="z,y")
    if len(sel.sensors) != 2:
        raise ConfigError("the S(f) sweep reads exactly two sensors "
                          "(--sensors 1,2)")
    return sel.device_name


def map_from(args, config):
    """--sensors: the sensors' own DAQ inputs (unless --map / --aux)."""
    if not args.sensors or args.map or args.aux:
        return args.map
    from bfield.sensors import Selection, parse_numbers
    return Selection(config, parse_numbers(args.sensors), axes="z,y").map


def main(argv=None):
    args = build_parser().parse_args(argv)
    modes = [m for m in sf.MODES if getattr(args, m)]
    if len(modes) != 1:
        raise ConfigError("pass exactly one of --plan, --precheck, --go, --dc")
    ctx, lab = open_from_args(args)
    s = sf.SweepSettings.from_config(
        lab.config, mode=modes[0], dc_ma=args.dc_ma, dc_dwell=args.dc_dwell,
        dc_blocks=args.dc_blocks, freqs=args.freqs,
        control_hz=args.control_hz, seed=args.seed, amp_ma=args.amp_ma,
        rmon_ohm=args.rmon_ohm, min_cycles=args.min_cycles,
        min_seconds=args.min_seconds,
        precheck_seconds=args.precheck_seconds,
        null_seconds=args.null_seconds, orientation=args.orientation,
        map=map_from(args, lab.config), aux=args.aux, scope=args.scope,
        scope_resource=args.scope_resource,
        scope_channel=args.scope_channel, ids=ids_from(args, lab.config),
        fs=args.fs,
        vrange=args.vrange, gain=args.gain, cal_v_per_nt=args.cal_v_per_nt,
        skip_setup=args.skip_setup, device=args.device, outdir=args.outdir,
        name=args.name)
    return sf.SfSweep(ctx, lab, s).run()


if __name__ == "__main__":
    raise SystemExit(run_main(main))
