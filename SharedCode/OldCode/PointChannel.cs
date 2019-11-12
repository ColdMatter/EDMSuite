//using System;
//using System.Collections.Generic;
//using System.Text;

//using Data;

//namespace Analysis.EDM
//{
//    [Serializable]
//    public class PointChannel : Channel<PointWithError>
//    {
//        public PointChannel()
//        {
//            this.On = new PointWithError();
//            this.Off = new PointWithError();
//            this.Difference = new PointWithError();
//        }
//        static public PointChannel operator +(PointChannel t1, PointChannel t2)
//        {
//            PointChannel temp = new PointChannel();
//            temp.On = t1.On + t2.On;
//            temp.Off = t1.Off + t2.Off;
//            temp.Difference = t1.Difference + t2.Difference;
//            return temp;
//        }

//        static public PointChannel operator -(PointChannel t1, PointChannel t2)
//        {
//            PointChannel temp = new PointChannel();
//            temp.On = t1.On - t2.On;
//            temp.Off = t1.Off - t2.Off;
//            temp.Difference = t1.Difference - t2.Difference;
//            return temp;
  
//        }

//        static public PointChannel operator /(PointChannel t, double d)
//        {
//            PointChannel temp = new PointChannel();
//            temp.On = t.On / d;
//            temp.Off = t.Off / d;
//            temp.Difference = t.Difference / d;
//            return temp;
//        }

//        static public PointChannel operator *(PointChannel t, double d)
//        {
//            PointChannel temp = new PointChannel();
//            temp.On = t.On * d;
//            temp.Off = t.Off * d;
//            temp.Difference = t.Difference * d;
//            return temp;
//        }

//        static public PointChannel operator /(PointChannel t1, PointChannel t2)
//        {
//            PointChannel temp = new PointChannel();
//            temp.On = t1.On / t2.On;
//            temp.Off = t1.Off / t2.Off;
//            temp.Difference = t1.Difference / t2.Difference;
//            return temp;
//        }

//        static public PointChannel operator *(PointChannel t1, PointChannel t2)
//        {
//            PointChannel temp = new PointChannel();
//            temp.On = t1.On * t2.On;
//            temp.Off = t1.Off * t2.Off;
//            temp.Difference = t1.Difference * t2.Difference;
//            return temp;
//        }
//    }
//}
