using System;
using System.Linq;


using DAQ.Environment;
using System.Threading;

namespace DAQ.HAL
{
    /// <summary>
    /// This class represents a USB controlled AnapicoSYN420 multichannel synth. 
    /// </summary>
    public class AnapicoSYN420 : USBInstrument
    {
        private int numberOfChannels;
        public AnapicoSYN420(String visaAddress)
            : base(visaAddress)
        {    
            numberOfChannels = 2;
        }

        public int NumberOfChannels
        {
            get { return numberOfChannels; }
        }

        // 0 for disable, 1 for enable
        public bool Enabled
        {
            set
            {
                if (value)
                {
                    Write(":OUTP:STAT ON\n");
                    for (int i = 0; i < numberOfChannels; i++)
                    {
                        Write(":SOUR" + (i + 1).ToString() + ":COUP OFF\n");
                    }
                }
                else Write(":OUTP:STAT OFF\n");
            }
        }

        public double CWFrequency
        {
            set
            {
                if (!Environs.Debug) Write(":SOUR:FREQ:CW " + value + "\n");
            }
        }

        public bool ListSweepEnabled
        {
            set
            {
                if (value)
                {
                    //annoyingly, I can't seem to write commands to all channels, and if no channel is given you write to the default channel only.
                    for (int i = numberOfChannels; i> 0; i--)
                    {
                        if (i == 0)
                        {
                            Write(":SOUR" + (i + 1).ToString() + ":FREQ:MODE LIST\n"); // Sets frequency to sweep mode.
                            Write(":SOUR" + (i + 1).ToString() + ":POW:MODE LIST\n"); // Sets power to sweep mode.
                            Write(":SOUR" + (i + 1).ToString() + ":LIST:COUN 1\n"); // Set repetitions to 1, not infinite!      
                        }
                        else
                        {
                            Write(":SOUR" + (i + 1).ToString() + ":FREQ:MODE FIX\n"); // Sets frequency to CW mode
                        }
                    }
                    
                    Write(":INIT:CONT ON"); // Sets trigger mode to Repeat.
                   
                    Write(":TRIG:TYPE NORM\n"); // Sets trigger parameter to execute complete list.
                    Write(":TRIG:SOUR EXT\n"); // Sets trigger source to external.
                    Write(":TRIG:SLOP POS\n"); // Sets trigger edge to rising.
                    Write(":TRIG:OUTP:MODE NORM\n"); // Sets trigger output to normal.
                }
                else
                {
                    for (int i = 0; i < numberOfChannels; i++)
                    {
                        Write(":SOUR" + (i + 1).ToString() + ":FREQ:MODE FIX\n"); // Sets frequency mode back to CW.
                        Write(":SOUR" + (i + 1).ToString() + ":POW:MODE FIX\n"); // Sets power mode back to CW
                    }
                }
            }
        }

        public void WriteList(string list)
        {
            Write(":MEM:FILE:LIST:DATA " + list);
        }

        public void WriteList(string[] chList)
        {
            for (int i=0; i< numberOfChannels; i++)
            {
                int numBytes = chList[i].Length;
                int numDigits = numBytes.ToString().Length;
                Write(":SOUR:SEL " + (i + 1).ToString() +"\r\n");   
                Write(":MEM:FILE:LIST:DATA " + "#" + numDigits.ToString() + numBytes.ToString() + chList[i]);
            }
            
        }

        public string ReadList()
        {
            Write(":MEM:FILE:LIST:DATA?\n");

            return Read();
        }
        
        public string[] ReadChannelList()
        {
            string[] chList = new string[numberOfChannels];
            for (int i = 0; i < numberOfChannels; i++)
            {
                Write(":MEM" + (i+1).ToString() + ":FILE:LIST:DATA?\n");
                chList[i] = Read();
            }
            return chList; 
        }
        


    }
}
