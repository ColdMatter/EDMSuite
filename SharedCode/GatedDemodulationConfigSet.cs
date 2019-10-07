using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Analysis.EDM
{
	/// <summary>
	/// The class provides a convenient way to XmlSerialize and deserialize the set of gate demodulation configs.
	/// </summary>
	[Serializable]
	public class GatedDemodulationConfigSet
	{
		public GatedDemodulationConfigSet()
		{
		}
	
		private List<GatedDemodulationConfig> gatedDemodulationConfigs;

		[XmlArray]
		[XmlArrayItem(Type = typeof(GatedDemodulationConfig))]
		public List<GatedDemodulationConfig> GatedDemodulationConfigs
        {
			get { return gatedDemodulationConfigs; }
			set { gatedDemodulationConfigs = value; }
		}
	}
	
}
