﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

using Data;
using Data.EDM;
using EDMConfig;

namespace Analysis.EDM
{
    [Serializable]
    public class QuickEDMAnalysis
    {
                
        // constants
        //private static double plateSpacing = 1.2; //ClassicEDM
        private static double plateSpacing = 1.825; //UEDM
        private static double electronCharge = 1.6022 * Math.Pow(10, -19);
        private static double bohrMagneton = 9.274 * Math.Pow(10, -24);
        private static double saturatedEffectiveField = 26 * Math.Pow(10, 9);
        private static double hbar = 1.05457 * Math.Pow(10, -34);

        // analysis results for asymmetry
        public double[] SIGValAndErr;
        public double[] BValAndErr;
        public double[] DBValAndErr;
        public double[] EValAndErr;
        public double[] EBValAndErr;
        public double[] BDBValAndErr;
        public double[] RawEDMValAndErr;

        public double ShotNoise;
        public double Contrast;

        // analysis results top probe 
        //public double[] SIGValAndErrtp; // ClassicEDM
        public double[] SIGValAndErrdB; // UEDM

        // analysis results bottom probe 
        //public double[] SIGValAndErrbp; // ClassicEDM
        public double[] SIGValAndErrdA; // UEDM

        //public double[] NorthCurrentValAndError;
        //public double[] SouthCurrentValAndError;
        //public double[] NorthECorrCurrentValAndError;
        //public double[] SouthECorrCurrentValAndError;

        public double[] WestCurrentValAndError;
        public double[] EastCurrentValAndError;
        public double[] WestECorrCurrentValAndError;
        public double[] EastECorrCurrentValAndError;

        public double[] MagValandErr;

        //public double[] rf1FreqAndErr;
        //public double[] rf2FreqAndErr;
        //public double[] rf1AmpAndErr;
        //public double[] rf2AmpAndErr;

        //public double[] RF1FDBDB;
        //public double[] RF2FDBDB;
        //public double[] RF1ADBDB;
        //public double[] RF2ADBDB;

        //public double[] LF1ValAndErr;
        //public double[] LF1DB;
        //public double[] LF1DBDB;

        //public double[] TopPDSIG;
        //public double[] BottomPDSIG;

        public static QuickEDMAnalysis AnalyseDBlock(DemodulatedBlock dblock, double GATE_LOW, double GATE_HIGH)
        {
            QuickEDMAnalysis analysis = new QuickEDMAnalysis();

            BlockConfig config = dblock.Config;
            //edm factor calculation
            double dbStep = ((AnalogModulation)config.GetModulationByName("DB")).Step;
            double magCal = (double)config.Settings["magnetCalibration"];
            double eField = cField((double)config.Settings["ePlus"], (double)config.Settings["eMinus"]);//arguments are in volts not kV
            double edmFactor = (bohrMagneton * dbStep * magCal * Math.Pow(10, -9) * 100) /
                        (electronCharge * saturatedEffectiveField * polarisationFactor(eField));
            //Add in interferometer length instead of 800 10^-6 after testing is done with old blocks
            double dbPhaseStep = dbStep * magCal * Math.Pow(10, -9) * bohrMagneton * 800 * Math.Pow(10, -6) / hbar;

            //analysis.SIGValAndErrtp = ConvertPointToArray(dblock.GetPointChannel(new string[] { "SIG" }, "topProbeNoBackground")); // ClassicEDM
            //analysis.SIGValAndErrbp = ConvertPointToArray(dblock.GetPointChannel(new string[] { "SIG" }, "bottomProbeScaled")); // ClassicEDM

            analysis.SIGValAndErrdB = ConvertPointToArray(dblock.GetPointChannel(new string[] { "SIG" }, "detBNoBackground")); // UEDM
            analysis.SIGValAndErrdA = ConvertPointToArray(dblock.GetPointChannel(new string[] { "SIG" }, "detAScaled")); // UEDM

            analysis.SIGValAndErr = dblock.GetTOFChannel(new string[] { "SIG" }, "asymmetry").GatedWeightedMeanAndUncertainty(GATE_LOW, GATE_HIGH);
            analysis.BValAndErr = dblock.GetTOFChannel(new string[] { "B" }, "asymmetry").GatedWeightedMeanAndUncertainty(GATE_LOW, GATE_HIGH);
            analysis.DBValAndErr = dblock.GetTOFChannel(new string[] { "DB" }, "asymmetry").GatedWeightedMeanAndUncertainty(GATE_LOW, GATE_HIGH);
            analysis.EValAndErr = dblock.GetTOFChannel(new string[] { "E" }, "asymmetry").GatedWeightedMeanAndUncertainty(GATE_LOW, GATE_HIGH);
            analysis.EBValAndErr = dblock.GetTOFChannel(new string[] { "E", "B" }, "asymmetry").GatedWeightedMeanAndUncertainty(GATE_LOW, GATE_HIGH);
            //analysis.BDBValAndErr = dblock.GetTOFChannel("BDB", "asymmetry").GatedWeightedMeanAndUncertainty(GATE_LOW, GATE_HIGH);

            //double bottomProbeCalibration = dblock.GetCalibration("bottomProbeScaled");       // ClassicEDM
            //double topProbeCalibration = dblock.GetCalibration("topProbeNoBackground");       // ClassicEDM
            //Replace 510 with the calibrations above after testing is done with old blocks
            //analysis.ShotNoise = 1.0 / Math.Sqrt(analysis.SIGValAndErrbp[0] * 510 + analysis.SIGValAndErrtp[0] * 510); // ClassicEDM
            analysis.ShotNoise = 1.0 / Math.Sqrt(analysis.SIGValAndErrdA[0] * 22.5 + analysis.SIGValAndErrdB[0] * 22.5); // ClassicEDM
            analysis.Contrast = analysis.DBValAndErr[0] / 2 / dbPhaseStep;

            //raw edm in asymmetry detector
            double[] edmdb;
            edmdb = dblock.GetTOFChannel("EDMDB", "asymmetry").GatedWeightedMeanAndUncertainty(GATE_LOW, GATE_HIGH);
            analysis.RawEDMValAndErr = new double[2] { edmFactor * edmdb[0], edmFactor * edmdb[1] };

            //leakage currents
            //analysis.NorthCurrentValAndError =
            //    ConvertPointToArray(dblock.GetPointChannel(new string[] { "SIG" }, "NorthCurrent"));
            //analysis.SouthCurrentValAndError =
            //    ConvertPointToArray(dblock.GetPointChannel(new string[] { "SIG" }, "SouthCurrent"));
            //analysis.NorthECorrCurrentValAndError =
            //    ConvertPointToArray(dblock.GetPointChannel(new string[] { "E" }, "NorthCurrent"));
            //analysis.SouthECorrCurrentValAndError =
            //    ConvertPointToArray(dblock.GetPointChannel(new string[] { "E" }, "SouthCurrent"));

            analysis.WestCurrentValAndError =
                ConvertPointToArray(dblock.GetPointChannel(new string[] { "SIG" }, "WestCurrent"));
            analysis.EastCurrentValAndError =
                ConvertPointToArray(dblock.GetPointChannel(new string[] { "SIG" }, "EastCurrent"));
            analysis.WestECorrCurrentValAndError =
                ConvertPointToArray(dblock.GetPointChannel(new string[] { "E" }, "WestCurrent"));
            analysis.EastECorrCurrentValAndError =
                ConvertPointToArray(dblock.GetPointChannel(new string[] { "E" }, "EastCurrent"));

            //magnetometer (I know it is not signed right but I just want the noise so any waveform will do)
            //analysis.MagValandErr = ConvertPointToArray(dblock.GetPointChannel(new string[] { "SIG" }, "magnetometer"));
            analysis.MagValandErr = ConvertPointToArray(dblock.GetPointChannel(new string[] { "SIG" }, "bartington_Y"));

            //rf freq
            //analysis.rf1FreqAndErr = dblock.GetTOFChannel(new string[] { "RF1F" }, "asymmetry").GatedWeightedMeanAndUncertainty(GATE_LOW, GATE_HIGH);
            //analysis.rf2FreqAndErr = dblock.GetTOFChannel(new string[] { "RF2F" }, "asymmetry").GatedWeightedMeanAndUncertainty(GATE_LOW, GATE_HIGH);
            //analysis.RF1FDBDB = dblock.GetTOFChannel("RF1FDBDB", "asymmetry").GatedWeightedMeanAndUncertainty(GATE_LOW, GATE_HIGH);
            //analysis.RF2FDBDB = dblock.GetTOFChannel("RF2FDBDB", "asymmetry").GatedWeightedMeanAndUncertainty(GATE_LOW, GATE_HIGH);

            //rf amp
            //analysis.rf1AmpAndErr = dblock.GetTOFChannel(new string[] { "RF1A" }, "asymmetry").GatedWeightedMeanAndUncertainty(GATE_LOW, GATE_HIGH);
            //analysis.rf2AmpAndErr = dblock.GetTOFChannel(new string[] { "RF2A" }, "asymmetry").GatedWeightedMeanAndUncertainty(GATE_LOW, GATE_HIGH);
            //analysis.RF1ADBDB = dblock.GetTOFChannel("RF1ADBDB", "asymmetry").GatedWeightedMeanAndUncertainty(GATE_LOW, GATE_HIGH);
            //analysis.RF2ADBDB = dblock.GetTOFChannel("RF2ADBDB", "asymmetry").GatedWeightedMeanAndUncertainty(GATE_LOW, GATE_HIGH);

            //probe laser frequency
            //analysis.LF1ValAndErr = dblock.GetTOFChannel(new string[] { "LF1" }, "asymmetry").GatedWeightedMeanAndUncertainty(GATE_LOW, GATE_HIGH);
            //analysis.LF1DB = dblock.GetTOFChannel("LF1DB", "asymmetry").GatedWeightedMeanAndUncertainty(GATE_LOW, GATE_HIGH);
            //analysis.LF1DBDB = dblock.GetTOFChannel("LF1DBDB", "asymmetry").GatedWeightedMeanAndUncertainty(GATE_LOW, GATE_HIGH);

            //probe photodiode monitors
            //analysis.TopPDSIG = ConvertPointToArray(dblock.GetPointChannel(new string[] {"SIG"}, "topPD"));
            //analysis.BottomPDSIG = ConvertPointToArray(dblock.GetPointChannel(new string[] {"SIG"}, "bottomPD"));
            return analysis;
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

        private static double[] ConvertPointToArray(PointWithError p)
        {
            return new double[2] { p.Value, p.Error };
        }

    }
}
