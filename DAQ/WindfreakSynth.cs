using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.VisaNS;

namespace DAQ.HAL
{
    public class WindfreakSynth : RS232Instrument
    {

        public static class CommandTypes
        {
            public static String Frequency { get { return "f"; } }
            public static String Amplitude { get { return "W"; } }
            public static String Lookup { get { return "L"; } }
            public static String PA { get { return "r"; } }
            public static String PLL { get { return "E"; } }
            public static String Trigger { get { return "w"; } }
            public static String Channel { get { return "C"; } }
            public static String RFPower { get { return "h"; } }
        }

        public enum TriggerMode
        {
            Continuous = 0,
            Pulse = 5
        }

        public class WindfreakChannel
        {
            protected double maxFreq = 13600.0;
            protected double minFreq = 54.0;
            protected double maxAmp = 20.0;
            protected double minAmp = -75.0;

            protected double frequency;
            protected double amplitude;
            protected bool channelName;
            protected bool rfOn;
            protected WindfreakSynth windfreak;

            public WindfreakChannel(WindfreakSynth windfreak, bool channelName)
            {
                this.channelName = channelName;
                this.windfreak = windfreak;
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

            protected void Write(string command)
            {
                string channelChangeCommand = CommandTypes.Channel + Convert.ToInt32(channelName).ToString();
                windfreak.Write(channelChangeCommand + command);
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

            public void SetFrequency(double freq)
            {
                Write(FreqCommand(freq));
                frequency = freq;
            }

            public double GetFrequency()
            {
                return frequency;
            }

            public void SetAmplitude(double amp)
            {
                Write(AmpCommand(amp));
                amplitude = amp;
            }

            public double GetAmplitude()
            {
                return amplitude;
            }

            public void SetRF(bool state)
            {
                string stateString = Convert.ToInt32(state).ToString();
                Write(CommandTypes.PLL + stateString + CommandTypes.PA + stateString);
                rfOn = state;
            }

            public bool RFOn()
            {
                return rfOn;
            }

            public void ResetOutput(bool output)
            {
                Write(CommandTypes.RFPower + Convert.ToInt32(output).ToString());
            }

            public void SyncSettings(string[] settings)
            {
                frequency = Convert.ToDouble(settings[0]);
                amplitude = Convert.ToDouble(settings[1]);
                rfOn = Convert.ToBoolean(Convert.ToInt32(settings[2]));
            }
        }

        public WindfreakChannel ChannelA;
        public WindfreakChannel ChannelB;

        protected new int BaudRate = 115200; // Device can accept higher data transfer rate than default for class of 9600
        
        protected TriggerMode triggerMode;

        public WindfreakSynth(string address) : base(address)
        {
            ChannelA = new WindfreakChannel(this, false);
            ChannelB = new WindfreakChannel(this, true);
        }

        public WindfreakChannel Channel(bool channel)
        {
            return channel ? ChannelB : ChannelA;
        }

        public void SetTriggerMode(TriggerMode trigger)
        {
            Write(CommandTypes.Trigger + trigger.ToString("d"));
            // For some reason, it is necessary to make sure the output is on (or off for pulsed operation) after changing the trigger mode
            if (trigger == TriggerMode.Pulse)
            {
                ChannelA.ResetOutput(false);
                ChannelB.ResetOutput(false);
            }
            else
            {
                ChannelA.ResetOutput(true);
                ChannelB.ResetOutput(true);
            }
            triggerMode = trigger;
        }

        public TriggerMode GetTriggerMode()
        {
            return triggerMode;
        }

        protected new string Query(string command)
        {
            if (!connected) Connect(SerialTerminationMethod.TerminationCharacter);
            string response = base.Query(command);
            Disconnect();
            return response;
        }

        public void ReadSettingsFromDevice()
        {
            string[] deviceCommands = new string[] { CommandTypes.Trigger };
            string deviceQueries = string.Concat(deviceCommands.Select(command => command + "?").ToArray());
            string[] channelCommands = new string[] { CommandTypes.Frequency, CommandTypes.Amplitude, CommandTypes.PLL };
            string channelQueries = string.Concat(channelCommands.Select(command => command + "?").ToArray());

            Connect(SerialTerminationMethod.TerminationCharacter);
            Write(deviceQueries + CommandTypes.Channel + "0" + channelQueries + CommandTypes.Channel + "1" + channelQueries, true);

            int numberCommands = deviceCommands.Length + 2 * channelCommands.Length;
            string[] responses = new string[numberCommands];
            for (int i = 0; i < numberCommands; i++)
            {
                string response = Read();
                responses[i] = response.Substring(0, response.Length - 1);
            }

            bool[] channels = new bool[2] {false, true};
            triggerMode = (TriggerMode)Convert.ToInt32(responses[0]);
            ChannelA.SyncSettings(responses.Skip(1).Take(3).ToArray());
            ChannelB.SyncSettings(responses.Skip(4).Take(3).ToArray());
        }

    }
}
