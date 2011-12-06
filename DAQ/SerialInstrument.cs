using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NationalInstruments.VisaNS;

namespace DAQ.HAL
{
    class SerialInstrument : Instrument
    {
        SerialSession serial;
        String address;


        public SerialInstrument(String address)
        {
            this.address = address;
        }

        public override void Connect()
        {
            serial = new SerialSession(address);
            serial.BaudRate = 9600;
            serial.DataBits = 8;
            serial.StopBits = StopBitType.One;
            serial.ReadTermination = SerialTerminationMethod.LastBit;
        }

        public override void Disconnect()
        {
            serial.Dispose();
        }

        protected override void Write(String command)
        {
            serial.Write(command);
        }

        protected override string Read()
        {
            return serial.ReadString();
        }
    }
}
