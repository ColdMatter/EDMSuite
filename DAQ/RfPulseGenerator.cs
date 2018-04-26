using System;

using NationalInstruments.Visa;
using NationalInstruments.ModularInstruments.NIRfsg;
using NationalInstruments.ModularInstruments.SystemServices.DeviceServices;

using DAQ.Environment;

using System.Windows.Forms;

namespace DAQ.HAL
{
	/// <summary>
	/// Represents a class to use the 2.7 GHz RF arbitrary waveform generator (NI PXI-5670)
	/// </summary>
	public class RfPulseGenerator : Instrument
	{
		NIRfsg rfsgSession;
		private string resourceName;

		public RfPulseGenerator(String address)
		{
			this.resourceName = address;
		}

		public override void Connect()
		{
			if (!Environs.Debug) 
			{
                // Initialize the NIRfsg session
                rfsgSession = new NIRfsg(resourceName, true, false);

                // Subscribe to Rfsg warnings
                rfsgSession.DriverOperation.Warning += new EventHandler<RfsgWarningEventArgs>(DriverOperation_Warning);
			}
		}

        public void StartGeneration(double frequency, double power)
        {
            if (!Environs.Debug)
            {
                // Configure the instrument 
                rfsgSession.RF.Configure(frequency, power);

                // Initiate Generation 
                rfsgSession.Initiate();
            }
        }

        public void UpdateGeneration(double frequency, double power)
        {
            if (!Environs.Debug)
            {
                rfsgSession.Abort();

                // Configure the instrument 
                rfsgSession.RF.Configure(frequency, power);

                // Initiate Generation 
                rfsgSession.Initiate();
            }
        }

        public void StopGeneration()
        {
            if (!Environs.Debug)
            {
                rfsgSession.Abort();
            }
        }

        public override void Disconnect()
		{
			if (!Environs.Debug)
			{
                if (rfsgSession != null)
                {
                    // Disable the output.  This sets the noise floor as low as possible.
                    rfsgSession.RF.OutputEnabled = false;

                    // Unsubscribe from warning events
                    rfsgSession.DriverOperation.Warning -= DriverOperation_Warning;

                    // Close the RFSG NIRfsg session
                    rfsgSession.Close();
                }
                rfsgSession = null;
			}
		}
        
        void DriverOperation_Warning(object sender, RfsgWarningEventArgs e)
        {
            // Display the rfsg warning
            Console.WriteLine(e.Message);
        }

        protected override void Write(String command)
        {
        }

        protected override string Read()
        {
            return null;
        }
	}
}
