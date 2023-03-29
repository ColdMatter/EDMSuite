using NationalInstruments;
using NationalInstruments.Controls;
using NationalInstruments.Controls.Rendering;
using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;
using NationalInstruments.Visa;
using NationalInstruments.DAQmx;
using NationalInstruments.ModularInstruments.NIScope;
using NationalInstruments.ModularInstruments;
using NationalInstruments.ModularInstruments.SystemServices.DeviceServices;
using NationalInstruments.ModularInstruments.SystemServices.TimingServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace DigitalTransferCavityLock
{
    public partial class ControlWindow : Form
    {

        public Controller controller;

        public ControlWindow(Controller _controller)
        {
            InitializeComponent();
            controller = _controller;
        }

        private void ControlWindow_Load(object sender, EventArgs e)
        {
            controller.CreateRampTask();
            controller.CreateCounterTasks();
            controller.window = this;
            (new Thread(new ThreadStart(controller.Update))).Start();
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

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void LockReference_CheckedChanged(object sender, EventArgs e)
        {
            if (LockReference.Checked)
            {
                controller.reference.ArmLock(Convert.ToDouble(this.refLockLocV.Text));
                this.UpdateRenderedObject(this.LockSlave, (Control a) => { a.Enabled = true; });
                this.UpdateRenderedObject(this.RefVoltageFeedback, (Control a) => { a.Enabled = false; });
                this.UpdateRenderedObject(this.refLockLocV, (Control a) => { a.Enabled = false; });
            }
            else
            {
                controller.reference.DisarmLock();
                this.UpdateRenderedObject(this.LockSlave, (Control a) => { a.Enabled = false; });
                this.UpdateRenderedObject(this.RefVoltageFeedback, (Control a) => { a.Enabled = true; });
                this.UpdateRenderedObject(this.refLockLocV, (Control a) => { a.Enabled = true; });
            }
        }

        private void LockSlave_CheckedChanged(object sender, EventArgs e)
        {
            if (LockSlave.Checked)
            {
                controller.slave.ArmLock(Convert.ToDouble(this.slaveLockLocV.Text), Convert.ToDouble(slaveGain.Text));
                this.UpdateRenderedObject(this.LockReference, (Control a) => { a.Enabled = false; });
                this.UpdateRenderedObject(this.SlaveVoltageFeedback, (Control a) => { a.Enabled = false; });
                this.UpdateRenderedObject(this.slaveLockLocV, (Control a) => { a.Enabled = false; });
                this.UpdateRenderedObject(this.slaveGain, (Control a) => { a.Enabled = false; });
            }
            else
            {
                controller.slave.DisarmLock();
                this.UpdateRenderedObject(this.LockReference, (Control a) => { a.Enabled = true; });
                this.UpdateRenderedObject(this.SlaveVoltageFeedback, (Control a) => { a.Enabled = true; });
                this.UpdateRenderedObject(this.slaveLockLocV, (Control a) => { a.Enabled = true; });
                this.UpdateRenderedObject(this.slaveGain, (Control a) => { a.Enabled = true; });
            }
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void SlaveVoltageFeedback_TextChanged(object sender, EventArgs e)
        {
            //controller.slave.CurrentVoltage = Convert.ToDouble(RefVoltageFeedback.Text);
            //this.SetTextField(this.SlaveVoltageFeedback, Convert.ToString(controller.slave.CurrentVoltage));
        }

        private void RefVoltageFeedback_TextChanged(object sender, EventArgs e)
        {
            //controller.reference.CurrentVoltage = Convert.ToDouble(RefVoltageFeedback.Text);
            //this.SetTextField(this.RefVoltageFeedback, Convert.ToString(controller.reference.CurrentVoltage));
        }

        private void SlaveVoltageFeedback_Leave(object sender, EventArgs e)
        {
            controller.slave.CurrentVoltage = Convert.ToDouble(SlaveVoltageFeedback.Text);
            this.SetTextField(this.SlaveVoltageFeedback, Convert.ToString(controller.slave.CurrentVoltage));
        }

        private void RefVoltageFeedback_Leave(object sender, EventArgs e)
        {
            controller.reference.CurrentVoltage = Convert.ToDouble(RefVoltageFeedback.Text);
            this.SetTextField(this.RefVoltageFeedback, Convert.ToString(controller.reference.CurrentVoltage));
        }

    }
}
