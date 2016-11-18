using System;
using System.Collections.Generic;
using System.Text;
//using NationalInstruments;
//using NationalInstruments.Analysis.Math;
//using NationalInstruments.Analysis.Dsp;

using Data;
using Data.EDM;
using EDMConfig;

namespace Analysis.EDM
{
    public class BlockDemodulator
    {
        // you'll be in trouble if the number of points per block is not divisible by this number!
        private const int kFourierAverage = 16;
        private const int kNumReplicates = 50;
        private Random r = new Random();

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
            int ind = 0;
            foreach (string d in b.detectors)
            {
                GatedDetectorExtractSpec gdes;
                config.GatedDetectorExtractSpecs.TryGetValue(d, out gdes);
                
                if (gdes != null)
                {
                    gatedDetectorData.Add(GatedDetectorData.ExtractFromBlock(b, gdes));
                    db.DetectorIndices.Add(gdes.Name, ind);
                    ind++;
                    db.DetectorCalibrations.Add(gdes.Name,
                        ((TOF)((EDMPoint)b.Points[0]).Shot.TOFs[gdes.Index]).Calibration);
                }     
            }

            //foreach (KeyValuePair<string, GatedDetectorExtractSpec> spec in config.GatedDetectorExtractSpecs)
            //{
            //    GatedDetectorExtractSpec gate = spec.Value;
            //    gatedDetectorData.Add(GatedDetectorData.ExtractFromBlock(b, gate));
            //    db.DetectorIndices.Add(gate.Name, ind);
            //    ind++;
            //    db.DetectorCalibrations.Add(gate.Name,
            //        ((TOF)((EDMPoint)b.Points[0]).Shot.TOFs[gate.Index]).Calibration);

            //}
            // ** normalise the top detector **
            gatedDetectorData.Add(
                gatedDetectorData[db.DetectorIndices["top"]] / gatedDetectorData[db.DetectorIndices["norm"]]);
            db.DetectorIndices.Add("topNormed", db.DetectorIndices.Count);

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

            // calculate the norm FFT
            db.NormFourier = DetectorFT.MakeFT(gatedDetectorData[db.DetectorIndices["norm"]], kFourierAverage);

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
        // integrated according to the Demodulation config. This is calculated for top and 
        // topNormed detectors only for speed. 
        // 
        public DemodulatedBlock DemodulateBlockNL(Block b, DemodulationConfig config)
        {
            // we start with the standard demodulated block
            DemodulatedBlock dblock = DemodulateBlock(b, config);
            // First do everything for the un-normalised top detector
            int tdi = dblock.DetectorIndices["top"];
            // TOF demodulate the block to get the channel wiggles
            // the BlockTOFDemodulator only demodulates the PMT detector
            BlockTOFDemodulator btdt = new BlockTOFDemodulator();
            TOFChannelSet tcst = btdt.TOFDemodulateBlock(b, tdi, false);
            
            // now repeat having normed the block
            // normalise the PMT signal
            b.Normalise(config.GatedDetectorExtractSpecs["norm"]);
            int tndi = dblock.DetectorIndices["topNormed"];
            // TOF demodulate the block to get the channel wiggles
            // the BlockTOFDemodulator only demodulates the PMT detector
            BlockTOFDemodulator btd = new BlockTOFDemodulator();
            TOFChannelSet tcs = btd.TOFDemodulateBlock(b, tndi, false);
            
            // get hold of the gating data
            GatedDetectorExtractSpec gate = config.GatedDetectorExtractSpecs["top"];

            // gate the special channels
            TOFChannel edmDB = (TOFChannel)tcs.GetChannel("EDMDB" );
            double edmDBG = edmDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel corrDB = (TOFChannel)tcs.GetChannel( "CORRDB" );
            double corrDBG = corrDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel edmCorrDB = (TOFChannel)tcs.GetChannel( "EDMCORRDB" );
            double edmCorrDBG = edmCorrDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel corrDB_old = (TOFChannel)tcs.GetChannel( "CORRDB_OLD" );
            double corrDBG_old = corrDB_old.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel edmCorrDB_old = (TOFChannel)tcs.GetChannel( "EDMCORRDB_OLD" );
            double edmCorrDBG_old = edmCorrDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel rf1fDB = (TOFChannel)tcs.GetChannel( "RF1FDB" );
            double rf1fDBG = rf1fDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel rf2fDB = (TOFChannel)tcs.GetChannel( "RF2FDB" );
            double rf2fDBG = rf2fDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel rf1fDBDB = (TOFChannel)tcs.GetChannel( "RF1FDBDB" );
            double rf1fDBDBG = rf1fDBDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel rf2fDBDB = (TOFChannel)tcs.GetChannel( "RF2FDBDB" );
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
            TOFChannel erf1fDB = (TOFChannel)tcs.GetChannel( "ERF1FDB" );
            double erf1fDBG = erf1fDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel erf2fDB = (TOFChannel)tcs.GetChannel( "ERF2FDB" );
            double erf2fDBG = erf2fDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel erf1fDBDB = (TOFChannel)tcs.GetChannel( "ERF1FDBDB" );
            double erf1fDBDBG = erf1fDBDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel erf2fDBDB = (TOFChannel)tcs.GetChannel("ERF2FDBDB" );
            double erf2fDBDBG = erf2fDBDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel brf1fCorrDB = (TOFChannel)tcs.GetChannel( "BRF1FCORRDB" );
            double brf1fCorrDBG = brf1fCorrDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel brf2fCorrDB = (TOFChannel)tcs.GetChannel( "BRF2FCORRDB" );
            double brf2fCorrDBG = brf2fCorrDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel DBSig = (TOFChannel)tcs.GetChannel("DBSIG");
            double DBSigG = DBSig.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel DBDBSigSig = (TOFChannel)tcs.GetChannel("DBDBSIGSIG");
            double DBDBSigSigG = DBDBSigSig.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel SIGDBDB = (TOFChannel)tcs.GetChannel("SIGDBDB");
            double SIGDBDBG = SIGDBDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel SIGNL = (TOFChannel)tcs.GetChannel("SIGNL");
            double SIGNLG = SIGNL.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel ONEOVERDB = (TOFChannel)tcs.GetChannel("ONEOVERDB");
            double ONEOVERDBG = ONEOVERDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);

            //Repeat for top

            
            TOFChannel edmDBtop = (TOFChannel)tcst.GetChannel("EDMDB");
            double edmDBGtop = edmDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel corrDBtop = (TOFChannel)tcst.GetChannel("CORRDB");
            double corrDBGtop = corrDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel edmCorrDBtop = (TOFChannel)tcst.GetChannel("EDMCORRDB");
            double edmCorrDBGtop = edmCorrDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel corrDB_oldtop = (TOFChannel)tcst.GetChannel("CORRDB_OLD");
            double corrDBG_oldtop = corrDB_old.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel edmCorrDB_oldtop = (TOFChannel)tcst.GetChannel("EDMCORRDB_OLD");
            double edmCorrDBG_oldtop = edmCorrDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel rf1fDBtop = (TOFChannel)tcst.GetChannel("RF1FDB");
            double rf1fDBGtop = rf1fDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel rf2fDBtop = (TOFChannel)tcst.GetChannel("RF2FDB");
            double rf2fDBGtop = rf2fDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel rf1fDBDBtop = (TOFChannel)tcst.GetChannel("RF1FDBDB");
            double rf1fDBDBGtop = rf1fDBDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel rf2fDBDBtop = (TOFChannel)tcst.GetChannel("RF2FDBDB");
            double rf2fDBDBGtop = rf2fDBDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel rf1aDBtop = (TOFChannel)tcst.GetChannel("RF1ADB");
            double rf1aDBGtop = rf1aDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel rf2aDBtop = (TOFChannel)tcst.GetChannel("RF2ADB");
            double rf2aDBGtop = rf2aDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel rf1aDBDBtop = (TOFChannel)tcst.GetChannel("RF1ADBDB");
            double rf1aDBDBGtop = rf1aDBDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel rf2aDBDBtop = (TOFChannel)tcst.GetChannel("RF2ADBDB");
            double rf2aDBDBGtop = rf2aDBDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel lf1DBtop = (TOFChannel)tcst.GetChannel("LF1DB");
            double lf1DBGtop = lf1DB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel lf1DBDBtop = (TOFChannel)tcst.GetChannel("LF1DBDB");
            double lf1DBDBGtop = lf1DBDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel lf2DBtop = (TOFChannel)tcst.GetChannel("LF2DB");
            double lf2DBGtop = lf2DB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel lf2DBDBtop = (TOFChannel)tcst.GetChannel("LF2DBDB");
            double lf2DBDBGtop = lf2DBDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel BDBtop = (TOFChannel)tcst.GetChannel("BDB");
            double BDBGtop = BDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel erf1fDBtop = (TOFChannel)tcst.GetChannel("ERF1FDB");
            double erf1fDBGtop = erf1fDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel erf2fDBtop = (TOFChannel)tcst.GetChannel("ERF2FDB");
            double erf2fDBGtop = erf2fDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel erf1fDBDBtop = (TOFChannel)tcst.GetChannel("ERF1FDBDB");
            double erf1fDBDBGtop = erf1fDBDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel erf2fDBDBtop = (TOFChannel)tcst.GetChannel("ERF2FDBDB");
            double erf2fDBDBGtop = erf2fDBDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel brf1fCorrDBtop = (TOFChannel)tcst.GetChannel("BRF1FCORRDB");
            double brf1fCorrDBGtop = brf1fCorrDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);
            TOFChannel brf2fCorrDBtop = (TOFChannel)tcst.GetChannel("BRF2FCORRDB");
            double brf2fCorrDBGtop = brf2fCorrDB.Difference.GatedMean(gate.GateLow, gate.GateHigh);




            // we bodge the errors, which aren't really used for much anyway
            // by just using the error from the normal dblock. I ignore the error in DB.
            // I use the simple correction error for the full correction. Doesn't much matter.
            DetectorChannelValues dcv = dblock.ChannelValues[tndi];
            double edmDBE = dcv.GetError(new string[] { "E", "B" }) / dcv.GetValue(new string[] { "DB" });
            double corrDBE = Math.Sqrt(
                Math.Pow(dcv.GetValue(new string[] { "E", "DB" }) * dcv.GetError(new string[] { "B" }), 2) +
                Math.Pow(dcv.GetValue(new string[] { "B" }) * dcv.GetError(new string[] { "E", "DB" }), 2) )
                / Math.Pow(dcv.GetValue(new string[] { "DB" }), 2);
            double edmCorrDBE = Math.Sqrt( Math.Pow(edmDBE, 2) + Math.Pow(corrDBE, 2));

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
            double DBSigE = dcv.GetError(new string[] { "DB" }) / dcv.GetValue(new string[] { "SIG" });
            double DBDBSigSigE = dcv.GetError(new string[] { "DB" }) / dcv.GetValue(new string[] { "SIG" });

            //repeat for top
            DetectorChannelValues dcvt = dblock.ChannelValues[tdi];
            double lf2DBEtop = dcvt.GetError(new string[] { "LF2" }) / dcvt.GetValue(new string[] { "DB" }); //Change the db channel back to topNormed
            double lf2DBDBEtop = dcvt.GetError(new string[] { "DB", "LF2" }) / dcvt.GetValue(new string[] { "DB" }); //Change the db channel back to topNormed

            double edmDBEtop = dcvt.GetError(new string[] { "E", "B" }) / dcvt.GetValue(new string[] { "DB" });
            double corrDBEtop = Math.Sqrt(
                Math.Pow(dcvt.GetValue(new string[] { "E", "DB" }) * dcvt.GetError(new string[] { "B" }), 2) +
                Math.Pow(dcvt.GetValue(new string[] { "B" }) * dcvt.GetError(new string[] { "E", "DB" }), 2))
                / Math.Pow(dcvt.GetValue(new string[] { "DB" }), 2);
            double edmCorrDBEtop = Math.Sqrt(Math.Pow(edmDBEtop, 2) + Math.Pow(corrDBEtop, 2));

            double rf1fDBEtop = dcvt.GetError(new string[] { "RF1F" }) / dcvt.GetValue(new string[] { "DB" });
            double rf2fDBEtop = dcvt.GetError(new string[] { "RF2F" }) / dcvt.GetValue(new string[] { "DB" });
            double rf1fDBDBEtop = dcvt.GetError(new string[] { "DB", "RF1F" }) / dcvt.GetValue(new string[] { "DB" });
            double rf2fDBDBEtop = dcvt.GetError(new string[] { "DB", "RF2F" }) / dcvt.GetValue(new string[] { "DB" });

            double rf1aDBEtop = dcvt.GetError(new string[] { "RF1A" }) / dcvt.GetValue(new string[] { "DB" });
            double rf2aDBEtop = dcvt.GetError(new string[] { "RF2A" }) / dcvt.GetValue(new string[] { "DB" });
            double rf1aDBDBEtop = dcvt.GetError(new string[] { "DB", "RF1A" }) / dcvt.GetValue(new string[] { "DB" });
            double rf2aDBDBEtop = dcvt.GetError(new string[] { "DB", "RF2A" }) / dcvt.GetValue(new string[] { "DB" });

            double lf1DBEtop = dcvt.GetError(new string[] { "LF1" }) / dcvt.GetValue(new string[] { "DB" });
            double lf1DBDBEtop = dcvt.GetError(new string[] { "DB", "LF1" }) / dcvt.GetValue(new string[] { "DB" });

            double brf1fDBEtop = dcvt.GetError(new string[] { "B", "RF1F" }) / dcvt.GetValue(new string[] { "DB" });
            double brf2fDBEtop = dcvt.GetError(new string[] { "B", "RF2F" }) / dcvt.GetValue(new string[] { "DB" });
            double erf1fDBEtop = dcvt.GetError(new string[] { "E", "RF1F" }) / dcvt.GetValue(new string[] { "DB" });
            double erf2fDBEtop = dcvt.GetError(new string[] { "E", "RF2F" }) / dcvt.GetValue(new string[] { "DB" });
            double erf1fDBDBEtop = dcvt.GetError(new string[] { "E", "DB", "RF1F" }) / dcvt.GetValue(new string[] { "DB" });
            double erf2fDBDBEtop = dcvt.GetError(new string[] { "E", "DB", "RF2F" }) / dcvt.GetValue(new string[] { "DB" });
            double BDBEtop = dcvt.GetError(new string[] { "B" }) / dcvt.GetValue(new string[] { "DB" });



            // stuff the data into the dblock
            dblock.ChannelValues[tndi].SpecialValues["EDMDB"] = new double[] { edmDBG, edmDBE };
            dblock.ChannelValues[tndi].SpecialValues["CORRDB"] = new double[] { corrDBG, corrDBE };
            dblock.ChannelValues[tndi].SpecialValues["EDMCORRDB"] = new double[] { edmCorrDBG, edmCorrDBE };
            dblock.ChannelValues[tndi].SpecialValues["CORRDB_OLD"] = new double[] { corrDBG_old, corrDBE };
            dblock.ChannelValues[tndi].SpecialValues["EDMCORRDB_OLD"] = new double[] { edmCorrDBG_old, edmCorrDBE };
            dblock.ChannelValues[tndi].SpecialValues["RF1FDB"] = new double[] { rf1fDBG, rf1fDBE };
            dblock.ChannelValues[tndi].SpecialValues["RF2FDB"] = new double[] { rf2fDBG, rf2fDBE };
            dblock.ChannelValues[tndi].SpecialValues["RF1FDBDB"] = new double[] { rf1fDBDBG, rf1fDBDBE };
            dblock.ChannelValues[tndi].SpecialValues["RF2FDBDB"] = new double[] { rf2fDBDBG, rf2fDBDBE };
            dblock.ChannelValues[tndi].SpecialValues["RF1ADB"] = new double[] { rf1aDBG, rf1aDBE };
            dblock.ChannelValues[tndi].SpecialValues["RF2ADB"] = new double[] { rf2aDBG, rf2aDBE };
            dblock.ChannelValues[tndi].SpecialValues["RF1ADBDB"] = new double[] { rf1aDBDBG, rf1aDBDBE };
            dblock.ChannelValues[tndi].SpecialValues["RF2ADBDB"] = new double[] { rf2aDBDBG, rf2aDBDBE };
            dblock.ChannelValues[tndi].SpecialValues["BRF1FCORRDB"] = new double[] { brf1fCorrDBG, brf1fDBE };
            dblock.ChannelValues[tndi].SpecialValues["BRF2FCORRDB"] = new double[] { brf2fCorrDBG, brf2fDBE };
            dblock.ChannelValues[tndi].SpecialValues["ERF1FDB"] = new double[] { erf1fDBG, erf1fDBE };
            dblock.ChannelValues[tndi].SpecialValues["ERF2FDB"] = new double[] { erf2fDBG, erf2fDBE };
            dblock.ChannelValues[tndi].SpecialValues["ERF1FDBDB"] = new double[] { erf1fDBDBG, erf1fDBDBE };
            dblock.ChannelValues[tndi].SpecialValues["ERF2FDBDB"] = new double[] { erf2fDBDBG, erf2fDBDBE };
            dblock.ChannelValues[tndi].SpecialValues["LF1DB"] = new double[] { lf1DBG, lf1DBE };
            dblock.ChannelValues[tndi].SpecialValues["LF1DBDB"] = new double[] { lf1DBDBG, lf1DBDBE };
            dblock.ChannelValues[tndi].SpecialValues["LF2DB"] = new double[] { lf2DBG, lf2DBE };
            dblock.ChannelValues[tndi].SpecialValues["LF2DBDB"] = new double[] { lf2DBDBG, lf2DBDBE };
            dblock.ChannelValues[tndi].SpecialValues["BDB"] = new double[] { BDBG, BDBE };
            dblock.ChannelValues[tndi].SpecialValues["DBSIG"] = new double[] { DBSigG, DBSigE };
            dblock.ChannelValues[tndi].SpecialValues["DBDBSIGSIG"] = new double[] { DBDBSigSigG, DBDBSigSigE };
            dblock.ChannelValues[tndi].SpecialValues["SIGDBDB"] = new double[] { SIGDBDBG, SIGDBDBG }; //This Error isn't at all right, also note that this channel isn't dimensionless
            dblock.ChannelValues[tndi].SpecialValues["SIGNL"] = new double[] { SIGNLG, SIGNLG }; //This Error isn't at all right, also note that this channel isn't dimensionless
            dblock.ChannelValues[tndi].SpecialValues["ONEOVERDB"] = new double[] { ONEOVERDBG, ONEOVERDBG }; //This Error isn't at all right, also note that this channel isn't dimensionless

            dblock.ChannelValues[tdi].SpecialValues["EDMDB"] = new double[] { edmDBGtop, edmDBEtop };
            dblock.ChannelValues[tdi].SpecialValues["CORRDB"] = new double[] { corrDBGtop, corrDBEtop };
            dblock.ChannelValues[tdi].SpecialValues["EDMCORRDB"] = new double[] { edmCorrDBGtop, edmCorrDBEtop };
            dblock.ChannelValues[tdi].SpecialValues["CORRDB_OLD"] = new double[] { corrDBG_oldtop, corrDBEtop };
            dblock.ChannelValues[tdi].SpecialValues["EDMCORRDB_OLD"] = new double[] { edmCorrDBG_oldtop, edmCorrDBEtop };
            dblock.ChannelValues[tdi].SpecialValues["RF1FDB"] = new double[] { rf1fDBGtop, rf1fDBEtop };
            dblock.ChannelValues[tdi].SpecialValues["RF2FDB"] = new double[] { rf2fDBGtop, rf2fDBEtop };
            dblock.ChannelValues[tdi].SpecialValues["RF1FDBDB"] = new double[] { rf1fDBDBGtop, rf1fDBDBEtop };
            dblock.ChannelValues[tdi].SpecialValues["RF2FDBDB"] = new double[] { rf2fDBDBGtop, rf2fDBDBEtop };
            dblock.ChannelValues[tdi].SpecialValues["RF1ADB"] = new double[] { rf1aDBGtop, rf1aDBEtop };
            dblock.ChannelValues[tdi].SpecialValues["RF2ADB"] = new double[] { rf2aDBGtop, rf2aDBEtop };
            dblock.ChannelValues[tdi].SpecialValues["RF1ADBDB"] = new double[] { rf1aDBDBGtop, rf1aDBDBEtop };
            dblock.ChannelValues[tdi].SpecialValues["RF2ADBDB"] = new double[] { rf2aDBDBGtop, rf2aDBDBEtop };
            dblock.ChannelValues[tdi].SpecialValues["BRF1FCORRDB"] = new double[] { brf1fCorrDBGtop, brf1fDBEtop };
            dblock.ChannelValues[tdi].SpecialValues["BRF2FCORRDB"] = new double[] { brf2fCorrDBGtop, brf2fDBEtop };
            dblock.ChannelValues[tdi].SpecialValues["ERF1FDB"] = new double[] { erf1fDBGtop, erf1fDBEtop };
            dblock.ChannelValues[tdi].SpecialValues["ERF2FDB"] = new double[] { erf2fDBGtop, erf2fDBEtop };
            dblock.ChannelValues[tdi].SpecialValues["ERF1FDBDB"] = new double[] { erf1fDBDBGtop, erf1fDBDBEtop };
            dblock.ChannelValues[tdi].SpecialValues["ERF2FDBDB"] = new double[] { erf2fDBDBGtop, erf2fDBDBEtop };
            dblock.ChannelValues[tdi].SpecialValues["LF1DB"] = new double[] { lf1DBGtop, lf1DBEtop };
            dblock.ChannelValues[tdi].SpecialValues["LF1DBDB"] = new double[] { lf1DBDBGtop, lf1DBDBEtop };
            dblock.ChannelValues[tdi].SpecialValues["LF2DB"] = new double[] { lf2DBGtop, lf2DBEtop };
            dblock.ChannelValues[tdi].SpecialValues["LF2DBDB"] = new double[] { lf2DBDBGtop, lf2DBDBEtop };
            dblock.ChannelValues[tdi].SpecialValues["BDB"] = new double[] { BDBGtop, BDBEtop };


            return dblock;
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

        public double bootstrapMean(double[] values)
        {
            // make the replicate
            double[] replicate = new double[values.Length];
            for (int i = 0; i < values.Length; i++) replicate[i] = values[r.Next(values.Length)];
            // find its mean
            double total = 0.0;
            foreach (double d in replicate) total += d;
            total /= replicate.Length;
            return total;
        }
    }
}