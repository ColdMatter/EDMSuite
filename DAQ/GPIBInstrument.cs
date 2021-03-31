using System;

using NationalInstruments.Visa;
using Ivi.Visa;

using DAQ.Environment;

namespace DAQ.HAL
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class GPIBInstrument : Instrument
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
				session.TimeoutMilliseconds = VisaConstants.InfiniteTimeout;

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
			session.RawIO.Write(command);
		}

        protected override string Read()
		{
			return session.RawIO.ReadString();
		}

        protected string Read(int numChars)
        {
            return session.RawIO.ReadString(numChars);
        }

        protected void Timeout()
        {
            session.TimeoutMilliseconds = VisaConstants.InfiniteTimeout;
        }

        protected void Timeout(int timeoutValue)
        {
            session.TimeoutMilliseconds = timeoutValue;
        }

        protected void TerminationCharacter(bool enabled)
        {
            session.TerminationCharacterEnabled = enabled;
        }
	}
}
