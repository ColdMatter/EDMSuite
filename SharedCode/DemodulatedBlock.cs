using System;
using System.Collections.Generic;
using System.Text;

using EDMConfig;

namespace Analysis.EDM
{
    [Serializable]
    public class DemodulatedBlock
    {
        public DateTime TimeStamp;
        public BlockConfig Config;
        public DemodulationConfig DemodulationConfig;

        public Dictionary<string, int> DetectorIndices = new Dictionary<string, int>();
        public List<DetectorChannelValues> ChannelValues = new List<DetectorChannelValues>();
        public Dictionary<string, double> DetectorCalibrations = new Dictionary<string, double>();
        public DetectorFT NormFourier;

        // This is a convenience function that pulls out the mean and error of a channel,
        // specified by a set of switches for a given detector. This isn't the most efficient
        // way to do it if pulling out a lot of values, but it's not bad. And it is convenient.
        public double[] GetChannelValueAndError(string[] switches, string detector)
        {
            int detectorIndex = DetectorIndices[detector];
            DetectorChannelValues dcv = ChannelValues[detectorIndex];
            uint channelIndex = dcv.GetChannelIndex(switches);
            return new double[] { dcv.Values[channelIndex], dcv.Errors[channelIndex] };
        }

        public double[] GetSpecialChannelValueAndError(string name, string detector)
        {
            int detectorIndex = DetectorIndices[detector];
            DetectorChannelValues dcv = ChannelValues[detectorIndex];
            return dcv.SpecialValues[name];
         }

    }
}
