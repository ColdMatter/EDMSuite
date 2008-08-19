using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;

namespace EDMBlockHead.GUI
{
	/// <summary>
	/// BlockHead's user interface.
	/// </summary>
	public class MainWindow : System.Windows.Forms.Form
	{
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem startMenuItem;
		private System.Windows.Forms.MenuItem stopMenuItem;
        private System.Windows.Forms.MenuItem exitMenuItem;
        private IContainer components;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.StatusBar statusBar;
		private System.Windows.Forms.MenuItem fileMenu;
		private System.Windows.Forms.MenuItem acquireMenu;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem menuItem5;
		private NationalInstruments.UI.XAxis xAxis1;
		private NationalInstruments.UI.YAxis yAxis1;
		public NationalInstruments.UI.WindowsForms.Tank progressTank;
		public System.Windows.Forms.TextBox textArea;
		private NationalInstruments.UI.WindowsForms.WaveformGraph tofGraph1;
		private NationalInstruments.UI.WaveformPlot tofPlot1;
		private System.Windows.Forms.MenuItem menuItem6;
		private System.Windows.Forms.MenuItem menuItem7;
        private WaveformGraph tofGraph2;
        private WaveformPlot tofPlot2;
        private XAxis xAxis2;
        private YAxis yAxis2;
        private WaveformGraph tofGraph3;
        private WaveformPlot tofPlot3;
        private XAxis xAxis3;
        private YAxis yAxis3;

		private Controller controller;

		public MainWindow(Controller controller)
		{
			this.controller = controller;
			InitializeComponent();
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
            this.components = new System.ComponentModel.Container();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.fileMenu = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.exitMenuItem = new System.Windows.Forms.MenuItem();
            this.acquireMenu = new System.Windows.Forms.MenuItem();
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.startMenuItem = new System.Windows.Forms.MenuItem();
            this.stopMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.statusBar = new System.Windows.Forms.StatusBar();
            this.tofGraph1 = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.tofPlot1 = new NationalInstruments.UI.WaveformPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.progressTank = new NationalInstruments.UI.WindowsForms.Tank();
            this.textArea = new System.Windows.Forms.TextBox();
            this.tofGraph2 = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.tofPlot2 = new NationalInstruments.UI.WaveformPlot();
            this.xAxis2 = new NationalInstruments.UI.XAxis();
            this.yAxis2 = new NationalInstruments.UI.YAxis();
            this.tofGraph3 = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.tofPlot3 = new NationalInstruments.UI.WaveformPlot();
            this.xAxis3 = new NationalInstruments.UI.XAxis();
            this.yAxis3 = new NationalInstruments.UI.YAxis();
            ((System.ComponentModel.ISupportInitialize)(this.tofGraph1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressTank)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tofGraph2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tofGraph3)).BeginInit();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.fileMenu,
            this.acquireMenu});
            // 
            // fileMenu
            // 
            this.fileMenu.Index = 0;
            this.fileMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem3,
            this.menuItem4,
            this.menuItem2,
            this.menuItem1,
            this.menuItem5,
            this.exitMenuItem});
            this.fileMenu.Text = "File";
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 0;
            this.menuItem3.Text = "Load block config ...";
            this.menuItem3.Click += new System.EventHandler(this.LoadConfigHandler);
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 1;
            this.menuItem4.Text = "Save block config ...";
            this.menuItem4.Click += new System.EventHandler(this.saveConfigHandler);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 2;
            this.menuItem2.Text = "-";
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 3;
            this.menuItem1.Text = "Save block ...";
            this.menuItem1.Click += new System.EventHandler(this.SaveBlockHandler);
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 4;
            this.menuItem5.Text = "-";
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Index = 5;
            this.exitMenuItem.Text = "Exit";
            this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
            // 
            // acquireMenu
            // 
            this.acquireMenu.Index = 1;
            this.acquireMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem6,
            this.startMenuItem,
            this.stopMenuItem,
            this.menuItem7});
            this.acquireMenu.Text = "Acquire";
            // 
            // menuItem6
            // 
            this.menuItem6.Index = 0;
            this.menuItem6.Text = "Start pattern";
            this.menuItem6.Click += new System.EventHandler(this.menuItem6_Click);
            // 
            // startMenuItem
            // 
            this.startMenuItem.Index = 1;
            this.startMenuItem.Text = "Start block";
            this.startMenuItem.Click += new System.EventHandler(this.startMenuItem_Click);
            // 
            // stopMenuItem
            // 
            this.stopMenuItem.Index = 2;
            this.stopMenuItem.Text = "Stop block";
            this.stopMenuItem.Click += new System.EventHandler(this.stopMenuItem_Click);
            // 
            // menuItem7
            // 
            this.menuItem7.Index = 3;
            this.menuItem7.Text = "Stop pattern";
            this.menuItem7.Click += new System.EventHandler(this.menuItem7_Click);
            // 
            // statusBar
            // 
            this.statusBar.Location = new System.Drawing.Point(0, 437);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(925, 22);
            this.statusBar.SizingGrip = false;
            this.statusBar.TabIndex = 0;
            this.statusBar.Text = "Ready.";
            // 
            // tofGraph1
            // 
            this.tofGraph1.Location = new System.Drawing.Point(8, 8);
            this.tofGraph1.Name = "tofGraph1";
            this.tofGraph1.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.tofPlot1});
            this.tofGraph1.Size = new System.Drawing.Size(228, 362);
            this.tofGraph1.TabIndex = 1;
            this.tofGraph1.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.tofGraph1.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            this.tofGraph1.PlotDataChanged += new NationalInstruments.UI.XYPlotDataChangedEventHandler(this.tofGraph1_PlotDataChanged);
            // 
            // tofPlot1
            // 
            this.tofPlot1.XAxis = this.xAxis1;
            this.tofPlot1.YAxis = this.yAxis1;
            // 
            // yAxis1
            // 
            this.yAxis1.Mode = NationalInstruments.UI.AxisMode.Fixed;
            this.yAxis1.Range = new NationalInstruments.UI.Range(0, 5);
            // 
            // progressTank
            // 
            this.progressTank.FillColor = System.Drawing.Color.DeepPink;
            this.progressTank.FillStyle = NationalInstruments.UI.FillStyle.ZigZag;
            this.progressTank.Location = new System.Drawing.Point(0, 376);
            this.progressTank.Name = "progressTank";
            this.progressTank.Range = new NationalInstruments.UI.Range(0, 4096);
            this.progressTank.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.progressTank.ScalePosition = NationalInstruments.UI.NumericScalePosition.Bottom;
            this.progressTank.Size = new System.Drawing.Size(925, 55);
            this.progressTank.TabIndex = 2;
            // 
            // textArea
            // 
            this.textArea.BackColor = System.Drawing.Color.Black;
            this.textArea.ForeColor = System.Drawing.Color.Lime;
            this.textArea.Location = new System.Drawing.Point(710, 8);
            this.textArea.Multiline = true;
            this.textArea.Name = "textArea";
            this.textArea.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.textArea.Size = new System.Drawing.Size(203, 362);
            this.textArea.TabIndex = 3;
            // 
            // tofGraph2
            // 
            this.tofGraph2.Location = new System.Drawing.Point(242, 8);
            this.tofGraph2.Name = "tofGraph2";
            this.tofGraph2.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.tofPlot2});
            this.tofGraph2.Size = new System.Drawing.Size(228, 362);
            this.tofGraph2.TabIndex = 4;
            this.tofGraph2.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis2});
            this.tofGraph2.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis2});
            // 
            // tofPlot2
            // 
            this.tofPlot2.LineColor = System.Drawing.Color.LightSkyBlue;
            this.tofPlot2.XAxis = this.xAxis2;
            this.tofPlot2.YAxis = this.yAxis2;
            // 
            // yAxis2
            // 
            this.yAxis2.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // tofGraph3
            // 
            this.tofGraph3.Location = new System.Drawing.Point(476, 8);
            this.tofGraph3.Name = "tofGraph3";
            this.tofGraph3.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.tofPlot3});
            this.tofGraph3.Size = new System.Drawing.Size(228, 362);
            this.tofGraph3.TabIndex = 5;
            this.tofGraph3.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis3});
            this.tofGraph3.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis3});
            // 
            // tofPlot3
            // 
            this.tofPlot3.LineColor = System.Drawing.Color.Red;
            this.tofPlot3.XAxis = this.xAxis3;
            this.tofPlot3.YAxis = this.yAxis3;
            // 
            // yAxis3
            // 
            this.yAxis3.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // MainWindow
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(925, 459);
            this.Controls.Add(this.tofGraph3);
            this.Controls.Add(this.tofGraph2);
            this.Controls.Add(this.textArea);
            this.Controls.Add(this.progressTank);
            this.Controls.Add(this.tofGraph1);
            this.Controls.Add(this.statusBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Menu = this.mainMenu1;
            this.Name = "MainWindow";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "BlockHead";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.MainWindow_Closing);
            ((System.ComponentModel.ISupportInitialize)(this.tofGraph1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressTank)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tofGraph2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tofGraph3)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		#region GUI Methods (thread safe)

		public String StatusText
		{
			set
			{
				statusBar.Invoke(new SetTextDelegate(SetText),new object[] {statusBar,value});
			}
		}

		public double TankLevel
		{
			set
			{
				progressTank.BeginInvoke(new SetTankDelegate(SetTank), new object[] {progressTank, value});
			}
		}

		public void PlotTOF(int tofIndex, double[] data, double start, double inc)
		{
            if (tofIndex == 0) PlotY(tofGraph1, tofPlot1, data, start, inc);
            if (tofIndex == 1) PlotY(tofGraph2, tofPlot2, data, start, inc);
            if (tofIndex == 2) PlotY(tofGraph3, tofPlot3, data, start, inc);
        }
		
		public void EnableMenus(bool enabled)
		{
			fileMenu.Enabled = enabled;
			acquireMenu.Enabled = enabled;
		}

		public void AppendToTextArea(string text)
		{
			textArea.Invoke(new AppendTextDelegate(AppendTextHelper), new object[] {textArea, text});
		}

		// thread-safe helpers
		private delegate void SetTextDelegate(StatusBar bar, String text);
		private void SetText(StatusBar bar, String text)
		{
			bar.Text = text;
		}

		private delegate void SetTankDelegate(Tank tank, double val);
		private void SetTank(Tank tank, double val)
		{
			tank.Value = val;
		}

		private delegate void PlotYDelegate(double[] yData, double start, double inc);
		private void PlotY(Graph graph, WaveformPlot p, double[] ydata, double start, double inc) 
		{
			graph.BeginInvoke(new PlotYDelegate(p.PlotY), new Object[] {ydata, start, inc});
		}

		private delegate void AppendTextDelegate(TextBox box, string text);
		private void AppendTextHelper(TextBox box, string text)
		{
			box.AppendText(text + Environment.NewLine);
		}

		#endregion

		#region Event Handlers

		private void exitMenuItem_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			controller.StopApplication();
		}

		private void startMenuItem_Click(object sender, System.EventArgs e)
		{
			controller.StartAcquisition();
		}

		private void stopMenuItem_Click(object sender, System.EventArgs e)
		{
			controller.StopAcquisition();
		}


		private void LoadConfigHandler(object sender, System.EventArgs e)
		{
			controller.LoadConfig();
		}

		private void saveConfigHandler(object sender, System.EventArgs e)
		{
			controller.SaveConfig();		
		}

		private void SaveBlockHandler(object sender, System.EventArgs e)
		{
			controller.SaveBlock();
		}

		private void menuItem6_Click(object sender, System.EventArgs e)
		{
			controller.StartPattern();
		}

		private void menuItem7_Click(object sender, System.EventArgs e)
		{
			controller.StopPattern();
		}

		#endregion

        private void tofGraph1_PlotDataChanged(object sender, XYPlotDataChangedEventArgs e)
        {

        }


    }
}
