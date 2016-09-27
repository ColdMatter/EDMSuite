
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using DAQ.Environment;

namespace DAQ.HAL
{
    public class NovatechSerialPort
    {
        public class ddsChannel
        {
            private double channelFreq;
            private string channelName;
            public ddsChannel(string name)
            {
                this.channelName = name;
                this.channelFreq = (double)1;
            }
            public double Freq
            {
                get { return channelFreq; }
                set { channelFreq = value; }
            }
            public string Name
            {
                get { return channelName; }
                set { channelName = Name; }
            }
        }
        // A class containing methods to write the commands listed in the Novatech API to the serial port.

        public SerialPort novatechSerialPort = new SerialPort();
        private int counter;
        private System.Timers.Timer serialTimer;
        private Dictionary<string, ddsChannel> channels;
        private Dictionary<string, string> serialCommands;
        public Dictionary<string, string> errorMessages;
        public NovatechSerialPort()
        {
            channels = new Dictionary<string, ddsChannel>();
            channels["ddsCh1"] = new ddsChannel("F0");
            channels["ddsCh2"] = new ddsChannel("F1");
            channels["ddsCh3"] = new ddsChannel("F2");
            channels["ddsCh4"] = new ddsChannel("F3");

            serialCommands = new Dictionary<string, string>();

            errorMessages = new Dictionary<string, string>();
            errorMessages["OK"] = "Good command received";
            errorMessages["?0"] = "Unrecognized command";
            errorMessages["?1"] = "Bad frequency";
            errorMessages["?2"] = "Bad AM command";
            errorMessages["?3"] = "Input line too long";
            errorMessages["?4"] = "Bad Phase";
            errorMessages["?5"] = "Bad time";
            errorMessages["?6"] = "Bad Mode";
            errorMessages["?7"] = "Bad amp";
            errorMessages["?8"] = "Bad constant";
            errorMessages["?f"] = "Bad byte";


        }
        public void openNovatechPort(string portName, Int32 baudRate)
        {
            novatechSerialPort.PortName = portName;
            novatechSerialPort.BaudRate = 19200;
            novatechSerialPort.Open();

        }

        public void writeSerialCommandToBuffer(string serialCommand)
        {
            novatechSerialPort.WriteLine(serialCommand);
        }

        public void closeNovatechPort()
        {
            novatechSerialPort.Close();
        }

        public void updateChannelFrequenciesDictionary(Dictionary<string, double> newFrequencies)
        {
            foreach (var key in newFrequencies.Keys)
            {
                var value = newFrequencies[key];
                if (channels[key].Freq != value)
                {
                    channels[key].Freq = value;
                }
            }
            writeFreqenciesdictionaryToBuffer();
        }

        private void writeFreqenciesdictionaryToBuffer()
        {
            serialTimer = new System.Timers.Timer();
            serialTimer.Interval = 100;
            serialTimer.AutoReset = true;
            serialTimer.Elapsed += onElapsedEvent;
            serialTimer.Enabled = true;
            counter = 0;

        }
        private void onElapsedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            writeOneChannel();
        }

        private void writeOneChannel()
        {
            if (counter == channels.Count())
            {
                counter = 0;
                serialTimer.Dispose();
            }
            else
            {
                var key = channels.ElementAt(counter).Key;
                ddsChannel channel = channels[key];
                //novatechSerialPort.Write(channel.Name + " ");
                //byte[] freqByte = BitConverter.GetBytes(channel.Freq);
                //novatechSerialPort.Write(freqByte, 0, 0);
                novatechSerialPort.WriteLine(channel.Name + " " + channel.Freq.ToString());
                counter++;
            }
        }
        // private void castTo

        public Dictionary<string, ddsChannel> getFrequenciesDictionary()
        {
            return channels;
        }
    }
}