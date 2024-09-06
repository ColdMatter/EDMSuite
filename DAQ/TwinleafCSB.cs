using System;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.Visa;
using Ivi.Visa;
using DAQ.Environment;
using System.Threading;

namespace DAQ.HAL
{
	public class TwinleafCSB : RS232Instrument
    {
        public static class CommandTypes
        {
            public static String QueryCurrent { get { return ""; } }
            public static String SetCurrent { get { return "coil.z.current "; } }
        }

        // Serial connection parameters for the CSB:
        protected new int BaudRate = 115200;
        protected new short DataBits = 8;
        protected new SerialParity ParitySetting = SerialParity.None;
        protected new byte TerminationCharacter = 0xD;

        /// <summary>
        /// Constructor for this class
        /// </summary>
        /// <param name="visaAddress"></param>
        /// 
		public TwinleafCSB(string visaAddress)
            : base(visaAddress)
        {
            base.BaudRate = 115200;
            base.DataBits = 8;
            base.ParitySetting = SerialParity.None;
            base.TerminationCharacter = TerminationCharacter;
        }

        private double upperCurrent = 0.999;
        private double lowerCurrent = -0.999;

        private double TrimCurrent(double current)
        {
            double outputCurrent = current;
            if (outputCurrent > upperCurrent)
            {
                outputCurrent = upperCurrent;
            }
            if (outputCurrent < lowerCurrent)
            {
                outputCurrent = lowerCurrent;
            }
            return outputCurrent;
        }

        public override void Connect()
        {
            Connect(SerialTerminationMethod.TerminationCharacter, SerialTerminationMethod.TerminationCharacter);
        }

        public string SetCurrent(double inputcurrent) // current in mA
        {
            double current = TrimCurrent(inputcurrent);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            // the code that you want to measure comes here
            string response = "No response";
            Connect(SerialTerminationMethod.TerminationCharacter, SerialTerminationMethod.TerminationCharacter);
            string cmd = CommandTypes.SetCurrent + current.ToString("F6");
            //Write(cmd, true);
            response = Query(cmd);
            Console.WriteLine(response);
            Disconnect();

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            //Console.WriteLine(elapsedMs+" ms elapsed");

            return (response+" takes "+elapsedMs+" ms");
        }

        public void QuerySession(string cmd)
        {
            string sessionID;
            Connect(SerialTerminationMethod.TerminationCharacter, SerialTerminationMethod.TerminationCharacter);
            //Clear();
            Write(cmd, true);
            Thread.Sleep(50);
            //string cmd = "dev.session";
            sessionID = Read();
            Console.WriteLine(sessionID);
            Disconnect();
        }

        public string ReadOutput()
        {
            string output = "";
            Connect(SerialTerminationMethod.TerminationCharacter, SerialTerminationMethod.TerminationCharacter);
            output = Read();
            Disconnect();
            return output;
        }
    }
}
