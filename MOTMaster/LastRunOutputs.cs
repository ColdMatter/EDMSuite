using System;
using System.Collections;
using System.Collections.Generic;
using DAQ.Analog;
using DAQ.HAL;
using DAQ.Pattern;

namespace MOTMaster
{
    /// <summary>
    /// The record Controller.GetLastRunOutputs serves over remoting: what a run's patterns left each channel at.
    /// </summary>
    public static class LastRunOutputs
    {
        static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Each analogue channel's value (a double, in volts as the script set it) at its latest event, and each
        /// registered digital channel's bit (a bool) in the last sample of its board's built pattern. Analogue
        /// channels with no events or with a calibration in calibrations, and boards with no built pattern, are
        /// left out, so every analogue value is the voltage on the output.
        /// </summary>
        public static Dictionary<String, Object> EndValues(MOTMasterSequence sequence, Hashtable digitalOutputChannels,
            Hashtable calibrations)
        {
            Dictionary<String, Object> values = new Dictionary<String, Object>();
            if (sequence.AnalogPattern != null)
            {
                foreach (AnalogPatternBuilderSingleBoard board in sequence.AnalogPattern.Boards.Values)
                {
                    foreach (KeyValuePair<String, Dictionary<Int32, Double>> channel in board.AnalogPatterns)
                    {
                        if (calibrations != null && calibrations.ContainsKey(channel.Key)) continue;
                        bool anyEvent = false;
                        int latestTime = 0;
                        foreach (int time in channel.Value.Keys)
                        {
                            if (!anyEvent || time > latestTime) latestTime = time;
                            anyEvent = true;
                        }
                        if (anyEvent) values[channel.Key] = channel.Value[latestTime];
                    }
                }
            }
            if (sequence.DigitalPattern != null && digitalOutputChannels != null)
            {
                foreach (DictionaryEntry entry in digitalOutputChannels)
                {
                    DigitalOutputChannel channel = entry.Value as DigitalOutputChannel;
                    if (channel == null || channel.Device == null) continue;
                    PatternBuilder32SingleBoard board;
                    if (!sequence.DigitalPattern.Boards.TryGetValue(channel.Device, out board)) continue;
                    UInt32[] pattern = board.Pattern;
                    if (pattern == null || pattern.Length == 0) continue;
                    values[(string)entry.Key] = (pattern[pattern.Length - 1] & (1u << channel.BitNumber)) != 0;
                }
            }
            return values;
        }

        /// <summary>
        /// The remoting record: "RunNumber" (int), "Script" (string), "Finished" (double, Unix seconds),
        /// "OutputsKnown" (bool) and "Values" (a new Dictionary&lt;String, Object&gt;, empty unless the outputs
        /// are known). Null values count as unknown outputs.
        /// </summary>
        public static Dictionary<String, Object> Record(int runNumber, string script, DateTime finishedUtc, bool outputsKnown,
            Dictionary<String, Object> values)
        {
            bool known = outputsKnown && values != null;
            return new Dictionary<String, Object>
            {
                { "RunNumber", runNumber },
                { "Script", script ?? "" },
                { "Finished", (finishedUtc - UnixEpoch).TotalSeconds },
                { "OutputsKnown", known },
                { "Values", known ? new Dictionary<String, Object>(values) : new Dictionary<String, Object>() }
            };
        }
    }
}
