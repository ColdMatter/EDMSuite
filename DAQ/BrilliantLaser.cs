using System;

using NationalInstruments.VisaNS;

using DAQ.Environment;


namespace DAQ.HAL
{
	/// <summary>
	/// This class, which conforms to the YAG laser interface, talks to the Brilliant
	/// YAG laser over RS232.
	/// </summary>
	public class BrilliantLaser : DAQ.HAL.YAGLaser
	{
		SerialSession serial;
		String address;
		private bool isConnected = false;

		public BrilliantLaser(String address)
		{
			this.address = address;
		}

		private void Connect()
		{
			if (!Environs.Debug) serial = new SerialSession(address);
			serial.BaudRate = 9600;
			serial.DataBits = 8;
			serial.StopBits = StopBitType.One;
			serial.ReadTermination = SerialTerminationMethod.LastBit;
			isConnected = true;
		}

		private void Disconnect()
		{
			if (!Environs.Debug) serial.Dispose();
			isConnected = false;
		}

		public void StartFlashlamps(bool internalClock)
		{
			if (!isConnected) Connect();
			if (!Environs.Debug) serial.Query("QE\r\n",17);
			if (internalClock)
			{
				if (!Environs.Debug) serial.Query("A\r\n",17);
			}
			else
			{
				if (!Environs.Debug)
				{
					serial.Query("E\r\n",17);
				}
			}
			Disconnect();
		}

		public void StopFlashlamps()
		{
			if (!isConnected) Connect();
			if (!Environs.Debug) serial.Query("S\r\n",17);
			Disconnect();
		}

		public void EnableQSwitch()
		{
			if (!isConnected) Connect();
			if (!Environs.Debug) serial.Query("CC\r\n",17);
			Disconnect();
		}

		public void DisableQSwitch()
		{
			if (!isConnected) Connect();
			if (!Environs.Debug) serial.Query("CS\r\n",17);
			Disconnect();
		}

		public bool InterlockFailed
		{
			get
			{
				if (!isConnected) Connect();
				String reply = "";
				if (!Environs.Debug) reply = serial.Query("IF2\r\n",17);
				Disconnect();
				return (reply.IndexOf("1") != -1);
				
			}
		}		

		public void PatternChangeStartingHandler(object source, EventArgs e)
		{}

		public void PatternChangeEndedHandler(object source, EventArgs e)
		{}

	}
}
