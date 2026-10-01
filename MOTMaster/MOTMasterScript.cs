using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DAQ.Pattern;
using DAQ.Analog;
using DAQ.Environment;

namespace MOTMaster
{
    public abstract class MOTMasterScript
    {
        public abstract PatternBuilder32 GetDigitalPattern();
        public abstract AnalogPatternBuilder GetAnalogPattern();
        public virtual AnalogStaticBuilder GetAnalogStatic()
        {
            return new AnalogStaticBuilder();
        }
        public Dictionary<String, Object> Parameters;

        /// <summary>
        /// Loads parameters shared between scripts from globalParameters.json in the
        /// scripts folder (edited via MOTMaster's Parameters > Edit parameter file menu).
        /// Call this after initialising Parameters and before any script-specific
        /// Parameters[...] assignments, so per-script values still take precedence.
        /// </summary>
        protected void LoadGlobalParameters()
        {
            string path = Path.Combine((string)Environs.FileSystem.Paths["scriptListPath"], "globalParameters.json");
            var groups = ParameterFileManager.ReadFile(path, typeof(double));
            foreach (ParameterEntry entry in ParameterFileManager.Flatten(groups))
            {
                Parameters[entry.Name] = Convert.ChangeType(entry.Value, entry.Type, CultureInfo.InvariantCulture);
            }
        }

        public virtual Dictionary<string, List<List<double>>> GetDDSPattern()
        {
            return new Dictionary<string, List<List<double>>>();
        }

        public Dictionary<string, List<object>> switchConfiguration = new Dictionary<string, List<object>> { };

        public MOTMasterSequence GetSequence()
        {
            MOTMasterSequence s = new MOTMasterSequence();
            s.DigitalPattern = GetDigitalPattern();
            s.AnalogPattern = GetAnalogPattern();
            s.AnalogStatic = GetAnalogStatic();
            s.DDSPattern = GetDDSPattern();
            return s;
        }

        public void EditDictionary(Dictionary<String, Object> dictionary)
        {
            foreach (KeyValuePair<string, object> k in dictionary)
            {
                if (Parameters.ContainsKey(k.Key))
                {
                    Parameters[k.Key] = k.Value;
                }
                else
                {
                    throw new ParameterNotInOriginalDictionaryException();
                }
            }
        }
        public class ParameterNotInOriginalDictionaryException : ApplicationException { }
    }

#if DDS
    /// <summary>
    /// Builds the dictionary <see cref="MOTMasterScript.GetDDSPattern"/> returns.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Replaces the <c>addDDSPattern</c> helper that is copy-pasted verbatim into
    /// every cafmot script. The units and conventions are exactly the old helper's,
    /// so a script can be converted a call at a time and mixed with the old helper
    /// in the meantime:
    /// </para>
    /// <list type="bullet">
    /// <item><description><paramref name="time"/> is in pattern ticks. Both the
    /// digital and analog clocks run at 100 kHz, so a tick is 10 us and the stored
    /// time is <c>time / 100</c> ms.</description></item>
    /// <item><description>Frequencies are MHz and amplitudes are fractions of the
    /// channel output level.</description></item>
    /// <item><description>Slopes are given per tick and stored per millisecond,
    /// hence the factor of 100.</description></item>
    /// </list>
    /// <para>
    /// t = 0 is the pulse on DDS_Analog_Trg, which the scripts fire at the Q-switch
    /// instant.
    /// </para>
    /// <para>
    /// Gated on the DDS compile symbol, which only the CaF configuration defines, so
    /// that experiments which do not use the DDS see no change at all.
    /// </para>
    /// </remarks>
    public class DDSPatternBuilder
    {
        /// <summary>Pattern ticks per millisecond, at the 100 kHz pattern clock.</summary>
        public const double TicksPerMillisecond = 100.0;

        private readonly Dictionary<string, List<List<double>>> pattern =
            new Dictionary<string, List<List<double>>>();

        /// <summary>The pattern, in the form GetDDSPattern must return.</summary>
        public Dictionary<string, List<List<double>>> Pattern { get { return pattern; } }

        /// <summary>Add an event holding steady frequencies and amplitudes.</summary>
        public void AddEvent(string name, int time,
                             double[] frequencies, double[] amplitudes)
        {
            AddEvent(name, time, frequencies, amplitudes, null, null);
        }

        /// <summary>Add an event that also starts a frequency or amplitude ramp.</summary>
        /// <param name="frequencySlopes">MHz per tick, or null for no ramp.</param>
        /// <param name="amplitudeSlopes">Amplitude per tick, or null for no ramp.</param>
        public void AddEvent(string name, int time,
                             double[] frequencies, double[] amplitudes,
                             double[] frequencySlopes, double[] amplitudeSlopes)
        {
            if (pattern.ContainsKey(name))
                throw new ArgumentException(
                    "there is already a DDS event called \"" + name + "\"", "name");

            pattern.Add(name, new List<List<double>>
            {
                new List<double> { time / TicksPerMillisecond },
                Row(name, "frequency", frequencies, 1.0),
                Row(name, "amplitude", amplitudes, 1.0),
                Row(name, "frequency slope", frequencySlopes, TicksPerMillisecond),
                Row(name, "amplitude slope", amplitudeSlopes, TicksPerMillisecond),
            });
        }

        private static List<double> Row(string name, string what, double[] values, double scale)
        {
            if (values == null) return new List<double> { 0.0, 0.0, 0.0, 0.0 };
            if (values.Length != 4)
                throw new ArgumentException(string.Format(
                    "DDS event \"{0}\": the {1} row has {2} entries, expected 4 (DDS1 to DDS4)",
                    name, what, values.Length));

            List<double> row = new List<double>(4);
            foreach (double v in values) row.Add(v * scale);
            return row;
        }
    }
#endif //DDS
}
