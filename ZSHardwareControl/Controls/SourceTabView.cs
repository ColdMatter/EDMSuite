using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZeemanSisyphusHardwareControl.Controls
{
    public partial class SourceTabView : ZeemanSisyphusHardwareControl.Controls.GenericView
    {
        protected SourceTabController castController;

        public SourceTabView(SourceTabController controllerInstance) : base(controllerInstance)
        {
            InitializeComponent();
            castController = (SourceTabController)controller; // saves casting in every method
        }

        #region UI Update Handlers

        public void UpdateCurrentTemperature(string temp, bool lowTemp)
        {
            currentTemperature.Text = temp;
            rbThermRT.Checked = !(lowTemp);
            rbThermLT.Checked = lowTemp;
        }
        public void UpdateCurrentPressure(string[] pressures)
        {
            currentPressureNear.Text = pressures[0];
            currentPressureFar.Text = pressures[1];
        }
        public void UpdateCurrentSF6Temperature(string temp)
        {
            currentSF6Temperature.Text = temp;
        }
        public void UpdateCurrent40KTemperature(string temp)
        {
            current40KTemperature.Text = temp;
        }
        public void UpdateGraph(double time, double temp)
        {
            tempGraph.PlotXYAppend(time, temp);
        }

        public void UpdateReadButton(bool state)
        {
            readButton.Text = state ? "Start Reading" : "Stop Reading";
        }

        public void UpdateRecordButton(bool state)
        {
            recordButton.Text = state ? "Start Recording" : "Stop Recording";
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
            recordButton.Enabled = state;
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

        public double GetReferenceResistance()
        {
            return ((double)numRref.Value)*1000;
        }

        #endregion

        #region UI Event Handlers

        private void toggleReading(object sender, EventArgs e)
        {
            castController.ToggleReading();
        }

        private void toggleRecording(object sender, EventArgs e)
        {
            castController.ToggleRecording();
        }
        private void toggleCycling(object sender, EventArgs e)
        {
            castController.ToggleCycling();
        }

        private void toggleHolding(object sender, EventArgs e)
        {
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

        private void tableLayoutPanel6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void currentPressure_TextChanged(object sender, EventArgs e)
        {

        }

        private void cycleLimit_ValueChanged(object sender, EventArgs e)
        {

        }

        private void numRref_ValueChanged(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void holdButton_Click(object sender, EventArgs e)
        {

        }

        private void currentSF6Temperature_TextChanged(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }
 
    }
}
