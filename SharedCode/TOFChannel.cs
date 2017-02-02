using System;
using System.Collections.Generic;
using System.Text;

using Data;

namespace Analysis.EDM
{
    [Serializable]
    public class TOFChannel : Channel<TOF>
    {
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

        static public TOFChannel operator /(double d, TOFChannel t)
        {
            TOFChannel temp = new TOFChannel();
            temp.On = d / t.On ;
            temp.Off = d / t.Off ;
            temp.Difference = d/ t.Difference;
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

        static public TOFChannel operator /(TOFChannel t1, TOFChannel t2)
        {
            TOFChannel temp = new TOFChannel();
            temp.On = t1.On / t2.On;
            temp.Off = t1.Off / t2.Off;
            temp.Difference = t1.Difference / t2.Difference;
            return temp;
        }

        static public TOFChannel operator *(TOFChannel t1, TOFChannel t2)
        {
            TOFChannel temp = new TOFChannel();
            temp.On = t1.On * t2.On;
            temp.Off = t1.Off * t2.Off;
            temp.Difference = t1.Difference * t2.Difference;
            return temp;
        }

        // makes a random TOFChannel, by making random TOFs
        public static TOFChannel Random()
        {
            TOFChannel tc = new TOFChannel();
            TOF on = TOF.Random();
            TOF off = TOF.Random();
            tc.On = on;
            tc.Off = off;
            tc.Difference = on - off;

            return tc;
        }
    }
}
