using System;

using NationalInstruments.VisaNS;

using DAQ.Environment;

namespace DAQ.HAL
{
	/// <summary>
	/// 
	/// </summary>
	public class GPIBInstrument : Instrument
	{
		GpibSession session;
		string address;

		public GPIBInstrument(String visaAddress)
		{
			this.address = visaAddress;
		}

		public override void Connect()
		{
			if (!Environs.Debug) 
			{
				session = new GpibSession(address);

			}
		}

        public override void Disconnect()
		{
			if (!Environs.Debug)
			{
				// temporarily disabled for HP34401A compatibility
				//session.Write("LCL");
				session.Dispose();
			}
		}

        protected override void Write(String command)
		{
			session.Write(command);
		}

        protected override string Read()
		{
			return session.ReadString();
		}

        protected string Read(int numChars)
        {
            return session.ReadString(numChars);
        }

        protected void Timeout()
        {
            session.Timeout = NationalInstruments.VisaNS.Session.InfiniteTimeout;
        }

        protected void Timeout(int timeoutValue)
        {
            session.Timeout = timeoutValue;
        }
	}
}
