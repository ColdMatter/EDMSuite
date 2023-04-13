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

namespace LatticeEDMHardwareControl
{
    public partial class ControlWindow : Form
    {
        #region Setup

        public LatticeEDMController controller;

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

        public void AppendTextBox(TextBox box, string text)
        {
            box.Invoke(new AppendTextBoxDelegate(AppendTextBoxHelper), new object[] { box, text });
        }
        private delegate void AppendTextBoxDelegate(TextBox box, string text);
        private void AppendTextBoxHelper(TextBox box, string text)
        {
            box.AppendText(text);
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

        public void AddPointToIChart(Chart chart, string series, DateTime xpoint, double ypoint)
        {
            chart.Invoke(new AddPointToIChartDelegate(AddPointToIChartHelper), new object[] { chart, series, xpoint, ypoint });
        }
        private delegate void AddPointToIChartDelegate(Chart chart, string series, DateTime xpoint, double ypoint);
        private void AddPointToIChartHelper(Chart chart, string series, DateTime xpoint, double ypoint)
        {
                chart.Series[series].Points.AddXY(xpoint, ypoint);
        }

        public void ClearChartSeriesData(Chart chart, string series)
        {
            chart.Invoke(new ClearChartSeriesDataDelegate(ClearChartSeriesDataHelper), new object[] { chart, series });
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
                if (scale == "Linear")
                {
                    chart.ChartAreas[0].AxisY.IsLogarithmic = false;
                }
            }
        }

        public void UpdateChartYScaleWhenXAxisRolling(Chart chart, int NumberOfPointsBeingDisplayed)
        {
            chart.Invoke(new UpdateChartYScaleWhenXAxisRollingDelegate(UpdateChartYScaleWhenXAxisRollingHelper), new object[] { chart, NumberOfPointsBeingDisplayed });
        }
        private delegate void UpdateChartYScaleWhenXAxisRollingDelegate(Chart chart, int NumberOfPointsBeingDisplayed);
        private void UpdateChartYScaleWhenXAxisRollingHelper(Chart chart, int NumberOfPointsBeingDisplayed)
        {
            double max = Double.MinValue;
            double min = Double.MaxValue;

            foreach (Series ser in chart.Series)
            {
                if (chart.Series[ser.Name].Enabled)
                {
                    int pointsCount = chart.Series[ser.Name].Points.Count; // Number of points in the series
                    double YValue;
                    int startPoint = pointsCount - NumberOfPointsBeingDisplayed;

                    if (startPoint < 0) startPoint = 0;

                    for (int ii = startPoint; ii < pointsCount; ii++)
                    {
                        YValue = chart.Series[ser.Name].Points[ii].YValues[0];
                        min = Math.Min(min, YValue);
                        max = Math.Max(max, YValue);
                    }
                }
            }

            // If the series data are cleared, then the min/max need to be defined until the next data point is added to the series:
            if (min == Double.MinValue)
            {
                min = 0;
            }
            if (max == Double.MaxValue)
            {
                max = 1;
            }

            Axis ay = chart.ChartAreas[0].AxisY;
            
            ay.Maximum = max;
            ay.Minimum = min;
            if (chart.Name == "chart1")
            {
                ay.LabelStyle.Format = "0.##E+0";
            }
            else
            {
                if (chart.Name == "chart2")
                {
                    ay.LabelStyle.Format = "#.####";
                }
            }
        }

        public void SetChartXAxisMinDateTime(Chart chart, DateTime xmin)
        {
            chart.Invoke(new SetChartXAxisMinDateTimeDelegate(SetChartXAxisMinDateTimeHelper), new object[] { chart, xmin });
        }
        private delegate void SetChartXAxisMinDateTimeDelegate(Chart chart, DateTime xmin);
        private void SetChartXAxisMinDateTimeHelper(Chart chart, DateTime xmin)
        {
            Axis xaxis = chart.ChartAreas[0].AxisX;
            xaxis.Minimum = xmin.ToOADate();
        }

        public void SetChartXAxisMinAuto(Chart chart)
        {
            chart.Invoke(new SetChartXAxisMinAutoDelegate(SetChartXAxisMinAutoHelper), new object[] { chart });
        }
        private delegate void SetChartXAxisMinAutoDelegate(Chart chart);
        private void SetChartXAxisMinAutoHelper(Chart chart)
        {
            Axis xaxis = chart.ChartAreas[0].AxisX;
            xaxis.Minimum = Double.NaN; 
            chart.ChartAreas[0].RecalculateAxesScale();
        }

        public void SetChartYAxisAuto(Chart chart)
        {
            chart.Invoke(new SetChartYAxisAutoDelegate(SetChartYAxisAutoHelper), new object[] { chart });
        }
        private delegate void SetChartYAxisAutoDelegate(Chart chart);
        private void SetChartYAxisAutoHelper(Chart chart)
        {
            Axis yaxis = chart.ChartAreas[0].AxisY;
            yaxis.Minimum = Double.NaN;
            yaxis.Maximum = Double.NaN; 
            chart.ChartAreas[0].RecalculateAxesScale();
        }

        public void SetAxisYIsStartedFromZero(Chart chart, bool YesNo)
        {
            chart.Invoke(new SetAxisYIsStartedFromZeroDelegate(SetAxisYIsStartedFromZeroHelper), new object[] { chart, YesNo });
        }
        private delegate void SetAxisYIsStartedFromZeroDelegate(Chart chart, bool YesNo);
        private void SetAxisYIsStartedFromZeroHelper(Chart chart, bool YesNo)
        {
            if (YesNo) chart.ChartAreas[0].AxisY.IsStartedFromZero = true;
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

        public void SetChartMovingAverage(Chart chart, int NumberOfPointsToAverage)
        {
            chart.Invoke(new SetChartMovingAverageDelegate(SetChartMovingAverageHelper), new object[] { chart, NumberOfPointsToAverage });
        }
        private delegate void SetChartMovingAverageDelegate(Chart chart, int NumberOfPointsToAverage);
        private void SetChartMovingAverageHelper(Chart chart, int NumberOfPointsToAverage)
        {
            foreach (Series series in chart.Series)
            {
                if (series.Points.Count > NumberOfPointsToAverage)
                {
                    chart.DataManipulator.FinancialFormula(FinancialFormula.MovingAverage, NumberOfPointsToAverage.ToString(), series.Name + ":Y", series.Name + ":Y");
                }
            }
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

        private void checkBoxCellTempPlot_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxCellTempPlot.Checked) controller.EnableChartSeries(chart2, "Cell Temperature", true);
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
            controller.EnableChartSeries(chart1, "Source Pressure", checkBoxSourcePressurePlot.Checked);
        }

        private void btClearSourcePressureData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart1, "Source Pressure");
        }

        private void btClearBeamlinePressureData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart1, "Beamline Pressure");
        }

        private void btClearDetectionPressureData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart1, "Detection Pressure");
        }

        private void btClearAllPressureData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart1, "Source Pressure");
            controller.ClearChartSeriesData(chart1, "Beamline Pressure");
            controller.ClearChartSeriesData(chart1, "Detection Pressure");
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
            if (checkBoxEnableHeatersS2.Checked) controller.EnableDigitalHeaters(2, true);
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
            controller.StartStage2HeaterControl();
        }

        private void btStartHeaterControlStage1_Click(object sender, EventArgs e)
        {
            controller.StartStage1HeaterControl();
        }

        private void btStopHeaterControlStage1_Click(object sender, EventArgs e)
        {
            controller.StopStage1HeaterControl();
        }

        private void btStopHeaterControlStage2_Click(object sender, EventArgs e)
        {
            controller.StopStage2HeaterControl();
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
            controller.RefreshModeSetWindowsAPIShutdownHandle(this.Handle);
            controller.StartRefreshMode();
        }

        private void btCancelRefreshMode_Click(object sender, EventArgs e)
        {
            controller.CancelRefreshMode();
        }

        private void dateTimePickerRefreshModeTurnHeatersOff_ValueChanged(object sender, EventArgs e)
        {
            controller.RefreshModeHeaterTurnOffDateTimeSpecified();
        }

        private void dateTimePickerStopHeatingAndTurnCryoOn_ValueChanged(object sender, EventArgs e)
        {
            controller.RefreshModeCryoTurnOnDateTimeSpecified();
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
            controller.UpdatePTMonitorPollPeriodUsingUIValue();
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

        private void btWarmUpModeTemperatureSetpointUpdate_Click(object sender, EventArgs e)
        {
            controller.UpdateWarmUpTemperature();
        }

        private void btStartWarmUpMode_Click(object sender, EventArgs e)
        {
            controller.WarmupModeSetWindowsAPIShutdownHandle(this.Handle);
            controller.StartWarmUpMode();
        }

        private void btCancelWarmUpMode_Click(object sender, EventArgs e)
        {
            controller.CancelWarmUpMode();
        }

        private void btCoolDownModeTemperatureSetpointUpdate_Click(object sender, EventArgs e)
        {
            controller.UpdateCoolDownTemperature();
        }

        private void dateTimePickerCoolDownModeTurnHeatersOff_ValueChanged(object sender, EventArgs e)
        {
            controller.CoolDownModeHeaterTurnOffDateTimeSpecified();
        }

        private void dateTimePickerCoolDownModeTurnCryoOn_ValueChanged(object sender, EventArgs e)
        {
            controller.CoolDownModeCryoTurnOnDateTimeSpecified();
        }

        private void dateTimePickerWarmUpModeTurnHeatersOff_ValueChanged(object sender, EventArgs e)
        {
            controller.WarmUpModeHeaterTurnOffDateTimeSpecified();
        }

        private void btStartCoolDownMode_Click(object sender, EventArgs e)
        {
            controller.CoolDownModeSetWindowsAPIShutdownHandle(this.Handle);
            controller.StartCoolDownMode();
        }

        private void btCancelCoolDownMode_Click(object sender, EventArgs e)
        {
            controller.CancelCoolDownMode();
        }

        private void cbEnableTemperatureChartRollingTimeAxis_CheckedChanged(object sender, EventArgs e)
        {
            controller.EnableTemperatureChartRollingTimeAxis(cbEnableTemperatureChartRollingTimeAxis.Checked);
        }

        private void btRollingTemperatureChartTimeAxis_Click(object sender, EventArgs e)
        {
            controller.UpdateTemperatureChartRollingPeriodUsingUIInput();
        }

        private void cbEnablePressureChartRollingTimeAxis_CheckedChanged(object sender, EventArgs e)
        {
            controller.EnablePressureChartRollingTimeAxis(cbEnablePressureChartRollingTimeAxis.Checked);
        }

        private void btRollingPressureChartTimeAxis_Click(object sender, EventArgs e)
        {
            controller.UpdatePressureChartRollingPeriod();
        }

        private void btGaugesCorrectionFactors_Click(object sender, EventArgs e)
        {
            controller.UpdateGaugesCorrectionFactorsUsingUIInputs();
        }

        private void tbSourceGaugeCorrectionFactor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                controller.UpdateGaugesCorrectionFactorsUsingUIInputs();
            }
        }

        private void tbBeamlineGaugeCorrectionFactor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                controller.UpdateGaugesCorrectionFactorsUsingUIInputs();
            }
        }

        private void tbDetectionGaugeCorrectionFactor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                controller.UpdateGaugesCorrectionFactorsUsingUIInputs();
            }
        }

        private void tbTandPPollPeriod_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                controller.UpdatePTMonitorPollPeriodUsingUIValue();
            }
        }

        private void tbRollingPressureChartTimeAxisPeriod_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                controller.UpdatePressureChartRollingPeriod();
            }
        }

        private void tbRollingTemperatureChartTimeAxisPeriod_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                controller.UpdateTemperatureChartRollingPeriodUsingUIInput();
            }
        }

        private void tbNewNeonFlowSetPoint_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                controller.SetNeonFlowSetpoint();
            }
        }

        private void cbDigitalOutputP00_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetDigitalOutput("Port00", cbDigitalOutputP00.Checked);
        }

        private void cbDigitalOutputP01_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetDigitalOutput("Port01", cbDigitalOutputP01.Checked);
        }

        private void cbDigitalOutputP02_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetDigitalOutput("Port02", cbDigitalOutputP02.Checked);
        }

        private void cbDigitalOutputP03_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetDigitalOutput("Port03", cbDigitalOutputP03.Checked);
        }

        private void comboBoxAnalogueInputsChartScaleY_SelectedIndexChanged(object sender, EventArgs e)
        {
            controller.ChangePlotYAxisScale(4);
        }

        private void btUpdateAnalogueMonitoringPollPeriod_Click(object sender, EventArgs e)
        {
            controller.UpdateAIMonitorPollPeriod();
        }

        private void btStartMonitoringAnalogueInputs_Click(object sender, EventArgs e)
        {
            controller.StartAnalogueInputsMonitorPoll();
        }

        private void btStopMonitoringAnalogueInputs_Click(object sender, EventArgs e)
        {
            controller.StopAnalogueInputsMonitorPoll();
        }

        private void cbPlotAnalogueInputAI11_CheckedChanged(object sender, EventArgs e)
        {
            if (comboBoxAI11Conversion.Text == "None")
            {
                controller.EnableChartSeries(chart4, "AI11", cbPlotAnalogueInputAI11.Checked);
            }
            else
            {
                controller.EnableChartSeries(chart4, "AI11 Converted", cbPlotAnalogueInputAI11.Checked);
            }
        }

        private void cbPlotAnalogueInputAI12_CheckedChanged(object sender, EventArgs e)
        {
            if (comboBoxAI12Conversion.Text == "None")
            {
                controller.EnableChartSeries(chart4, "AI12", cbPlotAnalogueInputAI12.Checked);
            }
            else
            {
                controller.EnableChartSeries(chart4, "AI12 Converted", cbPlotAnalogueInputAI12.Checked);
            }
        }

        private void cbPlotAnalogueInputAI13_CheckedChanged(object sender, EventArgs e)
        {
            if (comboBoxAI13Conversion.Text == "None")
            {
                controller.EnableChartSeries(chart4, "AI13", cbPlotAnalogueInputAI13.Checked);
            }
            else
            {
                controller.EnableChartSeries(chart4, "AI13 Converted", cbPlotAnalogueInputAI13.Checked);
            }
        }

        private void cbPlotAnalogueInputAI14_CheckedChanged(object sender, EventArgs e)
        {
            if (comboBoxAI14Conversion.Text == "None")
            {
                controller.EnableChartSeries(chart4, "AI14", cbPlotAnalogueInputAI14.Checked);
            }
            else
            {
                controller.EnableChartSeries(chart4, "AI14 Converted", cbPlotAnalogueInputAI14.Checked);
            }
        }

        private void cbPlotAnalogueInputAI15_CheckedChanged(object sender, EventArgs e)
        {
            if (comboBoxAI15Conversion.Text == "None")
            {
                controller.EnableChartSeries(chart4, "AI15", cbPlotAnalogueInputAI15.Checked);
            }
            else
            {
                controller.EnableChartSeries(chart4, "AI15 Converted", cbPlotAnalogueInputAI15.Checked);
            }
        }

        private void btClearAI11SeriesData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart4, "AI11");
            controller.ClearChartSeriesData(chart4, "AI11 Converted");
        }

        private void btClearAI12SeriesData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart4, "AI12");
            controller.ClearChartSeriesData(chart4, "AI12 Converted");
        }

        private void btClearAI13SeriesData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart4, "AI13");
            controller.ClearChartSeriesData(chart4, "AI13 Converted");
        }

        private void btClearAI14SeriesData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart4, "AI14");
            controller.ClearChartSeriesData(chart4, "AI14 Converted");
        }

        private void btClearAI15SeriesData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart4, "AI15");
            controller.ClearChartSeriesData(chart4, "AI15 Converted");
        }

        private void btClearAllAnalogueInputData_Click(object sender, EventArgs e)
        {
            controller.ClearAllAIChartSeries();
        }

        private void btUpdateAnalogueInputsChartRollingAxisPeriod_Click(object sender, EventArgs e)
        {
            controller.UpdateAIChartRollingPeriod();
        }

        private void cbEnableAnalogueInputsChartRollingTimeAxis_CheckedChanged(object sender, EventArgs e)
        {
            controller.EnableAIChartRollingTimeAxis(cbEnableAnalogueInputsChartRollingTimeAxis.Checked);
        }

        private void btSaveAICSVData_Click(object sender, EventArgs e)
        {
            controller.SaveAnalogueInputsDataToCSV();
        }

        private void comboBoxAI11Conversion_SelectedIndexChanged(object sender, EventArgs e)
        {
            controller.EnableConvertedAISeries("AI11", comboBoxAI11Conversion.Text);
        }

        private void comboBoxAI12Conversion_SelectedIndexChanged(object sender, EventArgs e)
        {
            controller.EnableConvertedAISeries("AI12", comboBoxAI12Conversion.Text);
        }

        private void comboBoxAI13Conversion_SelectedIndexChanged(object sender, EventArgs e)
        {
            controller.EnableConvertedAISeries("AI13", comboBoxAI13Conversion.Text);
        }

        private void comboBoxAI14Conversion_SelectedIndexChanged(object sender, EventArgs e)
        {
            controller.EnableConvertedAISeries("AI14", comboBoxAI14Conversion.Text);
        }

        private void comboBoxAI15Conversion_SelectedIndexChanged(object sender, EventArgs e)
        {
            controller.EnableConvertedAISeries("AI15", comboBoxAI15Conversion.Text);
        }

        private void checkBoxBeamlinePressurePlot_CheckedChanged(object sender, EventArgs e)
        {
            controller.EnableChartSeries(chart1, "Beamline Pressure", checkBoxBeamlinePressurePlot.Checked);
        }

        private void checkBoxDetectionPressurePlot_CheckedChanged(object sender, EventArgs e)
        {
            controller.EnableChartSeries(chart1, "Detection Pressure", checkBoxDetectionPressurePlot.Checked);
        }

        private void btResetPTCSVData_Click(object sender, EventArgs e)
        {
            controller.ResetPTCSVData();
        }

        private void btSaveAllPTDataToCSV_Click(object sender, EventArgs e)
        {
            controller.SavePTDataToCSV();
        }

        private void pressureAndTemperatureDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.SavePTDataToCSV();
        }

        private void pressueAndTemperatureImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var PTcharts = new List<Chart> { chart1, chart2 };
            controller.SaveMultipleChartImages(PTcharts);
        }

        private void chart1_MouseEnter(object sender, EventArgs e)
        {
            if (!chart1.Focused)
                chart1.Focus();
        }

        private void chart1_MouseLeave(object sender, EventArgs e)
        {
            if (chart1.Focused)
                chart1.Parent.Focus();
        }

        private void labelTemperatureRollingTimeAxisPeriod_Click(object sender, EventArgs e)
        {

        }

        private void btRefreshModeOptions_Click(object sender, EventArgs e)
        {
            controller.LoadRefreshModeOptionsDialog();
        }

        private void ButtonWarmUpModeOptions_Click(object sender, EventArgs e)
        {
            controller.LoadWarmupModeOptionsDialog();
        }

        private void ButtonCoolDownModeOptions_Click(object sender, EventArgs e)
        {
            controller.LoadCooldownModeOptionsDialog();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void updateIMonitorButton_Click(object sender, EventArgs e)
        {
            controller.UpdateIMonitor();
        }

        private void clearIMonitorButton_Click(object sender, EventArgs e)
        {
            controller.ClearIMonitorAv();
            controller.ClearIMonitorChart();
        }

        private void zeroIMonitorButton_Click(object sender, EventArgs e)
        {
            controller.CalibrateIMonitors();
        }

        private void startIMonitorPollButton_Click(object sender, EventArgs e)
        {
            controller.StartIMonitorPoll();
        }

        private void stopIMonitorPollButton_Click(object sender, EventArgs e)
        {
            controller.StopIMonitorPoll();
        }

        private void updateVMonitorButton_Click(object sender, EventArgs e)
        {
            controller.UpdateVMonitorUI();
        }

        private void rescaleIMonitorChartButton_Click(object sender, EventArgs e)
        {
            SetChartYAxisAuto(chart5);
        }

        private void updateFieldButton_Click(object sender, EventArgs e)
        {
            controller.UpdateVoltages();
        }

        private void btResetGaugesCorrectionFactors_Click(object sender, EventArgs e)
        {
            controller.ResetGaugesCorrectionFactors();
        }

        private void eOnCheck_CheckedChanged(object sender, EventArgs e)
        {
            controller.UpdateVoltages();
        }

    }
}
