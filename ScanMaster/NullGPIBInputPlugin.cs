using System;
using System.Xml.Serialization;
using System.Collections;

using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A GPIB input plugin that does nothing. Useful for debugging.
	/// </summary>
	[Serializable]
	public class NullGPIBInputPlugin : GPIBInputPlugin
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
		public override double GPIBval
		{
			get 
			{
				double l = rng.NextDouble();
				
				return l;
			}
		}
	}
}
