using System;
using System.Collections;
using System.Xml.Serialization;
using Utility;

namespace EDMConfig
{
	/// <summary>
	/// The idea is that a BlockConfig contains all of the configuration information that
	/// BlockHead needs to take a block (well, almost. It doesn't contain mappings to 
	/// physical channels at the minute). The config is serialized inside the block so that,
	/// in principle, the analysis code can analyse against any parameter.
	/// 
	/// The switching channels are stored as Modulations. There are currently three types of
	/// modulation: digital, analog and timing (not implemented yet). This distinction is primarily
	/// for the benefit of the analysis code. See BlockHead for how these channels are mapped to
	/// switches in the lab (maybe an example would help clear this up - there is a difference
	/// between what the modulation does and how it's implemented. Take the B step. It's an
	/// analog modulation as it modulates an analog quantity (the current) however in the lab
	/// the actual modulation is produced by a box which is fed a ttl signal, so it's mapped in
	/// blockhead to a TTLSwitchChannel. Make sense ?) 
	/// </summary>
	[Serializable]
	public class BlockConfig
	{
		// modulations - timing modulations are not implemented yet
		protected ArrayList digitalModulations = new ArrayList();
        protected ArrayList analogModulations = new ArrayList();
        protected ArrayList timingModulations = new ArrayList();
		public XmlSerializableHashtable Settings = new XmlSerializableHashtable();

		[XmlArray]
		[XmlArrayItem(Type = typeof(DigitalModulation))]
		public ArrayList DigitalModulations
		{
			get { return digitalModulations; }
		}

		[XmlArray]
		[XmlArrayItem(Type = typeof(AnalogModulation))]
		public ArrayList AnalogModulations
		{
			get { return analogModulations; }
		}

		[XmlArray]
		[XmlArrayItem(Type = typeof(TimingModulation))]
		public ArrayList TimingModulations
		{
			get { return timingModulations; }
		}

		public Modulation GetModulationByName(String name)
		{
			foreach (Modulation m in digitalModulations)
			{
				if (m.Name == name) return m;
			}
			foreach (Modulation m in analogModulations)
			{
				if (m.Name == name) return m;
			}
			foreach (Modulation m in timingModulations)
			{
				if (m.Name == name) return m;
			}
			return null;
		}
    }
}
