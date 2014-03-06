using NationalInstruments;
using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;
using NationalInstruments.DAQmx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace VCOLock
{
    public partial class ControlWindow : Form
    {
        public Controller controller;

        public ControlWindow()
        {
            InitializeComponent();
        }

        #region update UI delegates

        private delegate void SetTextDelegate(TextBox box, string text);
        public void SetTextBox(TextBox box, string text)
        {
            box.Invoke(new SetTextDelegate(SetTextHelper), new object[] { box, text });
        }
        private void SetTextHelper(TextBox box, string text)
        {
            box.Text = text;
        }

        public void EnableControl(Control control, bool enabled)
        {
            control.Invoke(new EnableControlDelegate(EnableControlHelper), new object[] { control, enabled });
        }
        private delegate void EnableControlDelegate(Control control, bool enabled);
        private void EnableControlHelper(Control control, bool enabled)
        {
            control.Enabled = enabled;
        }

        private delegate void SetCheckDelegate(CheckBox box, bool state);
        public void SetCheckBox(CheckBox box, bool state)
        {
            box.Invoke(new SetCheckDelegate(SetCheckHelper), new object[] { box, state });
        }
        private void SetCheckHelper(CheckBox box, bool state)
        {
            box.Checked = state;
        }

        private delegate void PlotYDelegate(double[] y);
        public void PlotYAppend(Graph graph, WaveformPlot plot, double[] y)
        {
            graph.Invoke(new PlotYDelegate(plot.PlotYAppend), new Object[] { y });
        }

        #endregion

        #region event handler methods

        private void counterFreqUpdateButton_Click(object sender, EventArgs e)
        {
            controller.UpdateFrequencyCounter();
        }

        private void startPollButton_Click(object sender, EventArgs e)
        {
            controller.StartPoll();
        }

        private void stopPollButton_Click(object sender, EventArgs e)
        {
            controller.StopPoll();
        }
        #endregion
    }
}
