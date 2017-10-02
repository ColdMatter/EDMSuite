using System;
using System.Reflection;
using System.Collections;
using System.Text;

namespace ScanMaster.Acquire.Plugin
{
	/// <summary>
	/// This class looks into plugins and gets hold of their settings.
	/// It does this by reflection. It supports the command processor.
	/// </summary>
	public class SettingsReflector
	{
		public String ListSettings(AcquisitorPlugin plugin)
		{
			return plugin.Settings.ToString();
		}

		public String[] ListSettingNames(AcquisitorPlugin plugin)
		{
			ICollection keys = plugin.Settings.Keys;
			ArrayList keyAL = new ArrayList();
			foreach (String key in keys) keyAL.Add(key);
			return (String[])keyAL.ToArray(typeof(string));
		}

		public bool SetField(AcquisitorPlugin plugin, String fieldName, String newValue)
		{

			PluginSettings ps = plugin.Settings;
			object currentValue = ps[fieldName];
            if (!this.HasField(plugin, fieldName))
            {
                return false;
            }
            else
            {
                try
                {
                    object convertedType = Convert.ChangeType(newValue, currentValue.GetType());
                    ps[fieldName] = convertedType;
                    return true;
                }
                catch (System.FormatException)
                {
                    return false;
                }
            catch (System.NullReferenceException)
            {
                return false;
            }

            }
		}

		public object GetField(AcquisitorPlugin plugin, String fieldName)
		{
            if (!this.HasField(plugin, fieldName))
            {
                return "No such parameter";
            }
            else
            {
                return plugin.Settings[fieldName]; 
            }
			
		}

		public bool HasField(AcquisitorPlugin plugin, String fieldName)
		{
			return plugin.Settings[fieldName] != null;
		}
	}
}
