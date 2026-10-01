"""Unit conversions shared by the whole package.

Extracted verbatim from YbF_spectroscopy_library.py on 2026-09-16 — the physics code is byte-identical to the
version that produced the group's published assignments. Do not retype values here;
edit and re-run Tests/test_levels_regression.py if a constant genuinely changes.
"""

c = 29.9792458
"""GHz per cm^-1. Multiply a cm^-1 value by `c` to get GHz."""

hck = 6.626 * 1.38065 * 0.299792458
"""h*c/k_B scaling used in the Boltzmann factors of the stick-spectrum code."""

CM_INV_TO_THZ = 29979245800.0 / 1e12
"""THz per cm^-1."""
