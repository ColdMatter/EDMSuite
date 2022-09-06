using System;
using System.Collections;
using System.Xml.Serialization;
using System.Net;
using System.Net.Sockets;

using NationalInstruments.DAQmx;

using DAQ.Environment;
using DAQ.HAL;

using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A plugin to capture analog data using an E-series board.
	/// </summary>
	[Serializable]
	public class WavemeterInputPlugin : AnalogInputPlugin
	{
		[NonSerialized]
		private double latestData;
		[NonSerialized]
		private WavemeterLock.Controller wavemeterContrller;
		[NonSerialized]
		private string serverComputerName;
		[NonSerialized]
		private string ipAddr;

		protected override void InitialiseSettings()
		{
			settings["laser"] =  "Laser";
			settings["computer"] = "IC-CZC136CFDJ";
			settings["offset"] = 0.0;//Frequency offset in THz
		}

		public override void AcquisitionStarting()
		{
            if (!Environs.Debug)
            {
				serverComputerName = (string)settings["computer"];

				foreach (var addr in Dns.GetHostEntry(serverComputerName).AddressList)
				{
					if (addr.AddressFamily == AddressFamily.InterNetwork)
						ipAddr = addr.ToString();
				}

				EnvironsHelper eHelper = new EnvironsHelper(serverComputerName);

				wavemeterContrller = (WavemeterLock.Controller)(Activator.GetObject(typeof(WavemeterLock.Controller), "tcp://" + ipAddr + ":" + "6666" + "/controller.rem"));
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
					latestData = 1000*(wavemeterContrller.getSlaveFrequency((string)settings["laser"]) - (double)settings["offset"]);
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
