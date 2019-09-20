using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Serialization;


namespace Utility
{
	/// <summary>
	/// This is a wrapper for the hashtable that you know and love that lets
	/// it serialize itself to an xml stream (with some difficulty, using
	/// an undocumented interface in the .NET framework).
	/// </summary>
	[Serializable]
	public class XmlSerializableHashtable : IXmlSerializable
	{
		private Hashtable table = new Hashtable();

		public XmlSerializableHashtable()
		{
			// for a reason I haven't figured out, deserialization goes into an infinite
			// loop if the dictionary is empty - this is a workaround.
			table["dummy"] = "dummy";
		}

		public void Add(String key, object value)
		{
			table.Remove(key);
			table.Add(key, value);
		}

		public object this[String key]
		{
			get
			{
				return table[key];
			}
			set
			{
				table.Remove(key);
				table[key] = value;
			}
		}

		[XmlIgnore]
		public ICollection Keys
		{
			get
			{
				return table.Keys;
			}
		}

		[XmlIgnore]
		public String[] StringKeyList
		{
			get
			{
				ArrayList tempList = new ArrayList();
				foreach (object key in Keys)
				{
					if (!(key.GetType() == Type.GetType("System.String"))) break;
					String keyString = (String)key;
					tempList.Add(keyString);
				}
				return (string[])tempList.ToArray(Type.GetType("System.String"));
			}

		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			ICollection keys = table.Keys;
			foreach (String key in keys)
				sb.Append(table[key].GetType().ToString() + " " + key + 
					" = " + table[key] + Environment.NewLine);
			return sb.ToString();
		}
		#region IXmlSerializable Members

		const string NS = "";//"http://j-star.org/xml/serialization";

		public void WriteXml(XmlWriter w)
		{
			w.WriteStartElement("dictionary", NS);
			foreach (object key in table.Keys)
			{
				object value = table[key];
				w.WriteStartElement("item", NS);
				w.WriteElementString("key", NS, key.ToString());
				w.WriteElementString("value", NS, value.ToString());
				w.WriteElementString("type", NS, value.GetType().ToString());
				w.WriteEndElement();
			}
			w.WriteEndElement();
		}

		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader r)
		{
			r.Read(); // move past container
			r.ReadStartElement("dictionary");
			while (r.NodeType != XmlNodeType.EndElement)
			{            
				r.ReadStartElement("item", NS);
				string key = r.ReadElementString("key", NS);
				string value = r.ReadElementString("value", NS);
				string type = r.ReadElementString("type", NS);
				r.ReadEndElement();
				object convertedValue = null;
				try 
				{
					convertedValue = Convert.ChangeType(value, Type.GetType(type));
				}
				catch (FormatException e)
				{
					Console.Error.Write("Format error in xml dictionary deserializer");
					Console.Error.Write(e.Message + e.StackTrace);
				}

				table[key] = convertedValue;
			}
			r.ReadEndElement();
			r.ReadEndElement();
		}

		#endregion
	}

}
