using System;
using DAQ.Environment;
using NationalInstruments.Visa;
using Ivi.Visa;

namespace DAQ.HAL
{
    public class SerialInstrument : Instrument
    {
        // These are overrideable by child classes
        protected int BaudRate = 9600;
        protected short DataBits = 8;
        protected SerialStopBitsMode StopBit = SerialStopBitsMode.One;
        protected SerialParity ParitySetting = SerialParity.None;
        protected SerialFlowControlModes FlowControl = SerialFlowControlModes.None;
        protected byte TerminationCharacter = 0xa;
        protected int TimeoutMilliseconds = 1000;

        protected SerialSession serial;
        protected string address;
        protected bool connected = false;

        public SerialInstrument(String address)
        {
            this.address = address;
        }

        public override void Connect()
        {
            Connect(SerialTerminationMethod.HighestBit);
        }

        protected void Connect(SerialTerminationMethod ReadTerminationMethod)
        {
            if (!Environs.Debug)
            {
                serial = new SerialSession(address);
                serial.BaudRate = BaudRate;
                serial.DataBits = DataBits;
                serial.StopBits = StopBit;
                serial.ReadTermination = ReadTerminationMethod;
                serial.TerminationCharacter = TerminationCharacter;
                serial.TimeoutMilliseconds = TimeoutMilliseconds;
            }
            connected = true;            
        }

        public override void Disconnect()
        {
            if (!Environs.Debug)
            {
                serial.Dispose();
            }
            connected = false;
        }

        protected override void Write(String command)
        {
            if (!connected) Connect();
            serial.RawIO.Write(command);
            Disconnect();
        }

        protected override string Read()
        {
            return serial.RawIO.ReadString();
        }
    }
}
