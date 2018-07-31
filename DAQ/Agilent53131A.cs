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

        private int channel = 1;

        public int Channel
        {
            set
            {
                channel = value;
            }
        }

        // returns the frequency in Hz, 1s gate time.
        public override double Frequency
        {
            get
            {
                if (!Environs.Debug)
                {
                    Connect();
                    Write(":FUNC 'FREQ " + channel + "'");
                    Write(":FREQ:ARM:STAR:SOUR IMM");
                    Write(":FREQ:ARM:STOP:SOUR TIM");
                    Write(":FREQ:ARM:STOP:TIM 1.0");
                    Write("READ:FREQ?");
                    string fr = Read();
                    Disconnect();
                    return Double.Parse(fr);
                }
                else
                {
                    return 170751000.0 + 2000 *(new Random()).NextDouble();
                }
            }
        }

        public override double Amplitude
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }
    }
}
