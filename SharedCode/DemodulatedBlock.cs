using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

using EDMConfig;

namespace Analysis.EDM
{
    public enum DemodulatedBlockType { TOF, GATED }
    [Serializable]
    [XmlInclude(typeof(GatedDemodulatedBlock))]
    [XmlInclude(typeof(TOFDemodulatedBlock))]
    public class DemodulatedBlock
    {
        public DateTime TimeStamp;
        public BlockConfig Config;
        public DemodulatedBlockType DataType;

        private readonly Dictionary<string, ChannelSet> ChannelSetDictionary = new Dictionary<string, ChannelSet>();
        private readonly Dictionary<string, double> DetectorCalibrations = new Dictionary<string, double>();

        public void AddDetector(string detector, double calibration, ChannelSet channelSet)
        {
            ChannelSetDictionary.Add(detector, channelSet);
            DetectorCalibrations.Add(detector, calibration);
        }
        
        public ChannelSet GetChannelSet(string detector)
        {
            return ChannelSetDictionary[detector];
        }

        public double GetCalibration(string detector)
        {
            return DetectorCalibrations[detector];
        }

        public List<string> Detectors
        {
            get
            {
                Dictionary<string, ChannelSet>.KeyCollection keys = ChannelSetDictionary.Keys;
                List<string> keyArray = new List<string>();
                foreach (string key in keys) keyArray.Add(key);
                return keyArray;
            }
        }

    }
}
