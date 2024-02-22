using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScanMaster.Acquire.Plugin;

using System.Xml.Serialization;
using DAQ.Environment;
using System.Threading;
using System.Windows.Forms;

namespace ScanMaster.Acquire.Plugins
{
    /// <summary>
	/// A plugin to step the WavemeterLock setpoint frequency
    /// Mostly ripped off from TCL
    /// Scans from offset(THz)+start(GHz) to offset(THz)+end(GHz) in setpoint mode
    /// Scans from start(V) to end(V) in voltage mode
	/// </summary>
    [Serializable]
    public class ManualOutputPlugin : ScanOutputPlugin
    {

        [NonSerialized]
        private double scanParameter = 0;


        protected override void InitialiseSettings()
        {
            settings["Scanned Parameter"] = "";
        }



        public override void AcquisitionStarting()
        {
            //Do Nothing

        }


        public override void ScanStarting()
        {
            //Do Nothing   
        }

        public override void ScanFinished()
        {
            //Do Nothing
        }

        public override void AcquisitionFinished()
        {
            //Do Nothing
        }

        [XmlIgnore]
        public override double ScanParameter
        {
            set
            {
                scanParameter = value;
                MessageBox.Show("Please set " + settings["Scanned Parameter"] + " to " + value.ToString() + ".");
            }
            get { return scanParameter; }
        }


    }
}
