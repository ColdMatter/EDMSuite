using System;
using System.Xml.Serialization;
using System.Linq;

namespace Data
{
    /// <summary>
    /// A TOF is the result of a single detector looking at a single pulse from the source.
    /// </summary>
    [Serializable]
    public class TOF : MarshalByRefObject
    {
        private double[] tofData;
        //private int length;
        private int gateStartTime;
        private int clockPeriod;
        public double calibration;

        public TOF() { }

        public double Integrate(bool rmbg = false)
        {
            double[] tmpData;

            if (rmbg)
            {
                tmpData = new double[tofData.Count()];
                double bg = tofData.Take(25).Average();
                for (int i = 0; i <= tofData.Count() - 1; i++)
                {
                    tmpData[i] = tofData[i] - bg;
                }
            }
            else
            {
                tmpData = tofData;
            }

            double sum = 0;
            foreach (double sample in tmpData) sum += sample;
            return (sum * clockPeriod);
        }

        // This helper function takes a pair of gates and trims them to the width of the
        // captured data. It returns null if the gates don't make sense.
        private double[] TrimGates(double startTime, double endTime)
        {
            // check for swapped gates
            if (startTime > endTime) return null;
            // is the gate region null, or entirely outside the TOF?
            int gateEndTime = gateStartTime + (Length - 1) * clockPeriod;
            if (startTime == endTime) return null;
            if (startTime > gateEndTime) return null;
            if (endTime < gateStartTime) return null;
            // trim the gates
            if (endTime > gateEndTime) endTime = gateEndTime;
            if (startTime < gateStartTime) startTime = gateStartTime;
            // it's now possible for the trimmed gate region to be null (!) so check again
            if (startTime == endTime) return null;

            return new double[] { startTime, endTime };
        }

        // Integrate returns the area under a part of the TOF curve. It interpolates linearly
        // between sample points i.e. between each pair of sample times is a trapezium: this
        // function returns the area of these trapeziums that are between the gates. A picture
        // would really be better here!
        public double Integrate(double startTime, double endTime)
        {
            double[] trimmedGates = TrimGates(startTime, endTime);
            if (trimmedGates == null) return 0;
            startTime = trimmedGates[0];
            endTime = trimmedGates[1];

            return IntegrateInternal(startTime, endTime);
        }
        // This function actually does the integration. Broken out so that GatedMean can use it also.
        private double IntegrateInternal(double startTime, double endTime)
        {
            // calculate the the points that are included in the range, plus the point above and below
            double p = (startTime - (double)gateStartTime) / (double)clockPeriod;
            double q = (endTime - (double)gateStartTime) / (double)clockPeriod;
            int lowest = (int)Math.Floor(p);
            int highest = (int)Math.Ceiling(q);

            // sum over all trapeziums included in the gate range, even those partially included
            double sum = 0.0;
            for (int i = lowest; i < highest; i++) sum += ((double)clockPeriod * 0.5) * (tofData[i] + tofData[i + 1]);

            // correct the first and last trapeziums which may not be fully included
            sum -= ((2 * tofData[lowest]) + (tofData[lowest + 1] - tofData[lowest])
                            * (p - Math.Floor(p))) * ((double)clockPeriod * 0.5 * (p - Math.Floor(p)));
            sum -= ((2 * tofData[highest]) - (tofData[highest] - tofData[highest - 1])
                            * (Math.Ceiling(q) - q)) * ((double)clockPeriod * 0.5 * (Math.Ceiling(q) - q));

            return sum;
        }

        // this is the old integrate method that did no interpolation.
        //public double IntegrateOld(double startTime, double endTime)
        //{
        //    int low = (int)Math.Ceiling((startTime - gateStartTime) / clockPeriod);
        //    int high = (int)Math.Floor((endTime - gateStartTime) / clockPeriod);

        //    // check the range is sensible
        //    if (low < 0) low = 0;
        //    if (high > length - 1) high = length - 1;
        //    if (low > high) return 0;

        //    double sum = 0;
        //    for (int i = low; i <= high; i++) sum += tofData[i];
        //    return (sum * clockPeriod);
        //}

        public double Mean
        {
            get
            {
                double tmp = 0;
                for (int i = 0; i < Data.Length; i++) tmp += Data[i];
                return tmp / Data.Length;
            }
        }

        public double GatedMean(double startTime, double endTime)
        {
            double[] trimmedGates = TrimGates(startTime, endTime);
            if (trimmedGates == null) return 0;
            startTime = trimmedGates[0];
            endTime = trimmedGates[1];

            return IntegrateInternal(startTime, endTime) / (endTime - startTime);
        }

        public static TOF operator +(TOF p1, TOF p2)
        {
            if (p1.ClockPeriod == p2.ClockPeriod && p1.GateStartTime == p2.GateStartTime
                && p1.Length == p2.Length)
            {
                double[] tempData = new double[p1.Length];
                for (int i = 0; i < p1.Length; i++)
                {
                    tempData[i] = p1.Data[i] + p2.Data[i];
                }
                TOF temp = new TOF();
                temp.Data = tempData;
                temp.GateStartTime = p1.GateStartTime;
                temp.ClockPeriod = p1.ClockPeriod;
                temp.Calibration = p1.Calibration;
                return temp;
            }
            else
            {
                if (p1.Length == 0) return p2;
                if (p2.Length == 0) return p1;
                return null;
            }
        }

        public static TOF operator -(TOF p1, TOF p2)
        {
            TOF temp = new TOF();
            temp.Data = new double[p2.Data.Length];
            temp.GateStartTime = p1.GateStartTime;
            temp.ClockPeriod = p1.ClockPeriod;
            temp.Calibration = p1.Calibration;

            for (int i = 0; i < p2.Data.Length; i++)
            {
                temp.Data[i] = -p2.Data[i];
            }
            return p1 + temp;
        }

        static public TOF operator *(TOF t, double d)
        {
            TOF temp = new TOF();
            temp.Data = new double[t.Data.Length];
            temp.GateStartTime = t.GateStartTime;
            temp.ClockPeriod = t.ClockPeriod;
            temp.Calibration = t.Calibration;

            for (int i = 0; i < t.Data.Length; i++)
            {
                temp.Data[i] = d * t.Data[i];
            }
            return temp;
        }

        public static TOF operator /(TOF p, double d)
        {
            double[] tempData = new double[p.Length];
            for (int i = 0; i < p.Length; i++)
            {
                tempData[i] = p.Data[i] / d;
            }
            TOF temp = new TOF();
            temp.Data = tempData;
            temp.GateStartTime = p.GateStartTime;
            temp.ClockPeriod = p.ClockPeriod;
            temp.Calibration = p.Calibration;
            return temp;
        }

        public static TOF operator /(double d,TOF p)
        {
            double[] tempData = new double[p.Length];
            for (int i = 0; i < p.Length; i++)
            {
                tempData[i] = d/p.Data[i];
            }
            TOF temp = new TOF();
            temp.Data = tempData;
            temp.GateStartTime = p.GateStartTime;
            temp.ClockPeriod = p.ClockPeriod;
            temp.Calibration = p.Calibration;
            return temp;
        }


        static public TOF operator /(TOF t1, TOF t2)
        {
            TOF temp = new TOF();
            temp.Data = new double[t1.Data.Length];
            temp.GateStartTime = t1.GateStartTime;
            temp.ClockPeriod = t1.ClockPeriod;
            temp.Calibration = t1.Calibration;

            for (int i = 0; i < t1.Data.Length; i++)
            {
                temp.Data[i] = t1.Data[i] / t2.Data[i];
            }
            return temp;
        }

        static public TOF operator *(TOF t1, TOF t2)
        {
            TOF temp = new TOF();
            temp.Data = new double[t1.Data.Length];
            temp.GateStartTime = t1.GateStartTime;
            temp.ClockPeriod = t1.ClockPeriod;

            for (int i = 0; i < t1.Data.Length; i++)
            {
                temp.Data[i] = t1.Data[i] * t2.Data[i];
            }
            return temp;
        }


        [XmlArrayItem("s")]
        public double[] Data
        {
            get { return tofData; }
            set
            {
                tofData = value;
                //                length = value.Length;
            }
        }

        public int Length
        {
            get
            {
                if (tofData != null) return tofData.Length;
                else return 0;
            }
        }

        public int GateStartTime
        {
            get { return gateStartTime; }
            set { gateStartTime = value; }
        }

        public int GateLength
        {
            get { return Length * clockPeriod; }
        }

        public int ClockPeriod
        {
            get { return clockPeriod; }
            set { clockPeriod = value; }
        }

        public double Calibration
        {
            get { return calibration; }
            set { calibration = value; }
        }

        public int[] Times
        {
            get
            {
                int[] times = new int[Length];
                for (int i = 0; i < Length; i++) times[i] = gateStartTime + (i * clockPeriod);
                return times;
            }
        }

        // returns a typically sized TOF with random data
        private const int RANDOM_TOF_SIZE = 80;
        private static Random r = new Random();

        public static TOF Random()
        {
            TOF t = new TOF();
            t.Data = new double[RANDOM_TOF_SIZE];
            for (int i = 0; i < RANDOM_TOF_SIZE; i++) t.Data[i] = r.NextDouble();
            t.Calibration = 1.0;
            t.ClockPeriod = 10;
            t.GateStartTime = 1800;
            return t;
        }

        // helper to make a TOF from a single data point
        public TOF(double d)
        {
            this.Data = new double[1];
            this.Data[0] = d;
            this.ClockPeriod = 1;
            this.gateStartTime = 0;
            this.Calibration = 1.0;
        }

    }
}
