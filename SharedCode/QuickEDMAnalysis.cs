using System;
using System.Collections.Generic;
using System.Text;

using Data.EDM;
using EDMConfig;

namespace Analysis.EDM
{
    public class QuickEDMAnalysis
    {
        // constants
        private static double plateSpacing = 1.2;
        private static double electronCharge = 1.6022 * Math.Pow(10, -19);
        private static double bohrMagneton = 9.274 * Math.Pow(10, -24);
        private static double saturatedEffectiveField = 26 * Math.Pow(10, 9);
        
        // analysis results for asymmetry
        public double[] SIGValAndErr;
        public double[] BValAndErr;
        public double[] DBValAndErr;
        public double[] EValAndErr;
        public double[] EBValAndErr;
        public double[] BDBValAndErr;
        public double[] RawEDMValAndErr;

        // analysis results top probe 
        public double[] SIGValAndErrtp;
        public double[] BValAndErrtp;
        public double[] DBValAndErrtp;
        public double[] EValAndErrtp;
        public double[] EBValAndErrtp;

        // analysis results bottom probe 
        public double[] SIGValAndErrbp;
        public double[] BValAndErrbp;
        public double[] DBValAndErrbp;
        public double[] EValAndErrbp;
        public double[] EBValAndErrbp;

        public double[] NorthCurrentValAndError;
        public double[] SouthCurrentValAndError;
        public double[] NorthECorrCurrentValAndError;
        public double[] SouthECorrCurrentValAndError;

        public double[] MagValandErr;

        public double[] rf1FreqAndErr;
        public double[] rf2FreqAndErr;
        public double[] rf1AmpAndErr;
        public double[] rf2AmpAndErr;

        public double[] RF1FDBDB;
        public double[] RF2FDBDB;
        public double[] RF1ADBDB;
        public double[] RF2ADBDB;

        public double[] LF1ValAndErr;
        public double[] LF1DB;
        public double[] LF1DBDB;

        public static QuickEDMAnalysis AnalyseDBlock(DemodulatedBlock dblock)
        {
            QuickEDMAnalysis analysis = new QuickEDMAnalysis();

            BlockConfig config = dblock.Config;
            //edm factor calculation
            double dbStep = ((AnalogModulation)config.GetModulationByName("DB")).Step;
            double magCal = (double)config.Settings["magnetCalibration"];
            double eField = cField((double)config.Settings["ePlus"], (double)config.Settings["eMinus"]);//arguments are in volts not kV
            double edmFactor = (bohrMagneton * dbStep * magCal * Math.Pow(10, -9)) /
                        (electronCharge * saturatedEffectiveField * polarisationFactor(eField));

            //Get relevant channel values and errors for top probe
            analysis.SIGValAndErrtp = dblock.GetChannelValueAndError(new string[] { "SIG" }, "topProbeNoBackground");
            analysis.BValAndErrtp = dblock.GetChannelValueAndError(new string[] { "B" }, "topProbeNoBackground");
            analysis.DBValAndErrtp = dblock.GetChannelValueAndError(new string[] { "DB" }, "topProbeNoBackground");
            analysis.EValAndErrtp = dblock.GetChannelValueAndError(new string[] { "E" }, "topProbeNoBackground");
            analysis.EBValAndErrtp = dblock.GetChannelValueAndError(new string[] { "E", "B" }, "topProbeNoBackground");

            //Get relevant channel values and errors for bottom probe
            analysis.SIGValAndErrbp = dblock.GetChannelValueAndError(new string[] { "SIG" }, "bottomProbeScaled");
            analysis.BValAndErrbp = dblock.GetChannelValueAndError(new string[] { "B" }, "bottomProbeScaled");
            analysis.DBValAndErrbp = dblock.GetChannelValueAndError(new string[] { "DB" }, "bottomProbeScaled");
            analysis.EValAndErrbp = dblock.GetChannelValueAndError(new string[] { "E" }, "bottomProbeScaled");
            analysis.EBValAndErrbp = dblock.GetChannelValueAndError(new string[] { "E", "B" }, "bottomProbeScaled");

            //Get relevant channel values and errors for asymmetry
            analysis.SIGValAndErr = dblock.GetChannelValueAndError(new string[] { "SIG" }, "asymmetry");
            analysis.BValAndErr = dblock.GetChannelValueAndError(new string[] { "B" }, "asymmetry");
            analysis.DBValAndErr = dblock.GetChannelValueAndError(new string[] { "DB" }, "asymmetry");
            analysis.EValAndErr = dblock.GetChannelValueAndError(new string[] { "E" }, "asymmetry");
            analysis.EBValAndErr = dblock.GetChannelValueAndError(new string[] { "E", "B" }, "asymmetry");
            analysis.BDBValAndErr = dblock.GetChannelValueAndError(new string[] { "B", "DB" }, "asymmetry");

            //corrected edm in asymmetry detector
            analysis.RawEDMValAndErr = dblock.GetSpecialChannelValueAndError("EDMCORRDB", "asymmetry");

            //leakage currents
            analysis.NorthCurrentValAndError = 
                dblock.GetChannelValueAndError(new string[] { "SIG" }, "NorthCurrent");
            analysis.SouthCurrentValAndError =
                dblock.GetChannelValueAndError(new string[] { "SIG" }, "SouthCurrent");
            analysis.NorthECorrCurrentValAndError =
                dblock.GetChannelValueAndError(new string[] { "E" }, "NorthCurrent");
            analysis.SouthECorrCurrentValAndError =
                dblock.GetChannelValueAndError(new string[] { "E" }, "SouthCurrent");

            //magnetometer (I know it is not signed right but I just want the noise so any waveform will do)
            analysis.MagValandErr = dblock.GetChannelValueAndError(new string[] { "SIG" }, "magnetometer");
           
            //rf freq
            analysis.rf1FreqAndErr = dblock.GetChannelValueAndError(new string[] { "RF1F" }, "asymmetry");
            analysis.rf2FreqAndErr = dblock.GetChannelValueAndError(new string[] { "RF2F" }, "asymmetry");
            analysis.RF1FDBDB = dblock.GetSpecialChannelValueAndError("RF1FDBDB", "asymmetry");
            analysis.RF2FDBDB = dblock.GetSpecialChannelValueAndError("RF2FDBDB", "asymmetry");

            //rf amp
            analysis.rf1AmpAndErr = dblock.GetChannelValueAndError(new string[] { "RF1A" }, "asymmetry");
            analysis.rf2AmpAndErr = dblock.GetChannelValueAndError(new string[] { "RF2A" }, "asymmetry");
            analysis.RF1ADBDB = dblock.GetSpecialChannelValueAndError("RF1ADBDB", "asymmetry");
            analysis.RF2ADBDB = dblock.GetSpecialChannelValueAndError("RF2ADBDB", "asymmetry");

            //probe laser frequency
            analysis.LF1ValAndErr = dblock.GetChannelValueAndError(new string[] { "LF1" }, "asymmetry");
            analysis.LF1DBDB = dblock.GetSpecialChannelValueAndError("LF1DB", "asymmetry");
            analysis.LF1DBDB = dblock.GetSpecialChannelValueAndError("LF1DBDB", "asymmetry");
            
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
        
    }
}
