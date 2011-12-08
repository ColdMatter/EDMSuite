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
            rangeHigh = interpolationData[interpolationData.GetLength(0)-1, 0];
        }

        public override double Convert(double input)
        {
            CheckRange(input);
            double idLength = interpolationData.GetLength(0);
            int n = 0;
            for (int i = 0; i < idLength; i++)
            {
                if (interpolationData[i , 0 ] == input)
                {
                    return interpolationData[i,0];
                }
                else
                {
                    if (interpolationData[i,0] < input)
                    {
                        n = i;
                    }
                }
            }
            double m = (interpolationData[n + 1, 1] - interpolationData[ n,1])
                / (interpolationData[ n + 1,0] - interpolationData[ n,0]);
            return interpolationData[ n,1] + (m * (input - interpolationData[n,0]));
        }

    }
}
