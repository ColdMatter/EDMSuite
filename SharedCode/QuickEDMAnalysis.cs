using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

using Data.EDM;
using EDMConfig;

namespace Analysis.EDM
{
    [Serializable]
    public class QuickEDMAnalysis
    {
        // constants
        private static double plateSpacing = 1.2;
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
        public double[] SIGValAndErrtp;
        public double[] BValAndErrtp;
        public double[] DBValAndErrtp;
        public double[] EValAndErrtp;
        public double[] EBValAndErrtp;
        public double[] BDBValAndErrtp;

        // analysis results bottom probe 
        public double[] SIGValAndErrbp;
        public double[] BValAndErrbp;
        public double[] DBValAndErrbp;
        public double[] EValAndErrbp;
        public double[] EBValAndErrbp;
        public double[] BDBValAndErrbp;

        public double[] NorthCurrentValAndError;
        public double[] SouthCurrentValAndError;
        public double[] NorthECorrCurrentValAndError;
        public double[] SouthECorrCurrentValAndError;

        public double[] MagValandErr;

        public double[] rf1FreqAndErr;
        public double[] rf2FreqAndErr;
        public double[] rf1AmpAndErr;
        public double[] rf2AmpAndErr;

        public double[] rf1FreqAndErrbp;
        public double[] rf2FreqAndErrbp;
        public double[] rf1AmpAndErrbp;
        public double[] rf2AmpAndErrbp;

        public double[] rf1FreqAndErrtp;
        public double[] rf2FreqAndErrtp;
        public double[] rf1AmpAndErrtp;
        public double[] rf2AmpAndErrtp;

        public double[] RF1FDBDB;
        public double[] RF2FDBDB;
        public double[] RF1ADBDB;
        public double[] RF2ADBDB;

        public double[] RF1FDBDBbp;
        public double[] RF2FDBDBbp;
        public double[] RF1ADBDBbp;
        public double[] RF2ADBDBbp;

        public double[] RF1FDBDBtp;
        public double[] RF2FDBDBtp;
        public double[] RF1ADBDBtp;
        public double[] RF2ADBDBtp;

        public double[] LF1ValAndErr;
        public double[] LF1DB;
        public double[] LF1DBDB;

        public static QuickEDMAnalysis AnalyseDBlock(GatedDemodulatedBlock dblock)
        {
            QuickEDMAnalysis analysis = new QuickEDMAnalysis();

            BlockConfig config = dblock.Config;
            //edm factor calculation
            double dbStep = ((AnalogModulation)config.GetModulationByName("DB")).Step;
            double magCal = (double)config.Settings["magnetCalibration"];
            double eField = cField((double)config.Settings["ePlus"], (double)config.Settings["eMinus"]);//arguments are in volts not kV
            double edmFactor = (bohrMagneton * dbStep * magCal * Math.Pow(10, -9)) /
                        (electronCharge * saturatedEffectiveField * polarisationFactor(eField));
            //Add in interferometer length instead of 800 10^-6 after testing is done with old blocks
            double dbPhaseStep = dbStep * magCal * Math.Pow(10, -9) * bohrMagneton * 800 * Math.Pow(10, -6) / hbar;

            //Get relevant channel values and errors for top probe
            analysis.SIGValAndErrtp = dblock.GetChannelValueAndError(new string[] { "SIG" }, "topProbeNoBackground");
            analysis.BValAndErrtp = dblock.GetChannelValueAndError(new string[] { "B" }, "topProbeNoBackground");
            analysis.DBValAndErrtp = dblock.GetChannelValueAndError(new string[] { "DB" }, "topProbeNoBackground");
            analysis.EValAndErrtp = dblock.GetChannelValueAndError(new string[] { "E" }, "topProbeNoBackground");
            analysis.EBValAndErrtp = dblock.GetChannelValueAndError(new string[] { "E", "B" }, "topProbeNoBackground");
            analysis.BDBValAndErrtp = dblock.GetChannelValueAndError(new string[] { "B", "DB" }, "topProbeNoBackground");

            //Get relevant channel values and errors for bottom probe
            analysis.SIGValAndErrbp = dblock.GetChannelValueAndError(new string[] { "SIG" }, "bottomProbeScaled");
            analysis.BValAndErrbp = dblock.GetChannelValueAndError(new string[] { "B" }, "bottomProbeScaled");
            analysis.DBValAndErrbp = dblock.GetChannelValueAndError(new string[] { "DB" }, "bottomProbeScaled");
            analysis.EValAndErrbp = dblock.GetChannelValueAndError(new string[] { "E" }, "bottomProbeScaled");
            analysis.EBValAndErrbp = dblock.GetChannelValueAndError(new string[] { "E", "B" }, "bottomProbeScaled");
            analysis.BDBValAndErrbp = dblock.GetChannelValueAndError(new string[] { "B", "DB" }, "bottomProbeScaled");

            //Get relevant channel values and errors for asymmetry
            analysis.SIGValAndErr = dblock.GetChannelValueAndError(new string[] { "SIG" }, "asymmetry");
            analysis.BValAndErr = dblock.GetChannelValueAndError(new string[] { "B" }, "asymmetry");
            analysis.DBValAndErr = dblock.GetChannelValueAndError(new string[] { "DB" }, "asymmetry");
            analysis.EValAndErr = dblock.GetChannelValueAndError(new string[] { "E" }, "asymmetry");
            analysis.EBValAndErr = dblock.GetChannelValueAndError(new string[] { "E", "B" }, "asymmetry");
            analysis.BDBValAndErr = dblock.GetChannelValueAndError(new string[] { "B", "DB" }, "asymmetry");

            double bottomProbeCalibration = dblock.GetCalibration("bottomProbe");
            double topProbeCalibration = dblock.GetCalibration("topProbe");
            //Replace 510 with the calibrations above after testing is done with old blocks
            analysis.ShotNoise = 1.0 / Math.Sqrt(analysis.SIGValAndErrbp[0] * 510 + analysis.SIGValAndErrtp[0] * 510);
            analysis.Contrast = analysis.DBValAndErr[0] / 2 / dbPhaseStep;

            //corrected edm in asymmetry detector
            analysis.RawEDMValAndErr = dblock.GetChannelValueAndError("EDMCORRDB", "asymmetry");

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
            analysis.RF1FDBDB = dblock.GetChannelValueAndError("RF1FDBDB", "asymmetry");
            analysis.RF2FDBDB = dblock.GetChannelValueAndError("RF2FDBDB", "asymmetry");

            //rf amp
            analysis.rf1AmpAndErr = dblock.GetChannelValueAndError(new string[] { "RF1A" }, "asymmetry");
            analysis.rf2AmpAndErr = dblock.GetChannelValueAndError(new string[] { "RF2A" }, "asymmetry");
            analysis.RF1ADBDB = dblock.GetChannelValueAndError("RF1ADBDB", "asymmetry");
            analysis.RF2ADBDB = dblock.GetChannelValueAndError("RF2ADBDB", "asymmetry");

            //rf freq bottom probe
            analysis.rf1FreqAndErrbp = dblock.GetChannelValueAndError(new string[] { "RF1F" }, "bottomProbeScaled");
            analysis.rf2FreqAndErrbp = dblock.GetChannelValueAndError(new string[] { "RF2F" }, "bottomProbeScaled");
            analysis.RF1FDBDBbp = dblock.GetChannelValueAndError("RF1FDBDB", "bottomProbeScaled");
            analysis.RF2FDBDBbp = dblock.GetChannelValueAndError("RF2FDBDB", "bottomProbeScaled");

            //rf amp bottom probe
            analysis.rf1AmpAndErrbp = dblock.GetChannelValueAndError(new string[] { "RF1A" }, "bottomProbeScaled");
            analysis.rf2AmpAndErrbp = dblock.GetChannelValueAndError(new string[] { "RF2A" }, "bottomProbeScaled");
            analysis.RF1ADBDBbp = dblock.GetChannelValueAndError("RF1ADBDB", "bottomProbeScaled");
            analysis.RF2ADBDBbp = dblock.GetChannelValueAndError("RF2ADBDB", "bottomProbeScaled");

            //rf freq top probe
            analysis.rf1FreqAndErrtp = dblock.GetChannelValueAndError(new string[] { "RF1F" }, "topProbeNoBackground");
            analysis.rf2FreqAndErrtp = dblock.GetChannelValueAndError(new string[] { "RF2F" }, "topProbeNoBackground");
            analysis.RF1FDBDBtp = dblock.GetChannelValueAndError("RF1FDBDB", "topProbeNoBackground");
            analysis.RF2FDBDBtp = dblock.GetChannelValueAndError("RF2FDBDB", "topProbeNoBackground");

            //rf amp top probe
            analysis.rf1AmpAndErrtp = dblock.GetChannelValueAndError(new string[] { "RF1A" }, "topProbeNoBackground");
            analysis.rf2AmpAndErrtp = dblock.GetChannelValueAndError(new string[] { "RF2A" }, "topProbeNoBackground");
            analysis.RF1ADBDBtp = dblock.GetChannelValueAndError("RF1ADBDB", "topProbeNoBackground");
            analysis.RF2ADBDBtp = dblock.GetChannelValueAndError("RF2ADBDB", "topProbeNoBackground");

            //probe laser frequency
            analysis.LF1ValAndErr = dblock.GetChannelValueAndError(new string[] { "LF1" }, "asymmetry");
            analysis.LF1DB = dblock.GetChannelValueAndError("LF1DB", "asymmetry");
            analysis.LF1DBDB = dblock.GetChannelValueAndError("LF1DBDB", "asymmetry");
            
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
