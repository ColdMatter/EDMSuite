using System;
using System.Collections.Generic;
using System.Text;

using DAQ.Analyze;

namespace TransferCavityLock2012
{
    class CavityScanFitHelper
    {
       

        public static double[] FitLorenzianToSlaveData(CavityScanData data)
        {
            double[] voltages = data.CavityData;
            double high = data.CavityData[data.CavityData.Length-1];
            double low = data.CavityData[0];
            System.Diagnostics.Debug.WriteLine("   " + low.ToString() + "   " + high.ToString());
            LorentzianFitter lorentzianFitter = new LorentzianFitter();

            // takes the parameters (in this order, in the double[])
            // N: background
            // Q: signal
            // c: centre
            // w: width

            dataPoint max = getMax(data.SlavePhotodiodeData);
            dataPoint min = getMin(data.SlavePhotodiodeData);
            lorentzianFitter.Fit(voltages, data.SlavePhotodiodeData,
                new double[] {min.value, max.value - min.value,
                    voltages[max.index], (high - low)/10,
                });
            
            double[] coefficients = lorentzianFitter.Parameters;

            fitFailSafe(coefficients, low, high); 

            return new double[] { coefficients[3], coefficients[2], coefficients[1], coefficients[0] }; //to be consistent with old convention.
        }

        public static double[] FitLorenzianToMasterData(CavityScanData data)
        {
            double[] voltages = data.CavityData;
            double high = data.CavityData[data.CavityData.Length - 1];
            double low = data.CavityData[0];
            LorentzianFitter lorentzianFitter = new LorentzianFitter();

            // takes the parameters (in this order, in the double[])
            // N: background
            // Q: signal
            // c: centre
            // w: width
            dataPoint max = getMax(data.MasterPhotodiodeData);
            dataPoint min = getMin(data.MasterPhotodiodeData);
            lorentzianFitter.Fit(voltages, data.MasterPhotodiodeData,
                new double[] {min.value, max.value - min.value,
                    voltages[max.index], (high - low)/10,
                });

            double[] coefficients = lorentzianFitter.Parameters;

            fitFailSafe(coefficients, low, high);

            return new double[] {coefficients[3],coefficients[2], coefficients[1], coefficients[0]};
        }

        private class dataPoint
        {
            public double value;
            public int index;
        }
        private static dataPoint getMax(double[] data)
        {
            dataPoint max = new dataPoint();
            max.value = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] > max.value)
                {
                    max.value = data[i];
                    max.index = i;
                }
            }
            return max;
        }
        
        private static dataPoint getMin(double[] data)
        {
            dataPoint min = new dataPoint();
            min.value = data[0];
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] < min.value)
                {
                    min.value = data[i];
                    min.index = i;
                }
            }
            return min;
        }
        private static double[] fitFailSafe(double[] coefficients, double limitLow, double limitHigh)
        {
            if (coefficients[2] < limitLow)
            { coefficients[2] = limitLow; }
            else if (coefficients[2] > limitHigh)
            { coefficients[2] = limitHigh; }

            return coefficients;
        }
    }
}
