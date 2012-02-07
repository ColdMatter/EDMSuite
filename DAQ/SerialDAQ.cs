using System;

using NationalInstruments.VisaNS;

using DAQ.Environment;


namespace DAQ.HAL
{
	/// <summary>
	/// This is is the interface to the serial DAQ board
	/// </summary>
	public class SerialDAQ : DAQ.HAL.RS232Instrument
	{

        public SerialDAQ(String address) : base(address) { }

        public void SetOut1(double vout)
        {
            Write("VOUTA = " + Convert.ToString(vout) + "\r\n");
        }

        public void SetOut2(double vout)
        {
            Write("VOUTB = " + Convert.ToString(vout) + "\r\n");
        }

        public double ReadVin1()
        {
            return QueryDouble("VINA?\n");
        }

        public double ReadVin2()
        {
            return QueryDouble("VINB?\n");
        }


	}
}
