using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaFBECHardwareController.Controls
{
    public partial class FlowTabView : CaFBECHardwareController.Controls.GenericView
    {
        protected FlowTabController castController;
        double sf6flowconversion = (double)DAQ.Environment.Environs.Hardware.GetInfo("flowConversionSF6");
        double heflowconversion = (double)DAQ.Environment.Environs.Hardware.GetInfo("flowConversionHe");

        public FlowTabView(FlowTabController controllerInstance) : base(controllerInstance)
        {
            InitializeComponent();
            castController = (FlowTabController)controller; // saves casting in every method
        }

        #region UI Update Handlers

        public void UpdateFlowRates(double sf6Flow, double HeFlow)
        {
            lblsf6flow.Text = sf6Flow.ToString("F3") + " sccm";
            lblheflow.Text = HeFlow.ToString("F3") + " sccm";
        }

        public bool[] GetAnalogOutputEnableStatus()
        {
            bool[] status = new bool[] { chkAO0Enable.Checked, chkAO1Enable.Checked };
            return status;
        }

        public void FlowEnable()
        {
            chkAO0Enable.Checked = true;
            chkAO1Enable.Checked = true;
        }

        public void FlowDisable()
        {
            chkAO0Enable.Checked = false;
            chkAO1Enable.Checked = false;
        }

        public void UpdateReadButton(bool state)
        {
            readButton.Text = state ? "Start Reading" : "Stop Reading";
        }

        #endregion

        #region UI Event Handlers

        private void toggleReading(object sender, EventArgs e)
        {
            castController.ToggleReading();
        }
        #endregion

        private void chkAO0Enable_CheckedChanged(object sender, EventArgs e)
        {
            castController.SwitchOutputAOVoltage(0);
        }

        private void chkAO1Enable_CheckedChanged(object sender, EventArgs e)
        {
            castController.SwitchOutputAOVoltage(1);
        }

        private void numAO0_ValueChanged(object sender, EventArgs e)
        {
            double Vset = (double)numAO0.Value / sf6flowconversion;
            castController.SetAnalogOutput(0, Vset);
            lblAO0.Text = "(" + Vset.ToString("F3") + " V)";
        }

        private void numAO1_ValueChanged(object sender, EventArgs e)
        {
            double Vset = (double)numAO1.Value / heflowconversion;
            castController.SetAnalogOutput(1, Vset);
            lblAO1.Text = "(" + Vset.ToString("F3") + " V)";
        }
    }
}
