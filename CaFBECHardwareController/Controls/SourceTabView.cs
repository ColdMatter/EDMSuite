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
    public partial class SourceTabView : CaFBECHardwareController.Controls.GenericView
    {
        protected SourceTabController castController;

        double sf6flowconversion = (double)DAQ.Environment.Environs.Hardware.GetInfo("flowConversionSF6");
        double heflowconversion = (double)DAQ.Environment.Environs.Hardware.GetInfo("flowConversionHe");

        public SourceTabView(SourceTabController controllerInstance) : base(controllerInstance)
        {
            InitializeComponent();
            castController = (SourceTabController)controller; // saves casting in every method
        }

        #region UI Update Handlers

        public bool SaveTraceStatus()
        {
            return chkSaveTrace.Checked;
        }

        public bool SaveTraceStatusAbs()
        {
            return chkSaveTraceAbs.Checked;
        }

        public void UpdateGraphPMT(double[] x, double[] y)
        {
            graphPMT.PlotXY(x, y);
        }

        public void UpdateGraphAbs(double[] x, double[] y)
        {
            graphAbs.PlotXY(x, y);
        }

        public bool ToFEnabled()
        {
            return chkToF.Checked;
        }

        public bool ToFEnabledAbs()
        {
            return chkToFAbs.Checked;
        }

        public void DisableTOF()
        {
            chkToF.Checked = false;
        }

        public void UpdateReadButton(bool state)
        {
            readButton.Text = state ? "Start Reading" : "Stop Reading";
        }

        //public void UpdateFlowRates(double sf6Flow, double HeFlow)
        //{
        //    lblsf6flow.Text = sf6Flow.ToString("F3") + " sccm";
        //    lblheflow.Text = HeFlow.ToString("F3") + " sccm";
        //}

        //public bool[] GetAnalogOutputEnableStatus()
        //{
        //    bool[] status = new bool[] { chkAO0Enable.Checked, chkAO1Enable.Checked };
        //    return status;
        //}

        //public void FlowEnable()
        //{
        //    chkAO0Enable.Checked = true;
        //    chkAO1Enable.Checked = true;
        //}

        //public void FlowDisable()
        //{
        //    chkAO0Enable.Checked = false;
        //    chkAO1Enable.Checked = false;
        //}

        #endregion

        #region UI Event Handlers

        private void samplingRateSelect(object sender, EventArgs e)
        {
            castController.SamplingRate = Int32.Parse(cmbSamplingRate.Text);
        }

        private void samplingRateAbsSelect(object sender, EventArgs e)
        {
            castController.SamplingRateAbs = Int32.Parse(cmbSamplingRateAbs.Text);
        }

        private void toggleReading(object sender, EventArgs e)
        {
            castController.ToggleReading();
        }

        private void chkToF_CheckedChanged(object sender, EventArgs e)
        {
            if (chkToF.Checked)
                chkSaveTrace.Enabled = true;
            else
            { 
                chkSaveTrace.Enabled = false;
                chkSaveTrace.Checked = false;
            }
        }

        private void chkToFAbs_CheckedChanged(object sender, EventArgs e)
        {
            if (chkToFAbs.Checked)
                chkSaveTraceAbs.Enabled = true;
            else
            {
                chkSaveTraceAbs.Enabled = false;
                chkSaveTraceAbs.Checked = false;
            }
        }

        private void chkAutoScale_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoScale.Checked)
            {
                graphPMT.YAxes[0].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose;
                graphPMT.XAxes[0].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose;
            }
            else
            {
                graphPMT.YAxes[0].Mode = NationalInstruments.UI.AxisMode.Fixed;
                graphPMT.XAxes[0].Mode = NationalInstruments.UI.AxisMode.Fixed;
            }
        }

        private void chkAutoScaleAbs_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoScaleAbs.Checked)
            {
                graphAbs.YAxes[0].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose;
                graphAbs.XAxes[0].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose;
            }
            else
            {
                graphAbs.YAxes[0].Mode = NationalInstruments.UI.AxisMode.Fixed;
                graphAbs.XAxes[0].Mode = NationalInstruments.UI.AxisMode.Fixed;
            }
        }

        //private void chkAO0Enable_CheckedChanged(object sender, EventArgs e)
        //{
        //    castController.SwitchOutputAOVoltage(0);
        //}

        //private void chkAO1Enable_CheckedChanged(object sender, EventArgs e)
        //{
        //    castController.SwitchOutputAOVoltage(1);
        //}

        //private void numAO0_ValueChanged(object sender, EventArgs e)
        //{
        //    double Vset = (double)numAO0.Value / sf6flowconversion;
        //    castController.SetAnalogOutput(0, Vset);
        //    lblAO0.Text = "(" + Vset.ToString("F3") + " V)";
        //}

        //private void numAO1_ValueChanged(object sender, EventArgs e)
        //{
        //    double Vset = (double)numAO1.Value / heflowconversion;
        //    castController.SetAnalogOutput(1, Vset);
        //    lblAO1.Text = "(" + Vset.ToString("F3") + " V)";
        //}

        #endregion
    }
}
