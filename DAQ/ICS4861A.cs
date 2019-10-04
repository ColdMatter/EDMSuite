using System;

using DAQ.Environment;

namespace DAQ.HAL
{
	/// <summary>
	/// Interface class for ICS4861A GPIB controlled analog input/output box.
	/// </summary>
	public class ICS4861A : GPIBInstrument
	{
		Random r = new Random();

		public ICS4861A(String visaAddress) : base(visaAddress)
		{}

		public void SetOutputVoltage(int channel, double voltage)
		{
			Connect();
			if (!Environs.Debug)
			{
				Write("C " + channel);
				Write("VOLT " + voltage);
			}
			Disconnect();
		}

		public double ReadInputVoltage(int channel)
		{
			double reading;
			Connect();
			if (!Environs.Debug)
			{
				Write("A?" + channel);
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
