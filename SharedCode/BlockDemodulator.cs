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
            foreach (KeyValuePair<string, GatedDetectorExtractSpec> spec in config.GatedDetectorExtractSpecs)
            {
                GatedDetectorExtractSpec gate = spec.Value;
                gatedDetectorData.Add(GatedDetectorData.ExtractFromBlock(b, gate));
                db.DetectorIndices.Add(gate.Name, ind);
                ind++;
                db.DetectorCalibrations.Add(gate.Name,
                    ((TOF)((EDMPoint)b.Points[0]).Shot.TOFs[gate.Index]).Calibration);

            }
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
        // integrated according to the Demodulation config. For reasons of speed they are
        // only calculated for the "topNormed" detector.
        public DemodulatedBlock DemodulateBlockNL(Block b, DemodulationConfig config)
        {
            // we start with the standard demodulated block
            DemodulatedBlock dblock = DemodulateBlock(b, config);
            // normalise the PMT signal
            b.Normalise(config.GatedDetectorExtractSpecs["norm"]);
            // TOF demodulate the block to get the channel wiggles
            // the BlockTOFDemodulator only demodulates the PMT detector
            BlockTOFDemodulator btd = new BlockTOFDemodulator();
            TOFChannelSet tcs = btd.TOFDemodulateBlock(b, 5, false);
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

            // we bodge the errors, which aren't really used for much anyway
            // by just using the error from the normal dblock. I ignore the error in DB.
            // I use the simple correction error for the full correction. Doesn't much matter.
            int tndi = dblock.DetectorIndices["topNormed"];
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
            double brf1fDBE = dcv.GetError(new string[] { "B", "RF1F" }) / dcv.GetValue(new string[] { "DB" });
            double brf2fDBE = dcv.GetError(new string[] { "B", "RF2F" }) / dcv.GetValue(new string[] { "DB" });
            double erf1fDBE = dcv.GetError(new string[] { "E", "RF1F" }) / dcv.GetValue(new string[] { "DB" });
            double erf2fDBE = dcv.GetError(new string[] { "E", "RF2F" }) / dcv.GetValue(new string[] { "DB" });
            double erf1fDBDBE = dcv.GetError(new string[] { "E", "DB", "RF1F" }) / dcv.GetValue(new string[] { "DB" });
            double erf2fDBDBE = dcv.GetError(new string[] { "E", "DB", "RF2F" }) / dcv.GetValue(new string[] { "DB" });

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
            dblock.ChannelValues[tndi].SpecialValues["BRF1FCORRDB"] = new double[] { brf1fCorrDBG, brf1fDBE };
            dblock.ChannelValues[tndi].SpecialValues["BRF2FCORRDB"] = new double[] { brf2fCorrDBG, brf2fDBE };
            dblock.ChannelValues[tndi].SpecialValues["ERF1FDB"] = new double[] { erf1fDBG, erf1fDBE };
            dblock.ChannelValues[tndi].SpecialValues["ERF2FDB"] = new double[] { erf2fDBG, erf2fDBE };
            dblock.ChannelValues[tndi].SpecialValues["ERF1FDBDB"] = new double[] { erf1fDBDBG, erf1fDBDBE };
            dblock.ChannelValues[tndi].SpecialValues["ERF2FDBDB"] = new double[] { erf2fDBDBG, erf2fDBDBE };

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