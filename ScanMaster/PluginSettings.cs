using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Serialization;


namespace ScanMaster.Acquire.Plugin
{
	/// <summary>
	/// This class holds the settings for a plugin. It knows how (with some difficulty, using
	/// an undocumented interface in the .NET framework) to serialize itself as xml.
	/// 
	/// The class is mainly a wrapper to a hashtable. The exception is that it knows to check
	/// the default settings store for a value that overrides the current setting.
	/// </summary>
	[Serializable]
	public class PluginSettings : IXmlSerializable
	{
		private Hashtable settings = new Hashtable();

		public PluginSettings()
		{
		}

		public void Add(String key, object value)
		{
			settings.Add(key, value);
		}

		public object this[String key]
		{
			get
			{
				ParameterHelper ph = Controller.GetController().ParameterHelper;
				if (ph.HasParameter(key))
				{
					object val = ph.GetParameter(key);
					return Convert.ChangeType(val, settings[key].GetType());
				}
				else return settings[key];
			}
			set
			{
				settings.Remove(key);
				settings[key] = value;
			}
		}

		[XmlIgnore]
		public ICollection Keys
		{
			get
			{
				return settings.Keys;
			}
		}

		//public override string ToString()
		//{
		//	StringBuilder sb = new StringBuilder();
		//	ICollection keys = settings.Keys;
		//	foreach (String key in keys)
		//		sb.Append(settings[key].GetType().ToString() + " " + key +
		//					" = " + settings[key] + Environment.NewLine);
		//	return sb.ToString();
		//}

		///// Now sorting the settings alphabetically
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			ICollection keys = settings.Keys;
			ArrayList OrderedKeys = new ArrayList();

			foreach (String key in keys)
				OrderedKeys.Add(key);

			OrderedKeys.Sort();

			foreach (String key in OrderedKeys)
				sb.Append(settings[key].GetType().ToString() + " " + key +
							" = " + settings[key] + Environment.NewLine);
			return sb.ToString();
		}



            #region IXmlSerializable Members

            const string NS = "";//"http://j-star.org/xml/serialization";

		public void WriteXml(XmlWriter w)
		{
			w.WriteStartElement("dictionary", NS);
			foreach (object key in settings.Keys)
			{
				object value = settings[key];
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

				settings.Add(key, convertedValue);
			}
			r.ReadEndElement();
			r.ReadEndElement();
		}

		#endregion
	}

}
