using System;
using System.Collections;
using System.Windows.Forms;
using System.Text;
using System.Threading;

using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;

using Data;
using DAQ.FakeData;
using System.Collections.Generic;


namespace ScanMaster.GUI
{

	/// <summary>
	/// </summary>
	public class ControllerWindow : System.Windows.Forms.Form
	{

		// the application controller
		private Controller controller;

		private String latestLine;
		private bool newLineAvailable = false;
		private ProfileManager manager;
		public String Prompt = ":> ";
        private List<string> commands = new List<string>(new string[] {""});
        private int commandMarker = 0;

		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem menuItem5;
		private System.Windows.Forms.MenuItem menuItem6;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.StatusBar statusBar1;
		private NationalInstruments.UI.XAxis pmtXAxis;
		private System.Windows.Forms.MenuItem menuItem7;
		private System.Windows.Forms.MenuItem menuItem12;
		private System.Windows.Forms.MenuItem menuItem13;
		private System.Windows.Forms.MenuItem menuItem14;
		private System.Windows.Forms.Button renameButton;
		private System.Windows.Forms.Label currentProfileLabel;
		private System.Windows.Forms.TextBox commandTextBox;
		private System.Windows.Forms.TextBox outputTextBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button deleteButton;
		private System.Windows.Forms.Button cloneButton;
		private System.Windows.Forms.Button newButton;
		private System.Windows.Forms.Button selectButton;
		private System.Windows.Forms.ListBox profileListBox;
		private System.Windows.Forms.MenuItem viewerMenu;
		private System.Windows.Forms.MenuItem fileMenu;
		private System.Windows.Forms.MenuItem acquireMenu;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.MenuItem menuItem9;
		private System.Windows.Forms.MenuItem schonMenu;
		private System.Windows.Forms.MenuItem menuItem10;
		private System.Windows.Forms.MenuItem menuItem11;
		private System.Windows.Forms.MenuItem patternMenu;
		private System.Windows.Forms.MenuItem menuItem8;
		private System.Windows.Forms.MenuItem menuItem15;

		public ControllerWindow(Controller controller)
		{
			this.controller = controller;
			this.manager = controller.ProfileManager;
			InitializeComponent();

			// build the viewer menu
			viewerMenu.MenuItems.Clear();
			Hashtable viewers = controller.ViewerManager.Viewers;
			foreach (DictionaryEntry de in viewers)
			{
				MenuItem item = new MenuItem(de.Key.ToString());
				item.Click +=new EventHandler(viewerClicked);
				viewerMenu.MenuItems.Add(item);
			}

			// build the Schon menu
			foreach (DictionaryEntry de in DataFaker.FakeScans)
			{
				MenuItem item = new MenuItem(de.Key.ToString());
				item.Click +=new EventHandler(schonClicked);
				schonMenu.MenuItems.Add(item);
			}
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControllerWindow));
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.fileMenu = new System.Windows.Forms.MenuItem();
            this.menuItem13 = new System.Windows.Forms.MenuItem();
            this.menuItem12 = new System.Windows.Forms.MenuItem();
            this.menuItem9 = new System.Windows.Forms.MenuItem();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.menuItem14 = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.menuItem15 = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.acquireMenu = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.viewerMenu = new System.Windows.Forms.MenuItem();
            this.patternMenu = new System.Windows.Forms.MenuItem();
            this.menuItem10 = new System.Windows.Forms.MenuItem();
            this.menuItem11 = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem8 = new System.Windows.Forms.MenuItem();
            this.schonMenu = new System.Windows.Forms.MenuItem();
            this.pmtXAxis = new NationalInstruments.UI.XAxis();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.renameButton = new System.Windows.Forms.Button();
            this.currentProfileLabel = new System.Windows.Forms.Label();
            this.commandTextBox = new System.Windows.Forms.TextBox();
            this.outputTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.deleteButton = new System.Windows.Forms.Button();
            this.cloneButton = new System.Windows.Forms.Button();
            this.newButton = new System.Windows.Forms.Button();
            this.selectButton = new System.Windows.Forms.Button();
            this.profileListBox = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.fileMenu,
            this.acquireMenu,
            this.viewerMenu,
            this.patternMenu,
            this.menuItem1,
            this.schonMenu});
            // 
            // fileMenu
            // 
            this.fileMenu.Index = 0;
            this.fileMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem13,
            this.menuItem12,
            this.menuItem9,
            this.menuItem4,
            this.menuItem14,
            this.menuItem2,
            this.menuItem7,
            this.menuItem15,
            this.menuItem3});
            this.fileMenu.Text = "File";
            // 
            // menuItem13
            // 
            this.menuItem13.Index = 0;
            this.menuItem13.Text = "Load profile set ...";
            this.menuItem13.Click += new System.EventHandler(this.LoadProfileSetHandler);
            // 
            // menuItem12
            // 
            this.menuItem12.Index = 1;
            this.menuItem12.Text = "Save profile set ...";
            this.menuItem12.Click += new System.EventHandler(this.SaveProfileSetHandler);
            // 
            // menuItem9
            // 
            this.menuItem9.Index = 2;
            this.menuItem9.Text = "-";
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 3;
            this.menuItem4.Text = "Load average data ...";
            this.menuItem4.Click += new System.EventHandler(this.LoadDataHandler);
            // 
            // menuItem14
            // 
            this.menuItem14.Index = 4;
            this.menuItem14.Text = "-";
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 5;
            this.menuItem2.Text = "Save scan data ...";
            this.menuItem2.Click += new System.EventHandler(this.SaveDataHandler);
            // 
            // menuItem7
            // 
            this.menuItem7.Index = 6;
            this.menuItem7.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
            this.menuItem7.Text = "Save average data ...";
            this.menuItem7.Click += new System.EventHandler(this.SaveAverageDataHandler);
            // 
            // menuItem15
            // 
            this.menuItem15.Index = 7;
            this.menuItem15.Text = "-";
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 8;
            this.menuItem3.Text = "Exit";
            this.menuItem3.Click += new System.EventHandler(this.MenuExitClicked);
            // 
            // acquireMenu
            // 
            this.acquireMenu.Index = 1;
            this.acquireMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem5,
            this.menuItem6});
            this.acquireMenu.Text = "Acquire";
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 0;
            this.menuItem5.Shortcut = System.Windows.Forms.Shortcut.CtrlG;
            this.menuItem5.Text = "Start";
            this.menuItem5.Click += new System.EventHandler(this.AcquireStartClicked);
            // 
            // menuItem6
            // 
            this.menuItem6.Index = 1;
            this.menuItem6.Shortcut = System.Windows.Forms.Shortcut.CtrlC;
            this.menuItem6.Text = "Stop";
            this.menuItem6.Click += new System.EventHandler(this.AcquireStopClicked);
            // 
            // viewerMenu
            // 
            this.viewerMenu.Index = 2;
            this.viewerMenu.Text = "Viewers";
            // 
            // patternMenu
            // 
            this.patternMenu.Index = 3;
            this.patternMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem10,
            this.menuItem11});
            this.patternMenu.Text = "Pattern";
            // 
            // menuItem10
            // 
            this.menuItem10.Index = 0;
            this.menuItem10.Shortcut = System.Windows.Forms.Shortcut.CtrlF;
            this.menuItem10.Text = "Start pattern output";
            this.menuItem10.Click += new System.EventHandler(this.menuItem10_Click);
            // 
            // menuItem11
            // 
            this.menuItem11.Index = 1;
            this.menuItem11.Shortcut = System.Windows.Forms.Shortcut.CtrlX;
            this.menuItem11.Text = "Stop pattern output";
            this.menuItem11.Click += new System.EventHandler(this.menuItem11_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 4;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem8});
            this.menuItem1.Text = "Debug";
            this.menuItem1.Visible = false;
            // 
            // menuItem8
            // 
            this.menuItem8.Index = 0;
            this.menuItem8.Text = "Test interlock";
            this.menuItem8.Click += new System.EventHandler(this.menuItem8_Click);
            // 
            // schonMenu
            // 
            this.schonMenu.Index = 5;
            this.schonMenu.Text = "Schon\'s menu";
            // 
            // pmtXAxis
            // 
            this.pmtXAxis.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 405);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Size = new System.Drawing.Size(778, 22);
            this.statusBar1.SizingGrip = false;
            this.statusBar1.TabIndex = 13;
            this.statusBar1.Text = "Ready";
            // 
            // renameButton
            // 
            this.renameButton.Location = new System.Drawing.Point(96, 376);
            this.renameButton.Name = "renameButton";
            this.renameButton.Size = new System.Drawing.Size(75, 23);
            this.renameButton.TabIndex = 23;
            this.renameButton.Text = "Rename ...";
            this.renameButton.Click += new System.EventHandler(this.RenameHandler);
            // 
            // currentProfileLabel
            // 
            this.currentProfileLabel.Location = new System.Drawing.Point(8, 312);
            this.currentProfileLabel.Name = "currentProfileLabel";
            this.currentProfileLabel.Size = new System.Drawing.Size(232, 24);
            this.currentProfileLabel.TabIndex = 22;
            this.currentProfileLabel.Text = "Current profile: ";
            // 
            // commandTextBox
            // 
            this.commandTextBox.BackColor = System.Drawing.Color.Black;
            this.commandTextBox.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.commandTextBox.ForeColor = System.Drawing.Color.Red;
            this.commandTextBox.Location = new System.Drawing.Point(280, 376);
            this.commandTextBox.Name = "commandTextBox";
            this.commandTextBox.Size = new System.Drawing.Size(488, 22);
            this.commandTextBox.TabIndex = 20;
            this.commandTextBox.TextChanged += new System.EventHandler(this.commandTextBox_TextChanged);
            this.commandTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.KeyUpHandler);
            // 
            // outputTextBox
            // 
            this.outputTextBox.BackColor = System.Drawing.Color.Black;
            this.outputTextBox.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.outputTextBox.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.outputTextBox.ForeColor = System.Drawing.Color.Lime;
            this.outputTextBox.Location = new System.Drawing.Point(280, 32);
            this.outputTextBox.Multiline = true;
            this.outputTextBox.Name = "outputTextBox";
            this.outputTextBox.ReadOnly = true;
            this.outputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.outputTextBox.Size = new System.Drawing.Size(488, 336);
            this.outputTextBox.TabIndex = 21;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 24);
            this.label1.TabIndex = 19;
            this.label1.Text = "Profiles:";
            // 
            // deleteButton
            // 
            this.deleteButton.Location = new System.Drawing.Point(8, 376);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(75, 23);
            this.deleteButton.TabIndex = 18;
            this.deleteButton.Text = "Delete";
            this.deleteButton.Click += new System.EventHandler(this.DeleteHandler);
            // 
            // cloneButton
            // 
            this.cloneButton.Location = new System.Drawing.Point(184, 344);
            this.cloneButton.Name = "cloneButton";
            this.cloneButton.Size = new System.Drawing.Size(75, 23);
            this.cloneButton.TabIndex = 17;
            this.cloneButton.Text = "Clone";
            this.cloneButton.Click += new System.EventHandler(this.CloneHandler);
            // 
            // newButton
            // 
            this.newButton.Location = new System.Drawing.Point(96, 344);
            this.newButton.Name = "newButton";
            this.newButton.Size = new System.Drawing.Size(75, 23);
            this.newButton.TabIndex = 16;
            this.newButton.Text = "New";
            this.newButton.Click += new System.EventHandler(this.NewProfileHandler);
            // 
            // selectButton
            // 
            this.selectButton.Location = new System.Drawing.Point(8, 344);
            this.selectButton.Name = "selectButton";
            this.selectButton.Size = new System.Drawing.Size(75, 23);
            this.selectButton.TabIndex = 15;
            this.selectButton.Text = "Select";
            this.selectButton.Click += new System.EventHandler(this.SelectProfileHandler);
            // 
            // profileListBox
            // 
            this.profileListBox.Location = new System.Drawing.Point(8, 32);
            this.profileListBox.Name = "profileListBox";
            this.profileListBox.Size = new System.Drawing.Size(232, 264);
            this.profileListBox.TabIndex = 14;
            this.profileListBox.DoubleClick += new System.EventHandler(this.SelectProfileHandler);
            // 
            // ControllerWindow
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(778, 427);
            this.Controls.Add(this.renameButton);
            this.Controls.Add(this.currentProfileLabel);
            this.Controls.Add(this.commandTextBox);
            this.Controls.Add(this.outputTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.deleteButton);
            this.Controls.Add(this.cloneButton);
            this.Controls.Add(this.newButton);
            this.Controls.Add(this.selectButton);
            this.Controls.Add(this.profileListBox);
            this.Controls.Add(this.statusBar1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Menu = this.mainMenu1;
            this.Name = "ControllerWindow";
            this.Text = "ScanMaster 2k8";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.ControllerWindow_Closing);
            this.Load += new System.EventHandler(this.ControllerWindow_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void AcquireStartClicked(object sender, System.EventArgs e)
		{
			controller.AcquireStart(-1);
		}

		private void AcquireStopClicked(object sender, System.EventArgs e)
		{
			controller.AcquireStop();
		}

		private void MenuExitClicked(object sender, System.EventArgs e)
		{
			Close();
		}

		private void SaveDataHandler(object sender, System.EventArgs e)
		{
			controller.SaveData();
		}
		private void SaveAverageDataHandler(object sender, System.EventArgs e)
		{
			controller.SaveAverageData();
		}
		private void LoadProfileSetHandler(object sender, System.EventArgs e)
		{
			controller.LoadProfileSet();
		}
		private void SaveProfileSetHandler(object sender, System.EventArgs e)
		{
			controller.SaveProfileSet();
		}
		private void ControllerWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			controller.StopApplication();
		}

		private void KeyUpHandler(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			// if enter is pressed we should process the command
			if (e.KeyData == Keys.Enter)
			{
				String command = commandTextBox.Text;
                commands.Add(command);
                commandMarker = commands.Count;
				commandTextBox.Clear();
				// echo the command
				WriteLine(Prompt + command);
				lock(this)
				{
					latestLine = command;
					newLineAvailable = true;
				}
						
			}
		    // if up is pressed return the previous command in the list in the usual way
            if (e.KeyData == Keys.Up)
            {
                commandMarker--;
                if (commandMarker<0) commandMarker = 0;
                commandTextBox.Clear();
                commandTextBox.Text= commands[commandMarker];
                commandTextBox.Select(commandTextBox.Text.Length,0); 
            }

            // if down is pressed return the next command in the usual way
            if (e.KeyData == Keys.Down)
            {
                commandMarker++;
                if (commandMarker > commands.Count) commandMarker = commands.Count;
                commandTextBox.Clear();
                if (commandMarker == commands.Count)
                {
                }
                else
                {
                    commandTextBox.Text = commands[commandMarker];
                    commandTextBox.Select(commandTextBox.Text.Length, 0);
                }
            }

			// tab attempts to autocomplete the command
			if (e.KeyData == Keys.Tab)
			{
				String[] suggestions = manager.Processor.GetCommandSuggestions(commandTextBox.Text);
				if (suggestions == null) return;
				if (suggestions.Length == 1) 
				{
					commandTextBox.Text = suggestions[0] + " ";
					commandTextBox.SelectionStart = commandTextBox.Text.Length;
					return;
				}
				StringBuilder sb = new StringBuilder();
				foreach (String s in suggestions) sb.Append(s + "\t");
				sb.Append(Environment.NewLine);
				WriteLine(sb.ToString());
			}
		
			// escape clears the current text
			if (e.KeyData == Keys.Escape) commandTextBox.Clear();
		
		}
		
		// this method disables the annoying thunk noise when enter, tab, escape are pressed !
		protected override bool ProcessDialogKey(System.Windows.Forms.Keys keyData)
		{
			if(keyData == Keys.Enter) return true;
			if(keyData == Keys.Tab) return true;
			if(keyData == Keys.Escape) return true;
			return false;
		}
		
		private delegate void AppendTextDelegate(String text);
		public void WriteLine(String text)
		{
			outputTextBox.BeginInvoke(new AppendTextDelegate(outputTextBox.AppendText),
				new object[] {text + Environment.NewLine});
		}
		
		public String GetNextLine()
		{
			for (;;)
			{
				lock(this)
				{
					if (newLineAvailable) 
					{
						newLineAvailable = false;
						String returnLine = latestLine;
						latestLine = null;
						return returnLine;
					}
				}
				Thread.Sleep(20);
			}
		}

		private void SelectProfileHandler(object sender, System.EventArgs e)
		{
			if (profileListBox.SelectedIndex == -1) return;
			else
			{
				manager.SelectProfile(profileListBox.SelectedIndex);
			}
		}

		private void NewProfileHandler(object sender, System.EventArgs e)
		{
			manager.AddNewProfile();
			UpdateUI();
		}		

		private void CloneHandler(object sender, System.EventArgs e)
		{
			if (profileListBox.SelectedIndex == -1) return;
			else
			{
				manager.CloneProfile(profileListBox.SelectedIndex);
				UpdateUI();
			}
		}

		private void DeleteHandler(object sender, System.EventArgs e)
		{
			if (profileListBox.SelectedIndex == -1) return;
			else
			{
				manager.DeleteProfile(profileListBox.SelectedIndex);
				UpdateUI();
			}
		}

		private void RenameHandler(object sender, System.EventArgs e)
		{
			if (profileListBox.SelectedIndex == -1) return;
			else
			{
				ProfileRenameDialog dialog = new ProfileRenameDialog();
				Profile selectedProfile = ((Profile)manager.Profiles[profileListBox.SelectedIndex]);
				dialog.ProfileName = selectedProfile.Name;
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					selectedProfile.Name = dialog.ProfileName;
					UpdateUI();
				}
			}
		}

		public void UpdateUI()
		{
            profileListBox.Invoke(new UpdateDelegate(ReallyUpdateUI), null);
		}

        private delegate void UpdateDelegate();
        private void ReallyUpdateUI()
        {
            profileListBox.Items.Clear();
            if (manager.Profiles.Count != 0)
            {
                foreach (Profile p in manager.Profiles) profileListBox.Items.Add(p.Name);
                if (manager.CurrentProfile == null) currentProfileLabel.Text = "Current profile: ";
                else currentProfileLabel.Text = "Current profile: " + manager.CurrentProfile.Name;
            }
            else
            {
                // this works around a bug in visual studio/winforms!
                profileListBox.Items.Add("Dummy profile (do not select!)");
            }

        }

		public void DisableMenus()
		{
			fileMenu.Enabled = false;
			acquireMenu.Enabled = false;
//			viewerMenu.Enabled = false;
			schonMenu.Enabled = false;
//			patternMenu.Enabled = false;
		}

		public void EnableMenus()
		{
			fileMenu.Enabled = true;
			acquireMenu.Enabled = true;
//			viewerMenu.Enabled = true;
			schonMenu.Enabled = true;
//			patternMenu.Enabled = true;
		}

		public System.Drawing.Color OutputColor
		{
			set
			{
				outputTextBox.ForeColor = value;
			}
		}

		private void viewerClicked(object sender, EventArgs e)
		{
			MenuItem item = (MenuItem)sender;
			((Viewer)Controller.GetController().ViewerManager.Viewers[item.Text]).ToggleVisible();
		}

		private void schonClicked(object sender, EventArgs e)
		{
			MenuItem item = (MenuItem)sender;
			Controller.GetController().LoadData(DataFaker.GetFakeDataPath(item.Text));
		}

		private void LoadDataHandler(object sender, System.EventArgs e)
		{
			Controller.GetController().LoadData();
		}

		private void menuItem10_Click(object sender, System.EventArgs e)
		{
			Controller.GetController().OutputPattern();
		}

		private void menuItem11_Click(object sender, System.EventArgs e)
		{
			Controller.GetController().StopPatternOutput();
		}

		private void menuItem8_Click(object sender, System.EventArgs e)
		{
			DAQ.HAL.BrilliantLaser laser = ((DAQ.HAL.BrilliantLaser)DAQ.Environment.Environs.Hardware.YAG);
	//		laser.Connect();
			bool test = laser.InterlockFailed;
//			laser.Disconnect();
		}

        private void ControllerWindow_Load(object sender, EventArgs e)
        {
            UpdateUI();
        }

        // thread-safe wrapper for setting window title.
        public void SetWindowTitle(string text)
        {
            BeginInvoke(new SetTextDelegate(SetWindowTitleInternal), new object[] { text });
        }
        private delegate void SetTextDelegate(string text);
        private void SetWindowTitleInternal(string text)
        {
            Text = text;
        }

        private void commandTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        

	}
}


