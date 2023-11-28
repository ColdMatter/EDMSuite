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

        private Task DAQTask;
        private AnalogMultiChannelReader dataReader;
        private List<string> AIchannels = (List<string>)Environs.Hardware.GetInfo("MMAnalogInputs");
        List<MOTMasterData> mmdata = new List<MOTMasterData>();
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

        private double[] xdata;

        private bool scanRunning = false;

        public MOTMasterStuff()
        {
            InitializeComponent();

            initPanels();
            WMLServer.Text = (new EnvironsHelper()).serverComputerName;

            selectedScan = ParamScan;
        }

        private void initPanels()
        {
            foreach (string aichannel in AIchannels)
            {
                TabPage dataGraph = new TabPage(aichannel);
                mmdata.Add(new MOTMasterData());
                dataGraph.Controls.Add(mmdata.Last());
                DataTabs.Controls.Add(dataGraph);

            }
        }

        private bool dataAcquired = false;

        private void UpdateReadings(object sender, TaskDoneEventArgs args)
        {
            double[,] data = dataReader.ReadMultiSample(Convert.ToInt32(this.sampNum.Text));

            for (int i = 0; i < mmdata.Count; ++i)
            {
                mmdata[i].UpdateData(xdata, Enumerable.Range(0, data.GetLength(1))
                .Select(x => data[i, x])
                .ToArray());

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
            DAQTask.Start();

            if (scanRunning)
            {
                scanResults.Last().Item2.Add(data);
                dataAcquired = true;
            }
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
        private TabPage selectedScan;

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
            PluginSettings.ignoreParameterHelper = true;

            MOTMaster.Controller mmaster = (MOTMaster.Controller)(Activator.GetObject(typeof(MOTMaster.Controller), "tcp://localhost:1187/controller.rem"));
            Dictionary<string, Object> dict = null;

            mmaster.SetIterations(1);
            mmaster.SetRunUntilStopped(false);


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

            }
            if (selectedScan == WMLScan)
            {
                scanPlugin = new WMLOutputPlugin();
                scanPlugin.Settings["laser"] = this.WMLLaser.Text;
                scanPlugin.Settings["computer"] = this.WMLServer;
                scanPlugin.Settings["WMLConfig"] = "WMLConfig";
                scanPlugin.Settings["scannedParameter"] = "setpoint";
                scanPlugin.Settings["setVoltageWaitTime"] = 50;
                scanPlugin.Settings["setSetPointWaitTime"] = 500;
                scanPlugin.Settings["offset"] = Convert.ToDouble(this.WMLOffset.Text); //Frequency offset in THz
                scanPlugin.Settings["start"] = Convert.ToDouble(this.WMLStart.Text);
                scanPlugin.Settings["end"] = Convert.ToDouble(this.WMLEnd.Text);
                scanPlugin.Settings["pointsPerScan"] = Convert.ToInt32(this.WMLSteps.Text);
                scanPlugin.Settings["shotsPerPoint"] = Convert.ToInt32(this.WMLShots.Text);
            }

            if (scanPlugin == null)
                throw new Exception("Bad configuration of scan setup. Check MOTMasterStuff:runScan");

            scanResults.Clear();

            scanPlugin.AcquisitionStarting();

            for (int i = 0; i < (int)scanPlugin.Settings["pointsPerScan"]; ++i)
            {

                scanPlugin.ScanStarting();

                scanPlugin.ScanParameter = NextScanParameter(scanPlugin, i, scanNumber);

                scanResults.Add(new Tuple<double, List<double[,]>>(scanPlugin.ScanParameter, new List<double[,]>()));

                for (int j = 0; j < (int)scanPlugin.Settings["shotsPerPoint"]; ++j)
                {
                    dataAcquired = false;
                    mmaster.Go(dict);

                    while (!dataAcquired) ;

                    if (!scanRunning) break;
                
                }

                scanPlugin.ScanFinished();

                Tuple<double, List<double[,]>> dp = scanResults.Last();

                double intavg = Enumerable.Range(0, dp.Item2.Count).Select(ind => Enumerable.Range(0, dp.Item2[ind].GetLength(1)).Select(x => dp.Item2[ind][dataTabsSelectedIndex, x]).ToArray().Sum()).ToArray().Average();
                scanGraph.Plots[0].PlotXYAppend(dp.Item1, intavg);

                if (!scanRunning) break;

            }

            scanPlugin.AcquisitionFinished();

/*            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter((string)Environs.FileSystem.Paths["ToFFilesPath"] + "Scan" + DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss-fff") + ".txt", false))
            {

                (new System.Xml.Serialization.XmlSerializer(typeof(List<Tuple<double, List<double[,]>>>))).Serialize(file, scanResults);
                file.Flush();
            }*/

            this.Invoke((Action)(() =>
            {
                stopScan.PerformClick();
                armToF.Enabled = true;
                scanTabs.Enabled = true;
                stopScan.Enabled = false;
                startScan.Enabled = true;
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
            armToF.Enabled = false;
            scanTabs.Enabled = false;
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

        private void DataTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            scanGraph.Plots[0].ClearData();
            dataTabsSelectedIndex = DataTabs.SelectedIndex;
            foreach (Tuple<double,List<double[,]>> dp in scanResults)
            {
                double intavg = Enumerable.Range(0, dp.Item2.Count).Select(i => Enumerable.Range(0, dp.Item2[i].GetLength(1)).Select(x => dp.Item2[i][dataTabsSelectedIndex, x]).ToArray().Sum()).ToArray().Average();
                scanGraph.Plots[0].PlotXYAppend(dp.Item1, intavg);
            }
            this.Invoke((Action)(()=>{ scanGraph.Update(); }));
        }

        private void scanTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedScan = scanTabs.SelectedTab;
        }
    }
}
