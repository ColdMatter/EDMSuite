#!/usr/bin/env python3
"""The recorder on its own: a bare-DAQ record, a probe, a hardware check.

Replaces direct use of IROP_research/scripts/qzfm_record.py (same flags,
same files, same exit codes). Normally the recorder is started for you
by run/qzfm.py --record; use this for an S(f) point, a floor check
with the sensor off, --probe (2 s: DC and headroom) or
--hardware-check (reads the wired channels, records nothing).

  python run/record.py --probe --2head
  python run/record.py --1head --axes z --seconds 60 --outdir ...
  python run/record.py --2head --sim --speed 10 --seconds 30   (simulated)
"""

import _path  # noqa: F401

from bfield.acquisition.recorder import main

if __name__ == "__main__":
    raise SystemExit(main())
