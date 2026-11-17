"""analysis_scripts/_paths.py maps the old "result/..." spellings onto
DATA_ROOT, and every ported script still compiles."""

import glob
import os
import py_compile
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
SCRIPTS = os.path.join(os.path.dirname(HERE), "analysis_scripts")


def test_rel_data_maps_result_prefix(tmp_path):
    code = ("import _paths, os; "
            "print(_paths.DATA_ROOT); "
            "print(_paths.rel_data('result/synctest/a.txt')); "
            "print(_paths.data_join('result', 'modetest', 'b.txt')); "
            "print(_paths.rel_data('synctest/c.txt'))")
    env = dict(os.environ, BFIELD_DATA_ROOT=str(tmp_path))
    out = subprocess.run([sys.executable, "-c", code], cwd=SCRIPTS, env=env,
                         capture_output=True, text=True, check=True).stdout
    root, a, b, c = out.strip().splitlines()
    assert root == os.path.normpath(str(tmp_path))
    assert a == os.path.join(root, "synctest", "a.txt")
    assert b == os.path.join(root, "modetest", "b.txt")
    assert c == os.path.join(root, "synctest", "c.txt")


def test_every_ported_script_compiles(tmp_path):
    files = glob.glob(os.path.join(SCRIPTS, "**", "*.py"), recursive=True)
    assert len(files) >= 45
    for f in files:
        py_compile.compile(f, cfile=str(tmp_path / "x.pyc"), doraise=True)
