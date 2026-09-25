using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace SpectrumDDS
{
    /// <summary>
    /// JSON load and save for DDS patterns, so the GUI can keep a library of them.
    /// </summary>
    /// <remarks>
    /// Uses DataContractJsonSerializer, as MMDataIOHelper does, rather than pulling
    /// a JSON package into a solution that has none.
    /// <para>
    /// The file keeps the same units the MOTMaster scripts use -- milliseconds, MHz,
    /// MHz/ms, per-ms -- rather than the driver's SI, so that a saved pattern can be
    /// read side by side with the script it came from.
    /// </para>
    /// </remarks>
    public static class DDSPatternFile
    {
        [DataContract(Name = "DDSChannel")]
        private class ChannelDto
        {
            [DataMember(Name = "frequencyMHz", Order = 0)] public double Frequency;
            [DataMember(Name = "amplitude", Order = 1)] public double Amplitude;
            [DataMember(Name = "frequencySlopeMHzPerMs", Order = 2)] public double FrequencySlope;
            [DataMember(Name = "amplitudeSlopePerMs", Order = 3)] public double AmplitudeSlope;
        }

        [DataContract(Name = "DDSEvent")]
        private class EventDto
        {
            [DataMember(Name = "name", Order = 0)] public string Name;
            [DataMember(Name = "timeMs", Order = 1)] public double Time;
            [DataMember(Name = "xio", Order = 2)] public int Xio;
            [DataMember(Name = "channels", Order = 3)] public ChannelDto[] Channels;
        }

        [DataContract(Name = "DDSPattern")]
        private class PatternDto
        {
            [DataMember(Name = "description", Order = 0)] public string Description;
            [DataMember(Name = "events", Order = 1)] public EventDto[] Events;
        }

        public static void Save(DDSPattern pattern, string path, string description = null)
        {
            PatternDto dto = new PatternDto
            {
                Description = description ?? "",
                Events = pattern.Events.Select(e => new EventDto
                {
                    Name = e.Name,
                    Time = e.Time * DDSPattern.MillisecondsPerSecond,
                    Xio = e.Xio,
                    Channels = e.Channels.Select(c => new ChannelDto
                    {
                        Frequency = c.Frequency / DDSPattern.HzPerMHz,
                        Amplitude = c.Amplitude,
                        FrequencySlope = c.FrequencySlope / DDSPattern.HzPerSecondPerMHzPerMs,
                        AmplitudeSlope = c.AmplitudeSlope / DDSPattern.PerSecondPerPerMs,
                    }).ToArray(),
                }).ToArray(),
            };

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(PatternDto));
            using (FileStream stream = File.Create(path))
            using (JsonWriterSettingsIndent writer = new JsonWriterSettingsIndent(stream))
            {
                serializer.WriteObject(writer.Writer, dto);
                writer.Writer.Flush();
            }
        }

        public static DDSPattern Load(string path)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(PatternDto));
            PatternDto dto;
            using (FileStream stream = File.OpenRead(path))
                dto = (PatternDto)serializer.ReadObject(stream);

            if (dto == null || dto.Events == null)
                throw new InvalidDataException(path + " does not contain a DDS pattern");

            List<DDSEvent> events = new List<DDSEvent>();
            foreach (EventDto e in dto.Events)
            {
                if (e.Channels == null || e.Channels.Length != DDSPattern.ChannelCount)
                    throw new InvalidDataException(string.Format(
                        "event \"{0}\" in {1} has {2} channels, expected {3}",
                        e.Name, path, e.Channels == null ? 0 : e.Channels.Length,
                        DDSPattern.ChannelCount));

                DDSChannelState[] channels = e.Channels.Select(c => new DDSChannelState(
                    c.Frequency * DDSPattern.HzPerMHz,
                    c.Amplitude,
                    c.FrequencySlope * DDSPattern.HzPerSecondPerMHzPerMs,
                    c.AmplitudeSlope * DDSPattern.PerSecondPerPerMs)).ToArray();

                events.Add(new DDSEvent(e.Name, e.Time / DDSPattern.MillisecondsPerSecond,
                                        channels, e.Xio));
            }
            return new DDSPattern(events);
        }

        /// <summary>
        /// DataContractJsonSerializer writes one unbroken line; this wraps it in an
        /// indenting writer so saved patterns can be read and hand-edited.
        /// </summary>
        private class JsonWriterSettingsIndent : IDisposable
        {
            public System.Xml.XmlDictionaryWriter Writer { get; private set; }

            public JsonWriterSettingsIndent(Stream stream)
            {
                Writer = JsonReaderWriterFactory.CreateJsonWriter(
                    stream, Encoding.UTF8, ownsStream: false, indent: true, indentChars: "  ");
            }

            public void Dispose()
            {
                if (Writer != null) { Writer.Close(); Writer = null; }
            }
        }
    }
}
