using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.Visa;
using Ivi.Visa;
using DAQ.Environment;
using System.Threading;
using System.Windows.Forms;

namespace DAQ.HAL
{
    public class AD9850DDS : RS232Instrument
    {
        public static class CommandTypes
        {
            public static String QueryFrequency { get { return "frequency query "; } } 
            public static String SetFrequency { get { return "frequency set "; } }
        }

        // Serial connection parameters for the Arduino microcontroller:
        protected new int BaudRate = 115200; // Device can accept higher data transfer rate than default for class of 9600
        protected new short DataBits = 8;
        protected new SerialParity ParitySetting = SerialParity.None;

        /// <summary>
        /// Constructor for this class
        /// </summary>
        /// <param name="visaAddress"></param>
        public AD9850DDS(String visaAddress)
            : base(visaAddress)
        {
            base.BaudRate = 115200;
            base.DataBits = 8;
            base.ParitySetting = SerialParity.None;
        }

        /// <summary>
        /// Set up serial connection to the Arduino. A wait of approx. 2000ms is required after the connection 
        /// is made because the Arduino needs time to reload its program. Yes, this is really, really 
        /// annoying. What's more annoying is how long it took to find the solution to the IOTimeoutException 
        /// exceptions I was getting. I found the solution here (accessed 30th Nov 2021): 
        /// https://forum.arduino.cc/t/vb-net-ni-visa-rawio-write-not-working/919790
        /// </summary>
        private void ConnectToArduino()
        {
            try
            {
                Connect(SerialTerminationMethod.TerminationCharacter, SerialTerminationMethod.None);
            }
            catch (Exception NativeVisaException)
            {
                MessageBox.Show(NativeVisaException.Message);
            }
            Thread.Sleep(2000);
        }
        
        // Frequency
        public string QueryFrequency()
        {
            ConnectToArduino();
            string Response;
            try
            {
                Response = Query(String.Concat(CommandTypes.QueryFrequency, "\n"));
            }
            catch (Exception IOTimeoutException)
            {
                MessageBox.Show(IOTimeoutException.Message);
                Response = "IOTimeoutException";
            }

            Disconnect();
            return Response;
        }

        //public void SetFrequency(long frequency) // frequency in Hz
        //{
        //    Connect(SerialTerminationMethod.TerminationCharacter);
        //    double MHzFrequency = frequency / Math.Pow(10, 6);
        //    string cmd = CommandTypes.SetFrequency + MHzFrequency.ToString();
        //    Write(cmd, true);
        //    Disconnect();
        //}

    }
}
