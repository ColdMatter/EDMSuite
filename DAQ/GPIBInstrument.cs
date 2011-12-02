using System;

using NationalInstruments.VisaNS;

using DAQ.Environment;

namespace DAQ.HAL
{
	/// <summary>
	/// 
	/// </summary>
	public class GPIBInstrument
	{
		GpibSession session;
		string address;

		public GPIBInstrument(String visaAddress)
		{
			this.address = visaAddress;
		}

		public void Connect()
		{
			if (!Environs.Debug) 
			{
				session = new GpibSession(address);

			}
		}

		public void Disconnect()
		{
			if (!Environs.Debug)
			{
				// temporarily disabled for HP34401A compatibility
				//session.Write("LCL");
				session.Dispose();
			}
		}

		protected void Write(String command)
		{
			session.Write(command);
		}

		protected string Read()
		{
			return session.ReadString();
		}
	}
}
