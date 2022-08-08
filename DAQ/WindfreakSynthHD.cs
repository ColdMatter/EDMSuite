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
            public static String QueryFrequency { get { return "f?"; } } 
            public static String SetFrequency { get { return "f"; } }
            public static String QueryPower { get { return "W?"; } }
            public static String SetPower { get { return "W"; } }
            public static String QueryChannel { get { return "C?"; } }
            public static String SetChannel { get { return "C"; } }
            public static String QueryTemperature { get { return "z"; } } // I have no idea why they didn't use a '?' here. 'z?' reports the temperature and a load of other information.
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
        public double QueryFrequency()
        {
            Connect(SerialTerminationMethod.TerminationCharacter);
            string response = Query(CommandTypes.QueryFrequency);
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
        public void SetFrequency(long frequency) // frequency in Hz
        {
            Connect(SerialTerminationMethod.TerminationCharacter);
            double MHzFrequency = frequency / Math.Pow(10, 6);
            string cmd = CommandTypes.SetFrequency + MHzFrequency.ToString();
            Write(cmd, true);
            Disconnect();
        }

        public double QueryPower()
        {
            Connect(SerialTerminationMethod.TerminationCharacter);
            string response = Query(CommandTypes.QueryPower);
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
        public void SetPower(double power) // frequency in Hz
        {
            string powerString = power.ToString();
            string cmdstr;
            if (powerString.Contains('.'))
            {
                cmdstr = powerString;
            }
            else
            {
                cmdstr = powerString + '.'; // If no decimal place is added then the Windfreak will not output the correct power.
            }
            Connect(SerialTerminationMethod.TerminationCharacter);
            string cmd = CommandTypes.SetPower + cmdstr;
            Write(cmd, true);
            Disconnect();
        }

        public int QueryChannel()
        {
            Connect(SerialTerminationMethod.TerminationCharacter);
            string response = Query(CommandTypes.QueryChannel);
            Disconnect();
            if (Int32.TryParse(response, out int parsedResponse))
            {
                return parsedResponse;
            }
            else
            {
                return 9998; // error parsing serial response from Windfreak
            }
        }

        public void SetChannel(int channel)
        {
            Connect(SerialTerminationMethod.TerminationCharacter);
            string cmd = CommandTypes.SetChannel + channel.ToString();
            Write(cmd, true);
            Disconnect();
        }

        public int QueryTemperature()
        {
            Connect(SerialTerminationMethod.TerminationCharacter);
            string response = Query(CommandTypes.QueryTemperature);
            Disconnect();
            if (Int32.TryParse(response, out int parsedResponse))
            {
                return parsedResponse;
            }
            else
            {
                return 9996; // error parsing serial response from Windfreak
            }
        }



    }
}
