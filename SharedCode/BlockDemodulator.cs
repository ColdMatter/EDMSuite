using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Data;
using Data.EDM;
using EDMConfig;

namespace Analysis.EDM
{
    public class BlockDemodulator
    {
        private string[] MOLECULE_DETECTORS = new string[3] { "asymmetry", "detAScaled", "detBNoBackground" };//"bottomProbeScaled", "topProbeNoBackground" };

        public DemodulatedBlock DemodulateBlock(Block b, DemodulationConfig demodulationConfig, int[] tofChannelsToAnalyse)
        {
            if (!b.detectors.Contains("asymmetry")) b.AddDetectorsToBlock();

            int blockLength = b.Points.Count;

            DemodulatedBlock db = new DemodulatedBlock(b.TimeStamp, b.Config, demodulationConfig);

            Dictionary<string, double[]> pointDetectorData = new Dictionary<string, double[]>();
            foreach (string d in demodulationConfig.GatedDetectors)
            {
                pointDetectorData.Add(d, GetGatedDetectorData(b, d, demodulationConfig.Gates.GetGate(d)));
            }
            foreach (string d in demodulationConfig.PointDetectors)
            {
                pointDetectorData.Add(d, GetPointDetectorData(b, d));
            }

            Dictionary<string, TOF[]> tofDetectorData = new Dictionary<string, TOF[]>();
            foreach (string d in demodulationConfig.TOFDetectors)
            {
                tofDetectorData.Add(d, GetTOFDetectorData(b, d));
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

            // --This is done for each point/gated detector--
            foreach (string d in pointDetectorData.Keys)
            {
                int detectorIndex = b.detectors.IndexOf(d);

                // We obtain one Channel Set for each detector
                ChannelSet<PointWithError> channelSet = new ChannelSet<PointWithError>();

                // Detector calibration
                double calibration = ((TOF)((EDMPoint)b.Points[0]).Shot.TOFs[detectorIndex]).Calibration;

                // Divide points into bins depending on switch state
                List<List<double>> statePoints = new List<List<double>>(numStates);
                for (int i = 0; i < numStates; i++) statePoints.Add(new List<double>(blockLength / numStates));
                for (int i = 0; i < blockLength; i++) statePoints[(int)switchStates[i]].Add(pointDetectorData[b.detectors[detectorIndex]][i]);
                int subLength = blockLength / numStates;

                // For each analysis channel, calculate the mean and standard error, then add to ChannelSet
                for (int channel = 0; channel < numStates; channel++)
                {
                    RunningStatistics stats = new RunningStatistics();
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
                        stats.Push(onVal - offVal);
                    }

                    PointWithError pointWithError = new PointWithError() { Value = stats.Mean, Error = stats.StandardErrorOfSampleMean };

                    // add the channel to the ChannelSet
                    List<string> usedSwitches = new List<string>();
                    for (int i = 0; i < modulations.Count; i++)
                        if ((channel & (1 << i)) != 0) usedSwitches.Add(modulations[i].Name);
                    string[] channelName = usedSwitches.ToArray();
                    // the SIG channel has a special name
                    if (channel == 0) channelName = new string[] { "SIG" };
                    channelSet.AddChannel(channelName, pointWithError);
                }
                            
                // Add the ChannelSet to the demodulated block
                db.AddDetector(b.detectors[detectorIndex], calibration, channelSet);
            }

            // --This is done for each TOF detector--
            foreach (string d in tofDetectorData.Keys)
            {
                int detectorIndex = b.detectors.IndexOf(d);

                // We obtain one Channel Set for each detector
                ChannelSet<TOFWithError> channelSet = new ChannelSet<TOFWithError>();

                // Detector calibration
                double calibration = ((TOF)((EDMPoint)b.Points[0]).Shot.TOFs[detectorIndex]).Calibration;

                // Divide TOFs into bins depending on switch state
                List<List<TOF>> statePoints = new List<List<TOF>>(numStates);
                for (int i = 0; i < numStates; i++) statePoints.Add(new List<TOF>(blockLength / numStates));
                for (int i = 0; i < blockLength; i++) statePoints[(int)switchStates[i]].Add(tofDetectorData[b.detectors[detectorIndex]][i]);
                int subLength = blockLength / numStates;

                // For each analysis channel, calculate the mean and standard error, then add to ChannelSet
                foreach (int channel in tofChannelsToAnalyse)
                {
                    TOFAccumulator tofAccumulator = new TOFAccumulator();
                    for (int subIndex = 0; subIndex < subLength; subIndex++)
                    {
                        TOF onTOF = new TOF();
                        TOF offTOF = new TOF();
                        for (int i = 0; i < numStates; i++)
                        {
                            if (stateSigns[channel, i] == 1) onTOF += statePoints[i][subIndex];
                            else offTOF += statePoints[i][subIndex];
                        }
                        onTOF /= numStates;
                        offTOF /= numStates;
                        tofAccumulator.Add(onTOF - offTOF);
                    }

                    // add the channel to the ChannelSet
                    List<string> usedSwitches = new List<string>();
                    for (int i = 0; i < modulations.Count; i++)
                        if ((channel & (1 << i)) != 0) usedSwitches.Add(modulations[i].Name);
                    string[] channelName = usedSwitches.ToArray();
                    Console.WriteLine(string.Join(",", channelName));
                    // the SIG channel has a special name
                    if (channel == 0) channelName = new string[] { "SIG" };
                    channelSet.AddChannel(channelName, tofAccumulator.GetResult());
                }

                // If the detector is a molecule detector, add the special channels
                if (MOLECULE_DETECTORS.Contains(d)) channelSet = AppendChannelSetWithSpecialValues(channelSet);

                // Add the ChannelSet to the demodulated block
                db.AddDetector(d, calibration, channelSet);
            }

            return db;
        }

        public DemodulatedBlock DemodulateBlock(Block b, DemodulationConfig demodulationConfig)
        {
            List<Modulation> modulations = GetModulations(b);
            int numStates = (int)Math.Pow(2, modulations.Count);
            int[] channelsToAnalyse = new int[numStates];
            for (int i = 0; i < numStates; i++) channelsToAnalyse[i] = i;

            return DemodulateBlock(b, demodulationConfig, channelsToAnalyse);
        }

        public DemodulatedBlock QuickDemodulateBlock(Block b)
        {
            List<Modulation> modulations = GetModulations(b);
            List<string> modNames = new List<string>();
            foreach (Modulation modulation in modulations) modNames.Add(modulation.Name);

            int bIndex = modNames.IndexOf("B");
            int dbIndex = modNames.IndexOf("DB");
            int eIndex = modNames.IndexOf("E");
            //int rf1fIndex = modNames.IndexOf("RF1F");
            //int rf2fIndex = modNames.IndexOf("RF2F");
            //int rf1aIndex = modNames.IndexOf("RF1A");
            //int rf2aIndex = modNames.IndexOf("RF2A");
            //int lf1Index = modNames.IndexOf("LF1");

            int sigChannel = 0;
            int eChannel = (1 << eIndex);
            int bChannel = (1 << bIndex);
            int dbChannel = (1 << dbIndex);
            int ebChannel = (1 << eIndex) + (1 << bIndex);
            int edbChannel = (1 << eIndex) + (1 << dbIndex);
            int bdbChannel = (1 << bIndex) + (1 << dbIndex);
            //int dbrf1fChannel = (1 << dbIndex) + (1 << rf1fIndex);
            //int dbrf2fChannel = (1 << dbIndex) + (1 << rf2fIndex);
            //int brf1fChannel = (1 << bIndex) + (1 << rf1fIndex);
            //int brf2fChannel = (1 << bIndex) + (1 << rf2fIndex);
            //int edbrf1fChannel = (1 << eIndex) + (1 << dbIndex) + (1 << rf1fIndex);
            //int edbrf2fChannel = (1 << eIndex) + (1 << dbIndex) + (1 << rf2fIndex);
            //int bdbrf1fChannel = (1 << bIndex) + (1 << dbIndex) + (1 << rf1fIndex);
            //int bdbrf2fChannel = (1 << bIndex) + (1 << dbIndex) + (1 << rf2fIndex);
            int ebdbChannel = (1 << eIndex) + (1 << bIndex) + (1 << dbIndex);
            //int ebdbrf1fChannel = (1 << eIndex) + (1 << bIndex) + (1 << dbIndex) + (1 << rf1fIndex);
            //int ebdbrf2fChannel = (1 << eIndex) + (1 << bIndex) + (1 << dbIndex) + (1 << rf2fIndex);
            //int rf1fChannel = (1 << rf1fIndex);
            //int rf2fChannel = (1 << rf2fIndex);
            //int erf1fChannel = (1 << eIndex) + (1 << rf1fIndex);
            //int erf2fChannel = (1 << eIndex) + (1 << rf2fIndex);
            //int rf1aChannel = (1 << rf1aIndex);
            //int rf2aChannel = (1 << rf2aIndex);
            //int dbrf1aChannel = (1 << dbIndex) + (1 << rf1aIndex);
            //int dbrf2aChannel = (1 << dbIndex) + (1 << rf2aIndex);

            List<int> channelsToAnalyse = new List<int> { sigChannel, eChannel, bChannel, dbChannel, ebChannel, edbChannel, bdbChannel, ebdbChannel};//, dbrf1fChannel,
                        //    dbrf2fChannel, brf1fChannel, brf2fChannel, edbrf1fChannel, edbrf2fChannel, bdbrf1fChannel, bdbrf2fChannel,  ebdbrf1fChannel, ebdbrf2fChannel,
                        //    rf1fChannel, rf2fChannel, erf1fChannel, erf2fChannel, rf1aChannel, rf2aChannel, dbrf1aChannel,
                        //    dbrf2aChannel
                        //};

            //if (lf1Index != -1) // Index = -1 if "LF1" not found
            //{
            //    int lf1Channel = (1 << lf1Index);
            //    channelsToAnalyse.Add(lf1Channel);
            //    int dblf1Channel = (1 << dbIndex) + (1 << lf1Index);
            //    channelsToAnalyse.Add(dblf1Channel);
            //}

            return DemodulateBlock(b, DemodulationConfig.MakeLiveAnalysisConfig(), channelsToAnalyse.ToArray());
        }

        private ChannelSet<TOFWithError> AppendChannelSetWithSpecialValues(ChannelSet<TOFWithError> tcs)
        {
            ChannelSet<TOFWithError> tcsWithSpecialValues = tcs;

            // Extract the TOFChannels that we need.
            Console.WriteLine("Before first special channel");
            TOFWithError c_e = (TOFWithError)tcs.GetChannel(new string[] { "E" });
            Console.WriteLine("After first, before first double");
            TOFWithError c_eb = (TOFWithError)tcs.GetChannel(new string[] { "B", "E" });
            TOFWithError c_edb = (TOFWithError)tcs.GetChannel(new string[] { "DB", "E" });
            TOFWithError c_bdb = (TOFWithError)tcs.GetChannel(new string[] { "B", "DB" });
            Console.WriteLine("After doubles");
            //TOFWithError c_eb = (TOFWithError)tcs.GetChannel(new string[] { "E", "B" });
            //TOFWithError c_edb = (TOFWithError)tcs.GetChannel(new string[] { "E", "DB" });
            //TOFWithError c_bdb = (TOFWithError)tcs.GetChannel(new string[] { "B", "DB" });
            //TOFWithError c_dbrf1f = (TOFWithError)tcs.GetChannel(new string[] { "DB", "RF1F" });
            //TOFWithError c_dbrf2f = (TOFWithError)tcs.GetChannel(new string[] { "DB", "RF2F" });
            //TOFWithError c_e = (TOFWithError)tcs.GetChannel(new string[] { "E" });
            TOFWithError c_b = (TOFWithError)tcs.GetChannel(new string[] { "B" });
            TOFWithError c_db = (TOFWithError)tcs.GetChannel(new string[] { "DB" });
            TOFWithError c_sig = (TOFWithError)tcs.GetChannel(new string[] { "SIG" });

            //TOFWithError c_brf1f = (TOFWithError)tcs.GetChannel(new string[] { "B", "RF1F" });
            //TOFWithError c_brf2f = (TOFWithError)tcs.GetChannel(new string[] { "B", "RF2F" });
            //TOFWithError c_edbrf1f = (TOFWithError)tcs.GetChannel(new string[] { "E", "DB", "RF1F" });
            //TOFWithError c_edbrf2f = (TOFWithError)tcs.GetChannel(new string[] { "E", "DB", "RF2F" });
            //TOFWithError c_bdbrf1f = (TOFWithError)tcs.GetChannel(new string[] { "B", "DB", "RF1F" });
            //TOFWithError c_bdbrf2f = (TOFWithError)tcs.GetChannel(new string[] { "B", "DB", "RF2F" });
            //TOFWithError c_ebdb = (TOFWithError)tcs.GetChannel(new string[] { "E", "B", "DB" });  //Classic ordering of the channels...
            TOFWithError c_ebdb = (TOFWithError)tcs.GetChannel(new string[] { "B", "DB", "E" });
            Console.WriteLine("After triples");

            //TOFWithError c_ebdbrf1f = (TOFWithError)tcs.GetChannel(new string[] { "E", "B", "DB", "RF1F" });
            //TOFWithError c_ebdbrf2f = (TOFWithError)tcs.GetChannel(new string[] { "E", "B", "DB", "RF2F" });

            //TOFWithError c_rf1f = (TOFWithError)tcs.GetChannel(new string[] { "RF1F" });
            //TOFWithError c_rf2f = (TOFWithError)tcs.GetChannel(new string[] { "RF2F" });

            //TOFWithError c_erf1f = (TOFWithError)tcs.GetChannel(new string[] { "E", "RF1F" });
            //TOFWithError c_erf2f = (TOFWithError)tcs.GetChannel(new string[] { "E", "RF2F" });

            //TOFWithError c_rf1a = (TOFWithError)tcs.GetChannel(new string[] { "RF1A" });
            //TOFWithError c_rf2a = (TOFWithError)tcs.GetChannel(new string[] { "RF2A" });
            //TOFWithError c_dbrf1a = (TOFWithError)tcs.GetChannel(new string[] { "DB", "RF1A" });
            //TOFWithError c_dbrf2a = (TOFWithError)tcs.GetChannel(new string[] { "DB", "RF2A" });

            // For SOME blocks there is no LF1 channel (and hence switch states).
            // To get around this problem I will populate the TOFChannel with "SIG"
            // It will then be obvious in the analysis when LF1 takes on real values.
            //TOFWithError c_lf1;
            //TOFWithError c_dblf1;
            //if (!tcs.Channels.Contains("LF1"))
            //{
            //    c_lf1 = c_sig;
            //    c_dblf1 = c_sig;
            //}
            //else
            //{
            //    c_lf1 = (TOFWithError)tcs.GetChannel(new string[] { "LF1" });
            //    c_dblf1 = (TOFWithError)tcs.GetChannel(new string[] { "DB", "LF1" });
            //}

            // Work out the terms for the full, corrected edm. 
            TOFWithError terms = c_eb * c_db
                - c_edb * c_b + c_bdb * c_e - c_ebdb * c_sig
                //+ c_erf1f * c_bdbrf1f + c_erf2f * c_bdbrf2f
                //- c_brf1f * c_edbrf1f - c_brf2f * c_edbrf2f
                //- c_ebdbrf1f * c_rf1f - c_ebdbrf2f * c_rf2f
                ;

            TOFWithError preDenominator = c_db * c_db
                - c_edb * c_edb + c_bdb * c_bdb - c_ebdb * c_ebdb
                //+ c_bdbrf1f * c_bdbrf1f + c_bdbrf2f * c_bdbrf2f
                //- c_edbrf1f * c_edbrf1f - c_edbrf2f * c_edbrf2f
                //+ c_ebdbrf1f * c_ebdbrf1f + c_ebdbrf2f * c_ebdbrf2f
                ;

            // Work out terms for corrected edm (no rf channels)
            TOFWithError termsNoRf = c_eb * c_db - c_edb * c_b + c_bdb * c_e + c_ebdb * c_sig;
            TOFWithError preDenominatorNoRf = c_db * c_db - c_edb * c_edb + c_bdb * c_bdb - c_ebdb * c_ebdb;

            // it's important when working out the non-linear channel
            // combinations to always keep them dimensionless. If you
            // don't you'll run into trouble with integral vs. average
            // signal.
            TOFWithError edmDB = c_eb / c_db;
            tcs.AddChannel(new string[] { "EDMDB" }, edmDB);

            // The corrected edm channel. This should be proportional to the edm phase.
            TOFWithError edmCorrDB = terms / preDenominator;
            tcs.AddChannel(new string[] { "EDMCORRDB" }, edmCorrDB);

            // It's useful to have an estimate of the size of the correction. Here
            // we return the difference between the corrected edm channel and the
            // naive guess, edmDB.
            TOFWithError correctionDB = edmCorrDB - edmDB;
            tcs.AddChannel(new string[] { "CORRDB" }, correctionDB);

            // The "old" correction that just corrects for the E-correlated amplitude change.
            // This is included in the dblocks for debugging purposes.
            TOFWithError correctionDB_old = (c_edb * c_b) / (c_db * c_db);
            tcs.AddChannel(new string[] { "CORRDB_OLD" }, correctionDB_old);

            TOFWithError edmCorrDB_old = edmDB - correctionDB_old;
            tcs.AddChannel(new string[] { "EDMCORRDB_OLD" }, edmCorrDB_old);

            // The "no rf" correction that just corrects for the E/B-correlated amplitude change.
            // This is included in the dblocks for debugging purposes.
            TOFWithError edmCorrDB_norf = termsNoRf / preDenominatorNoRf;
            tcs.AddChannel(new string[] { "EDMCORRDB_NORF" }, edmCorrDB_norf);

            TOFWithError correctionDB_norf = edmCorrDB_norf - edmDB;
            tcs.AddChannel(new string[] { "CORRDB_NORF" }, correctionDB_norf);

            // Normalised RFxF channels.
            //TOFWithError rf1fDB = c_rf1f / c_db;
            //tcs.AddChannel(new string[] { "RF1FDB" }, rf1fDB);

            //TOFWithError rf2fDB = c_rf2f / c_db;
            //tcs.AddChannel(new string[] { "RF2FDB" }, rf2fDB);

            //// And RFxF.DB channels, again normalised to DB. The naming of these channels is quite
            //// unfortunate, but it's just tough.
            //TOFWithError rf1fDBDB = c_dbrf1f / c_db;
            //tcs.AddChannel(new string[] { "RF1FDBDB" }, rf1fDBDB);

            //TOFWithError rf2fDBDB = c_dbrf2f / c_db;
            //tcs.AddChannel(new string[] { "RF2FDBDB" }, rf2fDBDB);

            //// Normalised RFxAchannels.
            //TOFWithError rf1aDB = c_rf1a / c_db;
            //tcs.AddChannel(new string[] { "RF1ADB" }, rf1aDB);

            //TOFWithError rf2aDB = c_rf2a / c_db;
            //tcs.AddChannel(new string[] { "RF2ADB" }, rf2aDB);

            //// And RFxA.DB channels, again normalised to DB. The naming of these channels is quite
            //// unfortunate, but it's just tough.
            //TOFWithError rf1aDBDB = c_dbrf1a / c_db;
            //tcs.AddChannel(new string[] { "RF1ADBDB" }, rf1aDBDB);

            //TOFWithError rf2aDBDB = c_dbrf2a / c_db;
            //tcs.AddChannel(new string[] { "RF2ADBDB" }, rf2aDBDB);

            //// the E.RFxF channels, normalized to DB
            //TOFWithError erf1fDB = c_erf1f / c_db;
            //tcs.AddChannel(new string[] { "ERF1FDB" }, erf1fDB);

            //TOFWithError erf2fDB = c_erf2f / c_db;
            //tcs.AddChannel(new string[] { "ERF2FDB" }, erf2fDB);

            //// the E.RFxF.DB channels, normalized to DB, again dodgy naming convention.
            //TOFWithError erf1fDBDB = c_edbrf1f / c_db;
            //tcs.AddChannel(new string[] { "ERF1FDBDB" }, erf1fDBDB);

            //TOFWithError erf2fDBDB = c_edbrf2f / c_db;
            //tcs.AddChannel(new string[] { "ERF2FDBDB" }, erf2fDBDB);

            // the LF1 channel, normalized to DB
            //TOFWithError lf1DB = c_lf1 / c_db;
            //tcs.AddChannel(new string[] { "LF1DB" }, lf1DB);

            //TOFWithError lf1DBDB = c_dblf1 / c_db;
            //tcs.AddChannel(new string[] { "LF1DBDB" }, lf1DBDB);

            TOFWithError bDB = c_b / c_db;
            tcs.AddChannel(new string[] { "BDB" }, bDB);

            // we also need to extract the rf-step induced phase shifts. These come out in the
            // B.RFxF channels, but like the edm, need to be corrected. 

            //// Work out the terms
            //TOFWithError brf1fCorrectionTerms = c_eb * c_db
            //    - c_edb * c_b + c_bdb * c_e - c_ebdb * c_sig
            //    + c_erf1f * c_bdbrf1f + c_erf2f * c_bdbrf2f
            //    - c_brf1f * c_edbrf1f - c_brf2f * c_edbrf2f
            //    - c_ebdbrf1f * c_rf1f - c_ebdbrf2f * c_rf2f
            //    ;

            //TOFWithError brf1fPreDenominator = c_db * c_db
            //    - c_edb * c_edb + c_bdb * c_bdb - c_ebdb * c_ebdb
            //    + c_bdbrf1f * c_bdbrf1f + c_bdbrf2f * c_bdbrf2f
            //    - c_edbrf1f * c_edbrf1f - c_edbrf2f * c_edbrf2f
            //    + c_ebdbrf1f * c_ebdbrf1f + c_ebdbrf2f * c_ebdbrf2f
            //    ;

            //TOFWithError brf1fCorrDB = (c_brf1f / c_db) - ((c_b * c_dbrf1f) / (c_db * c_db));
            //tcs.AddChannel(new string[] { "BRF1FCORRDB" }, brf1fCorrDB);

            //TOFWithError brf2fCorrDB = (c_brf2f / c_db) - ((c_b * c_dbrf2f) / (c_db * c_db));
            //tcs.AddChannel(new string[] { "BRF2FCORRDB" }, brf2fCorrDB);

            //Some extra channels for various shot noise calculations, these are a bit weird
            tcs.AddChannel(new string[] { "SIGNL" }, c_sig);

            tcs.AddChannel(new string[] { "ONEOVERDB" }, 1 / c_db);

            TOFWithError dbSigNL = c_db / c_sig;
            tcs.AddChannel(new string[] { "DBSIG" }, dbSigNL);


            TOFWithError dbdbSigSigNL = dbSigNL * dbSigNL;
            tcs.AddChannel(new string[] { "DBDBSIGSIG" }, dbdbSigSigNL);


            TOFWithError SigdbdbNL = c_sig / (c_db * c_db);
            tcs.AddChannel(new string[] { "SIGDBDB" }, SigdbdbNL);

            return tcsWithSpecialValues;
        }

        private delegate double[] GatedDetectorExtractFunction(int index, double startTime, double endTime);
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

        private TOF[] GetTOFDetectorData(Block b, string detector)
        {
            int detectorIndex = b.detectors.IndexOf(detector);
            TOF[] tofList = new TOF[b.Points.Count];
            for (int i = 0; i < b.Points.Count; i++)
            {
                tofList[i] = (TOF)((EDMPoint)b.Points[i]).Shot.TOFs[detectorIndex];
            }
            return tofList;
        }

        public List<Modulation> GetModulations(Block b)
        {
            List<Modulation> modulations = new List<Modulation>();

            foreach (AnalogModulation mod in b.Config.AnalogModulations)
            {
                modulations.Add(mod);
            }
            foreach (DigitalModulation mod in b.Config.DigitalModulations)
            {
                modulations.Add(mod);
            }
            foreach (TimingModulation mod in b.Config.TimingModulations)
            {
                modulations.Add(mod);
            }

            return modulations;
        }

        // -This returns, for each point in the block, its state for each of the switching channels-
        // -Each switch state is represented by a binary string converted to an unsigned int-
        public List<uint> GetSwitchStates(List<Modulation> modulations)
        {
            int blockLength = modulations[0].Waveform.Length;
            List<bool[]> wfBits = new List<bool[]>();
            foreach (Modulation mod in modulations) wfBits.Add(mod.Waveform.Bits);
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
            return switchStates;
        }
        public int[,] GetStateSigns(int numStates)
        {
            int[,] stateSigns = new int[numStates, numStates];
            for (uint i = 0; i < numStates; i++)
            {
                for (uint j = 0; j < numStates; j++)
                {
                    stateSigns[i, j] = stateSign(j, i);
                }
            }
            return stateSigns;
        }
        
        // Calculate, for a given analysis channel, whether a given state contributes positively or negatively
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
    }
}