using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggeredShapedPulses
{
    public class ShapedPulse
    {
        private double a0;
        private double a1;
        private double a2;
        private double freq;
        private double phase;

        public ShapedPulse() { }

        public ShapedPulse(double a0, double a1, double a2, double freq, double phase)
        {
            this.a0 = a0;
            this.a1 = a1;
            this.a2 = a2;
            this.freq = freq;
            this.phase = phase;
        }

        public double Param0
        {
            get { return a0; }
            set { a0 = value; }
        }

        public double Param1
        {
            get { return a1; }
            set { a1 = value; }
        }

        public double Param2
        {
            get { return a2; }
            set { a2 = value; }
        }

        public double Freq
        {
            get { return freq; }
            set { freq = value; }
        }

        public double Phase
        {
            get { return phase; }
            set { phase = value; }
        }
    }
}
