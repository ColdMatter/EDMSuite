using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.VisaNS;
using DAQ.Environment;


                               
namespace DAQ.HAL
{
    public class LakeShore336TemperatureController : RS232Instrument
    {
        protected new int BaudRate = 57600; // Device can accept higher data transfer rate than default for class of 9600
        protected new short DataBits = 7;
        protected new Parity ParitySetting = Parity.Odd;

        public static class CommandTypes
        {
            public static String RequestTemperature { get { return "RDG? "; } } // channel(s) and temperature unit defined in function
        }

        
        public LakeShore336TemperatureController(String visaAddress) : base(visaAddress)
        {
        }

        public LakeShore336TemperatureController(String visaAddress, int baudRate)
            : base(visaAddress)
        {
            this.BaudRate = baudRate;
        }

        public bool ValidateTemperatureRequest(int Channel, string TUnit)
        {
            if ((Channel < 5) && (Channel > -1) && ((TUnit == "K") || (TUnit == "C"))) { return true; }
            else { return false; }
        }

        public string setTUnit(string TUnit)
        {
            if (TUnit == "K") { return String.Concat("K", CommandTypes.RequestTemperature); }
            else { return String.Concat("C", CommandTypes.RequestTemperature); }
        }

        public string SetTChannel(int Channel, string TUnitCommand)
        {
            string[] Channels = { "0", "A", "B", "C", "D" };
            return String.Concat(TUnitCommand, Channels[Channel]);
        }


        public string GetTemperature(int Channel, string TUnit)
        {
            if (ValidateTemperatureRequest(Channel, TUnit))
            {
                string temperatureRequest = SetTChannel(Channel, setTUnit(TUnit));
                Write(temperatureRequest, true);
                string response = Read();
                Disconnect();
                return response;
            }
            else { return "err"; }
        }
        

        

        
    }
}
