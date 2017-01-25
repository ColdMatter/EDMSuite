using System;
using System.Windows.Forms;
using NationalInstruments.VisaNS;

using DAQ.Environment;


namespace DAQ.HAL
{
	/// <summary>
	/// This class talks to the Bath Cryostat Temperature Controller ITC503
	/// over RS232.
	/// </summary>
	public class BathCryostat
	{
		SerialSession serial;
		String address;
		private bool connected = false;

		public BathCryostat(String address)
		{
			this.address = address;
		}
        
		private void Connect()
		{
			if (!Environs.Debug) 
			{
				serial = new SerialSession(address);
				serial.BaudRate = 9600;
				serial.DataBits = 8;
				serial.StopBits = StopBitType.Two;
                serial.TerminationCharacter = System.Text.Encoding.ASCII.GetBytes("\r")[0];
				serial.ReadTermination = SerialTerminationMethod.TerminationCharacter;
                serial.WriteTermination = SerialTerminationMethod.TerminationCharacter;
                serial.Flush(BufferTypes.InBuffer, true);
                serial.Flush(BufferTypes.OutBuffer, true);
			}
			connected = true;
		}

		private void Disconnect()
		{
			if (!Environs.Debug) serial.Dispose();
			connected = false;
		}

        //This reads the sensors given by the array sensor 1 is Temp sensor 1, 2 is Temp sensor 2, etc. 
        //see ITC 503 manual for full list.
        public double[] ReadCryostatSensor(int[] sensor)
		{
            string[] readData = new string[sensor.Length];
            double[] dataValue = new double[sensor.Length]; 
            if (!connected) Connect();
            if (!Environs.Debug)
            {
                try
                {
                    for (int i = 0; i < sensor.Length; i++)
                    {
                        string sensorstring = string.Join("", "R", sensor[i].ToString(), "\r");
                        byte[] writBytes = System.Text.Encoding.ASCII.GetBytes(sensorstring);
                        readData[i] = serial.Query(sensorstring, 8);
                        if (readData[i] != null) dataValue[i] = Convert.ToDouble(readData[i].Substring(1, readData[i].Length - 3));
                        serial.Flush(BufferTypes.InBuffer, true);
                        serial.Flush(BufferTypes.OutBuffer, true);
                    }
                }
                catch (Exception e)
                {
                    // bug swatter for intermittance bug in reading from cryostat
                    Console.Error.Write(e.Message + e.StackTrace);
                    MessageBox.Show("Bath Cryo Bug:" + e.Message + "\n" + e.StackTrace, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
			Disconnect();
            return dataValue;
		}

        //This writes a Temperature set point for temperature scans
        public void SetTemperature(double setpoint)
        {
            if (!connected) Connect();
            if (!Environs.Debug)
            {
                string tempstring = string.Join("", "T", setpoint.ToString(), "\r"); 
                serial.Write(tempstring);
            }
            Disconnect();
        }
        
		public void PatternChangeStartingHandler(object source, EventArgs e)
		{}

		public void PatternChangeEndedHandler(object source, EventArgs e)
		{}

	}
}
