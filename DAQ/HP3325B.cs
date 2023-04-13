using System;
using NationalInstruments.Visa;
using Ivi.Visa;

using DAQ.Environment;

namespace DAQ.HAL
{
	/// <summary>
	/// This class represents a GPIB controlled HP3325B synth. It conforms to the Synth
	/// interface.
	/// </summary>
	public class HP3325BSynth : RS232Instrument
	{

		public HP3325BSynth(String visaAddress) : base(visaAddress)
		{}

        public override void Connect()
        {
			if (!Environs.Debug)
			{
				if (!Environs.Debug)
				{
					serial = new SerialSession(address);
					serial.BaudRate = 4800;
					serial.DataBits = 7;
					serial.StopBits = SerialStopBitsMode.One;
					serial.Parity = SerialParity.Even;
					serial.FlowControl = SerialFlowControlModes.XOnXOff;
					serial.ReadTermination = SerialTerminationMethod.HighestBit;
					serial.TerminationCharacter = TerminationCharacter;
				}
				connected = true;
			}
		}

        public double Frequency
		{
			set
			{
				if (!Environs.Debug) Write("FR" + value + "MH");
			}
		}

		// "disable" the synth by knocking it way off resonance
		public bool Enabled
		{
			set
			{
				if (!value) Frequency = 36;
			}
		}
	}
}
