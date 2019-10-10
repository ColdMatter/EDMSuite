using System;

namespace DAQ.HAL
{
    /// <summary>
    /// Represents a (GPIB) frequency counter
    /// </summary>
    public class FrequencyCounter : GPIBInstrument
    {
        public FrequencyCounter(String visaAddress)
            : base(visaAddress)
        { }

        public abstract double Frequency
        {
            get;
        }

        public abstract double Amplitude
        {
            get;
        }

        public void WriteToCounter(string command)
        {
            base.Write(command);
        }

        public string ReadFromCounter()
        {
            return base.Read();
        }
    }
}
