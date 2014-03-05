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

		override public double Frequency
		{
			get
			{
                double val = 0.0;
                if (!Environs.Debug)
                {
                    Write("trg");
                    string s = Read();
                    val = double.Parse(s);
                }
                return val;
			}
		}

        public override double Amplitude
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }
	}
}
