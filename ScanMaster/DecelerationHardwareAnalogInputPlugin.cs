using System;
using System.Collections;
using System.Text;
using System.Xml.Serialization;

using NationalInstruments.DAQmx;

using DAQ.Environment;
using DAQ.HAL;
using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
    [Serializable]
    public class DecelerationHardwareAnalogInputPlugin : AnalogInputPlugin
    {
        [NonSerialized]
        private Task inputTask;
        [NonSerialized]
        private AnalogMultiChannelReader reader;
        [NonSerialized]
        private double[] latestData;
        [NonSerialized]
        private DecelerationHardwareControl.Controller hardwareControl;
        [NonSerialized]
        private int lockCavityChannel;

        protected override void InitialiseSettings()
        {
            settings["channelList"] = "longcavity,lockcavity";
            settings["inputRangeLow"] = -5.0;
            settings["inputRangeHigh"] = 5.0;
        } 

        public override void AcquisitionStarting()
        {
            //connect to the hardware controller
            hardwareControl = new DecelerationHardwareControl.Controller();
            
            // configure the analog input
            inputTask = new Task("analog inputs");

            string[] channels = ((String)settings["channelList"]).Split(',');
            // check whether the lock cavity is in the list
            bool cavityFound = false;
            lockCavityChannel = 0;
            while (!cavityFound && lockCavityChannel < channels.Length)
            {
                if (channels[lockCavityChannel] == "lockcavity") cavityFound = true;
                else lockCavityChannel++;
            }
            if (!Environs.Debug)
            {
                foreach (string channel in channels)
                {
                    ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
                        inputTask,
                        (double)settings["inputRangeLow"],
                        (double)settings["inputRangeHigh"]
                        );
                }
                // Add the lockcavity if it's not already there
                if (!cavityFound)
                {
                    ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels["lockcavity"]).AddToTask(
                        inputTask,
                        (double)settings["inputRangeLow"],
                        (double)settings["inputRangeHigh"]
                        );
                }
                inputTask.Control(TaskAction.Verify);
            }
            reader = new AnalogMultiChannelReader(inputTask.Stream);

            // block the ADC to all other applications
            hardwareControl.AnalogInputsAvailable = false;
        }

        public override void ScanStarting()
        {
        }

        public override void ScanFinished()
        {
        }

        public override void AcquisitionFinished()
        {
            // release the analog input
            inputTask.Dispose();
            // release the ADC
            hardwareControl.AnalogInputsAvailable = true;
        } 

        public override void ArmAndWait()
        {
            lock (this)
            {
                if (!Environs.Debug)
                {
                    inputTask.Start();
                    latestData = reader.ReadSingleSample();
                    inputTask.Stop();
                    // send the new lock cavity data to the hardware controller
                    hardwareControl.UpdateLockCavityData(latestData[lockCavityChannel]);
                }
                else
                {
                    // do nothing
                }
            }
        }

        [XmlIgnore]
        public override ArrayList Analogs
        {
            get
            {
                lock (this)
                {
                    ArrayList a = new ArrayList();
                    if (!Environs.Debug) foreach (double d in latestData) a.Add(d);
                    else for (int i = 0; i < ((String)settings["channelList"]).Split(',').Length; i++)
                            a.Add(new Random().NextDouble());
                    return a;
                }
            }
        }
    }
}


