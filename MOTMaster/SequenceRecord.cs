using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DAQ.Analog;
using DAQ.HAL;
using DAQ.Pattern;

namespace MOTMaster
{
    /// <summary>
    /// Filled in by a recording run (<see cref="Controller.GoCachedRecording"/>) with the JSON of the sequence it built.
    /// Null for every other run, which then does exactly what it always did.
    /// </summary>
    public sealed class SequenceRecording
    {
        public string Json;
    }

    /// <summary>
    /// The JSON record of a built sequence: every event this machine's pattern generators were given, enough to
    /// rebuild the exact output without the script. Digital edges are per board and bit (the channel names that map
    /// to each bit are listed alongside, since two names can share a bit). Analog events are in the script's units
    /// and, through this machine's calibrations, in the volts the card output; each value holds until the next event.
    /// </summary>
    /// <remarks>
    /// Read only by remote callers that opt in; nothing in MOTMaster reads it. Building it touches the sequence
    /// read-only, after the patterns are built and before any shot.
    /// </remarks>
    public static class SequenceRecord
    {
        public const int Format = 1;

        public static string ToJson(MOTMasterSequence sequence, string scriptFileName, string scriptText,
            IDictionary<string, object> parameters, int digitalClock, int analogClock, bool debug,
            IDictionary<string, object> link)
        {
            Hardware hardware = DAQ.Environment.Environs.Hardware;
            var record = new SortedDictionary<string, object>(StringComparer.Ordinal)
            {
                { "format", Format },
                { "machine", System.Environment.MachineName },
                { "provenance", BuildProvenance.Describe() },
                { "debug", debug },
                { "script", new SortedDictionary<string, object>(StringComparer.Ordinal)
                    {
                        { "file", scriptFileName },
                        { "sha256", LinkFingerprint.OfText(scriptText) },
                        { "text", scriptText }
                    }
                },
                { "parameters", parameters == null ? null : new SortedDictionary<string, object>(parameters, StringComparer.Ordinal) },
                { "digital_clock_hz", digitalClock },
                { "analog_clock_hz", analogClock },
                { "digital", Digital(sequence.DigitalPattern, hardware.DigitalOutputChannels) },
                { "analog", Analog(sequence.AnalogPattern, hardware.Calibrations) },
                { "analog_static", AnalogStatic(sequence.AnalogStatic) },
                { "dds", sequence.DDSPattern },
                { "link", link }
            };
            var json = new StringBuilder();
            Write(json, record);
            return json.ToString();
        }

        static object Digital(PatternBuilder32 pattern, Hashtable channels)
        {
            var boards = new SortedDictionary<string, object>(StringComparer.Ordinal);
            if (pattern == null) return boards;
            foreach (KeyValuePair<string, PatternBuilder32SingleBoard> board in pattern.Boards)
            {
                var names = new SortedDictionary<string, object>(StringComparer.Ordinal);
                foreach (DictionaryEntry entry in channels)
                {
                    DigitalOutputChannel channel = entry.Value as DigitalOutputChannel;
                    if (channel != null && channel.Device == board.Key) names[(string)entry.Key] = channel.BitNumber;
                }
                // bit -> [[tick, 0 or 1], ...]
                var edges = new SortedDictionary<int, List<object>>();
                foreach (DictionaryEntry ev in board.Value.Layout.EventList)
                {
                    int time = (int)ev.Key;
                    EdgeSet set = (EdgeSet)ev.Value;
                    for (int bit = 0; bit < 32; bit++)
                    {
                        EdgeSense sense = set.GetEdge(bit);
                        if (sense == EdgeSense.NC) continue;
                        if (!edges.ContainsKey(bit)) edges[bit] = new List<object>();
                        edges[bit].Add(new object[] { time, sense == EdgeSense.UP ? 1 : 0 });
                    }
                }
                var edgesByBit = new SortedDictionary<string, object>(StringComparer.Ordinal);
                foreach (KeyValuePair<int, List<object>> bitEdges in edges)
                    edgesByBit[bitEdges.Key.ToString(CultureInfo.InvariantCulture)] = bitEdges.Value;
                UInt32[] built = board.Value.Pattern;
                boards[board.Key] = new SortedDictionary<string, object>(StringComparer.Ordinal)
                {
                    { "length", built == null ? (object)null : built.Length },
                    { "channels", names },
                    { "edges", edgesByBit }
                };
            }
            return boards;
        }

        static object Analog(AnalogPatternBuilder pattern, Hashtable calibrations)
        {
            var boards = new SortedDictionary<string, object>(StringComparer.Ordinal);
            if (pattern == null) return boards;
            foreach (KeyValuePair<string, AnalogPatternBuilderSingleBoard> board in pattern.Boards)
            {
                var channels = new SortedDictionary<string, object>(StringComparer.Ordinal);
                foreach (KeyValuePair<string, Dictionary<int, double>> channel in board.Value.AnalogPatterns)
                {
                    Calibration calibration = calibrations != null && calibrations.ContainsKey(channel.Key)
                        ? (Calibration)calibrations[channel.Key] : null;
                    List<int> ticks = channel.Value.Keys.OrderBy(t => t).ToList();
                    var values = new List<object>(ticks.Count);
                    var volts = new List<object>(ticks.Count);
                    foreach (int tick in ticks)
                    {
                        double value = channel.Value[tick];
                        values.Add(value);
                        volts.Add(calibration == null ? value : calibration.Convert(value));
                    }
                    channels[channel.Key] = new SortedDictionary<string, object>(StringComparer.Ordinal)
                    {
                        { "ticks", ticks.Cast<object>().ToList() },
                        { "values", values },
                        { "volts", volts }
                    };
                }
                boards[board.Key] = new SortedDictionary<string, object>(StringComparer.Ordinal)
                {
                    { "length", board.Value.PatternLength },
                    { "channels", channels }
                };
            }
            return boards;
        }

        static object AnalogStatic(AnalogStaticBuilder pattern)
        {
            var boards = new SortedDictionary<string, object>(StringComparer.Ordinal);
            if (pattern == null) return boards;
            foreach (KeyValuePair<string, AnalogStaticBuilderSingleBoard> board in pattern.Boards)
                boards[board.Key] = new SortedDictionary<string, object>(board.Value.StaticAnalogValues
                    .ToDictionary(kv => kv.Key, kv => (object)kv.Value), StringComparer.Ordinal);
            return boards;
        }

        /// <summary>A value that is already JSON (the linked secondary's record), written as is.</summary>
        public sealed class RawJson
        {
            public readonly string Json;
            public RawJson(string json) { Json = json; }
        }

        static void Write(StringBuilder json, object value)
        {
            if (value == null) { json.Append("null"); return; }
            if (value is RawJson) { json.Append(((RawJson)value).Json ?? "null"); return; }
            if (value is string) { WriteString(json, (string)value); return; }
            if (value is bool) { json.Append((bool)value ? "true" : "false"); return; }
            if (value is double) { WriteDouble(json, (double)value); return; }
            if (value is float) { WriteDouble(json, (float)value); return; }
            if (value is int || value is long || value is short || value is byte || value is uint || value is ulong
                || value is ushort || value is sbyte)
            {
                json.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
                return;
            }
            if (value is decimal) { json.Append(((decimal)value).ToString(CultureInfo.InvariantCulture)); return; }
            IDictionary dictionary = value as IDictionary;
            if (dictionary != null)
            {
                json.Append('{');
                bool first = true;
                foreach (DictionaryEntry entry in dictionary)
                {
                    if (!first) json.Append(',');
                    first = false;
                    WriteString(json, Convert.ToString(entry.Key, CultureInfo.InvariantCulture));
                    json.Append(':');
                    Write(json, entry.Value);
                }
                json.Append('}');
                return;
            }
            IEnumerable sequence = value as IEnumerable;
            if (sequence != null)
            {
                json.Append('[');
                bool first = true;
                foreach (object item in sequence)
                {
                    if (!first) json.Append(',');
                    first = false;
                    Write(json, item);
                }
                json.Append(']');
                return;
            }
            WriteString(json, Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        // Python's json module reads NaN and Infinity, which strict JSON has no spelling for.
        static void WriteDouble(StringBuilder json, double value)
        {
            if (double.IsNaN(value)) json.Append("NaN");
            else if (double.IsPositiveInfinity(value)) json.Append("Infinity");
            else if (double.IsNegativeInfinity(value)) json.Append("-Infinity");
            else json.Append(value.ToString("R", CultureInfo.InvariantCulture));
        }

        static void WriteString(StringBuilder json, string text)
        {
            json.Append('"');
            foreach (char c in text)
            {
                switch (c)
                {
                    case '"': json.Append("\\\""); break;
                    case '\\': json.Append("\\\\"); break;
                    case '\n': json.Append("\\n"); break;
                    case '\r': json.Append("\\r"); break;
                    case '\t': json.Append("\\t"); break;
                    default:
                        if (c < 0x20) json.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else json.Append(c);
                        break;
                }
            }
            json.Append('"');
        }
    }
}
