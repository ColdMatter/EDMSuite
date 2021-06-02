using System;
using System.Threading;
using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;

namespace DAQ.HAL
{
    /* Measures the value from a HV leakage monitor. The leakage monitor outputs a train of pulses whose
     * frequency depends on the current flowing through it. This class uses a counter (well, pair of
     * counters to measure that frequency. The class provides a re-settable zero offset as the 
     * leakage monitors are prone to drifting.
     */

    //This enum defines the analog or frequency counter versions of the leakage monitor so both can be used
    public enum LeakageMonitorType { ANALOG, COUNTER};

    public class LeakageMonitor
    {
        private Random rn;
        private CounterChannel currentLeakageCounterChannel;
        private Task counterTask;
        private CounterReader counterLeakageReader;
        private Task monitorTask;
        private AnalogSingleChannelReader analogLeakageReader;
        private string leakageChannel;
        private LeakageMonitorType leakageMonitorType;
        

        // calibration constants
        public double F2ISlope
        {
            get
            {
                return f2iSlope;
            }
            set
            {
                f2iSlope = value;
            }
        }
        public double V2FSlope
        {
            get
            {
                return v2fSlope;
            }
            set
            {
                v2fSlope = value;
            }
        }
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
                if (value != measurementTime)
                {
                    measurementTime = value;
                    //setMeasurementTime();
                }
            }
        }


        private double f2iSlope;
        private double v2fSlope;
        private double offset;
        private double measurementTime;
        private double slope;

        public LeakageMonitor(CounterChannel clChannel, double slope, double offset, double measurementTime)
        {
            currentLeakageCounterChannel = clChannel;
            this.slope = slope;
            this.offset = offset;
            this.measurementTime = measurementTime;
            rn = new Random((int)DateTime.Now.Ticks);
            this.leakageMonitorType = LeakageMonitorType.COUNTER;
        }

        public LeakageMonitor(string channel, double volt2freqSlope, double freq2ampSlope, double offset)
        {
            this.v2fSlope = volt2freqSlope;
            this.f2iSlope = freq2ampSlope;
            this.offset = offset;
            this.leakageChannel = channel;
            rn = new Random((int)DateTime.Now.Ticks);
            this.leakageMonitorType = LeakageMonitorType.ANALOG;
        }

        public void Initialize()
        {
            if (!Environs.Debug)
            {
                if (leakageMonitorType.Equals(LeakageMonitorType.ANALOG))
                {
                    monitorTask = new Task("EDMHCIn" + leakageChannel);
                    ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[leakageChannel]).AddToTask(
                        monitorTask,
                        0,
                        10
                    );
                    monitorTask.Control(TaskAction.Verify);
                    analogLeakageReader = new AnalogSingleChannelReader(monitorTask.Stream);
                }
                else
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
                    counterTask.Stream.Timeout = (int)(10.1 * 1000 * measurementTime);
                    counterLeakageReader = new CounterReader(counterTask.Stream);
                }
            }


        }


        public double getRawCount()
        {
            double raw;
            if (!Environs.Debug)
            {
                try
                {
                    if (leakageMonitorType.Equals(LeakageMonitorType.ANALOG))
                    {
                        raw = analogLeakageReader.ReadSingleSample();
                        monitorTask.Control(TaskAction.Unreserve);
                    } else
                    {
                        raw = counterLeakageReader.ReadSingleSampleDouble();
                        counterTask.Control(TaskAction.Unreserve);
                    }

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
            if (leakageMonitorType.Equals(LeakageMonitorType.ANALOG))
            {
                return (getRawCount() - offset) / (v2fSlope * f2iSlope);
            } else
            {
                return ((getRawCount() - offset) / slope);
            }
        }

        public void SetZero()
        {
            offset = getRawCount();
            return;
        }

        //private void setMeasurementTime()
        //{
        //    counterTask.Stream.Timeout = (int)(1.1 * 1000 * measurementTime);
        //    counterTask.CIChannels[0].FrequencyMeasurementTime = measurementTime;
        //}

    }
}
