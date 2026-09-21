"""Prints branch-frequency tables for the 4f branch functions (Pff/Qfe/Rff/
Pee/Qef/Ree), pivoted so branches are columns and J'' is the row index - the
display style tuned in "Bulk analysis scripts/4f spectroscopy.py".

Reuses the exact scenarios pinned by reference_grid.py / test_levels_regression.py,
so what prints here is exactly what the regression tests are protecting -
useful for eyeballing predicted frequencies and cross-checking against saved
or measured values.

Not a pytest test - run directly: python branch_frequency_tables.py
"""
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from reference_grid import get_4f_branch_scenarios

from ybf import constants, transitions, spectra

J_START, J_END = 0.5, 9.5
TEMP_K = 4  # only affects the (unused here) Population column

OUTPUT_DIR = Path(__file__).resolve().parent / 'data' / 'branch_tables'


if __name__ == "__main__":
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    ground_4f, scenarios = get_4f_branch_scenarios(constants)

    for scenario_name, dict_name, excited_4f in scenarios:
        branch_dict = getattr(transitions, dict_name)
        df = spectra.GetSpectra_J(J_START, J_END, branch_dict, excited_4f, ground_4f, TEMP_K)
        table = spectra.branch_table(df)  # both cm-1 and THz, side by side per branch

        print(f"\n=== {scenario_name} (cm-1 and THz) ===")
        print(table)

        csv_path = OUTPUT_DIR / f"{scenario_name.replace('->', '_to_')}.csv"
        table.to_csv(csv_path)
        print(f"Saved to {csv_path}")
