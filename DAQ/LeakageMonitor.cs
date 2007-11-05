using System;
using System.Threading;
using NationalInstruments.DAQmx;
using NationalInstruments.VisaNS;
using DAQ.Environment;
using DAQ.HAL;

namespace DAQ.HAL
{
    /* Measures the value from a HV leakage monitor. The leakage monitor outputs a train of pulses whose
     * frequency depends on the current flowing through it. This class uses a counter (well, pair of
     * counters to measure that frequency. The class provides a re-settable zero offset as the 
     * leakage monitors are prone to drifiting.
     */
    public class LeakageMonitor
    {
        private Random rn;
        private CounterChannel currentLeakageCounterChannel;
        private Task counterTask;
        private CounterReader leakageReader;

        // calibration constants
        public double Slope
        {
            get
            {
                return slope;
            }
            set
            {
                slope = value;
            }
        }
        public double Offset
        {
            get
            {
                return offset;
            }
            set
            {
                offset = value;
            }
        }
        public double MeasurementTime
        {
            get
            {
                return measurementTime;
            }
            set
            {
                measurementTime = value;
                setMeasurementTime();
            }
        }


        private double slope;
        private double offset;
        private double measurementTime;

        public LeakageMonitor(CounterChannel clChannel, double slope, double offset, double measurementTime)
        {
            currentLeakageCounterChannel = clChannel;
            this.slope = slope;
            this.offset = offset;
            this.measurementTime = measurementTime;
            rn = new Random((int)DateTime.Now.Ticks);
        }

        public void Initialize()
        {
            counterTask = new Task("");
            this.counterTask.CIChannels.CreateFrequencyChannel(
                currentLeakageCounterChannel.PhysicalChannel,
                "",
                0,
                150000,
                CIFrequencyStartingEdge.Rising,
                CIFrequencyMeasurementMethod.HighFrequencyTwoCounter,
                // the units of measurement time are not specified anywhere in the docs :-(
                measurementTime,
                // this has to be more than four to stop NIDAQ crashing, even though it is not used in this mode!
                100,
                CIFrequencyUnits.Hertz
                );
            counterTask.Stream.Timeout = (int)(1.5 * 1000 * measurementTime);
            leakageReader = new CounterReader(counterTask.Stream);
        }




        private double getRawCount()
        {
            double raw;
            if (!Environs.Debug)
            {
                try
                {
                    raw = leakageReader.ReadSingleSampleDouble();
                    //counterTask.Control(TaskAction.Unreserve);
                }
                catch (NationalInstruments.DAQmx.DaqException)
                {
                    raw = offset;
                }
            }
            else
            {
                raw = rn.NextDouble() * 5000;
            }
            return raw;
        }

        public double GetCurrent()
        {
            return ((getRawCount() - offset) / slope);
        }

        public void SetZero()
        {
            offset = getRawCount();
            return;
        }

        private void setMeasurementTime()
        {
            counterTask.Stream.Timeout = (int)(1.5 * 1000 * measurementTime);
            counterTask.CIChannels[0].FrequencyMeasurementTime = measurementTime;
        }

    }
}
