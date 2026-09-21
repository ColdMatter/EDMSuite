using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SpectrumDDS
{
    /// <summary>What one output is doing at one event. SI units throughout.</summary>
    [Serializable]
    public class DDSChannelState
    {
        /// <summary>Hz.</summary>
        public double Frequency { get; set; }
        /// <summary>Fraction of the channel's output level, 0 to 1.</summary>
        public double Amplitude { get; set; }
        /// <summary>Hz per second.</summary>
        public double FrequencySlope { get; set; }
        /// <summary>Amplitude fraction per second.</summary>
        public double AmplitudeSlope { get; set; }

        public DDSChannelState() { }

        public DDSChannelState(double frequency, double amplitude,
                               double frequencySlope = 0.0, double amplitudeSlope = 0.0)
        {
            Frequency = frequency;
            Amplitude = amplitude;
            FrequencySlope = frequencySlope;
            AmplitudeSlope = amplitudeSlope;
        }

        public DDSChannelState Clone()
        {
            return new DDSChannelState(Frequency, Amplitude, FrequencySlope, AmplitudeSlope);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0:g6} MHz, amp {1:g4}, df {2:g4} MHz/ms, da {3:g4} /ms",
                Frequency / 1e6, Amplitude, FrequencySlope / 1e9, AmplitudeSlope / 1e3);
        }
    }

    /// <summary>One step of a pattern: a time, four channel states and an XIO mask.</summary>
    [Serializable]
    public class DDSEvent
    {
        public string Name { get; set; }
        /// <summary>Seconds after the trigger on DDS_Analog_Trg.</summary>
        public double Time { get; set; }
        public DDSChannelState[] Channels { get; set; }
        /// <summary>Bit mask over the three multi-purpose outputs X0, X1, X2.</summary>
        public int Xio { get; set; }

        public DDSEvent()
        {
            Channels = NewChannels();
        }

        public DDSEvent(string name, double time, DDSChannelState[] channels, int xio = 0)
        {
            if (channels == null || channels.Length != DDSPattern.ChannelCount)
                throw new ArgumentException(string.Format(
                    "expected {0} channel states", DDSPattern.ChannelCount), "channels");
            Name = name;
            Time = time;
            Channels = channels;
            Xio = xio;
        }

        public static DDSChannelState[] NewChannels()
        {
            DDSChannelState[] channels = new DDSChannelState[DDSPattern.ChannelCount];
            for (int i = 0; i < channels.Length; i++) channels[i] = new DDSChannelState();
            return channels;
        }

        public DDSEvent Clone()
        {
            return new DDSEvent(Name, Time, Channels.Select(c => c.Clone()).ToArray(), Xio);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} @ {1:g6} ms", Name, Time * 1e3);
        }
    }

    /// <summary>
    /// An ordered set of DDS events, plus conversion to and from the dictionary
    /// format MOTMaster scripts have always used.
    /// </summary>
    [Serializable]
    public class DDSPattern
    {
        public const int ChannelCount = 4;

        /// <summary>
        /// Accepted range of SPC_DDS_TRG_TIMER, bisected on the card. The card
        /// rejects values outside this rather than clamping them, so the compiler
        /// checks gaps against it before programming anything.
        /// </summary>
        public const double TimerMinimum = 83.2e-9;
        public const double TimerMaximum = 27.487790730;

        private readonly List<DDSEvent> events = new List<DDSEvent>();

        /// <summary>Events in time order. Sorting happens on insertion.</summary>
        public IList<DDSEvent> Events { get { return events.AsReadOnly(); } }

        public int Count { get { return events.Count; } }

        public DDSEvent this[int index] { get { return events[index]; } }

        public DDSPattern() { }

        public DDSPattern(IEnumerable<DDSEvent> source)
        {
            foreach (DDSEvent e in source) events.Add(e);
            Sort();
        }

        public void Add(DDSEvent e)
        {
            events.Add(e);
            Sort();
        }

        public void Remove(DDSEvent e)
        {
            events.Remove(e);
        }

        public void Clear()
        {
            events.Clear();
        }

        /// <summary>Re-sort after times have been edited in place.</summary>
        public void Sort()
        {
            events.Sort((a, b) => a.Time.CompareTo(b.Time));
        }

        public DDSPattern Clone()
        {
            return new DDSPattern(events.Select(e => e.Clone()));
        }

        /// <summary>Total pattern duration, first event to last, in seconds.</summary>
        public double Span
        {
            get { return events.Count < 2 ? 0.0 : events[events.Count - 1].Time - events[0].Time; }
        }

        // -- the MOTMaster script format ---------------------------------------
        //
        // Dictionary<string, List<List<double>>>: the key is an arbitrary unique
        // event label and the value is exactly five lists --
        //   [0] time       1 entry,  milliseconds after the DDS trigger
        //   [1] frequency  4 entries, MHz
        //   [2] amplitude  4 entries, fraction 0-1
        //   [3] freq slope 4 entries, MHz/ms
        //   [4] amp slope  4 entries, per ms
        // Events are unordered in the dictionary, so they get sorted on time here.
        //
        // This stays the wire format between scripts and the driver so that none
        // of the ~130 existing scripts has to change.

        public const double MillisecondsPerSecond = 1e3;
        public const double HzPerMHz = 1e6;
        /// <summary>MHz/ms to Hz/s.</summary>
        public const double HzPerSecondPerMHzPerMs = 1e9;
        /// <summary>Per-ms to per-second.</summary>
        public const double PerSecondPerPerMs = 1e3;

        public static DDSPattern FromLegacyDictionary(Dictionary<string, List<List<double>>> source)
        {
            DDSPattern pattern = new DDSPattern();
            if (source == null) return pattern;

            foreach (KeyValuePair<string, List<List<double>>> entry in source)
            {
                List<List<double>> rows = entry.Value;
                if (rows == null || rows.Count != 5)
                    throw new ArgumentException(string.Format(
                        "DDS event \"{0}\": expected 5 rows (time, frequency, amplitude, " +
                        "frequency slope, amplitude slope), got {1}",
                        entry.Key, rows == null ? 0 : rows.Count));
                if (rows[0].Count < 1)
                    throw new ArgumentException(string.Format(
                        "DDS event \"{0}\": the time row is empty", entry.Key));
                for (int row = 1; row < 5; row++)
                    if (rows[row].Count != ChannelCount)
                        throw new ArgumentException(string.Format(
                            "DDS event \"{0}\": row {1} has {2} entries, expected {3}",
                            entry.Key, row, rows[row].Count, ChannelCount));

                DDSChannelState[] channels = new DDSChannelState[ChannelCount];
                for (int i = 0; i < ChannelCount; i++)
                    channels[i] = new DDSChannelState(
                        rows[1][i] * HzPerMHz,
                        rows[2][i],
                        rows[3][i] * HzPerSecondPerMHzPerMs,
                        rows[4][i] * PerSecondPerPerMs);

                pattern.events.Add(new DDSEvent(
                    entry.Key, rows[0][0] / MillisecondsPerSecond, channels));
            }
            pattern.Sort();
            return pattern;
        }

        public Dictionary<string, List<List<double>>> ToLegacyDictionary()
        {
            Dictionary<string, List<List<double>>> result =
                new Dictionary<string, List<List<double>>>();
            foreach (DDSEvent e in events)
            {
                result[e.Name] = new List<List<double>>
                {
                    new List<double> { e.Time * MillisecondsPerSecond },
                    e.Channels.Select(c => c.Frequency / HzPerMHz).ToList(),
                    e.Channels.Select(c => c.Amplitude).ToList(),
                    e.Channels.Select(c => c.FrequencySlope / HzPerSecondPerMHzPerMs).ToList(),
                    e.Channels.Select(c => c.AmplitudeSlope / PerSecondPerPerMs).ToList(),
                };
            }
            return result;
        }

        // -- validation ----------------------------------------------------------

        /// <summary>
        /// Check the pattern against the card's capability registers and the
        /// safety amplitude clamps. Returns a list of human-readable problems;
        /// empty means it will program.
        /// </summary>
        /// <param name="maximumAmplitudes">
        /// One clamp per channel, indexed DDS1 to DDS4.
        /// </param>
        public List<string> Validate(DDSCapabilities capabilities, double[] maximumAmplitudes)
        {
            if (maximumAmplitudes == null || maximumAmplitudes.Length != ChannelCount)
                throw new ArgumentException(string.Format(
                    "expected {0} amplitude clamps, one per channel", ChannelCount),
                    "maximumAmplitudes");

            List<string> problems = new List<string>();
            if (events.Count == 0)
            {
                problems.Add("the pattern is empty");
                return problems;
            }

            for (int k = 0; k + 1 < events.Count; k++)
            {
                double gap = events[k + 1].Time - events[k].Time;
                if (gap <= 0.0)
                    problems.Add(string.Format(CultureInfo.InvariantCulture,
                        "\"{0}\" and \"{1}\" share the time {2:g6} ms, so their order is ambiguous",
                        events[k].Name, events[k + 1].Name, events[k].Time * 1e3));
                else if (gap < TimerMinimum)
                    problems.Add(string.Format(CultureInfo.InvariantCulture,
                        "the gap from \"{0}\" to \"{1}\" is {2:g4} s, below the card's {3:g4} s timer minimum",
                        events[k].Name, events[k + 1].Name, gap, TimerMinimum));
                else if (gap > TimerMaximum)
                    problems.Add(string.Format(CultureInfo.InvariantCulture,
                        "the gap from \"{0}\" to \"{1}\" is {2:g4} s, above the card's {3:g4} s timer maximum",
                        events[k].Name, events[k + 1].Name, gap, TimerMaximum));
            }

            foreach (DDSEvent e in events)
            {
                for (int ch = 0; ch < ChannelCount; ch++)
                {
                    DDSChannelState c = e.Channels[ch];
                    string where = string.Format("\"{0}\" DDS{1}", e.Name, ch + 1);

                    if (c.Frequency < capabilities.FrequencyMinimum ||
                        c.Frequency > capabilities.FrequencyMaximum)
                        problems.Add(string.Format(CultureInfo.InvariantCulture,
                            "{0}: frequency {1:g6} MHz is outside the card's {2:g4} to {3:g6} MHz",
                            where, c.Frequency / 1e6,
                            capabilities.FrequencyMinimum / 1e6, capabilities.FrequencyMaximum / 1e6));

                    if (Math.Abs(c.Amplitude) > maximumAmplitudes[ch])
                        problems.Add(string.Format(CultureInfo.InvariantCulture,
                            "{0}: amplitude {1:g4} exceeds its {2:g4} safety clamp",
                            where, c.Amplitude, maximumAmplitudes[ch]));

                    if (c.FrequencySlope < capabilities.FrequencySlopeMinimum ||
                        c.FrequencySlope > capabilities.FrequencySlopeMaximum)
                        problems.Add(string.Format(CultureInfo.InvariantCulture,
                            "{0}: frequency slope {1:g6} MHz/ms is outside the card's range",
                            where, c.FrequencySlope / 1e9));

                    if (c.AmplitudeSlope < capabilities.AmplitudeSlopeMinimum ||
                        c.AmplitudeSlope > capabilities.AmplitudeSlopeMaximum)
                        problems.Add(string.Format(CultureInfo.InvariantCulture,
                            "{0}: amplitude slope {1:g6} /ms is outside the card's range",
                            where, c.AmplitudeSlope / 1e3));
                }

                if ((e.Xio & ~0x7) != 0)
                    problems.Add(string.Format(
                        "\"{0}\": XIO mask 0x{1:X} has bits outside X0/X1/X2", e.Name, e.Xio));
            }

            int needed = CommandCount();
            if (needed > capabilities.QueueCommandMaximum)
                problems.Add(string.Format(
                    "the pattern needs {0} commands but the card's queue holds {1}",
                    needed, capabilities.QueueCommandMaximum));

            return problems;
        }

        /// <summary>
        /// Number of queue slots the compiled pattern occupies, which is just the
        /// number of register writes: four parameters per channel, the XIO mask
        /// and the trigger source in every block, plus a timer in all but the last.
        /// </summary>
        public int CommandCount()
        {
            if (events.Count == 0) return 0;
            const int perBlock = 4 * ChannelCount + 2;
            return events.Count * perBlock + (events.Count - 1);
        }

        // -- plotting -------------------------------------------------------------

        /// <summary>
        /// Sample points for one channel, with ramps expanded into their start and
        /// end values so a line plot shows the real slope rather than a step.
        /// </summary>
        public void Timeline(int channel, out double[] times, out double[] frequencies,
                             out double[] amplitudes)
        {
            List<double> t = new List<double>();
            List<double> f = new List<double>();
            List<double> a = new List<double>();

            for (int i = 0; i < events.Count; i++)
            {
                DDSChannelState c = events[i].Channels[channel];
                t.Add(events[i].Time);
                f.Add(c.Frequency);
                a.Add(c.Amplitude);

                if (i + 1 < events.Count)
                {
                    double dt = events[i + 1].Time - events[i].Time;
                    t.Add(events[i + 1].Time);
                    f.Add(c.Frequency + c.FrequencySlope * dt);
                    a.Add(c.Amplitude + c.AmplitudeSlope * dt);
                }
            }

            times = t.ToArray();
            frequencies = f.ToArray();
            amplitudes = a.ToArray();
        }
    }
}
