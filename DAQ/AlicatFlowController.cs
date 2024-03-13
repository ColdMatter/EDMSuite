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
    public class AlicatFlowController : RS232Instrument
    {

        /// <summary>
        /// This is a summary of all the commands that can be sent to the Alicat flow controller.
        /// This document defines all the possible commands for the controller.
        /// </summary>
        public static class CommandTypes
        {
            public static String ChangeFlowSetpoint { get { return "s"; } } // Changes the flow rate going through the flow controller. Really this command is asXXXX where XXXX is a number but we will compose the full command later
            public static String CollectFlowData { get { return "a"; } } // Command needed to return large string of all the data
            public static String StopStreaming {  get { return "@@=a"; } } // Command to stop streaming data
        }

        // Serial connection parameters for the Alicat Flow Controller:
        protected new int BaudRate = 19200; // Device can accept higher data transfer rate than default for class of 9600
        protected new short DataBits = 8;
        protected new SerialStopBitsMode StopBit = SerialStopBitsMode.One;
        protected new SerialParity ParitySetting = SerialParity.None;
        protected new byte TerminationCharacter = 0xD;

        /// <summary>
        /// Constructor for this class
        /// </summary>
        /// <param name="visaAddress"></param>
        public AlicatFlowController(String visaAddress)
            : base(visaAddress)
        {
            base.BaudRate = 19200;
            base.DataBits = 8;
            base.ParitySetting = SerialParity.None;
            base.TerminationCharacter = 0xD;
            base.StopBit = SerialStopBitsMode.One;
        }

        private string SetLineFeed(string Command)
        {
            return String.Concat(Command, "\n"); // Concatenate the command and carriage return "\n".
        }


        #region Alicat Change Flow Setpoint Query

        /// <summary>
        /// This is a command to change the setpoint of the Helium flowcontroller
        /// It will return a string from the flow controller 
        /// </summary>
        /// <param name="ControllerAddress"></param>
        /// <param name="FlowRate"></param>
        /// <returns></returns>

        public override void Connect()
        {
            if (!Environs.Debug)
            {
                if (!Environs.Debug)
                {
                    serial = new SerialSession(address);
                    serial.BaudRate = 19200;
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



        public string QueryData(string ControllerAddress)
        {
            Connect();
            string DataQuery = String.Concat(ControllerAddress, "\r");
            string response = Query(DataQuery);
            Disconnect();
            return response;
        }

        public string QueryFlow(string ControllerAddress)
        {
            Connect();
            string DataQuery = String.Concat(ControllerAddress, "\r");
            string response = Query(DataQuery);
            string[] responses = response.Split();
            Disconnect();
            return responses[4];
        }

        public string QuerySetpoint(string ControllerAddress)
        {
            Connect();
            string DataQuery = String.Concat(ControllerAddress, "\r");
            string response = Query(DataQuery);
            string[] responses = response.Split();
            Disconnect();
            return responses[5];
        }

        public string SetSetpoint(string ControllerAddress, string flowrate)
        {
            Connect();
            string FlowQuery = String.Concat(ControllerAddress, "s", flowrate, "\r");
            string response = Query(FlowQuery);
            Disconnect();
            return response;
        }

        public void StopStreaming()
        {
            Connect();
            string StopCommand = String.Concat(CommandTypes.StopStreaming, "\r");
            string response = Query(StopCommand);
            Disconnect();
        }

        #endregion

    }
}
