"""Direct parity against the legacy monolith, run when its heavy imports are available.

Skipped automatically in a headless environment (the legacy module imports tkinter,
seaborn and matplotlib at module level). Run it in Spyder before deleting anything
from YbF_spectroscopy_library.py.
"""

import importlib.util
from pathlib import Path

import numpy as np
import pytest

from ybf import constants, levels, transitions
from reference_grid import compute_grid

LEGACY = Path(__file__).resolve().parents[1] / 'YbF_spectroscopy_library.py'


def _load_legacy():
    for mod in ('tkinter', 'seaborn', 'matplotlib', 'pandas', 'scipy'):
        if importlib.util.find_spec(mod) is None:
            pytest.skip(f'{mod} unavailable in this environment')
    spec = importlib.util.spec_from_file_location('legacy_ybf_spectroscopy', LEGACY)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


@pytest.mark.slow
def test_package_matches_legacy_library():
    legacy = _load_legacy()
    ref = compute_grid(legacy, legacy, legacy)
    new = compute_grid(constants, levels, transitions)
    assert set(ref) == set(new)
    for label in sorted(ref):
        np.testing.assert_allclose(new[label], ref[label], rtol=1e-12, atol=0.0,
                                   equal_nan=True, err_msg=f'{label} differs from legacy')
