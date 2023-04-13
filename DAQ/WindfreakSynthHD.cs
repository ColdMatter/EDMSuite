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
            public static String QueryRFMute { get { return "h?"; } } //  returns: 1=not muted or 0=muted
            public static String SetRFMute { get { return "h"; } } //  1=not muted or 0=muted PA Power On
            public static String QueryPAPowerOn { get { return "r?"; } } //  returns: 1=powered on or 0=powered off
            public static String SetPAPowerOn { get { return "r"; } } //  1=powered on or 0=powered off
            public static String QueryPLLPowerOn { get { return "E?"; } } //  returns: 1=powered on or 0=powered off
            public static String SetPLLPowerOn { get { return "E"; } } //  1=powered on or 0=powered off
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
        
        // Frequency
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

        // Power
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
        public void SetPower(double power) // power in dBm
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

        // Channel
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

        // Temperature
        public double QueryTemperature()
        {
            Connect(SerialTerminationMethod.TerminationCharacter);
            string response = Query(CommandTypes.QueryTemperature);
            Disconnect();
            if (Double.TryParse(response, out double parsedResponse))
            {
                return parsedResponse;
            }
            else
            {
                return 9996; // error parsing serial response from Windfreak
            }
        }

        // RF Mute
        /// <summary>
        /// "The SynthHD output power can be muted without fully powering down the PLL and output amplifier stages.  
        /// The amount of muting depends on frequency. hx sets the muting function where x=1=not muted and x=0=muted"
        /// </summary>
        /// <returns></returns>
        public bool QueryRFMute()
        {
            Connect(SerialTerminationMethod.TerminationCharacter);
            string response = Query(CommandTypes.QueryRFMute);
            Disconnect();
            if (Int32.TryParse(response, out int parsedResponse))
            {
                if (parsedResponse == 1)
                {
                    return false; // RF not muted
                }
                else
                {
                    return true; // RF muted
                }
            }
            else
            {
                return false; // error parsing serial response from Windfreak
            }
        }
        /// <summary>
        /// "The SynthHD output power can be muted without fully powering down the PLL and output amplifier stages.  
        /// The amount of muting depends on frequency. hx sets the muting function where x=1=not muted and x=0=muted"
        /// </summary>
        /// <param name="Flag"></param>
        public void SetRFMute(bool Flag) 
        {
            string cmdstr;
            if (!Flag)
            {
                cmdstr = "1";
            }
            else
            {
                cmdstr = "0"; 
            }
            Connect(SerialTerminationMethod.TerminationCharacter);
            string cmd = CommandTypes.SetRFMute + cmdstr;
            Write(cmd, true);
            Disconnect();
        }

        //PA Power
        /// <summary>
        /// "The SynthHD output power stage can be powered down without fully powering down the PLL and output amplifier stages.  
        /// This command enables and disables the linear regulator that supplies the VGA output power stage to save energy.  
        /// The amount of muting depends on frequency.  The SynthHD software GUI uses this command and the “E” command to toggle 
        /// the output RF on and off.  
        /// (E0r0=full quiet, E1r1=fully operational) rx sets the enable function where x=1=powered on and x=0=powered off"
        /// Flag = true = 1
        /// Flag = false = 0
        /// </summary>
        /// <returns></returns>
        public bool QueryPAPowerOn()
        {
            Connect(SerialTerminationMethod.TerminationCharacter);
            string response = Query(CommandTypes.QueryPAPowerOn);
            Disconnect();
            if (Int32.TryParse(response, out int parsedResponse))
            {
                if (parsedResponse == 1)
                {
                    return true; // powered on
                }
                else
                {
                    return false; // powered off
                }
            }
            else
            {
                return false; // error parsing serial response from Windfreak
            }
        }
        /// <summary>
        /// "The SynthHD output power stage can be powered down without fully powering down the PLL and output amplifier stages.  
        /// This command enables and disables the linear regulator that supplies the VGA output power stage to save energy.  
        /// The amount of muting depends on frequency.  The SynthHD software GUI uses this command and the “E” command to toggle 
        /// the output RF on and off.  
        /// (E0r0=full quiet, E1r1=fully operational) rx sets the enable function where x=1=powered on and x=0=powered off"
        /// Flag = true = 1
        /// Flag = false = 0
        /// </summary>
        /// <param name="Flag"></param>
        public void SetPAPowerOn(bool Flag)
        {
            string cmdstr;
            if (Flag)
            {
                cmdstr = "1";
            }
            else
            {
                cmdstr = "0";
            }
            Connect(SerialTerminationMethod.TerminationCharacter);
            string cmd = CommandTypes.SetPAPowerOn + cmdstr;
            Write(cmd, true);
            Disconnect();
        }

        //PLL Power
        /// <summary>
        /// "The SynthHD PLL can be powered down for absolute minimum noise on the output connector.  
        /// This command enables and disables the PLL and VCO to save energyand can take 20mS to boot up.  
        /// The SynthHD software GUI uses the “r” command and the “E” command to toggle the output RF on and off.  
        /// (E0r0=full quiet, E1r1=fully operational)Ex sets the enable function where x=1=powered on and x=0=powered off"
        /// Flag = true = 1
        /// Flag = false = 0
        /// </summary>
        /// <returns></returns>
        public bool QueryPLLPowerOn()
        {
            Connect(SerialTerminationMethod.TerminationCharacter);
            string response = Query(CommandTypes.QueryPLLPowerOn);
            Disconnect();
            if (Int32.TryParse(response, out int parsedResponse))
            {
                if (parsedResponse == 1)
                {
                    return true; // powered on
                }
                else
                {
                    return false; // powered off
                }
            }
            else
            {
                return false; // error parsing serial response from Windfreak
            }
        }
        /// <summary>
        /// "The SynthHD PLL can be powered down for absolute minimum noise on the output connector.  
        /// This command enables and disables the PLL and VCO to save energyand can take 20mS to boot up.  
        /// The SynthHD software GUI uses the “r” command and the “E” command to toggle the output RF on and off.  
        /// (E0r0=full quiet, E1r1=fully operational)Ex sets the enable function where x=1=powered on and x=0=powered off"
        /// Flag = true = 1
        /// Flag = false = 0
        /// </summary>
        /// <param name="Flag"></param>
        public void SetPLLPowerOn(bool Flag)
        {
            string cmdstr;
            if (Flag)
            {
                cmdstr = "1";
            }
            else
            {
                cmdstr = "0";
            }
            Connect(SerialTerminationMethod.TerminationCharacter);
            string cmd = CommandTypes.SetPLLPowerOn + cmdstr;
            Write(cmd, true);
            Disconnect();
        }

    }
}
