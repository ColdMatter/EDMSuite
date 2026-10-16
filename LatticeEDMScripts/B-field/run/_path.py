"""Put B-field/ on sys.path so `python run/<script>.py` works without
installing the package (pip install -e . also works)."""
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
PROJECT = os.path.dirname(HERE)
if PROJECT not in sys.path:
    sys.path.insert(0, PROJECT)
