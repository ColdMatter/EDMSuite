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
    public class LakeShore336TemperatureController : RS232Instrument
    {

        /// <summary>
        /// See page 124 of LakeShore Model 336 User Manual for information on these commands.
        /// </summary>
        public static class CommandTypes
        {
            public static String RequestTemperature { get { return "RDG? "; } } // channel(s) and temperature unit need defining
            public static String RelayControlParameterQuery { get { return "RELAY? "; } } // Relay number need defining. There are two relays on the back of the LakeShore 336 controller
            public static String RelayStatusQuery { get { return "RELAYST? "; } } // Relay number need defining. There are two relays on the back of the LakeShore 336 controller
            public static String RelayControlParameterCommand { get { return "RELAY "; } } // Relay number, mode, input alarm and alarm type need defining.
            public static String ControlSetpointQuery { get { return "SETP? "; } } // Output needs defining.
            public static String ControlSetpointCommand { get { return "SETP "; } } // Output and value need defining.
            public static String WarmupSupplyParameterCommand { get { return "WARMUP "; } } // Output and value need defining.
            public static String WarmupSupplyParameterQuery { get { return "WARMUP? "; } } // Output and value need defining.
            public static String HeaterRangeQuery { get { return "RANGE? "; } } // Output need defining.
            public static String HeaterRangeCommand { get { return "RANGE "; } } // Output and range need defining.
            public static String ControlLoopPIDValuesQuery { get { return "PID? "; } } // Output need defining.
            public static String ControlLoopPIDValuesCommand { get { return "PID "; } } // Output and PID Values need defining.
            public static String AutotuneCommand { get { return "ATUNE "; } } // Output and Mode need defining.
            public static String ControlTuningStatusQuery { get { return "TUNEST? "; } } // No other input
        }

        // Serial connection parameters for the LakeShore Model 336 Temperature Controller:
        protected new int BaudRate = 57600; // Device can accept higher data transfer rate than default for class of 9600
        protected new short DataBits = 7;
        protected new SerialParity ParitySetting = SerialParity.Odd;

        /// <summary>
        /// Constructor for this class
        /// </summary>
        /// <param name="visaAddress"></param>
        public LakeShore336TemperatureController(String visaAddress)
            : base(visaAddress)
        {
            base.BaudRate = 57600;
            base.DataBits = 7;
            base.ParitySetting = SerialParity.Odd;
        }

        private string SetLineFeed(string Command)
        {
            return String.Concat(Command, "\n"); // Concatenate the command and carriage return "\n".
        }

        #region LakeShore 336 Temperature Query

        /// <summary>
        /// Validate the temperature query parameters Channel and TUnit
        /// </summary>
        /// <param name="Channel"></param>
        /// <param name="TUnit"></param>
        /// <returns></returns>
        private bool ValidateTemperatureRequest(int Channel, string TUnit)
        {
            if ((Channel < 9) && (Channel > -1) && ((TUnit == "K") || (TUnit == "C"))) { return true; } // check that the temperature unit requested was in celsius or kelvin and that the integer channel number requested was 0, 1, 2, 3, or 4.
            else { return false; }
        }

        /// <summary>
        /// Add characters to the temperature request command. This will specify what temperature unit to use when sending a query to the LakeShore 336.
        /// </summary>
        /// <param name="TUnit"></param>
        /// <returns></returns>
        private string setTUnit(string TUnit)
        {
            if (TUnit == "K") { return String.Concat("K", CommandTypes.RequestTemperature); } // check if the temperature unit requested was kelvin. If yes, concatenate "K" with the temperature request command
            else { return String.Concat("C", CommandTypes.RequestTemperature); } // If no, concatenate "C" with the temperature request command to select Celsius units
        }

        public string GetChannelName(string channel)
        {
            string resp = "";
            if (!connected) Connect(SerialTerminationMethod.TerminationCharacter);
            if (!Environs.Debug)
            {
                serial.RawIO.Write("INNAME? " + channel + "\n");
                
                resp = System.Text.Encoding.UTF8.GetString(serial.RawIO.Read());
                
            }
            Disconnect();
            return resp.Trim();
        }

        ///<summary>
        /// Set the channel character in the temperature query command
        ///</summary>
        private string SetTChannel(int Channel, string TUnitCommand) 
        {
            string[] Channels = { "0", "A", "B", "C", "D", "D2", "D3", "D4", "D5" }; // 0 = all channels
            return String.Concat(TUnitCommand, Channels[Channel]); // Concatenate the channel and command. The command should already include the temperature unit.
        }

        private string BuildTempQuery(int Channel, string TUnit)
        {
            return SetLineFeed(SetTChannel(Channel, setTUnit(TUnit)));
        }

        public string GetTemperature(int Channel, string TUnit)
        {
            if (ValidateTemperatureRequest(Channel, TUnit)) // validate that the channel and temperature unit selected are valid options
            {
                Connect(SerialTerminationMethod.TerminationCharacter);
                string temperatureRequest = BuildTempQuery(Channel, TUnit); // build the temperature query command: add a character to it to select the desired temperature unit ("K" or "C" prefix) and then add the desired channel to the command ("0", "A", "B", "C" or "D" suffix). Finally, add the carriage return to the command.
                
                string response = Query(temperatureRequest);
                Disconnect();
                return response;
            }
            else { return "err"; }
        }

        

        
        #endregion

        #region LakeShore 336 Relay Control/Queries

        // Validate the parameters for the relay commands
        private bool ValidateRelayNumber(int RelayNumber)
        {
            if ((RelayNumber == 1) | (RelayNumber == 2)) return true; // There are only two relay numbers: 1 and 2
            else return false;
        }
        private bool ValidateRelayMode(int RelayMode)
        {
            if ((RelayMode == 0) | (RelayMode == 1) | (RelayMode == 2)) return true; // There are only three relay modes: 0 (Off), 1 (On) and 2 (Alarms)
            else return false;
        }
        private bool ValidateRelayInputAlarm(string RelayInputAlarm)
        {
            if ((RelayInputAlarm == "A") | (RelayInputAlarm == "B") | (RelayInputAlarm == "C") | (RelayInputAlarm == "D")) return true; // There are only four temperature channels: A, B, C and D
            else return false;
        }
        private bool ValidateRelayAlarmType(int AlarmType)
        {
            if ((AlarmType == 0) | (AlarmType == 1) | (AlarmType == 2)) return true; // There are only three relay alarm types: 0 (Low Alarm), 1 (High Alarm) and 2 (Both Alarms)
            else return false;
        }

        /// <summary>
        /// Write a command to the LakeShore 336 temperature controller to change the relay parameters. 
        /// This function will return a bool flag to indicate whether or not it has successfully written to the device (true = success). 
        /// However, it is advised that one queries the relay parameters after using this command to confirm that they have indeed changed.
        /// </summary>
        /// <param name="RelayNumber"></param>
        /// <param name="RelayMode"></param>
        /// <param name="RelayInputAlarm"></param>
        /// <param name="AlarmType"></param>
        /// <returns></returns>
        public bool SetRelayParameters(int RelayNumber, int RelayMode, string RelayInputAlarm, int AlarmType)
        {
            if (ValidateRelayNumber(RelayNumber) & ValidateRelayMode(RelayMode) & ValidateRelayInputAlarm(RelayInputAlarm) & ValidateRelayAlarmType(AlarmType)) // validate that the channel and temperature unit selected are valid options
            {
                if (!Environs.Debug)
                {
                    Connect(SerialTerminationMethod.TerminationCharacter);
                    string RelayCommand = SetLineFeed(String.Concat(CommandTypes.RelayControlParameterCommand, RelayNumber, ",", RelayMode, ",", RelayInputAlarm, ",", AlarmType));
                    Write(RelayCommand);
                    Disconnect();
                }
                return true; // success in writing the command, but should check that the relay parameters have been correctly changed
            }
            else return false; // The input parameters were not valid. 
        }
        
        /// <summary>
        /// Write a command to the LakeShore 336 temperature controller to change the relay parameters. 
        /// This function will return a bool flag to indicate whether or not it has successfully written to the device (true = success). 
        /// However, it is advised that one queries the relay parameters after using this command to confirm that they have indeed changed.
        /// </summary>
        /// <param name="RelayNumber"></param>
        /// <param name="RelayMode"></param>
        /// <param name="RelayInputAlarm"></param>
        /// <param name="AlarmType"></param>
        /// <returns></returns>
        public bool SetRelayParameters(int RelayNumber, int RelayMode)
        {
            if (ValidateRelayNumber(RelayNumber) & ValidateRelayMode(RelayMode)) // validate that the channel and temperature unit selected are valid options
            {
                if (!Environs.Debug)
                {
                    Connect(SerialTerminationMethod.TerminationCharacter);
                    string RelayCommand = SetLineFeed(String.Concat(CommandTypes.RelayControlParameterCommand, RelayNumber, ",", RelayMode));
                    Write(RelayCommand, true);
                    Thread.Sleep(2000);// Wait to prevent an attempt to interact with the LakeShore Controller. Without this, and if another command/query is immediately sent, the relay state won't change and the next 
                    Disconnect();
                }
                return true; // success in writing the command, but should check that the relay parameters have been correctly changed
            }
            else return false; // The input parameters were not valid. 
        }

        /// <summary>
        /// Queries the LakeShore 336 temperature controller regarding the relay control parameters.
        /// Will return the query response if successful.
        /// Will return "Exception: Invalid relay number" if the relay number argument is not 1 or 2.
        /// </summary>
        /// <param name="RelayNumber"></param>
        /// <returns></returns>
        public string QueryRelayControlParameters(int RelayNumber)
        {
            if(ValidateRelayNumber(RelayNumber))
            {
                string response = "";
                if (!Environs.Debug)
                {
                    Connect(SerialTerminationMethod.TerminationCharacter);
                    string query = SetLineFeed(String.Concat(CommandTypes.RelayControlParameterQuery, RelayNumber));
                    response = Query(query);
                    Disconnect();
                }
                return response;
            }
            else return "Exception: Invalid relay number";
        }

        /// <summary>
        /// Queries the LakeShore 336 temperature controller regarding the relay status.
        /// Will return the query response if successful.
        /// Will return "Exception: Invalid relay number" if the relay number argument is not 1 or 2.
        /// </summary>
        /// <param name="RelayNumber"></param>
        /// <returns></returns>
        public string QueryRelayStatus(int RelayNumber)
        {
            if (ValidateRelayNumber(RelayNumber))
            {
                Connect(SerialTerminationMethod.TerminationCharacter);
                string query = SetLineFeed(String.Concat(CommandTypes.RelayStatusQuery, RelayNumber));
                string response = Query(query);
                Thread.Sleep(500);
                Disconnect(); 
                string trimResponse = response.Trim();// Trim in case there are unexpected white spaces.
                string status = trimResponse.Substring(0, 1); // Take the first character of the string.
                return status;
            }
            else return "Exception: Invalid relay number";
        }

        #endregion

        #region Setpoint control

        /// <summary>
        /// Query the setpoint of a given output (1-4). Temperature in preferred units of the control loop sensor.
        /// </summary>
        /// <param name="Output"></param>
        /// <returns></returns>
        public string QueryControlSetpoint(int Output)
        {
            Connect(SerialTerminationMethod.TerminationCharacter);
            string query = SetLineFeed(String.Concat(CommandTypes.ControlSetpointQuery, Output));
            string response = Query(query);
            Thread.Sleep(500);
            Disconnect();
            return response;
        }

        /// <summary>
        /// Set the setpoint of a given output (1-4). Temperature in preferred units of the control loop sensor.
        /// </summary>
        /// <param name="Output"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public void SetControlSetpoint(int Output, double Value)
        {
            Connect(SerialTerminationMethod.TerminationCharacter);
            string cmd = SetLineFeed(String.Concat(CommandTypes.ControlSetpointCommand, Output, ",", Value));
            Write(cmd, true);
            Thread.Sleep(1000);
            Disconnect();
        }

        #endregion

        #region Warmup Supply Parameter

        // These command can be used the analogue outputs used for warming up the source. However, they do not control whether it is turned on or the temperature setpoint.

        /// <summary>
        /// Query the warmup supply parameter(s). Outputs a strin of format "control,percentage". The control parameter specifies the type of control used (0 = Auto Off, 1 = Continuous).
        /// </summary>
        /// <param name="Output"></param>
        /// <returns></returns>
        public string QueryWarmupSupplyParameter(int Output)
        {
            Connect(SerialTerminationMethod.TerminationCharacter);
            string query = SetLineFeed(String.Concat(CommandTypes.WarmupSupplyParameterQuery, Output));
            string response = Query(query);
            Thread.Sleep(500);
            Disconnect();
            return response;
        }

        public void SetWarmupSupplyParameter(int Output, int Control, double Percentage)
        {
            Connect(SerialTerminationMethod.TerminationCharacter);
            string cmd = SetLineFeed(String.Concat(CommandTypes.WarmupSupplyParameterCommand, Output, ",", Control,",",Percentage));
            Write(cmd, true);
            Thread.Sleep(1000);
            Disconnect();
        }

        #endregion

        #region Heater Range
        // The name of this is a little deceiving, however it is written like this to match the name in the LakeShore 336 manual.
        // These functions can be used to turn on and off the outputs (but not if an out is in Monitor Out mode - an output in Monitor Out mode is always on).

        /// <summary>
        /// Returns range parameter for a given output.
        /// For outputs 1 and 2: 0 = Off, 1 = Low, 2 = Medium and 3= High.
        /// For outputs 3 and 4: 0 = Off and 1 = On.
        /// </summary>
        /// <param name="Output"></param>
        /// <returns></returns>
        public string QueryHeaterRange(int Output)
        {
            Connect(SerialTerminationMethod.TerminationCharacter);
            string query = SetLineFeed(String.Concat(CommandTypes.HeaterRangeQuery, Output));
            string response = Query(query);
            Thread.Sleep(500);
            Disconnect();
            return response;
        }

        /// <summary>
        /// Sets the range parameter for a given output.
        /// For outputs 1 and 2: 0 = Off, 1 = Low, 2 = Medium and 3= High.
        /// For outputs 3 and 4: 0 = Off and 1 = On.
        /// Remark: The range setting has no effect if an output is in the Off mode, and does not apply to an output in Monitor Out mode. An output in Monitor Out mode is always on.
        /// </summary>
        /// <param name="Output"></param>
        /// <param name="Range"></param>
        public void SetHeaterRange(int Output, int Range)
        {
            Connect(SerialTerminationMethod.TerminationCharacter);
            string cmd = SetLineFeed(String.Concat(CommandTypes.HeaterRangeCommand, Output, ",", Range));
            Write(cmd, true);
            Thread.Sleep(1000);
            Disconnect();
        }

        #endregion

        #region PID Loops

        public string QueryPIDLoopValues(int Output)
        {
            Connect(SerialTerminationMethod.TerminationCharacter);
            string query = SetLineFeed(String.Concat(CommandTypes.ControlLoopPIDValuesQuery, Output));
            string response = Query(query);
            Thread.Sleep(500);
            Disconnect();
            return response;
        }

        public void SetPIDLoopValues(int Output, double proportional, double integral, double derivative)
        {
            Connect(SerialTerminationMethod.TerminationCharacter);
            string cmd = SetLineFeed(String.Concat(CommandTypes.ControlLoopPIDValuesCommand, Output, ",", proportional, ",", integral, ",", derivative));
            Write(cmd, true);
            Thread.Sleep(1000);
            Disconnect();
        }

        #endregion

        #region Autotune outputs

        /// <summary>
        /// Autotune a user defined output.
        /// Outputs 1-4 can be autotuned.
        /// Autotune modes: 0 = P; 1 = P and I; 2 = P, I and D.
        /// </summary>
        /// <param name="Output"></param>
        /// <param name="Mode"></param>
        public void AutotuneOutput(int Output, int Mode)
        {
            Connect(SerialTerminationMethod.TerminationCharacter);
            string cmd = SetLineFeed(String.Concat(CommandTypes.AutotuneCommand, Output, ",", Mode));
            Write(cmd, true);
            Thread.Sleep(1000);
            Disconnect();
        }

        /// <summary>
        /// If initial conditions required to autotune the specified loop are not met, an Autotune initialization error will occur and the Autotune process will not be performed.
        /// The TUNEST? query can be used to check if an autotune error occured.
        /// </summary>
        /// <returns></returns>
        public string QueryControlTuningStatus()
        {
            Connect(SerialTerminationMethod.TerminationCharacter);
            string query = SetLineFeed(String.Concat(CommandTypes.ControlTuningStatusQuery, ""));
            string response = Query(query);
            Thread.Sleep(500);
            Disconnect();
            return response;
        }
        

        #endregion
    }
}
