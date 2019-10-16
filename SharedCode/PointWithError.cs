using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    /// <summary>
    /// This class represents a data point with an error
    /// </summary>
    [Serializable]
    public class PointWithError
    {
        public double Value;
        public double Error;

        static public PointWithError operator +(PointWithError p1, PointWithError p2)
        {
            PointWithError p = new PointWithError();
            p.Value = p1.Value + p2.Value;
            p.Error = Math.Sqrt(Math.Pow(p1.Error, 2) + Math.Pow(p2.Error, 2));

            return p;
        }

        static public PointWithError operator -(PointWithError p1, PointWithError p2)
        {
            PointWithError p = new PointWithError();
            p.Value = p1.Value - p2.Value;
            p.Error = Math.Sqrt(Math.Pow(p1.Error, 2) + Math.Pow(p2.Error, 2));

            return p;
        }

        static public PointWithError operator *(PointWithError p1, PointWithError p2)
        {
            PointWithError p = new PointWithError();
            p.Value = p1.Value * p2.Value;
            p.Error = p.Value * Math.Sqrt(Math.Pow(p1.Error / p1.Value, 2) + Math.Pow(p2.Error / p2.Value, 2));

            return p;
        }

        static public PointWithError operator *(PointWithError p1, double d)
        {
            PointWithError p = new PointWithError();
            p.Value = p1.Value * d;
            p.Error = p1.Error * d;

            return p;
        }

        static public PointWithError operator /(PointWithError p1, PointWithError p2)
        {
            PointWithError p = new PointWithError();
            p.Value = p1.Value / p2.Value;
            p.Error = p.Value * Math.Sqrt(Math.Pow(p1.Error / p1.Value, 2) + Math.Pow(p2.Error / p2.Value, 2));

            return p;
        }

        static public PointWithError operator /(PointWithError p1, double d)
        {
            PointWithError p = new PointWithError();
            p.Value = p1.Value / d;
            p.Error = p1.Error / d;

            return p;
        }
    }
}
