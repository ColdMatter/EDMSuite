using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Threading;

using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ;
using DAQ.HAL;
using ScanMaster.Acquire.Plugin;
using ScanMaster.Acquire.Plugins;
using System.Xml.Serialization;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;
using System.Collections;
using System.Runtime.Serialization.Formatters;

namespace AlFHardwareControl
{

    public partial class MOTMasterStuff : UserControl
    {

        public static Type MMScanDataType = typeof(MOTMasterStuff.KVPair<ScanOutputPlugin, List<MOTMasterStuff.KVPair<double, List<List<List<MOTMasterStuff.KVPair<string, double[]>>>>>>>);
        //private XYCursor cursor1 = new XYCursor();
        //private XYCursor cursor2 = new XYCursor();

        //private bool scanCursor1Reset = true;
        //private bool scanCursor2Reset = true;


        private Task DAQTask;
        private AnalogMultiChannelReader dataReader;
        private List<string> AIchannels = (List<string>)Environs.Hardware.GetInfo("MMAnalogInputs");
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

            /*WMLServer.Text = (String)System.Environment.GetEnvironmentVariables()["COMPUTERNAME"];

            selectedScan = ParamScan;*/
        }

        private void initPanels()
        {
            foreach (string aichannel in AIchannels)
            {
                TabPage dataGraph = new TabPage(aichannel);
                mmdata.Add(new MOTMasterData());
                mmdata.Last().mmstuff = this;
                mmdata.Last().initNormalisation(AIchannels);
                dataGraph.Controls.Add(mmdata.Last());
                DataTabs.Controls.Add(dataGraph);

            }
        }

        private bool dataAcquired = false;

        public Dictionary<string, double[]> AIData = new Dictionary<string, double[]>();

        private void UpdateReadings(object sender, TaskDoneEventArgs args)
        {
            double[,] data = dataReader.ReadMultiSample(Convert.ToInt32(this.sampNum.Text));

            for (int i = 0; i < mmdata.Count; ++i)
            {
                AIData[AIchannels[i]] = Enumerable.Range(0, data.GetLength(1))
                .Select(x => data[i, x])
                .ToArray();
                mmdata[i].UpdateData(xdata, AIData[AIchannels[i]]);

                using (System.IO.StreamWriter file =
                            new System.IO.StreamWriter((string)Environs.FileSystem.Paths["ToFFilesPath"] + AIchannels[i] + "Tof_" + DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss-fff") + ".txt", false))
                {
                    file.WriteLine("Sampling Rate: " + this.cmbSamplingRate.Text + ", Number of points :" + this.sampNum.Text);
                    for (int j = 0; j < Convert.ToInt32(this.sampNum.Text); ++j)
                        file.WriteLine(xdata[i].ToString() + "," + data[i, j].ToString());
                    file.Flush();
                }
            }

            dataSaved = true;

            DAQTask.Stop();

            if (scanRunning)
            {
                lock (scanResults)
                {
                    scanResults.Last().Item2.Add(data);
                }   
            }
            dataAcquired = true;
            DAQTask.Start();
        }

        private void setUpTasks()
        {
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

            DAQTask = new Task("DAQTask");

            foreach (string aichannel in AIchannels)
            {
                ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[aichannel]).AddToTask(DAQTask, 0, 10);
            }

            DAQTask.Timing.ConfigureSampleClock("", Convert.ToDouble(this.cmbSamplingRate.Text), SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, Convert.ToInt32(this.sampNum.Text));
            DAQTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger((string)Environs.Hardware.GetInfo("MMAITrigger"), DigitalEdgeStartTriggerEdge.Rising);

            DAQTask.Done += UpdateReadings;

            dataReader = new AnalogMultiChannelReader(DAQTask.Stream);

            DAQTask.Control(TaskAction.Verify);
            DAQTask.Control(TaskAction.Commit);

            DAQTask.Start();

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
        private List<Tuple<double,List<double[,]>>> scanResults = new List<Tuple<double, List<double[,]>>>();
        private Dictionary<double,List<List<double[,]>>> prevScanResults = new Dictionary<double, List<List<double[,]>>>();

        public Dictionary<double, List<List<double[,]>>> ScanData
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

        private ScanOutputPlugin scanPlugin = null;

        private void runScan()
        {
            ++scanNumber;
            scanGraph.Plots[0].ClearData();

            MOTMaster.Controller mmaster = (MOTMaster.Controller)(Activator.GetObject(typeof(MOTMaster.Controller), "tcp://localhost:1187/controller.rem"));
            Dictionary<string, Object> dict = new Dictionary<string, object>();

            mmaster.SetIterations(1);
            mmaster.SetRunUntilStopped(false);

            /*
            if (selectedScan == ParamScan)
            {
                scanPlugin = new MOTMasterScan();
                dict = new Dictionary<string, Object>();
                scanPlugin.Settings["scanOut"] = dict;
                scanPlugin.Settings["scanKey"] = this.pParam.Text;
                scanPlugin.Settings["start"] = Convert.ToDouble(this.pStart.Text);
                scanPlugin.Settings["end"] = Convert.ToDouble(this.pEnd.Text);
                scanPlugin.Settings["pointsPerScan"] = Convert.ToInt32(this.pSteps.Text);
                scanPlugin.Settings["shotsPerPoint"] = Convert.ToInt32(this.pShots.Text);
                this.Invoke((Action)(()=>{ scanPlugin.Settings["scanMode"] = this.pScanDir.Text; }));
            }
            if (selectedScan == WMLScan)
            {
                scanPlugin = new WMLOutputPlugin();
                scanPlugin.Settings["laser"] = this.WMLLaser.Text;
                scanPlugin.Settings["computer"] = this.WMLServer.Text;
                scanPlugin.Settings["WMLConfig"] = "WMLConfig";
                scanPlugin.Settings["scannedParameter"] = "setpoint";
                scanPlugin.Settings["setVoltageWaitTime"] = 50;
                scanPlugin.Settings["setSetPointWaitTime"] = 500;
                scanPlugin.Settings["offset"] = Convert.ToDouble(this.WMLOffset.Text); //Frequency offset in THz
                scanPlugin.Settings["start"] = Convert.ToDouble(this.WMLStart.Text);
                scanPlugin.Settings["end"] = Convert.ToDouble(this.WMLEnd.Text);
                scanPlugin.Settings["pointsPerScan"] = Convert.ToInt32(this.WMLSteps.Text);
                scanPlugin.Settings["shotsPerPoint"] = Convert.ToInt32(this.WMLShots.Text);
                this.Invoke((Action)(() => { scanPlugin.Settings["scanMode"] = this.WMLScanDir.Text; }));
            }
            
            if (scanPlugin == null)
                throw new Exception("Bad configuration of scan setup. Check MOTMasterStuff:runScan");*/

            this.Invoke((Action)(() =>
            {
                if (PluginSelector.Text == "MOTMaster Parameter")
                    scanPlugin.Settings["scanOut"] = dict;
            }));


            scanResults.Clear();

            scanPlugin.AcquisitionStarting();
            scanPlugin.ScanStarting();

            for (int i = 0; i < (int)scanPlugin.Settings["pointsPerScan"]; ++i)
            {


                scanPlugin.ScanParameter = NextScanParameter(scanPlugin, i, scanNumber);
                lock (scanResults)
                {
                    scanResults.Add(new Tuple<double, List<double[,]>>(scanPlugin.ScanParameter, new List<double[,]>()));
                }

                for (int j = 0; j < (int)scanPlugin.Settings["shotsPerPoint"]; ++j)
                {
                    dataAcquired = false;
                    mmaster.Go(dict);

                    while (!dataAcquired) ;

                    if (!scanRunning) break;

                }

                if (!scanRunning)
                {
                    lock (scanResults)
                    {
                        scanResults.RemoveAt(scanResults.Count - 1);
                    }
                    break;
                }

                Tuple<double, List<double[,]>> dp = scanResults.Last();
                double intavg = Enumerable.Range(0, dp.Item2.Count).Select(ind => mmdata[dataTabsSelectedIndex].NormaliseData(dp.Item2[ind]).Sum()).Average();
                this.Invoke((Action)(() => { scanGraph.Plots[0].PlotXYAppend(dp.Item1, intavg); }));


            }

            scanPlugin.ScanFinished();
            scanPlugin.AcquisitionFinished();

            /* using (System.IO.StreamWriter file =
            new System.IO.StreamWriter((string)Environs.FileSystem.Paths["ToFFilesPath"] + "Scan" + DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss-fff") + ".txt", false))
            {

                (new System.Xml.Serialization.XmlSerializer(typeof(List<Tuple<double, List<double[,]>>>))).Serialize(file, scanResults);
                file.Flush();
            }*/

            lock (scanResults)
            {
                scanResults.Sort((Tuple<double, List<double[,]>> a, Tuple<double, List<double[,]>> b) => { return Comparer<double>.Default.Compare(a.Item1, b.Item1); });
            }

            this.Invoke((Action)(()=> { DataTabs_SelectedIndexChanged(null, new EventArgs()); }));

            if (scanRunning)
            {
                lock (ScanData)
                {
                    foreach (Tuple<double, List<double[,]>> scanPoint in scanResults)
                    {
                        if (!prevScanResults.ContainsKey(scanPoint.Item1))
                            prevScanResults[scanPoint.Item1] = new List<List<double[,]>>();
                        prevScanResults[scanPoint.Item1].Add(scanPoint.Item2);
                    }
                }

                UpdateScanAverage();

                foreach (MOTMasterData mm in mmdata)
                {
                    mm.UpdateScan();
                }

            } 

            this.Invoke((Action)(() =>
            {
                stopScan.PerformClick();
                armToF.Enabled = true;
                //scanTabs.Enabled = true;
                stopScan.Enabled = false;
                startScan.Enabled = true;
                save_data.Enabled = true;
                clear_data.Enabled = true;
            }));

        }

        private void UpdateScanAverage()
        {
            if (ScanData.Count == 0) return;
            int index = dataTabsSelectedIndex;

            double[] xs = ScanData.Keys.ToArray();
            double[] ydata;

            lock (ScanData)
            {
                ydata = Enumerable.Range(0, ScanData.Values.Count).Select(
                    i => Enumerable.Range(0, ScanData.Values.ToArray()[i].Count).Select(
                        j => Enumerable.Range(0, ScanData.Values.ToArray()[i][j].Count).Select(
                            k => mmdata[index].NormaliseData(ScanData.Values.ToArray()[i][j][k]).Sum()).Average()).Average()).ToArray();
            }

            this.Invoke((Action)(() =>
            {
                scanGraph.Plots[1].ClearData();
                scanGraph.Plots[1].PlotXY(xs, ydata);
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
            scanNumber = 0;
            tofArmed = armToF.Checked;
            ScanData.Clear();
            scanNumber = 0;
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
                sampNum.Enabled = true;
                cmbSamplingRate.Enabled = true;
                scanCtrl.Enabled = false;
            }
        }

        private void startScan_Click(object sender, EventArgs e)
        {
            scanRunning = true;
            save_data.Enabled = false;
            clear_data.Enabled = false;
            armToF.Enabled = false;
            //scanTabs.Enabled = false;
            stopScan.Enabled = true;
            startScan.Enabled = false;

            (new Thread(new ThreadStart(()=> { runScan(); }))).Start();
        }

        private void stopScan_Click(object sender, EventArgs e)
        {
            scanRunning = false;
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
            scanGraph.Plots[0].ClearData();
            scanGraph.Plots[1].ClearData();
            if (scanResults.Count == 0) return;
            UpdateScanAverage();
            lock (scanResults)
            {
                foreach (Tuple<double, List<double[,]>> dp in scanResults)
                {
                    if (dp.Item2.Count == 0) continue;
                    double intavg = Enumerable.Range(0, dp.Item2.Count).Select(i => mmdata[dataTabsSelectedIndex].NormaliseData(dp.Item2[i]).Sum()).Average();
                    scanGraph.Plots[0].PlotXYAppend(dp.Item1, intavg);
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
                ScanData.Clear();
                scanNumber = 0;
            }

            dw.ShowDialog();
        }

        private void PluginSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            ScanData.Clear();
            scanNumber = 0;
            scanPlugin = scanPlugins[PluginSelector.Text];
        }

        private void clear_data_Click(object sender, EventArgs e)
        {
            ScanData.Clear();
            scanNumber = 0;

        }

        private void save_data_Click(object sender, EventArgs e)
        {
            scanCtrl.Enabled = false;
            if (ScanData.Count == 0) goto end;

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "xml data file|*.xml";
            saveFileDialog1.Title = "Save scan data";
            saveFileDialog1.InitialDirectory = Environs.FileSystem.GetDataDirectory(
                                                (String)Environs.FileSystem.Paths["scanMasterDataPath"]);
            saveFileDialog1.FileName = Environs.FileSystem.GenerateNextDataFileName();

            if (saveFileDialog1.ShowDialog() != DialogResult.OK) goto end;
            if (saveFileDialog1.FileName == "") goto end;

            //Dictionary<double,List<List<double[,]>>>

            KVPair<ScanOutputPlugin, List<KVPair<double, List<List<List<KVPair<string,double[]>>>>>>> results
                = new KVPair<ScanOutputPlugin, List<KVPair<double, List<List<List<KVPair<string, double[]>>>>>>>(scanPlugin, new List<KVPair<double, List<List<List<KVPair<string, double[]>>>>>>());

            foreach (double k in ScanData.Keys)
            {
                results.Value.Add(new KVPair<double, List<List<List<KVPair<string, double[]>>>>>(k,
                    Enumerable.Range(0, ScanData[k].Count).Select(
                    i=>Enumerable.Range(0,ScanData[k][i].Count).Select(
                        j=>Enumerable.Range(0, ScanData[k][i][j].GetLength(0)).Select(
                            l=>new KVPair<string,double[]>(AIchannels[l],Enumerable.Range(0,ScanData[k][i][j].GetLength(1)).Select(
                                m=>ScanData[k][i][j][l,m]).ToArray())).ToList()).ToList()).ToList()));
            }


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
    }
}
