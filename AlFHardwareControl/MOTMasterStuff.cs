using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Threading;

using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;
using ScanMaster.Acquire.Plugin;
using ScanMaster.Acquire.Plugins;
using Data;

using System.Collections;
using System.Drawing;

namespace AlFHardwareControl
{

    public partial class MOTMasterStuff : UserControl
    {

        public static Type MMScanDataType = typeof(MOTMasterStuff.ScanResults);
        public static Type OldMMScanDataType = typeof(MOTMasterStuff.KVPair<ScanOutputPlugin, List<MOTMasterStuff.KVPair<double, List<List<List<MOTMasterStuff.KVPair<string, double[]>>>>>>>);
        //private XYCursor cursor1 = new XYCursor();
        //private XYCursor cursor2 = new XYCursor();

        //private bool scanCursor1Reset = true;
        //private bool scanCursor2Reset = true;


        private Task DAQTask;
        private Task CtrTask;
        private AnalogMultiChannelReader dataReader;
        private CounterMultiChannelReader counterReader;
        private List<string> AIchannels = (List<string>)Environs.Hardware.GetInfo("MMAnalogInputs");
        private List<string> Ctrchannels = (List<string>)Environs.Hardware.GetInfo("MMCtrInputs");
        public List<MOTMasterData> mmdata = new List<MOTMasterData>();
        private bool dataSaved = false;
        public bool isDataSaved
        {
            get
            {
                return dataSaved;
            }
        }

        public void resetDataSaveStatus()
        {
            dataSaved = false;
        }

        public class KVPair<T1,T2>
        {

            public KVPair() { }

            public KVPair(T1 key, T2 value)
            {
                Key = key;
                Value = value;
            }

            public T1 Key { get; set; }
            public T2 Value { get; set; }
        }


        private KVPair<string,object>[] scanPluginSettings;

        public KVPair<string, object>[] ScanPluginSettings
        {
            get
            {
                return scanPluginSettings;
            }

            set
            {
                scanPluginSettings = value;
            }
        }

        private Dictionary<string, ScanOutputPlugin> scanPlugins = new Dictionary<string, ScanOutputPlugin>();

        private double[] xdata;

        private bool scanRunning = false;

        public MOTMasterStuff()
        {
            InitializeComponent();
            PluginSettings.ignoreParameterHelper = true;

            initPanels();

            scanPlugins.Add("MOTMaster Parameter", new MOTMasterScan());
            PluginSelector.Items.Add("MOTMaster Parameter");

            foreach (string plugin in PluginRegistry.GetRegistry().GetOutputPlugins())
            {
                scanPlugins.Add(plugin, PluginRegistry.GetRegistry().GetOutputPlugin(plugin));
                PluginSelector.Items.Add(plugin);
            }
            PluginSelector.SelectedIndex = 0;

            reloadPatterns_Click(null, new EventArgs());

        }

        private void initPanels()
        {
            foreach (string aichannel in AIchannels)
            {
                TabPage dataGraph = new TabPage(aichannel);
                mmdata.Add(new MOTMasterData(aichannel));
                mmdata.Last().mmstuff = this;
                mmdata.Last().initNormalisation(AIchannels);
                dataGraph.Controls.Add(mmdata.Last());
                DataTabs.Controls.Add(dataGraph);

            }

            foreach (string ctrchannel in Ctrchannels)
            {
                TabPage dataGraph = new TabPage(ctrchannel);
                mmdata.Add(new MOTMasterData(ctrchannel, "Count rate [kHz]"));
                mmdata.Last().mmstuff = this;
                mmdata.Last().initNormalisation(AIchannels);
                dataGraph.Controls.Add(mmdata.Last());
                DataTabs.Controls.Add(dataGraph);

            }

        }


        AutoResetEvent dataAcquired = new AutoResetEvent(false);
        private bool AIReady = false;
        private bool CtrReady = false;

        public SerializableDictionary<string, List<double[]>> AIData = new SerializableDictionary<string, List<double[]>>();
        private int patternProgress = 0;
        private void UpdateReadings(object sender, TaskDoneEventArgs args)
        {
            if ((!AIReady && AI) || (!CtrReady && Ctr)) return;
            int samples = Convert.ToInt32(this.sampNum.Text);
            int frequency = Convert.ToInt32(this.cmbSamplingRate.Text);
            double[,] data = new double[1,1];
            if (AI)
                data = dataReader.ReadMultiSample(samples);
            int[,] ctrData = new int[1, 1];
            double[,] ctrFreq = new double[1, 1];
            if (Ctr)
            {
                ctrData = counterReader.ReadMultiSampleInt32(samples);
                ctrFreq = new double[Ctrchannels.Count, samples];
            }

            int offset = 0;
            for (int i = 0; i < Ctrchannels.Count; ++i)
            {
                int count = 0;
                if (!mmdata[i + AIchannels.Count].SourceEnabled)
                {
                    ++offset;
                    continue;
                }
                for (int j = 0; j < samples; ++j)
                {
                    ctrFreq[i - offset, j] = (ctrData[i - offset, j] - count) * frequency / 1000;
                    count += ctrData[i - offset, j];
                }
            }

            offset = 0;
            if (patternProgress == 0)
                AIData = new SerializableDictionary<string, List<double[]>>();
            for (int i = 0; i < AIchannels.Count; ++i)
            {
                if (!mmdata[i].SourceEnabled)
                {
                    ++offset;
                    continue;
                }
                if (!AIData.ContainsKey(AIchannels[i]))
                    AIData.Add(AIchannels[i], new List<double[]> { });
                AIData[AIchannels[i]].Add(Enumerable.Range(0, data.GetLength(1))
                .Select(x => data[i-offset, x])
                .ToArray());
                mmdata[i].ReDraw();
                if (!saveEnable.Checked) continue;
                using (System.IO.StreamWriter file =
                            new System.IO.StreamWriter((string)Environs.FileSystem.Paths["ToFFilesPath"] + AIchannels[i] + "Tof_" + DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss-fff") + ".txt", false))
                {
                    file.WriteLine("Sampling Rate: " + this.cmbSamplingRate.Text + ", Number of points :" + this.sampNum.Text);
                    for (int j = 0; j < Convert.ToInt32(this.sampNum.Text); ++j)
                        file.WriteLine(xdata[i].ToString() + "," + data[i, j].ToString());
                    file.Flush();
                }
            }

            offset = 0;
            for (int i = 0; i < Ctrchannels.Count; ++i)
            {
                if (!mmdata[AIchannels.Count + i].SourceEnabled)
                {
                    ++offset;
                    continue;
                }
                if (!AIData.ContainsKey(Ctrchannels[i]))
                    AIData.Add(Ctrchannels[i], new List<double[]> { });
                AIData[Ctrchannels[i]].Add(Enumerable.Range(0, ctrFreq.GetLength(1))
                .Select(x => ctrFreq[i - offset, x])
                .ToArray());
                mmdata[i + AIchannels.Count].ReDraw();
                if (!saveEnable.Checked) continue;
                using (System.IO.StreamWriter file =
                            new System.IO.StreamWriter((string)Environs.FileSystem.Paths["ToFFilesPath"] + Ctrchannels[i] + "Tof_" + DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss-fff") + ".txt", false))
                {
                    file.WriteLine("Sampling Rate: " + this.cmbSamplingRate.Text + ", Number of points :" + this.sampNum.Text);
                    for (int j = 0; j < Convert.ToInt32(this.sampNum.Text); ++j)
                        file.WriteLine(xdata[i].ToString() + "," + ctrFreq[i, j].ToString());
                    file.Flush();
                }
            }

            dataSaved = true;

            CtrReady = false;
            AIReady = false;

            if (AI)
                DAQTask.Stop();
            if (Ctr)
                CtrTask.Stop();

            if (scanRunning)
            {
                lock (this)
                {
                    if (patternProgress == 0)
                        scanResults.Last().Item2.Add(AIData);
                }   
            }
            if (Ctr)
                CtrTask.Start();
            if (AI)
                DAQTask.Start();
            dataAcquired.Set();
        }


        private bool AI = false;
        private bool Ctr = false;
        private void setUpTasks()
        {
            AIData = new SerializableDictionary<string, List<double[]>> { };
            dataSaved = false;

            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo((string)Environs.FileSystem.Paths["ToFFilesPath"]);

            foreach (System.IO.FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

            xdata = new double[Convert.ToInt32(this.sampNum.Text)];

            for (int i = 0; i < Convert.ToInt32(this.sampNum.Text); ++i)
            {
                xdata[i] = 1000*((double)i) / Convert.ToInt32(this.cmbSamplingRate.Text);
            }

            foreach (MOTMasterData data in mmdata)
            {
                data.UpdateXData(xdata);
            }

            DAQTask = new Task("DAQTask");
            CtrTask = new Task("CtrTask");

            AI = false;
            for (int c = 0; c < AIchannels.Count; ++c)
            {
                mmdata[c].Invoke((Action)(()=>
                {
                    mmdata[c].sourceEnable.Enabled = false;
                }));
                if (!mmdata[c].SourceEnabled) continue;
                ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[AIchannels[c]]).AddToTask(DAQTask, 0, 10);
                AI = true;
            }

            Ctr = false;
            for (int c = 0; c < Ctrchannels.Count; ++c)
            {
                mmdata[c + AIchannels.Count].Invoke((Action)(() =>
                {
                    mmdata[c + AIchannels.Count].sourceEnable.Enabled = false;
                }));
                if (!mmdata[c + AIchannels.Count].SourceEnabled) continue;
                CtrTask.CIChannels.CreateCountEdgesChannel(((CounterChannel)Environs.Hardware.CounterChannels[Ctrchannels[c]]).PhysicalChannel, Ctrchannels[c], CICountEdgesActiveEdge.Rising, 0,CICountEdgesCountDirection.Up);
                Ctr = true;
            }

            if (AI)
            {
                DAQTask.Timing.ConfigureSampleClock("", Convert.ToDouble(this.cmbSamplingRate.Text), SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, Convert.ToInt32(this.sampNum.Text));
                DAQTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger((string)Environs.Hardware.GetInfo("MMAITrigger"), DigitalEdgeStartTriggerEdge.Rising);
                DAQTask.Done += (object ob, TaskDoneEventArgs args) => { AIReady = true; UpdateReadings(ob, args); }; ;
                dataReader = new AnalogMultiChannelReader(DAQTask.Stream);
            }
            
            if (Ctr)
            {
                CtrTask.Timing.ConfigureSampleClock((string)Environs.Hardware.GetInfo("MMCtrSampleClock"), Convert.ToDouble(this.cmbSamplingRate.Text), SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, Convert.ToInt32(this.sampNum.Text));
                CtrTask.Triggers.ArmStartTrigger.ConfigureDigitalEdgeTrigger((string)Environs.Hardware.GetInfo("MMCtrTrigger"), DigitalEdgeArmStartTriggerEdge.Rising);
                CtrTask.Done += (object ob, TaskDoneEventArgs args) => { CtrReady = true; UpdateReadings(ob, args); };
                counterReader = new CounterMultiChannelReader(CtrTask.Stream);
            }
            //CtrTask.Triggers.Type = StartTriggerType.DigitalEdge;
            //CtrTask.Triggers.StartTrigger.DigitalEdge.Edge = DigitalEdgeStartTriggerEdge.Rising;
            //CtrTask.Triggers.StartTrigger.DigitalEdge.Source = (string)Environs.Hardware.GetInfo("MMCtrTrigger");

            if (AI)
            {
                DAQTask.Control(TaskAction.Verify);
                DAQTask.Control(TaskAction.Commit);
            }

            if (Ctr)
            {
                CtrTask.Control(TaskAction.Verify);
                CtrTask.Control(TaskAction.Commit);
            }

            CtrReady = false;
            AIReady = false;

            if (AI)
                DAQTask.Start();
            if (Ctr)
                CtrTask.Start();
        }

        private System.Collections.ArrayList scanValues;
        private double NextScanParameter(ScanOutputPlugin outputPlugin ,int pointNumber, int scanNumber)
        {
            PluginSettings outputSettings = outputPlugin.Settings;
            double scanParameter;
            string mode = (string)outputSettings["scanMode"];
            switch (mode)
            {
                case "up":
                    scanParameter = (double)outputSettings["start"] +
                    ((double)outputSettings["end"] - (double)outputSettings["start"]) * (pointNumber + 0.5)
                    / ((int)outputSettings["pointsPerScan"]);
                    break;
                case "down":
                    scanParameter = (double)outputSettings["end"] +
                        ((double)outputSettings["start"] - (double)outputSettings["end"]) * (pointNumber + 0.5)
                        / ((int)outputSettings["pointsPerScan"]);
                    break;
                case "updown":
                    if (scanNumber % 2 == 0)
                    {
                        scanParameter = (double)outputSettings["start"] +
                            ((double)outputSettings["end"] - (double)outputSettings["start"]) * (pointNumber + 0.5)
                            / ((int)outputSettings["pointsPerScan"]);
                    }
                    else
                    {
                        scanParameter = (double)outputSettings["end"] +
                                ((double)outputSettings["start"] - (double)outputSettings["end"]) * (pointNumber + 0.5)
                                / ((int)outputSettings["pointsPerScan"]);
                    }
                    break;
                case "downup":
                    if (scanNumber % 2 != 0)
                    {
                        scanParameter = (double)outputSettings["start"] +
                            ((double)outputSettings["end"] - (double)outputSettings["start"]) * (pointNumber + 0.5)
                            / ((int)outputSettings["pointsPerScan"]);
                    }
                    else
                    {
                        scanParameter = (double)outputSettings["end"] +
                                ((double)outputSettings["start"] - (double)outputSettings["end"]) * (pointNumber + 0.5)
                                / ((int)outputSettings["pointsPerScan"]);
                    }
                    break;
                case "random":
                    if (pointNumber == 0)
                    {
                        scanValues = new System.Collections.ArrayList();
                        for (int i = 0; i < (int)outputSettings["pointsPerScan"]; i++) // fill the array at the beginning of the scan
                        {
                            scanValues.Add((double)outputSettings["start"] +
                                ((double)outputSettings["end"] - (double)outputSettings["start"]) * (i + 0.5)
                                / ((int)outputSettings["pointsPerScan"]));
                        }
                    }
                    Random rnd = new Random();
                    int selectedIndex = rnd.Next(0, scanValues.Count);
                    scanParameter = (double)scanValues[selectedIndex];
                    scanValues.RemoveAt(selectedIndex);
                    break;
                default: //scan up by default
                    scanParameter = (double)outputSettings["start"] +
                    ((double)outputSettings["end"] - (double)outputSettings["start"]) * (pointNumber + 0.5)
                    / ((int)outputSettings["pointsPerScan"]);
                    break;
            }
            return scanParameter;
        }

        private int scanNumber = 0;
        private List<Tuple<double,List<SerializableDictionary<string, List<double[]>>>>> scanResults = new List<Tuple<double, List<SerializableDictionary<string, List<double[]>>>>>();
        private SerializableDictionary<double,List<List<SerializableDictionary<string, List<double[]>>>>> prevScanResults = new SerializableDictionary<double, List<List<SerializableDictionary<string, List<double[]>>>>>();

        public SerializableDictionary<double, List<List<SerializableDictionary<string, List<double[]>>>>> ScanData
        {
            get
            {
                return prevScanResults;
            }
        }

        public double ScanParameter
        {
            get
            {
                return scanPlugin.ScanParameter;
            }
        }
        public string ScanPlugin
        {
            get
            {
                return scanPlugin.ToString();
            }
        }

        public Hashtable ScanSettings
        {
            get
            {
                Hashtable settings = new Hashtable();
                foreach (string key in scanPlugin.Settings.Keys)
                {
                    settings[key] = scanPlugin.Settings[key];
                }
                return settings;
            }
        }

        private void runPattern()
        {
            MOTMaster.Controller mmaster = (MOTMaster.Controller)(Activator.GetObject(typeof(MOTMaster.Controller), "tcp://localhost:1187/controller.rem"));
            Dictionary<string, Object> dict = new Dictionary<string, object>();

            mmaster.SetIterations(1);
            mmaster.SetRunUntilStopped(false);
            mmaster.SetScriptPath(selectedPattern);

            switchConfiguration = mmaster.GetSwitchConfiguration();
            switchStates = switchConfiguration.Count == 0 ? 1 : switchConfiguration.Values.First().Count;
            UpdateScanViewSize();

            foreach (MOTMasterData data in mmdata)
            {
                data.UpdateScanStatus(true);
            }

            do
            {
                runSinglePattern(mmaster, dict, () => { return !patternRunning; });
            } while (repeatScanChecked && patternRunning);
            patternRunning = false;
            foreach (MOTMasterData data in mmdata)
            {
                data.UpdateScanStatus(false);
            }
            EnableScanControls();

        }

        private void runSinglePattern(MOTMaster.Controller mmaster, Dictionary<string, Object> dict, Func<bool> breakCondition)
        {
            string text = "ACCEPTED";
            do
            {
                for (int  i = 0; i < switchStates; ++i)
                {
                    foreach (KeyValuePair<string, List<bool>> Switch in switchConfiguration)
                    {
                        if (!dict.ContainsKey(Switch.Key))
                            dict.Add(Switch.Key, false);
                        dict[Switch.Key] = Switch.Value[i];
                    }

                    dataAcquired.Reset();
                    mmaster.Go(dict);

                    dataAcquired.WaitOne();
                    ++patternProgress;
                }

                if (breakCondition()) break;

                text = "ACCEPTED";
                Color color = Color.PaleGreen;
                foreach (MOTMasterData data in mmdata)
                {
                    if (data.SourceEnabled && data.reject_shot())
                    {
                        text = "REJECTED";
                        color = Color.Salmon;
                        if (!scanRunning) break;
                        lock (this)
                        {
                            scanResults.Last().Item2.RemoveAt(scanResults.Last().Item2.Count - 1);
                        }
                        break;
                    }
                }

                this.Invoke((Action)(() =>
                {
                    this.RejectionStatus.BackColor = color;
                    this.RejectionStatus.Text = text;
                }));


                patternProgress = 0;
            } while (text != "ACCEPTED");
            patternProgress = 0;

        }

        private void UpdateScanViewSize()
        {
            this.Invoke((Action)(() =>
            {
                while (switchStates * 2 < scanGraph.Plots.Count)
                {
                    scanGraph.Plots.RemoveAt(scanGraph.Plots.Count - 1);
                }
                while (switchStates * 2 > scanGraph.Plots.Count)
                {
                    NationalInstruments.UI.ScatterPlot points = new NationalInstruments.UI.ScatterPlot(scanGraph.XAxes[0], scanGraph.YAxes[0]);
                    NationalInstruments.UI.ScatterPlot scan = new NationalInstruments.UI.ScatterPlot(scanGraph.XAxes[0], scanGraph.YAxes[0]);
                    scanGraph.Plots.Add(points);
                    scanGraph.Plots.Add(scan);
                    points.PointColor = points.LineColor;
                    points.LineStyle = NationalInstruments.UI.LineStyle.None;
                    points.PointStyle = NationalInstruments.UI.PointStyle.Cross;
                    scan.LineColor = points.LineColor;
                }
                foreach (NationalInstruments.UI.ScatterPlot plot in scanGraph.Plots)
                {
                    plot.ClearData();
                }
            }));
        }

        private ScanOutputPlugin scanPlugin = null;
        private Dictionary<string, List<bool>> switchConfiguration;
        private int switchStates = 1;

        public int SwitchStates
        {
            get
            {
                return switchStates;
            }
        }

        private void runScan()
        {
            ++scanNumber;

            
            MOTMaster.Controller mmaster = (MOTMaster.Controller)(Activator.GetObject(typeof(MOTMaster.Controller), "tcp://localhost:1187/controller.rem"));
            Dictionary<string, Object> dict = new Dictionary<string, object>();

            mmaster.SetIterations(1);
            mmaster.SetRunUntilStopped(false);
            mmaster.SetScriptPath(selectedPattern);

            switchConfiguration = mmaster.GetSwitchConfiguration();
            int prevSwitchStates = switchStates;
            switchStates = switchConfiguration.Count == 0 ? 1 : switchConfiguration.Values.First().Count;
            if (prevSwitchStates != switchStates) clear_data_Click(null, new EventArgs());
            UpdateScanViewSize();

            this.Invoke((Action)(() =>
            {
                if (PluginSelector.Text == "MOTMaster Parameter")
                    scanPlugin.Settings["scanOut"] = dict;
            }));

            foreach (MOTMasterData data in mmdata)
            {
                data.UpdateScanStatus(true);
            }

            scanPlugin.AcquisitionStarting();

            do
            {
                for (int i = 0; i < switchStates; ++i)
                    scanGraph.Plots[2 * i].ClearData();
                scanResults.Clear();
                scanPlugin.ScanStarting();

                for (int i = 0; i < (int)scanPlugin.Settings["pointsPerScan"]; ++i)
                {


                    scanPlugin.ScanParameter = NextScanParameter(scanPlugin, i, scanNumber);
                    lock (this)
                    {
                        scanResults.Add(new Tuple<double, List<SerializableDictionary<string, List<double[]>>>>(scanPlugin.ScanParameter, new List<SerializableDictionary<string, List<double[]>>>()));
                    }

                    for (int j = 0; j < (int)scanPlugin.Settings["shotsPerPoint"]; ++j)
                    {

                        this.Invoke((Action)(() => { this.scanPointProgress.Text = String.Format("{0}/{1}", j + 1, (int)scanPlugin.Settings["shotsPerPoint"]); }));
                        runSinglePattern(mmaster, dict, () => { return !scanRunning; });

                        if (!scanRunning) break;

                    }

                    if (!scanRunning)
                    {
                        lock (this)
                        {
                            scanResults.RemoveAt(scanResults.Count - 1);
                        }
                        break;
                    }

                    if (!mmdata[dataTabsSelectedIndex].SourceEnabled) continue;
                    Tuple<double, List<SerializableDictionary<string, List<double[]>>>> dp = scanResults.Last();
                    List<List<double[]>> normedData = Enumerable.Range(0, dp.Item2.Count).Select(
                                            ind => mmdata[dataTabsSelectedIndex].NormaliseData(dp.Item2[ind])).ToList();
                    List<double> intavg = Enumerable.Range(0, switchStates).Select(
                                        ind => Enumerable.Range(0, normedData.Count).Select(
                                        j => normedData[j][ind].Sum()).Average()).ToList();
                    this.Invoke((Action)(() => {
                        for (int j = 0; j < switchStates; ++j)
                        {
                            scanGraph.Plots[2 * j].PlotXYAppend(dp.Item1, intavg[j]);
                            scanGraph.Update();
                        }
                    }));


                }

                scanPlugin.ScanFinished();


                /* using (System.IO.StreamWriter file =
                new System.IO.StreamWriter((string)Environs.FileSystem.Paths["ToFFilesPath"] + "Scan" + DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss-fff") + ".txt", false))
                {

                    (new System.Xml.Serialization.XmlSerializer(typeof(List<Tuple<double, List<double[,]>>>))).Serialize(file, scanResults);
                    file.Flush();
                }*/

                lock (this)
                {
                    scanResults.Sort((Tuple<double, List<SerializableDictionary<string, List<double[]>>>> a, Tuple<double, List<SerializableDictionary<string, List<double[]>>>> b) => { return Comparer<double>.Default.Compare(a.Item1, b.Item1); });
                }

                this.Invoke((Action)(() => { DataTabs_SelectedIndexChanged(null, new EventArgs()); }));

                if (scanRunning)
                {
                    lock (this)
                    {
                        foreach (Tuple<double, List<SerializableDictionary<string, List<double[]>>>> scanPoint in scanResults)
                        {
                            if (!prevScanResults.ContainsKey(scanPoint.Item1))
                                prevScanResults[scanPoint.Item1] = new List<List<SerializableDictionary<string, List<double[]>>>>();
                            prevScanResults[scanPoint.Item1].Add(scanPoint.Item2);
                        }
                    }

                    UpdateScanAverage();

                    foreach (MOTMasterData mm in mmdata)
                    {
                        mm.UpdateScan();
                    }

                }

            } while (scanRunning && repeatScanChecked);
            scanPlugin.AcquisitionFinished();

            this.Invoke((Action)(() => { stopScan.PerformClick(); }));
            EnableScanControls();

            foreach (MOTMasterData data in mmdata)
            {
                data.UpdateScanStatus(false);
            }

        }

        private void EnableScanControls()
        {
            this.Invoke((Action)(() =>
            {
                armToF.Enabled = true;
                //scanTabs.Enabled = true;
                stopScan.Enabled = false;
                startScan.Enabled = true;
                save_data.Enabled = true;
                clear_data.Enabled = true;
                repeatScan.Enabled = true;
                reloadPatterns.Enabled = true;
                startPattern.Enabled = true;
                PatternPicker.Enabled = true;
            }));
        }

        private void DisableScanControls()
        {

            save_data.Enabled = false;
            clear_data.Enabled = false;
            armToF.Enabled = false;
            //scanTabs.Enabled = false;
            stopScan.Enabled = true;
            startScan.Enabled = false;
            repeatScan.Enabled = false;
            reloadPatterns.Enabled = false;
            startPattern.Enabled = false;
            PatternPicker.Enabled = false;
        }

        private void UpdateScanAverage()
        {
            if (!mmdata[dataTabsSelectedIndex].SourceEnabled) return;
            if (ScanData.Count == 0) return;
            int index = dataTabsSelectedIndex;

            double[] xs = ScanData.Keys.ToArray();
            List<double[]> ydata;

            lock (this)
            {
                ydata = Enumerable.Range(0, switchStates).Select(
                    m => Enumerable.Range(0, ScanData.Values.Count).Select(
                    i => Enumerable.Range(0, ScanData.Values.ToArray()[i].Count).Select(
                    j => Enumerable.Range(0, ScanData.Values.ToArray()[i][j].Count).Select(
                    k => mmdata[index].NormaliseData(ScanData.Values.ToArray()[i][j][k])[m].Sum()).Average()).Average()).ToArray()).ToList();
            }

            this.Invoke((Action)(() =>
            {
                for (int i = 0; i < switchStates; ++i)
                {
                    scanGraph.Plots[2 * i + 1].ClearData();
                    scanGraph.Plots[2 * i + 1].PlotXY(xs, ydata[i]);

                }
                scanGraph.Update();
            }));

        }

        private bool tofArmed = false;
        public bool ToFArmed
        {
            get
            {
                return tofArmed;
            }
        }

        private void armToF_CheckedChanged(object sender, EventArgs e)
        {
            tofArmed = armToF.Checked;
            clear_data.PerformClick();
            if (armToF.Checked)
            {
                setUpTasks();
                sampNum.Enabled = false;
                cmbSamplingRate.Enabled = false;
                scanCtrl.Enabled = true;
            }
            else
            {

                DAQTask.Stop();
                DAQTask.Dispose();

                CtrTask.Stop();
                CtrTask.Dispose();

                for (int c = 0; c < mmdata.Count; ++c)
                {
                    mmdata[c].Invoke((Action)(() =>
                    {
                        mmdata[c].sourceEnable.Enabled = true;
                    }));
                }
                sampNum.Enabled = true;
                cmbSamplingRate.Enabled = true;
                scanCtrl.Enabled = false;
            }
        }

        private void startScan_Click(object sender, EventArgs e)
        {
            scanRunning = true;
            DisableScanControls();

            (new Thread(new ThreadStart(()=> { runScan(); }))).Start();
        }

        private void stopScan_Click(object sender, EventArgs e)
        {
            scanRunning = false;
            patternRunning = false;
        }
        public bool ScanRunning
        {
            get
            {
                return scanRunning;
            }
        }

        private int dataTabsSelectedIndex;

        public void ReDrawScanResults() 
        {
            foreach (NationalInstruments.UI.ScatterPlot plot in scanGraph.Plots)
                plot.ClearData();
            if (!mmdata[dataTabsSelectedIndex].SourceEnabled) return;
            if (scanResults.Count == 0) return;
            UpdateScanAverage();
            lock (this)
            {
                foreach (Tuple<double, List<SerializableDictionary<string, List<double[]>>>> dp in scanResults)
                {
                    if (dp.Item2.Count < switchStates) continue;
                    List<double> intavg = Enumerable.Range(0, switchStates).Select(
                                    j => Enumerable.Range(0, dp.Item2.Count).Select(
                                    i => mmdata[dataTabsSelectedIndex].NormaliseData(dp.Item2[i])[j].Sum()).Average()).ToList();
                    for (int i = 0; i < switchStates; ++i)
                        scanGraph.Plots[2 * i].PlotXYAppend(dp.Item1, intavg[i]);
                }
            }
        }

        private void DataTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataTabsSelectedIndex = DataTabs.SelectedIndex;
            ReDrawScanResults();
        }

        public void DictionaryViewClosing()
        {

            PluginSettings currentPluginSettings = scanPlugins[PluginSelector.Text].Settings;
            foreach (KVPair<string, object> kv in scanPluginSettings)
            {
                try
                {
                    currentPluginSettings[kv.Key] = Convert.ChangeType(kv.Value, currentPluginSettings[kv.Key].GetType());
                }
                catch(Exception e) when (e is System.FormatException || e is InvalidCastException)
                {

                }

            }

            this.Invoke((Action)(() =>
            {
            
                ScanParameterButton.Enabled = true;
                PluginSelector.Enabled = true;
            }));
        }

        private void ScanParameterButton_Click(object sender, EventArgs e)
        {
            ScanParameterButton.Enabled = false;
            PluginSelector.Enabled = false;
            DictionaryView dw = new DictionaryView(this);
            List<MOTMasterStuff.KVPair<string,object>> sps = new List<KVPair<string, object>>();
            PluginSettings currentPluginSettings = scanPlugins[PluginSelector.Text].Settings;
            foreach (string key in currentPluginSettings.Keys)
            {
                sps.Add(new KVPair<string, object>(key, currentPluginSettings[key]));
            }
            ScanPluginSettings = sps.ToArray(); 

            dw.DictionaryData.DataSource = ScanPluginSettings;
            dw.DictionaryData.Columns[0].ReadOnly = true;
            dw.DictionaryData.Columns[1].ReadOnly = scanRunning;

            if (!scanRunning)
            {
                clear_data.PerformClick();
            }

            dw.ShowDialog();
        }

        private void PluginSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            clear_data.PerformClick();
            scanPlugin = scanPlugins[PluginSelector.Text];
        }

        private void clear_data_Click(object sender, EventArgs e)
        {
            ScanData.Clear();
            scanResults.Clear();
            scanNumber = 0;

        }

        [Serializable]
        public class ScanResults
        {
            public ScanOutputPlugin scanOutputPlugin { get; set; }
            public double[] xData { get; set; }
            public SerializableDictionary<double, List<List<SerializableDictionary<string, List<double[]>>>>> scanResults { get; set; }
            public SerializableDictionary<string, object> additionalParameters { get; set; }

            public ScanResults() { }

            public ScanResults(ScanOutputPlugin _outplugin, double[] _xdata, SerializableDictionary<double, List<List<SerializableDictionary<string, List<double[]>>>>> _results, SerializableDictionary<string, object> _miscParams)
            {
                scanOutputPlugin = _outplugin;
                xData = _xdata;
                scanResults = _results;
                additionalParameters = _miscParams;
            }
        }

        private void save_data_Click(object sender, EventArgs e)
        {
            if (ScanData.Count == 0) return;
            scanCtrl.Enabled = false;

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "xml data file|*.xml";
            saveFileDialog1.Title = "Save scan data";
            saveFileDialog1.InitialDirectory = Environs.FileSystem.GetDataDirectory(
                                                (String)Environs.FileSystem.Paths["scanMasterDataPath"]);
            saveFileDialog1.FileName = Environs.FileSystem.GenerateNextDataFileName();

            if (saveFileDialog1.ShowDialog() != DialogResult.OK) goto end;
            if (saveFileDialog1.FileName == "") goto end;

            //Dictionary<double,List<List<double[,]>>>

            ScanResults results
                = new ScanResults(scanPlugin, xdata, ScanData, new SerializableDictionary<string, object>());


            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(saveFileDialog1.FileName))
            {

                (new System.Xml.Serialization.XmlSerializer(MMScanDataType)).Serialize(file, results);
                file.Flush();

            }
            end:
            scanCtrl.Enabled = true;
        }

        private void fixX_CheckedChanged(object sender, EventArgs e)
        {
            scanGraph.XAxes[0].Mode = fixX.Checked ? NationalInstruments.UI.AxisMode.Fixed : NationalInstruments.UI.AxisMode.AutoScaleLoose;
        }

        private void fixY_CheckedChanged(object sender, EventArgs e)
        {
            scanGraph.YAxes[0].Mode = fixY.Checked ? NationalInstruments.UI.AxisMode.Fixed : NationalInstruments.UI.AxisMode.AutoScaleLoose;
        }

        private bool repeatScanChecked = false;
        private void repeatScan_CheckedChanged(object sender, EventArgs e)
        {
            repeatScanChecked = repeatScan.Checked;
        }

        private Dictionary<string, string> patternPaths = new Dictionary<string, string> { };
        private void reloadPatterns_Click(object sender, EventArgs e)
        {
            patternPaths.Clear();
            PatternPicker.Items.Clear();
            List<Tuple<string,string>> scriptList = System.IO.Directory.EnumerateFiles((string)Environs.FileSystem.Paths["scriptListPath"], "*.cs").Select(i=>new Tuple<string, string> (i, System.IO.Path.GetFileName(i))).ToList();
            foreach (Tuple<string,string> script in scriptList)
            {
                PatternPicker.Items.Add(script.Item2);
                patternPaths.Add(script.Item2, script.Item1);
            }
            PatternPicker.SelectedIndex = 0;
        }

        private bool patternRunning = false;
        private void startPattern_Click(object sender, EventArgs e)
        {
            DisableScanControls();
            patternRunning = true;
            (new Thread(new ThreadStart(runPattern))).Start();
        }

        private string selectedPattern = "";
        private void PatternPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedPattern = patternPaths[PatternPicker.Text];
        }
    }
}
