using System;


using DAQ.Environment;

namespace DAQ.HAL
{
	/// <summary>
	/// This class represents a GPIB controlled HP8673B synth. It conforms to the Synth
	/// interface.
	/// </summary>
	public class HP8673BSynth : Synth
	{

		public HP8673BSynth(String visaAddress) : base(visaAddress)
		{}

		override public double Frequency
		{
			set
			{
				if (!Environs.Debug) Write("FR" + value + "GZ");
			}
		}

		override public double Amplitude
		{
			set
			{
				String s = "AP" + value + "DM";
				if (!Environs.Debug) Write(s);
			}
		}

		override public bool Enabled
		{
			set
			{
                if (value)
                {
                    if (!Environs.Debug) Write("RF1");
                }
                else
                {
                    if (!Environs.Debug) Write("RF0");
                }
			}
		}

        public override double DCFM
        {
            set
            {
                //TODO
            }
        }

        public override bool DCFMEnabled
        {
            set
            {
                // TODO
            }
        }


	}
}
