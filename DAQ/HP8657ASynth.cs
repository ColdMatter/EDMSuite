using System;


using DAQ.Environment;
using System.Threading;

namespace DAQ.HAL
{
	/// <summary>
	/// This class represents a GPIB controlled HP8657A synth. It conforms to the Synth
	/// interface.
	/// </summary>
	public class HP8657ASynth : Synth
	{

		public HP8657ASynth(String visaAddress) : base(visaAddress)
		{}

		override public double Frequency
		{
			set
			{
				// weird bug in the 8657 - it seems to swallow the first GPIB command
				// written to it. It has none standard RMT/LCL handling which might be
				// something to do with it. Could perhaps figure it out if I could find
				// the manual.
				//if (!Environs.Debug) Write("FR" + value + "MZ");
				if (!Environs.Debug) Write("FR" + value + "MZ");
                Thread.Sleep(25);
			}
		}

		override public double Amplitude
		{
			set
			{
				String s = "AP" + value + "DM";
				// weird bug in the 8657 - it seems to swallow the first GPIB command
				// written to it. It has none standard RMT/LCL handling which might be
				// something to do with it. Could perhaps figure it out if I could find
				// the manual.
				//if (!Environs.Debug) Write(s);
				if (!Environs.Debug) Write(s);
                Thread.Sleep(25);
			}
		}

		override public bool Enabled
		{
			set
			{
				// TODO: this could turn the rf off instead
				if (!value) Amplitude = -120;
                Thread.Sleep(25);
			}
		}

		private double lastDCFM = 0.0;
		public override double DCFM
		{
			set
			{
				lastDCFM = value;
				String s = "S5" + value + "KZ";
				if (!Environs.Debug) Write(s);
				if (!Environs.Debug) Write(s);
			}
		}

		// this disables the dcfm by turning it off, and turns it on by writing
		// the last value written again (default 0.0).
		public override bool DCFMEnabled
		{
			set
			{
				if (!value) 
				{
					// TODO: put the correct string in here
//					String s = "KJSADKJ";
//					if (!Environs.Debug) Write(s);
//					if (!Environs.Debug) Write(s);
				}
				else DCFM = lastDCFM;
			}
		}
	}
}
