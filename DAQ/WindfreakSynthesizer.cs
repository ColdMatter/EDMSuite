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

        public WindfreakSynthesizer(String address)
        {
            this.address = address;
        }

        public static class CommandTypes
        {
            public static String Frequency { get { return "f"; } }
            public static String Amplitude { get { return "a"; } }
            public static String Lookup { get { return "L"; } }
            public static String PA { get { return "r"; } }
            public static String PLL { get { return "E"; } }
            public static String Trigger { get { return "w"; } }
            public static String Channel { get { return "C"; } }
        }

        public enum TriggerModes
        {
            Continuous = 0,
            Pulse = 5
        }

        public void UpdateContinuousSettings(double freq, double amp)
        {
            SendCommand(FreqCommand(freq) + AmpCommand(amp));
        }

        public void UpdateTriggerMode(TriggerModes trigger)
        {
            SendCommand(CommandTypes.Trigger + trigger);
        }

        public void SetOutput(bool state)
        {
            string stateString = Convert.ToInt32(state).ToString();
            SendCommand(CommandTypes.PLL + stateString + CommandTypes.PA + stateString);
        }

        public void SetChannel(bool channel)
        {
            string channelString = Convert.ToInt32(channel).ToString();
            SendCommand(CommandTypes.Channel + channelString);
        }

        #region Helper methods

        protected override void Connect()
        {
            serial = new SerialSession(address);
            serial.BaudRate = 115200;
            serial.DataBits = 8;
            serial.StopBits = StopBitType.One;
            serial.ReadTermination = SerialTerminationMethod.LastBit;
            serial.Parity = Parity.None;
            serial.FlowControl = FlowControlTypes.None;
        }

        protected override void Disconnect()
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
            return CommandTypes.Frequency + freq.ToString("F7"); // Round to 7 decimal places
        }

        protected string AmpCommand(double amp)
        {
            ValidateAmplitude(amp);
            return CommandTypes.Amplitude + amp.ToString("F2"); // Round to 2 decimal places
        }

        #endregion
    }
    
}
