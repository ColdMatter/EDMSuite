using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NationalInstruments.UI.WindowsForms;
using DAQ.DigitalTransferCavityLock;
using DAQ.Environment;

namespace DigitalTransferCavityLock
{
    public partial class SlaveLaserControl : UserControl
    {
        public string laserName;
        public Laser laser;
        public CounterReaderHelper laserCounter;
        public RampGenerator rampGen;
        public CavityControl cavity;

        public double VToMHz; 

        public SlaveLaserControl(string name, string feedback, CavityControl _cavity, string counter, string samplingClock, string refClock, double _refClockFreq, string sync, RampGenerator _rampGen)
        {
            InitializeComponent();
            laserName = name;
            cavity = _cavity;
            laser = new Laser(() => { return laserCounter.dataMS; }, feedback, cavity.cavity);
            laserCounter = new CounterReaderHelper(counter, samplingClock, refClock, _refClockFreq, sync);
            rampGen = _rampGen;
            VToMHz = ((DTCLConfig)Environs.Hardware.GetInfo("DTCLConfig")).MHzConv;
        }

        public void InitDAQ()
        {
            if (this.InputEnable.Checked)
                laserCounter.InitiateRead();
            else
            {
                laserCounter.fail = true;
                laserCounter.ready = true;
            }

        }

        public int t = 0;
        public void UpdateData(bool updateGUI)
        {
            while (!laserCounter.ready)
            {
                if (!this.InputEnable.Checked)
                    return;
                Thread.Sleep(0);
            }
            laser.UpdateLock();
            if (laser.Locked)
            {
                this.SetTextField(this.SlaveVoltageFeedback, laser.CurrentVoltage.ToString());
                double MHzError = rampGen.ConvertToVoltage(laser.LockError)*VToMHz;
                MHzErr.Add(MHzError * MHzError);
                if (MHzErr.Count > 1000) MHzErr.RemoveAt(0);
                SetTextField(this.RMSError, Math.Sqrt(MHzErr.Average()).ToString());
                if (!updateGUI) return;
                ++t;
                UpdateRenderedObject(this.ErrorGraph, (ScatterGraph g) => { g.PlotXYAppend(t, MHzError); });
            }
            if (!updateGUI) return;
            if (!laserCounter.fail)
            {
                SetTextField(this.slaveLocMS, laserCounter.dataMS.ToString());
                SetTextField(this.slaveLocV, rampGen.ConvertToVoltage(laserCounter.dataMS).ToString());
            }
            else
            {
                SetTextField(this.slaveLocMS, "No Data");
                SetTextField(this.slaveLocV, "No Data");
            }

        }

        public void UpdateRenderedObject<T>(T obj, Action<T> updateFunc) where T : Control
        {
            obj.Invoke(new UpdateObjectDelegate<T>(UpdateObject), new object[] { obj, updateFunc });
        }

        private delegate void UpdateObjectDelegate<T>(T obj, Action<T> updateFunc) where T : Control;

        private void UpdateObject<T>(T obj, Action<T> updateFunc) where T : Control
        {
            updateFunc(obj);
        }

        public void SetTextField(Control box, string text)
        {
            box.Invoke(new SetTextDelegate(SetTextHelper), new object[] { box, text });
        }

        private delegate void SetTextDelegate(Control box, string text);

        private void SetTextHelper(Control box, string text)
        {
            box.Text = text;
        }

        private void SlaveVoltageFeedback_Leave(object sender, EventArgs e)
        {
            laser.CurrentVoltage = Convert.ToDouble(SlaveVoltageFeedback.Text);
            this.SetTextField(this.SlaveVoltageFeedback, Convert.ToString(laser.CurrentVoltage));
        }

        private void LockSlave_CheckedChanged(object sender, EventArgs e)
        {
            if (LockSlave.Checked)
            {
                laser.ArmLock(Convert.ToDouble(this.slaveLockLocV.Text), Convert.ToDouble(slaveGain.Text));
                this.UpdateRenderedObject(cavity.LockReference, (Control a) => { a.Enabled = false; });
                this.UpdateRenderedObject(this.SlaveVoltageFeedback, (Control a) => { a.Enabled = false; });
                this.UpdateRenderedObject(this.InputEnable, (Control a) => { a.Enabled = false; });
                //this.UpdateRenderedObject(this.slaveGain, (Control a) => { a.Enabled = false; });
            }
            else
            {
                laser.DisarmLock();
                cavity.UpdateAfterSlaveUnlock();
                this.UpdateRenderedObject(this.SlaveVoltageFeedback, (Control a) => { a.Enabled = true; });
                this.UpdateRenderedObject(this.InputEnable, (Control a) => { a.Enabled = true; });
                //this.UpdateRenderedObject(this.slaveGain, (Control a) => { a.Enabled = true; });
            }
        }

        private void InputEnable_CheckedChanged(object sender, EventArgs e)
        {
            if (InputEnable.Checked)
            {
                if(cavity.LockReference.Checked)
                    this.UpdateRenderedObject(this.LockSlave, (Control c) => { c.Enabled = true; });
                laserCounter.SetUpTask(rampGen.periodMS);
                laserCounter.ready = true;
                laserCounter.fail = true;
            }
            else
            {
                this.UpdateRenderedObject(this.LockSlave, (Control c) => { c.Enabled = false; });
                laserCounter.DismissTask();
            }
        }

        private void slaveLockLocV_Leave(object sender, EventArgs e)
        {
            laser.LockLevel = Convert.ToDouble(slaveLockLocV.Text);
        }

        private void slaveGain_Leave(object sender, EventArgs e)
        {
            laser.gain = Convert.ToDouble(slaveGain.Text);
        }

        public List<double> MHzErr = new List<double>();

        private void ResetRMS_Click(object sender, EventArgs e)
        {
            UpdateRenderedObject(this.ErrorGraph, (ScatterGraph g) => { g.ClearData(); });
            MHzErr.Clear();
        }

        public void UpdateRampPlot(ControlWindow win)
        {
            List<double> x = new List<double>();
            List<double> y = new List<double>();
            y.Add(rampGen.Offset);
            y.Add(rampGen.Offset + 0.25 * rampGen.Amplitude);
            y.Add(rampGen.Offset + 0.50 * rampGen.Amplitude);
            y.Add(rampGen.Offset + 0.75 * rampGen.Amplitude);
            y.Add(rampGen.Offset + 1.00 * rampGen.Amplitude);
            if (this.InputEnable.Checked)
            {
                x.Add(laserCounter.dataMS);
                x.Add(laserCounter.dataMS);
                x.Add(laserCounter.dataMS);
                x.Add(laserCounter.dataMS);
                x.Add(laserCounter.dataMS);
            }
            else
            {
                x.Add(0);
                x.Add(0);
                x.Add(0);
                x.Add(0);
                x.Add(0);
            }
            win.UpdateRenderedObject<NationalInstruments.UI.WindowsForms.ScatterGraph>(win.PeakPlot, (NationalInstruments.UI.WindowsForms.ScatterGraph g) => { g.Plots[2].PlotXY(x.ToArray(), y.ToArray()); });
        }
    }
}
