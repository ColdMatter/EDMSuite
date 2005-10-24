using System;
using System.Xml.Serialization;

namespace ScanMaster.Acquire.Plugin
{
	/// <summary>
	/// All data acquisition is done through plugins, and all plugins ultimately
	/// must conform to this interface. This interface specifies the lifecycle of
	/// a plugin - these methods will be called by the acquisitor at the appropriate
	/// times.
	/// A note about XmlSerialization. All plugins should be XmlSerializable so that the profiles
	/// can easily be serialized. When writing new plugins be sure to do the following:
	/// 1) Attach the [XmlIgnore] attribute to the Config property of the plugin. This needs to be
	/// done to avoid circular referencing
	/// 2) Add an [XmlInclude(typeof(YourNewPlugin))] to the abstract base class that your plugin extends.
	/// If you don't do this, serialization will fail for your plugin
	/// 3) If you have an associated settings class for the plugin, and if this settings class extends some
	/// other settings class, add the appropriate [XmlInclude] attribute to the latter.
	/// </summary>
	[Serializable]
	public abstract class AcquisitorPlugin
	{
		/// <summary>
		/// A plugin keeps a reference to the acquisitor configuration it
		/// belongs to. This makes it easy for a plugin to look into other plugins
		/// to use their settings.
		/// </summary>
		[XmlIgnore]
		protected AcquisitorConfiguration config;
		[XmlIgnore]
		public AcquisitorConfiguration Config
		{
			get { return config; }
			set { config = value; }
		}


		protected PluginSettings settings = new PluginSettings();
		public PluginSettings Settings 
		{
			get { return settings; }
			set { settings = value; }
		}

		public AcquisitorPlugin()
		{
			InitialiseAllSettings();
		}
		
		private void InitialiseAllSettings()
		{
			settings["dummy"] = "jony";
			InitialiseBaseSettings();
			InitialiseSettings();
		}
		protected abstract void InitialiseBaseSettings();
		protected abstract void InitialiseSettings();
		public abstract void AcquisitionStarting();
		public abstract void ScanStarting();
		public abstract void ScanFinished();
		public abstract void AcquisitionFinished();

	}
}
