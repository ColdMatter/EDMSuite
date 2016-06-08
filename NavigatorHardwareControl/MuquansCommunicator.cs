using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Diagnostics;
using System.IO;


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

        public TelnetConnection slaveConn = new TelnetConnection(slaveIPAddress, 23);
        public TelnetConnection edfaConn = new TelnetConnection(edfaIPAddress, 23);
        public TelnetConnection ddsSlaveConn = new TelnetConnection(ddsSlaveIPAddress, 23);
        public TelnetConnection ddsAOMConn = new TelnetConnection(ddsAOMIPAddress, 23);

        public Process slaveDDS = new Process();
        public Process aomDDS = new Process();


        public void Start()
        {

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

        public TextReader LockLaser(string laser)
        {
            string msg = "ukus autolock_" + laser;
            slaveConn.WriteLine(msg);
            TextReader reader = slaveConn.ReadStream();
            //Returns the reader so other classes can do stuff with the output and then close the reader
            return reader;
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
    }

}