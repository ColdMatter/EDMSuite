using System;
using System.Collections.Generic;
using System.Text;


using DAQ.Environment;

namespace DAQ.HAL
{
    public class EIP575 : FrequencyCounter
    {
 		public EIP575(String visaAddress) : base(visaAddress)
		{}

        public override double Frequency
        {
            get
            {
                if (!Environs.Debug)
                {

                    Write("B2");
                    Write("FR");
                    string fr = Read();
                    return Double.Parse(fr);
                }
                else
                {
                    return 170.730 + (new Random()).NextDouble();
                }
            }
        }

        public override double Amplitude
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }
    }
}
