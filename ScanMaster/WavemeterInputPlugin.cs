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
		private WavemeterLockServer.Controller wavemeterContrller;
		[NonSerialized]
		private string serverComputerName;
		[NonSerialized]
		private string ipAddr;

		protected override void InitialiseSettings()
		{
			settings["channel"] =  2;
			settings["computer"] = "IC-CZC136CFDJ";
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

				wavemeterContrller = (WavemeterLockServer.Controller)(Activator.GetObject(typeof(WavemeterLockServer.Controller), "tcp://" + ipAddr + ":" + "1984" + "/controller.rem"));
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
					latestData = wavemeterContrller.getFrequency((int)settings["channel"]);
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
