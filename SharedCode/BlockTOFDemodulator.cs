using System;
using System.Collections.Generic;
using System.Text;

using Data;
using Data.EDM;
using EDMConfig;

namespace Analysis.EDM
{
    /// <summary>
    /// This class takes a block and demodulates the TOFs into each analysis channel.
    /// A lot of the code is cheezily copied and pasted from the BlockDemodulator.
    /// </summary>
    public class BlockTOFDemodulator
    {
        public TOFChannelSet TOFDemodulateBlock(Block b, int detectorIndex, bool allChannels)
        {
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
            int[] channelsToAnalyse;
            if (allChannels)
            {
                channelsToAnalyse = new int[numStates];
                for (int i = 0; i < numStates; i++) channelsToAnalyse[i] = i;
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
                int lf1Channel = (1 << lf1Index);
                int dblf1Channel = (1 << dbIndex) + (1 << lf1Index);
                int lf2Channel = (1 << lf2Index);
                int dblf2Channel = (1 << dbIndex) + (1 << lf2Index);

                channelsToAnalyse = new int[] { bChannel, dbChannel, ebChannel, edbChannel, dbrf1fChannel,
                    dbrf2fChannel, brf1fChannel, brf2fChannel, edbrf1fChannel, edbrf2fChannel, ebdbChannel,
                    rf1fChannel, rf2fChannel, erf1fChannel, erf2fChannel, rf1aChannel, rf2aChannel, dbrf1aChannel,
                    dbrf2aChannel, lf1Channel, dblf1Channel, lf2Channel, dblf2Channel
                };
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
                tOn /= (blockLength / 2);
                tOff /= (blockLength / 2);
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
                if (channel == 0) channelName = new string[] {"SIG"};
                tcs.AddChannel(channelName, tc);
            }
            // ** add the special channels **

            // extract the TOFChannels that we need.
            TOFChannel c_eb = (TOFChannel)tcs.GetChannel(new string[] { "E", "B" });
            TOFChannel c_edb = (TOFChannel)tcs.GetChannel(new string[] {"E", "DB"});
            TOFChannel c_dbrf1f = (TOFChannel)tcs.GetChannel(new string[] { "DB", "RF1F" });
            TOFChannel c_dbrf2f = (TOFChannel)tcs.GetChannel(new string[] { "DB", "RF2F" });
            TOFChannel c_b = (TOFChannel)tcs.GetChannel(new string[] { "B" });
            TOFChannel c_db = (TOFChannel)tcs.GetChannel(new string[] { "DB" });
            TOFChannel c_brf1f = (TOFChannel)tcs.GetChannel(new string[] { "B", "RF1F" });
            TOFChannel c_brf2f = (TOFChannel)tcs.GetChannel(new string[] { "B", "RF2F" });
            TOFChannel c_edbrf1f = (TOFChannel)tcs.GetChannel(new string[] { "E", "DB", "RF1F" });
            TOFChannel c_edbrf2f = (TOFChannel)tcs.GetChannel(new string[] { "E", "DB", "RF2F" });
            TOFChannel c_ebdb= (TOFChannel)tcs.GetChannel(new string[] { "E", "B", "DB" });

            TOFChannel c_rf1f = (TOFChannel)tcs.GetChannel(new string[] { "RF1F" });
            TOFChannel c_rf2f = (TOFChannel)tcs.GetChannel(new string[] { "RF2F" });

            TOFChannel c_erf1f = (TOFChannel)tcs.GetChannel(new string[] { "E", "RF1F" });
            TOFChannel c_erf2f = (TOFChannel)tcs.GetChannel(new string[] { "E", "RF2F" });

            TOFChannel c_rf1a = (TOFChannel)tcs.GetChannel(new string[] { "RF1A" }); 
            TOFChannel c_rf2a = (TOFChannel)tcs.GetChannel(new string[] { "RF2A" }); 
            TOFChannel c_dbrf1a = (TOFChannel)tcs.GetChannel(new string[] { "DB", "RF1A" }); 
            TOFChannel c_dbrf2a = (TOFChannel)tcs.GetChannel(new string[] { "DB", "RF2A" });

            TOFChannel c_lf1 = (TOFChannel)tcs.GetChannel(new string[] { "LF1" });
            TOFChannel c_dblf1 = (TOFChannel)tcs.GetChannel(new string[] { "DB", "LF1" });
            TOFChannel c_lf2 = (TOFChannel)tcs.GetChannel(new string[] { "LF2" });
            TOFChannel c_dblf2 = (TOFChannel)tcs.GetChannel(new string[] { "DB", "LF2" });


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
            TOFChannel edmDB = c_eb / c_db;
            tcs.AddChannel(new string[] { "EDMDB" }, edmDB);

            // The corrected edm channel. This should be proportional to the edm phase.
            TOFChannel edmCorrDB = (squaredTerms + linearTerms) / preDenominator;
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

            // the LF1 channel, normalized to DB
            TOFChannel lf2DB = c_lf2 / c_db;
            tcs.AddChannel(new string[] { "LF2DB" }, lf2DB);

            TOFChannel lf2DBDB = c_dblf1 / c_db;
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

            return tcs;
        }
    }
}
