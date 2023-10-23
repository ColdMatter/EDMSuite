using System;

using NationalInstruments.Visa;
using NationalInstruments.ModularInstruments.NIRfsg;
using NationalInstruments.ModularInstruments.SystemServices.DeviceServices;

using Ivi.Visa;

using DAQ.Environment;

using System.Windows.Forms;

namespace DAQ.HAL
{
	/// <summary>
	/// Represents a class to use an NI RF signal generator (NI-Rfsg).
    /// When writing to the device, frequencies are in Hz 
    /// and amplitudes refer to the average rms power in dBm.
	/// </summary>
	public class NIRfsgInstrument : Instrument
	{
		NIRfsg _rfsgSession;
		private string _resourceName;
        private double _frequency;
        private double _amplitude;
        private double[] _iData;
        private double[] _qData;
        private RfsgGenerationStatus _generating;

		public NIRfsgInstrument(String address)
		{
			this._resourceName = address;
		}

        public double Frequency
        {
            get
            {
                return _rfsgSession.RF.Frequency;
            }
            set
            {
                this._frequency = value * 1000000;
            }
        }

        public double Amplitude
        {
            get
            {
                return _rfsgSession.RF.PowerLevel;
            }
            set
            {
                this._amplitude = value;
            }
        }

        public double[] IData
        {
            get
            {
                return _iData;
            }
            set
            {
                this._iData = value;
            }
        }

        public double[] QData
        {
            get
            {
                return _qData;
            }
            set
            {
                this._qData = value;
            }
        }

        public bool GenerationComplete
        {
            get
            {
                if (_rfsgSession != null && _generating == RfsgGenerationStatus.Complete) return true;
                else return false;
            }
        }

		public override void Connect()
		{
			if (!Environs.Debug) 
			{
                if (_rfsgSession != null)
                {
                    _rfsgSession.Close();
                    _rfsgSession = null;
                }

                // Initialize the NIRfsg session
                _rfsgSession = new NIRfsg(_resourceName, true, false);

                // Subscribe to Rfsg warnings
                _rfsgSession.DriverOperation.Warning += new EventHandler<RfsgWarningEventArgs>(DriverOperation_Warning);
			}
		}

        public override void Disconnect()
        {
            if (!Environs.Debug)
            {
                if (_rfsgSession != null)
                {
                    // Disable the output.  This sets the noise floor as low as possible.
                    _rfsgSession.RF.OutputEnabled = false;

                    // Unsubscribe from warning events
                    _rfsgSession.DriverOperation.Warning -= DriverOperation_Warning;

                    // Close the RFSG NIRfsg session
                    _rfsgSession.Close();
                }
                _rfsgSession = null;
            }
        }

        public void StartGeneration()
        {
            if (!Environs.Debug)
            {
                // Configure the instrument 
                _rfsgSession.RF.Configure(_frequency, _amplitude);

                // Initiate Generation 
                _rfsgSession.Initiate();
            }
        }

        public void StartPulsedGeneration()
        {
            if (!Environs.Debug)
            {
                // Configure the instrument
                _rfsgSession.RF.Configure(_frequency, _amplitude);
                _rfsgSession.Arb.GenerationMode = RfsgWaveformGenerationMode.ArbitraryWaveform;
                _rfsgSession.Arb.IQRate = 100e6;

                // Enable finite generation
                _rfsgSession.Arb.IsWaveformRepeatCountFinite = true;
                _rfsgSession.Arb.WaveformRepeatCount = 1;

                // Configure signal bandwidth to its max value
                _rfsgSession.Arb.SignalBandwidth = 20e6;

                // Configure power level type to Peak Power
                _rfsgSession.RF.PowerLevelType = RfsgRFPowerLevelType.PeakPower;

                // Configure trigger
                _rfsgSession.Triggers.StartTrigger.DigitalEdge.Configure(RfsgDigitalEdgeStartTriggerSource.Pfi0, RfsgTriggerEdge.RisingEdge);

                // Write the waveform
                _rfsgSession.Arb.ClearAllWaveforms();
                _rfsgSession.Arb.WriteWaveform("", _iData, _qData);

                // Initiate generation
                _rfsgSession.Initiate();
            }
        }

        public void UpdateGeneration()
        {
            if (!Environs.Debug)
            {
                _rfsgSession.Abort();

                // Configure the instrument 
                _rfsgSession.RF.Configure(_frequency, _amplitude);

                // Initiate Generation 
                _rfsgSession.Initiate();
            }
        }

        public void CheckGeneration()
        {
            if (!Environs.Debug)
            {
                _generating = _rfsgSession.CheckGenerationStatus();
            }
        }

        public void StopGeneration()
        {
            if (!Environs.Debug)
            {
                _rfsgSession.Abort();
            }
        }
        
        void DriverOperation_Warning(object sender, RfsgWarningEventArgs e)
        {
            // Display the rfsg warning
            Console.WriteLine(e.Message);
        }

        // Not a serial communication instrument
        protected override void Write(String command)
        {
        }

        // Not a serial communication instrument
        protected override string Read()
        {
            return null;
        }
	}
}
