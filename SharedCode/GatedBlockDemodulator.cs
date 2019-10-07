using System;
using System.Collections.Generic;
using System.Text;

using Data;
using Data.EDM;
using EDMConfig;

namespace Analysis.EDM
{
    public class GatedBlockDemodulator : BlockDemodulator
    {
        private delegate double[] GatedDetectorExtractFunction(int index, double startTime, double endTime);

        // This function gates the detector data first, and then demodulates the channels.
        // Since it's fast we only have the option of demodulating all channels for all detectors.
        public GatedDemodulatedBlock GateThenDemodulateBlock(Block b, GatedDemodulationConfig config)
        {
            int blockLength = b.Points.Count;

            // ----Copy across metadata----
            GatedDemodulatedBlock gatedDemodulatedBlock = new GatedDemodulatedBlock();
            gatedDemodulatedBlock.TimeStamp = b.TimeStamp;
            gatedDemodulatedBlock.Config = b.Config;
            gatedDemodulatedBlock.GateConfig = config;

            // ----Gate the detector data using the config----
            Dictionary<string, double[]> gatedDetectorData = new Dictionary<string, double[]>();
            foreach (string d in b.detectors)
            {
                gatedDetectorData.Add(d, GetGatedDetectorData(b, d, config.GetGate(d)));
            }

            // ----Demodulate channels----
            // --Build list of modulations--
            List<Modulation> modulations = GetModulations(b);

            // --Work out switch state for each point--
            List<uint> switchStates = GetSwitchStates(modulations);

            // --Calculate state signs for each analysis channel--
            // The first index selects the analysis channel, the second the switchState
            int numStates = (int)Math.Pow(2, modulations.Count);
            int[,] stateSigns = GetStateSigns(numStates);

            // --This is done for each detector--
            for (int detectorIndex = 0; detectorIndex < b.detectors.Count; detectorIndex++)
            {
                // We obtain one Channel Set for each detector
                ChannelSet<double> gatedChannelSet = new ChannelSet<double>();

                // Detector calibration
                double calibration = ((TOF)((EDMPoint)b.Points[0]).Shot.TOFs[detectorIndex]).Calibration;

                // Divide TOFs into bins depending on switch state
                List<List<double>> statePoints = new List<List<double>>(numStates);
                for (int i = 0; i < numStates; i++) statePoints.Add(new List<double>(blockLength / numStates));
                for (int i = 0; i < blockLength; i++) statePoints[(int)switchStates[i]].Add(gatedDetectorData[b.detectors[detectorIndex]][i]);
                int subLength = blockLength / numStates;

                // For each analysis channel, calculate the mean and error
                // by means of RunningStatistics, then add to ChannelSet
                for (int channel = 0; channel < numStates; channel++)
                {
                    Channel<RunningStatistics> runningStats = new Channel<RunningStatistics>();
                    for (int subIndex = 0; subIndex < subLength; subIndex++)
                    {
                        double onVal = 0.0;
                        double offVal = 0.0;
                        for (int i = 0; i < numStates; i++)
                        {
                            if (stateSigns[channel, switchStates[i]] == 1) onVal += statePoints[i][subIndex];
                            else offVal += statePoints[i][subIndex];
                        }
                        onVal /= blockLength;
                        offVal /= blockLength;
                        runningStats.On.Push(onVal);
                        runningStats.Off.Push(offVal);
                        runningStats.Difference.Push(onVal - offVal);
                    }

                    GatedChannel gatedChannel = new GatedChannel();
                    gatedChannel.On.Value = runningStats.On.Mean;
                    gatedChannel.On.Error = runningStats.On.StandardErrorOfSampleMean;
                    gatedChannel.Off.Value = runningStats.Off.Mean;
                    gatedChannel.Off.Error = runningStats.Off.StandardErrorOfSampleMean;
                    gatedChannel.Difference.Value = runningStats.Difference.Mean;
                    gatedChannel.Difference.Error = runningStats.Difference.StandardErrorOfSampleMean;

                    // add the Channel to the ChannelSet
                    List<string> usedSwitches = new List<string>();
                    for (int i = 0; i < modulations.Count; i++)
                        if ((channel & (1 << i)) != 0) usedSwitches.Add(modulations[i].Name);
                    string[] channelName = usedSwitches.ToArray();
                    // the SIG channel has a special name
                    if (channel == 0) channelName = new string[] { "SIG" };
                    gatedChannelSet.AddChannel(channelName, gatedChannel);
                }

                // Append the ChannelSet with special combinations of channels for the asymmetry detector only
                if(b.detectors[detectorIndex] == "asymmetry")
                {
                    var tofBlockDemodulator = new TOFBlockDemodulator();
                    // Get a TOF demodulated block containing only the special channels in the asymmetry deteector
                    var tofDemodulatedBlock = tofBlockDemodulator.TOFDemodulateBlockForSpecialChannels(b, new string[] { "asymmetry" });
                    var tofSpecialChannelSet = tofDemodulatedBlock.GetChannelSet("asymmetry");
                    
                    foreach(string channelName in tofSpecialChannelSet.Channels)
                    {
                        gatedChannelSet.AddChannel(
                            channelName, 
                            GateTOFChannel((TOFChannel)tofSpecialChannelSet.GetChannel(channelName), config.GetGate("asymmetry"))
                            );
                    }
                }

                // Add the ChannelSet to the demodulated block
                gatedDemodulatedBlock.AddDetector(b.detectors[detectorIndex], calibration, gatedChannelSet);
            }

            return gatedDemodulatedBlock;
        }

        public GatedDemodulatedBlock GateTOFDemodulatedBlock(TOFDemodulatedBlock tofDemodulatedBlock, GatedDemodulationConfig config)
        {
            var gatedDemodulatedBlock = new GatedDemodulatedBlock()
            {
                TimeStamp = tofDemodulatedBlock.TimeStamp,
                Config = tofDemodulatedBlock.Config
            };

            foreach(string detector in tofDemodulatedBlock.Detectors)
            {
                var channelSet = new ChannelSet<PointChannel>();

                if (config.Gates.Contains(detector))
                {
                    var tofChannelSet = tofDemodulatedBlock.GetChannelSet(detector);
                    foreach(string tofChannelName in tofChannelSet.Channels)
                    {
                        channelSet.AddChannel(
                            tofChannelName,
                            GateTOFChannel((TOFChannel)tofChannelSet.GetChannel(tofChannelName), config.GetGate(detector))
                            );
                    }
                }

                if (config.PointDetectors.Contains(detector))
                {
                    var tofChannelSet = tofDemodulatedBlock.GetChannelSet(detector);
                    foreach (string tofChannelName in tofChannelSet.Channels)
                    {
                        channelSet.AddChannel(
                            tofChannelName,
                            UnTOFulisePointChannel((TOFChannel)tofChannelSet.GetChannel(tofChannelName))
                            );
                    }
                }

                gatedDemodulatedBlock.AddDetector(detector, tofDemodulatedBlock.GetCalibration(detector), channelSet);
            }

            return gatedDemodulatedBlock;
        }

        private double[] GetGatedDetectorData(Block b, string detector, Gate gate)
        {
            int detectorIndex = b.detectors.IndexOf(detector);
            List<double> gatedData = new List<double>();
            GatedDetectorExtractFunction f;
            if (gate.Integrate) f = new GatedDetectorExtractFunction(b.GetTOFIntegralArray);
            else f = new GatedDetectorExtractFunction(b.GetTOFMeanArray);
            double[] rawData = f(detectorIndex, gate.GateLow, gate.GateHigh);
            return rawData;
        }

        private GatedChannel GateTOFChannel(TOFChannel tofChannel, Gate gate)
        {
            var gatedChannel = new GatedChannel();
            var onMeanAndErr = tofChannel.On.GatedMeanAndUncertainty(gate.GateLow, gate.GateHigh);
            var offMeanAndErr = tofChannel.Off.GatedMeanAndUncertainty(gate.GateLow, gate.GateHigh);
            var diffMeanAndErr = tofChannel.Difference.GatedMeanAndUncertainty(gate.GateLow, gate.GateHigh);

            gatedChannel.On.Value = onMeanAndErr[0];
            gatedChannel.On.Error = onMeanAndErr[1];
            gatedChannel.Off.Value = offMeanAndErr[0];
            gatedChannel.Off.Error = offMeanAndErr[1];
            gatedChannel.Difference.Value = diffMeanAndErr[0];
            gatedChannel.Difference.Error = diffMeanAndErr[1];

            return gatedChannel;
        }

        private PointChannel UnTOFulisePointChannel(TOFChannel tofChannel)
        {
            PointChannel pointChannel = new PointChannel();

            if (tofChannel.On is TOFWithError onTOF)
            {
                pointChannel.On.Value = onTOF.Data[0];
                pointChannel.On.Error = onTOF.Errors[0];
            }

            if(tofChannel.Off is TOFWithError offTOF)
            {
                pointChannel.Off.Value = offTOF.Data[0];
                pointChannel.Off.Error = offTOF.Errors[0];
            }

            if(tofChannel.Difference is TOFWithError diffTOF)
            {
                pointChannel.Difference.Value = diffTOF.Data[0];
                pointChannel.Difference.Error = diffTOF.Errors[0];
            }

            return pointChannel;
        }
    }
}