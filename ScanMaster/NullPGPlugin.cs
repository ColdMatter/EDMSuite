using System;
using System.Xml.Serialization;

using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A pattern plugin that doesn't do anything.
	/// </summary>
	[Serializable]
	public class NullPGPlugin : PatternPlugin
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

		public override void ReloadPattern()
		{
			Console.WriteLine((String)settings["test"]);
		}

		
	}
}
