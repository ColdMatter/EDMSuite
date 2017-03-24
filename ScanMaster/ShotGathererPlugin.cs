using System;
using System.Collections;
using System.Xml.Serialization;

using Data;
using ScanMaster.Acquire.Plugins;

namespace ScanMaster.Acquire.Plugin
{

	/// <summary>
	/// A plugin representing something that gathers time of flight
	/// data.
	/// </summary>
	[Serializable]
	[XmlInclude(typeof(AnalogShotGathererPlugin)), XmlInclude(typeof(NullShotGathererPlugin)), 
	XmlInclude(typeof(ModulatedAnalogShotGathererPlugin)), XmlInclude(typeof(BufferedEventCountingShotGathererPlugin)),
    XmlInclude(typeof(ImageGrabbingAnalogShotGathererPlugin)),XmlInclude(typeof(FastAnalogShotGathererPlugin)),
    XmlInclude(typeof(FastCountingShotGathererPlugin)), XmlInclude(typeof(MultiInputShotGathererPlugin)),
    XmlInclude(typeof(FastMultiInputShotGathererPlugin))]
	public abstract class ShotGathererPlugin : AcquisitorPlugin
	{
		protected override void InitialiseBaseSettings()
		{
			settings["gateStartTime"] = 1900;
			settings["gateLength"] = 700;
			settings["clockPeriod"] = 1;
			settings["sampleRate"] = 1000000;
			settings["channel"] = "pmt";
			settings["inputRangeLow"] = -1.0;
			settings["inputRangeHigh"] = 1.0;
            settings["preArm"] = false;
		}
		
		/// <summary>
		/// Arm the hardware and wait. This method should not return until it
		/// is safe for the acquisitor to read the shot data out.
		/// </summary>
		public abstract void ArmAndWait();

        /// <summary>
        /// This seperately starts the Task for quick acquisitions
        /// </summary>
        public virtual void PreArm()
        {
        }

        /// <summary>
        /// This sepereately stops the Task for quick acquisitions
        /// </summary>
        public virtual void PostArm()
        {
        }

		/// <summary>
		/// The most recently acquired shot.
		/// </summary>
		public abstract Shot Shot 
		{
			get;
		}

	}
}
