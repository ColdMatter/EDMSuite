using System;
using DAQ.Environment;


namespace DAQ.HAL
{
    /// <summary>
    /// This class represents a GPIB controlled Agilent 33250 arbitrary waveform generator. It conforms to the Synth
    /// interface.
    /// </summary>
    class Gigatronics7100Synth : Synth
    {
        public Gigatronics7100Synth(String visaAddress) : base(visaAddress)
        { }

        override public double Frequency
        {
            set
            {
                if (!Environs.Debug) Write("CW " + value + "KZ"); // the value is entered in kHz
               System.Threading.Thread.Sleep(35);
            }
        }

        public override double Amplitude
        {
            set
            {
                if (!Environs.Debug) Write("PL " + value + "DM"); // the value is entered in dBm. Do not send more than +10!
                System.Threading.Thread.Sleep(40);
            }
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
