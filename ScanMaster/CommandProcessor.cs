using System;
using System.Collections;
using System.Text;
using System.Threading;

using ScanMaster.Acquire.Plugin;
using ScanMaster.GUI;

namespace ScanMaster
{
	/// <summary>
	/// This class has its Run method called by the controller in its own thread.
	/// The class reads input from the command manager window and processes it.
	/// </summary>
	public class CommandProcessor
	{
		SettingsReflector sr = new SettingsReflector();
		ProfileManager manager;
		public bool groupEditMode = false;

		public CommandProcessor( ProfileManager manager )
		{
			this.manager = manager;
		}

		public void Start()
		{
			Thread commandThread = new Thread(new ThreadStart(Run));
			commandThread.Name = "Command proccessor";
			commandThread.Start();
			commandThread.IsBackground = true;
		}

		public void Run()
		{
			manager.Window.WriteLine("ScanMaster command shell.");
			for (;;)
			{
				String command = manager.Window.GetNextLine();
				
				if (manager.CurrentProfile == null) 
				{
					manager.Window.WriteLine("No profile selected !");
					continue;
				}

				if (Controller.GetController().appState != Controller.AppState.stopped)
				{
					if (command.StartsWith("tweak"))
					{
						manager.Window.WriteLine("Entering tweak mode ...");
						TweakMode(command);
						continue;
					}
					manager.Window.WriteLine("Only tweak is available when acquiring.");
					continue;
				}

				// info on the current profile
				if (command == "i") 
				{
					manager.Window.WriteLine(manager.CurrentProfile.ToString());
					continue;
				}

                // update profile set to incorporate any newly introduced settings
                if (command == "refresh")
                {
                    manager.UpdateProfiles();                                                           
                    continue;
                }

				if (command == "g")
				{
					if (groupEditMode)
					{
						groupEditMode = false;
						manager.Window.WriteLine("Group edit mode is off");
						manager.Window.Prompt = ":> ";
						manager.Window.OutputColor = System.Drawing.Color.Lime;
						continue;
					}
					else
					{
						groupEditMode = true;
						manager.Window.WriteLine("Group edit mode is on. Current group " +
							manager.CurrentProfile.Group);
						manager.Window.Prompt = manager.CurrentProfile.Group + ":> ";
						manager.Window.OutputColor = System.Drawing.Color.White;
						continue;
					}
				}

				// anything after here (apart from a syntax error) will change the profiles
				// so this is an appropriate point to
				manager.ProfilesChanged = true;

				if (command.StartsWith("set") && groupEditMode)
				{
					manager.Window.WriteLine("You can't set things in group mode.");
					continue;
				}

				// changing plugins
				if (command == "set out") 
				{
					String[] plugins = PluginRegistry.GetRegistry().GetOutputPlugins();
					int r = ChoosePluginDialog(plugins);
					if (r != -1) manager.CurrentProfile.AcquisitorConfig.SetOutputPlugin(plugins[r]);
					continue;
				}
				if (command == "set shot") 
				{
					String[] plugins = PluginRegistry.GetRegistry().GetShotGathererPlugins();
					int r = ChoosePluginDialog(plugins);
					if (r != -1) manager.CurrentProfile.AcquisitorConfig.SetShotGathererPlugin(plugins[r]);
					continue;
				}
				if (command == "set pg") 
				{
					String[] plugins = PluginRegistry.GetRegistry().GetPatternPlugins();
					int r = ChoosePluginDialog(plugins);
					if (r != -1) manager.CurrentProfile.AcquisitorConfig.SetPatternPlugin(plugins[r]);
					continue;
				}
				if (command == "set yag") 
				{
					String[] plugins = PluginRegistry.GetRegistry().GetYAGPlugins();
					int r = ChoosePluginDialog(plugins);
					if (r != -1) manager.CurrentProfile.AcquisitorConfig.SetYAGPlugin(plugins[r]);
					continue;
				}
				if (command == "set analog") 
				{
					String[] plugins = PluginRegistry.GetRegistry().GetAnalogPlugins();
					int r = ChoosePluginDialog(plugins);
					if (r != -1) manager.CurrentProfile.AcquisitorConfig.SetAnalogPlugin(plugins[r]);
					continue;
				}
				if (command == "set switch") 
				{
					String[] plugins = PluginRegistry.GetRegistry().GetSwitchPlugins();
					int r = ChoosePluginDialog(plugins);
					if (r != -1) manager.CurrentProfile.AcquisitorConfig.SetSwitchPlugin(plugins[r]);
					continue;
				}

				// changing group
				if (command.StartsWith("set group")) 
				{
					String[] bits = command.Split(new char[] {' '});
					if (bits.Length != 3) 
					{
						manager.Window.WriteLine("Syntax error.");
						continue;
					}
					manager.CurrentProfile.Group = bits[2];
					manager.Window.WriteLine("Group changed");
					continue;
				}


				// listing plugin settings
				if (command == "out")
				{
					String settings = sr.ListSettings(manager.CurrentProfile.AcquisitorConfig.outputPlugin);
					manager.Window.WriteLine(settings);
					continue;
				}
				if (command == "analog")
				{
					String settings = sr.ListSettings(manager.CurrentProfile.AcquisitorConfig.analogPlugin);
					manager.Window.WriteLine(settings);
					continue;
				}
				if (command == "switch")
				{
					String settings = sr.ListSettings(manager.CurrentProfile.AcquisitorConfig.switchPlugin);
					manager.Window.WriteLine(settings);
					continue;
				}
				if (command == "pg")
				{
					String settings = sr.ListSettings(manager.CurrentProfile.AcquisitorConfig.pgPlugin);
					manager.Window.WriteLine(settings);
					continue;
				}
				if (command == "yag")
				{
					String settings = sr.ListSettings(manager.CurrentProfile.AcquisitorConfig.yagPlugin);
					manager.Window.WriteLine(settings);
					continue;
				}
				if (command == "shot")
				{
					String settings = sr.ListSettings(manager.CurrentProfile.AcquisitorConfig.shotGathererPlugin);
					manager.Window.WriteLine(settings);
					continue;
				}
				if (command == "gui")
				{
					manager.Window.WriteLine("tofUpdate " + manager.CurrentProfile.GUIConfig.updateTOFsEvery);
					manager.Window.WriteLine("spectraUpdate " + manager.CurrentProfile.GUIConfig.updateSpectraEvery);
					manager.Window.WriteLine("switch " + manager.CurrentProfile.GUIConfig.displaySwitch);
					manager.Window.WriteLine("average " + manager.CurrentProfile.GUIConfig.average);
					continue;
				}

				// changing plugin settings
				if (command.StartsWith("out:") | command.StartsWith("analog:") | command.StartsWith("pg:")
					| command.StartsWith("yag:") | command.StartsWith("switch:") | command.StartsWith("shot:")
					| command.StartsWith("gui:"))
				{
					String[] bits = command.Split(new char[] {':', ' '});
					if (bits.Length != 3) 
					{
						manager.Window.WriteLine("Syntax error.");
						continue;
					}

					// special case for GUI settings (it's not a plugin)
					if (bits[0] == "gui")
					{
						if (groupEditMode) 
						{
							manager.Window.WriteLine("Sorry, but, hilariously, there is no "
								+ "group edit mode for GUI settings.");
							continue;
						}
						GUIConfiguration guiConfig = manager.CurrentProfile.GUIConfig;
						try 
						{
							if (bits[1] == "tofUpdate")
							{
								guiConfig.updateTOFsEvery = Convert.ToInt32(bits[2]);
								manager.Window.WriteLine("GUI:tofUpdate updated.");
								continue;
							}
							if (bits[1] == "spectraUpdate")
							{
								guiConfig.updateSpectraEvery = Convert.ToInt32(bits[2]);
								manager.Window.WriteLine("GUI:spectraUpdate updated.");
								continue;
							}
							if (bits[1] == "switch")
							{
								guiConfig.displaySwitch = Convert.ToBoolean(bits[2]);
								manager.Window.WriteLine("GUI:switch updated.");
								continue;
							}
							if (bits[1] == "average")
							{
								guiConfig.average = Convert.ToBoolean(bits[2]);
								manager.Window.WriteLine("GUI:average updated.");
								continue;
							}
							manager.Window.WriteLine("Unrecognised parameter");
						} 
						catch (Exception)
						{
							manager.Window.WriteLine("Error.");
						}
					}
					else
					{
						if (groupEditMode)
						{
							// first, check to make sure that every profile in the group has such
							// a setting.
							ArrayList groupProfiles = manager.ProfilesInGroup(manager.CurrentProfile.Group);
							bool fieldFlag = true;
							foreach (Profile p in groupProfiles)
							{
								AcquisitorPlugin pl = PluginForString(p, bits[0]);
								if (!sr.HasField(pl,bits[1])) fieldFlag = false;
							}
							if (!fieldFlag)
							{
								manager.Window.WriteLine("You can only change the value of a setting in group "
									+ "edit mode if all profiles in the group have that setting.");
								continue;
							}
							// if so, then set them all
							foreach (Profile p in groupProfiles)
							{
								AcquisitorPlugin plugin = PluginForString(p, bits[0]);
								if (sr.SetField(plugin, bits[1], bits[2]))
									manager.Window.WriteLine(p.Name + ":" + bits[0] + ":" + bits[1] + " modified.");
								else manager.Window.WriteLine("Error setting field");
							}
						}
						else
						{
							AcquisitorPlugin plugin = PluginForString(manager.CurrentProfile, bits[0]);
							if (sr.SetField(plugin, bits[1], bits[2]))
								manager.Window.WriteLine(bits[0] + ":" + bits[1] + " modified.");
							else manager.Window.WriteLine("Error setting field");
						}
					}
					continue;
				}

				// tweaking a setting
				
				// if we reach here there must be a syntax error
				manager.Window.WriteLine("Syntax error");
			}
		}

		public String[] GetCommandSuggestions(String commandStub)
		{
			String[] bits = commandStub.Split(new char[] {':'});
			// return null if can't help
			if (bits.Length !=2) return null;
			ArrayList suggestions = new ArrayList();
			AcquisitorPlugin plugin = PluginForString( manager.CurrentProfile, bits[0]);
			if (plugin == null) return null;
			String[] fieldNames = sr.ListSettingNames(plugin);
			if (fieldNames == null) return null;
			for (int i = 0 ; i < fieldNames.Length ; i++)
				if (fieldNames[i].StartsWith(bits[1])) suggestions.Add(bits[0] + ":" + fieldNames[i]);
			String[] r = new String[suggestions.Count];
			suggestions.CopyTo(r,0);
			return r;
		}

		public AcquisitorPlugin PluginForString(Profile p, String pluginType)
		{
			AcquisitorPlugin plugin = null;
			switch(pluginType)
			{
				case "out":
					plugin = p.AcquisitorConfig.outputPlugin;
					break;
				case "pg":
					plugin = p.AcquisitorConfig.pgPlugin;
					break;
				case "switch":
					plugin = p.AcquisitorConfig.switchPlugin;
					break;
				case "shot":
					plugin = p.AcquisitorConfig.shotGathererPlugin;
					break;
				case "analog":
					plugin = p.AcquisitorConfig.analogPlugin;
					break;
				case "yag":
					plugin = p.AcquisitorConfig.yagPlugin;
					break;
			}
			return plugin;
		}

		private int ChoosePluginDialog(String[] plugins)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0 ; i < plugins.Length ; i++) 
				sb.Append(" " + i + ": " + plugins[i] + Environment.NewLine);
			sb.Append("Choose a plugin:");
			manager.Window.WriteLine(sb.ToString());
			String pluginNumber = manager.Window.GetNextLine();
			try 
			{
				int index = Convert.ToInt32(pluginNumber);
				if (index >= plugins.Length | index < 0) 
				{
					manager.Window.WriteLine("Invalid input");
					return -1;
				}
				manager.Window.WriteLine(plugins[index] + " selected.");
				return index;
			}
			catch (System.FormatException)
			{
				manager.Window.WriteLine("Invalid input.");
			}
			return -1;
		}

		private void TweakMode(String command)
		{
			String[] bits = command.Split(new char[] {' '});
			if (bits.Length != 3) 
			{
				manager.Window.WriteLine("Syntax error.");
				return;
			}
			// check if this is a valid parameter to tweak
			PatternPlugin plugin = manager.CurrentProfile.AcquisitorConfig.pgPlugin;
			if (!sr.HasField(plugin, bits[1]))
			{
				manager.Window.WriteLine("The current profile's pg plugin has no field named " + bits[1]);
				return;
			}
			// is the increment valid
			int increment = 0;
			try
			{
				increment = Convert.ToInt32(bits[2]);
			} 
			catch (Exception)
			{
				manager.Window.WriteLine("Invalid increment");
				return;
			}
			
			manager.Window.WriteLine("Tweaking - i for increment, d for decrement, e for exit.");
			for (;;)
			{
				String s = manager.Window.GetNextLine();
				// check if the user wants to exit. Also check if acquisition has stopped.
				// This is not ideal but should stop anything terrible happening.
				if (s == "e" | Controller.GetController().appState != Controller.AppState.running)
				{
					manager.Window.WriteLine("Exiting tweak mode");
					return;
				}
				if (s == "i")
				{
					manager.Window.WriteLine("Incrementing " + bits[1] + " by " + increment + ".");
					int oldValue = (int)sr.GetField(plugin, bits[1]);
					int newValue = oldValue + increment;
					manager.Window.WriteLine("New value: " + newValue);
					sr.SetField(plugin, bits[1], newValue.ToString());
					manager.FireTweak(new TweakEventArgs(bits[1], newValue));
					continue;
				}
				if (s == "d")
				{
					manager.Window.WriteLine("Decrementing " + bits[1] + " by " + increment + ".");
					int oldValue = (int)sr.GetField(plugin, bits[1]);
					int newValue = oldValue - increment;
					manager.Window.WriteLine("New value: " + newValue);
					sr.SetField(plugin, bits[1], newValue.ToString());
					manager.FireTweak(new TweakEventArgs(bits[1], newValue));
					continue;
				}

				manager.Window.WriteLine("Syntax error");
			}
		}
	}
}
