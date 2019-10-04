using System;
using DAQ.Environment;


namespace DAQ.HAL
{
    /// <summary>
    /// This class represents a GPIB controlled Gigatronics 7100 arbitrary waveform generator. It conforms to the Synth
    /// interface.
    /// </summary>
    public class Gigatronics7100Synth : Synth
    {
        public Gigatronics7100Synth(String visaAddress)
            : base(visaAddress)
        { }

        override public double Frequency
        {
            set
            {
                if (!Environs.Debug) Write("CW" + value + "MZ"); // the value is entered in MHz
            }
        }

        public override double Amplitude
        {
            set 
            {
                if (!Environs.Debug) Write("PL" + value + "DM"); // the value is entered in dBm
            } // do nothing
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

        public double PulseDuration
        {
            set
            {
                if (!Environs.Debug)
                {
                    Write("PM4");
                    Write("PW" + value + "US");
                }
            }
        }
    }
}
