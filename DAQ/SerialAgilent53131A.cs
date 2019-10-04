using System;

using NationalInstruments.VisaNS;

using DAQ.Environment;
using System.Globalization;
using System.Threading;


namespace DAQ.HAL
{
	/// <summary>
	/// This is is the interface to the serial DAQ board
	/// </summary>
	public class SerialAgilent53131A : DAQ.HAL.RS232Instrument
	{

        public SerialAgilent53131A(String address) : base(address) { }

        public double Frequency()
        {
            if (!Environs.Debug)
            {
                string fr;
                NumberStyles styles;

                //First flush the input buffer.;
                Connect();
                Clear();
                fr = Read(20);

                //Now parse the string.
                // Data is output in the form "val MHz\r\n.." so split in two
                // and take only first element. The counter also spews the data
                // out in the form 85.823,323,232 MHz so get rid of all nasty
                // commas
                char[] delimiterChars = { ' ' };
                string[] words = fr.Split(delimiterChars);
                styles = NumberStyles.AllowParentheses | NumberStyles.AllowTrailingSign |
                            NumberStyles.Float | NumberStyles.AllowThousands;
                return 1e6 * Double.Parse(words[0].Replace(",", ""), styles);
            }
            else
            {
                return 170751000.0 + 2000 * (new Random()).NextDouble();
            }
        }
	}
}
