using System;
using System.Linq;
using System.Collections.Generic;
using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;
using DAQ.DigitalTransferCavityLock;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace DigitalTransferCavityLock
{
    public class Controller : MarshalByRefObject
    {
        public ControlWindow window;
        public RampGenerator rampGen;

        public Dictionary<string,CavityControl> cavities = new Dictionary<string, CavityControl> { };
        public DTCLConfig config
        {
            get
            {
                return (DTCLConfig)Environs.Hardware.GetInfo("DTCLConfig");
            }
        }

        public void windowLoaded()
        {
            rampGen = new RampGenerator(config.rampOut, config.synchronisationCounter, config.TimebaseChannel.PhysicalChannel, config.timebaseFrequency, this, config.resetOut);
            window.RampAmplitude.Text = config.defaultRampAmplitude.ToString();
            window.RampOffset.Text = config.defaultRampOffset.ToString();
            window.RampFreq.Text = config.defaultRampFrequency.ToString();

            foreach (DTCLCavityConfig cConfig in config.cavities.Values)
            {
                CavityControl cControl = new CavityControl(cConfig.Name, cConfig.MasterLaser.feedbackChannel, cConfig.MasterLaser.counterChannel
                    , cConfig.MasterLaser.inputChannel, cConfig.MasterLaser.TimebaseChannel.PhysicalChannel, cConfig.MasterLaser.timebaseFrequency, cConfig.MasterLaser.SyncChannel.PhysicalChannel, rampGen);

                cControl.Gain.Text = config.defaultCavityGain.ToString();

                TabPage tp = new TabPage(cControl.CavityName);
                tp.Controls.Add(cControl);
                window.CavityTabs.TabPages.Add(tp);
                cavities.Add(cControl.CavityName, cControl);

                foreach (DTCLCavityConfig.LaserConfig laserconfig in cConfig.SlaveLasers.Values)
                {
                    SlaveLaserControl sl = new SlaveLaserControl(laserconfig.Name, laserconfig.feedbackChannel, cControl, laserconfig.counterChannel
                    , laserconfig.inputChannel, laserconfig.TimebaseChannel.PhysicalChannel, laserconfig.timebaseFrequency, laserconfig.SyncChannel.PhysicalChannel, rampGen);
                    sl.slaveGain.Text = config.defaultGain.ToString();
                    cControl.AddSlave(sl);
                }

            }
        }

        #region Runtime Update

        public int loopCount = 0;

        public void ReadSamples()
        {
            bool updateGUI = !window.GUIDisable.Checked || loopCount % ((int)500000 / rampGen.samplesPerHalfPeriod) == 0;
            foreach (CavityControl cavity in cavities.Values)
                cavity.InitDAQ();
            foreach (CavityControl cavity in cavities.Values)
                cavity.UpdateData(updateGUI);
        }

        List<double> sElapsed = new List<double> { };
        public void UpdateLockRate(Stopwatch watch)
        {
            TimeSpan elapsed = watch.Elapsed;
            sElapsed.Add(elapsed.TotalSeconds);
            if (sElapsed.Count > 100)
                sElapsed.RemoveAt(0);
            window.SetTextField(window.LockRate, Convert.ToString(sElapsed.Count/(sElapsed.Sum())));
        }

        public Stopwatch watch = new Stopwatch();
        public bool ready = true;
        public bool updateReady = false;
        public bool close = false;
        public void Update()
        {
            while (window.StartRamp.Checked && !close)
            {
                Thread.Sleep(0);
                if (!updateReady) continue;
                if (/*!ready ||*/ loopCount % 2 == 1) continue;
                updateReady = false;
                //ready = false;
                ReadSamples();
                window.UpdatePlot();
                watch.Stop();
                UpdateLockRate(watch);
                watch.Reset();
                watch.Start();
                //ready = true;
            }
        }

        #endregion

        #region External Control

        public void UpdateLockPoint(string cavity, string laser, double lockPoint)
        {
            cavities[cavity].slaveLasers[laser].SetTextField(cavities[cavity].slaveLasers[laser].slaveLockLocV,lockPoint.ToString());
        }

        public double GetSetoint(string cavity, string laser)
        {
            return Convert.ToDouble(cavities[cavity].slaveLasers[laser].slaveLockLocV.Text);
        }

        public void LockLaser(string cavity, string laser)
        {
            cavities[cavity].UpdateRenderedObject(cavities[cavity], (CavityControl cc) =>
            {
                cc.LockReference.Checked = true;
                cc.slaveLasers[laser].LockSlave.Checked = true;
            });
        }

        #endregion
    }
}
