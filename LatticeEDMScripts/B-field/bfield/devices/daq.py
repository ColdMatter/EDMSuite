"""NI USB-6008 analog input: one task, all channels differential, one
shared hardware clock.

Moved from qzfm_common.py's DAQ layer. nidaqmx and numpy import inside
the methods, so importing this module works on a machine with no NI
driver. The simulated lab swaps in bfield.devices.simulated.SimUSB6008,
which has the same methods.

The USB-6008 appears as Dev2 on the lab PC. It is bus powered, has four
differential inputs (ai0..ai3) and a 10 kS/s aggregate rate.
"""

from __future__ import annotations

import contextlib

from bfield.errors import DaqDeviceNotFound, DaqReadError, DaqSetupError


def import_nidaqmx():
    try:
        import nidaqmx
        return nidaqmx
    except Exception as e:
        raise DaqSetupError(
            f"nidaqmx unavailable ({e}). Needs Windows + NI-DAQmx driver + "
            f"`pip install nidaqmx`.")


class USB6008:
    """The real card through nidaqmx."""

    name = "nidaqmx"

    def __init__(self):
        self.task = None

    def list_devices(self):
        nidaqmx = import_nidaqmx()
        try:
            return list(nidaqmx.system.System.local().devices.device_names)
        except Exception as e:
            raise DaqSetupError(
                f"NI driver cannot list devices ({type(e).__name__}: {e}) -- "
                "replug the USB-6008, check no other program holds it, retry")

    def resolve_device(self, requested, hardware_check=False):
        if hardware_check:
            return requested        # the self-test enumerates devices itself
        devices = self.list_devices()
        if not devices:
            raise DaqDeviceNotFound(
                "no NI DAQ device found (is the USB-6008 plugged in?)")
        if not requested and len(devices) > 1:
            # guessing devices[0] can record from the spare card
            raise DaqSetupError(
                f"{len(devices)} NI devices present {devices} and no device "
                f"given -- set [daq] device in config/lab.toml")
        device = requested or devices[0]
        if device not in devices:
            raise DaqDeviceNotFound(
                f"device {device!r} not found; available: {devices}")
        return device

    def open(self, channels, vrange, fs):
        """Create the continuous task (not started)."""
        nidaqmx = import_nidaqmx()
        task = None
        try:
            from nidaqmx.constants import (AcquisitionType,
                                           TerminalConfiguration)
            task = nidaqmx.Task()
            for phys in channels:
                task.ai_channels.add_ai_voltage_chan(
                    phys, terminal_config=TerminalConfiguration.DIFF,
                    min_val=-vrange, max_val=vrange)
            task.timing.cfg_samp_clk_timing(
                rate=fs, sample_mode=AcquisitionType.CONTINUOUS,
                samps_per_chan=int(fs) * 10)
        except Exception as e:
            if task is not None:
                with contextlib.suppress(Exception):
                    task.close()
            raise DaqSetupError(
                f"DAQ setup failed on {channels}: {e} -- check USB and that "
                "no other recorder or NI test panel holds the device, retry")
        self.task = task
        return self

    def blocks(self, n_ch, block_n, timeout=10.0):
        """Start the task and yield (n_ch, block_n) arrays forever."""
        # setup is 11, not 12 -- nothing has been read yet
        try:
            import numpy as np
            from nidaqmx.stream_readers import AnalogMultiChannelReader
            reader = AnalogMultiChannelReader(self.task.in_stream)
            buf = np.zeros((n_ch, block_n))
            self.task.start()
        except Exception as e:
            raise DaqSetupError(
                f"DAQ task would not start ({type(e).__name__}: {e}) -- check "
                "USB and that no other recorder holds the device, retry")
        while True:
            try:
                reader.read_many_sample(
                    buf, number_of_samples_per_channel=block_n,
                    timeout=timeout)
            except Exception as e:
                raise DaqReadError(
                    f"DAQ read failed/timed out: {e} -- if this repeats, "
                    "replug the USB-6008 and retry")
            yield buf.copy()

    def finite_read(self, channels, seconds, vrange=1.0, fs=1000.0):
        """A one-shot finite read (hardware check): (n_ch, n) array."""
        import numpy as np
        nidaqmx = import_nidaqmx()
        from nidaqmx.constants import AcquisitionType, TerminalConfiguration
        n = max(int(fs * seconds), 1)
        with nidaqmx.Task() as task:
            for phys in channels:
                task.ai_channels.add_ai_voltage_chan(
                    phys, terminal_config=TerminalConfiguration.DIFF,
                    min_val=-vrange, max_val=vrange)
            task.timing.cfg_samp_clk_timing(
                rate=fs, sample_mode=AcquisitionType.FINITE, samps_per_chan=n)
            return np.atleast_2d(
                np.asarray(task.read(number_of_samples_per_channel=n)))

    def close(self):
        if self.task is not None:
            for release in (self.task.stop, self.task.close):
                with contextlib.suppress(BaseException):
                    release()
        self.task = None
