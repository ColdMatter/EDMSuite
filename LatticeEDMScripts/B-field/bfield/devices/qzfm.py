"""QuSpin QZFM head over its serial port (no QuSpin GUI).

Moved from IROP_research/scripts/qzfm_serial.py (SensorLink,
HealthTracker, the Calib-reply parser). Behaviour is unchanged; the
differences are plumbing only:

  * messages go to a Context (ctx.say/ctx.warn), not print
  * time goes through bfield.clock (so the simulator can run fast)
  * the vendor object comes from a `backend` factory: the real QZFM
    package on the bench, bfield.devices.simulated.FakeQZFMPackage in
    the simulated lab. Everything above the vendor object -- timeouts,
    locking, settle criteria, the Calibrate read -- is the same code in
    both, so the simulator exercises the real adapter.

The COM port is exclusive: close the QuSpin GUI first.
"""

from __future__ import annotations

import math
import re
import threading

from bfield import clock
from bfield.constants import (CAL_FACTOR_MAX, CAL_FACTOR_MIN, COMP_ABORT_PT)
from bfield.devices.watchdog import run_with_watchdog
from bfield.errors import (ConfigError, LinkTimeout, PortBusyError,
                           PortLostError, SensorAbort, SensorError)

# every timeout exists because the QZFM package blocks without one
SERIAL_TIMEOUT_S = 2.0         # pyserial read timeout, set right after connect
CALL_TIMEOUT_S = 30.0          # watchdog for short package calls
CAL_TIMEOUT_S = 120.0          # calibrate waits on a serial message
# cell lock (LED2) drops ~16 s after handoffs (measured 2026-08-05,
# five times); the loops exit on the first good read, so waiting is free
LOCK_RECHECK_S = 45.0          # how long a re-zero waits before "not locked"
CELL_RELOCK_S = 60.0           # how long setup waits for LED2 to come back
REZERO_TIMEOUT_S = 120.0       # settle deadline for guard-heal re-zeroes
STANDBY_VERIFY_WAIT_S = 3.0    # settle before the post-reboot readback

ZERO_CONSECUTIVE = 5           # settle passes required, package/GUI practice
ZERO_POLL_S = 1.0
ZERO_REPORT_S = 10.0           # progress line while zeroing (up to 600 s)
BRINGUP_REPORT_S = 15.0        # progress line during a cold auto start
CELLT_SETTLE_CONSECUTIVE = 3
CELLT_BAD_LEVEL = 0.05         # provisional out-of-band level until M1 pins it
CELLT_SETTLE_ERR = 0.001       # |error| this small counts as settled
CELLT_SETTLE_STEP = 0.001      # an error that stopped moving also counts
CELLT_BAD_S = 60.0             # persistence before it counts as an abort
MESSAGES_KEEP = 500            # the package grows its message list unboundedly

# mid-session port-loss marks only. Deliberately narrow: "access is
# denied" / "could not open port" are PortBusyError at connect time
PORT_LOST_MARKS = ("handle is invalid", "readfile failed", "writefile failed",
                   "attempting to use a port that is not open",
                   "device reports readiness")


def port_was_lost(exc):
    name = type(exc).__name__
    text = str(exc).lower()
    return (name in ("SerialException", "PortNotOpenError")
            or any(m in text for m in PORT_LOST_MARKS))


# two reply shapes: 'Calib. Fact. : 0.64 (z)' (live sensor) and
# 'x:1.02,y:0.98' (GUI-style); both accepted
CALIB_VALUE_AXIS_RX = re.compile(
    r"([0-9]*\.?[0-9]+(?:[eE][+\-]?[0-9]+)?)\s*\(\s*([xyzXYZ])\s*\)")
CALIB_AXIS_VALUE_RX = re.compile(
    r"(?:^|[,;])\s*([xyzXYZ])\s*:\s*([0-9]*\.?[0-9]+(?:[eE][+\-]?[0-9]+)?)")


def parse_calib_message(text):
    """Best-effort per-axis factors from the sensor's Calib reply.

    Values outside CAL_FACTOR_MIN..MAX are dropped. The raw text is
    stored separately, so a failed parse loses nothing.
    """
    factors = {}
    text = str(text)
    for rx, axis_group, value_group in ((CALIB_VALUE_AXIS_RX, 2, 1),
                                        (CALIB_AXIS_VALUE_RX, 1, 2)):
        for m in rx.finditer(text):
            try:
                v = float(m.group(value_group))
            except ValueError:
                continue
            if CAL_FACTOR_MIN <= v <= CAL_FACTOR_MAX:
                factors.setdefault(m.group(axis_group).lower(), v)
    return factors


def frame_is_blank(s):
    """True when a status frame has every flag off and every number nan.

    Seen once right after a killed process: such a frame read both heads
    as cold, then fully locked 40 s later (a real cold start takes
    95-120 s), so it was a contentless frame. Names the pattern only.
    """
    return (not any(s[k] for k in ("laser_on", "cellT_lock",
                                   "laser_lock", "zeroed"))
            and all(math.isnan(s[k]) for k in ("B0_pT", "By_pT",
                                               "Bz_pT", "cellT_err")))


# the QZFM protocol is ASCII; anything above 0x7f is line noise
_HIGH_BYTES = bytes.maketrans(bytes(range(0x80, 0x100)), b"?" * 128)


def make_reads_decodable(q):
    """Stop a noise byte on the wire from crashing the vendor package.

    QZFM.py does a bare .decode('utf-8') on a fixed-length slice, so any
    byte over 0x7f raises UnicodeDecodeError (crashed every standby on
    2026-08-17). The '?' substitution is length-preserving on purpose:
    the vendor slices by byte count. Protocol is ASCII, nothing is lost.
    """
    ser = getattr(q, "ser", None)
    if ser is None or getattr(ser, "_reads_are_ascii", False):
        return
    inner = ser.read

    def read(size=1):
        data = inner(size)
        return data.translate(_HIGH_BYTES) if data else data

    ser.read = read
    ser._reads_are_ascii = True


def real_qzfm_backend():
    """The vendor object on the bench: ucn-triumf's QZFM package."""
    try:
        from QZFM import QZFM  # heavy: numpy/pandas/matplotlib backend
    except ImportError as e:
        raise ConfigError(
            f"the QZFM package is not installed ({e}) -- on the lab PC: "
            "pip install QZFM")
    return QZFM()


def _serial_exception_type():
    try:
        import serial as pyserial
        return pyserial.SerialException
    except ImportError:          # pragma: no cover - pyserial is a dep
        class _Never(Exception):
            pass
        return _Never


def list_serial_ports():
    """[(device, serial_number, description)] -- what --device-name wants.
    Read-only; works with the QuSpin GUI open."""
    try:
        from serial.tools import list_ports
    except ImportError as e:
        raise ConfigError(f"pyserial is not installed ({e}) -- on the lab "
                          "PC: pip install -e .[lab]")
    ports = sorted(list_ports.comports(), key=lambda p: p.device)
    return [(p.device, getattr(p, "serial_number", None) or "-",
             p.description or "") for p in ports]


class QZFMSensor:
    """One QZFM head. Thin adapter over the vendor package.

    Adds a serial read timeout, a lock around every serial access,
    watchdogs, liveness (`dead`), and message-list trimming. auto_start
    is always called with zero_calibrate=False: the full variant loops
    with no deadline and writes a YAML into the CWD.
    """

    def __init__(self, device_name, ctx, *, zero_cond=100.0,
                 lock_timeout=600.0, head="a", backend=None):
        self._lock = threading.Lock()
        self.ctx = ctx
        self.dead = False
        self.zero_off_failed = None
        self.head = head
        # how messages name this head; the session sets it from the
        # sensor list ("sensor 1 (Z3D0-AB3Q)")
        self.display = f"head {head} ({device_name})"
        # called with (sensor, frame) after every status frame -- the
        # session points it at the UI's indicators
        self.on_frame = None
        self.port = None         # the COM port that answered
        self.device_name = device_name
        self.zero_cond = zero_cond
        self.lock_timeout = lock_timeout
        self.backend = backend or real_qzfm_backend
        self.qzfm = None
        # the serial the port actually answered with; child records are
        # stamped with this, never a default
        self.serial_id = device_name

    # ------------------------------------------------------------ plumbing

    def _warn(self, msg):
        self.ctx.warn(msg)

    def _call(self, fn, timeout, desc, heartbeat=None, heartbeat_s=None):
        if self.dead:
            raise SensorError(f"serial link is dead ({desc} refused)")

        def locked():
            with self._lock:
                return fn()

        def kill_link():
            # deliberately no lock: the hung worker holds it, and closing
            # the port from here is what unblocks its read
            self.dead = True
            self._emergency_close()

        try:
            return run_with_watchdog(locked, timeout, desc, kill_link,
                                     heartbeat, heartbeat_s, warn=self._warn)
        except LinkTimeout:
            self.dead = True
            raise
        except Exception as e:
            # the port WAS open by here, so a SerialException means the
            # device left, not that someone else holds it (2026-08-05)
            if port_was_lost(e):
                self.dead = True
                raise PortLostError(
                    f"the sensor's serial port went away during {desc} "
                    f"({type(e).__name__}: {e}) -- USB re-enumerated or "
                    "lost power. Check the USB cable and the electronics "
                    "module's power, replug the sensor, then check status"
                ) from e
            raise

    def connect(self):
        # the package's connect blocks with no read timeout, so watchdog
        # it too; on_timeout frees a half-open COM handle (2026-08-14)
        return run_with_watchdog(self._connect_body, CALL_TIMEOUT_S,
                                 "opening the sensor port",
                                 on_timeout=self._emergency_close,
                                 warn=self._warn)

    def _connect_body(self):
        serial_exc = _serial_exception_type()
        try:
            q = self.backend()
            # visible to the emergency close WHILE connect blocks -- a
            # timeout mid-open must be able to reach the half-open port
            self.qzfm = q
            q.connect(self.device_name)
            make_reads_decodable(q)
        except IndexError:
            self.qzfm = None
            raise ConfigError(
                f"no serial port matches {self.device_name!r} -- sensor USB "
                "unplugged, or a different unit? List the ports")
        except RuntimeError as e:
            self.qzfm = None
            raise ConfigError(f"serial port scan: {e} -- give more of the "
                              "serial number in the device name")
        except serial_exc as e:
            self.qzfm = None
            raise PortBusyError(
                f"cannot open the sensor port ({e}) -- close the QuSpin GUI "
                "first, the COM port is exclusive")
        except (SensorError, ConfigError):
            self.qzfm = None
            raise
        except Exception as e:
            # name the connect stage, or the caller sees a bare exit 1;
            # the package's forced TkAgg backend is one real way in here
            self.qzfm = None
            raise ConfigError(
                f"opening the sensor failed with an unexpected "
                f"{type(e).__name__}: {e} -- the port was not opened")
        q.ser.timeout = SERIAL_TIMEOUT_S  # the package leaves reads unbounded
        self.qzfm = q
        self.serial_id = self._read_serial(q)
        # which port answered: bridges the FTDI serial we match on to
        # the QuSpin name calibrations are filed under
        self.port = getattr(q.ser, "port", None)
        return self

    def _read_serial(self, q):
        """Full serial of the head that answered, for the child's meta."""
        for attr in ("device_name", "serial_number", "name"):
            v = getattr(q, attr, None)
            if isinstance(v, str) and v.strip():
                return v.strip()
        # the package may not expose it; the device name is required and
        # unique by validation, so it is the next best identifier
        return self.device_name

    def _emergency_close(self):
        # tolerant of a half-constructed link: during connect the QZFM
        # object may not have its serial handle yet
        ser = getattr(self.qzfm, "ser", None)
        if ser is not None:
            ser.close()   # failures surface via run_with_watchdog

    def _snapshot(self):
        q = self.qzfm
        q.update_status()
        del q.messages[:-MESSAGES_KEEP]
        sp, led = q.sensor_par, q.led
        nan = float("nan")
        out = {
            "B0_pT": float(sp.get("B0 field (pT)", nan)),
            "By_pT": float(sp.get("By field (pT)", nan)),
            "Bz_pT": float(sp.get("Bz field (pT)", nan)),
            "cellT_err": float(sp.get("cell temp error", nan)),
            "laser_on": bool(led.get("laser on (LED1)", False)),
            "cellT_lock": bool(led.get("cell temp lock (LED2)", False)),
            "laser_lock": bool(led.get("laser lock (LED3)", False)),
            "zeroed": bool(led.get("field zeroed (LED4)", False)),
            "master": bool(led.get("is master", False)),
        }
        snap = out
        if self.on_frame is not None:
            try:
                self.on_frame(self, dict(snap))
            except Exception:
                pass
        return snap

    # ------------------------------------------------------------- reading

    def status(self):
        """One status frame (the head-reading path)."""
        return self._call(self._snapshot, CALL_TIMEOUT_S, "status read")

    def house(self):
        """A status frame for the housekeeping poller.

        Bounded lock acquire: a call abandoned by its watchdog still
        holds the lock, and a plain acquire once hung the poller.
        """
        if self.dead:
            raise SensorError("serial link is dead")
        if not self._lock.acquire(timeout=clock.real_seconds(CALL_TIMEOUT_S)):
            raise SensorError(
                "housekeeping could not take the serial lock within "
                f"{CALL_TIMEOUT_S:g} s -- an abandoned call is still "
                "holding it")
        try:
            return self._snapshot()
        finally:
            self._lock.release()

    # ------------------------------------------------------------- actions

    def bringup(self, force=False):
        """Auto start (laser + cell lock), skipped when already locked."""
        snap = self.status()
        if snap["laser_lock"] and snap["cellT_lock"] and not force:
            return {"warm_start": True,
                    "skip_reason": "laser lock (LED3) and cell temperature "
                                   "lock (LED2) are already on"}
        self.ctx.say("setup", "auto start"
                     + (" (forced, already locked)" if force
                        else " (cold: 95-120 s is normal)"))

        # wall-clock only: auto_start holds the link lock throughout,
        # so the heartbeat cannot read anything fresh
        def beat(elapsed, remaining):
            self.ctx.say("setup", f"  auto start {elapsed:.0f} s")
            self.ctx.progress(min(elapsed / 120.0, 1.0), "auto start")

        try:
            self._call(
                lambda: self.qzfm.auto_start(block=True, show=False,
                                             zero_calibrate=False),
                self.lock_timeout, "auto start",
                heartbeat=beat, heartbeat_s=BRINGUP_REPORT_S)
        except LinkTimeout as e:
            raise LinkTimeout(
                f"{self.display}: {e}. State at start: laser_on={snap['laser_on']}, "
                f"cellT_lock={snap['cellT_lock']}, "
                f"laser_lock={snap['laser_lock']}, "
                f"cellT_err={snap['cellT_err']}. A cold head locks in "
                "95-120 s -- check the USB cable and electronics power, "
                "then check status"
            ) from e
        # cell lock drops for ~16 s as auto start hands back, so poll up
        # to CELL_RELOCK_S instead of judging on one read (2026-08-19)
        t0 = clock.time()
        snap = self.status()
        while not (snap["laser_lock"] and snap["cellT_lock"]) \
                and clock.time() - t0 < CELL_RELOCK_S:
            self.ctx.sleep(1.0)
            snap = self.status()
        waited = round(clock.time() - t0, 1)
        if not (snap["laser_lock"] and snap["cellT_lock"]):
            raise SensorError(
                "auto start returned without laser/cell temperature lock "
                f"(laser_lock={snap['laser_lock']}, "
                f"cellT_lock={snap['cellT_lock']}, "
                f"cellT_err={snap['cellT_err']}, still so after "
                f"{CELL_RELOCK_S:g} s)")
        if waited > 1.0:
            self.ctx.say("setup", f"  cell temperature lock came back after "
                                  f"{waited:.0f} s")
        return {"warm_start": False, "skip_reason": None,
                "lock_wait_s": waited}

    def standby(self):
        """Serial 'e': reboot to power-on state (laser + heater off)."""
        try:
            self._call(lambda: self.qzfm.reboot(), CALL_TIMEOUT_S,
                       "reboot to standby")
        except UnicodeDecodeError as e:
            # reboot() reads status after sending; a decode failure says
            # nothing about whether the command landed -- do not crash
            self._warn(f"standby: command sent but the status readback "
                       f"failed ({e}) -- sensor state UNVERIFIED, not failed")

    def set_mode(self, mode):
        """Axis mode: 'dual', 'z' or 'y'. Resets the zero/cal flags."""
        self._call(lambda: self.qzfm.set_axis_mode(mode),
                   CALL_TIMEOUT_S, "axis mode")

    def set_gain(self, gain):
        # the package prints one message line here; tolerated
        self._call(lambda: self.qzfm.set_gain(gain), CALL_TIMEOUT_S, "gain")

    def set_master(self, is_master):
        """Set the modulation clock role, raw bytes on the wire.

        QuSpin support 2026-08-06: command 78 (b'N') to the master, 73
        (b'I') to the slave; not in the public command list. Do not
        switch to the package's set_master (113/114): that pair was
        measured NOT to fix the two-head calibrate silence. The LED
        |50/|51 master flag is read back by the caller."""
        cmd = b'N' if is_master else b'I'
        self._call(lambda: self.qzfm.ser.write(cmd), CALL_TIMEOUT_S,
                   "modulation " + ("master (command 78)" if is_master
                                    else "slave (command 73)"))

    def _zero_begin(self):
        self._call(
            lambda: self.qzfm.field_zero(True, axes_xyz=True, show=False),
            CALL_TIMEOUT_S, "field zero on")

    def _zero_end(self):
        self._call(
            lambda: self.qzfm.field_zero(False, axes_xyz=True, show=False),
            CALL_TIMEOUT_S, "field zero off")

    def zero(self, timeout, progress=None):
        """Field zero with our own settle criterion; returns the worst
        per-axis compensation drift (pT/s) at settle.

        A repeated serial frame reads as a perfect 0.0 drift -- same as
        a genuinely settled sensor -- so staleness is judged by the whole
        frame repeating, cell temperature error included (that one
        always jitters).
        """
        self._zero_begin()
        deadline = clock.monotonic() + timeout
        prev = prev_t = prev_frame = None
        streak, worst, stale = 0, None, 0
        t_report = clock.monotonic()
        try:
            while True:
                snap = self.status()
                comps = (snap["B0_pT"], snap["By_pT"], snap["Bz_pT"])
                frame = comps + (snap["cellT_err"],)
                for c in comps:
                    if math.isfinite(c) and abs(c) > COMP_ABORT_PT:
                        raise SensorAbort(
                            f"compensation at {c / 1000.0:.1f} nT is near "
                            "the +/-50 nT coil range -- background too large")
                now = clock.monotonic()
                repeated = (prev_frame is not None and frame == prev_frame
                            and not any(math.isnan(v) for v in frame))
                if repeated:
                    stale += 1          # neither progress nor a reset
                elif prev is not None and all(math.isfinite(c) for c in comps):
                    dt = max(now - prev_t, 1e-6)
                    worst = max(abs(c - p) / dt for c, p in zip(comps, prev))
                    streak = streak + 1 if worst < self.zero_cond else 0
                    if streak >= ZERO_CONSECUTIVE:
                        break
                if not repeated:
                    prev, prev_t, prev_frame = comps, now, frame
                if progress and now - t_report >= ZERO_REPORT_S:
                    t_report = now
                    progress(worst, streak, stale, deadline - now)
                if clock.monotonic() > deadline:
                    extra = (f"; {stale} of the reads were repeated frames "
                             "-- the sensor is not sending new values"
                             if stale else "")
                    raise LinkTimeout(
                        f"field zero did not settle below "
                        f"{self.zero_cond:g} pT/s within {timeout:g} s"
                        f"{extra}")
                self.ctx.sleep(ZERO_POLL_S)
        finally:
            # always try to switch zeroing off, loudly on failure: a
            # head still zeroing corrupts the calibrate that runs next
            try:
                self._zero_end()
            except SensorError as e:
                self._warn(f"could not switch field zeroing off ({e}) -- "
                           "head may still be zeroing; distrust this "
                           "setup's cal factors")
                self.zero_off_failed = str(e)
        self._wait_cell_settle(deadline)
        return worst

    def _wait_cell_settle(self, deadline):
        # zeroing perturbs the cell temperature servo (package behaviour):
        # wait until the error is small or no longer moving
        prev, streak = None, 0
        while True:
            err = self.status()["cellT_err"]
            if not math.isnan(err):
                ok = (abs(err) <= CELLT_SETTLE_ERR
                      or (prev is not None
                          and abs(err - prev) <= CELLT_SETTLE_STEP))
                streak = streak + 1 if ok else 0
                prev = err
            if streak >= CELLT_SETTLE_CONSECUTIVE:
                return
            if clock.monotonic() > deadline:
                raise LinkTimeout("cell temperature did not restabilise "
                                  "after zeroing")
            self.ctx.sleep(ZERO_POLL_S)

    def calibrate_factors(self):
        """Send Calibrate ('9') and read the reply off the wire ourselves.

        The sensor pushes the 'Calib. Fact.' line exactly once, no
        readback later. The package's receive path drops a reply that
        arrives alone on a quiet line (its _read_serial discards the
        first and last split elements) -- the 100 s calibrate silences
        of 2026-08-05/06/07. One attempt only: retries measured 0
        recoveries (2026-08-06). Returns (parsed dict, raw text, head
        state at the '9').
        """
        q = self.qzfm
        pre = self.status()
        state = (f"zeroed(LED4)={pre.get('zeroed')}, "
                 f"laser_lock={pre.get('laser_lock')}, "
                 f"cellT_lock={pre.get('cellT_lock')}, "
                 f"master={pre.get('master')}")

        def send_and_read():
            # reset once BEFORE the '9' (stale "#Calib" in the OS buffer);
            # never mid-wait -- that is how the package lost the reply
            q.ser.reset_input_buffer()
            q.ser.write(b"9")
            lines, buf = [], b""
            deadline = clock.monotonic() + CAL_TIMEOUT_S
            while clock.monotonic() < deadline:
                chunk = q.ser.read(1)      # pyserial timeout bounds this
                if not chunk:
                    continue
                waiting = getattr(q.ser, "in_waiting", 0) or 0
                if waiting:
                    chunk += q.ser.read(waiting)
                buf += chunk
                *complete, buf = buf.split(b"\r\n")
                for raw in complete:
                    text = (raw.decode("utf-8", "replace")
                            .replace("\x00", "").strip())
                    if not text:
                        continue
                    lines.append(text)
                    if "Calib" in text:
                        return lines, text
            return lines, None

        t0 = clock.time()
        lines, hit = self._call(send_and_read, CAL_TIMEOUT_S + 15.0,
                                "calibrate")
        elapsed = clock.time() - t0
        if hit is not None:
            text = hit.lstrip("#").strip()
            return parse_calib_message(text), text, state
        # "said nothing" and "said something unrecognised" need
        # different lab actions, so report what was seen
        seen = [ln.lstrip("#").strip() for ln in lines]
        return {}, ("no line containing 'Calib' arrived in "
                    f"{elapsed:.1f} s (head state at the command: "
                    f"{state}); {len(seen)} other line(s) in the "
                    "window: " + (" | ".join(seen[-5:])
                                  if seen else "(none at all)")), state

    def read_stream(self, seconds, axis):
        """The serial digital stream (200 S/s): (t, values)."""
        # a short read leaves the package as a bare AssertionError (a
        # 10 s read once returned 402 of 2000); retry once, then report
        for attempt in (0, 1):
            try:
                return self._call(
                    lambda: self.qzfm.read_data(seconds, axis=axis),
                    seconds + 30.0, "digital stream read")
            except AssertionError as e:
                if attempt == 0:
                    self._warn(f"digital stream read came back short ({e}) "
                               "-- retrying once")
                    continue
                raise LinkTimeout(
                    f"the sensor returned short reads twice ({e}) -- serial "
                    "diagnostic stream only, analog path unaffected") from e

    def close(self):
        if self.qzfm is not None:
            try:
                # disconnect talks to the port, so watchdog it too; a
                # hung disconnect must still free the COM handle
                run_with_watchdog(self.qzfm.disconnect, CALL_TIMEOUT_S,
                                  "closing the sensor port",
                                  on_timeout=self._emergency_close,
                                  warn=self._warn)
            except Exception as e:
                # non-fatal (the run is over), but an unclosed port is
                # what makes the NEXT run fail with a busy port
                self._warn(f"could not close the sensor port "
                           f"({type(e).__name__}: {e}) -- if the next start "
                           "says the port is busy, unplug the sensor USB")
        self.qzfm = None


class HealthTracker:
    """Abort criteria: compensation near the coil range, laser unlock,
    cell-temp error persistently out of band. NaN frames are missing
    data, not evidence."""

    def __init__(self):
        self.cellT_bad_since = None
        self.cellT_unlock_since = None
        self.empty_since = None
        self.reason = None

    def check(self, snap):
        if self.reason:
            return self.reason
        # an all-NaN, all-off frame is a serial glitch, not a lost lock
        # (reading it as one killed a night); a STREAM of them aborts below
        fields = [snap.get(k, float("nan"))
                  for k in ("B0_pT", "By_pT", "Bz_pT", "cellT_err")]
        leds = [snap.get(k, False)
                for k in ("laser_on", "cellT_lock", "laser_lock", "zeroed")]
        if not any(math.isfinite(v) for v in fields) and not any(leds):
            now = clock.monotonic()
            if self.empty_since is None:
                self.empty_since = now
            elif now - self.empty_since > CELLT_BAD_S:
                self.reason = (f"housekeeping frames empty for over "
                               f"{CELLT_BAD_S:g} s -- nothing is coming "
                               f"back from the head")
                return self.reason
            return None
        self.empty_since = None
        for key in ("B0_pT", "By_pT", "Bz_pT"):
            v = snap.get(key, float("nan"))
            if math.isfinite(v) and abs(v) > COMP_ABORT_PT:
                axis = key.replace("_pT", "")
                self.reason = (f"compensation {axis} at {v / 1000.0:.1f} "
                               "nT, near the +/-50 nT range")
                return self.reason
        # a missing laser_lock key is not a locked laser: defaulting it
        # True once disabled this abort for the rest of the night
        if not snap.get("laser_lock", False):
            self.reason = ("laser lock lost" if "laser_lock" in snap
                           else "the frame stopped reporting the laser lock")
            return self.reason
        now = clock.monotonic()
        # LED2 drops transiently (~16 s, measured 2026-08-05): one
        # unlocked frame is not evidence, a cell that STAYS unlocked is
        if not snap.get("cellT_lock", False):
            if self.cellT_unlock_since is None:
                self.cellT_unlock_since = now
            elif now - self.cellT_unlock_since > CELLT_BAD_S:
                self.reason = (
                    (f"cell temperature lock lost for over "
                     f"{CELLT_BAD_S:g} s") if "cellT_lock" in snap
                    else "the frame stopped reporting the cell lock")
                return self.reason
        else:
            self.cellT_unlock_since = None
        err = snap.get("cellT_err", float("nan"))
        if math.isfinite(err) and abs(err) > CELLT_BAD_LEVEL:
            if self.cellT_bad_since is None:
                self.cellT_bad_since = now
            elif now - self.cellT_bad_since > CELLT_BAD_S:
                self.reason = (f"cell temperature error {err:g} out of band "
                               f"for over {CELLT_BAD_S:g} s")
                return self.reason
        else:
            self.cellT_bad_since = None
        return None
