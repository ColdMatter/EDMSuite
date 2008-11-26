using System;
using System.Xml.Serialization;

namespace Data
{
    /// <summary>
    /// A TOF is the result of a single detector looking at a single pulse from the source.
    /// </summary>
    [Serializable]
    public class TOF : MarshalByRefObject
    {
        private double[] tofData;
        private int length;
        private int gateStartTime;
        private int clockPeriod;
        public double calibration;

        public double Integrate()
        {
            double sum = 0;
            foreach (double sample in tofData) sum += sample;
            return (sum * clockPeriod);
        }

        // Integrate returns the area under a part of the TOF curve. It interpolates linearly
        // between sample points i.e. between each pair of sample times is a trapezium: this
        // function returns the area of these trapeziums that are between the gates. A picture
        // would really be better here!
        public double Integrate(double startTime, double endTime)
        {
            // check for swapped gates
            if (startTime > endTime) return 0;
            // is the gate region null, or entirely outside the TOF?
            int gateEndTime = gateStartTime + (length - 1) * clockPeriod;
            if (startTime == endTime) return 0;
            if (startTime > gateEndTime) return 0;
            if (endTime < gateStartTime) return 0;
            // trim the gates
            if (endTime > gateEndTime) endTime = gateEndTime;
            if (startTime < gateStartTime) startTime = gateStartTime;
            // it's now possible for the trimmed gate region to be null (!) so check again
            if (startTime == endTime) return 0;
 
            // calculate the the points that are included in the range, plus the point above and below
            double p = (startTime - (double)gateStartTime) / (double)clockPeriod;
            double q = (endTime - (double)gateStartTime) / (double)clockPeriod;
            int lowest = (int)Math.Floor(p);
            int highest = (int)Math.Ceiling(q);

            // this shouldn't happen
            //if (lowest < 0) lowest = 0;
            //if (highest > length - 1) highest = length - 1;

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
            int low = (int)Math.Ceiling((startTime - gateStartTime) / clockPeriod);
            int high = (int)Math.Floor((endTime - gateStartTime) / clockPeriod);

            // check the range is sensible
            if (low < 0) low = 0;
            if (high > length - 1) high = length - 1;
            // cheezy, temporary hack to kill off some infinity errors
            if (low > high) return 0.000001;

            // cheezy, temporary hack to kill off some infinity errors
            double sum = 0.000001;
            for (int i = low; i <= high; i++) sum += tofData[i];
            return (sum / (high - low + 1));
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
                return temp;
            }
            else
            {
                if (p1.Length == 0) return p2;
                if (p2.Length == 0) return p1;
                return null;
            }
        }

        public static TOF operator /(TOF p, int n)
        {
            double[] tempData = new double[p.Length];
            for (int i = 0; i < p.Length; i++)
            {
                tempData[i] = p.Data[i] / n;
            }
            TOF temp = new TOF();
            temp.Data = tempData;
            temp.GateStartTime = p.GateStartTime;
            temp.ClockPeriod = p.ClockPeriod;
            return temp;
        }


        [XmlArrayItem("s")]
        public double[] Data
        {
            get { return tofData; }
            set
            {
                tofData = value;
                length = value.Length;
            }
        }

        public int Length
        {
            get { return length; }
        }

        public int GateStartTime
        {
            get { return gateStartTime; }
            set { gateStartTime = value; }
        }

        public int GateLength
        {
            get { return length * clockPeriod; }
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
                int[] times = new int[length];
                for (int i = 0; i < length; i++) times[i] = gateStartTime + (i * clockPeriod);
                return times;
            }
        }


    }
}
