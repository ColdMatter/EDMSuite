using System;
using System.Collections.Generic;
using System.Text;


using DAQ.Environment;

namespace DAQ.HAL
{
    public class Agilent53131A : FrequencyCounter
    {
        public Agilent53131A(String visaAddress) : base(visaAddress)
		{}

        public override double Frequency
        {
            get
            {
                if (!Environs.Debug)
                {
                    Write(":FUNC 'FREQ 1'");
                    Write(":FREQ:ARM:STAR:SOUR IMM");
                    Write(":FREQ:ARM:STOP:SOUR TIM");
                    Write(":FREQ:ARM:STOP:TIM 1.0");
                    Write("READ:FREQ?");
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
