using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScanMaster.Acquire.Plugin;
using DAQ.DigitalTransferCavityLock;
using System.Xml.Serialization;
using DAQ.Environment;
using System.Threading;
using System.Runtime.Remoting;
using System.Net;
using System.Net.Sockets;

namespace ScanMaster.Acquire.Plugins
{
    [Serializable]
    public class DTCLOutputPlugin : ScanOutputPlugin
    {

        [NonSerialized]
        private double scanParameter = 0;
        private string computer;
        private string name;
        private string hostName = (String)System.Environment.GetEnvironmentVariables()["COMPUTERNAME"];
        private double initialSetPoint = 0.0;
        [NonSerialized]
        private DigitalTransferCavityLock.Controller tclController;


        protected override void InitialiseSettings()
        {
            settings["channel"] = "laser";
            settings["cavity"] = "Hamish";
            settings["computer"] = hostName;
            settings["DTCLConfig"] = "DTCLConfig";
            settings["rampSteps"] = 100;
            // DTCL has faster lock rate, but keeping this at 500 for safety.
            settings["setSetPointWaitTime"] = 500;
        }



        public override void AcquisitionStarting()
        {
             //connect the DTCL controller over remoting network connection


            if (settings["computer"] == null)
            {
                computer = hostName;
            }
            else
            {
                computer = (String)settings["computer"];
            }

            IPHostEntry hostInfo = Dns.GetHostEntry(computer);

            foreach (var addr in Dns.GetHostEntry(computer).AddressList)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                name = addr.ToString();
            }

            EnvironsHelper eHelper = new EnvironsHelper(computer);

            string tcpChannel = ((DTCLConfig)eHelper.Hardware.GetInfo(settings["DTCLConfig"])).TCPChannel.ToString();

            tclController = (DigitalTransferCavityLock.Controller)(Activator.GetObject(typeof(DigitalTransferCavityLock.Controller), "tcp://"+ name + ":" + tcpChannel + "/controller.rem"));

            scanParameter = 0;

            initialSetPoint = tclController.GetSetoint((string)settings["cavity"], (string)settings["channel"]);


            tclController.LockLaser((string)settings["cavity"], (string)settings["channel"]);
            if ((string)settings["scanMode"] == "up" || (string)settings["scanMode"] == "updown")
            {
                tclController.UpdateLockPoint((string)settings["cavity"], (string)settings["channel"], (double)settings["start"]);
            }
            if ((string)settings["scanMode"] == "down" || (string)settings["scanMode"] == "downup")
            {
                tclController.UpdateLockPoint((string)settings["cavity"], (string)settings["channel"], (double)settings["end"]);
            }

        }
        

        public override void ScanStarting()
        {
            //Do Nothing   
        }

        public override void ScanFinished()
        {
            if ((string)settings["scanMode"] == "up" || (string)settings["scanMode"] == "updown")
            {
                tclController.UpdateLockPoint((string)settings["cavity"], (string)settings["channel"], (double)settings["start"]);
            }
            if ((string)settings["scanMode"] == "down" || (string)settings["scanMode"] == "downup")
            {
                tclController.UpdateLockPoint((string)settings["cavity"], (string)settings["channel"], (double)settings["end"]);
            }
        }

        public override void AcquisitionFinished()
        {
            //go gently to the initial position
            tclController.UpdateLockPoint((string)settings["cavity"], (string)settings["channel"], initialSetPoint);
        }

        [XmlIgnore]
        public override double ScanParameter
        {
            set
            {
                scanParameter = value;
                if (!Environs.Debug) tclController.UpdateLockPoint((string)settings["cavity"], (string)settings["channel"], scanParameter);
            }
            get { return scanParameter; }
        }

    }
}
