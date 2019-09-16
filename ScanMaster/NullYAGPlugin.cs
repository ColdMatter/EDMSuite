using System;
using System.Threading;
using System.Xml.Serialization;

using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A YAG plugin that doesn't do anything.
	/// </summary>
	[Serializable]
	public class NullYAGPlugin : YAGPlugin
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
		
	}
}
