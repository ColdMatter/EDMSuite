using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScanMaster.Acquire.Plugin;
using WavemeterLock;
using DAQ.WavemeterLock;
using System.Xml.Serialization;
using DAQ.Environment;
using System.Threading;
using System.Runtime.Remoting;
using System.Net;
using System.Net.Sockets;

namespace ScanMaster.Acquire.Plugins
{
    [Serializable]
    public class WMLOutputPlugin : ScanOutputPlugin
    {

        [NonSerialized]
        private double scanParameter = 0;
        private string computer;
        private string name;
        private string hostName = (String)System.Environment.GetEnvironmentVariables()["COMPUTERNAME"];
        private string scannedParameter;
        private double initialFrequency = 0.0;
        private double initialVoltage = 0.0;
        [NonSerialized]
        private WavemeterLock.Controller wmlController;


        protected override void InitialiseSettings()
        {
            settings["name"] = "laser";
            settings["computer"] = hostName;
            settings["WMLConfig"] = "WMLConfig";
            settings["rampSteps"] = 100;
            settings["scannedParameter"] = "setpoint";
            settings["setVoltageWaitTime"] = 50;
            settings["setSetPointWaitTime"] = 500;
        }



        public override void AcquisitionStarting()
        {
            //connect the WML controller over remoting network connection


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

            string tcpChannel = "6666";

            wmlController = (WavemeterLock.Controller)(Activator.GetObject(typeof(WavemeterLock.Controller), "tcp://" + name + ":" + tcpChannel + "/controller.rem"));

            scanParameter = 0;

            initialVoltage = wmlController .getSlaveVoltage((string)settings["name"]);
            initialFrequency = wmlController.getSlaveFrequency((string)settings["name"]);
            if (scannedParameter == "voltage")
            {
                wmlController.DisengageLock((string)settings["name"]);
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
                wmlController.EngageLock((string)settings["name"]);
            }
            if (scannedParameter == "setpoint")
            {
                rampV(initialFrequency, "setpoint");
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

        private void setV(double f, string scannedOutput)
        {
            switch (scannedOutput)
            {
                case "setpoint":
                    wmlController.setSlaveFrequency((string)settings["name"], f);
                    Thread.Sleep((int)settings["setSetPointWaitTime"]);
                    break;
                case "voltage":
                    wmlController.setSlaveVoltage((string)settings["name"], f);
                    Thread.Sleep((int)settings["setVoltageWaitTime"]);
                    break;
            }

        }

        // we need to ramp the laser output voltage slowly back to starting, but not the set point
        private void rampV(double f, string scannedOutput)
        {
            switch (scannedOutput)
            {
                case "setpoint":
                    wmlController.setSlaveFrequency((string)settings["name"], f);
                    // since we are moving the set point by quite a distance, wait for WML to move the laser to it by waiting 10x longer than usual
                    Thread.Sleep(10 * (int)settings["setSetPointWaitTime"]);
                    break;
                case "voltage":
                    for (int i = 1; i <= (int)settings["rampSteps"]; i++)
                    {
                        wmlController.setSlaveVoltage((string)settings["name"], initialVoltage - (i * (initialVoltage - f) / (int)settings["rampSteps"]));
                        Thread.Sleep((int)settings["setVoltageWaitTime"]);
                    }
                    break;
            }

            scanParameter = f;
        }

    }
}
