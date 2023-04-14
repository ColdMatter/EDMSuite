using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace DigitalTransferCavityLock
{
    public partial class CavityControl : UserControl
    {

        public Cavity cavity;
        public CounterReader cavityCounter;
        public RampGenerator rampGen;
        public List<SlaveLaserControl> slaveLasers = new List<SlaveLaserControl> { };
        public string CavityName;
        public CavityControl(string name, string feedback, string counter, string samplingClock, string refClock, double _refClockFreq, string sync, RampGenerator _rampGen)
        {
            InitializeComponent();
            CavityName = name;
            cavityCounter = new CounterReader(counter, samplingClock, refClock, _refClockFreq, sync);
            cavity = new Cavity(() => { return cavityCounter.dataMS; }, feedback);
            rampGen = _rampGen;
        }

        public void AddSlave(SlaveLaserControl sl)
        {
            TabPage tp = new TabPage(sl.laserName);
            tp.Controls.Add(sl);
            slaveLasers.Add(sl);
            this.SlaveLasersTabs.Controls.Add(tp);
        }

        public void InitDAQ()
        {
            if (this.EnableData.Checked)
                cavityCounter.InitiateRead();
            else
            {
                cavityCounter.fail = true;
                cavityCounter.ready = true;
            }
            foreach (SlaveLaserControl sl in slaveLasers)
                sl.InitDAQ();
        }

        public void UpdateData(bool updateGUI)
        {
            while (!cavityCounter.ready)
            {
                if (!this.EnableData.Checked)
                    return;
                Thread.Sleep(0);
            }
            cavity.UpdateLock();
            if(cavity.Locked)
                this.SetTextField(this.RefVoltageFeedback, cavity.CurrentVoltage.ToString());

            foreach (SlaveLaserControl sl in slaveLasers)
                sl.UpdateData(updateGUI);

            if (!updateGUI) return;
            if (!cavityCounter.fail)
            {
                SetTextField(this.refLocMS, cavityCounter.dataMS.ToString());
                SetTextField(this.refLocV, rampGen.ConvertToVoltage(cavityCounter.dataMS).ToString());
            }
            else
            {
                SetTextField(this.refLocMS, "No Data");
                SetTextField(this.refLocV, "No Data");
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

        private void LockReference_CheckedChanged(object sender, EventArgs e)
        {
            if (LockReference.Checked)
            {
                cavity.ArmLock(Convert.ToDouble(this.refLockLocV.Text), Convert.ToDouble(this.Gain.Text));
                this.UpdateRenderedObject(this.RefVoltageFeedback, (Control a) => { a.Enabled = false; });
                //this.UpdateRenderedObject(this.Gain, (Control a) => { a.Enabled = false; });
                foreach (SlaveLaserControl laser in slaveLasers)
                    if(laser.InputEnable.Checked)
                        laser.UpdateRenderedObject(laser.LockSlave ,(Control a) => { a.Enabled = true; });
                this.UpdateRenderedObject(this.EnableData, (Control a) => { a.Enabled = false; });
            }
            else
            {
                cavity.DisarmLock();
                this.UpdateRenderedObject(this.RefVoltageFeedback, (Control a) => { a.Enabled = true; });
                //this.UpdateRenderedObject(this.Gain, (Control a) => { a.Enabled = true; });
                foreach (SlaveLaserControl laser in slaveLasers)
                    laser.UpdateRenderedObject(laser.LockSlave, (Control a) => { a.Enabled = false; });
                this.UpdateRenderedObject(this.EnableData, (Control a) => { a.Enabled = true; });
            }
        }

        public void UpdateAfterSlaveUnlock()
        {
            foreach (SlaveLaserControl laser in slaveLasers)
                if (laser.LockSlave.Checked) return;
            this.UpdateRenderedObject(this.LockReference, (Control c) => { c.Enabled = true; });
        }

        private void EnableData_CheckedChanged(object sender, EventArgs e)
        {
            if (EnableData.Checked)
            {
                this.UpdateRenderedObject(this.LockReference, (Control c) => { c.Enabled = true; });
                cavityCounter.SetUpTask(rampGen.periodMS);
                cavityCounter.fail = true;
                cavityCounter.ready = true;
            }
            else
            {
                this.UpdateRenderedObject(this.LockReference, (Control c) => { c.Enabled = false; });
                cavityCounter.DismissTask();
            }
        }

        private void RefVoltageFeedback_Leave(object sender, EventArgs e)
        {
            cavity.CurrentVoltage = Convert.ToDouble(RefVoltageFeedback.Text);
            this.SetTextField(this.RefVoltageFeedback, Convert.ToString(cavity.CurrentVoltage));
        }

        private void refLockLocV_Leave(object sender, EventArgs e)
        {
            cavity.LockLevel = Convert.ToDouble(refLockLocV.Text);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Gain_Leave(object sender, EventArgs e)
        {
            cavity.gain = Convert.ToDouble(Gain.Text);
        }

        public void UpdateRampPlot(ControlWindow win)
        {
            List<double> x = new List<double>();
            List<double> y = new List<double>();
            y.Add(rampGen.Offset);
            y.Add(rampGen.Offset + 0.25*rampGen.Amplitude);
            y.Add(rampGen.Offset + 0.50*rampGen.Amplitude);
            y.Add(rampGen.Offset + 0.75*rampGen.Amplitude);
            y.Add(rampGen.Offset + 1.00*rampGen.Amplitude);
            if (this.EnableData.Checked)
            {
                x.Add(cavityCounter.dataMS);
                x.Add(cavityCounter.dataMS);
                x.Add(cavityCounter.dataMS);
                x.Add(cavityCounter.dataMS);
                x.Add(cavityCounter.dataMS);
            }
            else
            {
                x.Add(0);
                x.Add(0);
                x.Add(0);
                x.Add(0);
                x.Add(0);
            }
            win.UpdateRenderedObject<NationalInstruments.UI.WindowsForms.ScatterGraph>(win.PeakPlot, (NationalInstruments.UI.WindowsForms.ScatterGraph g) => { g.Plots[1].PlotXY(x.ToArray(), y.ToArray()); });
            ((SlaveLaserControl)SlaveLasersTabs.SelectedTab.Controls[0]).UpdateRampPlot(win);
        }
    }
}
