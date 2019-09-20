using System;
using System.Xml.Serialization;

namespace ScanMaster.GUI
{
	/// <summary>
	/// The settings that are needed to configure the GUI. These settings
	/// are stored per profile.
	/// </summary>
	[Serializable]
	public class GUIConfiguration
	{
		public int updateTOFsEvery = 10;
		public int updateSpectraEvery = 1;
		public bool displaySwitch = false;
		public bool average = false;
	}
}
