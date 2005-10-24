using System;
using System.Xml.Serialization;

using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A switch output plugin that doesn't do anything.
	/// </summary>
	[Serializable]
	public class NullSwitchPlugin : SwitchOutputPlugin
	{
		[NonSerialized]
		private bool state;

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

		[XmlIgnore]
		public override bool State
		{
			set { state = value; }
			get { return state; }
		}

		
	}
}
