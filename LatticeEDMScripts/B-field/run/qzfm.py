#!/usr/bin/env python3
"""Run the QZFM heads: status, setup, zero, one record, stream, standby.

Replaces IROP_research/scripts/qzfm_serial.py; same modes, same exit
codes, same files (qzfm_session.json + qzfm_session.log, Rnn_HHMM/ run
folders, _guard/ records). Bench settings (serials, wiring, fs, vrange)
come from config/lab.toml, so most runs need only a mode:

  python run/qzfm.py --list-ports
  python run/qzfm.py --sensors-list
  python run/qzfm.py --status --sensors 1
  python run/qzfm.py --setup  --sensors 1,2 --outdir <data_root>/labcheck/today
  python run/qzfm.py --record --sensors 1,2 --seconds 900 --name Baseline
  python run/qzfm.py --standby --sensors 1,2

--sensors picks sensors by number from config/sensors.toml; each records
on its own DAQ inputs (z_input / y_input there; the program says what
goes where). The first is the modulation master unless --master says
otherwise. --axes picks which axes are recorded. --device-name
gives USB serials directly (the old flag). The COM port is exclusive: close
the QuSpin GUI first. Exit codes: 0 ok, 13 config or busy port, 20
stopped early, 40 data with no readable meta.json, 41 recorder still
holds the DAQ, 130 Ctrl-C; --record passes the recorder's code through.
"""

import _path  # noqa: F401

from bfield.cli import add_lab_args, make_parser, open_from_args, run_main
from bfield.devices.qzfm import list_serial_ports
from bfield.errors import ConfigError
from bfield.procedures.qzfm_session import QZFMPlan, QZFMSession, validate


def build_parser():
    p = make_parser(__doc__)
    m = p.add_argument_group("mode (pick one)")
    m.add_argument("--list-ports", action="store_true",
                   help="list serial ports with their serial numbers")
    m.add_argument("--sensors-list", action="store_true",
                   help="list the configured sensors (number, QuSpin label, "
                        "USB serial, location)")
    m.add_argument("--status", action="store_true",
                   help="print sensor state and exit (read-only)")
    m.add_argument("--setup", action="store_true",
                   help="bring up + zero + calibrate + guard record")
    m.add_argument("--zero", action="store_true",
                   help="re-zero + re-calibrate a warm sensor")
    m.add_argument("--record", "--run", dest="record", action="store_true",
                   help="setup, then one record")
    m.add_argument("--stream", type=float, default=None, metavar="SECONDS",
                   help="serial digital stream (200 S/s) to csv")
    m.add_argument("--standby", action="store_true",
                   help="reboot the sensor dark (laser + heater OFF)")
    p.add_argument("--sensors", default=None,
                   help="sensors by number from config [[sensors]], e.g. 1 "
                        "or 1,2")
    p.add_argument("--master", type=int, default=None,
                   help="with two or more --sensors: the number of the "
                        "modulation "
                        "master (default: the first)")
    p.add_argument("--device-name", default=None,
                   help="sensors by serial instead of number: "
                        "A10QHMQMA or A10QHMQMA,A10QHJ1ZA (looked up in the "
                        "sensor list, so the data are still filed by number)")
    p.add_argument("--axes", default=None,
                   help="z,y subset (default z for one sensor, every "
                        "wired axis for more)")
    p.add_argument("--map", default=None,
                   help="channel:head:aiN list (default: from the "
                        "chosen sensors' inputs in config/sensors.toml)")
    p.add_argument("--gain", default=None, help="0.33x, 1x, 3x")
    p.add_argument("--fs", type=float, default=None)
    p.add_argument("--vrange", default=None)
    p.add_argument("--cal-v-per-nT", type=float, default=None,
                   dest="cal_v_per_nT",
                   help="asserted V/nT; unset = nominal for the gain")
    p.add_argument("--seconds", type=float, default=900.0)
    p.add_argument("--zero-cond", type=float, default=None)
    p.add_argument("--lock-timeout", type=float, default=None)
    p.add_argument("--poll-s", type=float, default=None)
    p.add_argument("--outdir", default=None,
                   help="where the session goes (default: the next dated "
                        "run folder, e.g. <data_root>/Oct 2026/15/"
                        "001_SingleSensorZ)")
    p.add_argument("--name", default=None,
                   help="the run's name in its dated folder (default: "
                        "automatic, e.g. SingleSensorZ, TwoSensorsZY)")
    p.add_argument("--notes", default=None)
    p.add_argument("--recorder-arg", action="append", default=[],
                   dest="extra_args", metavar="ARG",
                   help="extra flag for the recorder child (repeatable)")
    p.add_argument("--require-factors", action="store_true")
    p.add_argument("--force-start", action="store_true")
    p.add_argument("--log", default=None)
    p.add_argument("--child-console", action="store_true")
    p.add_argument("--sync", default=None,
                   choices=("none", "master-slave", "independent"))
    p.add_argument("--zero-shift-warn", type=float, default=None)
    p.add_argument("--stream-sensor", type=int, default=None,
                   help="--stream with several --sensors: which one "
                        "(default the first)")
    p.add_argument("--validate-only", action="store_true",
                   help="check the command line and exit; opens nothing")
    add_lab_args(p)
    return p


def plan_from(args, config, **kw):
    """--sensors (by number) or --device-name (serials) -> QZFMPlan."""
    if args.device_name and args.sensors:
        raise ConfigError("give --sensors or --device-name, not both")
    if args.sensors:
        from bfield.sensors import parse_numbers
        return QZFMPlan.for_sensors(config, parse_numbers(args.sensors),
                                    master=args.master, **kw)
    if args.master is not None:
        raise ConfigError("--master goes with --sensors")
    return QZFMPlan.from_config(config, device_name=args.device_name or "",
                                **kw)


def main(argv=None):
    args = build_parser().parse_args(argv)
    modes = [m for m in ("list_ports", "sensors_list", "status", "setup",
                         "zero", "record", "standby") if getattr(args, m)]
    if args.stream is not None:
        modes.append("stream")
    if len(modes) != 1:
        raise ConfigError("pick exactly one mode: --list-ports/"
                          "--sensors-list/--status/--setup/--zero/--record/"
                          "--stream/--standby")
    mode = modes[0]
    if mode == "sensors_list":
        from bfield.config import load_config
        from bfield.sensors import input_sharing, load_sensors
        cfg = load_config(args.config)
        sensors = load_sensors(cfg)
        print(f"  {'NO':<4s}{'QUSPIN LABEL':<16s}{'USB SERIAL':<14s}"
              f"{'DAQ WIRING':<20s}LOCATION")
        for s in sensors:
            print(f"  {s.number:<4d}{s.label:<16s}{s.usb_serial:<14s}"
                  f"{s.wiring:<20s}{s.location or '(not set)'}")
        for note in input_sharing(sensors):
            print(f"  note: {note}")
        return 0
    if mode == "list_ports":
        ports = list_serial_ports()
        if not ports:
            print("[ports] none found -- is anything plugged in?")
            return 0
        print(f"[ports] {len(ports)} serial port(s); the SERIAL column is "
              "a sensor's usb_serial in config [[sensors]]")
        print(f"  {'PORT':<10s} {'SERIAL':<20s} DESCRIPTION")
        for dev, serial, desc in ports:
            print(f"  {dev:<10s} {serial:<20s} {desc}")
        return 0

    ctx, lab = open_from_args(args)
    plan = plan_from(
        args, lab.config,
        axes=args.axes, map=args.map, gain=args.gain, fs=args.fs,
        vrange=args.vrange, cal_v_per_nT=args.cal_v_per_nT,
        seconds=args.seconds, zero_cond=args.zero_cond,
        lock_timeout=args.lock_timeout, poll_s=args.poll_s,
        outdir=args.outdir, name=args.name, notes=args.notes, extra_args=args.extra_args,
        require_factors=args.require_factors, force_start=args.force_start,
        log=args.log, child_console=args.child_console, sync=args.sync,
        zero_shift_warn=args.zero_shift_warn,
        stream_head=(None if args.stream_sensor is None
                     else f"s{args.stream_sensor}"),
        stream_seconds=args.stream)
    old_mode = {"record": "run"}.get(mode, mode)
    if args.validate_only:
        validate(plan, old_mode)
        print("[ok] arguments accepted", flush=True)
        return 0
    session = QZFMSession(ctx, lab, plan)
    if mode == "status":
        from bfield.channels import key_number
        for head, sensor, s, blank in session.status():
            pre = f"[status sensor {key_number(head)}]"
            print(f"{pre} {'serial':<15s} {sensor.serial_id}"
                  + (f"  (on {sensor.port})" if sensor.port else ""))
            for label, key in (("laser on", "laser_on"),
                               ("cell temp lock", "cellT_lock"),
                               ("laser lock", "laser_lock"),
                               ("field zeroed", "zeroed")):
                print(f"{pre} {label:<15s} {'yes' if s[key] else 'NO'}")
            print(f"{pre} {'compensation':<15s} B0 {s['B0_pT']:.1f}  "
                  f"By {s['By_pT']:.1f}  Bz {s['Bz_pT']:.1f} pT")
            print(f"{pre} {'cell T error':<15s} {s['cellT_err']:g}")
            if blank:
                print(f"{pre} CAUTION: every flag off AND every number nan "
                      "-- contentless frame (one such head read fully locked "
                      "40 s later, 2026-08-06). Check again before "
                      "believing it")
        return 0
    return {"setup": session.setup, "zero": session.zero,
            "record": session.record, "stream": session.stream,
            "standby": session.standby}[mode]()


if __name__ == "__main__":
    raise SystemExit(run_main(main))
