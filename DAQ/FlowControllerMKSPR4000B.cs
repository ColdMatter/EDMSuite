using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.VisaNS;
using DAQ.Environment;
using System.Globalization;
using System.Threading;

namespace DAQ.HAL
{
    public class FlowControllerMKSPR4000B : RS232Instrument
    {
        public static class CommandTypes
        {
            public static String RequestKey { get { return "KY"; } }
            public static String RemoteMode { get { return "RT"; } }
            public static String AccessChannel { get { return "AC"; } }
            public static String ActualValue { get { return "AV"; } }
            public static String Setpoint { get { return "SP"; } }
        }

        #region Serial Communication Parameters

        protected new int BaudRate = 19200;
        protected new Parity ParitySetting = Parity.Odd;
        protected new short DataBits = 7;
        private SerialTerminationMethod TerminationMethod = SerialTerminationMethod.TerminationCharacter;
        protected new byte TerminationCharacter = 0xd;

        #endregion

        public FlowControllerMKSPR4000B(String address)
            : base(address)
        {
            base.BaudRate = BaudRate;
            base.ParitySetting = ParitySetting;
            base.DataBits = DataBits;
            base.TerminationCharacter = TerminationCharacter;
        }

        #region Functions

        /// <summary>
        /// Appends the carriage return character to the input string
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private string SetCarriageReturn(string cmd)
        {
            string CarriageReturn = "\r";
            string response = String.Concat(cmd, CarriageReturn);
            return response;
        }

        private void ConnectToFlowController()
        {
            Connect(TerminationMethod);
        }

        /// <summary>
        /// Query a flow controller parameter. Will return a string (including carriage return \r).
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private string QueryParameter(string cmd, string parameter)
        {
            ConnectToFlowController();
            string query = SetCarriageReturn(String.Concat("?", cmd, parameter));
            Console.Write(query);
            string response = Query(query);
            Disconnect();
            return response;
        }

        /// <summary>
        /// Set a flow controller parameter.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameter"></param>
        private void SetParameter(string cmd, string parameter)
        {
            ConnectToFlowController();
            string command = SetCarriageReturn(String.Concat(cmd,",", parameter));
            Write(command);
            Thread.Sleep(100);
            Disconnect();
        }

        /// <summary>
        /// Set a flow controller channel parameter.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ChannelNumber"></param>
        /// <param name="parameter"></param>
        private void SetChannelParameter(string cmd, string ChannelNumber, string parameter)
        {
            ConnectToFlowController();
            string command = SetCarriageReturn(String.Concat(cmd, ChannelNumber, ",", parameter));
            Write(command);
            Thread.Sleep(100);
            Disconnect();
        }

        /// <summary>
        /// Set and query a flow controller parameter. Will return a string (including carriage return \r).
        /// For example, the Access Channel command can both set the channel setpoint and return the actual value.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private string SetAndQueryParameter(string cmd, string parameter)
        {
            ConnectToFlowController();
            string query = SetCarriageReturn(String.Concat(cmd, parameter));
            string response = Query(query);
            Disconnect();
            return response;
        }

        #endregion

        #region Commands

        /// <summary>
        /// "return key code and number of keys, which were not polled since last command."
        /// OFF = 00007,
        /// ON = 00008,
        /// ESC = 00009,
        /// ENTER = 00010,
        /// RIGHT = 00011,
        /// LEFT = 00012,
        /// UP = 00013,
        /// DOWN = 00014,
        /// No Key = 00255
        ///
        /// Number: 0 ... n
        /// </summary>
        /// <returns></returns>
        public string RequestKey()
        {
            string KeyCodeAndKeyNumber = QueryParameter(CommandTypes.RequestKey, "");
            return KeyCodeAndKeyNumber;
        }

        /// <summary>
        /// Ask if remote mode is on/off.
        /// Will return string "ON" or "OFF".
        /// </summary>
        /// <returns></returns>
        public string QueryRemoteMode()
        {
            string response = QueryParameter(CommandTypes.RemoteMode, "");
            return response;
        }

        /// <summary>
        /// Switch the PR4000 device remote mode on/off.
        /// </summary>
        /// <param name="OnOfState"></param>
        public void SetRemoteMode(bool OnOffState)
        {
            if (OnOffState)
            {
                SetParameter(CommandTypes.RemoteMode, "ON");
            }
            else
            {
                SetParameter(CommandTypes.RemoteMode, "OFF");
            }
        }

        /// <summary>
        /// set setpoint of channel and retrieve actual value 
        /// </summary>
        /// <param name="ChannelNumber"></param>
        /// <param name="Setpoint"></param>
        /// <returns></returns>
        public string SetAndQueryChannel(string ChannelNumber, string Setpoint)
        {
            string ActualSetpoint = SetAndQueryParameter(CommandTypes.AccessChannel, String.Concat(ChannelNumber, ",", Setpoint));
            return ActualSetpoint;
        }

        /// <summary>
        /// Retrieve channel setpoint and on/off state.
        /// Returns string array "setpoint","ON/OFF"
        /// </summary>
        /// <param name="ChannelNumber"></param>
        /// <returns></returns>
        public string[] QueryChannelSetPointAndOnOffState(string ChannelNumber)
        {
            string response = QueryParameter(CommandTypes.AccessChannel,ChannelNumber);
            string[] SetPointAndOnOffState = response.Split(',');
            return SetPointAndOnOffState;
        }

        /// <summary>
        /// Query the actual flow rate of a particular channel
        /// </summary>
        /// <param name="ChannelNumber"></param>
        /// <returns></returns>
        public double QueryActualValue(string ChannelNumber)
        {
            string response = QueryParameter(CommandTypes.ActualValue, ChannelNumber);
            response = response.Trim();
            response = response.Replace("\\r", "");
            double output = Double.Parse(response);
            return output;
        }

        /// <summary>
        /// Query the setpoint of a given flow controller channel
        /// </summary>
        /// <param name="ChannelNumber"></param>
        /// <returns></returns>
        public double QuerySetpoint(string ChannelNumber)
        {
            string response = QueryParameter(CommandTypes.Setpoint, ChannelNumber);
            response = response.Trim();
            response = response.Replace("\\r", "");
            double output = Double.Parse(response);
            return output;
        }

        public void SetSetpoint(string ChannelNumber, string Setpoint)
        {
            SetChannelParameter(CommandTypes.Setpoint, ChannelNumber, Setpoint);
        }
        

        #endregion

    }
}