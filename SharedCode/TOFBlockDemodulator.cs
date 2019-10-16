using System;
using System.Collections.Generic;
using System.Text;

using Data;
using Data.EDM;
using EDMConfig;

namespace Analysis.EDM
{
    public class TOFBlockDemodulator : BlockDemodulator
    {
        public TOFDemodulatedBlock TOFDemodulateBlock(Block b, string[] detectorsToDemodulate, int[] channelsToAnalyse)
        {
            if (!b.detectors.Contains("asymmetry")) b.AddDetectorsToBlock();

            int blockLength = b.Points.Count;

            // ----Copy across metadata----
            TOFDemodulatedBlock tofDemodulatedBlock = new TOFDemodulatedBlock(b.TimeStamp, b.Config, b.GetPointDetectors());

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
            for (int d = 0; d < detectorsToDemodulate.Length; d++)
            {
                // Find detector index in the block
                int detectorIndex = b.detectors.IndexOf(detectorsToDemodulate[d]);

                // We obtain one TOF Channel Set for each detector
                ChannelSet<TOF> tofChannelSet = new ChannelSet<TOF>();

                // Detector calibration
                double calibration = ((TOF)((EDMPoint)b.Points[0]).Shot.TOFs[detectorIndex]).Calibration;
                
                // Divide TOFs into bins depending on switch state
                List<List<TOF>> stateTOFs = new List<List<TOF>>(numStates);
                for (int i = 0; i < numStates; i++) stateTOFs.Add(new List<TOF>(blockLength / numStates));
                for (int i = 0; i < blockLength; i++) stateTOFs[(int)switchStates[i]].Add((TOF)((EDMPoint)(b.Points[i])).Shot.TOFs[detectorIndex]);
                int subLength = blockLength / numStates;

                // For each analysis channel, calculate the mean TOFChannel (with error)
                // by means of the TOFChannelAccumulator, then add it to the TOFChannelSet
                foreach (int channel in channelsToAnalyse)
                {
                    TOFChannelAccumulator tofChannelAccumulator = new TOFChannelAccumulator();
                    for (int subIndex = 0; subIndex < subLength; subIndex++)
                    {
                        TOFChannel tc = new TOFChannel();
                        TOF tOn = new TOF();
                        TOF tOff = new TOF();
                        for (int i = 0; i < numStates; i++)
                        {
                            if (stateSigns[channel, i] == 1) tOn += stateTOFs[i][subIndex];
                            else tOff += stateTOFs[i][subIndex];
                        }
                        tOn /= blockLength;
                        tOff /= blockLength;
                        tc.On = tOn;
                        tc.Off = tOff;
                        // This "if" is to take care of the case of the "SIG" channel, for which there is no off TOF.
                        if (tc.Off.Length != 0) tc.Difference = tc.On - tc.Off;
                        else tc.Difference = tc.On;
                        tofChannelAccumulator.Add(tc);
                    }

                    // add the Channel to the ChannelSet
                    List<string> usedSwitches = new List<string>();
                    for (int i = 0; i < modulations.Count; i++)
                        if ((channel & (1 << i)) != 0) usedSwitches.Add(modulations[i].Name);
                    string[] channelName = usedSwitches.ToArray();
                    // the SIG channel has a special name
                    if (channel == 0) channelName = new string[] { "SIG" };
                    tofChannelSet.AddChannel(channelName, tofChannelAccumulator.GetResult());
                }

                // Append the ChannelSet with special combinations of channels
                ChannelSet<TOF> tofChannelSetWithSpecialValues = AppendChannelSetWithSpecialValues(tofChannelSet);

                // Add the ChannelSet to the demodulated block
                tofDemodulatedBlock.AddDetector(detectorsToDemodulate[d], calibration, tofChannelSetWithSpecialValues);
            }

            return tofDemodulatedBlock;
        }

        // This demodulates all the detectors, in all the channels.
        public TOFDemodulatedBlock TOFDemodulateBlock(Block b)
        {
            if (!b.detectors.Contains("asymmetry")) b.AddDetectorsToBlock();
            string[] allDetectors = b.detectors.ToArray();

            List<Modulation> modulations = GetModulations(b);
            int numStates = (int)Math.Pow(2, modulations.Count);
            int[] channelsToAnalyse = new int[numStates];
            for (int i = 0; i < numStates; i++) channelsToAnalyse[i] = i;

            return TOFDemodulateBlock(b, allDetectors, channelsToAnalyse);
        }

        // This demodulates the block into the special channels for any detector(s).
        // This is so that we can quickly obtain gated special channels without going through
        // the entire TOF demodulation.
        public TOFDemodulatedBlock TOFDemodulateBlockForSpecialChannels(Block b, string[] detectors)
        {
            List<Modulation> modulations = GetModulations(b);
            List<string> modNames = new List<string>();
            foreach (Modulation modulation in modulations) modNames.Add(modulation.Name);

            int bIndex = modNames.IndexOf("B");
            int dbIndex = modNames.IndexOf("DB");
            int eIndex = modNames.IndexOf("E");
            int rf1fIndex = modNames.IndexOf("RF1F");
            int rf2fIndex = modNames.IndexOf("RF2F");
            int rf1aIndex = modNames.IndexOf("RF1A");
            int rf2aIndex = modNames.IndexOf("RF2A");
            int lf1Index = modNames.IndexOf("LF1");

            int sigChannel = 0;
            int eChannel = (1 << eIndex);
            int bChannel = (1 << bIndex);
            int dbChannel = (1 << dbIndex);
            int ebChannel = (1 << eIndex) + (1 << bIndex);
            int edbChannel = (1 << eIndex) + (1 << dbIndex);
            int bdbChannel = (1 << bIndex) + (1 << dbIndex);
            int dbrf1fChannel = (1 << dbIndex) + (1 << rf1fIndex);
            int dbrf2fChannel = (1 << dbIndex) + (1 << rf2fIndex);
            int brf1fChannel = (1 << bIndex) + (1 << rf1fIndex);
            int brf2fChannel = (1 << bIndex) + (1 << rf2fIndex);
            int edbrf1fChannel = (1 << eIndex) + (1 << dbIndex) + (1 << rf1fIndex);
            int edbrf2fChannel = (1 << eIndex) + (1 << dbIndex) + (1 << rf2fIndex);
            int bdbrf1fChannel = (1 << bIndex) + (1 << dbIndex) + (1 << rf1fIndex);
            int bdbrf2fChannel = (1 << bIndex) + (1 << dbIndex) + (1 << rf2fIndex);
            int ebdbChannel = (1 << eIndex) + (1 << bIndex) + (1 << dbIndex);
            int ebdbrf1fChannel = (1 << eIndex) + (1 << bIndex) + (1 << dbIndex) + (1 << rf1fIndex);
            int ebdbrf2fChannel = (1 << eIndex) + (1 << bIndex) + (1 << dbIndex) + (1 << rf2fIndex);
            int rf1fChannel = (1 << rf1fIndex);
            int rf2fChannel = (1 << rf2fIndex);
            int erf1fChannel = (1 << eIndex) + (1 << rf1fIndex);
            int erf2fChannel = (1 << eIndex) + (1 << rf2fIndex);
            int rf1aChannel = (1 << rf1aIndex);
            int rf2aChannel = (1 << rf2aIndex);
            int dbrf1aChannel = (1 << dbIndex) + (1 << rf1aIndex);
            int dbrf2aChannel = (1 << dbIndex) + (1 << rf2aIndex);

            List<int> channelsToAnalyse = new List<int> { sigChannel, eChannel, bChannel, dbChannel, ebChannel, edbChannel, bdbChannel, dbrf1fChannel,
                dbrf2fChannel, brf1fChannel, brf2fChannel, edbrf1fChannel, edbrf2fChannel, bdbrf1fChannel, bdbrf2fChannel, ebdbChannel, ebdbrf1fChannel, ebdbrf2fChannel,
                rf1fChannel, rf2fChannel, erf1fChannel, erf2fChannel, rf1aChannel, rf2aChannel, dbrf1aChannel,
                dbrf2aChannel
            };

            if (lf1Index != -1) // Index = -1 if "LF1" not found
            {
                int lf1Channel = (1 << lf1Index);
                channelsToAnalyse.Add(lf1Channel);
                int dblf1Channel = (1 << dbIndex) + (1 << lf1Index);
                channelsToAnalyse.Add(dblf1Channel);
            }

            return TOFDemodulateBlock(b, detectors, channelsToAnalyse.ToArray());
        }

        private ChannelSet<TOF> AppendChannelSetWithSpecialValues(ChannelSet<TOF> tcs)
        {
            ChannelSet<TOF> tcsWithSpecialValues = tcs;

            // Extract the TOFChannels that we need.
            TOFChannel c_eb = (TOFChannel)tcs.GetChannel(new string[] { "E", "B" });
            TOFChannel c_edb = (TOFChannel)tcs.GetChannel(new string[] { "E", "DB" });
            TOFChannel c_bdb = (TOFChannel)tcs.GetChannel(new string[] { "B", "DB" });
            TOFChannel c_dbrf1f = (TOFChannel)tcs.GetChannel(new string[] { "DB", "RF1F" });
            TOFChannel c_dbrf2f = (TOFChannel)tcs.GetChannel(new string[] { "DB", "RF2F" });
            TOFChannel c_e = (TOFChannel)tcs.GetChannel(new string[] { "E" });
            TOFChannel c_b = (TOFChannel)tcs.GetChannel(new string[] { "B" });
            TOFChannel c_db = (TOFChannel)tcs.GetChannel(new string[] { "DB" });
            TOFChannel c_sig = (TOFChannel)tcs.GetChannel(new string[] { "SIG" });

            TOFChannel c_brf1f = (TOFChannel)tcs.GetChannel(new string[] { "B", "RF1F" });
            TOFChannel c_brf2f = (TOFChannel)tcs.GetChannel(new string[] { "B", "RF2F" });
            TOFChannel c_edbrf1f = (TOFChannel)tcs.GetChannel(new string[] { "E", "DB", "RF1F" });
            TOFChannel c_edbrf2f = (TOFChannel)tcs.GetChannel(new string[] { "E", "DB", "RF2F" });
            TOFChannel c_bdbrf1f = (TOFChannel)tcs.GetChannel(new string[] { "B", "DB", "RF1F" });
            TOFChannel c_bdbrf2f = (TOFChannel)tcs.GetChannel(new string[] { "B", "DB", "RF2F" });
            TOFChannel c_ebdb = (TOFChannel)tcs.GetChannel(new string[] { "E", "B", "DB" });

            TOFChannel c_ebdbrf1f = (TOFChannel)tcs.GetChannel(new string[] { "E", "B", "DB", "RF1F" });
            TOFChannel c_ebdbrf2f = (TOFChannel)tcs.GetChannel(new string[] { "E", "B", "DB", "RF2F" });

            TOFChannel c_rf1f = (TOFChannel)tcs.GetChannel(new string[] { "RF1F" });
            TOFChannel c_rf2f = (TOFChannel)tcs.GetChannel(new string[] { "RF2F" });

            TOFChannel c_erf1f = (TOFChannel)tcs.GetChannel(new string[] { "E", "RF1F" });
            TOFChannel c_erf2f = (TOFChannel)tcs.GetChannel(new string[] { "E", "RF2F" });

            TOFChannel c_rf1a = (TOFChannel)tcs.GetChannel(new string[] { "RF1A" });
            TOFChannel c_rf2a = (TOFChannel)tcs.GetChannel(new string[] { "RF2A" });
            TOFChannel c_dbrf1a = (TOFChannel)tcs.GetChannel(new string[] { "DB", "RF1A" });
            TOFChannel c_dbrf2a = (TOFChannel)tcs.GetChannel(new string[] { "DB", "RF2A" });

            // For SOME blocks there is no LF1 channel (and hence switch states).
            // To get around this problem I will populate the TOFChannel with "SIG"
            // It will then be obvious in the analysis when LF1 takes on real values.
            TOFChannel c_lf1;
            TOFChannel c_dblf1;
            if (!tcs.Channels.Contains("LF1"))
            {
                c_lf1 = c_sig;
                c_dblf1 = c_sig;
            }
            else
            {
                c_lf1 = (TOFChannel)tcs.GetChannel(new string[] { "LF1" });
                c_dblf1 = (TOFChannel)tcs.GetChannel(new string[] { "DB", "LF1" });
            }

            // Work out the terms for the full, corrected edm. 
            TOFChannel terms = c_eb * c_db 
                - c_edb * c_b + c_bdb * c_e - c_ebdb * c_sig 
                + c_erf1f * c_bdbrf1f + c_erf2f * c_bdbrf2f
                - c_brf1f * c_edbrf1f - c_brf2f * c_edbrf2f 
                - c_ebdbrf1f * c_rf1f - c_ebdbrf2f * c_rf2f
                ;

            TOFChannel preDenominator = c_db * c_db 
                - c_edb * c_edb + c_bdb * c_bdb - c_ebdb * c_ebdb
                + c_bdbrf1f * c_bdbrf1f + c_bdbrf2f * c_bdbrf2f 
                - c_edbrf1f * c_edbrf1f - c_edbrf2f * c_edbrf2f
                + c_ebdbrf1f * c_ebdbrf1f + c_ebdbrf2f * c_ebdbrf2f
                ;

            // it's important when working out the non-linear channel
            // combinations to always keep them dimensionless. If you
            // don't you'll run into trouble with integral vs. average
            // signal.
            TOFChannel edmDB = c_eb / c_db;
            tcs.AddChannel(new string[] { "EDMDB" }, edmDB);

            // The corrected edm channel. This should be proportional to the edm phase.
            TOFChannel edmCorrDB = terms / preDenominator;
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

            TOFChannel bDB = c_b / c_db;
            tcs.AddChannel(new string[] { "BDB" }, bDB);

            // we also need to extract the rf-step induced phase shifts. These come out in the
            // B.RFxF channels, but like the edm, need to be corrected. 

            //// Work out the terms
            //TOFChannel brf1fCorrectionTerms = c_eb * c_db
            //    - c_edb * c_b + c_bdb * c_e - c_ebdb * c_sig
            //    + c_erf1f * c_bdbrf1f + c_erf2f * c_bdbrf2f
            //    - c_brf1f * c_edbrf1f - c_brf2f * c_edbrf2f
            //    - c_ebdbrf1f * c_rf1f - c_ebdbrf2f * c_rf2f
            //    ;

            //TOFChannel brf1fPreDenominator = c_db * c_db
            //    - c_edb * c_edb + c_bdb * c_bdb - c_ebdb * c_ebdb
            //    + c_bdbrf1f * c_bdbrf1f + c_bdbrf2f * c_bdbrf2f
            //    - c_edbrf1f * c_edbrf1f - c_edbrf2f * c_edbrf2f
            //    + c_ebdbrf1f * c_ebdbrf1f + c_ebdbrf2f * c_ebdbrf2f
            //    ;

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

            return tcsWithSpecialValues;
        }
    }
}