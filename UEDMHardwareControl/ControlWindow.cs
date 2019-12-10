using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Windows.Forms.DataVisualization.Charting;

namespace UEDMHardwareControl
{
    public partial class ControlWindow : Form
    {
        #region Setup

        public UEDMController controller;

        public ControlWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, EventArgs e)
        {
            controller.WindowLoaded();
        }

        # endregion

        #region ThreadSafe Wrappers

        public void EnableControl(Control control, bool enabled)
        {
            control.Invoke(new EnableControlDelegate(EnableControlHelper), new object[] { control, enabled });
        }
        private delegate void EnableControlDelegate(Control control, bool enabled);
        private void EnableControlHelper(Control control, bool enabled)
        {
            control.Enabled = enabled;
        }

        public void SetTextBox(TextBox box, string text)
        {
            box.Invoke(new SetTextDelegate(SetTextHelper), new object[] { box, text });
        }
        private delegate void SetTextDelegate(TextBox box, string text);
        private void SetTextHelper(TextBox box, string text)
        {
            box.Text = text;
        }

        public void SetRichTextBox(RichTextBox box, string text)
        {
            box.Invoke(new SetRichTextBoxDelegate(SetRichTextBoxHelper), new object[] { box, text });
        }
        private delegate void SetRichTextBoxDelegate(RichTextBox box, string text);
        private void SetRichTextBoxHelper(RichTextBox box, string text)
        {
            box.Text = text;
        }

        /// <summary>
        /// Adds point to chart (graph) in control window. This function is called from UEDMController on each poll of a pressure gauge.
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="series"></param>
        /// <param name="xpoint"></param>
        /// <param name="ypoint"></param>
        public void AddPointToChart(Chart chart, string series, DateTime xpoint, double ypoint)
        {
            chart.Invoke(new AddPointToChartDelegate(AddPointToChartHelper), new object[] { chart, series, xpoint, ypoint });
        }
        private delegate void AddPointToChartDelegate(Chart chart, string series, DateTime xpoint, double ypoint);
        private void AddPointToChartHelper(Chart chart, string series, DateTime xpoint, double ypoint)
        {
            // All charts are for temperature or pressure - these should not be zero or negative.
            // This also has the benefit of allowing logarithmic Y scales to be used on plots, without an exception being thrown.
            if (ypoint > 0) // if ypoint is greater than zero, then add it to the plot data. Otherwise, do not add it.
            {
                chart.Series[series].Points.AddXY(xpoint, ypoint);
            }
        }

        public void ClearChartSeriesData(Chart chart, string series)
        {
            chart.Invoke(new ClearChartSeriesDataDelegate(ClearChartSeriesDataHelper), new object[] { chart, series});
        }
        private delegate void ClearChartSeriesDataDelegate(Chart chart, string series);
        private void ClearChartSeriesDataHelper(Chart chart, string series)
        {
            chart.Series[series].Points.Clear();
        }

        public void EnableChartSeries(Chart chart, string series, bool enable)
        {
            chart.Invoke(new EnableChartSeriesDelegate(EnableChartSeriesHelper), new object[] { chart, series, enable });
        }
        private delegate void EnableChartSeriesDelegate(Chart chart, string series, bool enable);
        private void EnableChartSeriesHelper(Chart chart, string series, bool enable)
        {
            chart.Series[series].Enabled = enable;
        }

        public void ChangeChartYScale(Chart chart, string scale)
        {
            chart.Invoke(new ChangeChartYScaleDelegate(ChangeChartYScaleHelper), new object[] { chart, scale });
        }
        private delegate void ChangeChartYScaleDelegate(Chart chart, string scale);
        private void ChangeChartYScaleHelper(Chart chart, string scale)
        {
            if (scale == "Log")
            {
                chart.ChartAreas[0].AxisY.IsLogarithmic = true;
            }
            else
            {
                if(scale == "Linear")
                {
                    chart.ChartAreas[0].AxisY.IsLogarithmic = false;
                }
            }
        }

        public void SetAxisYIsStartedFromZero(Chart chart, bool YesNo)
        {
            chart.Invoke(new SetAxisYIsStartedFromZeroDelegate(SetAxisYIsStartedFromZeroHelper), new object[] { chart, YesNo });
        }
        private delegate void SetAxisYIsStartedFromZeroDelegate(Chart chart, bool YesNo);
        private void SetAxisYIsStartedFromZeroHelper(Chart chart, bool YesNo)
        {
            if(YesNo) chart.ChartAreas[0].AxisY.IsStartedFromZero = true;
            else chart.ChartAreas[0].AxisY.IsStartedFromZero = false;
        }

        public void SetDateTimePickerValue(DateTimePicker dateTimePicker, DateTime dateTime)
        {
            dateTimePicker.Invoke(new SetDateTimePickerValueDelegate(SetDateTimePickerValueHelper), new object[] { dateTimePicker, dateTime });
        }
        private delegate void SetDateTimePickerValueDelegate(DateTimePicker dateTimePicker, DateTime dateTime);
        private void SetDateTimePickerValueHelper(DateTimePicker dateTimePicker, DateTime dateTime)
        {
            dateTimePicker.Value = dateTime;
        }

        public void SetCheckBox(CheckBox checkBox, bool checkedStatus)
        {
            checkBox.Invoke(new SetCheckBoxDelegate(SetCheckBoxHelper), new object[] { checkBox, checkedStatus });
        }
        private delegate void SetCheckBoxDelegate(CheckBox checkBox, bool checkedStatus);
        private void SetCheckBoxHelper(CheckBox checkBox, bool checkedStatus)
        {
            checkBox.Checked = checkedStatus;
        }

        # endregion


        #region User Interactions

        // Menu items
        /// <summary>
        /// This menu item is used to exit the application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.Exit();
        }

        /// <summary>
        /// This menu item is used to save an image of the pressure graph in its current state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripMenuItemPlotPressureChart_Click(object sender, EventArgs e)
        {
            controller.SavePlotImage(chart1);
        }

        /// <summary>
        /// This menu item is used to save an image of the temperature graph in its current state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripMenuItemPlotTemperatureChart_Click(object sender, EventArgs e)
        {
            controller.SavePlotImage(chart2);
        }

        /// <summary>
        /// This menu item is used to save an image of the neon flow graph in its current state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripMenuItemPlotNeonFlowChart_Click(object sender, EventArgs e)
        {
            controller.SavePlotImage(chart3);
        }
        
        
        
        // Pressure monitoring
        /// <summary>
        /// When the "Start Pressure Monitoring" button is clicked by the user, this will call the function in UEDMController which will periodically update the pressure monitor textboxes in the controller window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btStartPressureMonitorPoll_Click(object sender, EventArgs e)
        {
            //controller.StartPressureMonitorPoll();
        }
        private void cbLogPressureData_CheckedChanged(object sender, EventArgs e)
        {
            if (cbLogPressureData.Checked)
            {
                controller.StartLoggingPressure();
            }
            else
            {
                controller.StopLoggingPressure();
            }
        }

        // Temperature monitoring
        private void btStartTempMonitorPoll_Click(object sender, EventArgs e)
        {
            //controller.StartTempMonitorPoll();
        }

        

        private void cbTurnCryoOn_CheckedChanged(object sender, EventArgs e)
        {
            if (cbTurnCryoOn.Checked)
            {
                controller.SetCryoState(true);
            }
            else
            {
                controller.SetCryoState(false);
            }
        }

        private void ControlWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            controller.WindowClosing();
        }

        private void btStartTandPMonitoring_Click(object sender, EventArgs e)
        {
            controller.StartPTMonitorPoll();
        }

        private void btStopTandPMonitoring_Click(object sender, EventArgs e)
        {
            controller.StopPTMonitorPoll();
        }

        private void comboBoxPlot1ScaleY_SelectedIndexChanged(object sender, EventArgs e)
        {
            controller.ChangePlotYAxisScale(1);
        }

        private void comboBoxPlot2ScaleY_SelectedIndexChanged(object sender, EventArgs e)
        {
            controller.ChangePlotYAxisScale(2);
        }

        private void checkBoxCryoEnable_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBoxCryoEnable.Checked)
            {
                controller.EnableCryoDigitalControl(true);
            }
            else controller.EnableCryoDigitalControl(false);
            
        }


        private void checkBoxCellTempPlot_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBoxCellTempPlot.Checked) controller.EnableChartSeries(chart2, "Cell Temperature", true);
            else controller.EnableChartSeries(chart2, "Cell Temperature", false);
        }

        private void checkBoxS1TempPlot_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxS1TempPlot.Checked) controller.EnableChartSeries(chart2, "S1 Temperature", true);
            else controller.EnableChartSeries(chart2, "S1 Temperature", false);
        }

        private void checkBoxS2TempPlot_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxS2TempPlot.Checked) controller.EnableChartSeries(chart2, "S2 Temperature", true);
            else controller.EnableChartSeries(chart2, "S2 Temperature", false);
        }

        private void checkBoxSF6TempPlot_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxSF6TempPlot.Checked) controller.EnableChartSeries(chart2, "SF6 Temperature", true);
            else controller.EnableChartSeries(chart2, "SF6 Temperature", false);
        }

        private void checkBoxNeonTempPlot_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxNeonTempPlot.Checked) controller.EnableChartSeries(chart2, "Neon Temperature", true);
            else controller.EnableChartSeries(chart2, "Neon Temperature", false);
        }

        private void btClearCellTempData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart2, "Cell Temperature");
        }

        private void btClearS1TempData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart2, "S1 Temperature");
        }

        private void btClearS2TempData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart2, "S2 Temperature");
        }

        private void btClearSF6TempData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart2, "SF6 Temperature");
        }

        private void btClearNeonTempData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart2, "Neon Temperature");
        }

        private void btClearAllTempData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart2, "Cell Temperature");
            controller.ClearChartSeriesData(chart2, "S1 Temperature");
            controller.ClearChartSeriesData(chart2, "S2 Temperature");
            controller.ClearChartSeriesData(chart2, "SF6 Temperature");
            controller.ClearChartSeriesData(chart2, "Neon Temperature");
        }

        private void checkBoxSourcePressurePlot_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxSourcePressurePlot.Checked) controller.EnableChartSeries(chart1, "Source Pressure", true);
            else controller.EnableChartSeries(chart1, "Source Pressure", false);
        }

        private void btClearSourcePressureData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart1, "Source Pressure");
        }

        private void btClearBeamlinePressureData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart1, "Beamline Pressure");
        }

        private void btClearAllPressureData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart1, "Source Pressure");
            controller.ClearChartSeriesData(chart1, "Beamline Pressure");
        }

        private void btStartNeonFlowActMonitor_Click(object sender, EventArgs e)
        {
            controller.StartNeonFlowMonitorPoll();
        }

        private void btStopNeonFlowActMonitor_Click(object sender, EventArgs e)
        {
            controller.StopNeonFlowMonitorPoll();
        }

        private void btClearNeonFlowActPlotData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart3, "Neon Flow");
        }

        private void btSetNewNeonFlowSetpoint_Click(object sender, EventArgs e)
        {
            controller.SetNeonFlowSetpoint();
        }

        private void checkBoxEnableHeatersS2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxEnableHeatersS2.Checked) controller.EnableDigitalHeaters(2,true);
            else controller.EnableDigitalHeaters(2, false);
        }

        private void checkBoxEnableHeatersS1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxEnableHeatersS1.Checked) controller.EnableDigitalHeaters(1, true);
            else controller.EnableDigitalHeaters(1, false);
        }

        private void btUpdateHeaterControlStage2_Click(object sender, EventArgs e)
        {
            controller.UpdateStage2TemperatureSetpoint();
        }

        private void btUpdateHeaterControlStage1_Click(object sender, EventArgs e)
        {
            controller.UpdateStage1TemperatureSetpoint();
        }

        private void btStartHeaterControlStage2_Click(object sender, EventArgs e)
        {
            controller.StartStage2DigitalHeaterControl();
        }

        private void btStartHeaterControlStage1_Click(object sender, EventArgs e)
        {
            controller.StartStage1DigitalHeaterControl();
        }

        private void btStopHeaterControlStage1_Click(object sender, EventArgs e)
        {
            controller.StopStage1DigitalHeaterControl();
        }

        private void btStopHeaterControlStage2_Click(object sender, EventArgs e)
        {
            controller.StopStage2DigitalHeaterControl();
        }

        private void btHeatersTurnOffWaitStart_Click(object sender, EventArgs e)
        {
            controller.StartTurnHeatersOffWait();
        }

        private void btHeatersTurnOffWaitCancel_Click(object sender, EventArgs e)
        {
            controller.CancelTurnHeatersOffWait();
        }

        private void btStartRefreshMode_Click(object sender, EventArgs e)
        {
            controller.StartRefreshMode();
        }

        private void btCancelRefreshMode_Click(object sender, EventArgs e)
        {
            controller.CancelRefreshMode();
        }

        private void checkBoxRefreshSourceAtRoomTemperature_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxRefreshSourceAtRoomTemperature.Checked) controller.EnableRefreshModeRoomTemperature(true);
            else controller.EnableRefreshModeRoomTemperature(false);
        }

        private void dateTimePickerRefreshModeTurnHeatersOff_ValueChanged(object sender, EventArgs e)
        {
            controller.HeaterTurnOffDateTimeSpecified();
        }

        private void dateTimePickerStopHeatingAndTurnCryoOn_ValueChanged(object sender, EventArgs e)
        {
            controller.CryoTurnOnDateTimeSpecified();
        }

        private void btRefreshModeTemperatureSetpointUpdate_Click(object sender, EventArgs e)
        {
            controller.UpdateRefreshTemperature();
        }

        private void ToolStripMenuItemSaveNeonFlowDataCSV_Click(object sender, EventArgs e)
        {
            //controller.SavePlotDataToCSV(chart3,"SCCM");
        }

        private void ToolStripMenuItemSavePressurePlotDataCSV_Click(object sender, EventArgs e)
        {
            //controller.SavePlotDataToCSV(chart1, "mbar");
        }

        private void ToolStripMenuItemSaveTemperaturePlotDataCSV_Click(object sender, EventArgs e)
        {
            //controller.SavePlotDataToCSV(chart2, "Kelvin");
        }

        private void btUpdatePTPollPeriod_Click(object sender, EventArgs e)
        {
            controller.UpdatePTMonitorPollPeriod();
        }

        #endregion

        private void checkBoxMonitorPressureWhenHeating_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxMonitorPressureWhenHeating.Checked) controller.EnableMonitorPressureWhenHeating(true);
            else controller.EnableMonitorPressureWhenHeating(false);
        }

        private void btQueryLakeShore336PIDvalues_Click(object sender, EventArgs e)
        {
            controller.QueryPIDLoopValues();
        }

        private void btSetLakeShore336PIDvalues_Click(object sender, EventArgs e)
        {
            controller.SetPIDLoopValues();
        }

        private void comboBoxLakeShore336OutputsAutotune_SelectedIndexChanged(object sender, EventArgs e)
        {
            controller.AutotuneOutputSelectionChanged();
        }

        private void comboBoxLakeShore336AutotuneModes_SelectedIndexChanged(object sender, EventArgs e)
        {
            controller.AutotuneModeSelectionChanged();
        }

        private void btAutotuneLakeShore336Output_Click(object sender, EventArgs e)
        {
            controller.AutotuneLakeShore336TemperatureControl();
        }

        private void btQueryAutotuneError_Click(object sender, EventArgs e)
        {
            controller.QueryAutotuneStatus();
        }
    }
}
