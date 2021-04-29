using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NationalInstruments.Visa;
using Ivi.Visa;

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
            serial.StopBits = SerialStopBitsMode.One;
            serial.ReadTermination = SerialTerminationMethod.HighestBit;
        }

        public override void Disconnect()
        {
            serial.Dispose();
        }

        protected override void Write(String command)
        {
            serial.RawIO.Write(command);
        }

        protected override string Read()
        {
            return serial.RawIO.ReadString();
        }
    }
}
