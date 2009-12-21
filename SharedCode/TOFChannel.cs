using System;
using System.Collections.Generic;
using System.Text;

using Data;

namespace Analysis.EDM
{
    [Serializable]
    public class TOFChannel : Channel<TOFWithError>
    {
        public double DifferenceChiSquared
        {
            get
            {
                double d = 0;
                for (int i = 0; i < Difference.Length; i++)
                    d += Math.Pow(Difference.Data[i] / Difference.Errors[i], 2);
                return d / ((double)Difference.Length);
            }
        }

        static public TOFChannel operator +(TOFChannel t1, TOFChannel t2)
        {
            TOFChannel temp = new TOFChannel();
            temp.On = t1.On + t2.On;
            temp.Off = t1.Off + t2.Off;
            temp.Difference = t1.Difference + t2.Difference;
            return temp;
        }

        static public TOFChannel operator -(TOFChannel t1, TOFChannel t2)
        {
            TOFChannel temp = new TOFChannel();
            temp.On = t1.On - t2.On;
            temp.Off = t1.Off - t2.Off;
            temp.Difference = t1.Difference - t2.Difference;
            return temp;
  
        }

        static public TOFChannel operator /(TOFChannel t, double d)
        {
            TOFChannel temp = new TOFChannel();
            temp.On = t.On / d;
            temp.Off = t.Off / d;
            temp.Difference = t.Difference / d;
            return temp;
        }

        static public TOFChannel operator *(TOFChannel t, double d)
        {
            TOFChannel temp = new TOFChannel();
            temp.On = t.On * d;
            temp.Off = t.Off * d;
            temp.Difference = t.Difference * d;
            return temp;
        }
    }
}
