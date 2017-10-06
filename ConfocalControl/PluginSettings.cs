using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Serialization;


namespace ConfocalControl
{
	/// <summary>
	/// This class holds the settings for a plugin.
	/// </summary>
	public class PluginSettings
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
				return settings[key];
			}
			set
			{
				settings.Remove(key);
				settings[key] = value;
			}
		}

		public ICollection Keys
		{
			get
			{
				return settings.Keys;
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			ICollection keys = settings.Keys;
			foreach (String key in keys)
				sb.Append(settings[key].GetType().ToString() + " " + key + 
							" = " + settings[key] + Environment.NewLine);
			return sb.ToString();
		}
	}
}
