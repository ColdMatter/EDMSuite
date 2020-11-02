using System;

using NationalInstruments.Visa;
using Ivi.Visa;

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
		private bool connected = false;

		public BrilliantLaser(String address)
		{
			this.address = address;
		}

		private void Connect()
		{
			if (!Environs.Debug) 
			{
				serial = new SerialSession(address);
				serial.BaudRate = 9600;
				serial.DataBits = 8;
				serial.StopBits = SerialStopBitsMode.One;
				serial.ReadTermination = SerialTerminationMethod.HighestBit;
			}
			connected = true;
		}

		private void Disconnect()
		{
			if (!Environs.Debug) serial.Dispose();
			connected = false;
		}

		public void StartFlashlamps(bool internalClock)
		{
			if (!connected) Connect();
			if (!Environs.Debug)
			{
				serial.RawIO.Write("QE\r\n");
				serial.RawIO.ReadString(17);
				//serial.Query("QE\r\n", 17);
			}
			if (internalClock)
			{
				if (!Environs.Debug)
				{
					serial.RawIO.Write("A\r\n");
					serial.RawIO.ReadString(17);
					//serial.Query("A\r\n", 17);
				}
			}
			else
			{
				if (!Environs.Debug)
				{
					serial.RawIO.Write("E\r\n");
					serial.RawIO.ReadString(17);
					//serial.Query("E\r\n",17);
				}
			}
			Disconnect();
		}

		public void StopFlashlamps()
		{
			if (!connected) Connect();
			if (!Environs.Debug)
			{
				serial.RawIO.Write("S\r\n");
				serial.RawIO.ReadString(17);
				//serial.Query("S\r\n", 17);
			}
			Disconnect();
		}

		public void EnableQSwitch()
		{
			if (!connected) Connect();
			if (!Environs.Debug)
			{
				serial.RawIO.Write("CC\r\n");
				serial.RawIO.ReadString(17);
				//serial.Query("CC\r\n", 17);
			}
			Disconnect();
		}

		public void DisableQSwitch()
		{
			if (!connected) Connect();
			if (!Environs.Debug)
			{
				serial.RawIO.Write("CS\r\n");
				serial.RawIO.ReadString(17);
				//serial.Query("CS\r\n", 17);
			}
			Disconnect();
		}

		public bool InterlockFailed
		{
			get
			{
				if (!connected) Connect();
				String reply = "";
				if (!Environs.Debug)
				{
					serial.RawIO.Write("IF2\r\n");
					reply = serial.RawIO.ReadString(17);
					//reply = serial.Query("IF2\r\n", 17);
				}
				Disconnect();
				return (reply.IndexOf("1") != -1);
				
			}
		}
	
		public void SetFlashlampVoltage(int voltage)
		{
			if (!connected) Connect();
			if (!Environs.Debug)
			{
				serial.RawIO.Write("V" + voltage + "\r\n");
				serial.RawIO.ReadString(17);
				//serial.Query("V" + voltage + "\r\n", 17);
			}
			Disconnect();
		}

		public void PatternChangeStartingHandler(object source, EventArgs e)
		{}

		public void PatternChangeEndedHandler(object source, EventArgs e)
		{}

	}
}
