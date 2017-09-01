using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NationalInstruments.DAQmx;

using DAQ.Environment;
using DAQ.HAL;
using Data;
using Data.Scans;
using DAQ.Analog;

namespace DAQ.Analog
{
    public class MMAIWrapper
    {
        #region ClassAttributes
        private Task AITask;
        private String device;
        private AnalogMultiChannelReader AIDataReader;
        private int samples;
        private int nChannels;
        public MMAIConfiguration AIConfig;
        private bool asyncRun;
        #endregion

        public MMAIWrapper(String device)
        {
            this.device = device;
        }

        public void Configure(MMAIConfiguration aiConfig, bool loop = false)
        {
        //For now lets just deal with adding a single analog input channel. Want things like sample rate to be specified in the mot master sequance.
            AIConfig = aiConfig;
            samples = aiConfig.Samples;
            AITask = new Task(this.device.Substring(1) + "AITask");
            
            foreach (string keys in aiConfig.AIChannels.Keys)
            {
                AddToAnalogInputTask(AITask, keys, aiConfig.AIChannels[keys].AIRangeLow,aiConfig.AIChannels[keys].AIRangeHigh);
            };
            AIConfig.AIData = new double[AIConfig.AIChannels.Count, samples];
        //For the timiming - for now just derive the ai sample clock from the PCI card, but this isn't synchronised with the PXI Card, so in future will
        //need to create a counting task on the AICard and count an exported timiming signal from the PXI or something similar.

            AITask.Timing.ConfigureSampleClock("", aiConfig.SampleRate, SampleClockActiveEdge.Rising,
                    SampleQuantityMode.FiniteSamples, aiConfig.Samples);

            AITask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                     (string)Environs.Hardware.GetInfo("AIAcquireTrigger"), DigitalEdgeStartTriggerEdge.Rising);

            asyncRun = loop;
            AITask.Control(TaskAction.Verify);
            AITask.Control(TaskAction.Commit);

            
        }

        #region private methods for creating timed Tasks/channels

        private void AddToAnalogInputTask(Task task, string channel, double RangeLow, double RangeHigh)
        {
        //The configuration settings for the analog input channel live in the RFMOTHardware class
        //Need to specify the input range high/low somewhere - perhaps in the scripts would be best. Then MOTMasterScript would have to return
        //something like an AIConfigurationObject, which we'd pass to the public Configure() method.
            AnalogInputChannel c = ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]);
            c.AddToTask(task, RangeLow, RangeHigh); 
        }
        #endregion
        public void StopPattern()
        {
            AITask.Stop();
            AITask.Dispose();
        }
        public void StartTask()
        {
           AITask.Start();
        }

        public void ReadAnalogDataFromBuffer()
        {
            AIDataReader = new AnalogMultiChannelReader(AITask.Stream);
            AIConfig.AIData = AIDataReader.ReadMultiSample(samples);
        }

        public double[,] GetAnalogData()
        {
            return AIConfig.AIData;
        }
        public double[] GetAnalogDataSingleArray()
        {
            double[] data = new double[AIConfig.AIData.Length];
            for (int i = 0; i < data.Length; i++) data[i] = AIConfig.AIData[0, i];
            return data;
        }

        
    }
}
