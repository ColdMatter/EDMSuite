using System;

using NationalInstruments.VisaNS;

using DAQ.Environment;


namespace DAQ.HAL
{
	/// <summary>
	/// This class, which conforms to the YAG laser interface, talks to the Brilliant
	/// YAG laser over RS232.
	/// </summary>
	public class SerialDAQ : DAQ.HAL.RS232Instrument
	{
        
		public SerialDAQ(String address) : base (address)
		{
			//this.address = address; Not necassary as this is handled in the base constructor
		}

		public new void Connect()
		{
            base.Connect();
		}

		public new void Disconnect()
		{
            base.Connect();
		}

        public void SetOut1(double vout)
        {
            if (!connected) Connect();
            if (!Environs.Debug) serial.Query("VOUTA = " + Convert.ToString(vout) + "\r\n", 17);
            Disconnect();
        }

        public void SetOut2(double vout)
        {
            if (!connected) Connect();
            if (!Environs.Debug) serial.Query("VOUTB = " + Convert.ToString(vout) + "\r\n", 17);
            Disconnect();
        }

        public double ReadVin1()
        {
            double vin = 0.0;
            if (!connected) Connect();
            if (!Environs.Debug) vin = Convert.ToDouble(serial.Query("VINA?\r\n", 17));
            Disconnect();
            return vin;
        }

        public double ReadVin2()
        {
            double vin = 0.0;
            if (!connected) Connect();
            if (!Environs.Debug) vin = Convert.ToDouble(serial.Query("VINB?\r\n", 17));
            Disconnect();
            return vin;
        }

		public void PatternChangeStartingHandler(object source, EventArgs e)
		{}

		public void PatternChangeEndedHandler(object source, EventArgs e)
		{}

	}
}
