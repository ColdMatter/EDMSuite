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

        public bool EnablePulseMode
        {
            set
            {
                if (value)
                {
                    Write("SOUR1:PULM:STAT ON\n");
                    Write("SOUR1:PULM:SOUR EXT\n");

                    Write("SOUR2:PULM:STAT ON\n");
                    Write("SOUR2:PULM:SOUR EXT\n");
                }
                else
                {
                    Write("SOUR1:PULM:STAT OFF\n");
                    Write("SOUR2:PULM:STAT OFF\n");
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

        public double PowerCH1
        {
            get
            {
                double power = 0.0;
                string list = "";
                if (!Environs.Debug)
                {
                    Write("SOUR:SEL 1\n");
                    Write(":SOUR:POW:LEV:IMM:AMPL?\n");
                    list = Read();
                    char[] delimiters = { ';', '\r', '\n' };
                    string[] splitList = list.Split(delimiters);
                    power = Convert.ToDouble(splitList[0]);
                }
                return power;
            }

            set
            {
                if (!Environs.Debug)
                {
                    Write(":SOUR:SEL 1\n");
                    Write(":SOUR:POW:MODE CW\n");
                    Write(":SOUR:POW:ALC:STAT OFF\n");                    // Disables Automatic Leveling Control (ALC)
                    //Write(":SOUR:POW:ALC:BAND:AUTO ON\n");               // Sets the bandwidth of the ALC to automatic
                    Write(":SOUR:POW:LEV:IMM:AMPL " + value + "\n");     // In units of dBm

                }
            }
        }


        public double PowerCH2
        {
            get
            {
                double power = 0.0;
                string list = "";
                if (!Environs.Debug)
                {
                    Write("SOUR:SEL 2\n");
                    Write(":SOUR:POW:LEV:IMM:AMPL?\n");
                    list = Read();
                    char[] delimiters = { ';', '\r', '\n' };
                    string[] splitList = list.Split(delimiters);
                    power = Convert.ToDouble(splitList[0]);
                }
                return power;
            }

            set
            {
                string list = "";
                if (!Environs.Debug)
                {
    
                    Write(":SOUR:SEL 2\n");
                    Write(":SOUR:POW:MODE CW\n");
                    Write(":SOUR:POW:LEV:IMM:AMPL?\n");
                    list = Read();// read the current power for debugging purpose, Guanchen 19Aug2025

                    Write(":SOUR:POW:ALC:STAT ON\n");                    // Disables Automatic Leveling Control (ALC)
                    Write(":SOUR:POW:ALC:BAND:AUTO ON\n");               // Sets the bandwidth of the ALC to automatic
                    Write(":SOUR:POW:LEV:IMM:AMPL " + value + "\n");     // In units of dBm
                    Write("SOUR:SEL 2\n");
                    Write(":SOUR:POW:LEV:IMM:AMPL?\n");
                    list = Read();// read the new power for debugging purpose, Guanchen 19Aug2025

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


        public double FMDeviationCH1
        {
            get
            {
                double dev = 0.0;
                string list = "";
                if (!Environs.Debug)
                {
                    Write("SOUR:SEL 1\n");
                    Write(":SOUR:FM:DEV?\n");
                    list = Read();
                    char[] delimiters = { ';', '\r', '\n' };
                    string[] splitList = list.Split(delimiters);
                    dev = Convert.ToDouble(splitList[0]);
                }
                return dev;
            }

            set
            {
                if (!Environs.Debug)
                {
                    Write(":SOUR:SEL 1\n");
                    Write(":SOUR:FM:DEV " + value + "\n");
                }
            }
        }

        public double FMDeviationCH2
        {
            get
            {
                double dev = 0.0;
                string list = "";
                if (!Environs.Debug)
                {
                    Write("SOUR:SEL 2\n");
                    Write(":SOUR:FM:DEV?\n");
                    list = Read();
                    char[] delimiters = { ';', '\r', '\n' };
                    string[] splitList = list.Split(delimiters);
                    dev = Convert.ToDouble(splitList[0]);
                }
                return dev;
            }

            set
            {
                if (!Environs.Debug)
                {
                    Write(":SOUR:SEL 2\n");
                    Write(":SOUR:FM:DEV " + value + "\n");
                }
            }
        }

        //Frequency modulation for output from ch1
        public bool FrequencyModulationCH1Enabled
        {
            set
            {
                if (value)
                {
                    Write(":SOUR:SEL 1\n"); // Channel to apply FM on -> Ch 1
                    Write(":SOUR:FM:SOUR INT\n"); // Signal source for frequency modulation set to internal
                    Write(":SOUR:FM:INT:SHAP SINE\n");// Frequency is modulated according to a sine wave
                    Write(":SOUR:FM:INT:FREQ 800000\n"); // Frequency is modulated at the maximum rate of 800 kHz
                    Write(":SOUR:FM:STAT ON\n"); // Enable FM
                }
                else
                {
                    Write(":SOUR:SEL 1\n"); // select channel 1
                    Write(":SOUR:FM:STAT OFF\n"); // disable FM
                    Write(":SOUR:FREQ:MODE FIX\n"); // Sets frequency mode back to CW.
                }
            }
        }

        //Frequency modulation for output from ch2
        public bool FrequencyModulationCH2Enabled
        {
            set
            {
                if (value)
                {
                    Write(":SOUR:SEL 2\n"); // Channel to apply FM on -> Ch 2
                    Write(":SOUR:FM:SOUR INT\n"); // Signal source for frequency modulation set to internal
                    Write(":SOUR:FM:INT:SHAP SINE\n");// Frequency is modulated according to a sine wave
                    Write(":SOUR:FM:INT:FREQ 800000\n"); // Frequency is modulated at a rate of 500 Hz
                    Write(":SOUR:FM:STAT ON\n"); // Enable FM
                }
                else
                {
                    Write(":SOUR:SEL 2\n"); // select channel 2
                    Write(":SOUR:FM:STAT OFF\n"); // disable FM
                    Write(":SOUR:FREQ:MODE FIX\n"); // Sets frequency mode back to CW.
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
