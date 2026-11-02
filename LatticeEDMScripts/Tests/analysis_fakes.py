"""Synthetic scans for testing the shared processing functions of
LatticeEDM_analysis_library and the analysis GUI without data files."""

import numpy as np

import LatticeEDM_analysis_library as EDM

RATE = 100000      # samples/s, as ScanMaster
T0_INDEX = 200     # first TOF sample (2 ms)
N_TIME = 8000      # samples per TOF (2 ms to 82 ms)


class FakeSettings(dict):
    """ScanSettings: missing keys give None, as the .NET hashtable"""

    def __getitem__(self, key):
        return self.get(key)


class FakeScan:
    def __init__(self, settings):
        self.ScanSettings = FakeSettings(settings)


def scan_settings(points, shots, extra=None):
    raw = {"out:pointsPerScan": points, "out:shotsPerPoint": shots, "shot:sampleRate": RATE,
           "out:start": 0., "out:end": 5000.,
           "pg:pluginName": "ScanMaster.Acquire.Plugins.LatticePumpProbePatternPlugin",
           "out:pluginName": "ScanMaster.Acquire.Plugins.TCLOutputPlugin"}
    raw.update(extra or {})
    return EDM.GetScanSettings(FakeScan(raw)), raw


def time_axis():
    return (np.arange(N_TIME) + T0_INDEX)/RATE


def pulse(amplitude, centre_ms=25., width_ms=3., baseline=0.5):
    """A molecule pulse on a flat background, [time]"""
    t = time_axis()*1000
    return baseline + amplitude*np.exp(-(t - centre_ms)**2/(2*width_ms**2))


def make_arrays(on, off, scan_params, wm=None, extra=None):
    """Arrays as EDM.ExtractScanArrays returns them, from TOFs
    [detector, point, time, shot]"""
    points, shots = on.shape[1], on.shape[3]
    settings, _ = scan_settings(points, shots, extra)
    t = time_axis()
    return {"settings": settings, "scan params": np.asarray(scan_params, dtype=float),
            "wm THz": None if wm is None else np.asarray(wm, dtype=float),
            "time on": t, "on": on, "time off": None if off is None else t, "off": off}


def onoff_scan(on_amps, off_amps, scan_params, shots=2, detectors=1, noise=0.0, seed=0, wm=None, extra=None):
    """Scan with On/Off pulses of the given amplitude at each point"""
    rng = np.random.default_rng(seed)
    P = len(on_amps)
    on = np.empty((detectors, P, N_TIME, shots))
    off = np.empty((detectors, P, N_TIME, shots))
    for p in range(P):
        for j in range(shots):
            for d in range(detectors):
                on[d, p, :, j] = pulse(on_amps[p]) + noise*rng.standard_normal(N_TIME)
                off[d, p, :, j] = pulse(off_amps[p]) + noise*rng.standard_normal(N_TIME)
    return make_arrays(on, off, scan_params, wm, extra)


def four_shot_scan(on_amps, off_amps, scan_params, shots=2, yag_off_level=0.5, noise=0.0, seed=0, extra=None):
    """4-shot scan: detector 0 = YAG On (pulse), detector 1 = YAG Off (background only)"""
    rng = np.random.default_rng(seed)
    P = len(on_amps)
    on = np.empty((2, P, N_TIME, shots))
    off = np.empty((2, P, N_TIME, shots))
    for p in range(P):
        for j in range(shots):
            on[0, p, :, j] = pulse(on_amps[p], baseline=yag_off_level) + noise*rng.standard_normal(N_TIME)
            off[0, p, :, j] = pulse(off_amps[p], baseline=yag_off_level) + noise*rng.standard_normal(N_TIME)
            on[1, p, :, j] = yag_off_level + noise*rng.standard_normal(N_TIME)
            off[1, p, :, j] = yag_off_level + noise*rng.standard_normal(N_TIME)
    extra = dict({"pg:pluginName": "ScanMaster.Acquire.Plugins.LatticeFourShotPatternPlugin",
                  "pg:v0Duration": 12000}, **(extra or {}))
    return make_arrays(on, off, scan_params, None, extra)
