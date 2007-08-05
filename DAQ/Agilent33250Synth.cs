using System;
using DAQ.Environment;


namespace DAQ.HAL
{
    /// <summary>
    /// This class represents a GPIB controlled Agilent 33250 arbitrary waveform generator. It conforms to the Synth
    /// interface.
    /// </summary>
    class Agilent33250Synth : Synth
    {
        public Agilent33250Synth(String visaAddress) : base(visaAddress)
        { }

        override public double Frequency
        {
            set
            {
                if (!Environs.Debug) Write("FREQ " + value); // the value is entered in Hz
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
