using System;
using System.Collections;
using System.Xml.Serialization;

using Utility;

namespace Data.EDM
{
	/// <summary>
	/// </summary>
	[Serializable]
	public class EDMPoint : MarshalByRefObject
	{
		public Shot Shot;
		public XmlSerializableHashtable SinglePointData = new XmlSerializableHashtable();
	}
}
