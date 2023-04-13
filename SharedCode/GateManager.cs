using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

using Data;

namespace Analysis.EDM
{
    /// <summary>
	/// The gate manager stores and edits gate sets.
	/// </summary>
	[Serializable]
    public class GateManager
    {
        private GatedDemodulationConfigSet gateSet = new GatedDemodulationConfigSet();
        public GatedDemodulationConfigSet GateSet
        {  
            get { return gateSet; }
            set { gateSet = value; }
        }

        public void LoadGateSetFromXml(FileStream stream)
        {
            XmlSerializer s = new XmlSerializer(typeof(GatedDemodulationConfigSet));
            gateSet = (GatedDemodulationConfigSet)s.Deserialize(stream);
        }

        public void SaveGateSetAsXml(FileStream stream)
        {
            XmlSerializer s = new XmlSerializer(typeof(GatedDemodulationConfigSet));
            s.Serialize(stream, gateSet);
        }
    }
}
