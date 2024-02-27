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
//using Newport.USBComm;//rhys removed 15/02
//using NewFocus.Picomotor; //rhys removed 15/02

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

        public void SetComboBoxSelectedIndex(ComboBox combobox, int index)
        {
            combobox.Invoke(new SetComboBoxSelectedIndexDelegate(SetComboBoxSelectedIndexHelper), new object[] { combobox, index });
        }
        private delegate void SetComboBoxSelectedIndexDelegate(ComboBox combobox, int index);
        private void SetComboBoxSelectedIndexHelper(ComboBox combobox, int index)
        {
            combobox.SelectedIndex = index;
        }

        public int GetComboBoxSelectedIndex(ComboBox combobox)
        {
            return (int)combobox.Invoke(new GetComboBoxSelectedIndexDelegate(GetComboBoxSelectedIndexHelper), new object[] { combobox });
        }
        private delegate int GetComboBoxSelectedIndexDelegate(ComboBox combobox);
        private int GetComboBoxSelectedIndexHelper(ComboBox combobox)
        {
            int index = combobox.SelectedIndex;
            return index;
        }

        public int GetComboBoxTextIndex(ComboBox combobox, string str)
        {
            return (int)combobox.Invoke(new GetComboBoxTextIndexDelegate(GetComboBoxTextIndexHelper), new object[] { combobox, str });
        }
        private delegate int GetComboBoxTextIndexDelegate(ComboBox combobox, string str);
        private int GetComboBoxTextIndexHelper(ComboBox combobox, string str)
        {
            int index = combobox.FindString(str);
            return index;
        }

        public string GetComboBoxSelectedItem(ComboBox combobox)
        {
            return (string)combobox.Invoke(new GetComboBoxSelectedItemDelegate(GetComboBoxSelectedItemHelper), new object[] { combobox });
        }
        private delegate string GetComboBoxSelectedItemDelegate(ComboBox combobox);
        private string GetComboBoxSelectedItemHelper(ComboBox combobox)
        {
            string str = (string)combobox.SelectedItem; // (string) casts the returned object to a string type variable
            return str;
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

        public bool IsChartSeriesEnabled(Chart chart)
        {
            return (bool)chart.Invoke(new IsChartSeriesEnabledDelegate(IsChartSeriesEnabledHelper), new object[] { chart });
        }
        private delegate bool IsChartSeriesEnabledDelegate(Chart chart);
        private bool IsChartSeriesEnabledHelper(Chart chart)
        {
            bool enabled = false; 
            foreach (Series ser in chart.Series)
            {
                if (ser.Enabled)
                {
                    enabled = true;
                }
            }
            return enabled;
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
                SetAxisYIsStartedFromZero(chart, false);
                chart.ChartAreas[0].AxisY.MajorTickMark.Interval = 0; // A value of zero represents an "Auto" value
                chart.ChartAreas[0].AxisY.MinorTickMark.Interval = 1;
            }
            else
            {
                if (scale == "Linear")
                {
                    chart.ChartAreas[0].AxisY.IsLogarithmic = false;
                    SetAxisYIsStartedFromZero(chart, false);
                    chart.ChartAreas[0].AxisY.MajorTickMark.Interval = 0; // A value of zero represents an "Auto" value
                    chart.ChartAreas[0].AxisY.MinorTickMark.Interval = chart.ChartAreas[0].AxisY.MajorTickMark.Interval/10;
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
                    int startPoint = pointsCount - NumberOfPointsBeingDisplayed; // Number of points that we will not loop over

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

        public void SetCheckBoxCheckedStatus(CheckBox checkBox, bool checkedStatus)
        {
            checkBox.Invoke(new SetCheckBoxDelegate(SetCheckBoxHelper), new object[] { checkBox, checkedStatus });
        }
        private delegate void SetCheckBoxDelegate(CheckBox checkBox, bool checkedStatus);
        private void SetCheckBoxHelper(CheckBox checkBox, bool checkedStatus)
        {
            checkBox.Checked = checkedStatus;
        }

        public bool GetCheckBoxCheckedStatus(CheckBox checkBox)
        {
            return (bool)checkBox.Invoke(new GetCheckBoxCheckedStatusDelegate(GetCheckBoxCheckedStatusHelper), new object[] { checkBox });
        }
        private delegate bool GetCheckBoxCheckedStatusDelegate(CheckBox checkBox);
        private bool GetCheckBoxCheckedStatusHelper(CheckBox checkBox)
        {
            bool checkedStatus = checkBox.Checked;
            return checkedStatus;
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

        public void SetLED(NationalInstruments.UI.WindowsForms.Led led, bool val)
        {
            led.Invoke(new SetLedDelegate(SetLedHelper), new object[] { led, val });
        }
        private delegate void SetLedDelegate(NationalInstruments.UI.WindowsForms.Led led, bool val);
        private void SetLedHelper(NationalInstruments.UI.WindowsForms.Led led, bool val)
        {
            led.Value = val;
        }

        public void AddAlert(string alertText)
        {
            Invoke(new AddAlertDelegate(AddAlertHelper), new object[] { alertText });
        }
        private delegate void AddAlertDelegate(string alertText);
        private void AddAlertHelper(string alertText)
        {
            BackColor = System.Drawing.Color.Red;
            WindowState = FormWindowState.Minimized;
            WindowState = FormWindowState.Normal;
            BringToFront();
            tbStatus.AppendText(DateTime.Now.ToString() + " " + alertText + "\n");
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

        //Step mirrors initial parameters
        static int initialposition1 = 0;
        static int initialposition2 = 0;
        static int initialposition3 = 0;
        static int initialposition4 = 0;
        //CmdLib8742 cmdLib = new CmdLib8742(); //rhys removed 15/02
        //End-----------------------------------------------------
        //--------------------------------------------------------

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
            if (checkBoxCellTempPlot.Checked) controller.EnableChartSeries(chart2, "Cell", true);
            else controller.EnableChartSeries(chart2, "Cell", false);
        }

        private void checkBoxS1TempPlot_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxS1TempPlot.Checked) controller.EnableChartSeries(chart2, "S1", true);
            else controller.EnableChartSeries(chart2, "S1", false);
        }

        private void checkBoxS2TempPlot_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxS2TempPlot.Checked) controller.EnableChartSeries(chart2, "S2", true);
            else controller.EnableChartSeries(chart2, "S2", false);
        }

        private void checkBoxSF6TempPlot_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxSF6TempPlot.Checked) controller.EnableChartSeries(chart2, "SF6", true);
            else controller.EnableChartSeries(chart2, "SF6", false);
        }

        private void checkBoxNeonTempPlot_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxNeonTempPlot.Checked) controller.EnableChartSeries(chart2, "Neon", true);
            else controller.EnableChartSeries(chart2, "Neon", false);
        }

        private void btClearCellTempData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart2, "Cell");
        }

        private void btClearS1TempData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart2, "S1");
        }

        private void btClearS2TempData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart2, "S2");
        }

        private void btClearSF6TempData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart2, "SF6");
        }

        private void btClearNeonTempData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart2, "Neon");
        }

        private void btClearAllTempData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart2, "Cell");
            controller.ClearChartSeriesData(chart2, "S1");
            controller.ClearChartSeriesData(chart2, "S2");
            controller.ClearChartSeriesData(chart2, "SF6");
            controller.ClearChartSeriesData(chart2, "Neon");
        }

        private void checkBoxSourcePressurePlot_CheckedChanged(object sender, EventArgs e)
        {
            controller.EnableChartSeries(chart1, "Source", checkBoxSourcePressurePlot.Checked);
        }

        private void btClearSourcePressureData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart1, "Source");
        }

        private void btClearBeamlinePressureData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart1, "Beamline");
        }

        private void btClearDetectionPressureData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart1, "Detection");
        }

        private void btClearAllPressureData_Click(object sender, EventArgs e)
        {
            controller.ClearChartSeriesData(chart1, "Source");
            controller.ClearChartSeriesData(chart1, "Beamline");
            controller.ClearChartSeriesData(chart1, "Detection");
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
            controller.EnableChartSeries(chart1, "Beamline", checkBoxBeamlinePressurePlot.Checked);
        }

        private void checkBoxDetectionPressurePlot_CheckedChanged(object sender, EventArgs e)
        {
            controller.EnableChartSeries(chart1, "Detection", checkBoxDetectionPressurePlot.Checked);
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

        private void clearStatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.StatusClearStatus();
        }

        private void btUpdateRFFrequency_Click(object sender, EventArgs e)
        {
            controller.UpdateRFFrequencyUsingUIInput();
        }

        private void btIncrementRFFrequency_Click(object sender, EventArgs e)
        {
            controller.IncrementRFFrequencyUsingUIInput();
        }

        private void btUpdateMWCHAFrequency_Click(object sender, EventArgs e)
        {
            controller.UpdateMWFrequencyUsingUIInput(0);
        }

        private void btIncrementMWCHAFrequency_Click(object sender, EventArgs e)
        {
            controller.IncrementMWFrequencyUsingUIInput(0);
        }

        private void btUpdateMWCHAPower_Click(object sender, EventArgs e)
        {
            controller.UpdateMWPowerUsingUIInput(0);
        }

        private void btIncrementMWCHAPower_Click(object sender, EventArgs e)
        {
            controller.IncrementMWPowerUsingUIInput(0);
        }

        private void btUpdateMWCHBPower_Click(object sender, EventArgs e)
        {
            controller.UpdateMWPowerUsingUIInput(1);
        }

        private void btIncrementMWCHBPower_Click(object sender, EventArgs e)
        {
            controller.IncrementMWPowerUsingUIInput(1);
        }

        private void btUpdateMWCHBFrequency_Click(object sender, EventArgs e)
        {
            controller.UpdateMWFrequencyUsingUIInput(1);
        }

        private void btIncrementMWCHBFrequency_Click(object sender, EventArgs e)
        {
            controller.IncrementMWFrequencyUsingUIInput(1);
        }

        private void btQueryMWCHAFrequency_Click(object sender, EventArgs e)
        {
            controller.QueryMWFrequency(0);
        }

        private void btQueryMWCHAPower_Click(object sender, EventArgs e)
        {
            controller.QueryMWPower(0);
        }

        private void btQueryMWCHBFrequency_Click(object sender, EventArgs e)
        {
            controller.QueryMWFrequency(1);
        }

        private void btQueryMWCHBPower_Click(object sender, EventArgs e)
        {
            controller.QueryMWPower(1);
        }

        private void cbCHARFMuted_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetRFMute(0, cbCHARFMuted.Checked);
        }

        private void cbCHBRFMuted_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetRFMute(1, cbCHBRFMuted.Checked);
        }

        private void cbCHAPAPoweredOn_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetPAPower(0, cbCHAPAPoweredOn.Checked);
        }

        private void cbCHBPAPoweredOn_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetPAPower(1, cbCHBPAPoweredOn.Checked);
        }

        private void cbCHAPLLPoweredOn_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetPLLPower(0, cbCHAPLLPoweredOn.Checked);
        }

        private void cbCHBPLLPoweredOn_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetPLLPower(1, cbCHBPLLPoweredOn.Checked);
        }

        private void btCHAFRMuteInfo_Click(object sender, EventArgs e)
        {
            controller.RFMuteInfoMessage();
        }

        private void btCHBRFMuteInfo_Click(object sender, EventArgs e)
        {
            controller.RFMuteInfoMessage();
        }

        private void btCHAPAPowerOnInfo_Click(object sender, EventArgs e)
        {
            controller.PAPowerInfoMessage();
        }

        private void btCHBPAPowerOnInfo_Click(object sender, EventArgs e)
        {
            controller.PAPowerInfoMessage();
        }

        private void btCHAPLLPowerOnInfo_Click(object sender, EventArgs e)
        {
            controller.PLLPowerInfoMessage();
        }

        private void btCHBPLLPowerOnInfo_Click(object sender, EventArgs e)
        {
            controller.PLLPowerInfoMessage();
        }

        private void btQueryMWSynthTemperature_Click(object sender, EventArgs e)
        {
            controller.UpdateMWSynthTemperature();
        }

        private void btQueryRFFrequency_Click(object sender, EventArgs e)
        {
            controller.QueryRFFrequency();
        }

        private void btClearCoolDownModeStatus_Click(object sender, EventArgs e)
        {
            SetTextBox(tbCoolDownModeStatus, "");
        }

        private void btClearWarmUpModeStatus_Click(object sender, EventArgs e)
        {
            SetTextBox(tbWarmUpModeStatus, "");
        }

        private void btClearRefreshModeStatus_Click(object sender, EventArgs e)
        {
            SetTextBox(tbRefreshModeStatus, "");
        }

        private void groupBoxWindfreaksynthhd_Enter(object sender, EventArgs e)
        {

        }

        private void groupBoxMWCHA_Enter(object sender, EventArgs e)
        {

        }

        private void btUpdateMWCHAFrequencyDetection_Click(object sender, EventArgs e)
        {
            controller.UpdateMWFrequencyUsingUIInputDetection(0);
        }

        private void btIncrementMWCHAFrequencyDetection_Click(object sender, EventArgs e)
        {
            controller.IncrementMWFrequencyUsingUIInputDetection(0);
        }

        private void btUpdateMWCHAPowerDetection_Click(object sender, EventArgs e)
        {
            controller.UpdateMWPowerUsingUIInputDetection(0);
        }

        private void btIncrementMWCHAPowerDetection_Click(object sender, EventArgs e)
        {
            controller.IncrementMWPowerUsingUIInputDetection(0);
        }

        private void btUpdateMWCHBPowerDetection_Click(object sender, EventArgs e)
        {
            controller.UpdateMWPowerUsingUIInputDetection(1);
        }

        private void btIncrementMWCHBPowerDetection_Click(object sender, EventArgs e)
        {
            controller.IncrementMWPowerUsingUIInputDetection(1);
        }

        private void btUpdateMWCHBFrequencyDetection_Click(object sender, EventArgs e)
        {
            controller.UpdateMWFrequencyUsingUIInputDetection(1);
        }

        private void btIncrementMWCHBFrequencyDetection_Click(object sender, EventArgs e)
        {
            controller.IncrementMWFrequencyUsingUIInputDetection(1);
        }

        private void btQueryMWCHAFrequencyDetection_Click(object sender, EventArgs e)
        {
            controller.QueryMWFrequencyDetection(0);
        }

        private void btQueryMWCHAPowerDetection_Click(object sender, EventArgs e)
        {
            controller.QueryMWPowerDetection(0);
        }

        private void btQueryMWCHBFrequencyDetection_Click(object sender, EventArgs e)
        {
            controller.QueryMWFrequencyDetection(1);
        }

        private void btQueryMWCHBPowerDetection_Click(object sender, EventArgs e)
        {
            controller.QueryMWPowerDetection(1);
        }

        private void cbCHARFMutedDetection_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetRFMuteDetection(0, cbCHARFMutedDetection.Checked);
        }

        private void cbCHBRFMutedDetection_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetRFMuteDetection(1, cbCHBRFMutedDetection.Checked);
        }

        private void cbCHAPAPoweredOnDetection_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetPAPowerDetection(0, cbCHAPAPoweredOnDetection.Checked);
        }

        private void cbCHBPAPoweredOnDetection_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetPAPowerDetection(1, cbCHBPAPoweredOnDetection.Checked);
        }

        private void cbCHAPLLPoweredOnDetection_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetPLLPowerDetection(0, cbCHAPLLPoweredOnDetection.Checked);
        }

        private void cbCHBPLLPoweredOnDetection_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetPLLPowerDetection(1, cbCHBPLLPoweredOnDetection.Checked);
        }

        private void btQueryMWSynthTemperatureDetection_Click(object sender, EventArgs e)
        {
            controller.UpdateMWSynthTemperatureDetection();
        }

        private void eOnCheck_CheckedChanged(object sender, EventArgs e)
        {
            controller.UpdateVoltages();
        }

        private void fieldsOffButton_Click(object sender, EventArgs e)
        {
            controller.FieldsOff();
        }

        private void switchEButton_Click(object sender, EventArgs e)
        {
            controller.SwitchE();
        }

        private void changePollPeriodButton_Click(object sender, EventArgs e)
        {
            controller.UpdateIMonitorPollPeriodUsingUIValue();
        }

        private void cPlusOffTextBox_TextChanged(object sender, EventArgs e)
        {
            //controller.VoltageSet();
        }

        private void StartDegauss_Click(object sender, EventArgs e)
        {
            controller.StartDegaussPoll();
            //controller.UpdateDegaussPulse();
        }

        private void scanningBUpdateButton_Click(object sender, EventArgs e)
        {
            controller.SetScanningBVoltage();
        }

        private void scanningBZeroButton_Click(object sender, EventArgs e)
        {
            controller.SetScanningBZero();
        }

        private void scanningBFSButton_Click(object sender, EventArgs e)
        {
            controller.SetScanningBFS();
        }

        private void labelCooldownModeInfoText_Click(object sender, EventArgs e)
        {

        }

        private void btn_FindDevice_Click(object sender, EventArgs e)
        {
            //cmdLib.DiscoverDevices();
            //string DeviceKey = cmdLib.GetFirstDeviceKey();
            //Show_DeviceKey.Text = DeviceKey;
        }

        private void btn_Motor1Forward_Click(object sender, EventArgs e)
        {
            //string DeviceKey = cmdLib.GetFirstDeviceKey();
            //int Stepsize1 = Convert.ToInt32(input_Stepsize1.Text);
            //cmdLib.RelativeMove(DeviceKey, 1, Stepsize1);
            //lbl_Motor1location.Text = (initialposition1 += Stepsize1).ToString();
        }

        private void btn_Motor1Backward_Click(object sender, EventArgs e)
        {
            //string DeviceKey = cmdLib.GetFirstDeviceKey();
            //int Stepsize1 = Convert.ToInt32(input_Stepsize1.Text);
            //cmdLib.RelativeMove(DeviceKey, 1, -Stepsize1);
            //lbl_Motor1location.Text = (initialposition1 -= Stepsize1).ToString();
        }

        private void btn_Motor2Forward_Click(object sender, EventArgs e)
        {
            //string DeviceKey = cmdLib.GetFirstDeviceKey();
            //int Stepsize2 = Convert.ToInt32(input_Stepsize2.Text);
            //cmdLib.RelativeMove(DeviceKey, 2, Stepsize2);
            //lbl_Motor2location.Text = (initialposition2 += Stepsize2).ToString();
        }

        private void btn_Motor2Backward_Click(object sender, EventArgs e)
        {
            //string DeviceKey = cmdLib.GetFirstDeviceKey();
            //int Stepsize2 = Convert.ToInt32(input_Stepsize2.Text);
            //cmdLib.RelativeMove(DeviceKey, 2, -Stepsize2);
            //lbl_Motor2location.Text = (initialposition2 -= Stepsize2).ToString();
        }

        private void btn_Motor3Forward_Click(object sender, EventArgs e)
        {
            //string DeviceKey = cmdLib.GetFirstDeviceKey();
            //int Stepsize3 = Convert.ToInt32(input_Stepsize3.Text);
            //cmdLib.RelativeMove(DeviceKey, 3, Stepsize3);
            //lbl_Motor3location.Text = (initialposition3 += Stepsize3).ToString();
        }

        private void btn_Motor3Backward_Click(object sender, EventArgs e)
        {
            //string DeviceKey = cmdLib.GetFirstDeviceKey();
            //int Stepsize3 = Convert.ToInt32(input_Stepsize3.Text);
            //cmdLib.RelativeMove(DeviceKey, 3, -Stepsize3);
            //lbl_Motor3location.Text = (initialposition3 -= Stepsize3).ToString();
        }

        private void btn_Motor4Forward_Click(object sender, EventArgs e)
        {
            //string DeviceKey = cmdLib.GetFirstDeviceKey();
            //int Stepsize4 = Convert.ToInt32(input_Stepsize4.Text);
            //cmdLib.RelativeMove(DeviceKey, 4, Stepsize4);
            //lbl_Motor4location.Text = (initialposition4 += Stepsize4).ToString();
        }

        private void btn_Motor4Backward_Click(object sender, EventArgs e)
        {
            //string DeviceKey = cmdLib.GetFirstDeviceKey();
            //int Stepsize4 = Convert.ToInt32(input_Stepsize4.Text);
            //cmdLib.RelativeMove(DeviceKey, 4, -Stepsize4);
            //lbl_Motor4location.Text = (initialposition4 -= Stepsize4).ToString();
        }

        #endregion

        private void UpdateBeatFreq_Click(object sender, EventArgs e)
        {
            controller.UpdateBeatFrequencyMonitor();
        }

        private void startFreqMonitorPollButton_Click(object sender, EventArgs e)
        {
            controller.StartFreqMonitorPoll();
        }

        private void stopFreqMonitorPollButton_Click(object sender, EventArgs e)
        {
            controller.StopFreqMonitorPoll();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            controller.ClearFreqMonitorAv();
            controller.ClearFreqMonitorChart();
        }

        private void updateBCurrentMonitorButton_Click(object sender, EventArgs e)
        {
            controller.UpdateBCurrentMonitor();
        }

        private void SteppingBBoxBiasUpdateButton_Click(object sender, EventArgs e)
        {
            controller.SetSteppingBBiasVoltage();
        }

        private void parametersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.LoadParametersWithDialog();
        }

        private void parametersToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            controller.SaveParametersWithDialog();
        }

        private void btUpdateStirapRFFrequency_Click(object sender, EventArgs e)
        {
            //
        }

        private void cbCHATrigger_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetDetectionMWTrigger(0, cbCHATrigger.Checked);
        }

        private void cbCHBRFTrigger_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetPumpingMWTrigger(1, cbCHBRFTrigger.Checked);
        }

        private void cbStirapRFOn_CheckedChanged(object sender, EventArgs e)
        {
            controller.EnableGreenSynth(cbStirapRFOn.Checked);
        }

        private void cbCHBTrigger_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetDetectionMWTrigger(1, cbCHBTrigger.Checked);
        }

        private void cBRemoteMode_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetFlowControllerRemoteMode(cbHeliumFlowRemoteMode.Checked);
        }

        private void cbFlowValveOnOff_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetFlowControllerValve(cbHeFlowValveOnOff.Checked);
        }

        private void btSetNewSF6FlowSetpoint_Click(object sender, EventArgs e)
        {
            controller.SetSF6FlowSetpoint();
        }

        private void cbSF6Valve_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void bFlipCheck_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetBFlip(bFlipCheck.Checked);
        }

        private void calFlipCheck_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetCalFlip(calFlipCheck.Checked);
        }

        private void TargetStepButton_Click(object sender, EventArgs e)
        {
            controller.StepTarget();
        }

        private void eConnectCheck_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetEConnect(eConnectCheck.Checked);
        }

        private void ePolarityCheck_CheckedChanged(object sender, System.EventArgs e)
        {
            controller.SetEPolarity(ePolarityCheck.Checked);
        }

        private void eBleedCheck_CheckedChanged(object sender, System.EventArgs e)
        {
            controller.SetBleed(eBleedCheck.Checked);
        }
    }
}
