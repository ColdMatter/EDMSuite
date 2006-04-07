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
				return DataFaker.GetFakeShot(1900,700,1,10, 1);
			}
		}
		
	}
}
