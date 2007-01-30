using System;
using DAQ.Environment;


namespace DAQ.HAL
{
    /// <summary>
    /// This class represents a GPIB controlled EIP578 microwave frequency counter. It conforms to the Synth
    /// interface.
    /// </summary>
    class EIP578Synth : Synth
    {
        public EIP578Synth(String visaAddress) : base(visaAddress)
		{}

		override public double Frequency
		{
			set
			{
				if (!Environs.Debug) Write("PF" + value + "K");
			}
		}

        public override double Amplitude
        {
            set { } // do nothing
        }

        public override double DCFM
        {
            set { } // do nothing
        }

        public override bool DCFMEnabled
        {
            set { } // do nothing
        }

        public override bool Enabled
        {
            set { } // do nothing
        }
    }
}
