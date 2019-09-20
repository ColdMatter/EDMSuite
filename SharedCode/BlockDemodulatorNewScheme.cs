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
    public class BlockDemodulatorNewScheme
    {
        private const int kNumReplicates = 50;
        private Random r = new Random();

        // This function gates the detector data first, and then demodulates the channels.
        // This means that it can give innacurate results for non-linear combinations
        // of channels that vary appreciably over the TOF. There's another, slower, function
        // DemodulateBlockNL that takes care of this.
        public DemodulatedBlockNewScheme DemodulateBlock(Block b, DemodulationConfigNewScheme config)
        {
            // *** copy across the metadata ***
            DemodulatedBlockNewScheme db = new DemodulatedBlockNewScheme();
            db.TimeStamp = b.TimeStamp;
            db.Config = b.Config;
            db.DemodulationConfig = config;

            // *** subtract background ***
            b.SubtractBackgroundFromProbeDetectorTOFs();

            // *** create scaled bottom probe ***
            b.CreateScaledBottomProbe();

            // *** create asymmetry TOF ***
            b.ConstructAsymmetryTOF();

            // *** convert point detector data into TOFs ***
            b.TOFuliseSinglePointData();

            return db;
        }

        // DemodulateBlockNL augments the channel values returned by DemodulateBlock
        // with all non-linear combinations of channels (E.B/DB, the correction, etc)
        // that involve the asymmetry detector.
        // These non-linear channels are calculated point-by-point for the TOF and then
        // integrated according to the Demodulation config.
        public DemodulatedBlockNewScheme DemodulateBlockNL(Block b, DemodulationConfigNewScheme config)
        {
            // we start with the standard demodulated block
            DemodulatedBlockNewScheme dblock = DemodulateBlock(b, config);

            int adi = dblock.DetectorIndices["asymmetry"];
            // TOF demodulate the block to get the channel wiggles
            // the BlockTOFDemodulator only demodulates the PMT detector
            BlockTOFDemodulator btd = new BlockTOFDemodulator();
            TOFChannelSet tcs = btd.TOFDemodulateBlock(b, adi, false);

            // get hold of the gating data
            GatedDetectorExtractSpec gate = config.GatedDetectorExtractSpecs["asymmetry"];

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
            DetectorChannelValues dcv = dblock.ChannelValues[adi];
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
            dblock.ChannelValues[adi].SpecialValues["EDMDB"] = new double[] { edmDBG, edmDBE };
            dblock.ChannelValues[adi].SpecialValues["CORRDB"] = new double[] { corrDBG, corrDBE };
            dblock.ChannelValues[adi].SpecialValues["EDMCORRDB"] = new double[] { edmCorrDBG, edmCorrDBE };
            dblock.ChannelValues[adi].SpecialValues["CORRDB_OLD"] = new double[] { corrDBG_old, corrDBE };
            dblock.ChannelValues[adi].SpecialValues["EDMCORRDB_OLD"] = new double[] { edmCorrDBG_old, edmCorrDBE };
            dblock.ChannelValues[adi].SpecialValues["RF1FDB"] = new double[] { rf1fDBG, rf1fDBE };
            dblock.ChannelValues[adi].SpecialValues["RF2FDB"] = new double[] { rf2fDBG, rf2fDBE };
            dblock.ChannelValues[adi].SpecialValues["RF1FDBDB"] = new double[] { rf1fDBDBG, rf1fDBDBE };
            dblock.ChannelValues[adi].SpecialValues["RF2FDBDB"] = new double[] { rf2fDBDBG, rf2fDBDBE };
            dblock.ChannelValues[adi].SpecialValues["RF1ADB"] = new double[] { rf1aDBG, rf1aDBE };
            dblock.ChannelValues[adi].SpecialValues["RF2ADB"] = new double[] { rf2aDBG, rf2aDBE };
            dblock.ChannelValues[adi].SpecialValues["RF1ADBDB"] = new double[] { rf1aDBDBG, rf1aDBDBE };
            dblock.ChannelValues[adi].SpecialValues["RF2ADBDB"] = new double[] { rf2aDBDBG, rf2aDBDBE };
            dblock.ChannelValues[adi].SpecialValues["BRF1FCORRDB"] = new double[] { brf1fCorrDBG, brf1fDBE };
            dblock.ChannelValues[adi].SpecialValues["BRF2FCORRDB"] = new double[] { brf2fCorrDBG, brf2fDBE };
            dblock.ChannelValues[adi].SpecialValues["ERF1FDB"] = new double[] { erf1fDBG, erf1fDBE };
            dblock.ChannelValues[adi].SpecialValues["ERF2FDB"] = new double[] { erf2fDBG, erf2fDBE };
            dblock.ChannelValues[adi].SpecialValues["ERF1FDBDB"] = new double[] { erf1fDBDBG, erf1fDBDBE };
            dblock.ChannelValues[adi].SpecialValues["ERF2FDBDB"] = new double[] { erf2fDBDBG, erf2fDBDBE };
            dblock.ChannelValues[adi].SpecialValues["LF1DB"] = new double[] { lf1DBG, lf1DBE };
            dblock.ChannelValues[adi].SpecialValues["LF1DBDB"] = new double[] { lf1DBDBG, lf1DBDBE };
            dblock.ChannelValues[adi].SpecialValues["LF2DB"] = new double[] { lf2DBG, lf2DBE };
            dblock.ChannelValues[adi].SpecialValues["LF2DBDB"] = new double[] { lf2DBDBG, lf2DBDBE };
            dblock.ChannelValues[adi].SpecialValues["BDB"] = new double[] { BDBG, BDBE };

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