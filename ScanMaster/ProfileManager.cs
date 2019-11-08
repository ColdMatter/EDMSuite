using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Windows.Forms;
using System.Xml.Serialization;

using ScanMaster.Acquire.Plugin;
using ScanMaster.Acquire.Plugins;

using ScanMaster.GUI;

namespace ScanMaster
{

	public delegate void TweakEventHandler(object sender, TweakEventArgs e);
	
	/// <summary>
	/// The profile manager stores and edits profiles. It owns a window and a command processor
	/// and handles control flow between them.
	/// </summary>
	[Serializable]
	public class ProfileManager
	{
		// when the profile manager is in tweak mode it will fire Tweak events
		public event TweakEventHandler Tweak;

		private ControllerWindow window;
		public ControllerWindow Window
		{
			get { return window; }
			set { window = value; }
		}
		private CommandProcessor processor;
		public CommandProcessor Processor
		{
			get { return processor; }
		}

		private ArrayList profiles = new ArrayList();
		public ArrayList Profiles
		{
			get { return profiles; }
			set { profiles = value; }
		}

		private Profile currentProfile;
		public Profile CurrentProfile
		{
			get { return currentProfile; }
			set { currentProfile = value; }
		}
		
		public bool ProfilesChanged = false;

		public void Start()
		{
			// stick in a dummy profiles for now
			profiles.Add(new Profile());

			processor = new CommandProcessor(this);
			processor.Start();
			window.UpdateUI();
		}

		public void Exit()
		{
			if (ProfilesChanged)
			{
				if (MessageBox.Show("Profile set has been modified. Save ?", "Save profiles", MessageBoxButtons.YesNo,
					MessageBoxIcon.Exclamation) == DialogResult.Yes)
				{
					Controller.GetController().SaveProfileSet();
				}
			}
		}
		
		public void LoadProfileSetFromSoap(FileStream stream)
		{
			// load the settings; soap format
			SoapFormatter s = new SoapFormatter();
			profiles = (ArrayList)s.Deserialize(stream);
			currentProfile = null;
			window.UpdateUI();
		}

		public void LoadProfileSetFromXml(FileStream stream)
		{
			// load the settings; xml format
			XmlSerializer s = new XmlSerializer(typeof(ProfileSet));
			ProfileSet ps = (ProfileSet)s.Deserialize(stream);
			profiles = ps.Profiles;
			currentProfile = null;
			// Xml serialization cannot handle circular referencing, so each of the plugins need to be
			// assigned their AquisitorConfigurations 'by hand'.
			foreach(Profile p in profiles)
			{
				p.AcquisitorConfig.outputPlugin.Config = p.AcquisitorConfig;
				p.AcquisitorConfig.switchPlugin.Config = p.AcquisitorConfig;
				p.AcquisitorConfig.shotGathererPlugin.Config = p.AcquisitorConfig;
				p.AcquisitorConfig.pgPlugin.Config = p.AcquisitorConfig;
				p.AcquisitorConfig.yagPlugin.Config = p.AcquisitorConfig;
				p.AcquisitorConfig.analogPlugin.Config = p.AcquisitorConfig;
			}
			window.UpdateUI();
		}

		public void SaveProfileSetAsSoap(FileStream stream) 
		{
			//save the settings; soap format
			SoapFormatter s = new SoapFormatter();
			s.Serialize(stream, Profiles);
		}

		public void SaveProfileSetAsXml(FileStream stream)
		{
			// save the settings; xml format
			XmlSerializer s = new XmlSerializer(typeof(ProfileSet));
			ProfileSet ps = new ProfileSet();
			ps.Profiles = this.Profiles;
			s.Serialize(stream, ps);
		}

		public void AddNewProfile()
		{
			profiles.Add(new Profile());
			ProfilesChanged = true;
		}

		public void DeleteProfile(int index)
		{
			profiles.RemoveAt(index);
			ProfilesChanged = true;
		}

		public void SelectProfile(int index)
		{
			// no changing profiles while the thing is running - just too confusing !
			if (Controller.GetController().appState == Controller.AppState.running) return;

			currentProfile = (Profile)profiles[index];
			window.WriteLine("Profile changed");
			if (processor.groupEditMode)
			{
				window.WriteLine("Group changed to " + currentProfile.Group);
				window.Prompt = currentProfile.Group + ":>";
			}
			window.UpdateUI();
		}

		public void SelectProfile(String profile)
		{
			int index = -1;
			for (int i = 0 ; i < profiles.Count ; i++)
			{
				if (((Profile)profiles[i]).Name == profile) index = i;
			}
			if (index != -1) SelectProfile(index);
		}


		public void CloneProfile(int index)
		{
			profiles.Add(((Profile)profiles[index]).Clone());
			ProfilesChanged = true;
		}
		
		public Profile GetCloneOfCurrentProfile()
		{
			return (Profile)currentProfile.Clone();
		}

		public ArrayList ProfilesInGroup(String group)
		{
			ArrayList groupProfiles = new ArrayList();
			foreach (Profile p in profiles) if (p.Group == group) groupProfiles.Add(p);
			return groupProfiles;
		}

		public void FireTweak(TweakEventArgs e)
		{
			OnTweak(e);
		}

		protected virtual void OnTweak( TweakEventArgs e ) 
		{
			if (Tweak != null) Tweak(this, e);
		}

        // When a new setting is introduced into a plugin, there needs to be a way to introduce it into existing
        // profile sets. This is the code that does it. 
        // For each profile, p, a dummy profile, d, is created whose plugins are the same as those of p.
        // Because d is constructed anew, all plugin settings will have their default values and any newly
        // introduced settings will be present in d. Any settings that are in d, but not in p, are then copied over
        // into p. This is done for all the profiles in the set.
        public void UpdateProfiles()
        {
            Type pluginType;
            System.Reflection.ConstructorInfo info;
           
            Profile tempProfile = new Profile();

            foreach (Profile prof in profiles)
            {
                // analog input plugin
                pluginType = (Type)prof.AcquisitorConfig.analogPlugin.GetType();
                info = pluginType.GetConstructor(new Type[] { });
                tempProfile.AcquisitorConfig.analogPlugin = (AnalogInputPlugin)info.Invoke(new object[] { });
                updateSettings(prof.AcquisitorConfig.analogPlugin.Settings, tempProfile.AcquisitorConfig.analogPlugin.Settings);

                // scan output plugin
                pluginType = (Type)prof.AcquisitorConfig.outputPlugin.GetType();
                info = pluginType.GetConstructor(new Type[] { });
                tempProfile.AcquisitorConfig.outputPlugin = (ScanOutputPlugin)info.Invoke(new object[] { });
                updateSettings(prof.AcquisitorConfig.outputPlugin.Settings, tempProfile.AcquisitorConfig.outputPlugin.Settings);

                // pattern plugin
                pluginType = (Type)prof.AcquisitorConfig.pgPlugin.GetType();
                info = pluginType.GetConstructor(new Type[] { });
                tempProfile.AcquisitorConfig.pgPlugin = (PatternPlugin)info.Invoke(new object[] { });
                updateSettings(prof.AcquisitorConfig.pgPlugin.Settings, tempProfile.AcquisitorConfig.pgPlugin.Settings);

                // shot gatherer plugin
                pluginType = (Type)prof.AcquisitorConfig.shotGathererPlugin.GetType();
                info = pluginType.GetConstructor(new Type[] { });
                tempProfile.AcquisitorConfig.shotGathererPlugin = (ShotGathererPlugin)info.Invoke(new object[] { });
                updateSettings(prof.AcquisitorConfig.shotGathererPlugin.Settings, tempProfile.AcquisitorConfig.shotGathererPlugin.Settings);

                // switch plugin
                pluginType = (Type)prof.AcquisitorConfig.switchPlugin.GetType();
                info = pluginType.GetConstructor(new Type[] { });
                tempProfile.AcquisitorConfig.switchPlugin = (SwitchOutputPlugin)info.Invoke(new object[] { });
                updateSettings(prof.AcquisitorConfig.switchPlugin.Settings, tempProfile.AcquisitorConfig.switchPlugin.Settings);

                // yag plugin
                pluginType = (Type)prof.AcquisitorConfig.yagPlugin.GetType();
                info = pluginType.GetConstructor(new Type[] { });
                tempProfile.AcquisitorConfig.yagPlugin = (YAGPlugin)info.Invoke(new object[] { });
                updateSettings(prof.AcquisitorConfig.yagPlugin.Settings, tempProfile.AcquisitorConfig.yagPlugin.Settings);
            }
        }

        // Supports the updateProfiles method
        private void updateSettings(PluginSettings currentSettings, PluginSettings defaultSettings)
        {
            ICollection defaultKeys = defaultSettings.Keys;
            String[] defaults = new String[defaultKeys.Count];
            defaultKeys.CopyTo(defaults, 0);

            foreach (String s in defaults)
                if (currentSettings[s] == null) currentSettings[s] = defaultSettings[s];
        }

	}

	public class TweakEventArgs : EventArgs
	{
		public String parameter;
		public double newValue;

		public TweakEventArgs( String parameter, int newValue)
		{
			this.parameter = parameter;
			this.newValue = newValue;
		}

	}

}
