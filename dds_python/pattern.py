"""Prototype of the DDS pattern model and compiler.

This is the Python twin of the C# `DDSPattern` / `DDSPatternCompiler`. Proving
the command sequence here means the C# only has to reproduce a known-good order
of register writes.

Timing model (confirmed by timing_model.py): parameters in a command block go
live at the trigger that ends the block, and the engine then waits for the next
trigger using those now-live registers. So block k carries the trigger source
and timer that govern the wait for event k+1.
"""

from spcm_core.regs import (
    SPC_DDS_TRG_SRC,
    SPCM_DDS_TRG_SRC_CARD,
    SPCM_DDS_TRG_SRC_TIMER,
    SPC_DDS_X_MANUAL_OUTPUT,
)

from spectrum_dds import CORE_FOR_CHANNEL, MAX_CORE_AMP

#: SPC_DDS_TRG_TIMER accepted range, bisected on the card (timer_range.py).
TIMER_MIN = 83.2e-9
TIMER_MAX = 27.487790730

N_CHANNELS = 4


class ChannelState:
    """Per-output settings at one event. SI units throughout."""

    __slots__ = ("freq", "amp", "freq_slope", "amp_slope")

    def __init__(self, freq=0.0, amp=0.0, freq_slope=0.0, amp_slope=0.0):
        self.freq = freq              # Hz
        self.amp = amp                # fraction of output level, 0..1
        self.freq_slope = freq_slope  # Hz/s
        self.amp_slope = amp_slope    # 1/s

    def __repr__(self):
        return ("ChannelState(freq=%.6g Hz, amp=%.4g, df=%.4g Hz/s, da=%.4g /s)"
                % (self.freq, self.amp, self.freq_slope, self.amp_slope))


class Event:
    """One step of a pattern: a time, four channel states, and an XIO mask."""

    def __init__(self, name, time_s, channels, xio=0):
        if len(channels) != N_CHANNELS:
            raise ValueError("expected %d channel states, got %d"
                             % (N_CHANNELS, len(channels)))
        self.name = name
        self.time_s = time_s
        self.channels = channels
        self.xio = xio

    def __repr__(self):
        return "Event(%r, t=%.6g s)" % (self.name, self.time_s)


class Pattern:
    """An ordered set of events, plus the conversions to and from the format
    MOTMaster scripts use."""

    def __init__(self, events=None):
        self.events = sorted(events or [], key=lambda e: e.time_s)

    def __len__(self):
        return len(self.events)

    # -- legacy MOTMaster script format ------------------------------------
    #
    # Dictionary<string, List<List<double>>> where the value is exactly five
    # lists: [time_ms], [f1..f4 MHz], [a1..a4], [df1..df4 MHz/ms], [da1..da4 /ms].
    # Events are unordered in the dictionary and must be sorted on time.

    @classmethod
    def from_legacy(cls, d):
        events = []
        for name, rows in d.items():
            if len(rows) != 5:
                raise ValueError("event %r: expected 5 rows, got %d" % (name, len(rows)))
            time_row, freqs, amps, fslopes, aslopes = rows
            for label, row in (("freq", freqs), ("amp", amps),
                               ("freq slope", fslopes), ("amp slope", aslopes)):
                if len(row) != N_CHANNELS:
                    raise ValueError("event %r: %s row has %d entries, expected %d"
                                     % (name, label, len(row), N_CHANNELS))
            channels = [
                ChannelState(freq=freqs[i] * 1e6,
                             amp=amps[i],
                             freq_slope=fslopes[i] * 1e9,   # MHz/ms -> Hz/s
                             amp_slope=aslopes[i] * 1e3)    # 1/ms   -> 1/s
                for i in range(N_CHANNELS)
            ]
            events.append(Event(name, time_row[0] / 1000.0, channels))
        return cls(events)

    def to_legacy(self):
        d = {}
        for e in self.events:
            d[e.name] = [
                [e.time_s * 1000.0],
                [c.freq / 1e6 for c in e.channels],
                [c.amp for c in e.channels],
                [c.freq_slope / 1e9 for c in e.channels],
                [c.amp_slope / 1e3 for c in e.channels],
            ]
        return d

    # -- validation --------------------------------------------------------
    def validate(self, caps, queue_max, max_amp=MAX_CORE_AMP):
        """Check the pattern against the card's capability registers.

        Returns a list of human-readable problems; empty means it will program.
        """
        problems = []
        if not self.events:
            return ["pattern is empty"]

        times = [e.time_s for e in self.events]
        if len(set(times)) != len(times):
            problems.append("two events share a time; ordering would be ambiguous")

        for a, b in zip(self.events, self.events[1:]):
            gap = b.time_s - a.time_s
            if gap < TIMER_MIN:
                problems.append(
                    "gap %r -> %r is %.3g s, below the %.3g s timer minimum"
                    % (a.name, b.name, gap, TIMER_MIN))
            elif gap > TIMER_MAX:
                problems.append(
                    "gap %r -> %r is %.3g s, above the %.3g s timer maximum"
                    % (a.name, b.name, gap, TIMER_MAX))

        for e in self.events:
            for ch, c in enumerate(e.channels):
                if not (caps["freq_min"] <= c.freq <= caps["freq_max"]):
                    problems.append("%r ch%d frequency %.6g Hz out of range"
                                    % (e.name, ch + 1, c.freq))
                if abs(c.amp) > max_amp:
                    problems.append("%r ch%d amplitude %.4g exceeds the %.4g safety limit"
                                    % (e.name, ch + 1, c.amp, max_amp))
                if not (caps["freq_slope_min"] <= c.freq_slope <= caps["freq_slope_max"]):
                    problems.append("%r ch%d frequency slope %.6g Hz/s out of range"
                                    % (e.name, ch + 1, c.freq_slope))
                if not (caps["amp_slope_min"] <= c.amp_slope <= caps["amp_slope_max"]):
                    problems.append("%r ch%d amplitude slope %.6g /s out of range"
                                    % (e.name, ch + 1, c.amp_slope))

        needed = self.command_estimate()
        if needed > queue_max:
            problems.append("pattern needs ~%d commands, queue holds %d"
                            % (needed, queue_max))
        return problems

    def command_estimate(self):
        # 4 params x 4 channels, plus xio, trg_src, timer and the exec command.
        return len(self.events) * (4 * N_CHANNELS + 4)

    # -- timeline for plotting --------------------------------------------
    def timeline(self, channel):
        """(times, freqs, amps) sample points for one channel, with ramps
        expanded into their start and end values so a line plot shows the real
        slope rather than a step."""
        ts, fs, amps = [], [], []
        for i, e in enumerate(self.events):
            c = e.channels[channel]
            ts.append(e.time_s)
            fs.append(c.freq)
            amps.append(c.amp)
            if i + 1 < len(self.events):
                dt = self.events[i + 1].time_s - e.time_s
                ts.append(self.events[i + 1].time_s)
                fs.append(c.freq + c.freq_slope * dt)
                amps.append(c.amp + c.amp_slope * dt)
        return ts, fs, amps


def compile_to_card(card, pattern, rearm=True):
    """Queue ``pattern`` onto the card. Does not flush; call write_to_card().

    The card must already be started and primed. Block k carries the timing that
    governs the wait for event k+1; the final block hands back to the external
    card trigger so the next shot starts on the next digital pulse.
    """
    events = pattern.events
    for k, e in enumerate(events):
        for ch, c in enumerate(e.channels):
            core = CORE_FOR_CHANNEL[ch]
            card.set_core(core, freq=c.freq, amp=c.amp,
                          freq_slope=c.freq_slope, amp_slope=c.amp_slope)
        card.set_i(SPC_DDS_X_MANUAL_OUTPUT, e.xio)

        if k + 1 < len(events):
            card.set_i(SPC_DDS_TRG_SRC, SPCM_DDS_TRG_SRC_TIMER)
            card.set_trg_timer(events[k + 1].time_s - e.time_s)
        elif rearm:
            card.set_i(SPC_DDS_TRG_SRC, SPCM_DDS_TRG_SRC_CARD)
        card.exec_at_trg()
