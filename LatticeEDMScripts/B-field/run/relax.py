#!/usr/bin/env python3
"""Evening degauss-relaxation program: degauss, then timed records.

Replaces IROP_research/scripts/degauss_relax.py; same flags, same
schedule logic, same folder layout (<bucket>/rx_<stamp>/...), same exit
codes: 0 completed / 13 config / 20 aborted / 130 Ctrl-C / 1 crash.

  python run/relax.py --plan
  python run/relax.py --go
  python run/relax.py --go --at 20,40,60,80,100,120
  python run/relax.py --plan --sim            (simulated lab)

--go needs you at the bench twice: 19V OFF before the degauss, then
unplug the banana leads and 19V ON after it. The degausser port and the
sensor serial come from config/lab.toml unless given.
"""

import sys

import _path  # noqa: F401

from bfield.cli import add_lab_args, make_parser, open_from_args, run_main
from bfield.errors import ConfigError
from bfield.procedures.relaxation import (DEFAULT_AT_MIN, DEFAULT_CHECKOUT_S,
                                          DEFAULT_SECONDS,
                                          DEFAULT_SLEEP_GAP_MIN,
                                          DEFAULT_SOAK_S,
                                          DEFAULT_STRONG_AMP_A,
                                          DEFAULT_WAKE_LEAD_MIN,
                                          DEFAULT_WINDING, WINDING_DEFAULTS,
                                          RelaxSettings, run)


def build_parser():
    p = make_parser(__doc__)
    p.add_argument("--plan", action="store_true",
                   help="print the schedule and the steps, touch nothing")
    p.add_argument("--go", action="store_true",
                   help="execute the evening program")
    p.add_argument("--dg-port", default=None,
                   help="DG serial port (default: config [degausser] port)")
    p.add_argument("--device-name", default=None,
                   help="sensor serial(s), e.g. A10QHMQMA (looked up in "
                        "the sensor list; default: the lowest-numbered "
                        "sensor)")
    p.add_argument("--sensors", default=None,
                   help="sensors by number from config [[sensors]]: 1 or "
                        "1,2 (the first is the modulation master)")
    p.add_argument("--winding", choices=sorted(WINDING_DEFAULTS),
                   default=DEFAULT_WINDING,
                   help="winding on the shield; sets the amp/freq/taper/"
                        "coil defaults")
    p.add_argument("--amp", type=float, default=None)
    p.add_argument("--freq", type=float, default=None)
    p.add_argument("--strong", action="store_true",
                   help="series windings only: 5 Hz pass then 3 Hz finish")
    p.add_argument("--strong-amp", type=float, default=DEFAULT_STRONG_AMP_A)
    p.add_argument("--soak", type=float, default=DEFAULT_SOAK_S)
    p.add_argument("--taper", type=float, default=None)
    p.add_argument("--coil", default=None)
    p.add_argument("--at", default=DEFAULT_AT_MIN,
                   help=f"point start times, minutes after t0 (default "
                        f"{DEFAULT_AT_MIN})")
    p.add_argument("--every", type=float, default=None,
                   help="fixed-interval schedule instead, minutes")
    p.add_argument("--span", type=float, default=12.0,
                   help="hours --every runs for (default 12)")
    p.add_argument("--checkout-seconds", type=float,
                   default=DEFAULT_CHECKOUT_S,
                   help="post-degauss health read in s (0 skips it)")
    p.add_argument("--seconds", type=float, default=DEFAULT_SECONDS,
                   help="record length per point (default 900)")
    p.add_argument("--sleep-between", type=float,
                   default=DEFAULT_SLEEP_GAP_MIN, metavar="MIN")
    p.add_argument("--no-sleep", action="store_true",
                   help="never standby between points")
    p.add_argument("--wake-lead", type=float, default=DEFAULT_WAKE_LEAD_MIN,
                   metavar="MIN")
    p.add_argument("--axes", default="z")
    p.add_argument("--map", default=None,
                   help="channel:input list, e.g. s1z:ai0 (default: the "
                        "sensors' own inputs)")
    p.add_argument("--gain", default="1x")
    p.add_argument("--fs", type=float, default=1200.0)
    p.add_argument("--vrange", default="2")
    p.add_argument("--outdir", default=None,
                   help="parent directory (default: the next dated run "
                        "folder, e.g. <data_root>/Oct 2026/15/"
                        "003_Relaxation)")
    p.add_argument("--name", default=None,
                   help="the run's name in its dated folder (default: "
                        "Relaxation)")
    p.add_argument("--notes", default=None)
    p.add_argument("--confirm", action="store_true",
                   help="skip the pre-degauss checklist, asserting 19V OFF")
    p.add_argument("--leads-stay", action="store_true")
    p.add_argument("--sensor-powered", action="store_true")
    p.add_argument("--allow-missing-factors", action="store_true")
    p.add_argument("--accept-flagged-degauss", action="store_true")
    p.add_argument("--accept-unverified-standby", action="store_true")
    add_lab_args(p, yes_help="answer yes to every question (the checklist "
                             "included); hand steps still need a terminal "
                             "unless --leads-stay --sensor-powered")
    return p


def sensors_from(args, config):
    """(device name, channel map): --sensors uses each sensor's own DAQ
    inputs (config/sensors.toml); --device-name keeps --map / the
    default map."""
    if args.device_name and args.sensors:
        raise ConfigError("give --sensors or --device-name, not both")
    if not args.sensors:
        return args.device_name, args.map
    from bfield.sensors import Selection, parse_numbers
    sel = Selection(config, parse_numbers(args.sensors), axes=args.axes)
    return sel.device_name, args.map or sel.map


def main(argv=None):
    if argv is None and len(sys.argv) == 1:
        raise ConfigError("no arguments -- pass --plan to see the schedule, "
                          "or --go to run the evening (see --help)")
    args = build_parser().parse_args(argv)
    if args.plan == args.go:
        raise ConfigError("pick exactly one mode: --plan or --go")
    # 19V is thrown by hand twice, so --go cannot start detached
    if args.go and not args.yes and not (sys.stdin.isatty()
                                         and sys.stdout.isatty()):
        raise ConfigError(
            "--go needs a terminal: the QZFM 19V switch is thrown by hand "
            "before the degauss and again before the first point -- run it "
            "attached (the run is logged to <bucket>/relax_session.log)")
    ctx, lab = open_from_args(args)
    device_name, chan_map = sensors_from(args, lab.config)
    s = RelaxSettings(
        dg_port=args.dg_port, device_name=device_name,
        winding=args.winding, amp=args.amp, freq=args.freq,
        strong=args.strong, strong_amp=args.strong_amp, soak=args.soak,
        taper=args.taper, coil=args.coil, at=args.at, every=args.every,
        span=args.span, checkout_seconds=args.checkout_seconds,
        seconds=args.seconds, sleep_between=args.sleep_between,
        no_sleep=args.no_sleep, wake_lead=args.wake_lead, axes=args.axes,
        map=chan_map, gain=args.gain, fs=args.fs, vrange=args.vrange,
        outdir=args.outdir, name=args.name, notes=args.notes,
        confirm=args.confirm,
        leads_stay=args.leads_stay, sensor_powered=args.sensor_powered,
        allow_missing_factors=args.allow_missing_factors,
        accept_flagged_degauss=args.accept_flagged_degauss,
        accept_unverified_standby=args.accept_unverified_standby)
    return run(ctx, lab, s, "plan" if args.plan else "go")


if __name__ == "__main__":
    raise SystemExit(run_main(main))
