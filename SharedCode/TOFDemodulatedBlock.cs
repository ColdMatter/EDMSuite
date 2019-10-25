using System;
using System.Collections.Generic;
using Data;
using EDMConfig;

namespace Analysis.EDM
{
    [Serializable]
    public class TOFDemodulatedBlock : DemodulatedBlock
    {
        public TOFDemodulatedBlock(DateTime timeStamp, BlockConfig config, List<string> pointDetectors)
            :base(timeStamp, config, DemodulatedBlockType.TOF, pointDetectors)
        {
        }

        public TOFWithError GetTOFWithErrorChannel(string[] switches, string detector)
        {
            ChannelSet channelSet = GetChannelSet(detector);
            return (TOFWithError)channelSet.GetChannel(switches).Difference;
        }

        public TOFWithError GetTOFWithErrorChannel(string channelName, string detector)
        {
            ChannelSet channelSet = GetChannelSet(detector);
            return (TOFWithError)channelSet.GetChannel(channelName).Difference;
        }

        public ChannelSet GetTOFChannelSet(string detector)
        {
            return GetChannelSet(detector);
        }

        public double[][] GetTOFChannelWithError(string[] switches, string detector)
        {
            ChannelSet channelSet = GetChannelSet(detector);
            TOFWithError channelTOF = (TOFWithError)channelSet.GetChannel(switches).Difference;
            double[] times = new double[channelTOF.Times.Length];
            for (int i = 0; i < channelTOF.Times.Length; i++) times[i] = Convert.ToDouble(channelTOF.Times[i]);

            List<double[]> result = new List<double[]>();
            result.Add(times);
            result.Add(channelTOF.Data);
            result.Add(channelTOF.Errors);

            return result.ToArray();
        }

        public double[][] GetTOFChannelWithError(string channelName, string detector)
        {
            ChannelSet channelSet = GetChannelSet(detector);
            TOFWithError channelTOF = (TOFWithError)channelSet.GetChannel(channelName).Difference;
            double[] times = new double[channelTOF.Times.Length];
            for (int i = 0; i < channelTOF.Times.Length; i++) times[i] = Convert.ToDouble(channelTOF.Times[i]);

            List<double[]> result = new List<double[]>();
            result.Add(times);
            result.Add(channelTOF.Data);
            result.Add(channelTOF.Errors);

            return result.ToArray();
        }
    }
}
