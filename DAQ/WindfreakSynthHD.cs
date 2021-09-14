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
    public class WindfreakSynthHD : RS232Instrument
    {
        public static class CommandTypes
        {
            public static String RequestFrequency { get { return "f?"; } } 
            public static String SetFrequency { get { return "f"; } }
            public static String RequestPower { get { return "W?"; } }
            public static String SetPower { get { return "W"; } }
            public static String SetRFmuted { get { return "h?"; } }
            public static String SetPAPowerOn { get { return "r?"; } }
            public static String SetPLLPowerOn { get { return "E?"; } }
            public static String RFmuted { get { return "h"; } }
            public static String PAPowerOn { get { return "r"; } }
            public static String PLLPowerOn { get { return "E"; } }
        }

        // Serial connection parameters for the Windfreak SynthHD:
        protected new int BaudRate = 57600; // Device can accept higher data transfer rate than default for class of 9600
        protected new short DataBits = 8;
        protected new SerialParity ParitySetting = SerialParity.None;

        /// <summary>
        /// Constructor for this class
        /// </summary>
        /// <param name="visaAddress"></param>
        public WindfreakSynthHD(String visaAddress)
            : base(visaAddress)
        {
            base.BaudRate = 57600;
            base.DataBits = 8;
            base.ParitySetting = SerialParity.None;
        }

        public double GetPower()
        {
            Connect(SerialTerminationMethod.TerminationCharacter);
            string response = Query(CommandTypes.RequestPower);
            Disconnect();
            if (Double.TryParse(response, out double parsedResponse))
            {
                return parsedResponse;
            }
            else
            {
                return 9999; // error parsing serial response from Windfreak
            }
        }
    }
}
