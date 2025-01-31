using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.Visa;
using Ivi.Visa;
using DAQ.Environment;

namespace DAQ.HAL
{
    public class StepperMotorController : DAQ.HAL.SerialInstrument
    {

        /// This is a summary of all the commands that can be sent to the homemade stepper motor controller.
        /// This document defines all the possible commands for the controller.
        /// </summary>
        public static class CommandTypes
        {
            //public static String ChangeFlowSetpoint { get { return "s"; } } // Changes the flow rate going through the flow controller. Really this command is asXXXX where XXXX is a number but we will compose the full command later
            //public static String CollectFlowData { get { return "a"; } } // Command needed to return large string of all the data
            //public static String StopStreaming { get { return "@@=a"; } } // Command to stop streaming data
        }

        // Serial connection parameters for the Stepper Motor Controller:
        protected new int BaudRate = 4800; // Device can accept higher data transfer rate than default for class of 9600
        protected new short DataBits = 8;
        protected new SerialStopBitsMode StopBit = SerialStopBitsMode.One;
        protected new SerialParity ParitySetting = SerialParity.None;
        protected new byte TerminationCharacter = 0x0D;
        protected int TimeoutMilliseconds = 2000;


        public StepperMotorController(String address)
            :base(address)
        {
            base.BaudRate = 4800;
            base.DataBits = 8;
            base.ParitySetting = SerialParity.None;
            base.StopBit = SerialStopBitsMode.One;
            base.TerminationCharacter = 0x0D;
            base.TimeoutMilliseconds = 2000;
        }
        protected void Clear()
        {
            serial.Clear();
        }

        private bool Precommand()
        {
            serial.RawIO.Write("?");
            if (serial.RawIO.ReadString(1) == "!") return true;
            else return false;
        }
        private void ResetToDefault()
        {
            string command = String.Concat("default", "\n\r");
            Clear();
            if (Precommand()) serial.RawIO.Write(command);
            string response = serial.RawIO.ReadString(5);
            Console.WriteLine(String.Concat("set to default with response: ", response));
        }
        private void Feedback(bool active)
        {
            string feedbackValue = active ? "1" : "0";
            string command = String.Concat("feedback ",feedbackValue,"\n\r");
            Clear();
            if (Precommand()) serial.RawIO.Write(command);
            string response = serial.RawIO.ReadString(5);
            Console.WriteLine(String.Concat("feedback change to ", feedbackValue, " with response: ", response));
        }
        private void Mode(string mode)
        {
            string command = String.Concat("mode ", mode, "\n\r");
            Clear();
            if (Precommand()) serial.RawIO.Write(command);
            string response = serial.RawIO.ReadString(5);
            Console.WriteLine(String.Concat("mode change to ", mode, " with response: ", response));
        }

        private void Delay(string delay)
        {
            string command = String.Concat("delay ", delay, "\n\r");
            Clear();
            if (Precommand()) serial.RawIO.Write(command);
            string response = serial.RawIO.ReadString(5);
            Console.WriteLine(String.Concat("mode change to ", delay, " with response: ", response));
        }

        private void Triggers(string triggers)
        {
            string command = String.Concat("triggers ", triggers, "\n\r");
            Clear();
            if (Precommand()) serial.RawIO.Write(command);
            string response = serial.RawIO.ReadString(5);
            Console.WriteLine(String.Concat("mode change to ", triggers, " with response: ", response));
        }

        private void Steps(string steps)
        {
            string command = String.Concat("steps ", steps, "\n\r");
            Clear();
            if (Precommand()) serial.RawIO.Write(command);
            string response = serial.RawIO.ReadString(5);
            Console.WriteLine(String.Concat("mode change to ", steps, " with response: ", response));
        }

        public void FeedbackOff()
        {
            if (!connected) Connect();
            Feedback(false);
            Disconnect();
        }
        public void FeedbackOn()
        {
            if (!connected) Connect();
            Feedback(true);
            Disconnect();
        }

        public void DisableMotor()
        {
            if (!connected) Connect();
            Mode("0");
            Disconnect();
        }

        public void ManualMode()
        {
            if (!connected) Connect();
            Feedback(false);
            Mode("0");
            Delay("30");
            Mode("1");
            Disconnect();
        }

        public void ExternalMode()
        {
            if (!connected) Connect();
            Feedback(false);
            Mode("0");
            Mode("2");
            Disconnect();
        }

        public void TriggerMoveMode(string delay, string triggers, string steps)
        {
            if (!connected) Connect();
            Feedback(false);
            Mode("0");
            Delay(delay);
            Triggers(triggers);
            Steps(steps);
            Mode("5");
            Disconnect();
        }

        public void ResetPosition()
        {
            if (!connected) Connect();
            ResetToDefault();
            string command = String.Concat("home", "\n\r");
            Clear();
            if (Precommand()) serial.RawIO.Write(command);
            string response = serial.RawIO.ReadString(5);
            Console.WriteLine(String.Concat("reset home position with response: ", response));
            Disconnect();
        }
    }
}
