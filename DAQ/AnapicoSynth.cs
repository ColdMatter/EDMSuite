using System;
using System.Linq;


using DAQ.Environment;
using System.Threading;

namespace DAQ.HAL
{
	/// <summary>
	/// This class represents a GPIB controlled HP8673B synth. It conforms to the Synth
	/// interface.
	/// </summary>
	public class AnapicoSynth : USBInstrument
	{

		public AnapicoSynth(String visaAddress) : base(visaAddress)
        {}

        // 0 for disable, 1 for enable
        public bool Enabled
        {
            set
            {
                if (value)
                {
                    Write(":OUTP1:STAT ON\n");
                    Write(":OUTP2:STAT ON\n");
                }
                else
                {
                    Write(":OUTP1:STAT OFF\n");
                    Write(":OUTP2:STAT OFF\n");
                }
            }
        }

		public double CWFrequencyCH1
		{
            get
            {
                double freq = 0.0;
                string list = "";
                if (!Environs.Debug)
                {
                    Write("SOUR:SEL 1\n");
                    Write(":SOUR:FREQ:CW?\n");
                    list = Read();
                    char[] delimiters = { ';', '\r', '\n' };
                    string[] splitList = list.Split(delimiters);
                    freq = Convert.ToDouble(splitList[0]);
                }
                return freq;
            }

			set
			{
                if (!Environs.Debug)
                {
                    Write(":SOUR:SEL 1\n");
                    Write(":SOUR:FREQ:CW " + value + "\n");
                }
			}
		}

        public double CWFrequencyCH2
        {
            get
            {
                double freq = 0.0;
                string list = "";
                if (!Environs.Debug)
                {
                    Write("SOUR:SEL 2\n");
                    Write(":SOUR:FREQ:CW?\n");
                    list = Read();
                    char[] delimiters = { ';', '\r', '\n' };
                    string[] splitList = list.Split(delimiters);
                    freq = Convert.ToDouble(splitList[0]);
                }
                return freq;
            }

            set
            {
                if (!Environs.Debug)
                {
                    Write(":SOUR:SEL 2\n");
                    Write(":SOUR:FREQ:CW " + value + "\n");
                }
            }
        }

        //ListSweep implemented for output from ch1
        public bool ListSweepEnabled
        {
            set
            {
                if (value)
                {
                    Write(":SOUR:SEL 1\n");
                    Write(":SOUR:FREQ:MODE LIST\n"); // Sets frequency to sweep mode.
                    Write(":SOUR:POW:MODE LIST\n"); // Sets power to sweep mode.
                    Write(":SOUR:LIST:COUN 1\n"); // Set repetitions to 1, not infinite!
                    Write(":INIT:CONT ON"); // Sets trigger mode to Repeat.
                    Write(":TRIG:TYPE NORM\n"); // Sets trigger parameter to execute complete list.
                    Write(":TRIG:SOUR EXT\n"); // Sets trigger source to external.
                    Write(":TRIG:SLOP POS\n"); // Sets trigger edge to rising.
                    Write(":TRIG:OUTP:MODE NORM\n"); // Sets trigger output to normal.
                }
                else
                {
                    Write(":SOUR:SEL 1\n");
                    Write(":SOUR:FREQ:MODE FIX\n"); // Sets frequency mode back to CW.
                    Write(":SOUR:POW:MODE FIX\n"); // Sets power mode back to CW
                }
            }
        }

        public void WriteList(string list)
        {
            Write(":MEM:FILE:LIST:DATA " + list);
        }

        public string ReadList()
        {

            Write(":MEM:FILE:LIST:DATA?\n");

            return Read();
        }

		//override public double Amplitude
		//{
		//	set
		//	{
		//		String s = "AP" + value + "DM";
		//		if (!Environs.Debug) Write(s);
		//	}
		//}


        //public override double DCFM
        //{
        //    set
        //    {
                //TODO
        //    }
        //}

        //public override bool DCFMEnabled
        //{
        //    set
        //    {
                // TODO
        //    }
        //}


	}
}
