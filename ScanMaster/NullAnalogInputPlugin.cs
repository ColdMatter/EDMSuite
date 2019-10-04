using System;
using System.Xml.Serialization;
using System.Collections;

using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// An analog input plugin that does nothing. Useful for debugging.
	/// </summary>
	[Serializable]
	public class NullAnalogInputPlugin : AnalogInputPlugin
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
		}

		Random rng = new Random();
		public override ArrayList Analogs
		{
			get 
			{
				ArrayList l = new ArrayList();
				l.Add(rng.NextDouble());
				l.Add(rng.NextDouble());
				return l;
			}
		}
	}
}
