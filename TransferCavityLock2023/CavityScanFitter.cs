using System;
using System.Collections.Generic;
using System.Text;
using NationalInstruments.Analysis.Math;

namespace TransferCavityLock
{
    class CavityScanFitter
    {
        public static double[] FitLorenzianToSlaveData(CavityScanData data, double limitLow, double limitHigh)
        {
            double mse = 0;
            double[] voltages = data.parameters.CalculateRampVoltages();
            double[] coefficients = new double[] {(data.parameters.High - data.parameters.Low)/10, voltages[ArrayOperation.GetIndexOfMax(data.SlavePhotodiodeData)],
                ArrayOperation.GetMax(data.SlavePhotodiodeData) - ArrayOperation.GetMin(data.SlavePhotodiodeData), 0};
            CurveFit.NonLinearFit(voltages, data.SlavePhotodiodeData, new ModelFunctionCallback(lorentzianNarrow),
                    coefficients, out mse, 4000);
            fitFailSafe(coefficients, limitLow, limitHigh);
            
            return coefficients;
        }
        public static double[] FitLorenzianToMasterData(CavityScanData data, double limitLow, double limitHigh)
        {
            double mse = 0;
            double[] voltages = data.parameters.CalculateRampVoltages();
            double[] coefficients = new double[] {(data.parameters.High - data.parameters.Low)/10, voltages[ArrayOperation.GetIndexOfMax(data.MasterPhotodiodeData)],
                ArrayOperation.GetMax(data.MasterPhotodiodeData) - ArrayOperation.GetMin(data.MasterPhotodiodeData), 0};
            
            CurveFit.NonLinearFit(voltages, data.MasterPhotodiodeData, new ModelFunctionCallback(lorentzianNarrow),
                    coefficients, out mse, 4000);
            
            fitFailSafe(coefficients, limitLow, limitHigh);

            return coefficients;
        }

        private static double[] fitFailSafe(double[] coefficients, double limitLow, double limitHigh)
        {
            if (coefficients[1] < limitLow)
            { coefficients[1] = limitLow; }
            else if (coefficients[1] > limitHigh)
            { coefficients[1] = limitHigh; }

            return coefficients;
        }

        private static double lorentzian(double x, double[] parameters) //A Lorentzian
        {
            double width = parameters[0];
            double centroid = parameters[1];
            double amplitude = parameters[2];
            double offset = parameters[3];
            if (width < 0) width = Math.Abs(width); // watch out for divide by zero
            return offset + amplitude / (1 + Math.Pow((1 / width), 2) * Math.Pow(x - centroid, 2));
        }
        private static double lorentzianNarrow(double x, double[] parameters) //A Narrow Lorentzian (Kind of silly to have to have this...)
        {
            double width = parameters[0];
            double centroid = parameters[1];
            double amplitude = parameters[2];
            double offset = parameters[3];

            if (width < 0) width = Math.Abs(width); // watch out for divide by zero
            return offset + amplitude / (1 + Math.Pow((1 / width), 2) * Math.Pow(x - centroid, 2));
        }

    }
}
