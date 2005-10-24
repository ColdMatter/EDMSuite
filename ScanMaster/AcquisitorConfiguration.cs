using System;
using System.Xml.Serialization;
using ScanMaster.Acquire.Plugin;
using ScanMaster.Acquire.Plugins;

namespace ScanMaster.Acquire
{
	/// <summary>
	/// This is a complete description of how the acquisitor should take data.
	/// These configurations are stored per profile. The profile is the union
	/// of 6 plugins, each of which has its own settings.
	/// </summary>
	[Serializable]
	public class AcquisitorConfiguration
	{
		public ScanOutputPlugin outputPlugin;
		public SwitchOutputPlugin switchPlugin;
		public ShotGathererPlugin shotGathererPlugin;
		public PatternPlugin pgPlugin;
		public YAGPlugin yagPlugin;
		public AnalogInputPlugin analogPlugin;

		public AcquisitorConfiguration()
		{
			// install a set of default plugins
			SetOutputPlugin("No scan");
			SetSwitchPlugin("No switch");
			SetShotGathererPlugin("Constant, fake data");
			SetPatternPlugin("No pattern");
			SetYAGPlugin("No YAG");
			SetAnalogPlugin("No analog input");
		}

		public void SetOutputPlugin(String type)
		{
			outputPlugin = PluginRegistry.GetRegistry().GetOutputPlugin(type);
			outputPlugin.Config = this;
		}

		public void SetSwitchPlugin(String type)
		{
			switchPlugin = PluginRegistry.GetRegistry().GetSwitchPlugin(type);
			switchPlugin.Config = this;
		}

		public void SetShotGathererPlugin(String type)
		{
			shotGathererPlugin = PluginRegistry.GetRegistry().GetShotGathererPlugin(type);
			shotGathererPlugin.Config = this;
		}
	
		public void SetPatternPlugin(String type)
		{
			pgPlugin = PluginRegistry.GetRegistry().GetPatternPlugin(type);
			pgPlugin.Config = this;
		}

		public void SetYAGPlugin(String type)
		{
			yagPlugin = PluginRegistry.GetRegistry().GetYAGPlugin(type);
			yagPlugin.Config = this;
		}

		public void SetAnalogPlugin(String type)
		{
			analogPlugin = PluginRegistry.GetRegistry().GetAnalogPlugin(type);
			analogPlugin.Config = this;
		}

		public override string ToString()
		{
			return outputPlugin + Environment.NewLine +
				pgPlugin + Environment.NewLine +
				switchPlugin + Environment.NewLine +
				shotGathererPlugin + Environment.NewLine +
				analogPlugin + Environment.NewLine +
				yagPlugin + Environment.NewLine;

		}


	}
}
