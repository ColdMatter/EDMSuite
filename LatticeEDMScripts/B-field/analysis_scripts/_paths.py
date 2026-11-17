"""Where the analysis scripts find the data.

Every script in analysis_scripts/ imports this first. It puts the B-field
project on sys.path (so `import bfield` works) and exposes DATA_ROOT, the
folder that holds the run buckets (gui_baseline/, synctest/, ...). It
comes from config/lab.toml [paths] data_root (config/lab.local.toml
overrides it); the environment variable BFIELD_DATA_ROOT overrides both.

The scripts still spell their inputs as "result/<bucket>/..." -- the
layout of the old IROP_research tree. rel_data() maps that onto
DATA_ROOT, so the literal paths stay readable and greppable.
"""

import os
import sys

ANALYSIS_DIR = os.path.dirname(os.path.abspath(__file__))
PROJECT_DIR = os.path.dirname(ANALYSIS_DIR)
if PROJECT_DIR not in sys.path:
    sys.path.insert(0, PROJECT_DIR)


def _data_root():
    env = os.environ.get("BFIELD_DATA_ROOT")
    if env:
        return os.path.normpath(env)
    from bfield.config import load_config
    return load_config().data_root


DATA_ROOT = _data_root()


def data(*parts):
    """DATA_ROOT/<parts...>, e.g. data("synctest", "sync5_30min")."""
    return os.path.join(DATA_ROOT, *parts)


def rel_data(rel):
    """Map an old-tree path "result/<bucket>/..." onto DATA_ROOT.

    A path without the "result/" prefix is taken as already relative to
    DATA_ROOT; an absolute path is returned unchanged.
    """
    if os.path.isabs(rel):
        return rel
    norm = rel.replace("\\", "/")
    if norm == "result":
        return DATA_ROOT
    if norm.startswith("result/"):
        norm = norm[len("result/"):]
    return os.path.join(DATA_ROOT, *norm.split("/"))


def data_join(*parts):
    """os.path.join(REPO, *parts) of the old scripts, with the leading
    "result" (or "result/...") mapped onto DATA_ROOT."""
    return rel_data(os.path.join(*parts).replace("\\", "/"))
