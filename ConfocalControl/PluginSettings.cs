using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Windows;

using DAQ.Environment;
using DAQ.HAL;

namespace ConfocalControl
{
	/// <summary>
	/// This class holds the settings for a plugin.
	/// </summary>
    /// 
    [Serializable]
	public class PluginSettings
	{
		private Hashtable settings = new Hashtable();
        private string _controller;
        public string controller { get { return _controller; } }

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

        public void Save()
        {
            PluginSaveLoad.WriteToBinaryFile(this);
        }
	}

    public class PluginSaveLoad
    {
        public static void WriteToBinaryFile(PluginSettings objectToWrite, bool append = false)
        {
            string directory = (string)Environs.FileSystem.Paths["settingsPath"];
            string filePath = directory + objectToWrite.controller + "Settings";

            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
            }
        }

        public static PluginSettings ReadFromBinaryFile(string controller)
        {
            string directory = (string)Environs.FileSystem.Paths["settingsPath"];
            string filePath = directory + controller + "Settings";

            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (PluginSettings)binaryFormatter.Deserialize(stream);
            }
        }

        public static PluginSettings LoadSettings(string controller)
        {
            try
            {
                return ReadFromBinaryFile(controller);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return new PluginSettings(controller);
            }
        }
    }
}
