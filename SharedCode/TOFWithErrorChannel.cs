using System;
using System.Collections.Generic;
using System.Text;

using Data;

namespace Analysis.EDM
{
    [Serializable]
    public class TOFWithErrorChannel : Channel<TOFWithError>
    {
        public TOFWithErrorChannel()
        {
            this.On = new TOFWithError();
            this.Off = new TOFWithError();
            this.Difference = new TOFWithError();
        }

        public double DifferenceChiSquared
        {
            get
            {
                double d = 0;
                for (int i = 0; i < this.Difference.Length; i++)
                    d += Math.Pow(this.Difference.Data[i] / this.Difference.Errors[i], 2);
                return d / ((double)this.Difference.Length);
            }
        }
        static public TOFWithErrorChannel operator +(TOFWithErrorChannel t1, TOFWithErrorChannel t2)
        {
            TOFWithErrorChannel temp = new TOFWithErrorChannel();
            temp.On = t1.On + t2.On;
            temp.Off = t1.Off + t2.Off;
            temp.Difference = t1.Difference + t2.Difference;
            return temp;
        }

        static public TOFWithErrorChannel operator -(TOFWithErrorChannel t1, TOFWithErrorChannel t2)
        {
            TOFWithErrorChannel temp = new TOFWithErrorChannel();
            temp.On = t1.On - t2.On;
            temp.Off = t1.Off - t2.Off;
            temp.Difference = t1.Difference - t2.Difference;
            return temp;
  
        }

        static public TOFWithErrorChannel operator /(TOFWithErrorChannel t, double d)
        {
            TOFWithErrorChannel temp = new TOFWithErrorChannel();
            temp.On = t.On / d;
            temp.Off = t.Off / d;
            temp.Difference = t.Difference / d;
            return temp;
        }

        static public TOFWithErrorChannel operator /(double d, TOFWithErrorChannel t)
        {
            TOFWithErrorChannel temp = new TOFWithErrorChannel();
            temp.On = d / t.On;
            temp.Off = d / t.Off;
            temp.Difference = d / t.Difference;
            return temp;
        }

        static public TOFWithErrorChannel operator *(TOFWithErrorChannel t, double d)
        {
            TOFWithErrorChannel temp = new TOFWithErrorChannel();
            temp.On = t.On * d;
            temp.Off = t.Off * d;
            temp.Difference = t.Difference * d;
            return temp;
        }

        static public TOFWithErrorChannel operator /(TOFWithErrorChannel t1, TOFWithErrorChannel t2)
        {
            TOFWithErrorChannel temp = new TOFWithErrorChannel();
            temp.On = t1.On / t2.On;
            temp.Off = t1.Off / t2.Off;
            temp.Difference = t1.Difference / t2.Difference;
            return temp;
        }

        static public TOFWithErrorChannel operator *(TOFWithErrorChannel t1, TOFWithErrorChannel t2)
        {
            TOFWithErrorChannel temp = new TOFWithErrorChannel();
            temp.On = t1.On * t2.On;
            temp.Off = t1.Off * t2.Off;
            temp.Difference = t1.Difference * t2.Difference;
            return temp;
        }
    }
}
