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
