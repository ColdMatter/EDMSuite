using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScanMaster.Acquire.Plugin;
using System.Xml.Serialization;
using DAQ.Environment;
using System.Threading;
using System.Runtime.Remoting;
using TriggeredShapedPulses;

namespace ScanMaster.Acquire.Plugins
{
    [Serializable]
    public class ShapedPulseMakerPlugin : ScanOutputPlugin
    {
        [NonSerialized]
        private double scanParameter = 0;
        [NonSerialized]
        private TriggeredShapedPulses.Controller shapedPulseController;

        protected override void InitialiseSettings()
        {
            //do nothing
        }

        public override void AcquisitionStarting()
        {
            // connect the Shaped Pulse maker over remoting network connection
            string tcpChannel = "1191";
            shapedPulseController = (TriggeredShapedPulses.Controller)(Activator.GetObject(typeof(TriggeredShapedPulses.Controller), "tcp://localhost:" + tcpChannel + "/controller.rem"));

            scanParameter = 0;
        }

        public override void ScanStarting()
        {
            //do nothing
        }

        public override void ScanFinished()
        {
            //do nothing         
        }

        public override void AcquisitionFinished()
        {
            //do nothing
        }

        [XmlIgnore]
        public override double ScanParameter
        {
            set
            {
                scanParameter = value;
            }
            get { return scanParameter; }
        }
    }
}
