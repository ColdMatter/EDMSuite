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

namespace ScanMaster.Acquire.Plugins
{
    [Serializable]
    public class TCLOutputPlugin : ScanOutputPlugin
    {

        [NonSerialized]
        private double scanParameter = 0;
        [NonSerialized]
        private TransferCavityLock2012.Controller tclController;

        protected override void InitialiseSettings()
        {
            settings["channel"] = "laser";
            settings["cavity"] = "Hamish";
            settings["settlingTime"] = 50;
        }


        public override void AcquisitionStarting()
        {
            // connect the TCL controller over remoting network connection
            string tcpChannel = ((TCLConfig)Environs.Hardware.GetInfo(settings["cavity"])).TCPChannel.ToString();
            tclController = (TransferCavityLock2012.Controller)(Activator.GetObject(typeof(TransferCavityLock2012.Controller), "tcp://localhost:" + tcpChannel + "/controller.rem"));
            
            scanParameter = 0;

            setV((double)settings["start"], 200);
        }

        public override void ScanStarting()
        {
            //do nothing
        }

        public override void ScanFinished()
        {
            setV((double)settings["start"], 200);
        }

        public override void AcquisitionFinished()
        {
            setV((double)settings["start"], 200);
        }

        [XmlIgnore]
        public override double ScanParameter
        {
            set
            {
                scanParameter = value;
                if (!Environs.Debug) setV(value, (int)settings["settlingTime"]);
            }
            get { return scanParameter; }
        }

        private void setV(double v, int waitTime)
        {
            tclController.SetLaserSetpoint((string)settings["channel"], v);
            Thread.Sleep(waitTime);
        }




    }
}
