"""Minimal helpers for talking to the Spectrum M4i.9622 DDS card from Python.

This exists to validate the hardware behaviour that the C# driver will rely on,
without needing to build any C#. It deliberately stays close to the raw register
API so that what is proven here maps one-to-one onto the C# P/Invoke layer.

Safety: this module only ever touches the DDS card. It never drives the NI
pattern generators and never fires the experiment. Core amplitudes are clamped
to MAX_CORE_AMP.
"""

import ctypes
import time

# Spectrum low-level driver bindings + register definitions.
from spcm_core.pyspcm import (
    spcm_hOpen,
    spcm_vClose,
    spcm_dwSetParam_i32,
    spcm_dwSetParam_i64,
    spcm_dwSetParam_d64,
    spcm_dwGetParam_i32,
    spcm_dwGetParam_i64,
    spcm_dwGetParam_d64,
    spcm_dwGetErrorInfo_i32,
)
from spcm_core.regs import *  # noqa: F401,F403  -- SPC_* and SPCM_DDS_* constants

DEVICE = b"/dev/spcm0"

#: Safety clamp on the per-core amplitude fraction, agreed with the experimenter.
MAX_CORE_AMP = 0.25

#: Comparator level for the Trg0 input, in mV. DDS_Analog_Trg is TTL, so half
#: way up a 3.3 V swing is a safe default for either 3.3 V or 5 V logic.
DEFAULT_TRIGGER_LEVEL_MV = 1500

#: The only legal routing for four independent outputs on this card. Channel 0
#: can take 47 or 50 cores and nothing else; channels 1-3 take exactly one core
#: each, and only cores 47, 48 and 49 respectively. Illegal masks are silently
#: snapped by the driver rather than rejected, so always read back.
CH0_CORES = (1 << 47) - 1          # cores 0..46
ROUTING_FOUR_CHANNEL = [CH0_CORES, 1 << 47, 1 << 48, 1 << 49]

#: Script channel (DDS1..DDS4) -> DDS core index, given ROUTING_FOUR_CHANNEL.
#: DDS1 is core 0 on Ch0; cores 1..46 also land on Ch0 and must be held at zero
#: amplitude or they contribute to that output.
CORE_FOR_CHANNEL = [0, 47, 48, 49]

CORE_FREQ = [SPC_DDS_CORE0_FREQ + c for c in range(64)]
CORE_AMP = [SPC_DDS_CORE0_AMP + c for c in range(64)]
CORE_PHASE = [SPC_DDS_CORE0_PHASE + c for c in range(64)]
CORE_FREQ_SLOPE = [SPC_DDS_CORE0_FREQ_SLOPE + c for c in range(64)]
CORE_AMP_SLOPE = [SPC_DDS_CORE0_AMP_SLOPE + c for c in range(64)]


class SpcmError(RuntimeError):
    pass


class DDSCard:
    """Context manager around a single open handle to the DDS card."""

    def __init__(self, device=DEVICE):
        self.device = device
        self.handle = None

    # -- lifecycle ---------------------------------------------------------
    def __enter__(self):
        self.handle = spcm_hOpen(ctypes.create_string_buffer(self.device))
        if not self.handle:
            raise SpcmError("could not open %s" % self.device.decode())
        return self

    def __exit__(self, *exc):
        if self.handle:
            spcm_vClose(self.handle)
            self.handle = None
        return False

    # -- register access ---------------------------------------------------
    def _check(self, err, what):
        if err:
            buf = ctypes.create_string_buffer(ERRORTEXTLEN)
            spcm_dwGetErrorInfo_i32(self.handle, None, None, buf)
            raise SpcmError("%s failed (err %d): %s"
                            % (what, err, buf.value.decode(errors="replace").strip()))

    def set_i(self, reg, value):
        self._check(spcm_dwSetParam_i64(self.handle, reg, int(value)), "set_i(%d)" % reg)

    def set_d(self, reg, value):
        self._check(spcm_dwSetParam_d64(self.handle, reg, float(value)), "set_d(%d)" % reg)

    def get_i32(self, reg):
        v = ctypes.c_int32(0)
        self._check(spcm_dwGetParam_i32(self.handle, reg, ctypes.byref(v)), "get_i32(%d)" % reg)
        return v.value

    def get_i(self, reg):
        v = ctypes.c_int64(0)
        self._check(spcm_dwGetParam_i64(self.handle, reg, ctypes.byref(v)), "get_i(%d)" % reg)
        return v.value

    def get_d(self, reg):
        v = ctypes.c_double(0)
        self._check(spcm_dwGetParam_d64(self.handle, reg, ctypes.byref(v)), "get_d(%d)" % reg)
        return v.value

    # -- card setup --------------------------------------------------------
    def configure_dds(self, out_level_mv=None, enable_outputs=False,
                      trigger_level_mv=DEFAULT_TRIGGER_LEVEL_MV):
        """Put the card into DDS mode with all four channels enabled.

        Output stages stay disabled unless ``enable_outputs`` is set, so this is
        safe to call just to read capability registers.
        """
        self.set_i(SPC_CARDMODE, SPC_REP_STD_DDS)
        self.set_i(SPC_CHENABLE, 0xF)
        self.set_i(SPC_CLOCKMODE, SPC_CM_INTPLL)
        for ch in range(4):
            self.set_i(SPC_ENABLEOUT0 + ch * (SPC_ENABLEOUT1 - SPC_ENABLEOUT0),
                       1 if enable_outputs else 0)
            if out_level_mv is not None:
                self.set_i(SPC_AMP0 + ch * (SPC_AMP1 - SPC_AMP0), int(out_level_mv))
            self.set_i(SPC_FILTER0 + ch * (SPC_FILTER1 - SPC_FILTER0), 0)
        self.configure_card_trigger(level_mv=trigger_level_mv)
        self.set_i(SPC_M2CMD, M2CMD_CARD_WRITESETUP)

    def configure_card_trigger(self, level_mv=DEFAULT_TRIGGER_LEVEL_MV, term=0):
        """Arm the card's trigger engine on a rising edge of Trg0.

        SPCM_DDS_TRG_SRC_CARD delegates to the card's ordinary trigger logic,
        which chapter 11 says defaults to SPC_TMASK_SOFTWARE. On this card it
        actually defaults to SPC_TMASK_NONE, and SPC_TMASK_SOFTWARE is useless
        here anyway: it fires the moment the card starts and burns straight
        through the queued blocks (observed: three steps then QUEUE_UNDERRUN).
        Trg0 is where DDS_Analog_Trg lands, so a positive edge is what we want.

        M2CMD_CARD_FORCETRIGGER works with this configuration and can be issued
        repeatedly -- each force advances the engine one step -- so bench tests
        can stand in for the digital pulse without touching the NI boards.
        """
        self.set_i(SPC_TRIG_EXT0_MODE, SPC_TM_POS)
        self.set_i(SPC_TRIG_EXT0_LEVEL0, int(level_mv))
        self.set_i(SPC_TRIG_TERM, term)
        self.set_i(SPC_TRIG_ORMASK, SPC_TMASK_EXT0)
        self.set_i(SPC_TRIG_ANDMASK, SPC_TMASK_NONE)

    def route_four_channels(self):
        """Route one core to each of the four outputs; verify by read-back.

        Returns the read-back masks. Raises if the driver snapped the request to
        something other than what we asked for.
        """
        regs = [SPC_DDS_CORES_ON_CH0, SPC_DDS_CORES_ON_CH1,
                SPC_DDS_CORES_ON_CH2, SPC_DDS_CORES_ON_CH3]
        for reg, mask in zip(regs, ROUTING_FOUR_CHANNEL):
            self.set_i(reg, mask)
        got = [self.get_i(reg) for reg in regs]
        if got != ROUTING_FOUR_CHANNEL:
            raise SpcmError(
                "routing was snapped by the driver: asked %s, got %s"
                % ([hex(m) for m in ROUTING_FOUR_CHANNEL], [hex(m) for m in got]))
        return got

    def start(self):
        self.set_i(SPC_M2CMD, M2CMD_CARD_START | M2CMD_CARD_ENABLETRIGGER)

    def stop(self):
        self.set_i(SPC_M2CMD, M2CMD_CARD_STOP)

    def force_trigger(self):
        self.set_i(SPC_M2CMD, M2CMD_CARD_FORCETRIGGER)

    def reset_dds(self):
        self.set_i(SPC_DDS_CMD, SPCM_DDS_CMD_RESET)

    # -- DDS command queue -------------------------------------------------
    def prime(self, force=True):
        """Write the first command block, which must be an EXEC_AT_TRG one.

        Undocumented precondition, established empirically (see debug_setup.py):
        SPCM_DDS_CMD_EXEC_NOW is rejected with err 267 "the setup isn't valid"
        until at least one EXEC_AT_TRG block has been flushed to the card after
        M2CMD_CARD_START. FORCETRIGGER is not required to satisfy it, but is
        needed if you want the primed block to actually go live.

        Note that forcing leaves the queue empty, so the card reads back "idle"
        rather than WAITING_FOR_TRG until the next pattern is flushed. That is
        expected -- see the ``armed`` property.
        """
        self.exec_at_trg()
        self.write_to_card()
        if force:
            self.force_trigger()

    def exec_now(self):
        self.set_i(SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_NOW)

    def exec_at_trg(self):
        self.set_i(SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_AT_TRG)

    def write_to_card(self):
        self.set_i(SPC_DDS_CMD, SPCM_DDS_CMD_WRITE_TO_CARD)

    def set_core(self, core, freq=None, amp=None, phase=None,
                 freq_slope=None, amp_slope=None):
        """Queue a change to one DDS core. Amplitude is clamped for safety."""
        if freq is not None:
            self.set_d(CORE_FREQ[core], freq)
        if amp is not None:
            if abs(amp) > MAX_CORE_AMP:
                raise ValueError("core %d amplitude %g exceeds MAX_CORE_AMP %g"
                                 % (core, amp, MAX_CORE_AMP))
            self.set_d(CORE_AMP[core], amp)
        if phase is not None:
            self.set_d(CORE_PHASE[core], phase)
        if freq_slope is not None:
            self.set_d(CORE_FREQ_SLOPE[core], freq_slope)
        if amp_slope is not None:
            self.set_d(CORE_AMP_SLOPE[core], amp_slope)

    def silence_all_cores(self):
        """Zero every core's amplitude and slopes.

        Necessary because cores 1..46 are summed onto channel 0 along with
        core 0, so anything left over from a previous run leaks into the output.
        """
        for core in range(self.num_cores):
            self.set_d(CORE_AMP[core], 0.0)
            self.set_d(CORE_AMP_SLOPE[core], 0.0)
            self.set_d(CORE_FREQ_SLOPE[core], 0.0)

    # -- introspection -----------------------------------------------------
    @property
    def num_cores(self):
        return self.get_i32(SPC_DDS_NUM_CORES)

    @property
    def trg_count(self):
        return self.get_i32(SPC_DDS_TRG_COUNT)

    @property
    def status(self):
        return self.get_i32(SPC_DDS_STATUS)

    @property
    def armed(self):
        """True when a queued command block is waiting for its trigger.

        SPCM_DDS_STAT_WAITING_FOR_TRG tracks the *queue*, not the trigger
        engine: with the queue empty the card reads back "idle" even though the
        trigger logic is perfectly happy to accept an edge. So this only means
        anything after a WRITE_TO_CARD, and it drops back to false as soon as
        the last queued block has been consumed.
        """
        return bool(self.status & SPCM_DDS_STAT_WAITING_FOR_TRG)

    def status_text(self):
        s = self.status
        flags = []
        if s & SPCM_DDS_STAT_WAITING_FOR_TRG:
            flags.append("WAITING_FOR_TRG")
        if s & SPCM_DDS_STAT_QUEUE_UNDERRUN:
            flags.append("QUEUE_UNDERRUN")
        if s & SPCM_DDS_STAT_QUEUE_OVERRUN:
            flags.append("QUEUE_OVERRUN")
        return "|".join(flags) if flags else "idle"

    def queue_fill(self):
        return self.get_i32(SPC_DDS_QUEUE_CMD_COUNT), self.get_i32(SPC_DDS_QUEUE_CMD_MAX)

    def set_trg_timer(self, seconds):
        """Set the DDS step timer, returning the value the card actually took."""
        self.set_d(SPC_DDS_TRG_TIMER, seconds)
        return self.get_d(SPC_DDS_TRG_TIMER)

    def capabilities(self):
        return {
            "num_cores": self.num_cores,
            "queue_cmd_max": self.get_i32(SPC_DDS_QUEUE_CMD_MAX),
            "freq_min": self.get_d(SPC_DDS_AVAIL_FREQ_MIN),
            "freq_max": self.get_d(SPC_DDS_AVAIL_FREQ_MAX),
            "freq_step": self.get_d(SPC_DDS_AVAIL_FREQ_STEP),
            "amp_min": self.get_d(SPC_DDS_AVAIL_AMP_MIN),
            "amp_max": self.get_d(SPC_DDS_AVAIL_AMP_MAX),
            "amp_step": self.get_d(SPC_DDS_AVAIL_AMP_STEP),
            "freq_slope_min": self.get_d(SPC_DDS_AVAIL_FREQ_SLOPE_MIN),
            "freq_slope_max": self.get_d(SPC_DDS_AVAIL_FREQ_SLOPE_MAX),
            "freq_slope_step": self.get_d(SPC_DDS_AVAIL_FREQ_SLOPE_STEP),
            "amp_slope_min": self.get_d(SPC_DDS_AVAIL_AMP_SLOPE_MIN),
            "amp_slope_max": self.get_d(SPC_DDS_AVAIL_AMP_SLOPE_MAX),
            "amp_slope_step": self.get_d(SPC_DDS_AVAIL_AMP_SLOPE_STEP),
        }

    def identity(self):
        return {
            "pcityp": self.get_i32(SPC_PCITYP),
            "serial": self.get_i32(SPC_PCISERIALNO),
            "extfeatures": self.get_i32(SPC_PCIEXTFEATURES),
            "availcardmodes": self.get_i32(SPC_AVAILCARDMODES),
            "channels": self.get_i32(SPC_MIINST_MODULES) * self.get_i32(SPC_MIINST_CHPERMODULE),
            "samplerate": self.get_i(SPC_SAMPLERATE),
        }

    # -- observation -------------------------------------------------------
    def watch_triggers(self, count, timeout):
        """Poll SPC_DDS_TRG_COUNT and timestamp each increment.

        Returns a list of (trigger_index, elapsed_seconds) for the first
        ``count`` increments after the call. Software polling resolves to well
        under a millisecond here, which is ample for distinguishing the tens- to
        hundreds-of-milliseconds steps we care about.
        """
        start = time.perf_counter()
        base = self.trg_count
        seen = base
        events = []
        while len(events) < count and (time.perf_counter() - start) < timeout:
            now = self.trg_count
            if now != seen:
                t = time.perf_counter() - start
                for i in range(seen + 1, now + 1):
                    events.append((i - base, t))
                seen = now
        return events
