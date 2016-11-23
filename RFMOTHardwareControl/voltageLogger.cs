using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using DAQ.Environment;
using DAQ.HAL;

using daqmx = NationalInstruments.DAQmx;
using NationalInstruments;
using MOTMaster;

namespace RFMOTHardwareControl
{
    public partial class voltageLogger : Form
    {
        private int sampleRate;
        private int nSamples;
        private bool saveData;
        private string filename;
        private string savePath;
        private double[,] voltages;
        private daqmx.Task voltageInputTask;
        private daqmx.Task continuousTask;
        private daqmx.Task runningTask;
        private daqmx.AnalogMultiChannelReader VIReader;
        private string channelToRead;
        private AsyncCallback callBack;
        private AnalogWaveform<double>[] data;

        private static Hashtable hardware = Environs.Hardware.AnalogInputChannels;


        private static string defaultSavePath = (string)Environs.FileSystem.Paths["DataPath"];

        
        public voltageLogger()
        {
            InitializeComponent();
            saveToTB.Text = defaultSavePath;
            getAnalogInputChannels();
            sampleRateTB.Text = "1000";
            filenameTB.Text = "filename";
            samplesTB.Text = "1000";
            
        }

        private void getInfo()
        {
            filename = filenameTB.Text + ".txt";
            saveData = saveCB.Checked;
            savePath = @saveToTB.Text;
            nSamples = Int32.Parse(samplesTB.Text);
            sampleRate = Int32.Parse(sampleRateTB.Text);
            channelToRead = @channelNamesComboBox.Text;
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void getAnalogInputChannels()
        {
            var dataSource = new List<analogInputChannelCombo>();
            
            foreach (string chanName in hardware.Keys)
            {
                AnalogInputChannel aichan = (AnalogInputChannel)hardware[chanName];
                dataSource.Add(new analogInputChannelCombo() {Name = (string)aichan.PhysicalChannel, Value=(string)aichan.PhysicalChannel });
            }

            channelNamesComboBox.DataSource = dataSource;
            channelNamesComboBox.DisplayMember = "Name";
            channelNamesComboBox.ValueMember = "Value";

            
        }

        private class analogInputChannelCombo
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        private void startVoltageAcquisition()
        {

            getInfo();
            voltages = acquireAnalogInputData(channelToRead, "AITask", sampleRate);
            attachDataToGraph();
            if (saveData)
            {
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                    MOTMaster.MMDataIOHelper iohelper = new MOTMaster.MMDataIOHelper(savePath, "Rb");
                    iohelper.SaveAnalogInputData(savePath + filename, voltages);
  
            }
            
        }

        private void attachDataToGraph()
        {
            AnalogWaveform<double>[] voltageWaveforms = AnalogWaveform<double>.FromArray2D(voltages);
            foreach (AnalogWaveform<double> voltagewf in voltageWaveforms)
            {
                voltageInGraph.PlotWaveform(voltagewf);
            }
        }


        #region setting up the analog input stream
        //I've actually done this elsewhere, but quite badly, so I'll do it again here and then maybe consolidate things later on.
        //The first job is to configure the task - this will fill the analog input buffer.
        //If we want to acquire at a constant rate we need to configure the timing and things

        public double[,] acquireAnalogInputData(string physicalChan,string taskName,double clockRate)
        {
            double[,] aiData = new double[1,nSamples];

            configureVITask(physicalChan, taskName, clockRate);
            voltageInputTask.Timing.SamplesPerChannel = nSamples;
            voltageInputTask.Control(daqmx.TaskAction.Verify);
            voltageInputTask.Start();
            aiData = readInVoltages();
            voltageInputTask.Stop();
            voltageInputTask.Dispose();
            return aiData;

        }

        
        public void configureVITask(string physicalChan,string taskName,double clockRate)
        {
            voltageInputTask = new daqmx.Task();

            voltageInputTask.AIChannels.CreateVoltageChannel(physicalChan, taskName, daqmx.AITerminalConfiguration.Differential,
            (double)0.0, (double)10.0, daqmx.AIVoltageUnits.Volts);

            voltageInputTask.Timing.ConfigureSampleClock("",clockRate, daqmx.SampleClockActiveEdge.Rising,daqmx.SampleQuantityMode.FiniteSamples);

            VIReader = new daqmx.AnalogMultiChannelReader(voltageInputTask.Stream);
        }

        public double[,] readInVoltages()
        {
            return VIReader.ReadMultiSample(nSamples);
        }


      #endregion
        #region Asynchronous Stuff
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                if (continuousTask != null)
                {
                    voltageInputTask = null;
                    continuousTask.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion
        private void logBtn_Click(object sender, EventArgs e)
        {
            if (!continuousCheck.Checked)
                startVoltageAcquisition();
            else
            {
                getInfo();
                try
                {
                    string taskName = channelToRead;
                    configureVITask(channelToRead, "", sampleRate);
                    continuousTask = voltageInputTask;
                    continuousTask.Timing.SamplesPerChannel = nSamples;
                    runningTask = continuousTask;
                    VIReader = new daqmx.AnalogMultiChannelReader(continuousTask.Stream);

                    callBack = new AsyncCallback(ReadCallBack);
                    voltageInputTask.Control(daqmx.TaskAction.Verify);
                    VIReader.SynchronizeCallbacks = true;
                    VIReader.BeginReadWaveform(Convert.ToInt32(continuousTask.Timing.SamplesPerChannel), callBack, continuousTask);

                    stopButton.Enabled = true;
                    logBtn.Enabled = false;
                   
                }
                catch (daqmx.DaqException ex)
                {
                    MessageBox.Show(ex.Message);
                    continuousTask.Dispose();
                }
            }
        }
        private void stopButton_Click(object sender, System.EventArgs e)
        {
            stopButton.Enabled = false;
            runningTask = null;

            try
            {
                continuousTask.Stop();
            }
            catch (daqmx.DaqException ex)
            {
                MessageBox.Show(ex.Message);
            }

            continuousTask.Dispose();
            logBtn.Enabled = true;
         

        }
        public void ReadCallBack(IAsyncResult ar)
        {
            try
            {
                if (runningTask != null && runningTask == ar.AsyncState)
                {
                    data = VIReader.EndReadWaveform(ar);
                    if (saveData)
                    {
                        if (!Directory.Exists(savePath))
                        {
                            Directory.CreateDirectory(savePath);
                        }
                        MOTMaster.MMDataIOHelper iohelper = new MOTMaster.MMDataIOHelper(savePath, "Rb");
                        iohelper.SaveAnalogInputData(savePath + filename, voltages, true);

                    }
                    voltageInGraph.PlotWaveformsAppend(data);

                    VIReader.BeginMemoryOptimizedReadWaveform(Convert.ToInt32(continuousTask.Timing.SamplesPerChannel), callBack, continuousTask, data);
                }
            }
            catch (daqmx.DaqException ex)
            {
                MessageBox.Show(ex.Message);
                continuousTask.Dispose();

                runningTask = null;
                logBtn.Enabled = true;
                stopButton.Enabled = false;
                
            }
        }

    }
}
