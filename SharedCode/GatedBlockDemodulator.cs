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
            if (!b.detectors.Contains("asymmetry")) b.AddDetectorsToBlock();
            if (!b.Config.Settings.Contains("mwState")) b.Config.Settings.Add("mwState", true);

            int blockLength = b.Points.Count;

            // ----Copy across metadata----
            GatedDemodulatedBlock gatedDemodulatedBlock = new GatedDemodulatedBlock(b.TimeStamp, b.Config, b.GetPointDetectors(), config);

            // ----Gate the detector data using the config----
            Dictionary<string, double[]> detectorData = new Dictionary<string, double[]>();
            List<string> detectorList = new List<string>();
            foreach (string d in config.GatedDetectors)
            {
                detectorData.Add(d, GetGatedDetectorData(b, d, config.GetGate(d)));
            }
            foreach (string d in b.GetPointDetectors())
            {
                detectorData.Add(d, GetPointDetectorData(b, d));
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
            foreach (string d in detectorData.Keys)
            {
                int detectorIndex = b.detectors.IndexOf(d);

                // We obtain one Channel Set for each detector
                ChannelSet<double> gatedChannelSet = new ChannelSet<double>();

                // Detector calibration
                double calibration = ((TOF)((EDMPoint)b.Points[0]).Shot.TOFs[detectorIndex]).Calibration;

                // Divide TOFs into bins depending on switch state
                List<List<double>> statePoints = new List<List<double>>(numStates);
                for (int i = 0; i < numStates; i++) statePoints.Add(new List<double>(blockLength / numStates));
                for (int i = 0; i < blockLength; i++) statePoints[(int)switchStates[i]].Add(detectorData[b.detectors[detectorIndex]][i]);
                int subLength = blockLength / numStates;

                // For each analysis channel, calculate the mean and error
                // by means of RunningStatistics, then add to ChannelSet
                for (int channel = 0; channel < numStates; channel++)
                {
                    Channel<RunningStatistics> runningStats = new Channel<RunningStatistics> 
                    { On = new RunningStatistics(), Off = new RunningStatistics(), Difference = new RunningStatistics() };
                    for (int subIndex = 0; subIndex < subLength; subIndex++)
                    {
                        double onVal = 0.0;
                        double offVal = 0.0;
                        for (int i = 0; i < numStates; i++)
                        {
                            if (stateSigns[channel, i] == 1) onVal += statePoints[i][subIndex];
                            else offVal += statePoints[i][subIndex];
                        }
                        onVal /= numStates;
                        offVal /= numStates;
                        runningStats.On.Push(onVal);
                        runningStats.Off.Push(offVal);
                        runningStats.Difference.Push(onVal - offVal);
                    }

                    PointWithError pweOn = new PointWithError() { Value = runningStats.On.Mean, Error = runningStats.On.StandardErrorOfSampleMean };
                    PointWithError pweOff = new PointWithError() { Value = runningStats.Off.Mean, Error = runningStats.Off.StandardErrorOfSampleMean };
                    PointWithError pweDifference = new PointWithError() { Value = runningStats.Difference.Mean, Error = runningStats.Difference.StandardErrorOfSampleMean };

                    GatedChannel gatedChannel = new GatedChannel();
                    gatedChannel.On = pweOn;
                    gatedChannel.Off = pweOff;
                    gatedChannel.Difference = pweDifference;

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
                // Also append for bottom probe detector for live analysis purposes
                if (b.detectors[detectorIndex] == "asymmetry" || b.detectors[detectorIndex] == "bottomProbeScaled" || b.detectors[detectorIndex] == "topProbeNoBackground")
                {
                    string detector = b.detectors[detectorIndex];
                    var tofBlockDemodulator = new TOFBlockDemodulator();
                    // Get a TOF demodulated block containing only the special channels in the asymmetry deteector
                    var tofDemodulatedBlock = tofBlockDemodulator.TOFDemodulateBlockForSpecialChannels(b, new string[] { detector });
                    var tofSpecialChannelSet = tofDemodulatedBlock.GetChannelSet(detector);
                    
                    foreach(string channelName in tofSpecialChannelSet.Channels)
                    {
                        //Only add special channels
                        if (!gatedChannelSet.Channels.Contains(channelName))
                        {
                            gatedChannelSet.AddChannel(
                            channelName,
                            GateTOFChannel((TOFChannel)tofSpecialChannelSet.GetChannel(channelName), config.GetGate(detector))
                            );
                        }
                        
                    }
                }

                // Add the ChannelSet to the demodulated block
                gatedDemodulatedBlock.AddDetector(b.detectors[detectorIndex], calibration, gatedChannelSet);
            }

            return gatedDemodulatedBlock;
        }

        // This function takes a TOF demodulated block and gates each channel according to the gate configuration.
        public GatedDemodulatedBlock GateTOFDemodulatedBlock(TOFDemodulatedBlock tofDemodulatedBlock, GatedDemodulationConfig config)
        {
            var gatedDemodulatedBlock = new GatedDemodulatedBlock(tofDemodulatedBlock.TimeStamp, tofDemodulatedBlock.Config, tofDemodulatedBlock.PointDetectors, config);

            foreach(string detector in tofDemodulatedBlock.Detectors)
            {
                var channelSet = new ChannelSet<PointChannel>();

                if (config.GatedDetectors.Contains(detector))
                {
                    var tofChannelSet = tofDemodulatedBlock.GetChannelSet(detector);
                    foreach(string tofChannelName in tofChannelSet.Channels)
                    {
                        TOFChannel tofChannel = (TOFChannel)tofChannelSet.GetChannel(tofChannelName);
                        channelSet.AddChannel(
                                tofChannelName,
                                GateTOFChannel(tofChannel, config.GetGate(detector))
                                );
                    }
                    gatedDemodulatedBlock.AddDetector(detector, tofDemodulatedBlock.GetCalibration(detector), channelSet);
                }

                if (tofDemodulatedBlock.PointDetectors.Contains(detector))
                {
                    var tofChannelSet = tofDemodulatedBlock.GetChannelSet(detector);
                    foreach (string tofChannelName in tofChannelSet.Channels)
                    {
                        channelSet.AddChannel(
                            tofChannelName,
                            UnTOFulisePointChannel((TOFChannel)tofChannelSet.GetChannel(tofChannelName))
                            );
                    }

                    gatedDemodulatedBlock.AddDetector(detector, tofDemodulatedBlock.GetCalibration(detector), channelSet);
                }
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

        private double[] GetPointDetectorData(Block b, string detector)
        {
            return b.GetSPData(detector).ToArray();
        }

        private GatedChannel GateTOFChannel(TOFChannel tofChannel, Gate gate)
        {
            var gatedChannel = new GatedChannel();
            var onMeanAndErr = tofChannel.On.GatedMeanAndUncertainty(gate.GateLow, gate.GateHigh);
            var offMeanAndErr = tofChannel.Off.GatedMeanAndUncertainty(gate.GateLow, gate.GateHigh);
            var diffMeanAndErr = tofChannel.Difference.GatedMeanAndUncertainty(gate.GateLow, gate.GateHigh);

            gatedChannel.On = new PointWithError() { Value = onMeanAndErr[0], Error = onMeanAndErr[1] };
            // For the SIG channel, there is no off TOF
            if (offMeanAndErr == null) gatedChannel.Off = new PointWithError() { Value = 0.0, Error = 0.0 };
            else gatedChannel.Off = new PointWithError() { Value = offMeanAndErr[0], Error = offMeanAndErr[1] };
            gatedChannel.Difference = new PointWithError() { Value = diffMeanAndErr[0], Error = diffMeanAndErr[1] };

            return gatedChannel;
        }

        private PointChannel UnTOFulisePointChannel(TOFChannel tofChannel)
        {
            PointChannel pointChannel = new PointChannel();

            TOFWithError onTOF = tofChannel.On as TOFWithError;
            if (onTOF != null)
            {
                pointChannel.On.Value = onTOF.Data[0];
                pointChannel.On.Error = onTOF.Errors[0];
            }

            TOFWithError offTOF = tofChannel.Off as TOFWithError;
            if(offTOF != null)
            {
                if (offTOF.Length != 0)
                {
                    pointChannel.Off.Value = offTOF.Data[0];
                    pointChannel.Off.Error = offTOF.Errors[0];
                }
            }

            TOFWithError diffTOF = tofChannel.Difference as TOFWithError;
            if(diffTOF != null)
            {
                pointChannel.Difference.Value = diffTOF.Data[0];
                pointChannel.Difference.Error = diffTOF.Errors[0];
            }

            return pointChannel;
        }
    }
}