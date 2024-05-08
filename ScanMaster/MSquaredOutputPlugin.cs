using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScanMaster.Acquire.Plugin;

using DAQ;
using System.Xml.Serialization;
using DAQ.Environment;
using System.Threading;
using System.Runtime.Remoting;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace ScanMaster.Acquire.Plugins
{
    /// <summary>
	/// A plugin to step the M2 Laser setpoint frequency
    /// Mostly ripped off from WML
    /// Scans from offset(THz)+start(GHz) to offset(THz)+end(GHz) in setpoint mode
	/// </summary>
    [Serializable]
    public class MSquaredOutputPlugin : ScanOutputPlugin
    {

        [NonSerialized]
        private double scanParameter = 0;
        [NonSerialized]
        private M2LaserInterface laser;


        protected override void InitialiseSettings()
        {
            settings["local_addr"] = "192.168.1.1:29922";
            settings["remote_addr"] = "192.168.1.222:29922";
            settings["setSetPointWaitTime"] = 500;
            settings["useHTTPFallback"] = false;
            settings["offset"] = 0.0; //Frequency offset in THz
        }



        public override void AcquisitionStarting()
        {

            laser = M2LaserInterface.getInterface((string) settings["local_addr"], (string) settings["remote_addr"]);

            scanParameter = 0;

            engage_lock();

            //go gently to the correct start position
            if ((string)settings["scanMode"] == "up" || (string)settings["scanMode"] == "updown")
            {
                move_to((double)settings["start"]);
            }
            else if ((string)settings["scanMode"] == "down" || (string)settings["scanMode"] == "downup")
            {
                move_to((double)settings["start"] + (double)settings["end"] / 1000);
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
                move_to((double)settings["start"]);
            }
            if ((string)settings["scanMode"] == "down")
            {
                move_to((double)settings["start"] + (double)settings["end"] / 1000.0);
            }
            //all other cases, do nothing
        }

        public override void AcquisitionFinished()
        {
            // Do Nothing
        }

        [XmlIgnore]
        public override double ScanParameter
        {
            set
            {
                scanParameter = value;
                if (!Environs.Debug) set_to(value);
            }
            get { return scanParameter; }
        }

        private void engage_lock()
        {
            /*Dictionary<string, object> data = */laser.IssueCommand("lock_wave_m", new Dictionary<string, object>
            {
                { "operation", "on" }
            });

            /*if (data == null || (string)((Dictionary<string, object>)data["message"])["op"] != "lock_wave_m_reply")
            {
                throw new IOException("Invalid reply from ICE-Block");
            }*/
        }

        private void update_lock_point(double freq)
        {

            if ((bool)settings["useHTTPFallback"])
                laser.set_wavelength(299_792_458 / (freq / 1000.0 + (double)settings["offset"]) / 1e3);
            else
                /*Dictionary<string, object> data = */
                laser.IssueCommand("set_wave_m", new Dictionary<string, object>
                {
                    { "wavelength", new List<object> { 299_792_458 / (freq / 1000.0 + (double)settings["offset"]) / 1e3 } }
                });

            /*if (data == null || (string)((Dictionary<string,object>)data["message"])["op"] != "set_wave_m_reply")
            {
                throw new IOException("Invalid reply from ICE-Block");
            }*/
        }

        private void set_to(double f)
        {

            update_lock_point(f);
            Thread.Sleep((int)settings["setSetPointWaitTime"]);

        }

        // we need to ramp the laser output voltage slowly back to starting, but not the set point
        private void move_to(double f)
        {
            update_lock_point(f);
            Thread.Sleep(10 * (int)settings["setSetPointWaitTime"]);
            scanParameter = f;
        }

    }
}
