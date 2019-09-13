using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScanMaster.Acquire.Plugin;
using TransferCavityLock2012;
using DAQ.TransferCavityLock2012;
using System.Xml.Serialization;
using DAQ.Environment;
using System.Threading;
using System.Runtime.Remoting;
using System.Net;
using System.Net.Sockets;

namespace ScanMaster.Acquire.Plugins
{
    [Serializable]
    public class TCLOutputPlugin : ScanOutputPlugin
    {

        [NonSerialized]
        private double scanParameter = 0;
        private string computer;
        private string name;
        private string hostName = (String)System.Environment.GetEnvironmentVariables()["COMPUTERNAME"];
        private string scannedParameter;
        private double initialVoltage = 0.0;
        private double initialSetPoint = 0.0;
        [NonSerialized]
        private TransferCavityLock2012.Controller tclController;


        protected override void InitialiseSettings()
        {
            settings["channel"] = "laser";
            settings["cavity"] = "Hamish";
            settings["computer"] = hostName;
            settings["scannedParameter"] = "setpoint";
            settings["TCLConfig"] = "TCLConfig";
        }



        public override void AcquisitionStarting()
        {
             //connect the TCL controller over remoting network connection


            if (settings["computer"] == null)
            {
                computer = hostName;
            }
            else
            {
                computer = (String)settings["computer"];
            }

            if (settings["scannedParameter"] == null)
            {
                scannedParameter = "setpoint";
            }
            else
            {
                scannedParameter = (String)settings["scannedParameter"];
            }

            IPHostEntry hostInfo = Dns.GetHostEntry(computer);

            foreach (var addr in Dns.GetHostEntry(computer).AddressList)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                name = addr.ToString();
            }

            EnvironsHelper eHelper = new EnvironsHelper(computer);

            string tcpChannel = ((TCLConfig)eHelper.Hardware.GetInfo(settings["TCLConfig"])).TCPChannel.ToString();

            tclController = (TransferCavityLock2012.Controller)(Activator.GetObject(typeof(TransferCavityLock2012.Controller), "tcp://"+ name + ":" + tcpChannel + "/controller.rem"));

            scanParameter = 0;

            initialVoltage = tclController.GetLaserVoltage((string)settings["cavity"],(string)settings["channel"]);
            initialSetPoint = tclController.GetLaserSetpoint((string)settings["cavity"], (string)settings["channel"]);
            if (scannedParameter == "voltage")
            {
                tclController.UnlockLaser((string)settings["cavity"], (string)settings["channel"]);
            }
            setV((double)settings["start"], 200, scannedParameter);
        }
        

        public override void ScanStarting()
        {
            //Do Nothing   
        }

        public override void ScanFinished()
        {
            setV((double)settings["start"], 200, scannedParameter);
        }

        public override void AcquisitionFinished()
        {
            setV(initialVoltage, 200, "voltage");
            setV(initialSetPoint, 200, "setpoint");
            tclController.LockLaser((string)settings["cavity"], (string)settings["channel"]);
        }

        [XmlIgnore]
        public override double ScanParameter
        {
            set
            {
                scanParameter = value;
                if (!Environs.Debug) setV(value, 50, scannedParameter);
            }
            get { return scanParameter; }
        }

        private void setV(double v, int waitTime,string scannedOutput)
        {
            switch (scannedOutput)
            {
            case "setpoint":
                    tclController.SetLaserSetpoint((string)settings["cavity"], (string)settings["channel"], v);
                break;
            case "voltage":
                tclController.SetLaserOutputVoltage((string)settings["cavity"], (string)settings["channel"], v);
                tclController.RefreshVoltageOnUI((string)settings["cavity"], (string)settings["channel"]);
                break;
            }
            Thread.Sleep(waitTime);
        }




    }
}
