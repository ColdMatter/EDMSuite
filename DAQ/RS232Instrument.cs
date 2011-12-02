using System;

using NationalInstruments.VisaNS;

using DAQ.Environment;

namespace DAQ.HAL
{
    /// <summary>
    /// 
    /// </summary>
    public class RS232Instrument
    {
        SerialSession session;
        string address;

        public RS232Instrument(String visaAddress)
        {
            this.address = visaAddress;
        }

        public void Connect()
        {
            if (!Environs.Debug)
            {
                session = new SerialSession(address);

            }
        }

        public void Disconnect()
        {
            if (!Environs.Debug)
            {
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
