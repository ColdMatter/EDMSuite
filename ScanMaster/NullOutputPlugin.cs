using System;
using System.Threading;
using System.Xml.Serialization;

using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A scan output plugin that doesn't so anything.
	/// </summary>
	[Serializable]
	public class NullOutputPlugin : ScanOutputPlugin
	{

		protected override void InitialiseSettings()
		{
		}

		
		[NonSerialized]
		private double scanParameter;

		public override void AcquisitionStarting()
		{
		}

		public override void ScanStarting()
		{
		}

		public override void ScanFinished()
		{
			//Thread.Sleep(2000);
		}

		public override void AcquisitionFinished()
		{
		}

		public override double ScanParameter
		{
			set { scanParameter = value; }
			get { return scanParameter; }
		}

		
	}
}
