using System;
using System.Collections;
using System.Threading;
using System.Runtime.Remoting.Lifetime;
using System.Windows.Forms;
using NationalInstruments;
using NationalInstruments.DAQmx;
using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;
using NationalInstruments.VisaNS;

using DAQ.HAL;
using DAQ.Environment;

namespace EDMHardwareControl
{
	/// <summary>
	/// This is the interface to the edm specific hardware.
	/// 
	/// Everything is just bundled into a single
	/// class. The methods/properties are grouped: the first set change the state of the hardware, these
	/// usually act immediately, but sometimes you need to call an update method. Read the code to find out
	/// which are which. The second set of methods read out the state of the hardware. These invariably need
	/// to be brought up to date with an update method before use.
	/// </summary>
	public class Controller : MarshalByRefObject
	{
		#region Constants
		private const double greenSynthOffAmplitude = -130.0;
		private const double redSynthOffFrequency = 36.0;
		private const int eDischargeTime = 5000;
		private const int eBleedTime = 1000;
		private const int eWaitTime = 500;
		private const int eChargeTime = 5000;
        // E field monitor scale factors - what you need to multiply the monitor voltage by
        // to get the plate voltage
        public double CPlusMonitorScale { get { return 10000; } }
        public double CMinusMonitorScale { get { return 10000; } }
        // E field controller mode
		/*private enum EFieldMode { TTL, GPIB };
		private EFieldMode eFieldMode = EFieldMode.TTL;*/
        //Current Leakage Monitor calibration 
        //Convention for monitor to plate mapping:
        //north -> monitor1
        //south -> monitor2
        private static double northSlope = 1;
        private static double southSlope = 1;
        private static double northOffset = 0;
        private static double southOffset = 0;
        private static double currentMonitorMeasurementTime = .5;



		#endregion

		#region Setup

		// hardware
		HP8657ASynth greenSynth = (HP8657ASynth)Environs.Hardware.GPIBInstruments["green"];
		Synth redSynth = (Synth)Environs.Hardware.GPIBInstruments["red"];
		ICS4861A voltageController = (ICS4861A)Environs.Hardware.GPIBInstruments["4861"];
		HP34401A bCurrentMeter = (HP34401A)Environs.Hardware.GPIBInstruments["bCurrentMeter"];
        EIP575 rfCounter = (EIP575)Environs.Hardware.GPIBInstruments["rfCounter"];
        Hashtable digitalTasks = new Hashtable();
		LeakageMonitor northLeakageMonitor =
            new LeakageMonitor((CounterChannel)Environs.Hardware.CounterChannels["northLeakage"], northSlope, northOffset, currentMonitorMeasurementTime);
		LeakageMonitor southLeakageMonitor =
            new LeakageMonitor((CounterChannel)Environs.Hardware.CounterChannels["southLeakage"], southSlope, southOffset, currentMonitorMeasurementTime);
      	BrilliantLaser yag = (BrilliantLaser)Environs.Hardware.YAG;
		Task bBoxAnalogOutputTask;
		Task rf1AttenuatorOutputTask;
		Task rf2AttenuatorOutputTask;
        Task rf1FMOutputTask;
        Task rf2FMOutputTask;
		Task probeMonitorInputTask;
		Task pumpMonitorInputTask;
        Task cPlusOutputTask;
        Task cMinusOutputTask;
        Task cPlusMonitorInputTask;
        Task cMinusMonitorInputTask;
        Task rfPowerMonitorInputTask;

		ControlWindow window;

		// without this method, any remote connections to this object will time out after
		// five minutes of inactivity.
		// It just overrides the lifetime lease system completely.
		public override Object InitializeLifetimeService()
		{
			return null;
		}
		
		
		public void Start()
		{
			// make the digital tasks
			CreateDigitalTask("notEOnOff");
			CreateDigitalTask("eOnOff");
			CreateDigitalTask("ePol");
			CreateDigitalTask("notEPol");
			CreateDigitalTask("eBleed");
			CreateDigitalTask("rfSwitch");
			CreateDigitalTask("fmSelect");
            CreateDigitalTask("attenuatorSelect");
			CreateDigitalTask("b");
			CreateDigitalTask("notDB");
			CreateDigitalTask("piFlip");
			CreateDigitalTask("piFlipEnable");
			CreateDigitalTask("pumpShutter");
			CreateDigitalTask("pump2Shutter");

			// initialise the current leakage monitors
			northLeakageMonitor.Initialize();
			southLeakageMonitor.Initialize();


			// analog outputs
			bBoxAnalogOutputTask = CreateAnalogOutputTask("b");
			rf1AttenuatorOutputTask = CreateAnalogOutputTask("rf1Attenuator");
			rf2AttenuatorOutputTask = CreateAnalogOutputTask("rf2Attenuator");
            rf1FMOutputTask = CreateAnalogOutputTask("rf1FM");
            rf2FMOutputTask = CreateAnalogOutputTask("rf2FM");
            cPlusOutputTask = CreateAnalogOutputTask("cPlus");
            cMinusOutputTask = CreateAnalogOutputTask("cMinus");

			// analog inputs
			probeMonitorInputTask = CreateAnalogInputTask("probePD");
			pumpMonitorInputTask = CreateAnalogInputTask("pumpPD");
            cPlusMonitorInputTask = CreateAnalogInputTask("cPlusMonitor");
            cMinusMonitorInputTask = CreateAnalogInputTask("cMinusMonitor");
            rfPowerMonitorInputTask = CreateAnalogInputTask("rfPower");
		
            // make the control window
			window = new ControlWindow();
			window.controller = this;
			Application.Run(window);
		}

		// this method runs immediately after the GUI sets up
		internal void WindowLoaded()
		{
			// update the GPIB switcher's cached voltages
			// works around a "first-time" bug with the E-field switch
            FieldsOff();
            /*lastGPlus = GPlusVoltage;
			lastGMinus = GMinusVoltage;
			lastCPlus = CPlusVoltage;
			lastCMinus = CMinusVoltage;*/
            // Set the leakage current monitor textboxes to the default values.
            window.SetTextBox(window.southOffsetIMonitorTextBox, southOffset.ToString());
            window.SetTextBox(window.northOffsetIMonitorTextBox, northOffset.ToString());
            window.SetTextBox(window.IMonitorMeasurementLengthTextBox, currentMonitorMeasurementTime.ToString());
		}

		private Task CreateAnalogInputTask(string channel)
		{
			Task task = new Task("EDMHCIn" + channel);
			((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
				task,
				0,
				10
			);
			task.Control(TaskAction.Verify);
			return task;
		}

		private Task CreateAnalogOutputTask(string channel)
		{
			Task task = new Task("EDMHCOut" + channel);
            AnalogOutputChannel c = ((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[channel]);
            c.AddToTask(
				task,
				c.RangeLow,
				c.RangeHigh
				);
			task.Control(TaskAction.Verify);
			return task;
		}

		private void SetAnalogOutput(Task task, double voltage)
		{
			AnalogSingleChannelWriter writer = new AnalogSingleChannelWriter(task.Stream);
			writer.WriteSingleSample(true, voltage);
			task.Control(TaskAction.Unreserve);
		}

		private double ReadAnalogInput(Task task)
		{
			AnalogSingleChannelReader reader = new AnalogSingleChannelReader(task.Stream);
			double val = reader.ReadSingleSample();
			task.Control(TaskAction.Unreserve);
			return val;
		}

        private double ReadAnalogInput(Task task, double sampleRate, int numOfSamples)
        {
            //Configure the timing parameters of the task
            task.Timing.ConfigureSampleClock("", sampleRate,
                SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, numOfSamples);
           
            //Read in multiple samples
            AnalogSingleChannelReader reader = new AnalogSingleChannelReader(task.Stream);
            double[] valArray = reader.ReadMultiSample(numOfSamples);
            task.Control(TaskAction.Unreserve);
            
            //Calculate the average of the samples
            double sum = 0;
            for (int j = 0; j < numOfSamples; j++)
            {
                sum = sum + valArray[j];
            }
            double val = sum / numOfSamples;
            return val;
        }

		private void CreateDigitalTask(String name)
		{
			Task digitalTask = new Task(name);
			((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[name]).AddToTask(digitalTask);
			digitalTask.Control(TaskAction.Verify);
			digitalTasks.Add(name, digitalTask);
		}

		private void SetDigitalLine(string name, bool value)
		{
			Task digitalTask = ((Task)digitalTasks[name]);
			DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(digitalTask.Stream);
			writer.WriteSingleSampleSingleLine(true, value);
			digitalTask.Control(TaskAction.Unreserve);
		}

		#endregion

		#region Public properties for controlling the hardware

		public double GreenSynthOnFrequency
		{
			get
			{
				return Double.Parse(window.greenOnFreqBox.Text);
			}
			set
			{
				window.SetTextBox(window.greenOnFreqBox, value.ToString());
			}
		}

		public double GreenSynthOnAmplitude
		{
			get
			{
				return Double.Parse(window.greenOnAmpBox.Text);
			}
			set
			{
				window.SetTextBox(window.greenOnAmpBox, value.ToString());
			}
		}

		public double GreenSynthDCFM
		{
			get
			{
				return Double.Parse(window.greenDCFMBox.Text);
			}
			set
			{
				window.SetTextBox(window.greenDCFMBox, value.ToString());
			}
		}


		public bool GreenSynthEnabled
		{
			get
			{
				return window.greenOnCheck.Checked;
			}
			set
			{
				window.SetCheckBox(window.greenOnCheck, value);
			}
		}
	
		public bool RFSwitchEnabled
		{
			get
			{
				return window.rfSwitchEnableCheck.Checked;
			}
			set
			{
				window.SetCheckBox(window.rfSwitchEnableCheck, value);
			}
		}


		public bool GreenDCFMSelected
		{
			get
			{
				return window.fmSelectCheck.Checked;
			}
			set
			{
				window.SetCheckBox(window.fmSelectCheck, value);
			}
		}

        public bool AttenuatorSelected
        {
            get
            {
                return window.attenuatorSelectCheck.Checked;
            }
            set
            {
                window.SetCheckBox(window.attenuatorSelectCheck, value);
            }
        }

        public bool EFieldEnabled
		{
			get
			{
				return window.eOnCheck.Checked;
			}
			set
			{
				window.SetCheckBox(window.eOnCheck, value);
			}
		}

		public bool EFieldPolarity
		{
			get
			{
				return window.ePolarityCheck.Checked;
			}
			set
			{
				window.SetCheckBox(window.ePolarityCheck, value);
			}
		}

		public bool EBleedEnabled
		{
			get
			{
				return window.eBleedCheck.Checked;
			}
			set
			{
				window.SetCheckBox(window.eBleedCheck, value);
			}
		}

		public double CPlusVoltage
		{
			get
			{
				return Double.Parse(window.cPlusTextBox.Text);
			}
			set
			{
				window.SetTextBox(window.cPlusTextBox, value.ToString());
			}
		}
		
		public double CMinusVoltage
		{
			get
			{
				return Double.Parse(window.cMinusTextBox.Text);
			}
			set
			{
				window.SetTextBox(window.cMinusTextBox, value.ToString());
			}
		}

		public double CPlusOffVoltage
		{
			get
			{
				return Double.Parse(window.cPlusOffTextBox.Text);
			}
			set
			{
				window.SetTextBox(window.cPlusOffTextBox, value.ToString());
			}
		}
		
		public double CMinusOffVoltage
		{
			get
			{
				return Double.Parse(window.cMinusOffTextBox.Text);
			}
			set
			{
				window.SetTextBox(window.cMinusOffTextBox, value.ToString());
			}
		}

		public bool CalFlipEnabled
		{
			get
			{
				return window.calFlipCheck.Checked;
			}
			set
			{
				window.SetCheckBox(window.calFlipCheck, value);
			}
		}

		public bool BFlipEnabled
		{
			get
			{
				return window.bFlipCheck.Checked;
			}
			set
			{
				window.SetCheckBox(window.bFlipCheck, value);
			}
		}

		public bool PumpShutter
		{
			get
			{
				return window.pumpShutterCheck.Checked;
			}
			set
			{
				window.SetCheckBox(window.pumpShutterCheck, value);
			}
		}

		public bool Pump2Shutter
		{
			get
			{
				return window.pump2ShutterCheck.Checked;
			}
			set
			{
				window.SetCheckBox(window.pump2ShutterCheck, value);
			}
		}

		/* This is something of a cheesy hack. It lets the edm script check to see if the YAG
		 * laser has failed.
		 */
		public bool YAGInterlockFailed
		{
			get
			{
				return ((BrilliantLaser)Environs.Hardware.YAG).InterlockFailed;
			}
		}
		
		#endregion

		#region Public properties for monitoring the hardware

		public double BCurrent00
		{
			get
			{
				return Double.Parse(window.bCurrent00TextBox.Text);
			}
		}

		public double BCurrent01
		{
			get
			{
				return Double.Parse(window.bCurrent01TextBox.Text);
			}
		}

		public double BCurrent10
		{
			get
			{
				return Double.Parse(window.bCurrent10TextBox.Text);
			}
		}

		public double BCurrent11
		{
			get
			{
				return Double.Parse(window.bCurrent11TextBox.Text);
			}
		}

		public double BiasCurrent
		{
			get
			{
				return Double.Parse(window.bCurrentBiasTextBox.Text);
			}
		}

		public double FlipStepCurrent
		{
			get
			{
				return Double.Parse(window.bCurrentFlipStepTextBox.Text);
			}
		}

		public double CalStepCurrent
		{
			get
			{
				return Double.Parse(window.bCurrentCalStepTextBox.Text);
			}
		}

        public double CPlusMonitorVoltage
        {
            get
            {
                return Double.Parse(window.cPlusVMonitorTextBox.Text);
            }
        }

        public double CMinusMonitorVoltage
        {
            get
            {
                return Double.Parse(window.cMinusVMonitorTextBox.Text);
            }
        }

        public double RF1AttCentre
        {
            get
            {
                return Double.Parse(window.rf1AttenuatorVoltageTextBox.Text);
            }
        }

        public double RF1AttStep
        {
            get
            {
                return Double.Parse(window.rf1AttIncTextBox.Text);
            }
        }

        public double RF2AttCentre
        {
            get
            {
                return Double.Parse(window.rf2AttenuatorVoltageTextBox.Text);
            }
        }

        public double RF2AttStep
        {
            get
            {
                return Double.Parse(window.rf2AttIncTextBox.Text);
            }
        }

        public double RF1FMCentre
        {
            get
            {
                return Double.Parse(window.rf1FMVoltage.Text);
            }
        }

        public double RF1FMStep
        {
            get
            {
                return Double.Parse(window.rf1FMIncTextBox.Text);
            }
        }

        public double RF2FMCentre
        {
            get
            {
                return Double.Parse(window.rf2FMVoltage.Text);
            }
        }

        public double RF2FMStep
        {
            get
            {
                return Double.Parse(window.rf2FMIncTextBox.Text);
            }
        }

        #endregion

		#region Hardware control methods - safe for remote

        public void FieldsOff()
        {
            CPlusVoltage = 0;
            CMinusVoltage = 0;
            CPlusOffVoltage = 0;
            CMinusOffVoltage = 0;
            UpdateVoltages();
            EFieldEnabled = false;

        }

		public void SwitchE()
		{
			SwitchE(!EFieldPolarity, eDischargeTime, eBleedTime, eWaitTime, eChargeTime);
		}

		public void SwitchE(bool state, int dischargeTime, int bleedTime, int switchTime, int chargeTime)
		{
			// don't waste time if the field isn't really switching
			if (state != EFieldPolarity)
			{
                EFieldEnabled = false;
				Thread.Sleep(dischargeTime);
				EBleedEnabled = true;
				Thread.Sleep(bleedTime);
				EBleedEnabled = false;
				EFieldPolarity = state;
				Thread.Sleep(switchTime);
				EFieldEnabled = true;
				Thread.Sleep(chargeTime);
			}
           
		}

        //Commented out by Henry, 20.4.06 I have fitted my measured voltage as a function of AO voltage, so to get a 
        //desired HV, the inverse equation is used. i.e.
        //I have V_hv=slope_x * V_AO_x + offset_x, so I arrange as:
        //V_AO_x=(V_hv - offset_x)/slope_x, where x is the appropriate channel

        //public void UpdateVoltages()
        //{
        //    voltageController.SetOutputVoltage(cPlusChan, (CPlusVoltage * cPlusSlope) - cPlusOffset);
        //    voltageController.SetOutputVoltage(cMinusChan, (CMinusVoltage * cMinusSlope) - cMinusOffset);
        //    voltageController.SetOutputVoltage(gPlusChan, (GPlusVoltage * gPlusSlope) - gPlusOffset);
        //    voltageController.SetOutputVoltage(gMinusChan, (GMinusVoltage * gMinusSlope) - gMinusOffset);
        //}

        public void UpdateVoltages()
        {
			// kludge in the asymmetric field switch here
			double cPlus = CPlusVoltage;
			double cMinus = CMinusVoltage;
            double cPlusOff = CPlusOffVoltage;
            double cMinusOff = CMinusOffVoltage;
			if (EFieldEnabled && window.eFieldAsymmetryCheckBox.Checked)
			{
				if (EFieldPolarity == false)
				{
					cPlus += Double.Parse(window.zeroPlusOneMinusBoostTextBox.Text);
					cPlus += Double.Parse(window.zeroPlusBoostTextBox.Text);
				}
				else
				{
					cMinus -= Double.Parse(window.zeroPlusOneMinusBoostTextBox.Text);
				}
			}

            //voltageController.SetOutputVoltage(cPlusChan, (cPlus - cPlusOffset) / cPlusSlope);
            //voltageController.SetOutputVoltage(cMinusChan, (cMinus - cMinusOffset) / cMinusSlope);
            //voltageController.SetOutputVoltage(gPlusChan, (GPlusVoltage - gPlusOffset) / gPlusSlope);
            //voltageController.SetOutputVoltage(gMinusChan, (GMinusVoltage - gMinusOffset) / gMinusSlope);
            //Checks if E field enable box is checked or not before setting the fields
            if (EFieldEnabled)
            {
                SetAnalogOutput(cPlusOutputTask, cPlus);
                SetAnalogOutput(cMinusOutputTask, cMinus);
            }
            else
            {
                SetAnalogOutput(cPlusOutputTask, cPlusOff);
                SetAnalogOutput(cMinusOutputTask, cMinusOff);
            }

        }

        public void UpdateRFFrequencyMonitor()
        {
            // make sure rf switch is off (this routes power to the measurement devices)
            window.SetCheckBox(window.rfSwitchEnableCheck, false);
            // rf1 - switch box off and then on to make sure it fires the checkChanged event
            window.SetCheckBox(window.fmSelectCheck, false);
            window.SetCheckBox(window.fmSelectCheck, true);
            window.SetRadioButton(window.rf1FMPlusRB, true);
            SetFMVoltages();
            Thread.Sleep(100);
            double rf1PlusFreq = rfCounter.Frequency;
            window.SetTextBox(window.rf1PlusFreqMon, rf1PlusFreq.ToString());
            window.SetRadioButton(window.rf1FMMinusRB, true);
            SetFMVoltages();
            Thread.Sleep(100);
            double rf1MinusFreq = rfCounter.Frequency;
            window.SetTextBox(window.rf1MinusFreqMon, rf1MinusFreq.ToString());
            window.SetTextBox(window.rf1CentreFreqMon, ((rf1MinusFreq + rf1PlusFreq) / 2).ToString());
            window.SetTextBox(window.rf1StepFreqMon, ((rf1PlusFreq - rf1MinusFreq) / 2).ToString());

            // rf2
            window.SetCheckBox(window.fmSelectCheck, false);
            window.SetRadioButton(window.rf2FMPlusRB, true);
            SetFMVoltages();
            Thread.Sleep(100);
            double rf2PlusFreq = rfCounter.Frequency;
            window.SetTextBox(window.rf2PlusFreqMon, rf2PlusFreq.ToString());
            window.SetRadioButton(window.rf2FMMinusRB, true);
            SetFMVoltages();
            Thread.Sleep(100);
            double rf2MinusFreq = rfCounter.Frequency;
            window.SetTextBox(window.rf2MinusFreqMon, rf2MinusFreq.ToString());
            window.SetTextBox(window.rf2CentreFreqMon, ((rf2MinusFreq + rf2PlusFreq) / 2).ToString());
            window.SetTextBox(window.rf2StepFreqMon, ((rf2PlusFreq - rf2MinusFreq) / 2).ToString());

        }

        public void UpdateRFPowerMonitor()
        {
            // make sure rf switch is off (this routes power to the measurement devices)
            window.SetCheckBox(window.rfSwitchEnableCheck, false);
            // rf1 - switch box off and then on to make sure it fires the checkChanged event
            window.SetCheckBox(window.attenuatorSelectCheck, false);
            window.SetCheckBox(window.attenuatorSelectCheck, true);
            window.SetRadioButton(window.rf1AttPlusRB, true);
            SetAttenutatorVoltages();
            Thread.Sleep(100);
            double rf1PlusPower = ReadPowerMonitor();
            window.SetTextBox(window.rf1PlusPowerMon, rf1PlusPower.ToString());
            window.SetRadioButton(window.rf1AttMinusRB, true);
            SetAttenutatorVoltages();
            Thread.Sleep(100);
            double rf1MinusPower = ReadPowerMonitor();
            window.SetTextBox(window.rf1MinusPowerMon, rf1MinusPower.ToString());
            window.SetTextBox(window.rf1CentrePowerMon, ((rf1MinusPower + rf1PlusPower) / 2).ToString());
            window.SetTextBox(window.rf1StepPowerMon, ((rf1PlusPower - rf1MinusPower) / 2).ToString());

            // rf2
            window.SetCheckBox(window.attenuatorSelectCheck, false);
            window.SetRadioButton(window.rf2AttPlusRB, true);
            SetAttenutatorVoltages();
            Thread.Sleep(100);
            double rf2PlusPower = ReadPowerMonitor();
            window.SetTextBox(window.rf2PlusPowerMon, rf2PlusPower.ToString());
            window.SetRadioButton(window.rf2AttMinusRB, true);
            SetAttenutatorVoltages();
            Thread.Sleep(100);
            double rf2MinusPower = ReadPowerMonitor();
            window.SetTextBox(window.rf2MinusPowerMon, rf2MinusPower.ToString());
            window.SetTextBox(window.rf2CentrePowerMon, ((rf2MinusPower + rf2PlusPower) / 2).ToString());
            window.SetTextBox(window.rf2StepPowerMon, ((rf2PlusPower - rf2MinusPower) / 2).ToString());
        }

        // This is a little cheezy - it probably should be in its own class.
        // This method reads the power meter input and converts the result to dBm.
        private double ReadPowerMonitor()
        {
            double rawReading = ReadAnalogInput(rfPowerMonitorInputTask, 10000, 5000);
            return rawReading;
        }

		public void UpdateBCurrentMonitor()
		{
			// DB0 dB0
			BFlipEnabled = false;
			CalFlipEnabled = false;
			double i00 = 1000000 * bCurrentMeter.ReadCurrent();
			window.SetTextBox(window.bCurrent00TextBox, i00.ToString());
			Thread.Sleep(50);

			// DB0 dB1
			BFlipEnabled = false;
			CalFlipEnabled = true;
			double i01 = 1000000 * bCurrentMeter.ReadCurrent();
			window.SetTextBox(window.bCurrent01TextBox, i01.ToString());
			Thread.Sleep(50);

			// DB1 dB0
			BFlipEnabled = true;
			CalFlipEnabled = false;
			double i10 = 1000000 * bCurrentMeter.ReadCurrent();
			window.SetTextBox(window.bCurrent10TextBox, i10.ToString());
			Thread.Sleep(50);
			
			// DB1 dB1
			BFlipEnabled = true;
			CalFlipEnabled = true;
			double i11 = 1000000 * bCurrentMeter.ReadCurrent();
			window.SetTextBox(window.bCurrent11TextBox, i11.ToString());
			Thread.Sleep(50);

			// calculate the steps
			double bias = (i00 + i01 + i10 + i11) / 4;
			double calStep = (i01 - i00 - i11 + i10) / 4;
			double flipStep = (i10 - i00 + i11 - i01) / 4;
			window.SetTextBox(window.bCurrentBiasTextBox, bias.ToString());
			window.SetTextBox(window.bCurrentCalStepTextBox, calStep.ToString());
			window.SetTextBox(window.bCurrentFlipStepTextBox, flipStep.ToString());

		}

		public void UpdateVMonitor()
		{
			/*window.SetTextBox(window.cPlusVMonitorTextBox, 
				(cScale * voltageController.ReadInputVoltage(cPlusChan)).ToString());
			window.SetTextBox(window.cMinusVMonitorTextBox, 
				(cScale * voltageController.ReadInputVoltage(cMinusChan)).ToString());
			window.SetTextBox(window.gPlusVMonitorTextBox, 
				(gScale * voltageController.ReadInputVoltage(gPlusChan)).ToString());
			window.SetTextBox(window.gMinusVMonitorTextBox, 
				(gScale * voltageController.ReadInputVoltage(gMinusChan)).ToString());*/
            double cPlusMonitor = ReadAnalogInput(cPlusMonitorInputTask, 100000, 50000);
			window.SetTextBox(window.cPlusVMonitorTextBox, cPlusMonitor.ToString());
			double cMinusMonitor = ReadAnalogInput(cMinusMonitorInputTask, 100000, 50000);
			window.SetTextBox(window.cMinusVMonitorTextBox, cMinusMonitor.ToString());
		}

		public void UpdateIMonitor()
		{
			window.SetTextBox(window.northIMonitorTextBox, (northLeakageMonitor.GetCurrent()).ToString());
            window.SetTextBox(window.southIMonitorTextBox, (southLeakageMonitor.GetCurrent()).ToString());
		}

        public void CalibrateIMonitors()
        {
            southLeakageMonitor.Calibrate();
            northLeakageMonitor.Calibrate();

            northOffset = northLeakageMonitor.Offset;
            southOffset = southLeakageMonitor.Offset;

            window.SetTextBox(window.southOffsetIMonitorTextBox, southOffset.ToString());
            window.SetTextBox(window.northOffsetIMonitorTextBox, northOffset.ToString());
           
        }

        public void setIMonitorMeasurementLength()
        {
            currentMonitorMeasurementTime = Double.Parse(window.IMonitorMeasurementLengthTextBox.Text);
            southLeakageMonitor.MeasurementTime = currentMonitorMeasurementTime;
            northLeakageMonitor.MeasurementTime = currentMonitorMeasurementTime;
        }

		public void UpdateLaserPhotodiodes()
		{
			double probePDValue = ReadAnalogInput(probeMonitorInputTask);
			window.SetTextBox(window.probeMonitorTextBox, probePDValue.ToString());
			double pumpPDValue = ReadAnalogInput(pumpMonitorInputTask);
			window.SetTextBox(window.pumpMonitorTextBox, pumpPDValue.ToString());
		}

		// TODO: I'm not sure whether these button enabling properties are threadsafe.
		// Probably had better wrap them.
		public void StartYAGFlashlamps()
		{
			yag.StartFlashlamps(false);
			window.startYAGFlashlampsButton.Enabled = false;
			window.stopYagFlashlampsButton.Enabled = true;
		}

		public void StopYAGFlashlamps()
		{
			yag.StopFlashlamps();
			window.startYAGFlashlampsButton.Enabled = true;
			window.stopYagFlashlampsButton.Enabled = false;
		}

		public void EnableYAGQ()
		{
			yag.EnableQSwitch();
			window.yagQEnableButton.Enabled = false;
			window.yagQDisableButton.Enabled = true;
		}

		public void DisableYAGQ()
		{
			yag.DisableQSwitch();
			window.yagQEnableButton.Enabled = true;
			window.yagQDisableButton.Enabled = false;
		}

		public void CheckYAGInterlock()
		{
			window.SetTextBox(window.interlockStatusTextBox, yag.InterlockFailed.ToString());
		}

		public void UpdateYAGFlashlampVoltage()
		{
			yag.SetFlashlampVoltage((int)Double.Parse(window.yagFlashlampVTextBox.Text));
		}

		#endregion

		#region Hardware control methods - local use only


		public void EnableGreenSynth(bool enable)
		{
			greenSynth.Connect();
			if (enable)
			{
				greenSynth.Frequency = GreenSynthOnFrequency;
				greenSynth.Amplitude = GreenSynthOnAmplitude;
				greenSynth.DCFM = GreenSynthDCFM;
			}
			else
			{
				greenSynth.Amplitude = greenSynthOffAmplitude;
			}
			greenSynth.Disconnect();
		}

		public void EnableRFSwitch(bool enable)
		{
			SetDigitalLine("rfSwitch", enable);
		}

		/*private double lastGPlus = 0;
		private double lastGMinus = 0;
		private double lastCPlus = 0;
		private double lastCMinus = 0;*/
		/*public void SetEFieldOnOff(bool enable)
		{
			/*if (eFieldMode == EFieldMode.TTL)
			{
				SetDigitalLine("eOnOff", enable);
				SetDigitalLine("notEOnOff", !enable);
			}
			if (eFieldMode == EFieldMode.GPIB)
			{
				if (!enable)
				{
					// switching off, so save the voltages for when we switch back on
					lastGPlus = GPlusVoltage;
					lastGMinus = GMinusVoltage;
					lastCPlus = CPlusVoltage;
					lastCMinus = CMinusVoltage;
					// set the voltages to zero and update
					GPlusVoltage = 0;
					GMinusVoltage = 0;
					CPlusVoltage = 0;
					CMinusVoltage = 0;
					UpdateVoltages();
				}
				else
				{
					// switching on, so restore the voltages at last switch off
					GPlusVoltage = lastGPlus;
					GMinusVoltage = lastGMinus;
					CPlusVoltage = lastCPlus;
					CMinusVoltage = lastCMinus;
					UpdateVoltages();
				}
			}               
            
		}*/

		public void SetEPolarity(bool state)
		{
			SetDigitalLine("ePol", state);
			SetDigitalLine("notEPol", !state);
		}

		public void SetBleed(bool enable)
		{
			SetDigitalLine("eBleed", enable);
		}

		public void SetBFlip(bool enable)
		{
			SetDigitalLine("b", enable);
		}

		public void SetCalFlip(bool enable)
		{
			SetDigitalLine("notDB", !enable);
		}

		public void SelectGreenDCFM(bool enable)
		{
			SetDigitalLine("fmSelect", enable);
		}

        internal void SelectAttenuator(bool enable)
        {
            SetDigitalLine("attenuatorSelect", enable);
        }
        
        public void SetPhaseFlip1(bool enable)
		{
			SetDigitalLine("piFlip", enable);
		}

		public void SetPhaseFlip2(bool enable)
		{
			SetDigitalLine("piFlipEnable", enable);
		}

		internal void SetPumpShutter(bool enable)
		{
			SetDigitalLine("pumpShutter", enable); 
		}

		internal void SetPump2Shutter(bool enable)
		{
			SetDigitalLine("pump2Shutter", enable);
		}

		public void SetScanningBVoltage()
		{
			double bBoxVoltage = Double.Parse(window.scanningBVoltageBox.Text);
			SetAnalogOutput(bBoxAnalogOutputTask, bBoxVoltage);
		}

		public void SetScanningBZero()
		{
			window.SetTextBox(window.scanningBVoltageBox, "0.0");
			SetScanningBVoltage();
		}

		public void SetScanningBFS()
		{
			window.SetTextBox(window.scanningBVoltageBox, "5.0");
			SetScanningBVoltage();
		}

        private double windowVoltage(double vIn, double vMin, double vMax)
        {
            if (vIn < vMin) return vMin;
            if (vIn > vMax) return vMax;
            return vIn;
        }

		public void SetAttenutatorVoltages()
		{
			double rf1AttenuatorVoltage = Double.Parse(window.rf1AttenuatorVoltageTextBox.Text);
            if (window.rf1AttMinusRB.Checked) rf1AttenuatorVoltage -= Double.Parse(window.rf1AttIncTextBox.Text);
            if (window.rf1AttPlusRB.Checked) rf1AttenuatorVoltage += Double.Parse(window.rf1AttIncTextBox.Text);
            rf1AttenuatorVoltage = windowVoltage(rf1AttenuatorVoltage, 0, 5);
			double rf2AttenuatorVoltage = Double.Parse(window.rf2AttenuatorVoltageTextBox.Text);
            if (window.rf2AttMinusRB.Checked) rf2AttenuatorVoltage -= Double.Parse(window.rf2AttIncTextBox.Text);
            if (window.rf2AttPlusRB.Checked) rf2AttenuatorVoltage += Double.Parse(window.rf2AttIncTextBox.Text);
            rf2AttenuatorVoltage = windowVoltage(rf2AttenuatorVoltage, 0, 5);
			SetAnalogOutput(rf1AttenuatorOutputTask, rf1AttenuatorVoltage);
			SetAnalogOutput(rf2AttenuatorOutputTask, rf2AttenuatorVoltage);
		}

        internal void SetFMVoltages()
        {
            double rf1FMVoltage = Double.Parse(window.rf1FMVoltage.Text);
            if (window.rf1FMMinusRB.Checked) rf1FMVoltage -= Double.Parse(window.rf1FMIncTextBox.Text);
            if (window.rf1FMPlusRB.Checked) rf1FMVoltage += Double.Parse(window.rf1FMIncTextBox.Text);
            double rf2FMVoltage = Double.Parse(window.rf2FMVoltage.Text);
            if (window.rf2FMMinusRB.Checked) rf2FMVoltage -= Double.Parse(window.rf2FMIncTextBox.Text);
            if (window.rf2FMPlusRB.Checked) rf2FMVoltage += Double.Parse(window.rf2FMIncTextBox.Text);
            SetAnalogOutput(rf1FMOutputTask, rf1FMVoltage);
            SetAnalogOutput(rf2FMOutputTask, rf2FMVoltage);
        }

#endregion


     }
}
