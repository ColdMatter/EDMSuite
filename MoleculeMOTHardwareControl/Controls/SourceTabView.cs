using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

namespace MoleculeMOTHardwareControl.Controls
{
    public partial class SourceTabView : MoleculeMOTHardwareControl.Controls.GenericView
    {
        protected SourceTabController castController;
        double sf6flowconversion = (double)DAQ.Environment.Environs.Hardware.GetInfo("flowConversionSF6");
        double heflowconversion = (double)DAQ.Environment.Environs.Hardware.GetInfo("flowConversionHe");
        public SerialPort serialPort;
        string serialPortCOMName = "COM12";

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

        public void UpdateAnalogOutputControls(double V0, double V1)
        {
            lblAO0.Text = "(" + V0.ToString("F2") + " V )";
            lblAO1.Text = "(" + V1.ToString("F2") + " V )";
            numAO0.Value = (decimal)(V0 * sf6flowconversion);
            numAO1.Value = (decimal)(V1 * heflowconversion);
        }

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

        public void UpdateCurrentSourcePressure(string pressure)
        {
            txtSourcePressure.Text = pressure;
        }

        public void UpdateCurrentMOTPressure(string pressure)
        {
            txtMOTchamberPressure.Text = pressure;
        }

        public void UpdateCurrentSourceTemperature(string temp)
        {
            currentTemperature.Text = temp;
        }

        public void UpdateCurrentSourceTemperature40K(string temp)
        {
            txt40Ktemp.Text = temp;
        }

        public void UpdateCurrentSourceTemperature40K2(string temp)
        {
            txt40Ktemp2.Text = temp;
        }

        public void UpdateCurrentSourceTemperature2(string temp)
        {
            txtSourceTemp2.Text = temp;
        }

        public void UpdateCurrentSourceTemperature3(string temp)
        {
            txtSourceTemp3.Text = temp;
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
        public bool AutomaticFlowControlEnabled()
        {
            return chkAutoFlowControl.Checked;
        }

        public void DisableAutomaticFlowControl()
        {
            chkAutoFlowControl.Checked = false;
            chkAO0Enable.Checked = false;
            chkAO1Enable.Checked = false;
        }

        public bool AutomaticValveControlEnabled()
        {
            return chkAutoValveControl.Checked;
        }

        public void DisableAutomaticValveControl()
        {
            chkAutoValveControl.Checked = false;
            chkSF6Valve.Checked = false;
            chkHeValve.Checked = false;
        }

        public void DisableTOF()
        {
            chkToF.Checked = false;
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

        public void ValveOpen()
        {
            chkSF6Valve.Checked = true;
            chkHeValve.Checked = true;
        }

        public void ValveClose()
        {
            chkSF6Valve.Checked = false;
            chkHeValve.Checked = false;
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

        public void UpdateHoldButton40K(bool state)
        {
            holdButton40K.Text = state ? "Heat and Hold" : "Stop Holding";
        }

        public void UpdateHoldButtonSF6(bool state)
        {
            holdButtonSF6.Text = state ? "Heat and Hold" : "Stop Holding";
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

        public void SetHeaterState40K(bool state)
        {
            heaterSwitch40K.Value = state;
            heaterLED40K.Value = state;
        }

        public void SetHeaterStateSF6(bool state)
        {
            heaterSwitchSF6.Value = state;
            heaterLEDSF6.Value = state;
        }

        #endregion

        #region UI Query Handlers

        public double GetCycleLimit()
        {
            return (double)cycleLimit.Value;
        }

        public double getCycleHoldTime()
        {
            return (double)cycleHoldTime.Value;
        }

        public double GetCycleLimit40K()
        {
            return (double)cycleLimit40K.Value;
        }

        public double GetCycleLimitSF6()
        {
            return (double)cycleLimitSF6.Value;
        }

        #endregion

        #region UI Event Handlers

        private void samplingRateSelect(object sender, EventArgs e)
        {
            castController.SamplingRate = Int32.Parse(cmbSamplingRate.Text);
        }

        private void toggleReading(object sender, EventArgs e)
        {
            castController.ToggleReading();
        }

        private void toggleCycling(object sender, EventArgs e)
        {
            chkToF.Checked = false;
            chkAutoFlowControl.Checked = false;
            chkAO0Enable.Checked = false;
            chkAO1Enable.Checked = false;
            castController.ToggleCycling();
        }

        private void toggleHolding(object sender, EventArgs e)
        {
            chkToF.Checked = false;
            chkAutoFlowControl.Checked = false;
            chkAO0Enable.Checked = false;
            chkAO1Enable.Checked = false;
            castController.ToggleHolding();
        }

        private void toggleHolding40K(object sender, EventArgs e)
        {
            /*
            chkToF.Checked = false;
            chkAutoFlowControl.Checked = false;
            chkAO0Enable.Checked = false;
            chkAO1Enable.Checked = false;*/
            castController.ToggleHolding40K();
        }

        private void toggleHoldingSF6(object sender, EventArgs e)
        {
            castController.ToggleHoldingSF6();
        }

        private void toggleHeater(object sender, NationalInstruments.UI.ActionEventArgs e)
        {
            bool state = heaterSwitch.Value;
            heaterLED.Value = state;
            castController.SetHeaterState(state);
        }

        private void toggleHeater40K(object sender, NationalInstruments.UI.ActionEventArgs e)
        {
            bool state = heaterSwitch40K.Value;
            heaterLED40K.Value = state;
            castController.SetHeaterState40K(state);
        }

        private void toggleHeaterSF6(object sender, NationalInstruments.UI.ActionEventArgs e)
        {
            bool state = heaterSwitchSF6.Value;
            heaterLEDSF6.Value = state;
            castController.SetHeaterStateSF6(state);
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

        private void numAO0_ValueChanged(object sender, EventArgs e)
        {

            double Vset = (double)numAO0.Value / sf6flowconversion;
            castController.SetAnalogOutput(0, Vset);
        }

        private void numAO1_ValueChanged(object sender, EventArgs e)
        {
            double Vset = (double)numAO1.Value / heflowconversion;
            castController.SetAnalogOutput(1, Vset);
        }

        private void chkAO0Enable_CheckedChanged(object sender, EventArgs e)
        {
            castController.SwitchOutputAOVoltage(0);
        }

        private void chkAO1Enable_CheckedChanged(object sender, EventArgs e)
        {
            castController.SwitchOutputAOVoltage(1);
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

        private void numPlotMax_ValueChanged(object sender, EventArgs e)
        {
            
        }

        private void chkAutoFlowControl_CheckedChanged(object sender, EventArgs e)
        {
            if(chkAutoFlowControl.Checked)
            {
                chkAO0Enable.Enabled = false;
                chkAO1Enable.Enabled = false;
            }
            else
            {
                chkAO0Enable.Enabled = true;
                chkAO1Enable.Enabled = true;
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void cmbPlotChannel_SelectedIndexChanged(object sender, EventArgs e)
        {
            castController.SetPlotChannel(cmbPlotChannel.SelectedIndex);
        }

        private void chkAutoScale_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoScale.Checked)
                tempGraph.YAxes[0].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose;
            else
                tempGraph.YAxes[0].Mode = NationalInstruments.UI.AxisMode.Fixed;
        }

        private void chkSF6Valve_CheckedChanged(object sender, EventArgs e)
        {
            castController.ToggleDigitalOutput(2, chkSF6Valve.Checked);
        }

        private void chkHeValve_CheckedChanged(object sender, EventArgs e)
        {

            castController.ToggleDigitalOutput(3, chkHeValve.Checked);
        }

        private void numFlowTimeout_ValueChanged(object sender, EventArgs e)
        {
            castController.FlowTimeOut = (int)numFlowTimeout.Value;
        }

        #region YAG control
        private void yag_X1_P (object sender, EventArgs e)
        {
            castController.moveYagX1((int)jogSteps.Value);
        }

        private void yag_X1_N(object sender, EventArgs e)
        {
            castController.moveYagX1(-(int)jogSteps.Value);
        }

        private void yag_Y1_P(object sender, EventArgs e)
        {
            castController.moveYagY1((int)jogSteps.Value);
        }

        private void yag_Y1_N(object sender, EventArgs e)
        {
            castController.moveYagY1(-(int)jogSteps.Value);
        }

        private void yag_X2_P(object sender, EventArgs e)
        {
            castController.moveYagX2((int)jogSteps.Value);
        }

        private void yag_X2_N(object sender, EventArgs e)
        {
            castController.moveYagX2(-(int)jogSteps.Value);
        }

        private void yag_Y2_P(object sender, EventArgs e)
        {
            castController.moveYagY2((int)jogSteps.Value);
        }

        private void yag_Y2_N(object sender, EventArgs e)
        {
            castController.moveYagY2(-(int)jogSteps.Value);
        }

        private void yag_connect_click(object sender, EventArgs e)
        {
            castController.connectDevice();
        }

        #endregion

        #region Target turner

        private void connect_target_turner(object sender, EventArgs e)
        {
            if(serialPort != null)
            {
                serialPort.Close();
            }
            
            serialPort = new SerialPort(serialPortCOMName, 4800, Parity.None, 8, StopBits.One);

            // Attach a method to be called when there
            // is data waiting in the port's buffer 
            serialPort.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);

            serialPort.Open();
            serialPort.WriteLine("?feedback 0\n");
            serialPort.WriteLine("?mode 0\n");
            serialPort.WriteLine("?mode 2\n");
            serialPort.WriteLine("?feedback 0\n");
        }

        private void turn_target_CW(object sender, EventArgs e)
        {
            serialPort.WriteLine("?home\n");
            serialPort.WriteLine("?move 20\n");
        }

        private void turn_target_CCW(object sender, EventArgs e)
        {
            serialPort.WriteLine("?home\n");
            serialPort.WriteLine("?move -20\n");
        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Show all the incoming data in the port's buffer
            targetTurnerReport.Text = serialPort.ReadExisting();
        }

        #endregion
        private void tempGraph_PlotDataChanged(object sender, NationalInstruments.UI.XYPlotDataChangedEventArgs e)
        {

        }

        private void heaterGroup_Enter(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void sf6Temperature_TextChanged(object sender, EventArgs e)
        {

        }

        private void currentTemperature_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtSourceTemp2_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtSourceTemp3_TextChanged(object sender, EventArgs e)
        {

        }

        private void txt40Ktemp_TextChanged(object sender, EventArgs e)
        {

        }

        private void txt40Ktemp2_TextChanged(object sender, EventArgs e)
        {

        }

        private void SourceTabView_Load(object sender, EventArgs e)
        {

        }

        private void jogSteps_ValueChanged(object sender, EventArgs e)
        {

        }

    }
}
