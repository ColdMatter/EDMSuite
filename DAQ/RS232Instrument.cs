using System;

using NationalInstruments.VisaNS;

using DAQ.Environment;

namespace DAQ.HAL
{
    /// <summary>
    /// 
    /// </summary>
    public class RS232Instrument : Instrument
    {
        protected SerialSession serial;
        protected string address;
        protected bool connected = false;
        protected int baudrate = 9600;

        public RS232Instrument(String visaAddress)
        {
            this.address = visaAddress;
        }

        public override void Connect()
        {
            Connect(SerialTerminationMethod.LastBit);
        }
        protected void Connect(SerialTerminationMethod method)
        {
            if (!Environs.Debug)
            {
                if (!Environs.Debug)
                {
                    serial = new SerialSession(address);
                    serial.BaudRate = this.baudrate;
                    serial.DataBits = 8;
                    serial.StopBits = StopBitType.One;
                    serial.ReadTermination = method;
                    serial.WriteTermination = method;
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
            if (!Environs.Debug) serial.Write(command);
            Disconnect();
        }

        protected void Write(byte[] command)
        {
            if (!connected) Connect();
            if (!Environs.Debug) serial.Write(command);
            Disconnect();
        }

        protected void Write(string command, bool keepOpen)
        {
            if (!connected) Connect();
            if (!Environs.Debug) serial.Write(command);
            if (!keepOpen)Disconnect();
        }
        protected string Query(string q)
        {
            return serial.Query(q);
        }

        protected override string Read()
        {
            return serial.ReadString();
        }

        protected string Read(int bytes)
        {
            return serial.ReadString(bytes);
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
