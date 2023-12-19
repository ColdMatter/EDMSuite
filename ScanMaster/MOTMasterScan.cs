using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScanMaster.Acquire.Plugin;

using DAQ.WavemeterLock;
using System.Xml.Serialization;
using DAQ.Environment;
using System.Threading;
using System.Runtime.Remoting;
using System.Net;
using System.Net.Sockets;

namespace ScanMaster.Acquire.Plugins
{
    /// <summary>
	/// A plugin to step the WavemeterLock setpoint frequency
    /// Mostly ripped off from TCL
    /// Scans from offset(THz)+start(GHz) to offset(THz)+end(GHz) in setpoint mode
    /// Scans from start(V) to end(V) in voltage mode
	/// </summary>
    [Serializable]
    public class MOTMasterScan : ScanOutputPlugin
    {

        private double scanParameter;
        private int scanProgress;

        protected override void InitialiseSettings()
        {
            Settings["scanOut"] = null;
            Settings["scanKey"] = null;
        }



        public override void AcquisitionStarting()
        {
            scanProgress = 0;
        }


        public override void ScanStarting()
        {
        }

        public override void ScanFinished()
        {
        }

        public override void AcquisitionFinished()
        {
        }

        [XmlIgnore]
        public override double ScanParameter
        {
            set
            {
                scanParameter = value;
                ((Dictionary<string,Object>)Settings["scanOut"])[(string)Settings["scanKey"]] = scanParameter;
            }
            get { return scanParameter; }
        }
    }
}
