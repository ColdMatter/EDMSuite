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
        private daqmx.AnalogMultiChannelReader VIReader;
        private string channelToRead;

        private static Hashtable hardware = Environs.Hardware.AnalogInputChannels;

        public Controller controller;

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
                MOTMaster.MMDataIOHelper iohelper = new MOTMaster.MMDataIOHelper(savePath,"Rb");
                iohelper.SaveAnalogInputData(savePath + filename , voltages);

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
            double[,] aiData;

            configureVITask(physicalChan, taskName, clockRate);
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

        private void logBtn_Click(object sender, EventArgs e)
        {
            startVoltageAcquisition();
        }
    }
}
