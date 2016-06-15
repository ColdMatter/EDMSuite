using System;


using DAQ.Environment;

namespace DAQ.HAL
{
	/// <summary>
	/// This class represents a GPIB controlled HP5350B rf counter. It conforms to the Counter
	/// interface.
	/// </summary>
	public class HP5350BCounter : FrequencyCounter
	{
        private char[] delimiterChars = { ' ', '\r', '\n' };

        public HP5350BCounter(String visaAddress) : base(visaAddress)
        {}

        public override void Connect()
        {
            base.Connect();
            TerminationCharacter(true);
        }

        private double tempFreq = 0.0;
		override public double Frequency
		{
			get
			{
                if (!Environs.Debug)
                {
                    double val;
                    Write("trg");
                    string s = Read();
                    double.TryParse(s, out val);
                    if (val < 20E9) tempFreq = val; // Range of counter is 20 GHz
                }
                return tempFreq;
			}
		}

        public override double Amplitude
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }
	}
}
