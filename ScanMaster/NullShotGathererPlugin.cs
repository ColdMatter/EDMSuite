using System;
using System.Threading;
using System.Xml.Serialization;

using DAQ.FakeData;
using Data;
using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A plugin that makes up fairly unconvincing time of flight data.
	/// Useful for debugging.
	/// </summary>
	[Serializable]
	public class NullShotGathererPlugin : ShotGathererPlugin
	{

		protected override void InitialiseSettings()
		{
		}

		
		public override void AcquisitionStarting()
		{
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
			Thread.Sleep(15);
		}

		public override Shot Shot
		{
			get 
			{
                ScanOutputPlugin outputPlugin = config.outputPlugin;
                double sp = outputPlugin.ScanParameter;
                double start = (double)outputPlugin.Settings["start"];
                double end = (double)outputPlugin.Settings["end"];
                
                double intensity = 1 + 10 * (1 / (1 + Math.Pow((sp - 2), 2) / Math.Pow((end + start) / 1, 2)));
			//	double intensity = 1 + 10 * Math.Pow(Math.Sin((sp - 2.5) / 0.3), 2);
				return DataFaker.GetFakeShot(1900, 700, 1, intensity, 1);
			}
		}
		
	}
}
