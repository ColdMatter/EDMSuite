using NationalInstruments.DAQmx;
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
        public MOTMasterStuff mmStuff;

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
            this.P3Name.Text = controller.pressure3Name;

            AddTemperature();
            AddPressure();
            AddTypeK();
            AddBake();
            AddTaskScheduler();
            AddMiscInstruments();
            AddMMStuff();

        }

        private void AddMiscInstruments()
        {
            TabPage temp = new TabPage("Misc Instruments");
            MiscInstruments misc = new MiscInstruments();
            misc.mSquaredLaserView1.FallbackActive = true;
            controller.MiscDataUpdate += (object a, EventArgs args) => { misc.YAG_Control.UpdateStatus(); };
            controller.MiscDataUpdate += (object a, EventArgs args) => { misc.mSquaredLaserView1.UpdateStatus(); };
                temp.Controls.Add(misc);
            MainTabs.TabPages.Add(temp);
        }

        private void AddSafetyInterlocks()
        {
            // Heater Shutoff
            Func<bool> Loop1On = () => { return Loop1Status.Text == "ON" && controller.interlocksActive; };
            Func<bool> Loop2On = () => { return Loop2Status.Text == "ON" && controller.interlocksActive; };
            Func<bool> heaterOn = () => { return Loop1On() || Loop2On(); };



            tSched.AddEvent(new SafetyInterlock(tSched, LabelA.Text + " Temperature", ">", Convert.ToString(TYPE_K_SHUTOFF + 273.15), "Turn off heaters", heaterOn));
            tSched.AddEvent(new SafetyInterlock(tSched, LabelB.Text + " Temperature", ">", Convert.ToString(TYPE_K_SHUTOFF + 273.15), "Turn off heaters", heaterOn));
            tSched.AddEvent(new SafetyInterlock(tSched, LabelC.Text + " Temperature", ">", Convert.ToString(TYPE_K_SHUTOFF + 273.15), "Turn off heaters", heaterOn));
            tSched.AddEvent(new SafetyInterlock(tSched, LabelD.Text + " Temperature", ">", Convert.ToString(TYPE_K_SHUTOFF + 273.15), "Turn off heaters", heaterOn));

            tSched.AddEvent(new SafetyInterlock(tSched, "Type-K Loop 1", ">", Convert.ToString(TYPE_K_SHUTOFF), "Turn off Loop 1", Loop1On));
            tSched.AddEvent(new SafetyInterlock(tSched, "Type-K Loop 2", ">", Convert.ToString(TYPE_K_SHUTOFF), "Turn off Loop 2", Loop2On));
            tSched.AddEvent(new SafetyInterlock(tSched, "Cryo state", "is", "ON", "Turn off heaters", heaterOn));

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

            tScheduler.AddResource(P3Name.Text + " Pressure", () =>
            {
                return controller.pressure3;
            });

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

            tScheduler.AddResource("Cryo state", () =>
            {
                return this.CryoStatus.Text;
            });
            #endregion

            #region Tasks
            tScheduler.AddTask("Turn on cryo", (bool discard) =>
            {
                if (this.CryoStatus.Text == "ON")
                {
                    tScheduler.UpdateEventLog("Cryo already engaged!");
                    return null;
                }
                if ((Loop1Status.Text == "ON" || Loop2Status.Text == "ON") && controller.InterlocksActive)
                {
                    tScheduler.UpdateEventLog("Can't turn on cryo while heaters are actve!");
                    return null; // Fix
                }
                if (Convert.ToDouble(controller.pressure1) > Convert.ToDouble(CRYO_SHUTOFF) && controller.InterlocksActive)
                {
                    // Add new event once implemented
                    tScheduler.UpdateEventLog("Pressure of " + controller.pressure1 + " mbar is too high to engage cryo.");
                    if (!discard)
                    {
                        tScheduler.AddEvent(new ResourceEvent(tScheduler, P1Name.Text + " Pressure", "<", CRYO_SHUTOFF, "Turn on cryo", false));
                        tScheduler.UpdateEventLog("Rescheduling \"Turn on cryo\" for when Src Pressure < 1e-4");
                        return null;
                    }
                    return new ResourceCondition(P1Name.Text + " Pressure", " < ", CRYO_SHUTOFF);
                }
                this.EngageCryo_Click(null, new EventArgs());
                return null;
            });

            tScheduler.AddTask("Turn off cryo", (bool discard) =>
            {
                if (this.CryoStatus.Text == "OFF")
                {
                    tScheduler.UpdateEventLog("Cryo is already OFF!");
                    return null;
                }
                this.DisengageCryo_Click(null, new EventArgs());
                return null;
            });

            tScheduler.AddTask("Turn on Loop 1", (bool discard) =>
            {
                if (controller.loop1PV > TYPE_K_SHUTOFF && controller.InterlocksActive)
                {
                    tScheduler.UpdateEventLog("Type-K temperature of " + controller.loop1PV + " C is too high to turn on heater!");
                    if (!discard)
                        tScheduler.UpdateEventLog("No reason to create an event in case the temperature goes below the maximum allowed!");
                    return null;
                }
                if (this.CryoStatus.Text == "ON" && controller.InterlocksActive)
                {
                    tScheduler.UpdateEventLog("Can't turn on Loop 1 while Cryo is ON!");
                    return new ResourceCondition("Cryo state", "is", "OFF");
                }
                if (Loop1Status.Text == "ON")
                {
                    tScheduler.UpdateEventLog("Loop 1 is already ON!");
                    return null;
                }
                this.Loop1Engage_Click(null, new EventArgs());
                return null;
            });

            tScheduler.AddTask("Turn on Loop 2", (bool discard) =>
            {
                if (controller.loop2PV > TYPE_K_SHUTOFF && controller.InterlocksActive)
                {
                    tScheduler.UpdateEventLog("Type-K temperature of " + controller.loop2PV + " C is too high to turn on heater!");
                    if (!discard)
                        tScheduler.UpdateEventLog("No reason to create an event in case the temperature goes below the maximum allowed!");
                    return null;
                }
                if (this.CryoStatus.Text == "ON" && controller.InterlocksActive)
                {
                    tScheduler.UpdateEventLog("Can't turn on Loop 2 while Cryo is ON!");
                    return new ResourceCondition("Cryo state", "is", "OFF");
                }
                if (Loop2Status.Text == "ON")
                {
                    tScheduler.UpdateEventLog("Loop 2 is already ON!");
                    return null;
                }
                this.Loop2Engage_Click(null, new EventArgs());
                return null;
            });

            tScheduler.AddTask("Turn off Loop 1", (bool discard) =>
            {
                if (Loop1Status.Text == "OFF")
                {
                    tScheduler.UpdateEventLog("Loop 1 is already OFF!");
                    return null;
                }
                this.Loop1Disengage_Click(null, new EventArgs());
                return null;
            });

            tScheduler.AddTask("Turn off Loop 2", (bool discard) =>
            {
                if (Loop2Status.Text == "OFF")
                {
                    tScheduler.UpdateEventLog("Loop 2 is already OFF!");
                    return null;
                }
                this.Loop2Disengage_Click(null, new EventArgs());
                return null;
            });

            tScheduler.AddTask("Turn on heaters", (bool discard) =>
            {
                if (this.CryoStatus.Text == "ON" && controller.InterlocksActive)
                {
                    tScheduler.UpdateEventLog("Can't turn on heaters while Cryo is ON!");
                    return new ResourceCondition("Cryo state", "is", "OFF");
                }
                if (controller.loop1PV > TYPE_K_SHUTOFF && controller.InterlocksActive)
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
                if (controller.loop2PV > TYPE_K_SHUTOFF && controller.InterlocksActive)
                {
                    tScheduler.UpdateEventLog("Type-K temperature of " + controller.loop2PV + " C is too high to turn on heater!");
                    if (!discard)
                        tScheduler.UpdateEventLog("No reason to create an event in case the temperature goes below the maximum allowed!");
                    return null;
                }
                if (Loop2Status.Text == "ON")
                {
                    tScheduler.UpdateEventLog("Loop 2 is already ON!");
                    return null;
                }
                this.Loop2Engage_Click(null, new EventArgs());
                return null;
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
                    return null;
                }
                this.Loop2Disengage_Click(null, new EventArgs());
                return null;
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
                grapher.UpdateRenderedObject<Chart>(grapher.DataGraph, (Chart obj) => { obj.Series[this.LabelA.Text].Points.AddXY(localDate, tempA); });
                grapher.UpdateRenderedObject<Chart>(grapher.DataGraph, (Chart obj) => { obj.Series[this.LabelB.Text].Points.AddXY(localDate, tempB); });
                grapher.UpdateRenderedObject<Chart>(grapher.DataGraph, (Chart obj) => { obj.Series[this.LabelC.Text].Points.AddXY(localDate, tempC); });
                grapher.UpdateRenderedObject<Chart>(grapher.DataGraph, (Chart obj) => { obj.Series[this.LabelD.Text].Points.AddXY(localDate, tempD); });


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

            tempGrapher.SetupDataDisplay();
            tempGrapher.unit = "K";
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

            typeKGrapher.SetupDataDisplay();
            typeKGrapher.unit = "C";
        }

        private void AddPressure()
        {
            TabPage pressure = new TabPage("Pressure");
            DataGrapher pressureGrapher = new DataGrapher("Pressure", "Pressure [mbar]", (DataGrapher grapher) =>
            {

                DateTime localDate = DateTime.Now;

                grapher.UpdateRenderedObject<Chart>(grapher.DataGraph, (Chart obj) => { obj.Series[controller.pressure1Name].Points.AddXY(localDate, controller.pressure1); });
                grapher.UpdateRenderedObject<Chart>(grapher.DataGraph, (Chart obj) => { obj.Series[controller.pressure2Name].Points.AddXY(localDate, controller.pressure2); });
                grapher.UpdateRenderedObject<Chart>(grapher.DataGraph, (Chart obj) => { obj.Series[controller.pressure3Name].Points.AddXY(localDate, controller.pressure3); });


            });
            pressureGrapher.DataGraph.Series.Add(controller.pressure1Name).ChartType = SeriesChartType.Line;
            pressureGrapher.DataGraph.Series.Add(controller.pressure2Name).ChartType = SeriesChartType.Line;
            pressureGrapher.DataGraph.Series.Add(controller.pressure3Name).ChartType = SeriesChartType.Line;

            foreach (Series series in pressureGrapher.DataGraph.Series)
            {
                series.XValueType = ChartValueType.DateTime;
            }

            graphers.Add("Pressure", pressureGrapher);
            pressureGrapher.TempDataSaveLoc.Text = "C:\\Users\\alfultra\\OneDrive - Imperial College London\\Pressure_logs\\Default.csv";
            pressure.Controls.Add(pressureGrapher);
            MainTabs.TabPages.Add(pressure);

            pressureGrapher.SetupDataDisplay();
            pressureGrapher.unit = "mbar";
        }

        private Task bakeTask;
        private AnalogMultiChannelReader bakeReader;

        private void AddBake()
        {
            try
            {
                bakeTask = new Task("bakeTask");
                bakeTask.AIChannels.CreateThermocoupleChannel("/Dev1/ai0", "Dispenser", 0, 300, AIThermocoupleType.K, AITemperatureUnits.DegreesC);
                bakeTask.AIChannels.CreateThermocoupleChannel("/Dev1/ai1", "Top", 0, 300, AIThermocoupleType.K, AITemperatureUnits.DegreesC);
                bakeTask.AIChannels.CreateThermocoupleChannel("/Dev1/ai2", "Bottom", 0, 300, AIThermocoupleType.K, AITemperatureUnits.DegreesC);
                bakeTask.AIChannels.CreateThermocoupleChannel("/Dev1/ai3", "Window", 0, 300, AIThermocoupleType.K, AITemperatureUnits.DegreesC);

                bakeTask.Timing.ConfigureSampleClock("", 2, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples);
                bakeTask.Timing.SamplesPerChannel = 2;

                bakeReader = new AnalogMultiChannelReader(bakeTask.Stream);
            } catch (NationalInstruments.DAQmx.DaqException)
            {
                
            }
            TabPage pressure = new TabPage("Baking temperature");
            DataGrapher pressureGrapher = new DataGrapher("Temperature", "Temperature [C]", (DataGrapher grapher) =>
            {

                DateTime localDate = DateTime.Now;
                try
                {
                    if (bakeReader == null)
                    {
                        bakeTask = new Task("bakeTask");
                        bakeTask.AIChannels.CreateThermocoupleChannel("/Dev1/ai0", "Dispenser", 0, 300, AIThermocoupleType.K, AITemperatureUnits.DegreesC);
                        bakeTask.AIChannels.CreateThermocoupleChannel("/Dev1/ai1", "Top", 0, 300, AIThermocoupleType.K, AITemperatureUnits.DegreesC);
                        bakeTask.AIChannels.CreateThermocoupleChannel("/Dev1/ai2", "Bottom", 0, 300, AIThermocoupleType.K, AITemperatureUnits.DegreesC);
                        bakeTask.AIChannels.CreateThermocoupleChannel("/Dev1/ai3", "Window", 0, 300, AIThermocoupleType.K, AITemperatureUnits.DegreesC);

                        bakeTask.Timing.ConfigureSampleClock("", 2, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples);
                        bakeTask.Timing.SamplesPerChannel = 2;

                        bakeReader = new AnalogMultiChannelReader(bakeTask.Stream);
                    }
                    double[] res = bakeReader.ReadSingleSample();

                    grapher.UpdateRenderedObject<Chart>(grapher.DataGraph, (Chart obj) => { obj.Series["Dispenser"].Points.AddXY(localDate, res[0]); });
                    grapher.UpdateRenderedObject<Chart>(grapher.DataGraph, (Chart obj) => { obj.Series["Top"].Points.AddXY(localDate, res[1]); });
                    grapher.UpdateRenderedObject<Chart>(grapher.DataGraph, (Chart obj) => { obj.Series["Bottom"].Points.AddXY(localDate, res[2]); });
                    grapher.UpdateRenderedObject<Chart>(grapher.DataGraph, (Chart obj) => { obj.Series["Window"].Points.AddXY(localDate, res[3]); });
                }
                catch (NationalInstruments.DAQmx.DaqException)
                {
                    grapher.Invoke((System.Action)(()=> { grapher.takeData.Checked = false; }));
                }

            });
            pressureGrapher.DataGraph.Series.Add("Dispenser").ChartType = SeriesChartType.Line;
            pressureGrapher.DataGraph.Series.Add("Top").ChartType = SeriesChartType.Line;
            pressureGrapher.DataGraph.Series.Add("Bottom").ChartType = SeriesChartType.Line;
            pressureGrapher.DataGraph.Series.Add("Window").ChartType = SeriesChartType.Line;

            foreach (Series series in pressureGrapher.DataGraph.Series)
            {
                series.XValueType = ChartValueType.DateTime;
            }

            graphers.Add("Bake Temp", pressureGrapher);
            pressureGrapher.TempDataSaveLoc.Text = "C:\\Users\\alfultra\\OneDrive - Imperial College London\\Bake_logs\\Default.csv";
            pressure.Controls.Add(pressureGrapher);
            MainTabs.TabPages.Add(pressure);

            pressureGrapher.SetupDataDisplay();
            //pressureGrapher.takeData.Checked = false;
            pressureGrapher.unit = "C";
        }

        private void AddMMStuff()
        {
            TabPage tp = new TabPage("MOTMaster");
            this.mmStuff = new MOTMasterStuff();
            tp.Controls.Add(mmStuff);
            MainTabs.Controls.Add(tp);
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

        private void EngageCryo_Click(object sender, EventArgs eargs)
        {
            if (Convert.ToDouble(controller.pressure1) > Convert.ToDouble(CRYO_SHUTOFF) && controller.InterlocksActive)
                return;
            UpdateRenderedObject(this.EngageCryo, (Button but) => { but.Enabled = false; });
            /*
            UpdateRenderedObject(this.DisengageCryo, (Button but) => { but.Enabled = true; });
            UpdateRenderedObject(this.CryoStatus, (Label lab) => { lab.Text = "ON"; });
            */
            lock (controller.lakeshore)
            {
                try
                {
                    controller.lakeshore.SetRelayParameters(1, 1);
                }
                catch (Exception e) when (e is Ivi.Visa.NativeVisaException || e is Ivi.Visa.IOTimeoutException)
                {
                    this.tSched.UpdateEventLog("Error in communicating with LakeShore:" + e.ToString());
                }
            }
        }

        private void DisengageCryo_Click(object sender, EventArgs eargs)
        {
            /*
            UpdateRenderedObject(this.DisengageCryo, (Button but) => { but.Enabled = false; });
            UpdateRenderedObject(this.EngageCryo, (Button but) => { but.Enabled = true; });
            UpdateRenderedObject(this.CryoStatus, (Label lab) => { lab.Text = "OFF"; });
            */
            lock (controller.lakeshore)
            {
                try
                {
                    controller.lakeshore.SetRelayParameters(1, 0);
                }
                catch (Exception e) when (e is Ivi.Visa.NativeVisaException || e is Ivi.Visa.IOTimeoutException)
                {
                    this.tSched.UpdateEventLog("Error in communicating with LakeShore:" + e.ToString());
                }
            }
        }

        private void Loop1Engage_Click(object sender, EventArgs eargs)
        {
            if(controller.loop1PV > TYPE_K_SHUTOFF && controller.InterlocksActive) return;
            UpdateRenderedObject(this.Loop1Engage, (Button but) => { but.Enabled = false; });
            lock (controller.eurotherm)
            {
                try
                {
                    controller.eurotherm.SetHeaterShutoff(0, false);
                    controller.eurotherm.SetAMSwitch(0, false);
                }
                catch (Exception e) when (e is Ivi.Visa.NativeVisaException || e is Ivi.Visa.IOTimeoutException)
                {
                    this.tSched.UpdateEventLog("Error in communicating with EuroTherm:" + e.ToString());
                }
            }
        }

        private void Loop1Disengage_Click(object sender, EventArgs eargs)
        {
            UpdateRenderedObject(this.Loop1Disengage, (Button but) => { but.Enabled = false; });
            lock (controller.eurotherm)
            {
                try
                {
                    controller.eurotherm.SetHeaterShutoff(0, true);
                    controller.eurotherm.SetAMSwitch(0, true);
                    controller.eurotherm.SetManOut(0, 0);
                }
                catch (Exception e) when (e is Ivi.Visa.NativeVisaException || e is Ivi.Visa.IOTimeoutException)
                {
                    this.tSched.UpdateEventLog("Error in communicating with EuroTherm:" + e.ToString());
                }
            }
        }

        private void Loop2Engage_Click(object sender, EventArgs eargs)
        {
            if (controller.loop2PV > TYPE_K_SHUTOFF && controller.InterlocksActive) return;
            UpdateRenderedObject(this.Loop2Engage, (Button but) => { but.Enabled = false; });
            lock (controller.eurotherm)
            {
                try
                {
                    controller.eurotherm.SetHeaterShutoff(1, false);
                    controller.eurotherm.SetAMSwitch(1, false);
                }
                catch (Exception e) when (e is Ivi.Visa.NativeVisaException || e is Ivi.Visa.IOTimeoutException)
                {
                    this.tSched.UpdateEventLog("Error in communicating with EuroTherm:" + e.ToString());
                }
            }
        }

        private void Loop2Disengage_Click(object sender, EventArgs eargs)
        {
            UpdateRenderedObject(this.Loop2Disengage, (Button but) => { but.Enabled = false; });
            lock (controller.eurotherm)
            {
                try
                {
                    controller.eurotherm.SetHeaterShutoff(1, true);
                    controller.eurotherm.SetAMSwitch(1, true);
                    controller.eurotherm.SetManOut(1, 0);
                }
                catch (Exception e) when (e is Ivi.Visa.NativeVisaException || e is Ivi.Visa.IOTimeoutException)
                {
                    this.tSched.UpdateEventLog("Error in communicating with EuroTherm:" + e.ToString());
                }
            }
        }

        private void SafetyInterlockEngage_Click(object sender, EventArgs e)
        {
            this.SetTextField(this.SafetyInterlockStatus, "ACTIVE");
            controller.interlocksActive = true;
            this.UpdateRenderedObject(this.SafetyInterlockEngage, (Button but) => { but.Enabled = false; });
            this.UpdateRenderedObject(this.SafetyInterlockDisengage, (Button but) => { but.Enabled = true; });
            this.BackColor = SystemColors.Control;
        }

        private void SafetyInterlockDisengage_Click(object sender, EventArgs e)
        {
            this.SetTextField(this.SafetyInterlockStatus, "DEFEATED");
            controller.interlocksActive = false;
            this.UpdateRenderedObject(this.SafetyInterlockEngage, (Button but) => { but.Enabled = true; });
            this.UpdateRenderedObject(this.SafetyInterlockDisengage, (Button but) => { but.Enabled = false; });
            this.BackColor = SystemColors.ControlDark;
        }

        private void AlFControlWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            controller.exiting = true;
            MSquaredLaserView.saveLineData();
            controller.UpdateThread.Abort();
            controller.DAQ_sync.AbortThreads();
            tSched.Exit();
        }
    }
}
