#!/usr/bin/env python3
"""Twinleaf DG degausser: status, stepped saturation check, one cycle.

Replaces IROP_research/scripts/degauss.py; same modes, same checks,
same files (<bucket>/dg_<stamp>/degauss_session.json + stream CSVs),
same exit codes: 0 verified pass / 30 QC refused or stream missing
(walls NOT trusted) / 10 connect / 13 config / 32 readback refused the
trigger (nothing driven) / 130 Ctrl-C / 1 crash.

  python run/degauss.py --status
  python run/degauss.py --check --amps "1,1.5,2,2.5"
  python run/degauss.py --run --amp 8 --coil parallel4

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
                   help="parent for dg_<stamp>/ (default: the degauss "
                        "bucket); --status records only when given")
    p.add_argument("--coil", default=None,
                   help="label of the connected winding, e.g. parallel4 "
                        "(required by --run)")
    p.add_argument("--confirm-checklist", action="store_true",
                   help="assert the safety checklist (19V OFF, endcaps "
                        "closed) instead of being asked")
    add_lab_args(p)
    return p


def main(argv=None):
    args = build_parser().parse_args(argv)
    picked = [m for m in ("status", "check", "run") if getattr(args, m)]
    if len(picked) != 1:
        raise ConfigError("pick exactly one mode: --status, --check or --run")
    ctx, lab = open_from_args(args)
    dcfg = lab.config.section("degausser")
    s = DegaussSettings(
        mode=picked[0], port=args.port or dcfg.get("port"), amp=args.amp,
        amps=None if args.amps is None else parse_amps(args.amps),
        freq=args.freq, soak=args.soak, taper=args.taper,
        max_amp=(args.max_amp if args.max_amp is not None
                 else float(dcfg.get("max_amp_A", 8.0))),
        outdir=args.outdir, coil=args.coil,
        confirm_checklist=args.confirm_checklist)
    return DegaussSession(ctx, lab, s).run()


if __name__ == "__main__":
    raise SystemExit(run_main(main))
