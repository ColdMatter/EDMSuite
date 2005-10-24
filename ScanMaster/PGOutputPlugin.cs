using System;
using System.Threading;
using System.Xml.Serialization;

using NationalInstruments.DAQmx;

using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public class PGOutputPlugin : ScanOutputPlugin
	{
		[NonSerialized]
		private double scanParameter;

		// this plugin uses the tweak mechanism to scan pg parameters
		public event TweakEventHandler Tweak;

		protected override void InitialiseSettings()
		{
			settings["parameter"] = "flashToQ";
		}

		public override void AcquisitionStarting()
		{
			// connect ourselves to the acquisitor's tweak handler
			Tweak +=new TweakEventHandler(Controller.GetController().Acquisitor.HandleTweak);
			scanParameter = 0;
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

		public override double ScanParameter
		{
			set
			{
				scanParameter = value;
				OnTweak(new TweakEventArgs((string)settings["parameter"], (int)scanParameter));
			}
			get { return scanParameter; }
		}

		protected virtual void OnTweak( TweakEventArgs e ) 
		{
			if (Tweak != null) Tweak(this, e);
		}
	
	}
}
