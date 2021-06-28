using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MoleculeMOTHardwareControl.Controls
{
    public partial class SourceTabView : MoleculeMOTHardwareControl.Controls.GenericView
    {
        protected SourceTabController castController;

        public SourceTabView(SourceTabController controllerInstance) : base(controllerInstance)
        {
            InitializeComponent();
            castController = (SourceTabController)controller; // saves casting in every method
        }

        #region UI Update Handlers

        public bool LogStatus()
        {
            return chkLog.Checked;
        }

        public bool SaveTraceStatus()
        {
            return chkSaveTrace.Checked;
        }

        public void UpdateCurrentSourcePressure(string pressure)
        {
            txtSourcePressure.Text = pressure;
        }

        public void UpdateCurrentSourceTemperature(string temp)
        {
            currentTemperature.Text = temp;
        }

        public void UpdateCurrentSourceTemperature2(string temp)
        {
            txtSourceTemp2.Text = temp;
        }

        public void UpdateCurrentSF6Temperature(string temp)
        {
            sf6Temperature.Text = temp;
        }

        public void UpdateGraph(double time, double temp)
        {
            tempGraph.PlotXYAppend(time, temp);
        }
        
        public void UpdateGraph(double[] x, double[] y)
        {
            tempGraph.PlotXY(x, y);
        }

        public bool ToFEnabled()
        {
            return chkToF.Checked;
        }

        public void UpdateReadButton(bool state)
        {
            readButton.Text = state ? "Start Reading" : "Stop Reading";
        }

        public void UpdateCycleButton(bool state)
        {
            cycleButton.Text = state ? "Cycle Source" : "Stop Cycling";
            holdButton.Enabled = state;
        }

        public void UpdateHoldButton(bool state)
        {
            holdButton.Text = state ? "Hold Source" : "Stop Holding";
            cycleButton.Enabled = state;
        }

        public void EnableControls(bool state)
        {
            heaterSwitch.Enabled = state;
            cryoSwitch.Enabled = state;
            cycleButton.Enabled = state;
            holdButton.Enabled = state;
        }

        public void SetCryoState(bool state)
        {
            cryoSwitch.Value = state;
            cryoLED.Value = state;
        }

        public void SetHeaterState(bool state)
        {
            heaterSwitch.Value = state;
            heaterLED.Value = state;
        }

        #endregion

        #region UI Query Handlers

        public double GetCycleLimit()
        {
            return (double)cycleLimit.Value;
        }

        #endregion

        #region UI Event Handlers

        private void toggleReading(object sender, EventArgs e)
        {
            castController.ToggleReading();
        }

        private void toggleCycling(object sender, EventArgs e)
        {
            chkToF.Checked = false;
            castController.ToggleCycling();
        }

        private void toggleHolding(object sender, EventArgs e)
        {
            chkToF.Checked = false;
            castController.ToggleHolding();
        }

        private void toggleHeater(object sender, NationalInstruments.UI.ActionEventArgs e)
        {
            bool state = heaterSwitch.Value;
            heaterLED.Value = state;
            castController.SetHeaterState(state);
        }

        private void toggleCryo(object sender, NationalInstruments.UI.ActionEventArgs e)
        {
            bool state = cryoSwitch.Value;
            cryoLED.Value = state;
            castController.SetCryoState(state);
        }

        #endregion

        private void tableLayoutPanel3_Paint(object sender, PaintEventArgs e)
        {

        }
 
    }
}
