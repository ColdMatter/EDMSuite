"""Instrument facts that must be identical everywhere.

Moved unchanged from IROP_research/scripts/qzfm_common.py and the
per-program constant blocks. Bench settings that change (ports, serials,
wiring) are NOT here -- they live in config/lab.toml.
"""

from __future__ import annotations

import math

# stamped into every meta.json / session json ("common_version"); the
# old suite wrote "1 (2026-08-14)". Analyses may gate on it, so it names
# the rewrite explicitly.
COMMON_VERSION = "bfield 0.1 (from qzfm_common 1, 2026-08-14)"

# ---------------------------------------------------------------- scale

GAIN_TABLE = {"0.33x": 0.9, "1x": 2.7, "3x": 8.1}  # V/nT, QuSpin nominal
# Measured at 1x on MEASURED_GAIN_HEAD (50 Hz tone vs its GUI stream).
# Never a default: the operator asserts it; recorder warns on another head.
WIZARD_GAIN_V_PER_NT = 2.624

CAL_FACTOR_MIN = 0.1         # Calibrate-step factor outside this window is a typo
CAL_FACTOR_MAX = 3.0
CAL_FACTOR_WARN = 1.5        # QuSpin health rule: above this the cell may need service

# ------------------------------------- QZFM analog output & modulation

QZFM_RAIL_V = 5.0            # the sensor's own analog output limit
QZFM_SOFT_RAIL_V = 4.25      # measured soft rail +4.27/-4.32 V (07-31); the
                             # spec-derived 0.98*5.0 = 4.90 V never fires
QZFM_RAIL_THRESH_V = QZFM_SOFT_RAIL_V
PINNED_DOMINANT_FRAC = 0.99  # one ADC code owning a block = pinned output
PINNED_BLOCK_LIMIT = 0.5     # flag when most blocks are pinned
RAIL_LEVEL = 0.99            # ADC clipping is crisp, so a tighter threshold
QZFM_LINEAR_NT = 1.0         # quasi-linear closed-loop window (+/-)

# Do not merge: QZFM_MOD_HZ 923.0 = spec, used to pick fs; MOD_CARRIER_HZ
# 922.9 = measured (drifts, 926.5-927.25 later), un-aliases recorded data.
QZFM_MOD_HZ = 923.0
MOD_CARRIER_HZ = 922.9
LF_BAND_HZ = 160.0           # the band the recorders must keep clean

VALID_AXES = ("x", "y", "z")
RECORDABLE_AXES = ("z", "y")   # the Dual unit has no x measurement mode
HEADS = ("a", "b")

# ------------------------------------------------- housekeeping contract
# The sensor session writes house*.csv. Threshold and column order live
# here so a reader cannot disagree with the session that wrote the file.

COMP_ABORT_PT = 45_000.0     # near the coils' +/-50 nT range: abort
                             # rather than record a saturating head
COMP_ABORT_NT = COMP_ABORT_PT / 1000.0
HOUSE_COLUMNS = ("time_iso", "elapsed_s", "B0_pT", "By_pT", "Bz_pT",
                 "cellT_err", "leds")
HOUSE_HEADER = ",".join(HOUSE_COLUMNS)
HOUSE_LED_KEYS = ("laser_on", "cellT_lock", "laser_lock", "zeroed")

# ------------------------------------------------- degausser cycle time

DG_MEASURE_PAD_S = 30.0      # measure+ramp+cooldown, 12.6-23.6 s measured;
                             # a shorter pad fires completion during cooldown
# Vendor recipe for the 4-group parallel winding: 8 A / 15 Hz / 300 s
DG_RECIPE_AMP_A = 8.0        # also the unit's own firmware cap (08-12/13)
DG_RECIPE_FREQ_HZ = 15.0
DG_TAPER_FALLBACK_S = 60.0   # taper shown to complete at 8 A; 300 s cut
                             # near 46 s x4 on 08-14 (open with Twinleaf)


def dg_cycle_plan_s(soak_s, taper_s):
    """Nominal wall time of one triggered degauss cycle, in seconds."""
    return DG_MEASURE_PAD_S + soak_s + taper_s


def mod_alias_hz(fs, harmonic=1):
    """Where the (harmonic-th) modulation line folds to at rate fs."""
    r = math.fmod(harmonic * QZFM_MOD_HZ, fs)
    return min(r, fs - r)


# ------------------------------------------------------------------ DAQ
# The card's inputs, ranges and rate limit depend on the model
# ([daq] model in config/lab.toml): see bfield.daq_models.

PROBE_HEADROOM = 1.5           # a probe peak must fit a range with margin
MAP_AXIS_RX = r"[xyz]2?"       # a map channel name: axis, or axis+'2'
