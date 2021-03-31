using System;

using NationalInstruments.Visa;
using Ivi.Visa;

using DAQ.Environment;

namespace DAQ.HAL
{
    /// <summary>
    /// 
    /// </summary>
    public class RS232Instrument : Instrument
    {
        // These are overrideable by child classes
        protected int BaudRate = 9600;
        protected short DataBits = 8;
        protected SerialStopBitsMode StopBit = SerialStopBitsMode.One;
        protected SerialParity ParitySetting = SerialParity.None;
        protected SerialFlowControlModes FlowControl = SerialFlowControlModes.None;
        protected byte TerminationCharacter = 0xa;
        
        protected SerialSession serial;
        protected string address;
        protected bool connected = false;

        public RS232Instrument(String visaAddress)
        {
            this.address = visaAddress;
        }

        public override void Connect()
        {
            Connect(SerialTerminationMethod.HighestBit);
        }
        protected void Connect(SerialTerminationMethod method)
        {
            if (!Environs.Debug)
            {
                if (!Environs.Debug)
                {
                    serial = new SerialSession(address);
                    serial.BaudRate = BaudRate;
                    serial.DataBits = DataBits;
                    serial.StopBits = StopBit;
                    serial.Parity = ParitySetting;
                    serial.FlowControl = FlowControl; 
                    serial.ReadTermination = method;
                    serial.TerminationCharacter = TerminationCharacter;
                }
                connected = true;
            }
        }

        public override void Disconnect()
        {
            if (!Environs.Debug) serial.Dispose();
            connected = false;
        }

        protected override void Write(string command)
        {
            if (!connected) Connect();
            if (!Environs.Debug) serial.RawIO.Write(command);
            Disconnect();
        }

        protected void Write(byte[] command)
        {
            if (!connected) Connect();
            if (!Environs.Debug) serial.RawIO.Write(command);
            Disconnect();
        }

        protected void Write(string command, bool stayOpen)
        {
            if (!connected) Connect();
            if (!Environs.Debug) serial.RawIO.Write(command);
            if (!stayOpen) Disconnect();
        }

        protected string Query(string q)
        {
            serial.RawIO.Write(q);
            return serial.RawIO.ReadString();
            //return serial.Query(q);
        }

        protected override string Read()
        {
            return serial.RawIO.ReadString();
        }

        protected string Read(int bytes)
        {
            return serial.RawIO.ReadString(bytes);
        }

        protected void Clear()
        {
            serial.Clear();
        }

        protected double QueryDouble(string q)
        {
            double d = 0.0;
            if (!connected) Connect();
            if (!Environs.Debug) d = Convert.ToDouble(Query(q));
            Disconnect();
            return d;
        }
    }
}
