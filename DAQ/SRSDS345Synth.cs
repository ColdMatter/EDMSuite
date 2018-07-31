using System;


using DAQ.Environment;

namespace DAQ.HAL
{
    /// <summary>
    /// This class represents a GPIB controlled HP8656B synth. It conforms to the Synth
    /// interface. 
    /// </summary>
    public class SRSDS345Synth : Synth
    {

        public SRSDS345Synth(String visaAddress)
            : base(visaAddress)
        { }

        override public double Frequency
        {
            set
            {
                if (!Environs.Debug) Write("FREQ" + (1000000 * value));
            }
        }

        // amplitude not supported on the 3325
        override public double Amplitude
        {
            set
            {
            }
        }

        // DCFM not yet supported on the 3325
        public override bool DCFMEnabled
        {
            set
            {
            }
        }
        public override double DCFM
        {
            set
            {
            }
        }



        // "disable" the synth by knocking it way off resonance
        override public bool Enabled
        {
            set
            {
                if (!value) Frequency = 36;
            }
        }
    }
}
