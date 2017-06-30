using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.VisaNS;

namespace DAQ.HAL
{
    /// <summary>
    /// Controls the microwave synthesizer from Windfreak
    /// 
    /// </summary>
    public class WindfreakSynthesizer : DAQ.HAL.Instrument
    {
        SerialSession serial;
        String address;

        // Parameters limitations
        Double maxFreq = 13600.0;
        Double minFreq = 54.0;
        Double maxAmp = 20.0;
        Double minAmp = -75.0;
        Int32 maxSteps = 100;

        // Command List
        String freqKey = "f";
        String ampKey = "a";
        String lookUpKey = "L";
        String paKey = "r";
        String pllKey = "E";


        public WindfreakSynthesizer(String address)
        {
            this.address = address;
        }

        public enum TriggerTypes
        {
            Software = 0,
            Sweep = 1,
            Step = 2
        }

        public static class CommandTypes
        {
            public static String Frequency { get { return "f"; } }
            public static String Amplitude { get { return "a"; } }
            public static String Lookup { get { return "L"; } }
            public static String PA { get { return "r"; } }
            public static String PLL { get { return "E"; } }
        }

        public override void Connect()
        {
            serial = new SerialSession(address);
            serial.BaudRate = 115200;
            serial.DataBits = 8;
            serial.StopBits = StopBitType.One;
            serial.ReadTermination = SerialTerminationMethod.LastBit;
            serial.Parity = Parity.None;
            serial.FlowControl = FlowControlTypes.None;
        }

        public override void Disconnect()
        {
            serial.Dispose();
        }

        protected override void Write(String command)
        {
            serial.Write(command);
        }

        protected override string Read()
        {
            return serial.ReadString();
        }

        public void UpdateContinuousSettings(double freq, double amp)
        {
            SendCommand(FreqCommand(freq) + AmpCommand(amp));
        }

        public void SetOutput(bool state)
        {
            string stateString = Convert.ToInt32(state).ToString();
            SendCommand(pllKey + state + paKey + state);
        }

        #region Helper methods

        protected void SendCommand(string command)
        {
            Connect();
            Write(command);
            Disconnect();
        }

        protected void ValidateEntry(double value, double maxValue, double minValue, string name, string unit)
        {
            if (value > maxValue || value < minValue)
            {
                throw new System.ArgumentException(
                    value.ToString() + " is not a valid value for " + name + ". Value must lie between "
                    + minValue.ToString() + "-" + maxValue.ToString() + " " + unit
                );
            }
        }

        protected void ValidateFrequency(double freq)
        {
            ValidateEntry(freq, maxFreq, minFreq, "Frequency", "MHz");
        }

        protected void ValidateAmplitude(double amp)
        {
            ValidateEntry(amp, maxAmp, minAmp, "Amplitude", "dBm");
        }

        protected string FreqCommand(double freq)
        {
            ValidateFrequency(freq);
            return freqKey + freq.ToString("F7"); // Round to 7 decimal places
        }

        protected string AmpCommand(double amp)
        {
            ValidateAmplitude(amp);
            return ampKey + amp.ToString("F2"); // Round to 2 decimal places
        }

        protected double GreatestCommonDivisor(double a, double b)
        {
            while (a !=0 && b!= 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }
            if (a == 0)
                return b;
            else
                return a;
        }

        protected double GreatestCommonDivisor(double[] nums)
        {
            return nums.Aggregate(GreatestCommonDivisor);
        }

        protected string BuildPatternStep(int step, double freq, double amp)
        {
            ValidateFrequency(freq);
            ValidateAmplitude(amp);
            string stepString = step.ToString();
            return lookUpKey + stepString + FreqCommand(freq) + lookUpKey + stepString + AmpCommand(amp);
        }

        protected string createPatternFromEventSequence(double[,] eventSequence)
        {
            int numEvents = eventSequence.GetLength(0);
            double patternLength = eventSequence[numEvents - 1, 0];

            double[] eventTimes = new double[numEvents];
            for (int i = 0; i < numEvents; i++)
            {
                eventTimes[i] = eventSequence[i, 1];
            }
            double stepSize = GreatestCommonDivisor(eventTimes);
            int numSteps = (int)(patternLength / stepSize) + 1;

            if (numSteps > maxSteps)
            {
                throw new System.ArgumentException("Events list cannot be converted to 100 step pattern.", "events");
            }
            if (eventTimes.Distinct().Count() != eventTimes.Count())
            {
                throw new System.ArgumentException("Events list has multiple entries for same time.", "events");
            }

            StringBuilder pattern = new StringBuilder();
            double freq = minFreq;
            double amp = minAmp;
            int eventNo = 0;
            for (int i = 0; i < numSteps; i++)
            {
                double time = i * stepSize;
                if (eventSequence[eventNo,0] == time)
                {
                    freq = eventSequence[eventNo, 1];
                    amp = eventSequence[eventNo, 2];
                    eventNo += 1;
                }
                pattern.Append(BuildPatternStep(i, freq, amp));
            }

            return "t" + stepSize.ToString() + "Ld" + pattern.ToString();
        }

        #endregion
    }
    
}
