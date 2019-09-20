using System;

namespace DAQ.HAL
{
	/// <summary>
	/// Represents a synth and what you can do to it.
	/// </summary>
	public abstract class Synth : GPIBInstrument
	{
		public Synth(String visaAddress) : base(visaAddress)
		{}

		public abstract double Frequency
		{
			set;
		}
		public abstract double Amplitude
		{
			set;
		}
		public abstract bool Enabled
		{
			set;
		}

		public abstract bool DCFMEnabled
		{
			set;
		}

		public abstract double  DCFM
		{
			set;
		}
	}
}
