using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.VisaNS;

namespace PaddlePolStabiliser
{
    class SerialControl
    {
        
        SerialSession serial;
        String address;

        public SerialControl(String address)
        {
            this.address = address;
        }

        public void Connect()
        {
            serial = new SerialSession(address);
            serial.BaudRate = 19200;
            serial.DataBits = 8;
            serial.StopBits = StopBitType.One;
            serial.ReadTermination = SerialTerminationMethod.LastBit;
        }

        public void Disconnect()
        {
            serial.Dispose();
        }

        public void Write(String command)
        {
            serial.Write(command);
        }

        public string Read()
        {
            return serial.ReadString(serial.AvailableNumber);
        }

        }
}
