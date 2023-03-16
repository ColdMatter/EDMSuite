using NationalInstruments;
using NationalInstruments.Analysis;
using NationalInstruments.Analysis.Conversion;
using NationalInstruments.Analysis.Dsp;
using NationalInstruments.Analysis.Dsp.Filters;
using NationalInstruments.Analysis.Math;
using NationalInstruments.Analysis.Monitoring;
using NationalInstruments.Analysis.SignalGeneration;
using NationalInstruments.Analysis.SpectralMeasurements;
using NationalInstruments.Controls;
using NationalInstruments.Controls.Rendering;
using NationalInstruments.NetworkVariable;
using NationalInstruments.NetworkVariable.WindowsForms;
using NationalInstruments.Tdms;
using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;
using NationalInstruments.Visa;
using NationalInstruments.DAQmx;
using NationalInstruments.ModularInstruments.NIScope;
using NationalInstruments.ModularInstruments;
using NationalInstruments.ModularInstruments.SystemServices.TimingServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using DAQ.Environment;
using DAQ.HAL;
using System.Windows.Forms.DataVisualization.Charting;


namespace AlFHardwareControl
{
    public partial class AlFControlWindow : Form
    {

        public const double TYPE_K_SHUTOFF = 30;
        public const string CRYO_SHUTOFF = "1e-4";

        public AlFController controller;
        public Dictionary<string, DataGrapher> graphers = new Dictionary<string, DataGrapher> { };
        public TaskScheduler tSched;

        public AlFControlWindow(AlFController _controller)
        {
            InitializeComponent();

            controller = _controller;

            controller.UpdateLakeshoreNames();
            controller.UpdateLeyboldNames();
            this.LabelA.Text = controller.nameA;
            this.LabelB.Text = controller.nameB;
            this.LabelC.Text = controller.nameC;
            this.LabelD.Text = controller.nameD;

            this.P1Name.Text = controller.pressure1Name;
            this.P2Name.Text = controller.pressure2Name;

            AddTemperature();
            AddPressure();
            AddTypeK();
            AddTaskScheduler();

        }

        private void AddSafetyInterlocks()
        {
            // Heater Shutoff
            Func<bool> Loop1On = () => { return Loop1Status.Text == "ON"; };
            Func<bool> Loop2On = () => { return Loop2Status.Text == "ON"; };
            Func<bool> heaterOn = () => { return Loop1On() || Loop2On(); };
            tSched.AddEvent(new SafetyInterlock(tSched, LabelA.Text + " Temperature", ">", Convert.ToString(TYPE_K_SHUTOFF + 273.15), "Turn off heaters", heaterOn));
            tSched.AddEvent(new SafetyInterlock(tSched, LabelB.Text + " Temperature", ">", Convert.ToString(TYPE_K_SHUTOFF + 273.15), "Turn off heaters", heaterOn));
            tSched.AddEvent(new SafetyInterlock(tSched, LabelC.Text + " Temperature", ">", Convert.ToString(TYPE_K_SHUTOFF + 273.15), "Turn off heaters", heaterOn));
            tSched.AddEvent(new SafetyInterlock(tSched, LabelD.Text + " Temperature", ">", Convert.ToString(TYPE_K_SHUTOFF + 273.15), "Turn off heaters", heaterOn));

            tSched.AddEvent(new SafetyInterlock(tSched, "Type-K Loop 1", ">", Convert.ToString(TYPE_K_SHUTOFF), "Turn off Loop 1", Loop1On));
            tSched.AddEvent(new SafetyInterlock(tSched, "Type-K Loop 2", ">", Convert.ToString(TYPE_K_SHUTOFF), "Turn off Loop 2", Loop1On));

        }

        private void AddTaskScheduler()
        {
            TabPage taskScheduler = new TabPage("Task Scheduler");
            TaskScheduler tScheduler = new TaskScheduler();

            #region Resources
            tScheduler.AddResource(P1Name.Text +" Pressure", () =>
            {
                return controller.pressure1;
            });

            tScheduler.AddResource(P2Name.Text + " Pressure", () =>
            {
                return controller.pressure3;
            });

            //tScheduler.AddResource(P3Name.Text + " Pressure", () =>
            //{
            //    return controller.pressure1;
            //});

            tScheduler.AddResource("Type-K Loop 1", () =>
            {
                return Convert.ToString(controller.loop1PV);
            });

            tScheduler.AddResource("Type-K Loop 2", () =>
            {
                return Convert.ToString(controller.loop2PV);
            });

            tScheduler.AddResource(this.LabelA.Text + " Temperature", () =>
            {
                return TempA.Text.Trim(new char[] { 'K', ' ', '+' });
            });

            tScheduler.AddResource(this.LabelB.Text + " Temperature", () =>
            {
                return TempB.Text.Trim(new char[] { 'K', ' ', '+' });
            });

            tScheduler.AddResource(this.LabelC.Text + " Temperature", () =>
            {
                return TempC.Text.Trim(new char[] { 'K', ' ', '+' });
            });

            tScheduler.AddResource(this.LabelD.Text + " Temperature", () =>
            {
                return TempD.Text.Trim(new char[] { 'K', ' ', '+' });
            });
            #endregion

            #region Tasks
            tScheduler.AddTask("Turn on cryo", (bool discard) =>
            {
                if (this.CryoStatus.Text == "ON")
                {
                    tScheduler.UpdateEventLog("Cryo already engaged!");
                    return;
                }
                if (Loop1Status.Text == "ON" || Loop2Status.Text == "ON")
                {
                    tScheduler.UpdateEventLog("Can't turn on cryo while heaters are actve!");
                    return;
                }
                if (Convert.ToDouble(controller.pressure1) > Convert.ToDouble(CRYO_SHUTOFF))
                {
                    // Add new event once implemented
                    tScheduler.UpdateEventLog("Pressure of " + controller.pressure1 + " mbar is too high to engage cryo.");
                    if (!discard)
                    {
                        tScheduler.AddEvent(new ResourceEvent(tScheduler, "Src Pressure", "<", CRYO_SHUTOFF, "Turn on cryo", false));
                        tScheduler.UpdateEventLog("Rescheduling \"Turn on cryo\" for when Src Pressure < 1e-4");
                    }
                    return;
                }
                this.EngageCryo_Click(null, new EventArgs());

            });

            tScheduler.AddTask("Turn off cryo", (bool discard) =>
            {
                if (this.CryoStatus.Text == "OFF")
                {
                    tScheduler.UpdateEventLog("Cryo is already OFF!");
                    return;
                }
                this.DisengageCryo_Click(null, new EventArgs());
            });

            tScheduler.AddTask("Turn on Loop 1", (bool discard) =>
            {
                if (controller.loop1PV > TYPE_K_SHUTOFF)
                {
                    tScheduler.UpdateEventLog("Type-K temperature of " + controller.loop1PV + " C is too high to turn on heater!");
                    if (!discard)
                        tScheduler.UpdateEventLog("No reason to create an event in case the temperature goes below the maximum allowed!");
                    return;
                }
                if (this.CryoStatus.Text == "ON")
                {
                    tScheduler.UpdateEventLog("Can't turn on Loop 1 while Cryo is ON!");
                    return;
                }
                if (Loop1Status.Text == "ON")
                {
                    tScheduler.UpdateEventLog("Loop 1 is already ON!");
                    return;
                }
                this.Loop1Engage_Click(null, new EventArgs());
            });

            tScheduler.AddTask("Turn on Loop 2", (bool discard) =>
            {
                if (controller.loop2PV > TYPE_K_SHUTOFF)
                {
                    tScheduler.UpdateEventLog("Type-K temperature of " + controller.loop2PV + " C is too high to turn on heater!");
                    if (!discard)
                        tScheduler.UpdateEventLog("No reason to create an event in case the temperature goes below the maximum allowed!");
                    return;
                }
                if (this.CryoStatus.Text == "ON")
                {
                    tScheduler.UpdateEventLog("Can't turn on Loop 2 while Cryo is ON!");
                    return;
                }
                if (Loop2Status.Text == "ON")
                {
                    tScheduler.UpdateEventLog("Loop 2 is already ON!");
                    return;
                }
                this.Loop2Engage_Click(null, new EventArgs());
            });

            tScheduler.AddTask("Turn off Loop 1", (bool discard) =>
            {
                if (Loop1Status.Text == "OFF")
                {
                    tScheduler.UpdateEventLog("Loop 1 is already OFF!");
                    return;
                }
                this.Loop1Disengage_Click(null, new EventArgs());
            });

            tScheduler.AddTask("Turn off Loop 2", (bool discard) =>
            {
                if (Loop2Status.Text == "OFF")
                {
                    tScheduler.UpdateEventLog("Loop 2 is already OFF!");
                    return;
                }
                this.Loop2Disengage_Click(null, new EventArgs());
            });

            tScheduler.AddTask("Turn on heaters", (bool discard) =>
            {
                if (this.CryoStatus.Text == "ON")
                {
                    tScheduler.UpdateEventLog("Can't turn on heaters while Cryo is ON!");
                    return;
                }
                if (controller.loop1PV > TYPE_K_SHUTOFF)
                {
                    tScheduler.UpdateEventLog("Type-K temperature of " + controller.loop1PV + " C is too high to turn on heater!");
                    if (!discard)
                        tScheduler.UpdateEventLog("No reason to create an event in case the temperature goes below the maximum allowed!");
                    goto Loop2;
                }
                if (Loop1Status.Text == "ON")
                {
                    tScheduler.UpdateEventLog("Loop 1 is already ON!");
                    goto Loop2;
                }
                this.Loop1Engage_Click(null, new EventArgs());

                Loop2:
                if (controller.loop2PV > TYPE_K_SHUTOFF)
                {
                    tScheduler.UpdateEventLog("Type-K temperature of " + controller.loop2PV + " C is too high to turn on heater!");
                    if (!discard)
                        tScheduler.UpdateEventLog("No reason to create an event in case the temperature goes below the maximum allowed!");
                    return;
                }
                if (Loop2Status.Text == "ON")
                {
                    tScheduler.UpdateEventLog("Loop 2 is already ON!");
                    return;
                }
                this.Loop2Engage_Click(null, new EventArgs());
            });

            tScheduler.AddTask("Turn off heaters", (bool discard) =>
            {
                if (Loop1Status.Text == "OFF")
                {
                    tScheduler.UpdateEventLog("Loop 1 is already OFF!");
                    goto Loop2;
                }
                this.Loop1Disengage_Click(null, new EventArgs());

                Loop2:
                if (Loop2Status.Text == "OFF")
                {
                    tScheduler.UpdateEventLog("Loop 2 is already OFF!");
                    return;
                }
                this.Loop2Disengage_Click(null, new EventArgs());
            });
            #endregion

            taskScheduler.Controls.Add(tScheduler);
            MainTabs.TabPages.Add(taskScheduler);
            tSched = tScheduler;
        }

        private void AddTemperature()
        {
            TabPage temp = new TabPage("Temperature");
            DataGrapher tempGrapher = new DataGrapher("Temperature", "Temperature [K]", (DataGrapher grapher) =>
            {

                DateTime localDate = DateTime.Now;
                string tempA = this.TempA.Text.Trim(new char[] { 'K', ' ', '+' });
                string tempB = this.TempB.Text.Trim(new char[] { 'K', ' ', '+' });
                string tempC = this.TempC.Text.Trim(new char[] { 'K', ' ', '+' });
                string tempD = this.TempD.Text.Trim(new char[] { 'K', ' ', '+' });
                grapher.UpdateRenderedObject<Chart>(grapher.DataGraph, (Chart obj) => { if (Convert.ToDouble(tempA) != 0) obj.Series[this.LabelA.Text].Points.AddXY(localDate, tempA); });
                grapher.UpdateRenderedObject<Chart>(grapher.DataGraph, (Chart obj) => { if (Convert.ToDouble(tempB) != 0) obj.Series[this.LabelB.Text].Points.AddXY(localDate, tempB); });
                grapher.UpdateRenderedObject<Chart>(grapher.DataGraph, (Chart obj) => { if (Convert.ToDouble(tempC) != 0) obj.Series[this.LabelC.Text].Points.AddXY(localDate, tempC); });
                grapher.UpdateRenderedObject<Chart>(grapher.DataGraph, (Chart obj) => { if (Convert.ToDouble(tempD) != 0) obj.Series[this.LabelD.Text].Points.AddXY(localDate, tempD); });


            });
            tempGrapher.DataGraph.Series.Add(this.LabelA.Text).ChartType = SeriesChartType.Line;
            tempGrapher.DataGraph.Series.Add(this.LabelB.Text).ChartType = SeriesChartType.Line;
            tempGrapher.DataGraph.Series.Add(this.LabelC.Text).ChartType = SeriesChartType.Line;
            tempGrapher.DataGraph.Series.Add(this.LabelD.Text).ChartType = SeriesChartType.Line;

            foreach (Series series in tempGrapher.DataGraph.Series)
            {
                series.XValueType = ChartValueType.DateTime;
            }

            graphers.Add("Temperature", tempGrapher);
            temp.Controls.Add(tempGrapher);
            MainTabs.TabPages.Add(temp);
        }

        private void AddTypeK()
        {
            TabPage typeK = new TabPage("Type-K Temperature");
            DataGrapher typeKGrapher = new DataGrapher("Type-K Temperature", "Temperature [C]", (DataGrapher grapher) =>
            {

                DateTime localDate = DateTime.Now;

                grapher.UpdateRenderedObject<Chart>(grapher.DataGraph, (Chart obj) => { obj.Series["Loop1"].Points.AddXY(localDate, controller.loop1PV); });
                grapher.UpdateRenderedObject<Chart>(grapher.DataGraph, (Chart obj) => { obj.Series["Loop2"].Points.AddXY(localDate, controller.loop2PV); });


            });
            typeKGrapher.DataGraph.Series.Add("Loop1").ChartType = SeriesChartType.Line;
            typeKGrapher.DataGraph.Series.Add("Loop2").ChartType = SeriesChartType.Line;

            foreach (Series series in typeKGrapher.DataGraph.Series)
            {
                series.XValueType = ChartValueType.DateTime;
            }

            graphers.Add("Type-K Temperature", typeKGrapher);
            typeKGrapher.TempDataSaveLoc.Text = "C:\\Users\\alfultra\\OneDrive - Imperial College London\\Tyoe-K_logs\\Default.csv";
            typeK.Controls.Add(typeKGrapher);
            MainTabs.TabPages.Add(typeK);
        }

        private void AddPressure()
        {
            TabPage pressure = new TabPage("Pressure");
            DataGrapher pressureGrapher = new DataGrapher("Pressure", "Pressure [mbar]", (DataGrapher grapher) =>
            {

                DateTime localDate = DateTime.Now;

                grapher.UpdateRenderedObject<Chart>(grapher.DataGraph, (Chart obj) => { if (Convert.ToDouble(controller.pressure1) != 0) obj.Series[controller.pressure1Name].Points.AddXY(localDate, controller.pressure1); });
                grapher.UpdateRenderedObject<Chart>(grapher.DataGraph, (Chart obj) => { if (Convert.ToDouble(controller.pressure2) != 0) obj.Series[controller.pressure2Name].Points.AddXY(localDate, controller.pressure2); });


            });
            pressureGrapher.DataGraph.Series.Add(controller.pressure1Name).ChartType = SeriesChartType.Line;
            pressureGrapher.DataGraph.Series.Add(controller.pressure2Name).ChartType = SeriesChartType.Line;

            foreach (Series series in pressureGrapher.DataGraph.Series)
            {
                series.XValueType = ChartValueType.DateTime;
            }

            graphers.Add("Pressure", pressureGrapher);
            pressureGrapher.TempDataSaveLoc.Text = "C:\\Users\\alfultra\\OneDrive - Imperial College London\\Pressure_logs\\Default.csv";
            pressure.Controls.Add(pressureGrapher);
            MainTabs.TabPages.Add(pressure);
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

        public void UpdateRenderedObject<T>(T obj, Action<T> updateFunc) where T : Control
        {
            obj.Invoke(new UpdateObjectDelegate<T>(UpdateObject), new object[] { obj, updateFunc });
        }

        private delegate void UpdateObjectDelegate<T>(T obj, Action<T> updateFunc) where T : Control;

        private void UpdateObject<T>(T obj, Action<T> updateFunc) where T : Control
        {
            updateFunc(obj);
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void legend1_ItemsChanged(object sender, CollectionChangeEventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void TemperatureLayout_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click_1(object sender, EventArgs e)
        {

        }

        private void TempA_Click(object sender, EventArgs e)
        {

        }

        private void tempB_Click(object sender, EventArgs e)
        {

        }

        private void AlFControlWindow_Load(object sender, EventArgs e)
        {
            AddSafetyInterlocks();
            controller.WindowLoaded();
        }

        private void label1_Click_2(object sender, EventArgs e)
        {

        }

        private void tempGraph_Click(object sender, EventArgs e)
        {

        }

        private void EngageCryo_Click(object sender, EventArgs e)
        {
            if (Convert.ToDouble(controller.pressure1) > Convert.ToDouble(CRYO_SHUTOFF))
            {
                return;
            }
            UpdateRenderedObject(this.EngageCryo, (Button but) => { but.Enabled = false; });
            /*
            UpdateRenderedObject(this.DisengageCryo, (Button but) => { but.Enabled = true; });
            UpdateRenderedObject(this.CryoStatus, (Label lab) => { lab.Text = "ON"; });
            */
            lock (controller.lakeshore)
            {
                controller.lakeshore.SetRelayParameters(1, 1);
            }
        }

        private void DisengageCryo_Click(object sender, EventArgs e)
        {
            /*
            UpdateRenderedObject(this.DisengageCryo, (Button but) => { but.Enabled = false; });
            UpdateRenderedObject(this.EngageCryo, (Button but) => { but.Enabled = true; });
            UpdateRenderedObject(this.CryoStatus, (Label lab) => { lab.Text = "OFF"; });
            */
            lock (controller.lakeshore)
            {
                controller.lakeshore.SetRelayParameters(1, 0);
            }
        }

        private void Loop1Engage_Click(object sender, EventArgs e)
        {
            if(controller.loop1PV > TYPE_K_SHUTOFF) return;
            UpdateRenderedObject(this.Loop1Engage, (Button but) => { but.Enabled = false; });
            lock (controller.eurotherm)
            {
                controller.eurotherm.SetAMSwitch(0, false);
            }
        }

        private void Loop1Disengage_Click(object sender, EventArgs e)
        {
            UpdateRenderedObject(this.Loop1Disengage, (Button but) => { but.Enabled = false; });
            lock (controller.eurotherm)
            {
                controller.eurotherm.SetAMSwitch(0, true);
                controller.eurotherm.SetManOut(0, 0);
            }
        }

        private void Loop2Engage_Click(object sender, EventArgs e)
        {
            if (controller.loop2PV > TYPE_K_SHUTOFF) return;
            UpdateRenderedObject(this.Loop2Engage, (Button but) => { but.Enabled = false; });
            lock (controller.eurotherm)
            {
                controller.eurotherm.SetAMSwitch(1, false);
            }
        }

        private void Loop2Disengage_Click(object sender, EventArgs e)
        {
            UpdateRenderedObject(this.Loop2Disengage, (Button but) => { but.Enabled = false; });
            lock (controller.eurotherm)
            {
                controller.eurotherm.SetAMSwitch(1, true);
                controller.eurotherm.SetManOut(1, 0);
            }
        }
    }
}
