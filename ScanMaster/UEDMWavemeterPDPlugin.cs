using DAQ.Environment;
using DAQ.HAL;
using NationalInstruments.DAQmx;
using ScanMaster.Acquire.Plugin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net;
using System.Net.Sockets;
using System.Xml.Serialization;
using WavemeterLockServer;

namespace ScanMaster.Acquire.Plugins
{
    /// <summary>
    /// A plugin to capture wavemeter reading from WavemeterLock. Returns measured frequency-offset in GHz.
    /// </summary>
    [Serializable]
    public class UEDMWavemeterPDPlugin : AnalogInputPlugin
    {
        [NonSerialized]
        private double latestData;
        [NonSerialized]
        private double[] latestPD;
        [NonSerialized]
        private WavemeterLockServer.Controller wavemeterServerContrller;
        [NonSerialized]
        private string serverComputerName;
        [NonSerialized]
        private string ipAddr;
        [NonSerialized]
        UEDMHardwareControl.UEDMController hardwareController;
        //private string hostName = "IC-CZC136CFDJ";// (String)System.Environment.GetEnvironmentVariables()["IC-CZC136CFDJ"];

        protected override void InitialiseSettings()
        {
            settings["channel"] = 1;
            settings["computer"] = "IC-CZC136CFDJ";
            settings["offset"] = 0.0;//Frequency offset in THz

            settings["pdChannels"] = "1,2,3,4,5,6,7,8"; //8 photodiodes channels to read from the Hardware Controller
        }

        public override void AcquisitionStarting()
        {
            latestPD = new double[8];
            if (!Environs.Debug)
            {
                hardwareController = new UEDMHardwareControl.UEDMController();
                serverComputerName = (string)settings["computer"];

                /*foreach (var addr in Dns.GetHostEntry(serverComputerName).AddressList)
				{
					if (addr.AddressFamily == AddressFamily.InterNetwork)
						ipAddr = addr.ToString();
				}*/

                EnvironsHelper eHelper = new EnvironsHelper(serverComputerName);

                wavemeterServerContrller = (WavemeterLockServer.Controller)(Activator.GetObject(typeof(WavemeterLockServer.Controller), "tcp://" + Dns.GetHostByName(serverComputerName).AddressList[0].ToString() + ":" + eHelper.serverTCPChannel + "/controller.rem"));
            }

        }

        private int[] ParsePDChannels()
        {
            string s = (string)settings["pdChannels"];

            if (string.IsNullOrWhiteSpace(s))
                return new int[0];

            string[] parts = s.Split(',');

            List<int> channels = new List<int>();

            foreach (string p in parts)
            {
                if (int.TryParse(p.Trim(), out int ch))
                {
                    // convert user 1-8 → index 0-7
                    if (ch >= 1 && ch <= 8)
                        channels.Add(ch - 1);
                }
            }

            return channels.ToArray();
        }

        private int[] activePDChannels;
        private string lastPDChannelConfig;

        private int[] GetActivePDChannels()
        {
            string current = (string)settings["pdChannels"];

            // only re-parse if config changed
            if (activePDChannels == null || current != lastPDChannelConfig)
            {
                activePDChannels = ParsePDChannels();
                lastPDChannelConfig = current;
            }

            return activePDChannels;
        }

        public override void ScanStarting()
        {
        }

        public override void ScanFinished()
        {
        }

        public override void AcquisitionFinished()
        {
        }

        public override void ArmAndWait()
        {
            lock (this)
            {
                if (Environs.Debug) return;

                // wavemeter part
                latestData =
                    wavemeterServerContrller.getFrequency((int)settings["channel"])
                    - (double)settings["offset"];

                var snapshot = hardwareController.AcquirePDSnapshot();
                double[] pd = snapshot.Voltages;

                int[] activePD = GetActivePDChannels();

                for (int i = 0; i < 8; i++)
                {
                    latestPD[i] = double.NaN;
                }

                foreach (int idx in activePD)
                {
                    if (idx >= 0 && idx < pd.Length)
                        latestPD[idx] = pd[idx];
                    else
                        latestPD[idx] = double.NaN;
                }
            }
        }

        [XmlIgnore]
        public override ArrayList Analogs
        {
            get
            {
                lock (this)
                {
                    ArrayList a = new ArrayList();

                    if (Environs.Debug)
                    {
                        a.Add(new Random().NextDouble());
                        return a;
                    }

                    // wavemeter
                    a.Add(latestData);

                    // PDs (selected only)
                    int[] activePD = GetActivePDChannels();

                    foreach (int idx in activePD)
                        a.Add(latestPD[idx]);

                    return a;
                }
            }
        }
    }

}
