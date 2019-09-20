using System;

using DAQ.Environment;

namespace DAQ.HAL
{
	/// <summary>
	/// Inteface class for HP34401A 6.5 digit multimeter
	/// </summary>
	public class HP34401A : GPIBInstrument
	{
		Random r = new Random();

		public HP34401A(String visaAddress) : base(visaAddress)
		{}

		public double ReadVoltage()
		{
			double reading;
			Connect();
			if (!Environs.Debug)
			{
				Write("MEAS:VOLT:DC? 1,0.000001");
				reading = Double.Parse(Read());
			}
			else
			{
				reading = r.NextDouble();
			}
			Disconnect();
			return reading;
		}

		public double ReadCurrent()
		{
			double reading;
			Connect();
			if (!Environs.Debug)
			{
				Write("MEASURE:CURRENT:DC? 0.01,0.00000001");
				reading = Double.Parse(Read());
			}
			else
			{
				reading = r.NextDouble();
			}
			Disconnect();
			return reading;
		}

	}
}