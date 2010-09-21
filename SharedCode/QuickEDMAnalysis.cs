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
        
        // analysis results
        public double[] SIGValAndErr;
        public double[] BValAndErr;
        public double[] DBValAndErr;
        public double[] EValAndErr;
        public double[] EBValAndErr;
        public double RawEDM;
        public double RawEDMErr;

        public double[] SIGValAndErrNormed;
        public double[] BValAndErrNormed;
        public double[] DBValAndErrNormed;
        public double[] EValAndErrNormed;
        public double[] EBValAndErrNormed;
        public double RawEDMNormed;
        public double RawEDMErrNormed;

        public double[] NorthCurrentValAndError;
        public double[] SouthCurrentValAndError;

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

            //Get relevant channel values and errors
            analysis.SIGValAndErr = dblock.GetChannelValueAndError(new string[] { "SIG" }, "top");
            analysis.BValAndErr = dblock.GetChannelValueAndError(new string[] { "B" }, "top");
            analysis.DBValAndErr = dblock.GetChannelValueAndError(new string[] { "DB" }, "top");
            analysis.EValAndErr = dblock.GetChannelValueAndError(new string[] { "E" }, "top");
            analysis.EBValAndErr = dblock.GetChannelValueAndError(new string[] { "E", "B" }, "top");

            //edm error calculation
            analysis.RawEDM = edmFactor * (analysis.EBValAndErr[0] / analysis.DBValAndErr[0]);
            analysis.RawEDMErr = Math.Abs(analysis.RawEDM)
                * Math.Sqrt(Math.Pow(analysis.EBValAndErr[1] / analysis.EBValAndErr[0], 2)
                               + Math.Pow(analysis.DBValAndErr[1] / analysis.DBValAndErr[0], 2));

            //same again for normed data.
            analysis.SIGValAndErrNormed = dblock.GetChannelValueAndError(new string[] { "SIG" }, "topNormed");
            analysis.BValAndErrNormed = dblock.GetChannelValueAndError(new string[] { "B" }, "topNormed");
            analysis.DBValAndErrNormed = dblock.GetChannelValueAndError(new string[] { "DB" }, "topNormed");
            analysis.EValAndErrNormed = dblock.GetChannelValueAndError(new string[] { "E" }, "topNormed");
            analysis.EBValAndErrNormed = dblock.GetChannelValueAndError(new string[] { "E", "B" }, "topNormed");

            //normed edm error
            analysis.RawEDMNormed = edmFactor * (analysis.EBValAndErrNormed[0] / analysis.DBValAndErrNormed[0]);
            analysis.RawEDMErrNormed = Math.Abs(analysis.RawEDMNormed)
                * Math.Sqrt(Math.Pow(analysis.EBValAndErrNormed[1] / analysis.EBValAndErrNormed[0], 2)
                                     + Math.Pow(analysis.DBValAndErrNormed[1] / analysis.DBValAndErrNormed[0], 2));

            //leakage currents
            analysis.NorthCurrentValAndError = 
                dblock.GetChannelValueAndError(new string[] { "SIG" }, "NorthCurrent");
            analysis.SouthCurrentValAndError =
                dblock.GetChannelValueAndError(new string[] { "SIG" }, "SouthCurrent");

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
            return efield / 1000;
        }
        
    }
}
