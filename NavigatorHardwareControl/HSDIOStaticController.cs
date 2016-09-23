using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.HAL;
using NationalInstruments.ModularInstruments.Interop;

namespace NavigatorHardwareControl
{
    public class HSDIOStaticChannelController
    {
        /// <summary>
        /// Rather than create a task for each channel, as with the DAQ devices, 
        /// we create a static generator that writes to every channel, but only 
        /// allows the "created" channels to be accessed publically.
        /// </summary>
        niHSDIO my_niHSDIO;
        string deviceName;
        string channelList;

        Dictionary<string, int> HSDigitalChannels;
        bool[] ChannelValues;

        public HSDIOStaticChannelController(string deviceName, string channelList)
        {
            this.deviceName = deviceName;
            this.channelList = channelList;

                ReclaimHardware();
               
   


            HSDigitalChannels = new Dictionary<string, int>();

            ChannelValues = new bool[32];
            for (int i = 0; i < 32; i++)
            {
                ChannelValues[i] = false;
            }
            }

        public void CreateHSDigitalTask(string name, int channelNumber)
        {
            HSDigitalChannels.Add(name, channelNumber);
        }

        public void SetHSDigitalLine(string name, bool value)
        {
            ChannelValues[HSDigitalChannels[name]] = value;
            WriteOut();
        }

        private void WriteOut()
        {
            uint valOut = 0;

            for (int i = 0; i < 32; i++)
            {
                valOut += (uint)(Convert.ToInt32(ChannelValues[i]) * Math.Pow(2, i));
            }

            my_niHSDIO.WriteStaticU32(valOut, (uint)(Math.Pow(2, 32) - 1));
        }

        public void ReleaseHardware()
        {
            my_niHSDIO.Dispose();
          
        }

        public void ReclaimHardware()
        {
            my_niHSDIO = niHSDIO.InitGenerationSession(deviceName, true, false, "");
            my_niHSDIO.AssignStaticChannels(channelList);
        }
    }
}