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
            controller.window = this;
            controller.windowLoaded();
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

        private void RampFreq_TextChanged(object sender, EventArgs e)
        {
            //SetTextField(RampFreq, Convert.ToString((double)500000 / controller.rampGen.GetSamplesPerHalfPeriod(Convert.ToDouble(RampFreq.Text))));
        }

        private void RampOffset_TextChanged(object sender, EventArgs e)
        {

        }

        private void RampAmplitude_TextChanged(object sender, EventArgs e)
        {

        }

        private void StartRamp_CheckedChanged(object sender, EventArgs e)
        {
            if (StartRamp.Checked)
            {
                int halfP = controller.rampGen.SetUpTasks(Convert.ToDouble(RampAmplitude.Text), Convert.ToDouble(RampFreq.Text), Convert.ToDouble(RampOffset.Text));
                SetTextField(RampFreq, Convert.ToString((double)500000 / halfP));
                UpdateRenderedObject(RampAmplitude, (Control c) => { c.Enabled = false; });
                UpdateRenderedObject(RampFreq, (Control c) => { c.Enabled = false; });
                UpdateRenderedObject(RampOffset, (Control c) => { c.Enabled = false; });
                (new Thread(new ThreadStart(controller.Update))).Start();
            }
            else
            {
                controller.rampGen.StopTasks();
                UpdateRenderedObject(RampAmplitude, (Control c) => { c.Enabled = true; });
                UpdateRenderedObject(RampFreq, (Control c) => { c.Enabled = true; });
                UpdateRenderedObject(RampOffset, (Control c) => { c.Enabled = true; });
            }
            controller.rampGen.UpdateRampPlot(this, StartRamp.Checked);
        }

        private void RampFreq_Leave(object sender, EventArgs e)
        {
            //SetTextField(RampFreq, Convert.ToString((double)500000 / controller.rampGen.GetSamplesPerHalfPeriod(Convert.ToDouble(RampFreq.Text))));
        }


        public void UpdatePlot()
        {
            UpdateRenderedObject(CavityTabs, (TabControl c) => { ((CavityControl)c.SelectedTab.Controls[0]).UpdateRampPlot(this); });
        }

        private void ControlWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            controller.rampGen.StopTasks();
            controller.close = true;
            Thread.Sleep(100);
        }
    }
}
