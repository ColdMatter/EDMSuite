"""The simulated lab: stand-ins for the vendor objects.

The fakes sit at the LOWEST level -- they imitate the QZFM package, the
tio device and the NI card -- so the real adapters in qzfm.py,
degausser.py and daq.py run unchanged on top of them. What you test in
the simulator is the same code that drives the bench.

State the parent process and the recorder child must share (is the
laser on, are the two heads on one modulation clock, is the degausser
driving) lives in a small json "world" file. The fake QZFM writes it,
the child's SimUSB6008 reads it every block. Faults can be injected by
editing the world's "inject" section (tests do this):

  calib_silent       [serials]  no Calib reply after '9'
  calib_factor       {serial: {axis: f}}  replace the factors
  zero_never_settles [serials]  compensation keeps wandering
  rail_axis          ["ai1", ...] that DAQ input sits at the QZFM rail
  flat_axis          ["ai0", ...] that DAQ input reads a dead sensor
  laser_drop_at_s    {serial: s} laser lock drops s after auto start
  dg_fault_at_s      s into the taper the DG faults (status -3)
  nonfinite_block    n  the n-th DAQ block has a NaN

Nothing here imports a vendor package; all of it runs on any machine.
"""

from __future__ import annotations

import json
import math
import os
import random
import threading
import time as _time

from bfield import clock
from bfield.constants import GAIN_TABLE, QZFM_SOFT_RAIL_V
from bfield.errors import DaqDeviceNotFound
from bfield.io.files import write_json_atomic

# ------------------------------------------------------------------ world

DEFAULT_HEADS = {
    # FTDI serial -> the simulated head
    "A10QHMQMA": {"port": "COM3", "quspin": "Z3D0-AB3Q",
                  "cal": {"z": 0.80, "y": 0.77, "x": 1.02}},
    "A10QHJ1ZA": {"port": "COM4", "quspin": "Z3D0-AB3S",
                  "cal": {"z": 0.82, "y": 0.79, "x": 0.99}},
}
# standing wiring: head a on ai0 (Z) / ai1 (Y), head b on ai2 / ai3
DEFAULT_WIRING = {"ai0": ["A10QHMQMA", "z"], "ai1": ["A10QHMQMA", "y"],
                  "ai2": ["A10QHJ1ZA", "z"], "ai3": ["A10QHJ1ZA", "y"]}

COLD_START_S = 100.0        # a cold head locks in 95-120 s
CELL_DROP_S = 16.0          # LED2 drops ~16 s after auto start / zero
ZERO_TAU_S = 8.0            # compensation settles with this time constant
BACKGROUND_PT = {"B0": 1800.0, "By": -950.0, "Bz": 2600.0}


class World:
    """The shared state file. Every method re-reads it, so two processes
    (and two fake heads in one process) see each other's writes."""

    _lock = threading.Lock()

    def __init__(self, path, sensors=None):
        """sensors: bfield.sensors.Sensor list -- one simulated head per
        configured sensor (default: the two bench heads)."""
        self.path = path
        self.sensors = list(sensors or [])
        if not os.path.exists(path):
            self.write(self.default(self.sensors))
        else:
            # a sensor added to the config since: give it a head too
            want = self.default(self.sensors)["heads"]
            self.update(lambda d: [d["heads"].setdefault(k, v)
                                   for k, v in want.items()])

    @staticmethod
    def default(sensors=None):
        heads = {}
        rows = ([(s.usb_serial, s.label, s.number) for s in sensors]
                if sensors else
                [(k, v["quspin"], i + 1)
                 for i, (k, v) in enumerate(DEFAULT_HEADS.items())])
        for usb, label, number in rows:
            known = DEFAULT_HEADS.get(usb)
            heads[usb] = {"laser_on": False, "locked": False,
                          "zeroed": False, "master": False,
                          "mode": "dual", "gain": "1x",
                          "t_lock": None, "t_zero": None,
                          "label": label,
                          "port": known["port"] if known
                          else f"COM{20 + int(number)}",
                          "cal": dict(known["cal"]) if known else
                          {"z": round(0.78 + 0.01 * (number % 7), 2),
                           "y": round(0.75 + 0.01 * (number % 5), 2),
                           "x": 1.0}}
        return {"heads": heads, "wiring": DEFAULT_WIRING,
                "dg_driving": False, "inject": {}}

    def read(self):
        for _ in range(50):
            try:
                with open(self.path, encoding="utf-8") as fh:
                    return json.load(fh)
            except (OSError, ValueError):
                _time.sleep(0.01)    # a writer is mid-replace; retry
        return self.default()

    def _replace(self, doc):
        # Windows refuses os.replace while the other process has the file
        # open for its read; retry briefly instead of failing the run
        for attempt in range(50):
            try:
                write_json_atomic(self.path, doc)
                return
            except PermissionError:
                if attempt == 49:
                    raise
                _time.sleep(0.01)

    def write(self, doc):
        with World._lock:
            self._replace(doc)

    def update(self, fn):
        with World._lock:
            doc = self.read()
            fn(doc)
            self._replace(doc)
            return doc

    def inject(self, **faults):
        self.update(lambda d: d.setdefault("inject", {}).update(faults))

    # --- coil drive (S(f) sweep) ---
    def set_coil(self, freq_hz=None, amp_mA=0.0, dc_mA=0.0,
                 orientation="axial", rmon_ohm=0.3):
        """The external coil is driven: freq_hz/amp_mA (0-p) for AC, or
        dc_mA with freq_hz None for a DC dwell. The DAQ sees it shielded,
        the simulated scope reads it across rmon_ohm."""
        coil = {"freq_hz": freq_hz, "amp_mA": float(amp_mA),
                "dc_mA": float(dc_mA), "orientation": orientation,
                "rmon_ohm": float(rmon_ohm)}
        self.update(lambda d: d.update(coil=coil))

    def clear_coil(self):
        self.update(lambda d: d.pop("coil", None))
    # --- end coil drive ---

    def head(self, serial):
        return self.read()["heads"].get(serial, {})

    def ports(self):
        """[(port, usb_serial, description)] the simulated PC would list
        -- what a USB scan finds in the simulator."""
        heads = self.read()["heads"]
        return sorted((h.get("port") or "COM?", usb,
                       "USB Serial Port (FTDI) -- simulated QZFM")
                      for usb, h in heads.items())

    def plug_in_new_head(self):
        """Rehearse adding a sensor: a head the config does not know yet
        appears on a new COM port. Returns its USB serial."""
        import string
        doc = self.read()
        used = {h.get("port") for h in doc["heads"].values()}
        n = next(i for i in range(5, 99) if f"COM{i}" not in used)
        usb = "A10QH" + "".join(random.choice(string.ascii_uppercase
                                              + string.digits)
                                for _ in range(3)) + "A"
        head = World.default([])["heads"]
        new = dict(next(iter(head.values())))
        new.update(label="", port=f"COM{n}",
                   cal={"z": 0.84, "y": 0.81, "x": 1.01})
        self.update(lambda d: d["heads"].__setitem__(usb, new))
        return usb


# ------------------------------------------------------------ fake QZFM

class _FakeSerial:
    """Just enough of a pyserial port for the adapter's raw paths:
    Calibrate ('9') and the modulation role bytes ('N' / 'I')."""

    def __init__(self, pkg, port):
        self.pkg = pkg
        self.port = port
        self.timeout = None
        self.is_open = True
        self._rx = bytearray()
        self._due = []          # (clock time, bytes) not yet "arrived"

    def _pump(self):
        now = clock.monotonic()
        keep = []
        for t, data in self._due:
            if t <= now:
                self._rx += data
            else:
                keep.append((t, data))
        self._due = keep

    @property
    def in_waiting(self):
        self._pump()
        return len(self._rx)

    def reset_input_buffer(self):
        self._pump()
        self._rx.clear()

    def write(self, data):
        if not self.is_open:
            raise OSError("attempting to use a port that is not open")
        self.pkg._command(bytes(data), self)
        return len(data)

    def read(self, size=1):
        if not self.is_open:
            raise OSError("attempting to use a port that is not open")
        deadline = clock.monotonic() + (self.timeout or 0.0)
        while True:
            self._pump()
            if self._rx:
                out = bytes(self._rx[:size])
                del self._rx[:size]
                return out
            if clock.monotonic() >= deadline:
                return b""
            clock.sleep(min(0.05, max(deadline - clock.monotonic(), 0.0)))

    def queue(self, data, delay_s):
        self._due.append((clock.monotonic() + delay_s, data))

    def close(self):
        self.is_open = False


class FakeQZFMPackage:
    """Imitates ucn-triumf's QZFM object as QZFMSensor uses it."""

    def __init__(self, world):
        self.world = world
        self.serial = None
        self.device_name = None
        self.ser = None
        self.messages = []
        self.sensor_par = {}
        self.led = {}
        self._zeroing = False
        self._comp = dict.fromkeys(BACKGROUND_PT, 0.0)
        self._t_zero_on = None

    # -- connection
    def connect(self, device_name):
        heads = self.world.read()["heads"]
        matches = [s for s in heads if device_name in s]
        if not matches:
            raise IndexError(device_name)
        if len(matches) > 1:
            raise RuntimeError(f"{len(matches)} ports match {device_name!r}")
        self.serial = matches[0]
        self.device_name = self.serial
        port = heads[self.serial].get("port") or \
            DEFAULT_HEADS.get(self.serial, {}).get("port", "COM9")
        self.ser = _FakeSerial(self, port)

    def disconnect(self):
        if self.ser is not None:
            self.ser.close()

    def _head(self):
        return self.world.head(self.serial)

    def _set(self, **kv):
        self.world.update(lambda d: d["heads"][self.serial].update(kv))

    # -- status
    def update_status(self):
        h = self._head()
        now = clock.time()
        inject = self.world.read().get("inject", {})
        drop = (inject.get("laser_drop_at_s") or {}).get(self.serial)
        laser_lock = bool(h.get("locked"))
        if drop is not None and h.get("t_lock") and now - h["t_lock"] > drop:
            laser_lock = False
        cell_lock = bool(h.get("locked")) and not (
            h.get("t_lock") and now - h["t_lock"] < CELL_DROP_S) and not (
            h.get("t_zero") and now - h["t_zero"] < CELL_DROP_S)
        if self._zeroing:
            wander = self.serial in (inject.get("zero_never_settles") or [])
            k = 1.0 - math.exp(-(now - self._t_zero_on) / ZERO_TAU_S)
            for key, bg in BACKGROUND_PT.items():
                noise = random.gauss(0, 400.0 if wander else 2.0)
                self._comp[key] = bg * k + noise
        on = bool(h.get("laser_on"))
        # a dark head still answers: zero compensation, a cold cell
        self.sensor_par = {
            "B0 field (pT)": self._comp["B0"] if on else 0.0,
            "By field (pT)": self._comp["By"] if on else 0.0,
            "Bz field (pT)": self._comp["Bz"] if on else 0.0,
            "cell temp error": (random.gauss(0, 0.0003) if cell_lock
                                else 0.02 + random.gauss(0, 0.002)) if on
            else 0.35 + random.gauss(0, 0.001),
        }
        self.led = {"laser on (LED1)": on,
                    "cell temp lock (LED2)": cell_lock,
                    "laser lock (LED3)": laser_lock and on,
                    "field zeroed (LED4)": bool(h.get("zeroed")),
                    "is master": bool(h.get("master"))}
        self.messages.append("|status")

    # -- actions
    def auto_start(self, block=True, show=False, zero_calibrate=False):
        h = self._head()
        if not h.get("locked"):
            self._set(laser_on=True)
            clock.sleep(COLD_START_S)
        self._set(laser_on=True, locked=True, t_lock=clock.time())

    def reboot(self):
        self._set(laser_on=False, locked=False, zeroed=False, master=False,
                  t_lock=None)
        self._comp = dict.fromkeys(BACKGROUND_PT, 0.0)

    def set_axis_mode(self, mode):
        self._set(mode=str(mode), zeroed=False)

    def set_gain(self, gain):
        self.messages.append(f"gain {gain}")
        self._set(gain=str(gain))

    def field_zero(self, on, axes_xyz=True, show=False):
        if on:
            self._zeroing = True
            self._t_zero_on = clock.time()
        else:
            self._zeroing = False
            self._set(zeroed=True, t_zero=clock.time())

    def _command(self, data, ser):
        if data == b"9":
            inject = self.world.read().get("inject", {})
            if self.serial in (inject.get("calib_silent") or []):
                ser.queue(b"#other line\r\n", 1.0)
                return
            cal = dict(self._head().get("cal")
                       or DEFAULT_HEADS.get(self.serial, {}).get("cal")
                       or {"z": 0.8, "y": 0.8})
            cal.update((inject.get("calib_factor") or {}).get(self.serial)
                       or {})
            text = "Calib. Fact. : " + " ".join(
                f"{v:.2f} ({a})" for a, v in cal.items())
            # the reply arrives alone on a quiet line, ~10 s later
            ser.queue(b"#" + text.encode() + b"\r\n", 10.0)
        elif data in (b"N", b"I"):
            self._set(master=(data == b"N"))

    def read_data(self, seconds, axis="z"):
        n = max(int(200 * seconds), 1)
        clock.sleep(seconds)
        on = self._head().get("laser_on")
        t = [i / 200.0 for i in range(n)]
        v = [(random.gauss(0, 2.0) if on else float("nan")) for _ in t]
        return t, v


def qzfm_backend(world):
    """A backend factory for QZFMSensor(backend=...)."""
    return lambda: FakeQZFMPackage(world)


# --- coil drive (S(f) sweep) ---
COIL_K_NT_PER_MA = 20.0 / 9.0     # free-space B/I at the coil centre
SIM_S_DC = 2.0e4                  # made-up shielding factor at DC
SIM_EDDY_CORNER_HZ = 6.0          # ~1 mm wall
COIL_AXIS = {"axial": "z", "transverse": "y"}


def sim_shielding_factor(f_hz):
    """The simulator's S(f): flat to the eddy corner, then rising."""
    f = float(f_hz or 0.0)
    return SIM_S_DC * math.sqrt(1.0 + (f / SIM_EDDY_CORNER_HZ) ** 2)


def coil_field_pT(coil, axis, t):
    """Shielded coil field (pT) on one head axis, or 0 when undriven."""
    if not coil or axis != COIL_AXIS.get(coil.get("orientation", "axial")):
        return 0.0
    import numpy as np
    f = coil.get("freq_hz")
    out = (float(coil.get("dc_mA") or 0.0) * COIL_K_NT_PER_MA * 1000.0
           / sim_shielding_factor(0.0))
    if f:
        amp = (float(coil.get("amp_mA") or 0.0) * COIL_K_NT_PER_MA * 1000.0
               / sim_shielding_factor(f))
        out = out + amp * np.sin(2 * math.pi * float(f) * t)
    return out
# --- end coil drive ---


# --------------------------------------------------------------- fake DAQ

class SimUSB6008:
    """Imitates NIDaq: volts the QZFM heads in the world would put out,
    digitised like the configured card (bfield.daq_models)."""

    name = "simulated"

    def __init__(self, world, model=None, device="Dev2"):
        from bfield.daq_models import active
        self.world = world
        self.model = model or active()
        self.device = device or "Dev2"
        self.channels = []
        self.vrange = 2.0
        self.fs = 1200.0
        self._n = 0
        self._rw = {}

    def list_devices(self):
        return [self.device]

    def resolve_device(self, requested, hardware_check=False):
        if requested and requested not in self.list_devices():
            raise DaqDeviceNotFound(
                f"device {requested!r} not found; available: "
                f"{self.list_devices()}")
        return requested or self.device

    def open(self, channels, vrange, fs):
        self.channels = list(channels)
        self.vrange = float(vrange)
        self.fs = float(fs)
        self._n = 0
        return self

    def _volts(self, phys, t, doc):
        import numpy as np
        pin = phys.split("/")[-1]
        inject = doc.get("inject", {})
        n = t.size
        if pin in (inject.get("rail_axis") or []):
            return np.full(n, QZFM_SOFT_RAIL_V + 0.03)
        serial, axis = (doc.get("wiring") or DEFAULT_WIRING).get(
            pin, [None, None])
        head = doc["heads"].get(serial) if serial else None
        if (pin in (inject.get("flat_axis") or []) or head is None
                or not head.get("laser_on")):
            return np.zeros(n)          # a dead output: one ADC code
        v_per_nT = GAIN_TABLE.get(head.get("gain", "1x"), 2.7)
        # field in pT: offset (zeroed or not), 1/f-ish walk, white floor
        # 23 fT/rtHz, a 50 Hz line and the aliased modulation carrier
        offset = 25.0 if head.get("zeroed") else 700.0
        walk = self._rw.get(pin, 0.0)
        steps = np.cumsum(np.random.normal(0, 0.02, n)) + walk
        self._rw[pin] = float(steps[-1]) * 0.999
        white = np.random.normal(0, 0.023 * math.sqrt(self.fs / 2.0), n)
        line = 3.0 * np.sin(2 * math.pi * 50.0 * t)
        synced = sum(1 for h in doc["heads"].values() if h.get("master")) == 1
        carrier_hz = 922.9 + (0.0 if synced or serial == "A10QHMQMA" else 0.8)
        carrier = 5.0 * np.sin(2 * math.pi * carrier_hz * t)
        pT = offset + steps + white + line + carrier
        # --- coil drive (S(f) sweep) ---
        pT = pT + coil_field_pT(doc.get("coil"), axis, t)
        # --- end coil drive ---
        return pT / 1000.0 * v_per_nT

    def blocks(self, n_ch, block_n, timeout=10.0):
        import numpy as np
        due = clock.monotonic()
        while True:
            due += block_n / self.fs
            clock.sleep(max(0.0, due - clock.monotonic()))
            doc = self.world.read()
            t = (self._n + np.arange(block_n)) / self.fs
            blk = np.vstack([self._volts(p, t, doc) for p in self.channels])
            # the card's ADC over +/-vrange, clipped at the rail
            lsb = self.model.lsb(self.vrange)
            blk = np.clip(np.rint(blk / lsb) * lsb, -self.vrange, self.vrange)
            self._n += block_n
            if doc.get("inject", {}).get("nonfinite_block") == \
                    self._n // block_n:
                blk[0, 0] = float("nan")
            yield blk

    def finite_read(self, channels, seconds, vrange=1.0, fs=1000.0):
        import numpy as np
        self.open(channels, vrange, fs)
        n = max(int(fs * seconds), 1)
        src = self.blocks(len(channels), n)
        return np.atleast_2d(next(src))

    def close(self):
        pass


# ---------------------------------------------------------------- fake DG

DG_COLUMNS = ["degauss.A", "degauss.V", "degauss.Ohm", "degauss.status"]
DG_ROW_HZ = 348.0           # measured stream rate
DG_MEASURE_S = 3.0
DG_COOLDOWN_S = 6.0
DG_CAP_A = 8.0


class _Callable:
    """tio exposes each setting as a callable: f() reads, f(v) writes."""

    def __init__(self, get, put=None, children=None):
        self._get, self._put = get, put
        for k, v in (children or {}).items():
            setattr(self, k, v)

    def __call__(self, *args):
        if args:
            if self._put is None:
                raise AttributeError("read-only")
            self._put(*args)
            return None
        return self._get()


class _NS:
    pass


class FakeTioDevice:
    """Imitates tldevice.Device(port) for a Twinleaf DG R4."""

    def __init__(self, port, world):
        self.port = port
        self.world = world
        self.wave = {"current": 5.0, "frequency": 15.0, "soaktime": 15.0,
                     "duration": 300.0}      # arrives holding 5 A (D-7)
        self.t_trig = None
        self.plan = None
        self.aborted_at = None
        self.fault_at = None
        self.closed = False

        dev = _NS()
        dev.desc = lambda: "Twinleaf DG R4 N78 [2023-07-14/4d4c6f]"
        dev.name = lambda: "DG"
        dev.serial = lambda: "N78"
        self.dev = dev

        def setting(key, lo=None, hi=None):
            def put(v):
                v = float(v)
                if lo is not None:
                    v = max(v, lo)
                if hi is not None:
                    v = min(v, hi)          # the firmware clamps silently
                self.wave[key] = v
            return _Callable(lambda: self.wave[key], put)

        waveform = _NS()
        waveform.current = setting("current", 0.0, DG_CAP_A)
        waveform.current.max = lambda: DG_CAP_A
        waveform.frequency = setting("frequency", 3.0, 60.0)
        waveform.soaktime = setting("soaktime", 0.0, 120.0)
        waveform.duration = setting("duration", 5.0, 600.0)
        degauss = _NS()
        degauss.waveform = waveform
        degauss.trigger = self._trigger
        degauss.abort = self._abort
        data = _NS()
        data.active = lambda *a: None
        degauss.data = data
        self.degauss = degauss

        stream = _NS()
        stream.iter = self._iter
        stream.columnnames = lambda *a: list(DG_COLUMNS)
        self.data = stream
        # tio 3.8 lists rpcs as dicts (bench D-6)
        self._tio = _NS()
        self._tio.rpc_names = [{"name": n} for n in (
            "degauss.abort", "degauss.data.active", "degauss.trigger",
            "degauss.waveform.current", "degauss.waveform.current.max",
            "degauss.waveform.duration", "degauss.waveform.frequency",
            "degauss.waveform.soaktime", "dev.desc", "dev.name")]

    # -- the cycle
    def _trigger(self):
        w = self.wave
        ramp = 1.6 + 1.4 * w["current"]
        self.plan = [("measuring", 4, DG_MEASURE_S), ("ramping", 5, ramp),
                     ("soaking", 6, w["soaktime"]),
                     ("taper", 7, w["duration"]),
                     ("cooldown", 8, DG_COOLDOWN_S)]
        self.t_trig = clock.monotonic()
        self.aborted_at = None
        inject = self.world.read().get("inject", {})
        f = inject.get("dg_fault_at_s")
        self.fault_at = (None if f is None else
                         DG_MEASURE_S + ramp + w["soaktime"] + float(f))
        self.world.update(lambda d: d.update(dg_driving=True))

    def _abort(self):
        if self.t_trig is not None:
            self.aborted_at = clock.monotonic() - self.t_trig

    def _phase_at(self, t):
        """(status code, current envelope 0..1) t s after trigger."""
        if self.t_trig is None or t < 0:
            return 0, 0.0
        stop = self.aborted_at
        if self.fault_at is not None and (stop is None or self.fault_at < stop):
            if t >= self.fault_at:
                return (-3, 0.0) if t < self.fault_at + 14.0 else (0, 0.0)
        if stop is not None and t >= stop:
            return 0, 0.0
        acc = 0.0
        for name, code, dur in self.plan:
            if t < acc + dur:
                x = (t - acc) / max(dur, 1e-9)
                env = {"measuring": 0.0, "ramping": x, "soaking": 1.0,
                       "taper": 1.0 - x, "cooldown": 0.0}[name]
                return code, env
            acc += dur
        return 0, 0.0

    def status(self, samples=1, flush=False):
        if self.t_trig is None:
            return [0]
        code, _ = self._phase_at(clock.monotonic() - self.t_trig)
        if code == 0 and self.world.read().get("dg_driving"):
            self.world.update(lambda d: d.update(dg_driving=False))
        return [code]

    def _iter(self):
        t_row = clock.monotonic()
        k = 0
        while not self.closed:
            now = clock.monotonic()
            if t_row > now:
                clock.sleep(t_row - now)
            t = t_row - (self.t_trig if self.t_trig is not None else t_row)
            code, env = self._phase_at(t)
            amp = self.wave["current"] * env
            i = amp * math.sin(2 * math.pi * self.wave["frequency"]
                               * k / DG_ROW_HZ)
            z = 0.82 if abs(i) < 0.9 * DG_CAP_A else 0.4333
            yield (i, i * z, z if abs(i) > 1e-6 else 0.0, float(code))
            k += 1
            t_row += 1.0 / DG_ROW_HZ

    def close(self):
        self.closed = True


def dg_backend(world):
    """A backend factory for TwinleafDG(backend=...)."""
    return lambda port: FakeTioDevice(port, world)
