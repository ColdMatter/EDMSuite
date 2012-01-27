using System;
using System.Xml.Serialization;

using ScanMaster.Acquire.Plugins;

namespace ScanMaster.Acquire.Plugin
{
	/// <summary>
	/// A plugin representing something that can be switched between two states
	/// (like an rf amplitude, ttl line).
	/// </summary>
	[Serializable]
    [XmlInclude(typeof(NullSwitchPlugin)), 
    XmlInclude(typeof(TTLSwitchPlugin))]
   
	public abstract class SwitchOutputPlugin : AcquisitorPlugin
	{
		protected override void InitialiseBaseSettings()
		{
			settings["switchActive"] = false;
		}

		/// <summary>
		/// Setting this parameter should update the hardware.
		/// Reading should return the current state (quickly).
		/// </summary>
        [XmlIgnore]
		public abstract bool State
		{
			get;
			set;
		}
	}
}
