using System;
using DAQ.Environment;

namespace DAQ.HAL
{
    /// <summary>
    /// This class represents a GPIB controlled Anrituso 37247C microwave VNA.
    /// </summary>   
    public class AnritsuVNA : GPIBInstrument
    {

        string averaging; /// Internal variable used for setting averaging settings
        // private string bgSub; /// Internal variable used for setting background subtractino settings settings
        string finalString;

        public AnritsuVNA(String visaAddress)
            : base(visaAddress) /// Connect the VNA
        { }

        public void Test()
        {
            Write("CNTR 13.9706E10 GHZ;SPAN 20 MHZ;TRS");
            Write("RTL");
        }
        public void VNALocalControl()
        {
            Write("RTL"); /// Return the VNA to local control
        }
        public void SetCent(double centre)
        {
            Write("CNTR " + centre.ToString() + " GHZ"); /// Set the centre frequency of the VNA scan in GHz
        }
        public void SetSpan(double span)
        {
            Write("SPAN " + span.ToString() + " MHZ"); /// Set span of the vna scan in MHz. Note that centre +/- span must be in the range 40 MHz to 30 GHz
        }
        public string SWRTrace(int av, int avFactor, int numPoints) /// Take a trace of the Standing Wave Ratio
        {
            if (av == 1)
            {
                averaging = "AON;AVG " + avFactor.ToString(); /// Turn on averaging and set the averaging factor
            }
            else
            {
                averaging = "AOF"; /// Turn off averaging
                avFactor = 1;
            }
            if (!Environs.Debug)
            {
                Write(averaging + ";NP" + numPoints.ToString() + ";");
                Write("MOF;CH1;HLD;SWR;TRS;WFS;ASC;*OPC?"); // Prepare the channel for magnitude data
                string opcMag = Read(); // Wait for operations complete
                Write("FDH2;OFV"); // Prepare to read frequency data
                string freq = Read(numPoints*19); // Read frequency data
                DateTime startTime = DateTime.UtcNow; /// Set a start time for read timeout
                while (true)
                {
                    if (freq.Length == numPoints * 19) /// Break once all points have been read
                    {
                        break;
                    }
                    if (DateTime.UtcNow - startTime > TimeSpan.FromSeconds(10)) /// Or break once timeout is reached
                    {
                        break;
                    }
                }
                Write("FDH2;OFD"); // Prepare to read magnitude trace data
                string data = Read(19 * numPoints); // Read magnitude trace data
                startTime = DateTime.UtcNow;
                while (true)
                {
                    if (data.Length == numPoints * 19)
                    {
                        break;
                    }
                    if (DateTime.UtcNow - startTime > TimeSpan.FromSeconds(10))
                    {
                        break;
                    }
                }
                finalString = freq + ";" + data;
                Write("RTL");
                return finalString; /// Output the list of frequencies and both magnitude and phase data (records both because of the "DPRS1" command)
            }
            else
            {
                return null;
            }
        }
        public string MagTrace(int av, int avFactor, int numPoints)
        {
            if (av == 1)
            {
                averaging = "AON;AVG " + avFactor.ToString(); /// Turn on averaging and set the averaging factor
            }
            else
            {
                averaging = "AOF"; /// Turn off averaging
                avFactor = 1;
            }
            if (!Environs.Debug)
            {
                Write(averaging + ";NP" + numPoints.ToString() + ";");
                Write("MOF;CH1;HLD;MAG;TRS;WFS;ASC;*OPC?"); // Prepare the channel for magnitude data
                string opcMag = Read(); // Wait for operations complete
                Write("FDH2;OFV"); // Prepare to read frequency data
                string freq = Read(19*numPoints); // Read frequency data
                DateTime startTime = DateTime.UtcNow;
                while (true)
                {                    
                    if (freq.Length == numPoints * 19)
                    {
                        break;
                    }
                    if (DateTime.UtcNow - startTime > TimeSpan.FromSeconds(30))
                    {
                        break;
                    }
                }
                Write("FDH2;OFD"); // Prepare to read magnitude trace data
                string data = Read(19*numPoints); // Read magnitude trace data
                startTime = DateTime.UtcNow;
                while (true)
                {
                    if (data.Length == numPoints * 19)
                    {
                        break;
                    }
                    if (DateTime.UtcNow - startTime > TimeSpan.FromSeconds(30))
                    {
                        break;
                    }
                }
                finalString = freq + ";" + data;
                Write("RTL");
                return finalString; /// Output the list of frequencies and both magnitude and phase data (records both because of the "DPRS1" command)
            }
            else
            {
                return null;
            }
        }
        public string PhaseTrace(int av, int avFactor, int numPoints)
        {
            if (av == 1)
            {
                averaging = "AON;AVG " + avFactor.ToString(); /// Turn on averaging and set the averaging factor
            }
            else
            {
                averaging = "AOF"; /// Turn off averaging
                avFactor = 1;
            }
            if (!Environs.Debug)
            {
                Write(averaging + ";NP" + numPoints.ToString() + ";");
                Write("MOF;CH2;HLD;PHA;TRS;WFS;*OPC?"); /// Prepare the channel for phase data
                string opcPha = Read();
                Write("FDH2;OFV"); // Prepare to read frequency data
                string freq = Read(19*numPoints); // Read frequency data
                DateTime startTime = DateTime.UtcNow;
                while (true)
                {
                    if (freq.Length == numPoints * 19)
                    {
                        break;
                    }
                    if (DateTime.UtcNow - startTime > TimeSpan.FromSeconds(30))
                    {
                        break;
                    }

                }
                Write("FDH2;OFD"); // Prepare to read phase data
                string data = Read(19*numPoints); // Read phase data
                startTime = DateTime.UtcNow;
                while (true)
                {
                    if (data.Length == numPoints * 19)
                    {
                        break;
                    }
                    if (DateTime.UtcNow - startTime > TimeSpan.FromSeconds(30))
                    {
                        break;
                    }
                }
                finalString = freq + ";" + data;
                Write("RTL");
                return finalString; /// Output the list of frequencies and both magnitude and phase data (records both because of the "DPRS1" command)
            }
            else
            {
                return null;
            }
        }
    }
}