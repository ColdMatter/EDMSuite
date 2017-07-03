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
        private static double plateSpacing = 1.1;
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
        public double RawEDM;
        public double RawEDMErr;

        // analysis results top probe 
        public double[] SIGValAndErrtp;
        public double[] BValAndErrtp;
        public double[] DBValAndErrtp;
        public double[] EValAndErrtp;
        public double[] EBValAndErrtp;
        public double RawEDMtp;
        public double RawEDMErrtp;

        // analysis results bottom probe 
        public double[] SIGValAndErrbp;
        public double[] BValAndErrbp;
        public double[] DBValAndErrbp;
        public double[] EValAndErrbp;
        public double[] EBValAndErrbp;
        public double RawEDMbp;
        public double RawEDMErrbp;

        public double[] NorthCurrentValAndError;
        public double[] SouthCurrentValAndError;
        public double[] NorthECorrCurrentValAndError;
        public double[] SouthECorrCurrentValAndError;

        public double[] MagValandErr;

        public double[] rf1FreqAndErrtp;
        public double[] rf2FreqAndErrtp;
        public double[] rf1FreqAndErrbp;
        public double[] rf2FreqAndErrbp;
        public double[] rf1AmpAndErrtp;
        public double[] rf2AmpAndErrtp;
        public double[] rf1AmpAndErrbp;
        public double[] rf2AmpAndErrbp;

        public double[] RF1FDBDB;
        public double[] RF2FDBDB;
        public double[] RF1ADBDB;
        public double[] RF2ADBDB;

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
            analysis.SIGValAndErrtp = dblock.GetChannelValueAndError(new string[] { "SIG" }, "topProbe");
            analysis.BValAndErrtp = dblock.GetChannelValueAndError(new string[] { "B" }, "topProbe");
            analysis.DBValAndErrtp = dblock.GetChannelValueAndError(new string[] { "DB" }, "topProbe");
            analysis.EValAndErrtp = dblock.GetChannelValueAndError(new string[] { "E" }, "topProbe");
            analysis.EBValAndErrtp = dblock.GetChannelValueAndError(new string[] { "E", "B" }, "topProbe");

            //edm error calculation
            analysis.RawEDMtp = edmFactor * (analysis.EBValAndErrtp[0] / analysis.DBValAndErrtp[0]);
            analysis.RawEDMErrtp = Math.Abs(analysis.RawEDMtp)
                * Math.Sqrt(Math.Pow(analysis.EBValAndErrtp[1] / analysis.EBValAndErrtp[0], 2)
                               + Math.Pow(analysis.DBValAndErrtp[1] / analysis.DBValAndErrtp[0], 2));

            //Get relevant channel values and errors for bottom probe
            analysis.SIGValAndErrbp = dblock.GetChannelValueAndError(new string[] { "SIG" }, "bottomProbe");
            analysis.BValAndErrbp = dblock.GetChannelValueAndError(new string[] { "B" }, "bottomProbe");
            analysis.DBValAndErrbp = dblock.GetChannelValueAndError(new string[] { "DB" }, "bottomProbe");
            analysis.EValAndErrbp = dblock.GetChannelValueAndError(new string[] { "E" }, "bottomProbe");
            analysis.EBValAndErrbp = dblock.GetChannelValueAndError(new string[] { "E", "B" }, "bottomProbe");

            //edm error calculation for bottom probe
            analysis.RawEDMbp = edmFactor * (analysis.EBValAndErrbp[0] / analysis.DBValAndErrbp[0]);
            analysis.RawEDMErrbp = Math.Abs(analysis.RawEDMbp)
                * Math.Sqrt(Math.Pow(analysis.EBValAndErrbp[1] / analysis.EBValAndErrbp[0], 2)
                               + Math.Pow(analysis.DBValAndErrbp[1] / analysis.DBValAndErrbp[0], 2));

            //Get relevant channel values and errors for asymmetry
            analysis.SIGValAndErr = dblock.GetChannelValueAndError(new string[] { "SIG" }, "asymmetry");
            analysis.BValAndErr = dblock.GetChannelValueAndError(new string[] { "B" }, "asymmetry");
            analysis.DBValAndErr = dblock.GetChannelValueAndError(new string[] { "DB" }, "asymmetry");
            analysis.EValAndErr = dblock.GetChannelValueAndError(new string[] { "E" }, "asymmetry");
            analysis.EBValAndErr = dblock.GetChannelValueAndError(new string[] { "E", "B" }, "asymmetry");
            analysis.BDBValAndErr = dblock.GetChannelValueAndError(new string[] { "B", "DB" }, "asymmetry");

            //edm error calculation for asymmetry
            analysis.RawEDM = edmFactor * (analysis.EBValAndErr[0] / analysis.DBValAndErr[0]);
            analysis.RawEDMErr = Math.Abs(analysis.RawEDM)
                * Math.Sqrt(Math.Pow(analysis.EBValAndErr[1] / analysis.EBValAndErr[0], 2)
                               + Math.Pow(analysis.DBValAndErr[1] / analysis.DBValAndErr[0], 2));
            


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
            analysis.rf1FreqAndErrtp = dblock.GetChannelValueAndError(new string[] { "RF1F" }, "topProbe");
            analysis.rf2FreqAndErrtp = dblock.GetChannelValueAndError(new string[] { "RF2F" }, "topProbe");
            analysis.rf1FreqAndErrbp = dblock.GetChannelValueAndError(new string[] { "RF1F" }, "bottomProbe");
            analysis.rf2FreqAndErrbp = dblock.GetChannelValueAndError(new string[] { "RF2F" }, "bottompProbe");
            analysis.RF1FDBDB = dblock.GetChannelValueAndError( new string[] { "RF1F", "DB" }, "asymmetry" );
            analysis.RF2FDBDB = dblock.GetChannelValueAndError(new string[] { "RF2F", "DB" }, "asymmetry");

            //rf amp
            analysis.rf1AmpAndErrtp = dblock.GetChannelValueAndError(new string[] { "RF1A" }, "topProbe");
            analysis.rf2AmpAndErrtp = dblock.GetChannelValueAndError(new string[] { "RF2A" }, "topProbe");
            analysis.rf1AmpAndErrbp = dblock.GetChannelValueAndError(new string[] { "RF1A" }, "bottomProbe");
            analysis.rf2AmpAndErrbp = dblock.GetChannelValueAndError(new string[] { "RF2A" }, "bottomProbe");
            analysis.RF1ADBDB = dblock.GetChannelValueAndError( new string[] { "RF1A", "DB" }, "asymmetry" );
            analysis.RF2ADBDB = dblock.GetChannelValueAndError(new string[] { "RF2A", "DB" }, "asymmetry");

            
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
            //double efield = (ePlus - eMinus) / plateSpacing;
            double efield = (4) / plateSpacing;
            return efield / 1000;
        }
        
    }
}
