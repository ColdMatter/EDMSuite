using System;
using System.Threading;
using System.Xml.Serialization;

using DAQ.Environment;
using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A plugin that asks the YAG to do something sensible.
	/// </summary>
	[Serializable]
	public class DefaultYAGPlugin : YAGPlugin
	{

		protected override void InitialiseSettings()
		{
		}

		public override void AcquisitionStarting()
		{
			Environs.Hardware.YAG.StartFlashlamps(false);
		}

		public override void ScanStarting()
		{
			Environs.Hardware.YAG.EnableQSwitch();
		}

		public override void ScanFinished()
		{
			Environs.Hardware.YAG.DisableQSwitch();
		}

		public override void AcquisitionFinished()
		{
			if ((bool)config.pgPlugin.Settings["stopFlashlamps"]) Environs.Hardware.YAG.StopFlashlamps();

		}
		
	}
}
