using System;
using System.Threading;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using NationalInstruments;
using NationalInstruments.DAQmx;
using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;
using DAQ.Environment;
using DAQ.HAL;


namespace EDMLeakageMonitor
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class MainWindow : System.Windows.Forms.Form, ICurrentLeakageFrontEnd
	{
		private Thread backEndThread;
		private NationalInstruments.UI.XAxis xAxis1;
		private NationalInstruments.UI.YAxis yAxis1;
		private NationalInstruments.UI.WaveformPlot waveformPlot1;
		private NationalInstruments.UI.WaveformPlot waveformPlot2;
		private NationalInstruments.UI.XAxis xAxis2;
		private NationalInstruments.UI.YAxis yAxis2;
		private NationalInstruments.UI.WaveformPlot waveformPlot3;
		private NationalInstruments.UI.XAxis xAxis3;
		private NationalInstruments.UI.YAxis yAxis3;
		private NationalInstruments.UI.WaveformPlot waveformPlot4;
		private NationalInstruments.UI.XAxis xAxis4;
		private NationalInstruments.UI.YAxis yAxis4;
		private NationalInstruments.UI.WindowsForms.WaveformGraph northCGraph;
		private NationalInstruments.UI.WindowsForms.WaveformGraph southCGraph;
		private NationalInstruments.UI.WindowsForms.WaveformGraph southGGraph;
		private NationalInstruments.UI.WindowsForms.WaveformGraph northGGraph;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItemFile;
		private System.Windows.Forms.MenuItem menuItemExit;
		private System.Windows.Forms.MenuItem menuItemStop;
		// Current leakage class declaration
		private CurrentLeakageBackEnd clb;
		private TextReadoutDialog textBasedResultsDialog;

		private System.Windows.Forms.StatusBarPanel statusBarMessage;
		private System.Windows.Forms.StatusBar statusBar;
		private System.Windows.Forms.MenuItem menuItemMonitor;
		private System.Windows.Forms.MenuItem menuItemStart;
		private System.Windows.Forms.MenuItem menuItemViewer;
		private System.Windows.Forms.MenuItem menuItemResetGraphs;
		private System.Windows.Forms.MenuItem menuItemShowDialog;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private ArrayList dataStore1;
		private System.Windows.Forms.MenuItem mISave;
		private ArrayList dataStore2;

		public MainWindow()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			textBasedResultsDialog = new TextReadoutDialog();
			dataStore1 = new ArrayList();
			dataStore2 = new ArrayList();

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MainWindow));
			this.northCGraph = new NationalInstruments.UI.WindowsForms.WaveformGraph();
			this.waveformPlot1 = new NationalInstruments.UI.WaveformPlot();
			this.xAxis1 = new NationalInstruments.UI.XAxis();
			this.yAxis1 = new NationalInstruments.UI.YAxis();
			this.southCGraph = new NationalInstruments.UI.WindowsForms.WaveformGraph();
			this.waveformPlot2 = new NationalInstruments.UI.WaveformPlot();
			this.xAxis2 = new NationalInstruments.UI.XAxis();
			this.yAxis2 = new NationalInstruments.UI.YAxis();
			this.southGGraph = new NationalInstruments.UI.WindowsForms.WaveformGraph();
			this.waveformPlot3 = new NationalInstruments.UI.WaveformPlot();
			this.xAxis3 = new NationalInstruments.UI.XAxis();
			this.yAxis3 = new NationalInstruments.UI.YAxis();
			this.northGGraph = new NationalInstruments.UI.WindowsForms.WaveformGraph();
			this.waveformPlot4 = new NationalInstruments.UI.WaveformPlot();
			this.xAxis4 = new NationalInstruments.UI.XAxis();
			this.yAxis4 = new NationalInstruments.UI.YAxis();
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.menuItemFile = new System.Windows.Forms.MenuItem();
			this.menuItemExit = new System.Windows.Forms.MenuItem();
			this.menuItemMonitor = new System.Windows.Forms.MenuItem();
			this.menuItemStart = new System.Windows.Forms.MenuItem();
			this.menuItemStop = new System.Windows.Forms.MenuItem();
			this.menuItemViewer = new System.Windows.Forms.MenuItem();
			this.menuItemShowDialog = new System.Windows.Forms.MenuItem();
			this.menuItemResetGraphs = new System.Windows.Forms.MenuItem();
			this.statusBar = new System.Windows.Forms.StatusBar();
			this.statusBarMessage = new System.Windows.Forms.StatusBarPanel();
			this.mISave = new System.Windows.Forms.MenuItem();
			((System.ComponentModel.ISupportInitialize)(this.northCGraph)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.southCGraph)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.southGGraph)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.northGGraph)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.statusBarMessage)).BeginInit();
			this.SuspendLayout();
			// 
			// northCGraph
			// 
			this.northCGraph.Caption = "North C";
			this.northCGraph.Location = new System.Drawing.Point(8, 8);
			this.northCGraph.Name = "northCGraph";
			this.northCGraph.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
																						  this.waveformPlot1});
			this.northCGraph.Size = new System.Drawing.Size(312, 224);
			this.northCGraph.TabIndex = 0;
			this.northCGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
																				   this.xAxis1});
			this.northCGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
																				   this.yAxis1});
			// 
			// waveformPlot1
			// 
			this.waveformPlot1.XAxis = this.xAxis1;
			this.waveformPlot1.YAxis = this.yAxis1;
			// 
			// yAxis1
			// 
			this.yAxis1.Caption = "nA";
			// 
			// southCGraph
			// 
			this.southCGraph.Caption = "South C";
			this.southCGraph.Location = new System.Drawing.Point(328, 8);
			this.southCGraph.Name = "southCGraph";
			this.southCGraph.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
																						  this.waveformPlot2});
			this.southCGraph.Size = new System.Drawing.Size(312, 224);
			this.southCGraph.TabIndex = 1;
			this.southCGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
																				   this.xAxis2});
			this.southCGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
																				   this.yAxis2});
			// 
			// waveformPlot2
			// 
			this.waveformPlot2.XAxis = this.xAxis2;
			this.waveformPlot2.YAxis = this.yAxis2;
			// 
			// yAxis2
			// 
			this.yAxis2.Caption = "nA";
			// 
			// southGGraph
			// 
			this.southGGraph.Caption = "South G";
			this.southGGraph.Location = new System.Drawing.Point(328, 240);
			this.southGGraph.Name = "southGGraph";
			this.southGGraph.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
																						  this.waveformPlot3});
			this.southGGraph.Size = new System.Drawing.Size(312, 224);
			this.southGGraph.TabIndex = 3;
			this.southGGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
																				   this.xAxis3});
			this.southGGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
																				   this.yAxis3});
			// 
			// waveformPlot3
			// 
			this.waveformPlot3.XAxis = this.xAxis3;
			this.waveformPlot3.YAxis = this.yAxis3;
			// 
			// yAxis3
			// 
			this.yAxis3.Caption = "nA";
			// 
			// northGGraph
			// 
			this.northGGraph.Caption = "North G";
			this.northGGraph.Location = new System.Drawing.Point(8, 240);
			this.northGGraph.Name = "northGGraph";
			this.northGGraph.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
																						  this.waveformPlot4});
			this.northGGraph.Size = new System.Drawing.Size(312, 224);
			this.northGGraph.TabIndex = 2;
			this.northGGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
																				   this.xAxis4});
			this.northGGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
																				   this.yAxis4});
			// 
			// waveformPlot4
			// 
			this.waveformPlot4.XAxis = this.xAxis4;
			this.waveformPlot4.YAxis = this.yAxis4;
			// 
			// yAxis4
			// 
			this.yAxis4.Caption = "nA";
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItemFile,
																					  this.menuItemMonitor,
																					  this.menuItemViewer});
			// 
			// menuItemFile
			// 
			this.menuItemFile.Index = 0;
			this.menuItemFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuItemExit,
																						 this.mISave});
			this.menuItemFile.Text = "File";
			// 
			// menuItemExit
			// 
			this.menuItemExit.Index = 0;
			this.menuItemExit.Text = "Exit";
			this.menuItemExit.Click += new System.EventHandler(this.menuItemExit_Click);
			// 
			// menuItemMonitor
			// 
			this.menuItemMonitor.Index = 1;
			this.menuItemMonitor.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																							this.menuItemStart,
																							this.menuItemStop});
			this.menuItemMonitor.Text = "Monitor";
			// 
			// menuItemStart
			// 
			this.menuItemStart.Index = 0;
			this.menuItemStart.Text = "Start";
			this.menuItemStart.Click += new System.EventHandler(this.menuItemStart_Click);
			// 
			// menuItemStop
			// 
			this.menuItemStop.Enabled = false;
			this.menuItemStop.Index = 1;
			this.menuItemStop.Text = "Stop";
			this.menuItemStop.Click += new System.EventHandler(this.menuItemStop_Click);
			// 
			// menuItemViewer
			// 
			this.menuItemViewer.Index = 2;
			this.menuItemViewer.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						   this.menuItemShowDialog,
																						   this.menuItemResetGraphs});
			this.menuItemViewer.Text = "View";
			// 
			// menuItemShowDialog
			// 
			this.menuItemShowDialog.Index = 0;
			this.menuItemShowDialog.Text = "Text Values";
			this.menuItemShowDialog.Click += new System.EventHandler(this.menuItemShowDialog_Click);
			// 
			// menuItemResetGraphs
			// 
			this.menuItemResetGraphs.Index = 1;
			this.menuItemResetGraphs.Text = "Reset Graphs";
			this.menuItemResetGraphs.Click += new System.EventHandler(this.menuItemResetGraphs_Click);
			// 
			// statusBar
			// 
			this.statusBar.Location = new System.Drawing.Point(0, 491);
			this.statusBar.Name = "statusBar";
			this.statusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
																						 this.statusBarMessage});
			this.statusBar.ShowPanels = true;
			this.statusBar.Size = new System.Drawing.Size(648, 22);
			this.statusBar.TabIndex = 4;
			// 
			// statusBarMessage
			// 
			this.statusBarMessage.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
			this.statusBarMessage.Width = 632;
			// 
			// mISave
			// 
			this.mISave.Index = 1;
			this.mISave.Text = "Save";
			this.mISave.Click += new System.EventHandler(this.mISave_Click);
			// 
			// MainWindow
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(648, 513);
			this.Controls.Add(this.statusBar);
			this.Controls.Add(this.southGGraph);
			this.Controls.Add(this.northGGraph);
			this.Controls.Add(this.southCGraph);
			this.Controls.Add(this.northCGraph);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Menu = this.mainMenu1;
			this.Name = "MainWindow";
			this.Text = "Leakage Current Monitor";
			this.Load += new System.EventHandler(this.MainWindow_Load);
			((System.ComponentModel.ISupportInitialize)(this.northCGraph)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.southCGraph)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.southGGraph)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.northGGraph)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.statusBarMessage)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MainWindow());
		}

		private void cRegionOnly()
		{
			northCGraph.Enabled = false;
			southCGraph.Enabled = false; 
		}

		public void HandleBackEndMessage(string message)
		{
			statusBarMessage.Text = "Back End: " + message;
		}

		private void HandleFrontEndMessage(string message)
		{
			statusBarMessage.Text = "Front End: " + message;
		}
		
		public delegate void PlotYAppendSingleDelegate(double data);
		public void PlotDataPoints(ArrayList recentData)
		{
			//convert real currents to nA
			for(int i = 0;i<4;i++)
			{
				recentData[i]=(double)recentData[i]*1000000000;
			}

			//todo: Kill these threads before closing.
			northCGraph.Invoke( new PlotYAppendSingleDelegate(northCGraph.PlotYAppend), new object[] {(double)recentData[0]});
			northGGraph.Invoke( new PlotYAppendSingleDelegate(northGGraph.PlotYAppend), new object[] {(double)recentData[1]});
			southCGraph.Invoke( new PlotYAppendSingleDelegate(southCGraph.PlotYAppend), new object[] {(double)recentData[2]});
			southGGraph.Invoke( new PlotYAppendSingleDelegate(southGGraph.PlotYAppend), new object[] {(double)recentData[3]});

			if (textBasedResultsDialog.Visible)
			{
				textBasedResultsDialog.populateTextBoxes(recentData);
			}
			if (dataStore1.Count>1000000)
			{
				dataStore1.RemoveAt(0);
				dataStore2.RemoveAt(0);
			}
			dataStore1.Add(recentData[0]);
			dataStore2.Add(recentData[1]);

		}

		//Menu item actions 
 
		private void menuItemStart_Click(object sender, System.EventArgs e)
		{
			//Initialise the backend
			clb = new CurrentLeakageBackEnd(this);

			//Set the menuItem states.
			menuItemStart.Enabled = false;
			menuItemStop.Enabled = true;
			
			//Start the backEnd in it's own thread.
			backEndThread = new Thread( new ThreadStart(clb.start)); 
			backEndThread.Start();
		}

		private void menuItemStop_Click(object sender, System.EventArgs e)
		{
			HandleFrontEndMessage("Stop Acquisition");
			clb.BreakAcquisition = true;
			menuItemStart.Enabled = true;
			menuItemStop.Enabled = false;
		}

		private void menuItemExit_Click(object sender, System.EventArgs e)
		{
			Application.Exit();
		}

		private void menuItemShowDialog_Click(object sender, System.EventArgs e)
		{
			textBasedResultsDialog.Show();
			textBasedResultsDialog.Focus();
		}

		private void menuItemResetGraphs_Click(object sender, System.EventArgs e)
		{
			this.southCGraph.ClearData();
			this.southGGraph.ClearData();
			this.northCGraph.ClearData();
			this.northGGraph.ClearData();
		}

		private void MainWindow_Load(object sender, System.EventArgs e)
		{
		
		}

		private void mISave_Click(object sender, System.EventArgs e)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.ShowDialog(this);
			if (sfd.FileName !="")
			{
				FileStream fs = (FileStream)sfd.OpenFile();
				StreamWriter sw = new StreamWriter(fs);
				for(int i=0; i<dataStore1.Count; i++)
				{
					sw.WriteLine("{0},{1}",dataStore1[i],dataStore2[i]);
				}
					
				sw.Close();
				fs.Close();
			}
		}
	}
}
