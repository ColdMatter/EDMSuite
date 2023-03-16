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
    public class BigSkyYAG : RS232Instrument
    {

        /// <summary>
        /// This is a summary of all the commands that can be sent to the Alicat flow controller.
        /// This document defines all the possible commands for the controller.
        /// </summary>
        public static class CommandTypes
        {
            public static String Standby { get { return ">s"; } } // Changes the flow rate going through the flow controller. Really this command is asXXXX where XXXX is a number but we will compose the full command later
            public static String CheckTemp { get { return ">cg"; } } // Command needed to return large string of all the data 
        }

        // Serial connection parameters for the Lattice Ablation Laser:
        protected new int BaudRate = 9600; // Device can accept higher data transfer rate than default for class of 9600
        protected new short DataBits = 8;
        protected new SerialStopBitsMode StopBit = SerialStopBitsMode.One;
        protected new SerialParity ParitySetting = SerialParity.None;

        /// <summary>
        /// Constructor for this class
        /// </summary>
        /// <param name="visaAddress"></param>
        public BigSkyYAG(String visaAddress)
            : base(visaAddress)
        {
            base.BaudRate = 9600;
            base.DataBits = 8;
            base.ParitySetting = SerialParity.None;
            base.StopBit = SerialStopBitsMode.One;
    }

        private string SetLineFeed(string Command)
        {
            return String.Concat(Command, "\n"); // Concatenate the command and carriage return "\n".
        }


        #region YAG commands

        /// <summary>
        /// This is a command to check the temperature of the YAG laser
        /// It will return a string of the temperature 
        /// </summary>
        /// <param name="ControllerAddress"></param>
        /// <param name="FlowRate"></param>
        /// <returns></returns>

        public string CheckTemperature()
        {
            Connect(SerialTerminationMethod.TerminationCharacter);
            //string TempRequest = String.Concat(CommandTypes.CheckTemp, "\");
            //string response = Query(">cg\n");
            Disconnect();
            return "";
        }
        #endregion

    }
}
