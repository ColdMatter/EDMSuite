using System;
using System.Collections;
using System.IO;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using System.Text;

using DAQ.Environment;
using DAQ.HAL;
using Data;
using Data.Scans;
using ScanMaster.GUI;
using ScanMaster.Acquire;
using ScanMaster.Acquire.Test;
using ScanMaster.Acquire.Plugin;
using ScanMaster.Analyze;



namespace ScanMaster
{
	/// <summary>
	/// The controller is the heart of ScanMaster. The application is built around this
	/// controller, which gets the execution thread early on in application startup (look
	/// at the Main method in Runner).
	/// 
	/// The controller is a singleton (there is only ever one controller). And I really mean
	/// only one - as in there is only ever one running on the machine. There are two ways to
	/// get hold of a reference to the controller object. If you are in the same (local) context
	/// that the Controller was instantiated in (which means, almost certainly, that you are writing
	/// code that is part of ScanMaster) then you should use the static factory method
	/// Controller.GetController(). If you are outside the Controller's context (which means you are
	/// probably writing another application that wants to use ScanMaster's services) then you should
	/// get hold of an object reference through the remoting system. You might well do this by
	/// registering the Controller type as well known within your application and then using new to
	/// to get a reference, but you shouldn't forget that you're actually dealing with a singleton !
	/// 
	/// The controller is published at "tcp://localhost:1170/controller.rem".
	/// 
	/// The controller inherits from MarshalByRefObject so that references to it can be passed
	/// around by the remoting system.
	/// </summary>
	public class Controller : MarshalByRefObject	
	{

		#region Class members

		public enum AppState {stopped, running};

     	private ControllerWindow controllerWindow;
		private Acquisitor acquisitor;
		public Acquisitor Acquisitor
		{
			get { return acquisitor; }
		}
		public ScanSerializer serializer = new ScanSerializer();
		private ProfileManager profileManager = new ProfileManager();
		public ProfileManager ProfileManager
		{
			get { return profileManager; }
		}
		private ViewerManager viewerManager = new ViewerManager();
		public ViewerManager ViewerManager
		{
			get { return viewerManager; }
		}

		private DataStore dataStore = new DataStore();
		public DataStore DataStore
		{
			get { return dataStore; }
		}

		private ParameterHelper parameterHelper = new ParameterHelper();
		public ParameterHelper ParameterHelper
		{
			get { return parameterHelper;}
		}

		private static Controller controllerInstance;
		public AppState appState = AppState.stopped;

		#endregion

		#region Initialisation

		// This is the right way to get a reference to the controller. You shouldn't create a
		// controller yourself.
		public static Controller GetController() 
		{
			if (controllerInstance == null) 
			{
				controllerInstance = new Controller();
			}
			return controllerInstance;
		}

		// without this method, any remote connections to this object will time out after
		// five minutes of inactivity.
		// It just overrides the lifetime lease system completely.
		public override Object InitializeLifetimeService()
		{
			return null;
		}
		
		// This function is called at the very start of application execution.
		public void StartApplication() 
		{
            // make an acquisitor and connect ourself to its events
			acquisitor = new Acquisitor();
			acquisitor.Data += new DataEventHandler(DataHandler);
			acquisitor.ScanFinished += new ScanFinishedEventHandler(ScanFinishedHandler);

			controllerWindow = new ControllerWindow(this);
			controllerWindow.Show();

			// initialise the profile manager
			profileManager.Window = controllerWindow;
			profileManager.Start();

			// try to load in the last profile set
			// first deserialize the profile set path
			//try
			//{
			//    BinaryFormatter bf = new BinaryFormatter();
			//    String settingsPath = (string)Environs.FileSystem.Paths["settingsPath"];
			//    String filePath = settingsPath + "\\ScanMaster\\profilePath.bin";
			//    FileStream fs = File.Open(filePath, FileMode.Open);
			//    lastProfileSetPath = (string)bf.Deserialize(fs);
			//    fs.Close();
			//}
			//catch (Exception)
			//{
			//    Console.Error.WriteLine("Couldn't find saved profile path");
			//}
			//try
			//{
			//    if (lastProfileSetPath != null) LoadProfileSet(lastProfileSetPath);
			//}
			//catch (Exception e)
			//{
			//    Console.Error.WriteLine("Couldn't load last profile set");
			//    Console.Error.WriteLine(e.Message);
			//}

			// initialise the parameter helper
			parameterHelper = new ParameterHelper();
			parameterHelper.Initialise();

			// connect the acquisitor to the profile manager, which will send it events when the
			// user is in tweak mode.
			profileManager.Tweak +=new TweakEventHandler(acquisitor.HandleTweak);

            // Get access to any other applications required
            Environs.Hardware.ConnectApplications();
	
			// run the main event loop
			Application.Run(controllerWindow);
					
		}
		
		// When the main window gets told to shut, it calls this function.
		// In here things that need to be done before the application stops
		// are sorted out.
		public void StopApplication() 
		{
			AcquireStop();
			profileManager.Exit();
			
			// serialize the lastProfileSet path
			if (lastProfileSetPath != null)
			{
				BinaryFormatter bf = new BinaryFormatter();
				String settingsPath = (string)Environs.FileSystem.Paths["settingsPath"];
                String filePath = settingsPath; // + "\\ScanMaster\\profilePath.bin";
				FileStream fs = File.Open(filePath, FileMode.Create);
				bf.Serialize(fs, lastProfileSetPath);
				fs.Close();
			}

			parameterHelper.Exit();
		}

		#endregion

		#region Local functions - these should only be called locally
		
		// Main window File->Save
        public void SaveData()
        {
            // saves a zip file containing each scan, plus the average
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "zipped xml data file|*.zip";
            saveFileDialog1.Title = "Save scan data";
            saveFileDialog1.InitialDirectory = Environs.FileSystem.GetDataDirectory(
                                                (String)Environs.FileSystem.Paths["scanMasterDataPath"]);
            saveFileDialog1.FileName = Environs.FileSystem.GenerateNextDataFileName();
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog1.FileName != "")
                {
                    SaveData(saveFileDialog1.FileName);
                }

            }
        }

		// Main window File->Save Average
		public void SaveAverageData()
		{
			// serialize the average scan as zipped xml
			SaveFileDialog saveFileDialog1 = new SaveFileDialog();
			saveFileDialog1.Filter = "zipped xml data file|*.zip";
			saveFileDialog1.Title = "Save averaged scan data";
			saveFileDialog1.InitialDirectory = Environs.FileSystem.GetDataDirectory(
												(String)Environs.FileSystem.Paths["scanMasterDataPath"]);
			saveFileDialog1.FileName = Environs.FileSystem.GenerateNextDataFileName();
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				if (saveFileDialog1.FileName != "")
				{
					SaveAverageData((System.IO.FileStream)saveFileDialog1.OpenFile());
				}
			}
		}

		// Main window File->Load Average
		public void LoadData()
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "zipped xml data|*.zip";
			dialog.Title = "Open profile set";
			dialog.InitialDirectory = Environs.FileSystem.GetDataDirectory(
												(String)Environs.FileSystem.Paths["scanMasterDataPath"]);
			dialog.ShowDialog();
			if(dialog.FileName != "") LoadData(dialog.FileName);
		}


		private String lastProfileSetPath;
		public void LoadProfileSet()
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "xml profile set|*.xml";
			dialog.Title = "Open profile set";
            if (Directory.Exists(Environs.FileSystem.Paths["settingsPath"] + "ScanMaster"))
            { 
                dialog.InitialDirectory = Environs.FileSystem.Paths["settingsPath"] + "ScanMaster";
            }
            else
            {
                dialog.InitialDirectory = Environs.FileSystem.Paths["settingsPath"] + "";
            }
			dialog.ShowDialog();
			if(dialog.FileName != "")
			{
				System.IO.FileStream fs = 
					(System.IO.FileStream)dialog.OpenFile();
				profileManager.LoadProfileSetFromXml(fs);
				fs.Close();
				lastProfileSetPath = dialog.FileName;
                UpdateWindowTitle(dialog.FileName);
			}
		}

		private void LoadProfileSet(string path)
		{
			System.IO.FileStream fs = File.Open(path, FileMode.Open);
			profileManager.LoadProfileSetFromXml(fs);
			fs.Close();
			lastProfileSetPath = path;
            UpdateWindowTitle(path);
        }

		public void SaveProfileSet()
		{
			SaveFileDialog dialog = new SaveFileDialog();
			dialog.Filter = "xml profile set|*.xml";
			dialog.Title = "Save profile set";
			dialog.InitialDirectory = Environs.FileSystem.Paths["settingsPath"] + "ScanMaster";
			dialog.ShowDialog();
			if(dialog.FileName != "")
			{
				System.IO.FileStream fs = 
					(System.IO.FileStream)dialog.OpenFile();
				profileManager.SaveProfileSetAsXml(fs);
				fs.Close();
                UpdateWindowTitle(dialog.FileName);
			}
		}

        // puts the current profile's filename in the window title
        private void UpdateWindowTitle(string path)
        {
            string[] pathBits = path.Split(new char[] { '\\' });
            string profileName = pathBits[pathBits.Length - 1];
            controllerWindow.SetWindowTitle("ScanMaster 2k8 - " + profileName);
        }

		#endregion

		#region Remote functions - these functions can be called remotely

		// this method prepares the application for remote control
		public void CaptureRemote()
		{
			controllerWindow.DisableMenus();
		}

		// remote clients call this to release control
		public void ReleaseRemote()
		{
			controllerWindow.EnableMenus();
		}

		// Selecting Acquire->Start on the main window will result in this function being called.
		// If numberOfScans == -1, the acquisitor will scan forever.
		public void AcquireStart(int numberOfScans)
		{
			if (appState != AppState.running) 
			{
				if (profileManager.CurrentProfile == null)
				{
					MessageBox.Show("No profile selected !", "Profile error", MessageBoxButtons.OK,
						MessageBoxIcon.Exclamation);
					return;
				}
				Profile currentProfile = profileManager.GetCloneOfCurrentProfile();

				// clear stored data
				dataStore.ClearAll();

                // delete any temporary files
                string tempPath = Environment.GetEnvironmentVariable("TEMP") + "\\ScanMasterTemp";
                if (Directory.Exists(tempPath))
                {
                    string[] tempFiles = Directory.GetFiles(tempPath);
                    foreach (string file in tempFiles) File.Delete(file);
                }

				// tell the viewers that acquisition is starting
				viewerManager.AcquireStart();

				// start the acquisition
				acquisitor.Configuration = currentProfile.AcquisitorConfig;
                WriteScanSettings(DataStore.TotalScan);
				acquisitor.AcquireStart(numberOfScans);
				appState = AppState.running;
			}
		}

		// Selecting Acquire->Stop on the front panel will result in this function being called.
		public void AcquireStop()
		{
			if (appState == AppState.running) 
			{
				acquisitor.AcquireStop();
				appState = AppState.stopped;

				// tell the viewers acquisition has stopped
				viewerManager.AcquireStop();
             }
		}

		public void AcquireAndWait(int numberOfScans)
		{
			Monitor.Enter(acquisitor.AcquisitorMonitorLock);
			AcquireStart(numberOfScans);
            // check that acquisition is underway. Provided it is, release the lock
			if (appState == AppState.running) Monitor.Wait(acquisitor.AcquisitorMonitorLock);
			Monitor.Exit(acquisitor.AcquisitorMonitorLock);
			AcquireStop();
		}

        bool patternRunning = false;

		// outputs the pattern from the currently selected profile - used by BlockHead
		public void OutputPattern()
		{
            if (!patternRunning)
            {
                PatternPlugin pgPlugin = profileManager.CurrentProfile.AcquisitorConfig.pgPlugin;
                YAGPlugin yagPlugin = profileManager.CurrentProfile.AcquisitorConfig.yagPlugin;
                pgPlugin.AcquisitionStarting();
                yagPlugin.AcquisitionStarting();
                pgPlugin.ScanStarting();
                yagPlugin.ScanStarting();
                patternRunning = true;
            }
		}

		// stop outputting the pattern started above
		public void StopPatternOutput()
		{
            if (patternRunning)
            {
                PatternPlugin pgPlugin = profileManager.CurrentProfile.AcquisitorConfig.pgPlugin;
                YAGPlugin yagPlugin = profileManager.CurrentProfile.AcquisitorConfig.yagPlugin;
                yagPlugin.AcquisitionFinished();
                pgPlugin.AcquisitionFinished();
                controllerWindow.EnableMenus();
                patternRunning = false;
            }
		}

		// select a profile by name
		public void SelectProfile(String profile)
		{
			profileManager.SelectProfile(profile);
		}

		// change a parameter in the current profile. The way that the plugin is selected is a cheesy hack -
		// clearly the commandProcessor/pluginManager/settingsReflector design is inadequate
		// The group mode flag is also a cheesy hack, but might be quite useful
		public void AdjustProfileParameter(String plugin, String parameter, String newValue, bool groupMode)
		{
			SettingsReflector sr = new SettingsReflector();
			if (!groupMode)
			{
				sr.SetField(profileManager.Processor.PluginForString(profileManager.CurrentProfile, plugin),
					parameter, newValue);
			} 
			else
			{
				ArrayList profiles = profileManager.ProfilesInGroup(profileManager.CurrentProfile.Group);
				foreach (Profile p in profiles)
					sr.SetField(profileManager.Processor.PluginForString(p, plugin), parameter, newValue);
			}		
		}

		// this is a bit unclean ! It lets you pull out a setting from the PG plugin. Mainly a hack
		// to get BlockHead working
		public object GetPGSetting(String key)
		{
			return profileManager.CurrentProfile.AcquisitorConfig.pgPlugin.Settings[key];
		}

		public PluginSettings GetPGSettings()
		{
			return profileManager.CurrentProfile.AcquisitorConfig.pgPlugin.Settings;
		}

		// this is even more cheesy !
		public object GetShotSetting(String key)
		{
			return profileManager.CurrentProfile.AcquisitorConfig.shotGathererPlugin.Settings[key];
		}

        public object GetOutputSetting(String key)
        {
            return profileManager.CurrentProfile.AcquisitorConfig.outputPlugin.Settings[key];
        }

        // Saves the scan data to the specified file
        public void SaveData(string filename)
        {
            System.IO.FileStream fs = new FileStream(filename, FileMode.Create);
            serializer.PrepareZip(fs);
            string tempPath = Environment.GetEnvironmentVariable("TEMP") + "\\ScanMasterTemp";
            for (int k = 1; k <= DataStore.NumberOfScans; k++)
            {
                Scan sc = serializer.DeserializeScanAsBinary(tempPath + "\\scan_" + k.ToString());
                serializer.AppendToZip(sc, "scan_" + k.ToString() + ".xml");
            }
            serializer.AppendToZip(DataStore.AverageScan, "average.xml");
            serializer.CloseZip();
            fs.Close();
            Console.WriteLine(((int)(DataStore.AverageScan.GetSetting("out", "pointsPerScan"))).ToString());
        }

		// Saves the latest average scan in the datastore to the given filestream
		public void SaveAverageData( System.IO.FileStream fs )
		{
            serializer.SerializeScanAsZippedXML(fs, dataStore.AverageScan, "average.xml");
			fs.Close();
		}

		public void SaveAverageData( String filePath )
		{
			SaveAverageData( File.Create(filePath) );
		}

		public void LoadData( String path )
		{
			Scan scan = serializer.DeserializeScanFromZippedXML(path, "average.xml");
			dataStore.AverageScan = scan;
			viewerManager.NewScanLoaded();
		}


		#endregion

		#region Private functions

		// This function is registered to handle data events from the acquisitor.
		// Whenever the acquisitor has new data it will call this function.
		// Note well that this will be called on the acquisitor thread (meaning
		// no direct GUI manipulation in this function).
		private void DataHandler(object sender, DataEventArgs e)
		{
			lock (this)
			{
				// grab the settings
				GUIConfiguration guiConfig = profileManager.CurrentProfile.GUIConfig;
				// store the datapoint
				dataStore.AddScanPoint(e.point);

				// tell the viewers to handle the data point
				viewerManager.HandleDataPoint(e);
			}
		}

        // a method for saving the acquisitior settings into the scan
        private void WriteScanSettings(Scan scan)
        {
            PluginSettings st;
            ICollection keys;

            scan.ScanSettings.Add("out:pluginName", acquisitor.Configuration.outputPlugin.GetType().ToString());
            scan.ScanSettings.Add("switch:pluginName", acquisitor.Configuration.switchPlugin.GetType().ToString());
            scan.ScanSettings.Add("shot:pluginName", acquisitor.Configuration.shotGathererPlugin.GetType().ToString());
            scan.ScanSettings.Add("pg:pluginName", acquisitor.Configuration.pgPlugin.GetType().ToString());
            scan.ScanSettings.Add("yag:pluginName", acquisitor.Configuration.yagPlugin.GetType().ToString());
            scan.ScanSettings.Add("analog:pluginName", acquisitor.Configuration.analogPlugin.GetType().ToString());

            // settings from the output plugin
            st = acquisitor.Configuration.outputPlugin.Settings;
            keys = st.Keys;
            foreach (String key in keys) scan.ScanSettings.Add("out:" + key, st[key]);

            // settings from the switch plugin
            st = acquisitor.Configuration.switchPlugin.Settings;
            keys = st.Keys;
            foreach (String key in keys) scan.ScanSettings.Add("switch:" + key, st[key]);

            // settings from the shot gatherer plugin
            st = acquisitor.Configuration.shotGathererPlugin.Settings;
            keys = st.Keys;
            foreach (String key in keys) scan.ScanSettings.Add("shot:" + key, st[key]);

            // settings from the pattern plugin
            st = acquisitor.Configuration.pgPlugin.Settings;
            keys = st.Keys;
            foreach (String key in keys) scan.ScanSettings.Add("pg:" + key, st[key]);

            // settings from the yag plugin
            st = acquisitor.Configuration.yagPlugin.Settings;
            keys = st.Keys;
            foreach (String key in keys) scan.ScanSettings.Add("yag:" + key, st[key]);

            // settings from the analog plugin
            st = acquisitor.Configuration.analogPlugin.Settings;
            keys = st.Keys;
            foreach (String key in keys) scan.ScanSettings.Add("analog:" + key, st[key]);
        }


		// This function is registered with the acquisitor to handle
		// scan finished events.
		// Note well that this will be called on the acquisitor thread (meaning
		// no direct GUI manipulation in this function).
		private void ScanFinishedHandler(object sender, EventArgs e)
		{
			lock (this)
			{
				// update the datastore
				dataStore.UpdateTotal();

                // save the acquisitior settings in the scan
                WriteScanSettings(DataStore.CurrentScan);
               
				// serialize the last scan
                string tempPath = Environment.GetEnvironmentVariable("TEMP") + "\\ScanMasterTemp";
                if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
                serializer.SerializeScanAsBinary(tempPath + "\\scan_" +
                    dataStore.NumberOfScans.ToString(), dataStore.CurrentScan);

				dataStore.ClearCurrentScan();

				// tell the viewers that the scan is finished
				viewerManager.ScanFinished();

				// hint to the GC that now might be a good time
				GC.Collect();
			}
		}

		#endregion
	}
}
