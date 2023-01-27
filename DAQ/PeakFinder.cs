using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ
{
    public class PeakFinder
    {   

        public class PeakFinderStateMachine
        {
            public enum State
            {
                WAITING, ACQUIRING
            }

            private State state;

            public double lowthresh;
            public double highthresh;

            public PeakFinderStateMachine(double lthresh, double hthresh)
            {
                this.state = State.WAITING;
                this.lowthresh = lthresh;
                this.highthresh = hthresh;
            }

            public State advanceStateMachine(double datapoint)
            {
                if (this.state == State.WAITING && datapoint > this.highthresh)
                {
                    this.state = State.ACQUIRING;
                    return State.ACQUIRING;
                }

                if (this.state == State.ACQUIRING && datapoint < this.lowthresh)
                {
                    this.state = State.WAITING;
                }

                return this.state;
            }

        }

        private static double hthresh = 0.05;
        private static double lthresh = 0.05;

        public static void SetThresholds(double low, double high)
        {
            hthresh = high;
            lthresh = low;
        }
        public struct DataPoint
        {
            public double x;
            public double y;

            public DataPoint(double _x, double _y)
            {
                x = _x;
                y = _y;
            }
        }

        public static double peakLocator(List<DataPoint> data)
        {
            List<DataPoint> processed = new List<DataPoint> { };
            for (int i = 2; i < data.Count() - 2; ++i)
            {
                processed.Add(new DataPoint(data[i].x,(-2*data[i-2].y - data[i-1].y + data[i+1].y + data[i+2].y)/10));
            }


            int low = 0;
            int high = processed.Count() - 1;
            while (high - low > 1)
            {
                int mid = (high + low) / 2;
                if (data[mid].y > 0)
                {
                    low = mid;
                }
            }

            return processed[low].x -processed[low].y * (processed[high].x - processed[low].x) * (processed[high].y - processed[low].y);

        }

        public static double findPeak(double[] xdata, double[] data)
        {

            PeakFinderStateMachine sm = new PeakFinderStateMachine(hthresh, lthresh);

            List<double> peaks = new List<double> { };
            List<DataPoint> cachedData = new List<DataPoint> { };

            for (int i = 0; i < data.GetLength(0); ++i)
            {
                PeakFinderStateMachine.State state = sm.advanceStateMachine(data[i]);
                switch (state) {
                    case PeakFinderStateMachine.State.ACQUIRING:
                        cachedData.Add(new DataPoint(xdata[i], data[i]));
                        break;
                    case PeakFinderStateMachine.State.WAITING:
                        if (cachedData.Count() >= 6)
                        {
                            return peakLocator(cachedData);
                        }
                        break;
                }

            }

            return 0; // Probably a better err handling is preferred. This is non destructive so it's used

        }

    }
}
