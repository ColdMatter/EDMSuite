using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using DAQ.Environment;
using DAQ.HAL;

namespace ConfocalControl
{
	/// <summary>
	/// This class holds the settings for a plugin.
	/// </summary>
	public class PluginSettings
	{
		private Hashtable settings = new Hashtable();
        private string _controller;

		public PluginSettings(string controller)
		{
            _controller = controller;
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

        public void SaveSettings()
        {
            string directory = (string)Environs.FileSystem.Paths["settingsPath"];
            string address = directory + _controller + "Settings.txt";

            System.IO.File.WriteAllText(@address, ToString());
        }

        private enum ISTYPE { DOUBLE, INT, STRING, UNKNOWN };

        private ISTYPE CheckType(string type)
        {
            if (type == "System.Int32") return ISTYPE.INT;
            else if (type == "System.Double") return ISTYPE.DOUBLE;
            else if (type == "System.String") return ISTYPE.STRING;
            else return ISTYPE.UNKNOWN;
        }

        public void LoadSettings()
        {
            string directory = (string)Environs.FileSystem.Paths["settingsPath"];
            string address = directory + _controller + "Settings.txt";

            System.IO.StreamReader file = new System.IO.StreamReader(@address);

            string line;
            while((line = file.ReadLine()) != null)
            {
                string[] slist = line.Split(null);
                string name = slist[1];

                switch (CheckType(slist[0]))
                {
                    case ISTYPE.INT:
                        settings[name] = Convert.ToInt32(slist[3]);
                        break;

                    case ISTYPE.DOUBLE:
                        settings[name] = Convert.ToDouble(slist[3]);
                        break;

                    case ISTYPE.STRING:
                        settings[name] = slist[3];
                        break;

                    case ISTYPE.UNKNOWN:
                        break;

                    default:
                        break;
                }
            }

            file.Close();
        }
	}
}
