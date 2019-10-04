//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Xml.Serialization;

//namespace Data
//{
//    /// <summary>
//    /// This class represents a time of flight curve with an associated error at every point.
//    /// </summary>
//    [Serializable]
//    public class TOFWithError : TOF
//    {
//        [XmlArrayItem("e")]
//        public double[] Errors;

//        public TOFWithError() { }

//        public TOFWithError(TOF t)
//        {
//            this.Data = t.Data;
//            this.Errors = new double[Length];
//            this.Calibration = t.Calibration;
//            this.GateStartTime = t.GateStartTime;
//            this.ClockPeriod = t.ClockPeriod;
//        }

//        // this method adds two TOFWithErrors together. It combines the errors using the
//        // usual addition of errors formula.
//        static public TOFWithError operator +(TOFWithError t1, TOFWithError t2)
//        {
//            if (t1.ClockPeriod == t2.ClockPeriod && t1.GateStartTime == t2.GateStartTime
//               && t1.Length == t2.Length)
//            {
//                TOFWithError temp = new TOFWithError();
//                temp.Calibration = t1.Calibration;
//                temp.ClockPeriod = t1.ClockPeriod;
//                temp.GateStartTime = t1.GateStartTime;

//                temp.Data = new double[t1.Length];
//                temp.Errors = new double[t1.Length];
//                for (int i = 0; i < t1.Length; i++)
//                {
//                    temp.Data[i] = t1.Data[i] + t2.Data[i];
//                    temp.Errors[i] = Math.Sqrt(Math.Pow(t1.Errors[i], 2) + Math.Pow(t2.Errors[i], 2));
//                }
//                return temp;
//            }
//            else return null;
//        }

//        // this gives the difference of two TOFWithErrors, respecting the errors.
//        static public TOFWithError operator -(TOFWithError t1, TOFWithError t2)
//        {
//            // this funny construction lets us use the multiplication code to subtract
//            return t1 + (t2 * -1.0);
//        }

//        static public TOFWithError operator /(TOFWithError t, double d)
//        {
//            TOFWithError temp = new TOFWithError();
//            temp.Data = new double[t.Data.Length];
//            temp.Errors = new double[t.Errors.Length];
//            temp.GateStartTime = t.GateStartTime;
//            temp.ClockPeriod = t.ClockPeriod;
//            temp.Calibration = t.Calibration;

//            for (int i = 0; i < t.Data.Length; i++)
//            {
//                temp.Data[i] = t.Data[i] / d;
//                temp.Errors[i] = t.Errors[i] / Math.Abs(d);
//            }
//            return temp;
//        }

//        static public TOFWithError operator *(TOFWithError t, double d)
//        {
//            TOFWithError temp = new TOFWithError();
//            temp.Data = new double[t.Data.Length];
//            temp.Errors = new double[t.Errors.Length];
//            temp.GateStartTime = t.GateStartTime;
//            temp.ClockPeriod = t.ClockPeriod;
//            temp.Calibration = t.Calibration;

//            for (int i = 0; i < t.Data.Length; i++)
//            {
//                temp.Data[i] = d * t.Data[i];
//                temp.Errors[i] = Math.Abs(d) * t.Errors[i];
//            }
//            return temp;
//        }

//        static public TOFWithError operator /(TOFWithError t1, TOFWithError t2)
//        {
//            TOFWithError temp = new TOFWithError();
//            temp.Data = new double[t1.Data.Length];
//            temp.Errors = new double[t1.Errors.Length];
//            temp.GateStartTime = t1.GateStartTime;
//            temp.ClockPeriod = t1.ClockPeriod;
//            temp.Calibration = t1.Calibration;

//            for (int i = 0; i < t1.Data.Length; i++)
//            {
//                temp.Data[i] = t1.Data[i] / t2.Data[i];
//                temp.Errors[i] = temp.Data[i] * Math.Sqrt(
//                    Math.Pow(t1.Errors[i] / t1.Data[i], 2) +
//                    Math.Pow(t2.Errors[i] / t2.Data[i], 2)
//                    );
//            }
//            return temp;
//        }

//        static public TOFWithError operator *(TOFWithError t1, TOFWithError t2)
//        {
//            TOFWithError temp = new TOFWithError();
//            temp.Data = new double[t1.Data.Length];
//            temp.Errors = new double[t1.Errors.Length];
//            temp.GateStartTime = t1.GateStartTime;
//            temp.ClockPeriod = t1.ClockPeriod;

//            for (int i = 0; i < t1.Data.Length; i++)
//            {
//                temp.Data[i] = t1.Data[i] * t2.Data[i];
//                temp.Errors[i] = temp.Data[i] * Math.Sqrt(
//                    Math.Pow(t1.Errors[i]/t1.Data[i], 2) +
//                    Math.Pow(t2.Errors[i]/t2.Data[i], 2)
//                    );
//            }
//            return temp;
//        }
//    }
//}
