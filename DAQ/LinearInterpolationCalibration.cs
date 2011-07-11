using System;
using System.Collections.Generic;
using System.Text;

namespace DAQ.HAL
{
    public class LinearInterpolationCalibration : Calibration
    {
        //if input sits between points a and b, calibrate to a linear interpolation between the two
        double[,] interpolationData;
        public LinearInterpolationCalibration(double[,] interpolationData)
        {
            this.interpolationData = interpolationData;
            rangeLow = interpolationData[0, 0];
            rangeHigh = interpolationData[0, interpolationData.GetLength(0)];
        }

        public override double Convert(double input)
        {
            CheckRange(input);
            double idLength = interpolationData.GetLength(1);
            int n = 0;
            for (int i = 0; i < idLength; i++)
            {
                if (interpolationData[0, i] == input)
                {
                    return interpolationData[1, i];
                }
                else
                {
                    if (interpolationData[0, i] < input)
                    {
                        n = i;
                    }
                }
            }
            double m = (interpolationData[1, n + 1] - interpolationData[1, n])
                / (interpolationData[0, n + 1] - interpolationData[0, n]);
            return interpolationData[1, n] + (m * (input - interpolationData[0, n]));
        }

    }
}
