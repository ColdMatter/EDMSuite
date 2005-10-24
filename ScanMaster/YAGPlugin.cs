using System;
using System.Xml.Serialization;

using ScanMaster.Acquire.Plugins;

namespace ScanMaster.Acquire.Plugin
{
	/// <summary>
	/// Controls the YAG laser as the acquisitor scans.
	/// </summary>
	[Serializable]
	[XmlInclude(typeof(NullYAGPlugin)), XmlInclude(typeof(DefaultYAGPlugin)),
		XmlInclude(typeof(NotInTheLeastBitBrilliantYAGPlugin))]
	public abstract class YAGPlugin : AcquisitorPlugin
	{
		protected override void InitialiseBaseSettings()
		{
		}

	}
}
