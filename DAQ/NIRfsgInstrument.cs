using System;

using NationalInstruments.Visa;
using NationalInstruments.ModularInstruments.NIRfsg;
using NationalInstruments.ModularInstruments.SystemServices.DeviceServices;

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

        public bool Enabled
        {
            set
            {
                if (!value) this._frequency = 36 * 1000000;
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
