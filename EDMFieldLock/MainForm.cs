using System;
using System.Collections;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using DAQ.Environment;
using DAQ.HAL;
using NationalInstruments.DAQmx;
using NationalInstruments.UI;
//using NationalInstruments.Analysis.Math;

namespace EDMFieldLock
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
		#region GUI and Control Stuff

		private NationalInstruments.UI.XAxis xAxis1;
		private NationalInstruments.UI.YAxis yAxis1;
		private NationalInstruments.UI.WaveformPlot waveformPlot1;
		private NationalInstruments.UI.WindowsForms.WaveformGraph outputGraph;
		private NationalInstruments.UI.WaveformPlot waveformPlot2;
		private NationalInstruments.UI.XAxis xAxis2;
		private NationalInstruments.UI.YAxis yAxis2;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.MenuItem menuItem5;
		private NationalInstruments.UI.WindowsForms.WaveformGraph deviationGraph;
        private System.Windows.Forms.MenuItem menuItem6;
        private TextBox setPointTextBox;
        private Label setPointLabel;
        private Button setPointButton;
        private Label controlParametersLabel;
        private Label pGainLabel;
        private TextBox pGainTextBox;
        private TextBox iGainTextBox;
        private Label iGainLabel;
        private TextBox dGainTextBox;
        private Label dGainLabel;
        private Button controlParametersButton;
        private IContainer components;

		public MainForm()
		{
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

		// without this method, any remote connections to this object will time out after
		// five minutes of inactivity.
		// It just overrides the lifetime lease system completely.
		public override Object InitializeLifetimeService()
		{
			return null;
		}



		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.deviationGraph = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.waveformPlot1 = new NationalInstruments.UI.WaveformPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.outputGraph = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.waveformPlot2 = new NationalInstruments.UI.WaveformPlot();
            this.xAxis2 = new NationalInstruments.UI.XAxis();
            this.yAxis2 = new NationalInstruments.UI.YAxis();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.setPointTextBox = new System.Windows.Forms.TextBox();
            this.setPointLabel = new System.Windows.Forms.Label();
            this.setPointButton = new System.Windows.Forms.Button();
            this.controlParametersLabel = new System.Windows.Forms.Label();
            this.pGainLabel = new System.Windows.Forms.Label();
            this.pGainTextBox = new System.Windows.Forms.TextBox();
            this.iGainTextBox = new System.Windows.Forms.TextBox();
            this.iGainLabel = new System.Windows.Forms.Label();
            this.dGainTextBox = new System.Windows.Forms.TextBox();
            this.dGainLabel = new System.Windows.Forms.Label();
            this.controlParametersButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.deviationGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputGraph)).BeginInit();
            this.SuspendLayout();
            // 
            // deviationGraph
            // 
            this.deviationGraph.Caption = "Deviation from target field (nT)";
            this.deviationGraph.Location = new System.Drawing.Point(16, 37);
            this.deviationGraph.Name = "deviationGraph";
            this.deviationGraph.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.waveformPlot1});
            this.deviationGraph.Size = new System.Drawing.Size(704, 176);
            this.deviationGraph.TabIndex = 0;
            this.deviationGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.deviationGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            // 
            // waveformPlot1
            // 
            this.waveformPlot1.LineColor = System.Drawing.Color.SteelBlue;
            this.waveformPlot1.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.waveformPlot1.PointColor = System.Drawing.Color.Cyan;
            this.waveformPlot1.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.waveformPlot1.XAxis = this.xAxis1;
            this.waveformPlot1.YAxis = this.yAxis1;
            // 
            // xAxis1
            // 
            this.xAxis1.Mode = NationalInstruments.UI.AxisMode.StripChart;
            this.xAxis1.Range = new NationalInstruments.UI.Range(0D, 100D);
            // 
            // yAxis1
            // 
            this.yAxis1.Mode = NationalInstruments.UI.AxisMode.Fixed;
            this.yAxis1.Range = new NationalInstruments.UI.Range(-10D, 10D);
            // 
            // outputGraph
            // 
            this.outputGraph.Caption = "Lock output (V)";
            this.outputGraph.Location = new System.Drawing.Point(16, 221);
            this.outputGraph.Name = "outputGraph";
            this.outputGraph.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.waveformPlot2});
            this.outputGraph.Size = new System.Drawing.Size(704, 176);
            this.outputGraph.TabIndex = 1;
            this.outputGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis2});
            this.outputGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis2});
            // 
            // waveformPlot2
            // 
            this.waveformPlot2.LineColor = System.Drawing.Color.Goldenrod;
            this.waveformPlot2.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.waveformPlot2.PointStyle = NationalInstruments.UI.PointStyle.EmptyCircle;
            this.waveformPlot2.XAxis = this.xAxis2;
            this.waveformPlot2.YAxis = this.yAxis2;
            // 
            // xAxis2
            // 
            this.xAxis2.Mode = NationalInstruments.UI.AxisMode.StripChart;
            this.xAxis2.Range = new NationalInstruments.UI.Range(0D, 100D);
            // 
            // yAxis2
            // 
            this.yAxis2.Range = new NationalInstruments.UI.Range(0D, 5D);
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.menuItem2});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem5});
            this.menuItem1.Text = "File";
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 0;
            this.menuItem5.Text = "Exit";
            this.menuItem5.Click += new System.EventHandler(this.menuItem5_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 1;
            this.menuItem2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem6,
            this.menuItem3,
            this.menuItem4});
            this.menuItem2.Text = "Lock";
            // 
            // menuItem6
            // 
            this.menuItem6.Index = 0;
            this.menuItem6.Text = "Monitor";
            this.menuItem6.Click += new System.EventHandler(this.menuItem6_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 1;
            this.menuItem3.Text = "Lock";
            this.menuItem3.Click += new System.EventHandler(this.menuItem3_Click);
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 2;
            this.menuItem4.Text = "Stop";
            this.menuItem4.Click += new System.EventHandler(this.menuItem4_Click);
            // 
            // setPointTextBox
            // 
            this.setPointTextBox.Location = new System.Drawing.Point(91, 8);
            this.setPointTextBox.Name = "setPointTextBox";
            this.setPointTextBox.Size = new System.Drawing.Size(49, 20);
            this.setPointTextBox.TabIndex = 3;
            this.setPointTextBox.Text = "0";
            // 
            // setPointLabel
            // 
            this.setPointLabel.AutoSize = true;
            this.setPointLabel.Location = new System.Drawing.Point(13, 11);
            this.setPointLabel.Name = "setPointLabel";
            this.setPointLabel.Size = new System.Drawing.Size(74, 13);
            this.setPointLabel.TabIndex = 4;
            this.setPointLabel.Text = "Set point (nT):";
            // 
            // setPointButton
            // 
            this.setPointButton.Location = new System.Drawing.Point(145, 7);
            this.setPointButton.Name = "setPointButton";
            this.setPointButton.Size = new System.Drawing.Size(52, 23);
            this.setPointButton.TabIndex = 5;
            this.setPointButton.Text = "Update";
            this.setPointButton.UseVisualStyleBackColor = true;
            this.setPointButton.Click += new System.EventHandler(this.setPointButton_Click);
            // 
            // controlParametersLabel
            // 
            this.controlParametersLabel.AutoSize = true;
            this.controlParametersLabel.Location = new System.Drawing.Point(214, 12);
            this.controlParametersLabel.Name = "controlParametersLabel";
            this.controlParametersLabel.Size = new System.Drawing.Size(99, 13);
            this.controlParametersLabel.TabIndex = 6;
            this.controlParametersLabel.Text = "Control Parameters:";
            // 
            // pGainLabel
            // 
            this.pGainLabel.AutoSize = true;
            this.pGainLabel.Location = new System.Drawing.Point(319, 12);
            this.pGainLabel.Name = "pGainLabel";
            this.pGainLabel.Size = new System.Drawing.Size(14, 13);
            this.pGainLabel.TabIndex = 7;
            this.pGainLabel.Text = "P";
            // 
            // pGainTextBox
            // 
            this.pGainTextBox.Location = new System.Drawing.Point(339, 9);
            this.pGainTextBox.Name = "pGainTextBox";
            this.pGainTextBox.Size = new System.Drawing.Size(49, 20);
            this.pGainTextBox.TabIndex = 8;
            this.pGainTextBox.Text = "-80";
            // 
            // iGainTextBox
            // 
            this.iGainTextBox.Location = new System.Drawing.Point(422, 9);
            this.iGainTextBox.Name = "iGainTextBox";
            this.iGainTextBox.Size = new System.Drawing.Size(49, 20);
            this.iGainTextBox.TabIndex = 10;
            this.iGainTextBox.Text = "-60";
            // 
            // iGainLabel
            // 
            this.iGainLabel.AutoSize = true;
            this.iGainLabel.Location = new System.Drawing.Point(402, 12);
            this.iGainLabel.Name = "iGainLabel";
            this.iGainLabel.Size = new System.Drawing.Size(10, 13);
            this.iGainLabel.TabIndex = 9;
            this.iGainLabel.Text = "I";
            // 
            // dGainTextBox
            // 
            this.dGainTextBox.Location = new System.Drawing.Point(511, 8);
            this.dGainTextBox.Name = "dGainTextBox";
            this.dGainTextBox.Size = new System.Drawing.Size(49, 20);
            this.dGainTextBox.TabIndex = 12;
            this.dGainTextBox.Text = "0";
            // 
            // dGainLabel
            // 
            this.dGainLabel.AutoSize = true;
            this.dGainLabel.Location = new System.Drawing.Point(491, 11);
            this.dGainLabel.Name = "dGainLabel";
            this.dGainLabel.Size = new System.Drawing.Size(15, 13);
            this.dGainLabel.TabIndex = 11;
            this.dGainLabel.Text = "D";
            // 
            // controlParametersButton
            // 
            this.controlParametersButton.Location = new System.Drawing.Point(571, 7);
            this.controlParametersButton.Name = "controlParametersButton";
            this.controlParametersButton.Size = new System.Drawing.Size(52, 23);
            this.controlParametersButton.TabIndex = 13;
            this.controlParametersButton.Text = "Update";
            this.controlParametersButton.UseVisualStyleBackColor = true;
            this.controlParametersButton.Click += new System.EventHandler(this.controlParametersButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(736, 405);
            this.Controls.Add(this.controlParametersButton);
            this.Controls.Add(this.dGainTextBox);
            this.Controls.Add(this.dGainLabel);
            this.Controls.Add(this.iGainTextBox);
            this.Controls.Add(this.iGainLabel);
            this.Controls.Add(this.pGainTextBox);
            this.Controls.Add(this.pGainLabel);
            this.Controls.Add(this.controlParametersLabel);
            this.Controls.Add(this.setPointButton);
            this.Controls.Add(this.setPointLabel);
            this.Controls.Add(this.setPointTextBox);
            this.Controls.Add(this.outputGraph);
            this.Controls.Add(this.deviationGraph);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Menu = this.mainMenu1;
            this.Name = "MainForm";
            this.Text = "Magnetic Field Lock";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.MainForm_Closing);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.deviationGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputGraph)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		public void StartApplication() 
		{
			Application.Run(this);
		}

		private void StopApplication()
		{
			if (running) StopAcquisition();
		}

		private void ClearGUI()
		{
			deviationGraph.ClearData();
			outputGraph.ClearData();
		}

		private void UpdateGUI()
		{
			deviationGraph.Plots[0].PlotYAppend((double[])meanDeviationData.ToArray(Type.GetType("System.Double")));
			deviationPlotData.Clear();
			meanDeviationData.Clear();

			outputGraph.Plots[0].PlotYAppend((double[])outputPlotData.ToArray(Type.GetType("System.Double")), LOCK_UPDATE_EVERY);
			outputPlotData.Clear();

		}

		
		#endregion

		#region Menu Handlers

		private void menuItem5_Click(object sender, System.EventArgs e)
		{
			StopApplication();
			Close();
		}

		private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			StopApplication();
		}

		private void menuItem3_Click(object sender, System.EventArgs e)
		{
			LockField();
		}

		private void menuItem6_Click(object sender, System.EventArgs e)
		{
			MonitorField();
		}

		private void menuItem4_Click(object sender, System.EventArgs e)
		{
			StopFieldLock();
		}

		private void setPointButton_Click(object sender, EventArgs e)
		{
			setPoint = Convert.ToDouble(setPointTextBox.Text) / FIELD_PER_VOLT_INPUT;
		}

		private void controlParametersButton_Click(object sender, EventArgs e)
		{
			proportionalGain = Convert.ToDouble(pGainTextBox.Text);
			integralGain = Convert.ToDouble(iGainTextBox.Text);
			derivativeGain = Convert.ToDouble(dGainTextBox.Text);
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			StopAcquisition();
		}

		#endregion

		#region DAQ

		Task analogInputTask;
		Task analogOutputTask;
		AnalogSingleChannelReader analogReader;
		AnalogSingleChannelWriter analogWriter;

		bool running = false;
		bool lockField;
		bool reset = false;

		// keep track of gui updates
		private int sampleCounter = 0;

		// constants
		const double FIELD_PER_VOLT_INPUT = 1000;		// in units of nT/V
		const int SAMPLE_CLOCK_RATE = 50;
		const int SAMPLE_MULTI_READ = 25;				// this is how many samples to read at a time, sets a limit on bandwidth - lower for more bandwidth but higher computer load
		const int GUI_UPDATE_EVERY = 1;					// if you multiply this by SAMPLE_MULTI_READ and divide by SAMPLE_CLOCK_RATE you get the GUI update interval in seconds
		const int LOCK_UPDATE_EVERY = 1;                // this is how often the lock is updated in terms of SAMPLE_MULTI_READs (same idea as GUI update interval above)
		const double OUTPUT_LIMIT_LO = 0;               // (V). this sets the output limit.
		const double OUTPUT_LIMIT_HI = 5;               // (V). this sets the output limit.
		const double OUTPUT_ZERO = 2.5;					// (V). this sets the zero output level.
		const double INPUT_LOW = -10;					// lower limit to Bartington input
		const double INPUT_HIGH = 10;					// upper limit to Bartington input
		const int OUTPUT_RAMP_STEPS = 50;			
		const int OUTPUT_RAMP_DELAY = 100;
		const int CURRENT_SETTLE_TIME = 1000;

		double setPoint = 0.0;							// in volts
		double lockOutput = OUTPUT_ZERO;				// in volts
		double proportionalGain = 1.0;
		double integralGain = 0.0;
		double derivativeGain = 0.0;
		double lastDeviation = 0.0;
		double lastIntegral = 0.0;
		double lastOutput = 0.0;

		private void StartAcquisition()
		{
            ConfigureAnalogOutput();
			ConfigureAnalogInput();

			proportionalGain = Convert.ToDouble(pGainTextBox.Text);
			integralGain = Convert.ToDouble(iGainTextBox.Text);
			derivativeGain = Convert.ToDouble(dGainTextBox.Text);

			InitialiseSetPoint();

			ClearGUI();
			PrepareDataStores();
			
			if (!Environs.Debug) StartReader();
			else StartFakeReader();
		}

        private void ConfigureAnalogOutput()
        {
            if (!Environs.Debug)
            {
				analogOutputTask = new Task("field lock analog output");
				AnalogOutputChannel outputChannel = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["bFieldFeedbackOutput"];
				outputChannel.AddToTask(analogOutputTask, OUTPUT_LIMIT_LO, OUTPUT_LIMIT_HI);
				analogWriter = new AnalogSingleChannelWriter(analogOutputTask.Stream);
				analogWriter.WriteSingleSample(true, OUTPUT_ZERO); // start out with no current
			}
        }

		private void ConfigureAnalogInput()
        {
			if (!Environs.Debug)
			{
				analogInputTask = new Task("field lock analog input");
				AnalogInputChannel inputChannel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["bFieldFeedbackInput"];
				CounterChannel clockChannel = ((CounterChannel)Environs.Hardware.CounterChannels["bFieldFeedbackClock"]);
				inputChannel.AddToTask(analogInputTask, INPUT_LOW, INPUT_HIGH);
				analogReader = new AnalogSingleChannelReader(analogInputTask.Stream)
				{
					SynchronizeCallbacks = true
				};
				analogInputTask.Timing.ConfigureSampleClock(
					clockChannel.PhysicalChannel,
					SAMPLE_CLOCK_RATE,
					SampleClockActiveEdge.Falling,
					SampleQuantityMode.ContinuousSamples
					);
			}
			
		}

		private void InitialiseSetPoint()
        {
			if (!Environs.Debug)
			{
				double[] dataArray = analogReader.ReadMultiSample(SAMPLE_MULTI_READ);
				double dataMean = 0.0;
				for (int i = 0; i < dataArray.Length; i++) dataMean += dataArray[i] / dataArray.Length;
				setPoint = dataMean;
				setPointTextBox.Text = ((int)(setPoint * FIELD_PER_VOLT_INPUT)).ToString();
			}
		}

		ArrayList deviationPlotData;
		ArrayList meanDeviationData;
		ArrayList outputPlotData;
		ArrayList lockFieldData;
		private void PrepareDataStores()
		{
			deviationPlotData = new ArrayList();
			meanDeviationData = new ArrayList();
			outputPlotData = new ArrayList();
			lockFieldData = new ArrayList();
		}

		private void StartReader()
		{
			if (!Environs.Debug)
				analogReader.BeginReadMultiSample(
					SAMPLE_MULTI_READ,
					new AsyncCallback(CounterCallBack),
					null
					);
			
		}

		#region Debug support

		// This is a little bit tricky - because the program is all in one thread and relies on NI-DAQ calling the callback to drive it it fails with simulated devices (because simulated devices never block).
		// The solution is to create new thread which periodically invokes the callback (on the GUI thread !)

		Thread debugDriverThread;
		bool debugAbortFlag = false;
		private void StartFakeReader()
		{
			debugAbortFlag = false;
			debugDriverThread = new Thread(new ThreadStart(DebugDriver));
			debugDriverThread.Start();
		}

		private void DebugDriver()
		{
			for (;;)
			{
				int sleepPeriod = (int)((1000 * SAMPLE_MULTI_READ) / SAMPLE_CLOCK_RATE);
				Thread.Sleep( sleepPeriod );
				Invoke(new AsyncCallback(CounterCallBack), new object[] {null});
				lock(this) if (debugAbortFlag) return;
			}
		}

		#endregion

		private void CounterCallBack(IAsyncResult result)
		{
			// read the latest data from the analog input
			double[] data;
	
			if (!Environs.Debug)
			{
				data = analogReader.EndReadMultiSample(result);
			}
			else
			{
				Random r = new Random();
				data = new double[SAMPLE_MULTI_READ];
				for (int i = 0 ; i < SAMPLE_MULTI_READ ; i++)
				{
					data[i] = 2 * r.NextDouble() - 1;
				}
			}
			
			// start the counter reading again right away
			if (!Environs.Debug && running)
				analogReader.BeginReadMultiSample(
					SAMPLE_MULTI_READ,
					new AsyncCallback(CounterCallBack),
					null
					);

			// if reset, update set point
			if (reset) ResetLockPoint(data);

			// deal with the data
			StoreData(data);

			if (lockField && (sampleCounter % LOCK_UPDATE_EVERY == 0) && running) UpdateLock();

			// update the gui ?
			if (sampleCounter % GUI_UPDATE_EVERY == 0)
			{
				UpdateGUI();
			}
			sampleCounter++;
		}

		private void StoreData(double[] data)
		{
			// create an array of the deviations of each point from the target
			double[] deviationArray = new double[data.Length];
			for (int i = 0; i < deviationArray.Length; i++) deviationArray[i] = data[i] - setPoint;
			lockFieldData.AddRange(deviationArray);

			// convert voltage into a field for plotting
			double[] fieldArray = new double[deviationArray.Length];
			for (int j = 0; j < deviationArray.Length; j++) fieldArray[j] = deviationArray[j] * FIELD_PER_VOLT_INPUT;
			deviationPlotData.AddRange(fieldArray);

			// add to averaged deviation array
			double mean = 0.0;
			for (int k = 0; k < fieldArray.Length; k++) mean += fieldArray[k] / fieldArray.Length;
			meanDeviationData.Add(mean);
		}

		private void ResetLockPoint(double[] data)
        {
			double mean = 0.0;
			for (int i = 0; i < data.Length; i++) mean += data[i] / data.Length;
			setPoint = mean;
			setPointTextBox.Text = ((int)(setPoint * FIELD_PER_VOLT_INPUT)).ToString();
			reset = false;
        }

		private object lockParameterLockObject = new Object();
		private void UpdateLock()
		{
			lock(lockParameterLockObject)
			{
				double meanDeviation = 0.0;
				foreach (double j in lockFieldData) meanDeviation += j / lockFieldData.Count;
				
				double p, i, d, dt;
				dt = (double)LOCK_UPDATE_EVERY * (double)SAMPLE_MULTI_READ / (double)SAMPLE_CLOCK_RATE; // Time between this lock update and the last
				p = - proportionalGain * meanDeviation;
				i = - integralGain * meanDeviation * dt + lastIntegral;
				d = - derivativeGain * (meanDeviation - lastDeviation) / dt;
				lockOutput = p + i + d + OUTPUT_ZERO;

				// if reached voltage limit, reset lock point, otherwise update lock
				if (lockOutput >= OUTPUT_LIMIT_HI || lockOutput <= OUTPUT_LIMIT_LO)
				{
					reset = true;

					double[] outputRamp = new double[OUTPUT_RAMP_STEPS];
					for (int k = 0; k < OUTPUT_RAMP_STEPS; k++) outputRamp[k] = lastOutput - (((double)k + 1) * (lastOutput - OUTPUT_ZERO) / (double)OUTPUT_RAMP_STEPS);
					for (int m = 0; m < outputRamp.Length; m++)
                    {
						analogWriter.WriteSingleSample(true, outputRamp[m]);
						Thread.Sleep(OUTPUT_RAMP_DELAY);
                    }
					Thread.Sleep(CURRENT_SETTLE_TIME);

					lastOutput = 0.0;
					lastDeviation = 0.0;
					lastIntegral = 0.0;
				}

                else
                {
					if (!Environs.Debug) analogWriter.WriteSingleSample(true, lockOutput);
					lastOutput = lockOutput;
					lastDeviation = meanDeviation;
					lastIntegral = i;
				}

				// update stored data
				outputPlotData.Add(lockOutput);
				lockFieldData.Clear();
			}
		}

		private void StopAcquisition()
		{
			if (!Environs.Debug) 
			{
                lock (lockParameterLockObject)
                {
					if (analogInputTask != null)
					{
						analogInputTask.Dispose();
					}
					if (analogOutputTask != null)
					{
						if(running) analogWriter.WriteSingleSample(true, OUTPUT_ZERO);
						analogOutputTask.Dispose();
					}
				}
			}
			else lock(lockParameterLockObject) debugAbortFlag = true;           
		}

        #endregion

        #region Remote Methods

		public void MonitorField()
        {
			if (!running)
			{
				lockField = false;
				running = true;
				StartAcquisition();
			}
		}

		public void LockField()
        {
			if (!running)
			{
				lockField = true;
				running = true;
				StartAcquisition();
				Thread.Sleep(1000); // to let the lock start
			}
		}

		public void StopFieldLock()
        {
			if (running)
			{
				StopAcquisition();
				running = false;
				Thread.Sleep(1000); // to let the currents settle
			}
		}

        #endregion
    }
}
