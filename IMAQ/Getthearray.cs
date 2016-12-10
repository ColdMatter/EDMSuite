 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAQ
{
    public static class Getthearray
    {

        public static byte[,] convertToU8(NationalInstruments.Vision.Rgb32Value[,] rgb32Array)
        {
            byte[,] arrayToReturn = new byte[rgb32Array.GetLength(0),rgb32Array.GetLength(1)];
            for(int i = 0; i < rgb32Array.GetLength(0); i++)
            {
                for (int j = 0; j < rgb32Array.GetLength(1); j++){
                    arrayToReturn[i,j] = rgb32Array[i,j].Blue;


                }
            }
            return arrayToReturn;

        }

        public static double [] convertTO1D(byte[,] U8array)
        {
            double[] arrayToReturn = new double[U8array.GetLength(0)];
            double temp;
            for (int i = 0; i < U8array.GetLength(0); i++ )
                {
                    temp = 0;
                    for (int j = 0; j < U8array.GetLength(0); j++)
                        {
                            if (U8array[i, j] > 3)
                            {
                                temp = (double)U8array[i, j] + temp;
                            }
                        }
                    arrayToReturn[i] = temp;

                }
            return arrayToReturn;

        }

        public static double Findthemaximum(byte[,] U8array)
        {
            double max = 0;
            for (int i=0; i<U8array.GetLength(0);i++)
            {
                for (int j = 0; j < U8array.GetLength(1); j++)
                {
                    if( max<(double) U8array[i,j])
                    {
                        max = U8array[i, j];

                    }
                }
            }
            return max;
        }

        public static double[] CreateArray(int min, int max)
        {
            double [] a = new Double [max - min];
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = min + i;
            }
            return a;
        }


        public static double[] smooth(double[] data)
        {
            double[] final = new Double[data.GetLength(0)];
            for (int i = 1; i < data.GetLength(0)-1;i++)
            {

                    final[i] = (data[i - 1] + data[i + 1] + data[i]) / 3;

            }
            return final;
        }

    }
      
}
