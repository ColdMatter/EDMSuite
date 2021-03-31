using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;
using System.Windows.Forms.DataVisualization.Charting;
using NationalInstruments.Restricted;

namespace EDMPhaseLock2020
{
    public partial class ControlWindow : Form
    {
		#region Private fields and constants
		
		Task counterTask;
		Synth redSynth;
		Task analogOutputTask;
		AnalogSingleChannelWriter analogWriter;
		CounterSingleChannelReader counterReader;
		bool running = false;
		bool lockOscillator;
		double oscillatorFrequency;

		enum ControlMethod { synth, analog };

		ControlMethod cm;

		// constants
		const double SAMPLE_CLOCK_RATE = 50;            // (Hz) this defines what a second is
		const double OSCILLATOR_TARGET_RATE = 1000000;  // (Hz) how many cycles you'd like the
														// fast oscillator to produce in a second (as defined
														// by the SAMPLE_CLOCK_RATE)
		const int SAMPLE_MULTI_READ = 10;       // this is how many samples to read at a time, sets a limit on
												// bandwidth - lower for more bandwidth but higher computer load
		const int GUI_UPDATE_EVERY = 1;         // if you multiply this by SAMPLE_MULTI_READ and divide by
												// SAMPLE_CLOCK_RATE you get the GUI update interval in seconds
		const double GUI_RATE_CENTRE = 0;
		const double GUI_RATE_RANGE = 1500;
		const int LOCK_UPDATE_EVERY = 5;        // this is how often the lock is updated in terms
												// of SAMPLE_MULTI_READs (same idea as GUI update interval above)
												// lock parameters
		const double PROPORTIONAL_GAIN = 1.2;       // the units are Hz per count
		const double DERIVATIVE_GAIN = 50;      // the units are difficult to work out
		const double OSCILLATOR_DEVIATION_LIMIT = 75000;        // this is the furthest the output frequency
																// can deviate from the target frequency
		const double DIFFERENCE_LIMIT = 5000;   // in counts. This times by the sample clock rate
												// is the maximum frequency deviation that will be passed through
												// to the lock. 
		const long LOCK_UNPINNING_STEP = 100; // (Hz) This is a small constant to be fed back if the difference 
											  // array exceeds the difference limit; it keeps the useful behaviour 
											  // of the difference limit while at the same preventing the lock from
											  // unlocking permanently if the frequency genuinely exceeds the difference limt. 
		const double VCO_CENTRAL = 1;           // when using a VCO to generate the clock signal, this is the
												// input voltage to the VCO that generates the target frequency 

		const double VCO_CAL = 100000;          // The calibration of the VCO, in Hz per V, around the central value
		const double VCO_HIGH = 10;             // upper input range of the VCO
		const double VCO_LOW = 0;               // lower input range of the VCO

		const double PLOT_POINTS = 1000;

		#endregion

		#region GUI control
		public ControlWindow()
        {
            InitializeComponent();
        }

		// without this method, any remote connections to this object will time out after
		// five minutes of inactivity.
		// It just overrides the lifetime lease system completely.
		public override Object InitializeLifetimeService()
		{
			return null;
		}

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
			ClearChart(deviationChart, "DeviationSeries");
			ClearChart(outputChart, "OutputSeries");
			ClearChart(phaseErrorChart, "PhaseErrorSeries");
		}

		private void UpdateGUI()
		{
			UpdateChart(deviationChart, "DeviationSeries", deviationPlotData, 1);
			UpdateChart(outputChart, "OutputSeries", oscillatorPlotData, LOCK_UPDATE_EVERY * SAMPLE_MULTI_READ);
			UpdateChart(phaseErrorChart, "PhaseErrorSeries", phasePlotData, 1);
		}

		private void ClearChart(Chart chart, string seriesName)
        {
			chart.Series[seriesName].Points.Clear();
        }

		private void UpdateChart(Chart chart, string seriesName, List<double> list)
        {
			chart.Series[seriesName].Points.SuspendUpdates();

            for (int i = 0; i < list.Count; i++)
            {
                chart.Series[seriesName].Points.AddY(list[i]);
                if (chart.Series[seriesName].Points.Count > PLOT_POINTS) chart.Series[seriesName].Points.RemoveAt(0);
            }

			chart.Series[seriesName].Points.ResumeUpdates();

            chart.ChartAreas[0].RecalculateAxesScale();

			list.Clear();
        }

		private void UpdateChart(Chart chart, string seriesName, List<double> list, double xInc)
		{
			chart.Series[seriesName].Points.SuspendUpdates();

			for (int i = 0; i < list.Count; i++)
			{
				if (chart.Series[seriesName].Points.IsEmpty()) chart.Series[seriesName].Points.AddXY(0, list[i]);
				else chart.Series[seriesName].Points.AddXY(chart.Series[seriesName].Points[chart.Series[seriesName].Points.Count - 1].XValue + xInc, list[i]);

				if (chart.Series[seriesName].Points.Count > PLOT_POINTS / xInc) chart.Series[seriesName].Points.RemoveAt(0);
			}

			chart.Series[seriesName].Points.ResumeUpdates();

			chart.ChartAreas[0].RecalculateAxesScale();

			list.Clear();
		}

		#endregion

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
			for (; ; )
			{
				int sleepPeriod = (int)((1000 * SAMPLE_MULTI_READ) / SAMPLE_CLOCK_RATE);
				Thread.Sleep(sleepPeriod);
				Invoke(new AsyncCallback(CounterCallBack), new object[] { null });
				lock (this) if (debugAbortFlag) return;
			}
		}
		private double FAKE_DATA_SPREAD = 0.001;
		private double FAKE_DATA_MOD = 0.002;
		private double FAKE_DATA_PERIOD = 1000;
		private Int32 fakeCounterValue = 0;

		#endregion

		#region DAQ

		private void StartAcquisition()
		{
			if (((string)Environs.Hardware.GetInfo("phaseLockControlMethod")) == "analog")
				cm = ControlMethod.analog;
			else cm = ControlMethod.synth; //synth is default

			if (cm == ControlMethod.synth)
			{
				redSynth = (Synth)Environs.Hardware.Instruments["red"];
				redSynth.Connect();
			}
			else redSynth = null;

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
					(AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["phaseLockAnalogOutput"];
			outputChannel.AddToTask(analogOutputTask, VCO_LOW, VCO_HIGH);
			analogWriter = new AnalogSingleChannelWriter(analogOutputTask.Stream);
			analogWriter.WriteSingleSample(true, VCO_CENTRAL); // start out with the central value
		}

		List<double> deviationPlotData;
		List<double> phasePlotData;
		List<double> oscillatorPlotData;
		List<double> lockPhaseData;
		private void PrepareDataStores()
		{
			deviationPlotData = new List<double>();
			phasePlotData = new List<double>();
			oscillatorPlotData = new List<double>();
			lockPhaseData = new List<double>();
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

		private void CounterCallBack(IAsyncResult result)
		{
			// read the latest data from the counter
			Int32[] data;

			if (!Environs.Debug)
			{
				data = counterReader.EndReadMultiSampleInt32(result);
			}
			else
			{
				Random r = new Random();
				data = new Int32[SAMPLE_MULTI_READ];
				for (int i = 0; i < SAMPLE_MULTI_READ; i++)
				{
					data[i] = fakeCounterValue;

					fakeCounterValue += (Int32)((oscillatorFrequency / SAMPLE_CLOCK_RATE) *
						(1 + (FAKE_DATA_SPREAD * (r.NextDouble() - 0.5)) +
						(FAKE_DATA_MOD *
									Math.Sin(((sampleCounter * SAMPLE_MULTI_READ) + i) / FAKE_DATA_PERIOD))));
				}
			}

			// start the counter reading again right away
			if (!Environs.Debug && running)
				counterReader.BeginReadMultiSampleInt32(
					SAMPLE_MULTI_READ,
					new AsyncCallback(CounterCallBack),
					null
					);

			// deal with the data
			StoreData(data);

			if (lockOscillator && (sampleCounter % LOCK_UPDATE_EVERY == 0)) UpdateLock();

			// update the gui ?
			if (sampleCounter % GUI_UPDATE_EVERY == 0)
			{
				UpdateGUI();
			}
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
			if (differenceArray[0] < 0) differenceArray[0] += (Int64)Math.Pow(2, 32);
			for (int i = 1; i < differenceArray.Length; i++)
			{
				differenceArray[i] = (Int64)(data[i] - data[i - 1]);
				if (differenceArray[i] < 0) differenceArray[i] += (Int64)Math.Pow(2, 32);
			}
			lastElementOfOldData = data[data.Length - 1];

			// calculate deviations of each period from ideal
			Int64[] deviationArray = new Int64[differenceArray.Length];
			Int64 targetStep = (Int64)(OSCILLATOR_TARGET_RATE / SAMPLE_CLOCK_RATE);
			for (int i = 0; i < differenceArray.Length; i++)
			{
				deviationArray[i] = differenceArray[i] - targetStep;
				// this eats glitches before they get to the lock
				if (Math.Abs(deviationArray[i]) > DIFFERENCE_LIMIT) deviationArray[i] = Math.Sign(deviationArray[i]) * LOCK_UNPINNING_STEP;
			}
			// calculate the accumlated phase difference
			double[] phaseArray = new double[differenceArray.Length];
			for (int i = 0; i < differenceArray.Length; i++)
			{
				accumulatedPhaseDifference += (int)deviationArray[i];
				phaseArray[i] = (double)accumulatedPhaseDifference;
			}
			lockPhaseData.AddRange(phaseArray);

			// create an array of deviations from the target frequency in Hz for the plot
			double[] plotArray = new double[differenceArray.Length];
			for (int i = 0; i < differenceArray.Length; i++)
				plotArray[i] = (differenceArray[i] * SAMPLE_CLOCK_RATE) - OSCILLATOR_TARGET_RATE;
			deviationPlotData.AddRange(plotArray);

			// calculate the phase difference in degrees plot data
			double[] phasePlotArray = new double[phaseArray.Length];
			for (int i = 0; i < differenceArray.Length; i++)
				phasePlotArray[i] = ((double)phaseArray[i] * 360.0) / (double)targetStep;
			phasePlotData.AddRange(phasePlotArray);
		}

		private object lockParameterLockObject = new Object();
		double phaseErrorInDegrees = 10;//changed to non-zero starting value so we can check phase lock is running with blockhead
		private void UpdateLock()
		{
			lock (lockParameterLockObject)
			{
				//calculate the new oscillator frequency
				// calculate the slope
				double[] lockXVals = new double[lockPhaseData.Count];
				for (int i = 0; i < lockPhaseData.Count; i++) lockXVals[i] = i;
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

				if (cm == ControlMethod.analog && !Environs.Debug)
					analogWriter.WriteSingleSample(true,
						VCO_CENTRAL + (oscillatorFrequency - OSCILLATOR_TARGET_RATE) / VCO_CAL);

				// update the plot
				phaseErrorInDegrees = (double)phasePlotData[phasePlotData.Count - 1];
				oscillatorPlotData.Add(oscillatorFrequency / 1000);
				lockPhaseData.Clear();
			}
		}

		private void StopAcquisition()
		{
			if (!Environs.Debug)
			{
				if (counterTask != null) counterTask.Dispose();
				if (cm == ControlMethod.analog) analogOutputTask.Dispose();
			}
			else lock (this) debugAbortFlag = true;
			if (cm == ControlMethod.synth) redSynth.Disconnect();

		}

        #endregion

        #region Menu handlers

        private void ControlWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
			StopAcquisition();
			StopApplication();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
			StopAcquisition();
			StopApplication();
			Close();
		}

        private void monitorToolStripMenuItem_Click(object sender, EventArgs e)
        {
			if (!running)
			{
				lockOscillator = false;
				running = true;
				StartAcquisition();
			}
		}

        private void lockToolStripMenuItem1_Click(object sender, EventArgs e)
        {
			if (!running)
			{
				lockOscillator = true;
				running = true;
				StartAcquisition();
			}
		}

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
			if(running)
			{
				StopAcquisition();
				running = false;
			}
		}

        #endregion
    }
}
