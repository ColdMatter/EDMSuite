"""Every pinned level energy and transition frequency must be reproduced exactly.

This is the safety net for refactoring: it fails loudly if a move, a rename or a
units "tidy-up" changes a number. Golden values were minted from
YbF_spectroscopy_library.py (see Tests/data/golden_levels.json).
"""

import json
from pathlib import Path

import numpy as np
import pytest

from ybf import constants, levels, transitions
from reference_grid import compute_grid

GOLDEN = json.loads((Path(__file__).parent / 'data' / 'golden_levels.json').read_text())['values']
CURRENT = compute_grid(constants, levels, transitions)


def test_no_pinned_quantity_disappeared():
    assert set(GOLDEN) - set(CURRENT) == set(), 'pinned quantities no longer computed'


@pytest.mark.parametrize('label', sorted(GOLDEN))
def test_matches_golden(label):
    np.testing.assert_allclose(CURRENT[label], GOLDEN[label],
                               rtol=1e-12, atol=0.0, equal_nan=True,
                               err_msg=f'{label} changed')
