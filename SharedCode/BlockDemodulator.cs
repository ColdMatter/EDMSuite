using System;
using System.Collections.Generic;
using System.Text;

using Data;
using Data.EDM;
using EDMConfig;

namespace Analysis.EDM
{
    public class BlockDemodulator
    {
        private const int kNumReplicates = 50;
        private Random r = new Random();

        // constants
        private static double plateSpacing = 1.2;
        private static double electronCharge = 1.6022 * Math.Pow(10, -19);
        private static double bohrMagneton = 9.274 * Math.Pow(10, -24);
        private static double saturatedEffectiveField = 26 * Math.Pow(10, 9);

        // This function adds background-subtracted TOFs, scaled bottom probe TOF, and asymmetry TOF to the block
        // Also has the option of converting single point data to TOFs
        public void AddDetectorsToBlock(Block b, bool spvToTof)
        {
            b.SubtractBackgroundFromProbeDetectorTOFs();

            b.CreateScaledBottomProbe();

            b.ConstructAsymmetryTOF();

            if(spvToTof) b.TOFuliseSinglePointData();
        }

        // This function gates the detector data first, and then demodulates the channels.
        // This means that it can give innacurate results for non-linear combinations
        // of channels that vary appreciably over the TOF. There's another, slower, function
        // DemodulateBlockNL that takes care of this.
        public DemodulatedBlock DemodulateBlock(Block b, DemodulationConfig config)
        {
            // *** copy across the metadata ***
            DemodulatedBlock db = new DemodulatedBlock();
            db.TimeStamp = b.TimeStamp;
            db.Config = b.Config;
            db.DemodulationConfig = config;

            // *** extract the gated detector data using the given config ***
            List<GatedDetectorData> gatedDetectorData = new List<GatedDetectorData>();
            foreach (string d in b.detectors)
            {
                GatedDetectorExtractSpec gdes;
                config.GatedDetectorExtractSpecs.TryGetValue(d, out gdes);

                if (gdes != null)
                {
                    gatedDetectorData.Add(GatedDetectorData.ExtractFromBlock(b, gdes));
                    db.DetectorIndices.Add(gdes.Name, b.detectors.IndexOf(gdes.Name));
                    db.DetectorCalibrations.Add(gdes.Name,
                        ((TOF)((EDMPoint)b.Points[0]).Shot.TOFs[b.detectors.IndexOf(gdes.Name)]).Calibration);
                }
            }

            // *** extract the point detector data ***
            List<PointDetectorData> pointDetectorData = new List<PointDetectorData>();
            foreach (string channel in config.PointDetectorChannels)
            {
                pointDetectorData.Add(PointDetectorData.ExtractFromBlock(b, channel));
                // for the moment all single point detector channels are set to have a calibration
                // of 1.0 .
                db.DetectorCalibrations.Add(channel, 1.0);
            }

            // *** build the list of detector data ***
            List<DetectorData> detectorData = new List<DetectorData>();
            for (int i = 0; i < gatedDetectorData.Count; i++) detectorData.Add(gatedDetectorData[i]);
            for (int i = 0; i < config.PointDetectorChannels.Count; i++)
            {
                detectorData.Add(pointDetectorData[i]);
                db.DetectorIndices.Add(config.PointDetectorChannels[i], i + gatedDetectorData.Count);
            }

            // *** demodulate channels ***
            // ** build the list of modulations **
            List<string> modNames = new List<string>();
            List<Waveform> modWaveforms = new List<Waveform>();
            foreach (AnalogModulation mod in b.Config.AnalogModulations)
            {
                modNames.Add(mod.Name);
                modWaveforms.Add(mod.Waveform);
            }
            foreach (DigitalModulation mod in b.Config.DigitalModulations)
            {
                modNames.Add(mod.Name);
                modWaveforms.Add(mod.Waveform);
            }
            foreach (TimingModulation mod in b.Config.TimingModulations)
            {
                modNames.Add(mod.Name);
                modWaveforms.Add(mod.Waveform);
            }

            // ** work out the switch state for each point **
            int blockLength = modWaveforms[0].Length;
            List<bool[]> wfBits = new List<bool[]>();
            foreach (Waveform wf in modWaveforms) wfBits.Add(wf.Bits);
            List<uint> switchStates = new List<uint>(blockLength);
            for (int i = 0; i < blockLength; i++)
            {
                uint switchState = 0;
                for (int j = 0; j < wfBits.Count; j++)
                {
                    if (wfBits[j][i]) switchState += (uint)Math.Pow(2, j);
                }
                switchStates.Add(switchState);
            }

            // pre-calculate the state signs for each analysis channel
            // the first index selects the analysis channel, the second the switchState
            int numStates = (int)Math.Pow(2, modWaveforms.Count);
            int[,] stateSigns = new int[numStates, numStates];
            for (uint i = 0; i < numStates; i++)
            {
                for (uint j = 0; j < numStates; j++)
                {
                    stateSigns[i, j] = stateSign(j, i);
                }
            }

            // ** the following needs to be done for each detector **
            for (int detector = 0; detector < detectorData.Count; detector++)
            {
                DetectorChannelValues dcv = new DetectorChannelValues();
                for (int i = 0; i < modNames.Count; i++) dcv.SwitchMasks.Add(modNames[i], (uint)(1 << i));
                // * divide the data up into bins according to switch state *
                List<List<double>> statePoints = new List<List<double>>(numStates);
                for (int i = 0; i < numStates; i++) statePoints.Add(new List<double>(blockLength / numStates));
                for (int i = 0; i < blockLength; i++)
                {
                    statePoints[(int)switchStates[i]].Add(detectorData[detector].PointValues[i]);
                }

                // * calculate the channel values *
                int subLength = blockLength / numStates;
                double[,] channelValues = new double[numStates, subLength];
                for (int channel = 0; channel < numStates; channel++)
                {
                    for (int subIndex = 0; subIndex < subLength; subIndex++)
                    {
                        double chanVal = 0;
                        for (int i = 0; i < numStates; i++) chanVal +=
                            stateSigns[channel, i] * statePoints[i][subIndex];
                        chanVal /= (double)numStates;
                        channelValues[channel, subIndex] = chanVal;
                    }
                }

                //* calculate the channel means *
                double[] channelMeans = new double[numStates];
                for (int channel = 0; channel < numStates; channel++)
                {
                    double total = 0;
                    for (int i = 0; i < subLength; i++) total += channelValues[channel, i];
                    total /= blockLength / numStates;
                    channelMeans[channel] = total;
                }
                dcv.Values = channelMeans;

                //* calculate the channel errors *
                double[] channelErrors = new double[numStates];
                for (int channel = 0; channel < numStates; channel++)
                {
                    double total = 0;
                    for (int i = 0; i < subLength; i++)
                        total += Math.Pow(channelValues[channel, i] - channelMeans[channel], 2);
                    total /= subLength * (subLength - 1);
                    total = Math.Sqrt(total);
                    channelErrors[channel] = total;
                }
                dcv.Errors = channelErrors;

                db.ChannelValues.Add(dcv);
            }

            return db;
        }

        // DemodulateBlockNL augments the channel values returned by DemodulateBlock
        // with several non-linear combinations of channels (E.B/DB, the correction, etc).
        // These non-linear channels are calculated point-by-point for the TOF and then
        // integrated according to the Demodulation config. This is calculated for
        // only the asymmetry detector.
        public DemodulatedBlock DemodulateBlockNL(Block b, DemodulationConfig config)
        {
            // Start with the standard demodulated block
            DemodulatedBlock dblock = DemodulateBlock(b, config);
            
            // TOF-demodulate the asymmetry detector
            // TOF demodulate the block to get the channel wiggles
            // but only do it for the important channels (to calculate the
            // special nonlinear terms we need)
            TOFDemodulatedBlock tdb = TOFDemodulateBlock(b, "asymmetry", false);
            TOFChannelSet tcs = tdb.TOFChannels;

            // get hold of the gating data
            GatedDetectorExtractSpec gate = config.GatedDetectorExtractSpecs["asymmetry"];
            int asymmetryIndex = b.detectors.IndexOf("asymmetry");

            // gate the special channels
            TOFChannel edmDB = (TOFChannel)tcs.GetChannel("EDMDB");
            double edmDBG = edmDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel corrDB = (TOFChannel)tcs.GetChannel("CORRDB");
            double corrDBG = corrDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel edmCorrDB = (TOFChannel)tcs.GetChannel("EDMCORRDB");
            double edmCorrDBG = edmCorrDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel corrDB_old = (TOFChannel)tcs.GetChannel("CORRDB_OLD");
            double corrDBG_old = corrDB_old.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel edmCorrDB_old = (TOFChannel)tcs.GetChannel("EDMCORRDB_OLD");
            double edmCorrDBG_old = edmCorrDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel rf1fDB = (TOFChannel)tcs.GetChannel("RF1FDB");
            double rf1fDBG = rf1fDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel rf2fDB = (TOFChannel)tcs.GetChannel("RF2FDB");
            double rf2fDBG = rf2fDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel rf1fDBDB = (TOFChannel)tcs.GetChannel("RF1FDBDB");
            double rf1fDBDBG = rf1fDBDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel rf2fDBDB = (TOFChannel)tcs.GetChannel("RF2FDBDB");
            double rf2fDBDBG = rf2fDBDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel rf1aDB = (TOFChannel)tcs.GetChannel("RF1ADB");
            double rf1aDBG = rf1aDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel rf2aDB = (TOFChannel)tcs.GetChannel("RF2ADB");
            double rf2aDBG = rf2aDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel rf1aDBDB = (TOFChannel)tcs.GetChannel("RF1ADBDB");
            double rf1aDBDBG = rf1aDBDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel rf2aDBDB = (TOFChannel)tcs.GetChannel("RF2ADBDB");
            double rf2aDBDBG = rf2aDBDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel lf1DB = (TOFChannel)tcs.GetChannel("LF1DB");
            double lf1DBG = lf1DB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel lf1DBDB = (TOFChannel)tcs.GetChannel("LF1DBDB");
            double lf1DBDBG = lf1DBDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel lf2DB = (TOFChannel)tcs.GetChannel("LF2DB");
            double lf2DBG = lf2DB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel lf2DBDB = (TOFChannel)tcs.GetChannel("LF2DBDB");
            double lf2DBDBG = lf2DBDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel BDB = (TOFChannel)tcs.GetChannel("BDB");
            double BDBG = BDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel erf1fDB = (TOFChannel)tcs.GetChannel("ERF1FDB");
            double erf1fDBG = erf1fDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel erf2fDB = (TOFChannel)tcs.GetChannel("ERF2FDB");
            double erf2fDBG = erf2fDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel erf1fDBDB = (TOFChannel)tcs.GetChannel("ERF1FDBDB");
            double erf1fDBDBG = erf1fDBDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel erf2fDBDB = (TOFChannel)tcs.GetChannel("ERF2FDBDB");
            double erf2fDBDBG = erf2fDBDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel brf1fCorrDB = (TOFChannel)tcs.GetChannel("BRF1FCORRDB");
            double brf1fCorrDBG = brf1fCorrDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel brf2fCorrDB = (TOFChannel)tcs.GetChannel("BRF2FCORRDB");
            double brf2fCorrDBG = brf2fCorrDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);

            // we bodge the errors, which aren't really used for much anyway
            // by just using the error from the normal dblock. I ignore the error in DB.
            // I use the simple correction error for the full correction. Doesn't much matter.
            DetectorChannelValues dcv = dblock.ChannelValues[asymmetryIndex];

            double edmDBE = dcv.GetError(new string[] { "E", "B" }) / dcv.GetValue(new string[] { "DB" });
            double corrDBE = Math.Sqrt(
                Math.Pow(dcv.GetValue(new string[] { "E", "DB" }) * dcv.GetError(new string[] { "B" }), 2) +
                Math.Pow(dcv.GetValue(new string[] { "B" }) * dcv.GetError(new string[] { "E", "DB" }), 2))
                / Math.Pow(dcv.GetValue(new string[] { "DB" }), 2);
            double edmCorrDBE = Math.Sqrt(Math.Pow(edmDBE, 2) + Math.Pow(corrDBE, 2));

            double rf1fDBE = dcv.GetError(new string[] { "RF1F" }) / dcv.GetValue(new string[] { "DB" });
            double rf2fDBE = dcv.GetError(new string[] { "RF2F" }) / dcv.GetValue(new string[] { "DB" });
            double rf1fDBDBE = dcv.GetError(new string[] { "DB", "RF1F" }) / dcv.GetValue(new string[] { "DB" });
            double rf2fDBDBE = dcv.GetError(new string[] { "DB", "RF2F" }) / dcv.GetValue(new string[] { "DB" });

            double rf1aDBE = dcv.GetError(new string[] { "RF1A" }) / dcv.GetValue(new string[] { "DB" });
            double rf2aDBE = dcv.GetError(new string[] { "RF2A" }) / dcv.GetValue(new string[] { "DB" });
            double rf1aDBDBE = dcv.GetError(new string[] { "DB", "RF1A" }) / dcv.GetValue(new string[] { "DB" });
            double rf2aDBDBE = dcv.GetError(new string[] { "DB", "RF2A" }) / dcv.GetValue(new string[] { "DB" });

            double lf1DBE = dcv.GetError(new string[] { "LF1" }) / dcv.GetValue(new string[] { "DB" });
            double lf1DBDBE = dcv.GetError(new string[] { "DB", "LF1" }) / dcv.GetValue(new string[] { "DB" });
            double lf2DBE = dcv.GetError(new string[] { "LF2" }) / dcv.GetValue(new string[] { "DB" });
            double lf2DBDBE = dcv.GetError(new string[] { "DB", "LF2" }) / dcv.GetValue(new string[] { "DB" });

            double brf1fDBE = dcv.GetError(new string[] { "B", "RF1F" }) / dcv.GetValue(new string[] { "DB" });
            double brf2fDBE = dcv.GetError(new string[] { "B", "RF2F" }) / dcv.GetValue(new string[] { "DB" });
            double erf1fDBE = dcv.GetError(new string[] { "E", "RF1F" }) / dcv.GetValue(new string[] { "DB" });
            double erf2fDBE = dcv.GetError(new string[] { "E", "RF2F" }) / dcv.GetValue(new string[] { "DB" });
            double erf1fDBDBE = dcv.GetError(new string[] { "E", "DB", "RF1F" }) / dcv.GetValue(new string[] { "DB" });
            double erf2fDBDBE = dcv.GetError(new string[] { "E", "DB", "RF2F" }) / dcv.GetValue(new string[] { "DB" });
            double BDBE = dcv.GetError(new string[] { "B" }) / dcv.GetValue(new string[] { "DB" });

            // stuff the data into the dblock
            dblock.ChannelValues[asymmetryIndex].SpecialValues["EDMDB"] = new double[] { edmDBG, edmDBE };
            dblock.ChannelValues[asymmetryIndex].SpecialValues["CORRDB"] = new double[] { corrDBG, corrDBE };
            dblock.ChannelValues[asymmetryIndex].SpecialValues["EDMCORRDB"] = new double[] { edmCorrDBG, edmCorrDBE };
            dblock.ChannelValues[asymmetryIndex].SpecialValues["CORRDB_OLD"] = new double[] { corrDBG_old, corrDBE };
            dblock.ChannelValues[asymmetryIndex].SpecialValues["EDMCORRDB_OLD"] = new double[] { edmCorrDBG_old, edmCorrDBE };
            dblock.ChannelValues[asymmetryIndex].SpecialValues["RF1FDB"] = new double[] { rf1fDBG, rf1fDBE };
            dblock.ChannelValues[asymmetryIndex].SpecialValues["RF2FDB"] = new double[] { rf2fDBG, rf2fDBE };
            dblock.ChannelValues[asymmetryIndex].SpecialValues["RF1FDBDB"] = new double[] { rf1fDBDBG, rf1fDBDBE };
            dblock.ChannelValues[asymmetryIndex].SpecialValues["RF2FDBDB"] = new double[] { rf2fDBDBG, rf2fDBDBE };
            dblock.ChannelValues[asymmetryIndex].SpecialValues["RF1ADB"] = new double[] { rf1aDBG, rf1aDBE };
            dblock.ChannelValues[asymmetryIndex].SpecialValues["RF2ADB"] = new double[] { rf2aDBG, rf2aDBE };
            dblock.ChannelValues[asymmetryIndex].SpecialValues["RF1ADBDB"] = new double[] { rf1aDBDBG, rf1aDBDBE };
            dblock.ChannelValues[asymmetryIndex].SpecialValues["RF2ADBDB"] = new double[] { rf2aDBDBG, rf2aDBDBE };
            dblock.ChannelValues[asymmetryIndex].SpecialValues["BRF1FCORRDB"] = new double[] { brf1fCorrDBG, brf1fDBE };
            dblock.ChannelValues[asymmetryIndex].SpecialValues["BRF2FCORRDB"] = new double[] { brf2fCorrDBG, brf2fDBE };
            dblock.ChannelValues[asymmetryIndex].SpecialValues["ERF1FDB"] = new double[] { erf1fDBG, erf1fDBE };
            dblock.ChannelValues[asymmetryIndex].SpecialValues["ERF2FDB"] = new double[] { erf2fDBG, erf2fDBE };
            dblock.ChannelValues[asymmetryIndex].SpecialValues["ERF1FDBDB"] = new double[] { erf1fDBDBG, erf1fDBDBE };
            dblock.ChannelValues[asymmetryIndex].SpecialValues["ERF2FDBDB"] = new double[] { erf2fDBDBG, erf2fDBDBE };
            dblock.ChannelValues[asymmetryIndex].SpecialValues["LF1DB"] = new double[] { lf1DBG, lf1DBE };
            dblock.ChannelValues[asymmetryIndex].SpecialValues["LF1DBDB"] = new double[] { lf1DBDBG, lf1DBDBE };
            dblock.ChannelValues[asymmetryIndex].SpecialValues["LF2DB"] = new double[] { lf2DBG, lf2DBE };
            dblock.ChannelValues[asymmetryIndex].SpecialValues["LF2DBDB"] = new double[] { lf2DBDBG, lf2DBDBE };
            dblock.ChannelValues[asymmetryIndex].SpecialValues["BDB"] = new double[] { BDBG, BDBE };

            return dblock;
        }

        public TOFDemodulatedBlock TOFDemodulateBlock(Block b, string detector, bool allChannels)
        {
            if (!b.detectors.Contains("asymmetry")) AddDetectorsToBlock(b, true);

            int detectorIndex = b.detectors.IndexOf(detector);
            if (detectorIndex == -1) return null;

            //edm factor calculation
            double dbStep = ((AnalogModulation)b.Config.GetModulationByName("DB")).Step;
            double magCal = (double)b.Config.Settings["magnetCalibration"];
            double eField = cField((double)b.Config.Settings["ePlus"], (double)b.Config.Settings["eMinus"]);//arguments are in volts not kV
            double edmFactor = (bohrMagneton * dbStep * magCal * Math.Pow(10, -9)) /
                        (electronCharge * saturatedEffectiveField * polarisationFactor(eField));

            // *** demodulate channels ***
            // ** build the list of modulations **
            List<string> modNames = new List<string>();
            List<Waveform> modWaveforms = new List<Waveform>();
            foreach (AnalogModulation mod in b.Config.AnalogModulations)
            {
                modNames.Add(mod.Name);
                modWaveforms.Add(mod.Waveform);
            }
            foreach (DigitalModulation mod in b.Config.DigitalModulations)
            {
                modNames.Add(mod.Name);
                modWaveforms.Add(mod.Waveform);
            }
            foreach (TimingModulation mod in b.Config.TimingModulations)
            {
                modNames.Add(mod.Name);
                modWaveforms.Add(mod.Waveform);
            }
            // ** work out the switch state for each point **
            int blockLength = modWaveforms[0].Length;
            List<bool[]> wfBits = new List<bool[]>();
            foreach (Waveform wf in modWaveforms) wfBits.Add(wf.Bits);
            List<uint> switchStates = new List<uint>(blockLength);
            for (int i = 0; i < blockLength; i++)
            {
                uint switchState = 0;
                for (int j = 0; j < wfBits.Count; j++)
                {
                    if (wfBits[j][i]) switchState += (uint)Math.Pow(2, j);
                }
                switchStates.Add(switchState);
            }
            // pre-calculate the state signs for each analysis channel
            // the first index selects the analysis channel, the second the switchState
            int numStates = (int)Math.Pow(2, modWaveforms.Count);
            bool[,] stateSigns = new bool[numStates, numStates];
            // make a BlockDemodulator just to use its stateSign code
            // They should probably share a base class.
            BlockDemodulator bd = new BlockDemodulator();
            for (uint i = 0; i < numStates; i++)
            {
                for (uint j = 0; j < numStates; j++)
                {
                    stateSigns[i, j] = (bd.stateSign(j, i) == 1);
                }
            }

            TOFChannelSet tcs = new TOFChannelSet();
            // By setting all channels to false only a limited number of channels are analysed,
            // namely those required to extract the edm (and the correction term). This speeds
            // up the execution enormously when the BlockTOFDemodulator is used by the
            // BlockDemodulator for calculating the non-linear channel combinations.
            //int[] channelsToAnalyse;
            List<int> channelsToAnalyse;
            if (allChannels)
            {
                //channelsToAnalyse = new int[numStates];
                channelsToAnalyse = new List<int>();
                //for (int i = 0; i < numStates; i++) channelsToAnalyse[i] = i;
                for (int i = 0; i < numStates; i++) channelsToAnalyse.Add(i);
            }
            else
            {
                // just the essential channels - this code is a little awkward because, like
                // so many bits of the analysis code, it was added long after the original
                // code was written, and goes against some assumptions that were made back then!
                int bIndex = modNames.IndexOf("B");
                int dbIndex = modNames.IndexOf("DB");
                int eIndex = modNames.IndexOf("E");
                int rf1fIndex = modNames.IndexOf("RF1F");
                int rf2fIndex = modNames.IndexOf("RF2F");
                int rf1aIndex = modNames.IndexOf("RF1A");
                int rf2aIndex = modNames.IndexOf("RF2A");
                int lf1Index = modNames.IndexOf("LF1");
                int lf2Index = modNames.IndexOf("LF2");

                int sigChannel = 0;
                int bChannel = (1 << bIndex);
                int dbChannel = (1 << dbIndex);
                int ebChannel = (1 << eIndex) + (1 << bIndex);
                int edbChannel = (1 << eIndex) + (1 << dbIndex);
                int dbrf1fChannel = (1 << dbIndex) + (1 << rf1fIndex);
                int dbrf2fChannel = (1 << dbIndex) + (1 << rf2fIndex);
                int brf1fChannel = (1 << bIndex) + (1 << rf1fIndex);
                int brf2fChannel = (1 << bIndex) + (1 << rf2fIndex);
                int edbrf1fChannel = (1 << eIndex) + (1 << dbIndex) + (1 << rf1fIndex);
                int edbrf2fChannel = (1 << eIndex) + (1 << dbIndex) + (1 << rf2fIndex);
                int ebdbChannel = (1 << eIndex) + (1 << bIndex) + (1 << dbIndex);
                int rf1fChannel = (1 << rf1fIndex);
                int rf2fChannel = (1 << rf2fIndex);
                int erf1fChannel = (1 << eIndex) + (1 << rf1fIndex);
                int erf2fChannel = (1 << eIndex) + (1 << rf2fIndex);
                int rf1aChannel = (1 << rf1aIndex);
                int rf2aChannel = (1 << rf2aIndex);
                int dbrf1aChannel = (1 << dbIndex) + (1 << rf1aIndex);
                int dbrf2aChannel = (1 << dbIndex) + (1 << rf2aIndex);

                channelsToAnalyse = new List<int>() { sigChannel, bChannel, dbChannel, ebChannel, edbChannel, dbrf1fChannel,
                    dbrf2fChannel, brf1fChannel, brf2fChannel, edbrf1fChannel, edbrf2fChannel, ebdbChannel,
                    rf1fChannel, rf2fChannel, erf1fChannel, erf2fChannel, rf1aChannel, rf2aChannel, dbrf1aChannel,
                    dbrf2aChannel,
                };

                if (lf1Index != -1) // Index = -1 if "LF1" not found
                {
                    int lf1Channel = (1 << lf1Index);
                    channelsToAnalyse.Add(lf1Channel);
                    int dblf1Channel = (1 << dbIndex) + (1 << lf1Index);
                    channelsToAnalyse.Add(dblf1Channel);
                }

                if (lf2Index != -1) // Index = -1 if "LF2" not found
                {
                    int lf2Channel = (1 << lf2Index);
                    channelsToAnalyse.Add(lf2Channel);
                    int dblf2Channel = (1 << dbIndex) + (1 << lf2Index);
                    channelsToAnalyse.Add(dblf2Channel);
                }

                //channelsToAnalyse = new int[] { bChannel, dbChannel, ebChannel, edbChannel, dbrf1fChannel,
                //    dbrf2fChannel, brf1fChannel, brf2fChannel, edbrf1fChannel, edbrf2fChannel, ebdbChannel,
                //    rf1fChannel, rf2fChannel, erf1fChannel, erf2fChannel, rf1aChannel, rf2aChannel, dbrf1aChannel,
                //    dbrf2aChannel, lf1Channel, dblf1Channel, lf2Channel, dblf2Channel
                //};
            }

            foreach (int channel in channelsToAnalyse)
            {
                // generate the Channel
                TOFChannel tc = new TOFChannel();
                TOF tOn = new TOF();
                TOF tOff = new TOF();
                for (int i = 0; i < blockLength; i++)
                {
                    if (stateSigns[channel, switchStates[i]]) tOn += ((TOF)((EDMPoint)(b.Points[i])).Shot.TOFs[detectorIndex]);
                    else tOff += ((TOF)((EDMPoint)(b.Points[i])).Shot.TOFs[detectorIndex]);
                }
                tOn /= blockLength;
                tOff /= blockLength;
                tc.On = tOn;
                tc.Off = tOff;
                // This "if" is to take care of the case of the "SIG" channel, for which there
                // is no off TOF.
                if (tc.Off.Length != 0) tc.Difference = tc.On - tc.Off;
                else tc.Difference = tc.On;

                // add the Channel to the ChannelSet
                List<string> usedSwitches = new List<string>();
                for (int i = 0; i < modNames.Count; i++)
                    if ((channel & (1 << i)) != 0) usedSwitches.Add(modNames[i]);
                string[] channelName = usedSwitches.ToArray();
                // the SIG channel has a special name
                if (channel == 0) channelName = new string[] { "SIG" };
                tcs.AddChannel(channelName, tc);
            }
            // ** add the special channels **

            // extract the TOFChannels that we need.
            TOFChannel c_eb = (TOFChannel)tcs.GetChannel(new string[] { "E", "B" });
            TOFChannel c_edb = (TOFChannel)tcs.GetChannel(new string[] { "E", "DB" });
            TOFChannel c_dbrf1f = (TOFChannel)tcs.GetChannel(new string[] { "DB", "RF1F" });
            TOFChannel c_dbrf2f = (TOFChannel)tcs.GetChannel(new string[] { "DB", "RF2F" });
            TOFChannel c_b = (TOFChannel)tcs.GetChannel(new string[] { "B" });
            TOFChannel c_db = (TOFChannel)tcs.GetChannel(new string[] { "DB" });
            TOFChannel c_sig = (TOFChannel)tcs.GetChannel(new string[] { "SIG" });

            TOFChannel c_brf1f = (TOFChannel)tcs.GetChannel(new string[] { "B", "RF1F" });
            TOFChannel c_brf2f = (TOFChannel)tcs.GetChannel(new string[] { "B", "RF2F" });
            TOFChannel c_edbrf1f = (TOFChannel)tcs.GetChannel(new string[] { "E", "DB", "RF1F" });
            TOFChannel c_edbrf2f = (TOFChannel)tcs.GetChannel(new string[] { "E", "DB", "RF2F" });
            TOFChannel c_ebdb = (TOFChannel)tcs.GetChannel(new string[] { "E", "B", "DB" });

            TOFChannel c_rf1f = (TOFChannel)tcs.GetChannel(new string[] { "RF1F" });
            TOFChannel c_rf2f = (TOFChannel)tcs.GetChannel(new string[] { "RF2F" });

            TOFChannel c_erf1f = (TOFChannel)tcs.GetChannel(new string[] { "E", "RF1F" });
            TOFChannel c_erf2f = (TOFChannel)tcs.GetChannel(new string[] { "E", "RF2F" });

            TOFChannel c_rf1a = (TOFChannel)tcs.GetChannel(new string[] { "RF1A" });
            TOFChannel c_rf2a = (TOFChannel)tcs.GetChannel(new string[] { "RF2A" });
            TOFChannel c_dbrf1a = (TOFChannel)tcs.GetChannel(new string[] { "DB", "RF1A" });
            TOFChannel c_dbrf2a = (TOFChannel)tcs.GetChannel(new string[] { "DB", "RF2A" });

            TOFChannel c_lf1;
            TOFChannel c_dblf1;
            if (modNames.IndexOf("LF1") == -1) // Index = -1 if "LF1" not found
            {
                TOF tofTemp = new TOF();
                TOFChannel tcTemp = new TOFChannel();
                // For SOME blocks there is no LF1 channel (and hence switch states).
                // To get around this problem I will populate the TOFChannel with "SIG"
                // It will then be obvious in the analysis when LF1 takes on real values.
                for (int i = 0; i < blockLength; i++)
                {
                    tofTemp += ((TOF)((EDMPoint)(b.Points[i])).Shot.TOFs[detectorIndex]);
                }
                tofTemp /= (blockLength / 2);

                tcTemp.On = tofTemp;
                tcTemp.Off = tofTemp;
                tcTemp.Difference = tofTemp;

                c_lf1 = tcTemp;
                c_dblf1 = tcTemp;
            }
            else
            {
                c_lf1 = (TOFChannel)tcs.GetChannel(new string[] { "LF1" });
                c_dblf1 = (TOFChannel)tcs.GetChannel(new string[] { "DB", "LF1" });
            }


            TOFChannel c_lf2;
            TOFChannel c_dblf2;
            if (modNames.IndexOf("LF2") == -1) // Index = -1 if "LF2" not found
            {
                TOF tofTemp = new TOF();
                TOFChannel tcTemp = new TOFChannel();
                // For many blocks there is no LF2 channel (and hence switch states).
                // To get around this problem I will populate the TOFChannel with "SIG"
                // It will then be obvious in the analysis when LF2 takes on real values.
                for (int i = 0; i < blockLength; i++)
                {
                    tofTemp += ((TOF)((EDMPoint)(b.Points[i])).Shot.TOFs[detectorIndex]);
                }
                tofTemp /= (blockLength / 2);

                tcTemp.On = tofTemp;
                tcTemp.Off = tofTemp;
                tcTemp.Difference = tofTemp;

                c_lf2 = tcTemp;
                c_dblf2 = tcTemp;
            }
            else
            {
                c_lf2 = (TOFChannel)tcs.GetChannel(new string[] { "LF2" });
                c_dblf2 = (TOFChannel)tcs.GetChannel(new string[] { "DB", "LF2" });
            }



            // work out some intermediate terms for the full, corrected edm. The names
            // refer to the joint power of c_db and c_b in the term.
            TOFChannel squaredTerms = (((c_db * c_db) - (c_dbrf1f * c_dbrf1f) - (c_dbrf2f * c_dbrf2f)) * c_eb)
                                        - (c_b * c_db * c_edb);

            // this is missing the term /beta c_db c_ebdb at the moment, mainly because
            // I've no idea what beta should be.
            TOFChannel linearTerms = (c_b * c_dbrf1f * c_edbrf1f) + (c_b * c_dbrf2f * c_edbrf2f)
                - (c_db * c_brf1f * c_edbrf1f) - (c_db * c_brf2f * c_edbrf2f);

            TOFChannel preDenominator = (c_db * c_db * c_db)
                + (c_dbrf1f * c_edb * c_edbrf1f) + (c_dbrf1f * c_edb * c_edbrf1f)
                + (c_dbrf2f * c_edb * c_edbrf2f) + (c_dbrf2f * c_edb * c_edbrf2f)
                - c_db * (
                    (c_dbrf1f * c_dbrf1f) + (c_dbrf2f * c_dbrf2f) + (c_edb * c_edb)
                        + (c_edbrf1f * c_edbrf1f) + (c_edbrf2f * c_edbrf2f)
                    );

            // it's important when working out the non-linear channel
            // combinations to always keep them dimensionless. If you
            // don't you'll run into trouble with integral vs. average
            // signal.
            TOFChannel edmDB = (c_eb / c_db) * edmFactor;
            tcs.AddChannel(new string[] { "EDMDB" }, edmDB);

            // The corrected edm channel. This should be proportional to the edm phase.
            TOFChannel edmCorrDB = ((squaredTerms + linearTerms) / preDenominator) * edmFactor;
            tcs.AddChannel(new string[] { "EDMCORRDB" }, edmCorrDB);

            // It's useful to have an estimate of the size of the correction. Here
            // we return the difference between the corrected edm channel and the
            // naive guess, edmDB.
            TOFChannel correctionDB = edmCorrDB - edmDB;
            tcs.AddChannel(new string[] { "CORRDB" }, correctionDB);

            // The "old" correction that just corrects for the E-correlated amplitude change.
            // This is included in the dblocks for debugging purposes.
            TOFChannel correctionDB_old = (c_edb * c_b) / (c_db * c_db);
            tcs.AddChannel(new string[] { "CORRDB_OLD" }, correctionDB_old);

            TOFChannel edmCorrDB_old = edmDB - correctionDB_old;
            tcs.AddChannel(new string[] { "EDMCORRDB_OLD" }, edmCorrDB_old);

            // Normalised RFxF channels.
            TOFChannel rf1fDB = c_rf1f / c_db;
            tcs.AddChannel(new string[] { "RF1FDB" }, rf1fDB);

            TOFChannel rf2fDB = c_rf2f / c_db;
            tcs.AddChannel(new string[] { "RF2FDB" }, rf2fDB);

            // And RFxF.DB channels, again normalised to DB. The naming of these channels is quite
            // unfortunate, but it's just tough.
            TOFChannel rf1fDBDB = c_dbrf1f / c_db;
            tcs.AddChannel(new string[] { "RF1FDBDB" }, rf1fDBDB);

            TOFChannel rf2fDBDB = c_dbrf2f / c_db;
            tcs.AddChannel(new string[] { "RF2FDBDB" }, rf2fDBDB);

            // Normalised RFxAchannels.
            TOFChannel rf1aDB = c_rf1a / c_db;
            tcs.AddChannel(new string[] { "RF1ADB" }, rf1aDB);

            TOFChannel rf2aDB = c_rf2a / c_db;
            tcs.AddChannel(new string[] { "RF2ADB" }, rf2aDB);

            // And RFxA.DB channels, again normalised to DB. The naming of these channels is quite
            // unfortunate, but it's just tough.
            TOFChannel rf1aDBDB = c_dbrf1a / c_db;
            tcs.AddChannel(new string[] { "RF1ADBDB" }, rf1aDBDB);

            TOFChannel rf2aDBDB = c_dbrf2a / c_db;
            tcs.AddChannel(new string[] { "RF2ADBDB" }, rf2aDBDB);

            // the E.RFxF channels, normalized to DB
            TOFChannel erf1fDB = c_erf1f / c_db;
            tcs.AddChannel(new string[] { "ERF1FDB" }, erf1fDB);

            TOFChannel erf2fDB = c_erf2f / c_db;
            tcs.AddChannel(new string[] { "ERF2FDB" }, erf2fDB);

            // the E.RFxF.DB channels, normalized to DB, again dodgy naming convention.
            TOFChannel erf1fDBDB = c_edbrf1f / c_db;
            tcs.AddChannel(new string[] { "ERF1FDBDB" }, erf1fDBDB);

            TOFChannel erf2fDBDB = c_edbrf2f / c_db;
            tcs.AddChannel(new string[] { "ERF2FDBDB" }, erf2fDBDB);

            // the LF1 channel, normalized to DB
            TOFChannel lf1DB = c_lf1 / c_db;
            tcs.AddChannel(new string[] { "LF1DB" }, lf1DB);

            TOFChannel lf1DBDB = c_dblf1 / c_db;
            tcs.AddChannel(new string[] { "LF1DBDB" }, lf1DBDB);

            // the LF2 channel, normalized to DB
            TOFChannel lf2DB = c_lf2 / c_db;
            tcs.AddChannel(new string[] { "LF2DB" }, lf2DB);

            TOFChannel lf2DBDB = c_dblf2 / c_db;
            tcs.AddChannel(new string[] { "LF2DBDB" }, lf2DBDB);

            TOFChannel bDB = c_b / c_db;
            tcs.AddChannel(new string[] { "BDB" }, bDB);

            // we also need to extract the rf-step induced phase shifts. These come out in the
            // B.RFxF channels, but like the edm, need to be corrected. I'm going to use just the
            // simplest level of correction for these.

            TOFChannel brf1fCorrDB = (c_brf1f / c_db) - ((c_b * c_dbrf1f) / (c_db * c_db));
            tcs.AddChannel(new string[] { "BRF1FCORRDB" }, brf1fCorrDB);

            TOFChannel brf2fCorrDB = (c_brf2f / c_db) - ((c_b * c_dbrf2f) / (c_db * c_db));
            tcs.AddChannel(new string[] { "BRF2FCORRDB" }, brf2fCorrDB);

            //Some extra channels for various shot noise calculations, these are a bit weird


            tcs.AddChannel(new string[] { "SIGNL" }, c_sig);

            tcs.AddChannel(new string[] { "ONEOVERDB" }, 1 / c_db);

            TOFChannel dbSigNL = new TOFChannel();
            dbSigNL.On = c_db.On / c_sig.On;
            dbSigNL.Off = c_db.Off / c_sig.On; ;
            dbSigNL.Difference = c_db.Difference / c_sig.Difference;
            tcs.AddChannel(new string[] { "DBSIG" }, dbSigNL);


            TOFChannel dbdbSigSigNL = dbSigNL * dbSigNL;
            tcs.AddChannel(new string[] { "DBDBSIGSIG" }, dbdbSigSigNL);


            TOFChannel SigdbdbNL = new TOFChannel();
            SigdbdbNL.On = c_sig.On / (c_db.On * c_db.On);
            SigdbdbNL.Off = c_sig.On / (c_db.Off * c_db.Off);
            SigdbdbNL.Difference = c_sig.Difference / (c_db.Difference * c_db.Difference);
            tcs.AddChannel(new string[] { "SIGDBDB" }, SigdbdbNL);

            TOFDemodulatedBlock tdb = new TOFDemodulatedBlock();
            tdb.Config = b.Config;
            tdb.Detector = detector;
            tdb.DetectorIndex = detectorIndex;
            tdb.DetectorCalibration = ((TOF)((EDMPoint)b.Points[0]).Shot.TOFs[b.detectors.IndexOf(detector)]).Calibration;
            tdb.TimeStamp = b.TimeStamp;
            tdb.TOFChannels = tcs;
            return tdb;
        }

        // calculate, for a given analysis channel, whether a given state contributes
        // positively or negatively
        public int stateSign(uint state, uint analysisChannel)
        {
            uint maskedState = state & analysisChannel;
            if (parity(maskedState) == parity(analysisChannel)) return 1;
            else return -1;
        }

        // now _this_ is code! Old skool!!!
        public uint parity(uint p)
        {
            p = p ^ (p >> 16);
            p = p ^ (p >> 8);
            p = p ^ (p >> 4);
            p = p ^ (p >> 2);
            p = p ^ (p >> 1);
            return p & 1;
        }

        private static double polarisationFactor(double electricField/*in kV/cm*/)
        {
            double polFactor = 0.00001386974812686904
                + 0.09020507207607358 * electricField
                + 0.0008134261949342792 * Math.Pow(electricField, 2)
                - 0.001365930559363722 * Math.Pow(electricField, 3)
                + 0.00016467328012807663 * Math.Pow(electricField, 4)
                - 9.53482 * Math.Pow(10, -6) * Math.Pow(electricField, 5)
                + 2.804041112473679 * Math.Pow(10, -7) * Math.Pow(electricField, 6)
                - 3.352604355536456 * Math.Pow(10, -9) * Math.Pow(electricField, 7);

            return polFactor;
        }

        private static double cField(double ePlus, double eMinus)//voltage in volts returns kV/cm
        {
            double efield = (ePlus - eMinus) / plateSpacing;
            //double efield = 4/ plateSpacing;
            return efield / 1000;
        }
    }
}