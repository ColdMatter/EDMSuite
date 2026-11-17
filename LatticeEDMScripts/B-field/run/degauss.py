#!/usr/bin/env python3
r"""Twinleaf DG degausser: status, stepped saturation check, one cycle.

Replaces IROP_research/scripts/degauss.py; same modes, same checks,
same files (degauss_session.json + stream CSVs, in the dated run folder
or <outdir>/dg_<stamp>/),
same exit codes: 0 verified pass / 30 QC refused or stream missing
(walls NOT trusted) / 10 connect / 13 config / 32 readback refused the
trigger (nothing driven) / 130 Ctrl-C / 1 crash.

  python run/degauss.py --status
  python run/degauss.py --check --amps "1,1.5,2,2.5"
  python run/degauss.py --run --amp 8 --coil parallel4
  python run/degauss.py --run --amp 8 --coil parallel4 --plot
  python run/degauss.py --replay <run folder>\stream_c01.csv [more.csv ...]

Every cycle also saves stream_cNN.png beside its stream_cNN.csv: current
and voltage against time and the I-V loop, as in Twinleaf's DG Control
VI. --plot shows the same live while the cycle runs (needs the [gui]
extra; in --check every amplitude's loop stays on the plot) and keeps
the window up until you close it. --replay opens saved streams in that
window, loops overlaid; it touches no hardware.

Safety: QZFM 19V power switch OFF before --check / --run (you are asked
unless --confirm-checklist); Ctrl-C sends degauss.abort; unplug the
banana leads after a cycle. The port comes from config [degausser].
"""

import _path  # noqa: F401

from bfield.cli import add_lab_args, make_parser, open_from_args, run_main
from bfield.errors import ConfigError
from bfield.operations.degauss_cycle import (DG_FREQ_DEFAULT_HZ,
                                             DG_SOAK_DEFAULT_S,
                                             DG_TAPER_DEFAULT_S,
                                             DegaussSession, DegaussSettings,
                                             parse_amps)


def build_parser():
    p = make_parser(__doc__)
    p.add_argument("--status", action="store_true",
                   help="dump identity, RPC list and waveform settings")
    p.add_argument("--check", action="store_true",
                   help="stepped short cycles for the saturation search")
    p.add_argument("--run", action="store_true",
                   help="one full degauss cycle")
    p.add_argument("--port", default=None,
                   help="serial port (default: config [degausser] port)")
    p.add_argument("--amp", type=float, default=None,
                   help="waveform current amplitude in A (--run)")
    p.add_argument("--amps", default=None,
                   help="comma list of increasing amplitudes in A (--check)")
    p.add_argument("--freq", type=float, default=DG_FREQ_DEFAULT_HZ)
    p.add_argument("--soak", type=float, default=DG_SOAK_DEFAULT_S)
    p.add_argument("--taper", type=float, default=DG_TAPER_DEFAULT_S)
    p.add_argument("--max-amp", type=float, default=None,
                   help="refuse amplitudes above this (default: config "
                        "[degausser] max_amp_A)")
    p.add_argument("--outdir", default=None,
                   help="parent for dg_<stamp>/ (default: the next "
                        "dated run folder, e.g. <data_root>/Oct 2026/15/"
                        "002_Degauss); --status records only when given")
    p.add_argument("--name", default=None,
                   help="the run's name in its dated folder (default: "
                        "Degauss, or DegaussCheck for --check)")
    p.add_argument("--coil", default=None,
                   help="label of the connected winding, e.g. parallel4 "
                        "(required by --run)")
    p.add_argument("--confirm-checklist", action="store_true",
                   help="assert the safety checklist (19V OFF, endcaps "
                        "closed) instead of being asked")
    p.add_argument("--decimation", type=int, default=None,
                   help="diagnostic only: set degauss.data.decimation (the "
                        "waveform stream's row divider) for this --check / "
                        "--run; the old value is put back afterwards. Leave "
                        "the unit at 8 for normal runs: 16 gives only 10 "
                        "samples per period at 20 Hz, too few for the "
                        "waveform (2026-11-04 run 4)")
    p.add_argument("--plot", action="store_true",
                   help="--check / --run: live window with current, voltage "
                        "and the I-V loop; stays up until closed")
    p.add_argument("--replay", nargs="+", default=None, metavar="CSV",
                   help="show saved stream_cNN.csv files in the plot window "
                        "(no hardware)")
    add_lab_args(p)
    return p


def open_live_window():
    try:
        from bfield.gui.dg_live import DgLiveWindow
    except ImportError as e:
        raise ConfigError(f"--plot needs the GUI packages ({e}); install "
                          f"them with: .venv\\Scripts\\python -m pip "
                          f"install -e \".[gui]\"")
    return DgLiveWindow("Degauss -- live")


def main(argv=None):
    args = build_parser().parse_args(argv)
    if args.replay:
        try:
            from bfield.gui.dg_live import replay
        except ImportError as e:
            raise ConfigError(f"--replay needs the GUI packages ({e})")
        replay(args.replay, say=lambda tag, msg: print(f"[{tag}] {msg}"))
        return 0
    picked = [m for m in ("status", "check", "run") if getattr(args, m)]
    if len(picked) != 1:
        raise ConfigError("pick exactly one mode: --status, --check or --run"
                          " (or --replay)")
    if args.plot and picked[0] == "status":
        raise ConfigError("--plot goes with --check or --run")
    ctx, lab = open_from_args(args)
    dcfg = lab.config.section("degausser")
    s = DegaussSettings(
        mode=picked[0], port=args.port or dcfg.get("port"), amp=args.amp,
        amps=None if args.amps is None else parse_amps(args.amps),
        freq=args.freq, soak=args.soak, taper=args.taper,
        max_amp=(args.max_amp if args.max_amp is not None
                 else float(dcfg.get("max_amp_A", 8.0))),
        outdir=args.outdir, name=args.name, coil=args.coil,
        confirm_checklist=args.confirm_checklist,
        decimation=args.decimation)
    live = open_live_window() if args.plot else None
    sess = DegaussSession(ctx, lab, s, live=live)
    code = sess.run()
    if live is not None and sess.live is not None:
        # the cycle is over and its files are written; the window is
        # only for looking now
        if live.loops:
            live.hold(say=ctx.say)
        else:
            live.close()
    return code


if __name__ == "__main__":
    raise SystemExit(run_main(main))
