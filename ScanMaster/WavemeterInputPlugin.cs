using System;
using System.Collections;
using System.Xml.Serialization;
using System.Net;
using System.Net.Sockets;
using WavemeterLockServer;
using NationalInstruments.DAQmx;
using WavemeterLockServer;
using DAQ.Environment;
using DAQ.HAL;

using System.Net;
using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A plugin to capture wavemeter reading from WavemeterLock. Returns measured frequency-offset in GHz.
	/// </summary>
	[Serializable]
	public class WavemeterInputPlugin : AnalogInputPlugin
	{
		[NonSerialized]
		private double latestData;
		[NonSerialized]
		private WavemeterLockServer.Controller wavemeterServerContrller;
		[NonSerialized]
		private string serverComputerName;
		[NonSerialized]
		private string ipAddr;

		//private string hostName = "IC-CZC136CFDJ";// (String)System.Environment.GetEnvironmentVariables()["IC-CZC136CFDJ"];

		protected override void InitialiseSettings()
		{
			settings["channel"] =  1;
			settings["computer"] = "IC-CZC136CFDJ";
			settings["offset"] = 0.0;//Frequency offset in THz
		}

		public override void AcquisitionStarting()
		{
            if (!Environs.Debug)
            {
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
				if (!Environs.Debug)
				{
					latestData = (wavemeterServerContrller.getFrequency((int)settings["channel"]) - (double)settings["offset"]);
				}
			}
		}

		[XmlIgnore]
		public override ArrayList Analogs
		{
			get 
			{
				lock(this)
				{
					ArrayList a = new ArrayList();
					if (!Environs.Debug) a.Add(latestData);
					else a.Add(new Random().NextDouble());
					return a;
				}
			}
		}
	}
}
