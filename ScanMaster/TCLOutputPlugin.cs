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
            settings["rampSteps"] = 100;
            settings["setVoltageWaitTime"] = 50;
            // TCL updates at a rate of 4-6 Hz, so waiting 500ms gives it at least 2/3 ramp cycles to move the laser to the correct frequency
            settings["setSetPointWaitTime"] = 500;
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

            //go gently to the correct start position
            if ((string)settings["scanMode"] == "up" || (string)settings["scanMode"] == "updown")
            {
                rampV((double)settings["start"], scannedParameter);
            }
            if ((string)settings["scanMode"] == "down" || (string)settings["scanMode"] == "downup")
            {
                rampV((double)settings["end"], scannedParameter);
            }

        }
        

        public override void ScanStarting()
        {
            //Do Nothing   
        }

        public override void ScanFinished()
        {
            //go gently to the correct start position
            if ((string)settings["scanMode"] == "up")
            {
                rampV((double)settings["start"], scannedParameter);
            }
            if ((string)settings["scanMode"] == "down")
            {
                rampV((double)settings["end"], scannedParameter);
            }
            //all other cases, do nothing
        }

        public override void AcquisitionFinished()
        {
            //go gently to the initial position
            if (scannedParameter == "voltage")
            {
                rampV(initialVoltage, "voltage");
                tclController.LockLaser((string)settings["cavity"], (string)settings["channel"]);
            }
            if (scannedParameter == "setpoint")
            {
                rampV(initialSetPoint, "setpoint");
            }
        }

        [XmlIgnore]
        public override double ScanParameter
        {
            set
            {
                scanParameter = value;
                if (!Environs.Debug) setV(value, scannedParameter);
            }
            get { return scanParameter; }
        }

        private void setV(double v, string scannedOutput)
        {
            switch (scannedOutput)
            {
            case "setpoint":
                    tclController.SetLaserSetpoint((string)settings["cavity"], (string)settings["channel"], v);
                    Thread.Sleep((int)settings["setSetPointWaitTime"]);
                    break;
            case "voltage":
                    tclController.SetLaserOutputVoltage((string)settings["cavity"], (string)settings["channel"], v);
                    tclController.RefreshVoltageOnUI((string)settings["cavity"], (string)settings["channel"]);
                    Thread.Sleep((int)settings["setVoltageWaitTime"]);
                    break;
            }
        }

        // we need to ramp the laser output voltage slowly back to starting, but not the set point
        private void rampV(double v, string scannedOutput)
        {
            switch (scannedOutput)
            {
                case "setpoint":
                    tclController.SetLaserSetpoint((string)settings["cavity"], (string)settings["channel"], v);
                    // since we are moving the set point by quite a distance, wait for TCL to move the laser to it by waiting 10x longer than usual
                    Thread.Sleep(10*(int)settings["setSetPointWaitTime"]);
                    break;
                case "voltage":
                    for (int i = 1; i <= (int)settings["rampSteps"]; i++)
                    {
                        tclController.SetLaserOutputVoltage((string)settings["cavity"], (string)settings["channel"], scanParameter - (i * (scanParameter - v) / (int)settings["rampSteps"]));
                        tclController.RefreshVoltageOnUI((string)settings["cavity"], (string)settings["channel"]);
                        Thread.Sleep((int)settings["setVoltageWaitTime"]);
                    }
                    break;
            }
            
            scanParameter = v;
        }

    }
}
