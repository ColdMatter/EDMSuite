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

namespace EDMPhaseLock
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
		private NationalInstruments.UI.WindowsForms.WaveformGraph phaseGraph;
		private NationalInstruments.UI.WaveformPlot waveformPlot3;
		private NationalInstruments.UI.XAxis xAxis3;
		private NationalInstruments.UI.YAxis yAxis3;
        private System.Windows.Forms.MenuItem menuItem6;
        private System.Windows.Forms.DataVisualization.Charting.Chart deviationChart;
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
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
            this.phaseGraph = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.waveformPlot3 = new NationalInstruments.UI.WaveformPlot();
            this.xAxis3 = new NationalInstruments.UI.XAxis();
            this.yAxis3 = new NationalInstruments.UI.YAxis();
            this.deviationChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.deviationGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.phaseGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.deviationChart)).BeginInit();
            this.SuspendLayout();
            // 
            // deviationGraph
            // 
            this.deviationGraph.Caption = "Deviation from target frequency (Hz, reference-clock based)";
            this.deviationGraph.Location = new System.Drawing.Point(16, 8);
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
            this.waveformPlot1.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.waveformPlot1.PointColor = System.Drawing.Color.Lime;
            this.waveformPlot1.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.waveformPlot1.XAxis = this.xAxis1;
            this.waveformPlot1.YAxis = this.yAxis1;
            // 
            // xAxis1
            // 
            this.xAxis1.Mode = NationalInstruments.UI.AxisMode.StripChart;
            this.xAxis1.Range = new NationalInstruments.UI.Range(0D, 1000D);
            // 
            // outputGraph
            // 
            this.outputGraph.Caption = "Output frequency (kHz, wall-clock based)";
            this.outputGraph.Location = new System.Drawing.Point(16, 192);
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
            this.waveformPlot2.LineColor = System.Drawing.Color.Red;
            this.waveformPlot2.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.waveformPlot2.XAxis = this.xAxis2;
            this.waveformPlot2.YAxis = this.yAxis2;
            // 
            // xAxis2
            // 
            this.xAxis2.Mode = NationalInstruments.UI.AxisMode.StripChart;
            this.xAxis2.Range = new NationalInstruments.UI.Range(0D, 1000D);
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
            // phaseGraph
            // 
            this.phaseGraph.Caption = "Accumulated phase error (degrees)";
            this.phaseGraph.Location = new System.Drawing.Point(16, 376);
            this.phaseGraph.Name = "phaseGraph";
            this.phaseGraph.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.waveformPlot3});
            this.phaseGraph.Size = new System.Drawing.Size(704, 176);
            this.phaseGraph.TabIndex = 2;
            this.phaseGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis3});
            this.phaseGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis3});
            // 
            // waveformPlot3
            // 
            this.waveformPlot3.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.waveformPlot3.PointColor = System.Drawing.Color.Lime;
            this.waveformPlot3.PointStyle = NationalInstruments.UI.PointStyle.EmptyCircle;
            this.waveformPlot3.XAxis = this.xAxis3;
            this.waveformPlot3.YAxis = this.yAxis3;
            // 
            // xAxis3
            // 
            this.xAxis3.Mode = NationalInstruments.UI.AxisMode.StripChart;
            this.xAxis3.Range = new NationalInstruments.UI.Range(0D, 1000D);
            // 
            // yAxis3
            // 
            this.yAxis3.Range = new NationalInstruments.UI.Range(-5D, 5D);
            // 
            // deviationChart
            // 
            chartArea1.BackColor = System.Drawing.Color.Black;
            chartArea1.Name = "ChartArea1";
            this.deviationChart.ChartAreas.Add(chartArea1);
            this.deviationChart.Location = new System.Drawing.Point(726, 8);
            this.deviationChart.Name = "deviationChart";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            series1.Name = "Series1";
            this.deviationChart.Series.Add(series1);
            this.deviationChart.Size = new System.Drawing.Size(147, 176);
            this.deviationChart.TabIndex = 3;
            this.deviationChart.Text = "chart1";
            title1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            title1.Name = "deviationTitle";
            title1.Text = "Deviation from target frequency (Hz, reference-clock based)";
            this.deviationChart.Titles.Add(title1);
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(727, 554);
            this.Controls.Add(this.deviationChart);
            this.Controls.Add(this.phaseGraph);
            this.Controls.Add(this.outputGraph);
            this.Controls.Add(this.deviationGraph);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Menu = this.mainMenu1;
            this.Name = "MainForm";
            this.Text = "Phase Lock";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.MainForm_Closing);
            ((System.ComponentModel.ISupportInitialize)(this.deviationGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.phaseGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.deviationChart)).EndInit();
            this.ResumeLayout(false);

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
			deviationChart.Series[0].Points.Clear();
			deviationGraph.ClearData();
			outputGraph.ClearData();
			phaseGraph.ClearData();
			outputGraph.ClearData();
			deviationGraph.YAxes[0].Range = new Range(GUI_RATE_CENTRE - GUI_RATE_RANGE,
				GUI_RATE_CENTRE + GUI_RATE_RANGE);
		}

		private void UpdateGUI()
		{

			deviationGraph.Plots[0].PlotYAppend(
				(double[])deviationPlotData.ToArray(Type.GetType("System.Double")));
			deviationPlotData.Clear();

			phaseGraph.Plots[0].PlotYAppend(
				(double[])phasePlotData.ToArray(Type.GetType("System.Double")));
			phasePlotData.Clear();

			outputGraph.Plots[0].PlotYAppend(
				(double[])oscillatorPlotData.ToArray(Type.GetType("System.Double")),
				LOCK_UPDATE_EVERY * SAMPLE_MULTI_READ		
				);
			oscillatorPlotData.Clear();

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
			if (!running)
			{
				lockOscillator = true;
				running = true;
				StartAcquisition();
			}
		}

		private void menuItem6_Click(object sender, System.EventArgs e)
		{
			if (!running)
			{
				lockOscillator = false;
				running = true;
				StartAcquisition();
			}
		}

		private void menuItem4_Click(object sender, System.EventArgs e)
		{
			if (running)
			{
				StopAcquisition();
				running = false;
			}
		}

		#endregion

		#region DAQ

        enum ControlMethod { synth, analog, usb };

        ControlMethod cm;
		
        Task counterTask;
		HP3325BSynth redSynth;
		RigolDG811 wavGen;
        Task analogOutputTask;
        AnalogSingleChannelWriter analogWriter;
		CounterSingleChannelReader counterReader;
		bool running = false;
		bool lockOscillator;
		double oscillatorFrequency;

		// constants
		const double SAMPLE_CLOCK_RATE = 50;			// (Hz) this defines what a second is
		const double OSCILLATOR_TARGET_RATE = 1000000;	// (Hz) how many cycles you'd like the
														// fast oscillator to produce in a second (as defined
														// by the SAMPLE_CLOCK_RATE)
		const int SAMPLE_MULTI_READ = 10;		// this is how many samples to read at a time, sets a limit on
												// bandwidth - lower for more bandwidth but higher computer load
		const int GUI_UPDATE_EVERY = 1;			// if you multiply this by SAMPLE_MULTI_READ and divide by
												// SAMPLE_CLOCK_RATE you get the GUI update interval in seconds
		const double GUI_RATE_CENTRE = 0;
		const double GUI_RATE_RANGE = 7500;
		const int LOCK_UPDATE_EVERY = 5;		// this is how often the lock is updated in terms
												// of SAMPLE_MULTI_READs (same idea as GUI update interval above)
		// lock parameters
		const double PROPORTIONAL_GAIN = 1.0;		// the units are Hz per count
		const double DERIVATIVE_GAIN = 25;		// the units are difficult to work out
		const double OSCILLATOR_DEVIATION_LIMIT = 50000;		// this is the furthest the output frequency
															// can deviate from the target frequency
		const double DIFFERENCE_LIMIT = 5000;	// in counts. This times by the sample clock rate
												// is the maximum frequency deviation that will be passed through
												// to the lock. 
        const long LOCK_UNPINNING_STEP = 100; // (Hz) This is a small constant to be fed back if the difference 
                                                // array exceeds the difference limit; it keeps the useful behaviour 
                                                // of the difference limit while at the same preventing the lock from
                                                // unlocking permanently if the frequency genuinely exceeds the difference limt. 
        const double VCO_CENTRAL = 2.15;           // when using a VCO to generate the clock signal, this is the
                                                // input voltage to the VCO that generates the target frequency 

        const double VCO_CAL = 150000;          // The calibration of the VCO, in Hz per V, around the central value
        const double VCO_HIGH = 5;             // upper input range of the VCO
        const double VCO_LOW = 0;               // lower input range of the VCO

		const double USB_AMP = 5;               // The output Vpp for the usb synth
		const double USB_OFFS = 2.5;			// The output voltage should be offset to be between 0 and 5

		private void StartAcquisition()
		{
			if (((string)Environs.Hardware.GetInfo("phaseLockControlMethod")) == "analog")
				cm = ControlMethod.analog;
			else
			{
				if (((string)Environs.Hardware.GetInfo("phaseLockControlMethod")) == "synth")
                {
					cm = ControlMethod.synth; //synth is default
				}
				else cm = ControlMethod.usb;
			}

            if (cm == ControlMethod.synth && !Environs.Debug)
            {
                redSynth = (HP3325BSynth)Environs.Hardware.Instruments["red"];
                redSynth.Connect();
            }
            else redSynth = null;

			if (cm == ControlMethod.usb && !Environs.Debug)
            {
				wavGen = (RigolDG811)Environs.Hardware.Instruments["rigolWavGen"];
				wavGen.Connect();
				wavGen.SquareWave = false;
				wavGen.Amplitude = USB_AMP;
				wavGen.Offset = USB_OFFS;
				wavGen.Enabled = true;
            }

            if (cm == ControlMethod.analog && !Environs.Debug) configureAnalogOutput();

            oscillatorFrequency = OSCILLATOR_TARGET_RATE;
			accumulatedPhaseDifference = 0;
			sampleCounter = 0;
			ClearGUI();
			PrepareDataStores();
			
			if (!Environs.Debug) StartCounter();
			else StartFakeCounter();
		}

        private void configureAnalogOutput()
        {
            analogOutputTask = new Task("phase lock analog output");
			AnalogOutputChannel outputChannel = 
					(AnalogOutputChannel) Environs.Hardware.AnalogOutputChannels["phaseLockAnalogOutput"];
			outputChannel.AddToTask(analogOutputTask, VCO_LOW, VCO_HIGH);
			analogWriter = new AnalogSingleChannelWriter(analogOutputTask.Stream);
			analogWriter.WriteSingleSample(true, VCO_CENTRAL); // start out with the central value
        }

		ArrayList deviationPlotData;
		ArrayList phasePlotData;
		ArrayList oscillatorPlotData;
		ArrayList lockPhaseData;
		private void PrepareDataStores()
		{
			deviationPlotData = new ArrayList(); 
			phasePlotData = new ArrayList();
			oscillatorPlotData = new ArrayList(); 
			lockPhaseData = new ArrayList(); 
		}

		// this method configures and starts the counter.
		private void StartCounter()
		{
			// set up the counter - the fast oscillator is fed into a counter's source input
			counterTask = new Task("PhaseLock task");
			CounterChannel oscillatorChannel =
				((CounterChannel)Environs.Hardware.CounterChannels["phaseLockOscillator"]);
			counterTask.CIChannels.CreateCountEdgesChannel(
				oscillatorChannel.PhysicalChannel,
				oscillatorChannel.Name,
				CICountEdgesActiveEdge.Rising,
				0,
				CICountEdgesCountDirection.Up
				);

			// this counter is sample-clocked by the slow reference that we are locking to.
			// the buffer is set to be "large"
			CounterChannel referenceChannel =
				((CounterChannel)Environs.Hardware.CounterChannels["phaseLockReference"]);

			counterTask.Timing.ConfigureSampleClock(
				referenceChannel.PhysicalChannel,
				SAMPLE_CLOCK_RATE,
				SampleClockActiveEdge.Rising,
				SampleQuantityMode.ContinuousSamples,
				1000
				);

			//counterTask.Stream.Timeout = -1;

			counterReader = new CounterSingleChannelReader(counterTask.Stream);
			counterReader.SynchronizeCallbacks = true;

			if (!Environs.Debug)
				counterReader.BeginReadMultiSampleInt32(
					SAMPLE_MULTI_READ,
					new AsyncCallback(CounterCallBack),
					null
					);
			
		}

		// keep track of gui updates
		private int sampleCounter = 0;

		#region Debug support

		// this is a little bit tricky - because the program is all in one thread
		// and relies on NI-DAQ calling the callback to drive it it fails with simulated
		// devices (because simulated devices never block).
		// The solution is to create new thread which periodically invokes the callback
		// (on the GUI thread !)

		Thread debugDriverThread;
		bool debugAbortFlag = false;
		private void StartFakeCounter()
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

		private double FAKE_DATA_SPREAD = 0.001;
		private double FAKE_DATA_MOD = 0.002;
		private double FAKE_DATA_PERIOD = 1000;
		private Int32 fakeCounterValue = 0;

		private void CounterCallBack(IAsyncResult result)
		{
			// read the latest data from the counter
			Int32[] data;
	
			if (!Environs.Debug)
			{
				Console.WriteLine("Taking data");
                //try
                //{
					data = counterReader.EndReadMultiSampleInt32(result);
                //}
                //           catch (Exception e)
                //           {
                //Console.Write("Exception: " + e.Message);
                //               Random r = new Random();
                //               data = new Int32[SAMPLE_MULTI_READ];
                //               for (int i = 0; i < SAMPLE_MULTI_READ; i++)
                //               {
                //                   data[i] = fakeCounterValue;

                //                   fakeCounterValue += (Int32)((oscillatorFrequency / SAMPLE_CLOCK_RATE) *
                //                       (1 + (FAKE_DATA_SPREAD * (r.NextDouble() - 0.5)) +
                //                       (FAKE_DATA_MOD *
                //                                   Math.Sin(((sampleCounter * SAMPLE_MULTI_READ) + i) / FAKE_DATA_PERIOD))));
                //               }
            //}
            Console.WriteLine("Got data");
			}
			else
			{
				Random r = new Random();
				data = new Int32[SAMPLE_MULTI_READ];
				for (int i = 0 ; i < SAMPLE_MULTI_READ ; i++)
				{
					data[i] = fakeCounterValue;

					fakeCounterValue += (Int32)((oscillatorFrequency / SAMPLE_CLOCK_RATE) *
						( 1 + (FAKE_DATA_SPREAD * (r.NextDouble() - 0.5)) +
						( FAKE_DATA_MOD * 
									Math.Sin( ((sampleCounter * SAMPLE_MULTI_READ) + i) / FAKE_DATA_PERIOD) )));
				}
			}
			//Console.WriteLine("Start new Async");
			// start the counter reading again right away
			if (!Environs.Debug && running)
				counterReader.BeginReadMultiSampleInt32(
					SAMPLE_MULTI_READ,
					new AsyncCallback(CounterCallBack),
					null
					);
			//Console.WriteLine("Finish new async");
			// deal with the data
			StoreData(data);
			//Console.WriteLine("Stored Data");
			if (lockOscillator && (sampleCounter % LOCK_UPDATE_EVERY == 0)) UpdateLock();
			//Console.WriteLine("Updated Lock");
			// update the gui ?
			if (sampleCounter % GUI_UPDATE_EVERY == 0)
			{
				UpdateGUI();
			}
			//Console.WriteLine("Updated GUI");
			sampleCounter++;
		}


		Int32 lastElementOfOldData = 0;
		int accumulatedPhaseDifference = 0;
		private void StoreData(Int32[] data)
		{
			// create an array of the differences
			Int64[] differenceArray = new Int64[data.Length];
			differenceArray[0] = (Int64)data[0] - lastElementOfOldData;
			// cheat for the first point
			if (sampleCounter == 0) differenceArray[0] = (Int64)(OSCILLATOR_TARGET_RATE / SAMPLE_CLOCK_RATE);
			if (differenceArray[0] < 0) differenceArray[0] += (Int64)Math.Pow(2,32);
			for (int i = 1 ; i < differenceArray.Length ; i++)
			{
				differenceArray[i] = (Int64)(data[i] - data[i-1]);
				if (differenceArray[i] < 0) differenceArray[i] += (Int64)Math.Pow(2,32);
			}
			lastElementOfOldData = data[data.Length - 1];

			// calculate deviations of each period from ideal
			Int64[] deviationArray = new Int64[differenceArray.Length];
			Int64 targetStep = (Int64)(OSCILLATOR_TARGET_RATE / SAMPLE_CLOCK_RATE);
			for (int i = 0 ; i < differenceArray.Length ; i++)
			{
				deviationArray[i] = differenceArray[i] - targetStep;
				// this eats glitches before they get to the lock
				if (Math.Abs(deviationArray[i]) > DIFFERENCE_LIMIT) deviationArray[i] = Math.Sign(deviationArray[i]) * LOCK_UNPINNING_STEP; 
			}
			// calculate the accumlated phase difference
			int[] phaseArray = new int[differenceArray.Length];
			for (int i = 0 ; i < differenceArray.Length ; i++)
			{
				accumulatedPhaseDifference += (int)deviationArray[i];
				phaseArray[i] = accumulatedPhaseDifference;
			}
			lockPhaseData.AddRange(phaseArray);

			// create an array of deviations from the target frequency in Hz for the plot
			double[] plotArray = new double[differenceArray.Length];
			for (int i = 0 ; i < differenceArray.Length ; i++)
				plotArray[i] = (differenceArray[i] * SAMPLE_CLOCK_RATE) - OSCILLATOR_TARGET_RATE;
			deviationPlotData.AddRange(plotArray);

			// calculate the phase difference in degrees plot data
			double[] phasePlotArray = new double[phaseArray.Length];
			for (int i = 0 ; i < differenceArray.Length ; i++)
				phasePlotArray[i] = ((double)phaseArray[i] * 360.0) / (double)targetStep;
			phasePlotData.AddRange(phasePlotArray);
		}

		private object lockParameterLockObject = new Object();
		double phaseErrorInDegrees = 10;//changed to non-zero starting value so we can check phase lock is running with blockhead
		private void UpdateLock()
		{
			lock(lockParameterLockObject)
			{
				//calculate the new oscillator frequency
				// calculate the slope
				double[] lockXVals = new double[lockPhaseData.Count];
				for (int i = 0 ; i < lockPhaseData.Count ; i++) lockXVals[i] = i;
				double slope;//, intercept, err;
				ArrayList lockPhaseDataDouble = new ArrayList();
				foreach (int p in lockPhaseData) lockPhaseDataDouble.Add((double)p);
				double[] lockYVals = (double[])lockPhaseDataDouble.ToArray(Type.GetType("System.Double"));
				//CurveFit.LinearFit(lockXVals, lockYVals, FitMethod.LeastSquare, out slope, out intercept, out err);
                slope = (lockYVals[lockYVals.Length - 1] - lockYVals[0]) / (lockXVals[lockXVals.Length - 1] - lockXVals[0]);
				// proportional gain
				oscillatorFrequency -= accumulatedPhaseDifference * PROPORTIONAL_GAIN;
				// derivative gain
				oscillatorFrequency -= slope * DERIVATIVE_GAIN;

				// limit the output swing
				if (oscillatorFrequency > OSCILLATOR_TARGET_RATE + OSCILLATOR_DEVIATION_LIMIT)
					oscillatorFrequency = OSCILLATOR_TARGET_RATE + OSCILLATOR_DEVIATION_LIMIT;
				if (oscillatorFrequency < OSCILLATOR_TARGET_RATE - OSCILLATOR_DEVIATION_LIMIT)
					oscillatorFrequency = OSCILLATOR_TARGET_RATE - OSCILLATOR_DEVIATION_LIMIT;

				// write to the synth or VCO
				if (cm == ControlMethod.synth) redSynth.Frequency = oscillatorFrequency / 1000000;

				if (cm == ControlMethod.usb) wavGen.Frequency = oscillatorFrequency / 1000000;

				if (cm == ControlMethod.analog && !Environs.Debug)
                    analogWriter.WriteSingleSample(true, 
                        VCO_CENTRAL + (oscillatorFrequency - OSCILLATOR_TARGET_RATE) / VCO_CAL);
                
				// update the plot
				phaseErrorInDegrees = (double)phasePlotData[phasePlotData.Count - 1];
				oscillatorPlotData.Add(oscillatorFrequency / 1000);
				lockPhaseData.Clear();
				//Console.WriteLine("UpdatedLock");
			}
		}

		private void StopAcquisition()
		{
			lockOscillator = false;
			if (!Environs.Debug) 
			{
				if (counterTask != null) counterTask.Dispose();
                if (cm == ControlMethod.analog) analogOutputTask.Dispose();
			}
			else lock(this) debugAbortFlag = true;
			if (cm == ControlMethod.synth) redSynth.Disconnect();
			if (cm == ControlMethod.usb)
			{
				wavGen.Enabled = false;
				wavGen.SquareWave = false;
				wavGen.Disconnect();
			}
		}

		#endregion

		#region Remote Methods

		public double OutputFrequency
		{
			get
			{
				lock(lockParameterLockObject)
				{
					return oscillatorFrequency;
				}
			}
		}

		public double PhaseError
		{
			get
			{
				lock(lockParameterLockObject)
				{
					return phaseErrorInDegrees;
				}
			}
		}

		public void Lock()
        {
			if (!running)
			{
				lockOscillator = true;
				running = true;
				StartAcquisition();
			}
		}

		public void Monitor()
		{
			if (!running)
			{
				lockOscillator = false;
				running = true;
				StartAcquisition();
			}
		}

		public void Stop()
        {
			if (running)
			{
				StopAcquisition();
				running = false;
			}
		}

		#endregion

	}
}
