using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using DAQ.Environment;



namespace NavigatorHardwareControl
{
    /// <summary>
    /// This class implements an asynchronous connection to each computer on board the muquans laser to control various parameters such as the offset locks, edfa power and monitoring the laser. It also launches the executable used for real-time control of the DDS
    /// </summary>
    public class MuquansCommunicator
    {
        //Define the IP address for each computer
        private static string slaveIPAddress = "192.168.1.118";
        private static string edfaIPAddress = "192.168.1.75";
        private static string ddsSlaveIPAddress = "192.168.1.125";
        private static string ddsAOMIPAddress = "192.168.1.126";

        public TelnetConnection slaveConn;
        public TelnetConnection edfaConn;
        public TelnetConnection ddsSlaveConn;
        public TelnetConnection ddsAOMConn;
        public Process slaveDDS = new Process();
        public Process aomDDS = new Process();


        public void Start()
        {
            if (!Environs.Debug)
            {
                //Use connections to the Muquans laser
                slaveConn = new TelnetConnection(slaveIPAddress, 23);
                edfaConn = new TelnetConnection(edfaIPAddress, 23);
                ddsSlaveConn = new TelnetConnection(ddsSlaveIPAddress, 23);
                ddsAOMConn = new TelnetConnection(ddsAOMIPAddress, 23);
            }
            else
            {
                try
                {
                    slaveConn = new TelnetConnection("127.0.0.1", 6000);
                    edfaConn = new TelnetConnection("127.0.0.1", 6001);
                    ddsSlaveConn = new TelnetConnection("127.0.0.1", 6002);
                    ddsAOMConn = new TelnetConnection("127.0.0.1", 6003);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Couldn't start muquans connections: " + e.Message);
                }
            }
        }
        public ProcessStartInfo ConfigureDDS(string id, int port)
        {
            ///<summary>
            ///Configures the starting parameters for a dds process
            ///id - The identifier for the DDS. This is either "slave" or "aom"
            ///port - The port number used to communicate to the DDS. The default values are 18 and 20
            /// </summary>

            ProcessStartInfo info = new ProcessStartInfo();
            info.Arguments = "UKUS Comm\\ukus_dds_" + id + "_conf.txt comm " + port;
            info.FileName = "UKUS Comm\\serial_to_dds_gw.exe";
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.UseShellExecute = false;
            info.RedirectStandardError = true;
            info.RedirectStandardOutput = true;
            info.ErrorDialog = true;
            return info;

        }
        public void Close()
        {

        }
        
        public void LockLaser(object laser)
        {
            string msg = "ukus autolock_" + (string)laser;
            slaveConn.WriteLine(msg);
            TextReader reader = slaveConn.ReadStream();
            while (true)
            {
                string line = reader.ReadLine();
                Console.WriteLine(line);
            }
        }
        public void ScanMasterLaser()
        {
            string msg = "ukus autolock_scan_only";
            slaveConn.WriteLine(msg);
        }

        public void UnlockLaser(string laser)
        {
            string msg = "ukus unlock_" + laser;
            slaveConn.WriteLine(msg);
        }
        public void StartEDFA(string edfa)
        {
            string msg = "driver_edfa_tool rearm " + edfa;
            edfaConn.WriteLine(msg);
        }
        public void StopEDFA(string edfa)
        {
            string msg = "driver_edfa_tool shutdown " + edfa;
            edfaConn.WriteLine(msg);
        }
        public void LockEDFA(string edfa, bool lockParam, double voltValue)
        {
            string lockType;
            if (lockParam)
            {
                lockType = "phd_out";
            }
            else
            {
                lockType = "current";
            }
            string msg = "driver_edfa_tool ctrl_"+lockType+" "+edfa+" "+voltValue;
            edfaConn.WriteLine(msg);
            String val = edfaConn.Read();
            Console.WriteLine(val);
        }
        public void UpdateDDS(Dictionary<string,double[]> slaveVals,Dictionary<string,double[]> mphiVals, double[] ramanVals)
        {
            if (slaveVals != null)
            {
            foreach (KeyValuePair<string,double[]> entry in slaveVals)
            {
                string msg = "ukus dds_"+entry.Key+"_set "+entry.Value[0].ToString()+"e6 "+entry.Value[1].ToString();
                ddsSlaveConn.WriteLine(msg);
            }
            }
            if (mphiVals != null)
            {
                foreach (KeyValuePair<string, double[]> entry in mphiVals)
                {
                    string msg = "ukus dds_" + entry.Key + "_set " + entry.Value[0].ToString() + " " + entry.Value[1].ToString();
                    ddsAOMConn.WriteLine(msg);
                }
            }
            if (ramanVals!=null)
            {
                string msg = "ukus dds_raman_set " + ramanVals[0].ToString() + "e6 " + ramanVals[1].ToString();
                ddsSlaveConn.WriteLine(msg);
            }
        }
    }

}