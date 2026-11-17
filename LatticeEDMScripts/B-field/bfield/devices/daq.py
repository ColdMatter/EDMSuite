"""NI DAQ analog input: one task, all channels differential, one shared
hardware clock.

Moved from qzfm_common.py's DAQ layer. nidaqmx and numpy import inside
the methods, so importing this module works on a machine with no NI
driver. The simulated lab swaps in bfield.devices.simulated.SimUSB6008,
which has the same methods.

Which card it is (inputs, ranges, rate) comes from bfield.daq_models
([daq] model in config/lab.toml). On the lab PC the PCIe-6321 (8
differential inputs, 250 kS/s aggregate, from 2026-10-20) is Dev3; the
USB-6008 (4 differential inputs, 10 kS/s, every run before) was Dev2.
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


class NIDaq:
    """The real card through nidaqmx."""

    name = "nidaqmx"

    def __init__(self, model=None):
        from bfield.daq_models import active
        self.model = model or active()
        self.task = None

    def list_devices(self):
        nidaqmx = import_nidaqmx()
        try:
            return list(nidaqmx.system.System.local().devices.device_names)
        except Exception as e:
            raise DaqSetupError(
                f"NI driver cannot list devices ({type(e).__name__}: {e}) -- "
                f"check the {self.model.title} is connected and no other "
                "program holds it, retry")

    def resolve_device(self, requested, hardware_check=False):
        if hardware_check:
            return requested        # the self-test enumerates devices itself
        devices = self.list_devices()
        if not devices:
            raise DaqDeviceNotFound(
                f"no NI DAQ device found (is the {self.model.title} "
                "connected?)")
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
                f"DAQ setup failed on {channels}: {e} -- check the "
                f"{self.model.title} and that no other recorder or NI test "
                "panel holds the device, retry")
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
                f"the {self.model.title} and that no other recorder holds "
                "the device, retry")
        while True:
            try:
                reader.read_many_sample(
                    buf, number_of_samples_per_channel=block_n,
                    timeout=timeout)
            except Exception as e:
                raise DaqReadError(
                    f"DAQ read failed/timed out: {e} -- if this repeats, "
                    f"check the {self.model.title} and retry")
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


USB6008 = NIDaq     # the old name
