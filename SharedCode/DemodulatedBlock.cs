using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

using EDMConfig;
using Data;

namespace Analysis.EDM
{
    [Serializable]
    public class DemodulatedBlock
    {
        public DateTime TimeStamp { get; }
        public BlockConfig Config { get; }
        public DemodulationConfig DemodulationConfig { get; }

        private readonly Dictionary<string, ChannelSet<TOFWithError>> TOFChannelSetDictionary = new Dictionary<string, ChannelSet<TOFWithError>>();
        private readonly Dictionary<string, ChannelSet<PointWithError>> PointChannelSetDictionary = new Dictionary<string, ChannelSet<PointWithError>>();
        private readonly Dictionary<string, double> DetectorCalibrations = new Dictionary<string, double>();

        public DemodulatedBlock(DateTime timeStamp, BlockConfig config, DemodulationConfig demodulationConfig)
        {
            this.TimeStamp = timeStamp;
            this.Config = config;
            this.DemodulationConfig = demodulationConfig;
        }

        public PointWithError GetPointChannel(string[] switches, string detector)
        {
            return GetPointChannelSet(detector).GetChannel(switches);
        }

        public PointWithError GetPointChannel(string channel, string detector)
        {
            return GetPointChannelSet(detector).GetChannel(channel);
        }

        public TOFWithError GetTOFChannel(string[] switches, string detector)
        {
            return GetTOFChannelSet(detector).GetChannel(switches);
        }

        public TOFWithError GetTOFChannel(string channel, string detector)
        {
            return GetTOFChannelSet(detector).GetChannel(channel);
        }

        public void AddDetector(string detector, double calibration, ChannelSet<PointWithError> channelSet)
        {
            PointChannelSetDictionary.Add(detector, channelSet);
            DetectorCalibrations.Add(detector, calibration);
        }

        public void AddDetector(string detector, double calibration, ChannelSet<TOFWithError> channelSet)
        {
            TOFChannelSetDictionary.Add(detector, channelSet);
            DetectorCalibrations.Add(detector, calibration);
        }

        public ChannelSet<PointWithError> GetPointChannelSet(string detector)
        {
            return PointChannelSetDictionary[detector];
        }

        public ChannelSet<TOFWithError> GetTOFChannelSet(string detector)
        {
            return TOFChannelSetDictionary[detector];
        }

        public double GetCalibration(string detector)
        {
            return DetectorCalibrations[detector];
        }

        public List<string> Detectors
        {
            get
            {
                Dictionary<string, ChannelSet<PointWithError>>.KeyCollection keys1 = PointChannelSetDictionary.Keys;
                Dictionary<string, ChannelSet<TOFWithError>>.KeyCollection keys2 = TOFChannelSetDictionary.Keys;
                List<string> keyArray = new List<string>();
                foreach (string key in keys1) keyArray.Add(key);
                foreach (string key in keys2) keyArray.Add(key);
                return keyArray;
            }
        }

    }
}
