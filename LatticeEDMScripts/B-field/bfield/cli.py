"""What every run/*.py script shares: the lab flags, the console UI,
and turning an exception into the exit code the old scripts used.

This is the ONLY layer that prints to the terminal directly or exits.
"""

from __future__ import annotations

import argparse
import sys
import traceback

from bfield.config import load_config
from bfield.errors import BFieldError
from bfield.lab import open_lab
from bfield.ui import ConsoleUI, Context, robust_console


def add_lab_args(p, *, yes_help=None):
    g = p.add_argument_group("which lab")
    g.add_argument("--sim", action="store_true",
                   help="run in the simulated lab (no hardware is touched; "
                        "data goes to B-field/sim_data)")
    g.add_argument("--speed", type=float, default=1.0,
                   help="with --sim: run the clock this many times faster")
    g.add_argument("--sim-dir", default=None,
                   help="with --sim: where the simulated lab keeps its "
                        "world and data (default B-field/sim_data)")
    g.add_argument("--config", default=None,
                   help="config file instead of config/lab.toml")
    if yes_help:
        g.add_argument("--yes", action="store_true", help=yes_help)
    return g


def make_parser(doc):
    return argparse.ArgumentParser(
        allow_abbrev=False, description=(doc or "").split("\n\n")[0],
        epilog=None, formatter_class=argparse.RawDescriptionHelpFormatter)


def open_from_args(args):
    """(ctx, lab) for a parsed command line."""
    robust_console()
    cfg = load_config(args.config)
    if args.speed != 1.0 and not args.sim:
        raise SystemExit("--speed only works with --sim")
    lab = open_lab(cfg, sim=args.sim, speed=args.speed,
                   sim_dir=getattr(args, "sim_dir", None))
    ctx = Context(ConsoleUI(assume_yes=getattr(args, "yes", False)),
                  config=cfg)
    if lab.sim:
        ctx.say("lab", f"SIMULATED LAB (speed x{lab.speed:g}) -- data in "
                       f"{lab.data_root}")
    return ctx, lab


def run_main(fn, argv=None):
    """Call fn(argv) -> exit code; exceptions become the old exit codes."""
    try:
        return fn(argv)
    except KeyboardInterrupt:
        print("\n[stop] interrupted.", file=sys.stderr)
        return 130
    except BFieldError as e:
        print(f"ERROR[{e.code}] {type(e).__name__}: {e}", file=sys.stderr)
        return e.code
    except Exception as e:
        print(f"ERROR[1] {type(e).__name__}: {e}", file=sys.stderr)
        traceback.print_exc()
        return 1
