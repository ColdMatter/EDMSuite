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
        private List<double> latestData = new List<double>();
        [NonSerialized]
        //private List<double> latestPD = new List<double>();
        private double[] latestPD;
        [NonSerialized]
        private WavemeterLockServer.Controller wavemeterServerContrller;
        [NonSerialized]
        private string serverComputerName;
        [NonSerialized]
        private string ipAddr;
        [NonSerialized]
        private UEDMHardwareControl.UEDMController hardwareController;
        //private string hostName = "IC-CZC136CFDJ";// (String)System.Environment.GetEnvironmentVariables()["IC-CZC136CFDJ"];

        protected override void InitialiseSettings()
        {
            settings["channel"] = "1,7";
            settings["computer"] = "WS8SERVERHUXLEY";
            settings["offset"] = 0.0;//Frequency offset in THz

            settings["pdChannels"] = "1,2,3,4,5,6,7,8"; //8 photodiodes channels to read from the Hardware Controller
        }

        public override void AcquisitionStarting()
        {
            latestPD = new double[8];
            if (!Environs.Debug)
            {
                hardwareController =
                (UEDMHardwareControl.UEDMController)
                Activator.GetObject(
                    typeof(UEDMHardwareControl.UEDMController),
                    "tcp://localhost:1172/UEDMController.rem");
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
            DisconnectProxy();
        }

        public override void AcquisitionFinished()
        {
            DisconnectProxy();
        }

        private void DisconnectProxy()
        {
            lock (this)
            {
                if (hardwareController != null)
                {
                    try { System.Runtime.Remoting.RemotingServices.Disconnect(hardwareController); } catch { }
                    hardwareController = null;
                }
                if (wavemeterServerContrller != null)
                {
                    try { System.Runtime.Remoting.RemotingServices.Disconnect(wavemeterServerContrller); } catch { }
                    wavemeterServerContrller = null;
                }
            }
        }

        public override void ArmAndWait()
        {
            lock (this)
            {
                if (Environs.Debug) return;

                // wavemeter part
                if (latestData == null)
                    latestData = new List<double>();
                latestData.Clear();
                if (!Environs.Debug)
                {
                    string channelList = (string)settings["channel"];
                    string[] channels = channelList.Split(new char[] { ',' });
                    foreach (string channel in channels)
                    {
                        latestData.Add(wavemeterServerContrller.getFrequency(int.Parse(channel)) - (double)settings["offset"]);
                    }
                }

                // PD part
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
                    
                    // wavemeter data
                    if (latestData != null)
                    {
                        foreach (double freq in latestData)
                        {
                            a.Add((double)freq);
                        }
                    }

                    // Append active PD data sequentially
                    int[] activePD = GetActivePDChannels();
                    foreach (int idx in activePD)
                    {
                        a.Add((double)latestPD[idx]);
                    }

                    return a;
                }
            }
        }

    }

}
