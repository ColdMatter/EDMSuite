using System;
using System.Collections.Generic;

using DAQ.Environment;
using DAQ.HAL;

using NationalInstruments.DAQmx;

namespace DAQ.Analog
{
    /// <summary>
    /// Drives named analog outputs to their safe values with a one-off, software-timed
    /// write. An NI AO channel holds its last value when its task stops, so this is how
    /// a channel is put somewhere known after a pattern ends, or is cut short.
    /// The channels must not be reserved by a running task when this is called.
    ///
    /// An experiment declares its safe outputs in its Hardware class:
    ///     Info.Add(DAQMxAnalogSafeState.InfoKey, new Dictionary&lt;string, double&gt; { { "MOTCoilsCurrent", 0.0 } });
    /// </summary>
    public static class DAQMxAnalogSafeState
    {
        public const string InfoKey = "MOTMasterSafeOutputs";

        /// <summary>
        /// This machine's safe outputs (channel name to volts), or null if the
        /// experiment declares none.
        /// </summary>
        public static Dictionary<string, double> FromHardware()
        {
            return (Dictionary<string, double>)Environs.Hardware.GetInfo(InfoKey);
        }

        /// <summary>
        /// Every problem with the declared safe outputs: unregistered channel names and
        /// values outside a channel's range. Empty if they are all usable.
        /// </summary>
        public static List<string> Problems(IDictionary<string, double> safeOutputs)
        {
            List<string> problems = new List<string>();
            if (safeOutputs == null) return problems;
            foreach (KeyValuePair<string, double> kv in safeOutputs)
            {
                AnalogOutputChannel c = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[kv.Key];
                if (c == null)
                    problems.Add("\"" + kv.Key + "\" is not a registered analog output channel.");
                else if (kv.Value < c.RangeLow || kv.Value > c.RangeHigh)
                    problems.Add(kv.Key + ": safe value " + kv.Value + " V is outside its range "
                        + c.RangeLow + " to " + c.RangeHigh + " V.");
            }
            return problems;
        }

        public static void SetSafeValues(IDictionary<string, double> safeOutputs)
        {
            if (safeOutputs == null || safeOutputs.Count == 0) return;

            // Write nothing unless every channel can be written: a half-applied safe
            // state is worse than a loud failure.
            List<string> problems = Problems(safeOutputs);
            if (problems.Count > 0) throw new ArgumentException(string.Join(" ", problems));

            Task task = new Task("SafeOutputs");
            try
            {
                double[] values = new double[safeOutputs.Count];
                int i = 0;
                foreach (KeyValuePair<string, double> kv in safeOutputs)
                {
                    AnalogOutputChannel c = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[kv.Key];
                    c.AddToTask(task, c.RangeLow, c.RangeHigh);
                    values[i++] = kv.Value;
                }
                AnalogMultiChannelWriter writer = new AnalogMultiChannelWriter(task.Stream);
                writer.WriteSingleSample(true, values);
                task.Stop();
            }
            finally
            {
                task.Dispose();
            }
        }

        /// <summary>"MOTCoilsCurrent = 0 V, transferCoils = 0 V", for messages.</summary>
        public static string Describe(IDictionary<string, double> safeOutputs)
        {
            List<string> parts = new List<string>();
            foreach (KeyValuePair<string, double> kv in safeOutputs) parts.Add(kv.Key + " = " + kv.Value + " V");
            return string.Join(", ", parts);
        }
    }
}
