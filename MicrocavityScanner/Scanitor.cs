using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.Environment;
using DAQ.TransferCavityLock2012;
using ScanMaster;

namespace MicrocavityScanner.Acquire
{
    public class Scanitor
    {
        private TransferCavityLock2012.Controller tclController;
        private ScanMaster.Controller smController;

        public void ConnectRemoting()
        {
            // connect the TCL controller over remoting network connection
            string tcpChannel = ((TCLConfig)Environs.Hardware.GetInfo(settings["cavity"])).TCPChannel.ToString();
            tclController = (TransferCavityLock2012.Controller) (Activator.GetObject(typeof(TransferCavityLock2012.Controller), "tcp://localhost:" + tcpChannel + "/controller.rem"));

            // connect the ScanMaster controller over remoting network connection
            smController = (ScanMaster.Controller) (Activator.GetObject(typeof(TransferCavityLock2012.Controller), "tcp://localhost:1170/controller.rem"));
        }
    }
}
