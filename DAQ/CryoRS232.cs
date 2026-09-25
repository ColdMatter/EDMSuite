using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.Visa;
using Ivi.Visa;
using DAQ.Environment;
using System.Threading;

namespace DAQ.HAL
{
    public class CryoRS232 : RS232Instrument
    {
        // Serial connection parameters for the Alicat Flow Controller:
        protected new int BaudRate = 9600; // Device can accept higher data transfer rate than default for class of 9600
        protected new short DataBits = 8;
        protected new SerialStopBitsMode StopBit = SerialStopBitsMode.One;
        protected new SerialParity ParitySetting = SerialParity.None;
        protected new byte TerminationCharacter = 0xD;

        /// <summary>
        /// Constructor for this class
        /// </summary>
        /// <param name="visaAddress"></param>
        /// 
        public CryoRS232(String visaAddress)
            : base(visaAddress)
        {
            base.BaudRate = 9600;
            base.DataBits = 8;
            base.ParitySetting = SerialParity.None;
            base.TerminationCharacter = 0xD;
            base.StopBit = SerialStopBitsMode.One;
        }



        /// <summary>
        /// This is a command to set the cryo state
        /// It will return a string from the cryo comfirming state 
        /// </summary>
        /// <param name="ControllerAddress"></param>
        /// <param name="State"></param>
        /// <returns></returns>

        public override void Connect()
        {
            if (!Environs.Debug)
            {
                if (!Environs.Debug)
                {
                    serial = new SerialSession(address);
                    serial.BaudRate = 9600;
                    serial.DataBits = 8;
                    serial.StopBits = SerialStopBitsMode.One;
                    serial.Parity = SerialParity.None;
                    serial.TerminationCharacter = 0xD;
                    serial.ReadTermination = SerialTerminationMethod.TerminationCharacter;
                    serial.TerminationCharacter = TerminationCharacter;
                }
                connected = true;
            }
        }
        public string cryoSetState(string ControllerAddress, bool state)
        {
            Connect();
            string command = "$OFF9188";
            switch (state)
            {
                case true:
                        command = "$ON177CF";
                    break;
                case false:
                        command = "$OFF9188";
                    break;
            }
            
            string cryoState = String.Concat(ControllerAddress, command, "\r");
            string response = Query(cryoState);
            Disconnect();
            return response;

        }
    }
}
