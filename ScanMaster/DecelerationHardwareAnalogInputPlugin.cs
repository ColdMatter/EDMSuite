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
        private double normalized_Data;
        [NonSerialized]
        private DecelerationHardwareControl.Controller hardwareControl;
        [NonSerialized]
        private int lockCavityChannel;
        [NonSerialized]
        private int refCavityChannel;

        protected override void InitialiseSettings()
        {
            settings["channelList"] = "longcavity,lockcavity,refcavity";
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
            // check whether the reference cavity is in the list
            bool refcavityFound = false;
            refCavityChannel = 0;
            while (!refcavityFound && refCavityChannel < channels.Length)
            {
                if (channels[refCavityChannel] == "refcavity") refcavityFound = true;
                else refCavityChannel++;
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
                // Add the refcavity if it's not already there
                if (!refcavityFound)
                {
                    ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels["refcavity"]).AddToTask(
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
                    //check that photodiode is not saturating
                    if (latestData[lockCavityChannel] > 4.7)
                    {
                        hardwareControl.diodeSaturationError();
                    }
                    else
                    {
                        hardwareControl.diodeSaturation();
                    }
                    // send the new lock cavity data to the hardware controller
                    normalized_Data = latestData[lockCavityChannel] / latestData[refCavityChannel];
                    hardwareControl.UpdateLockCavityData(normalized_Data);
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
                    if (!Environs.Debug)
                    {
                        a.Add(latestData[lockCavityChannel]/latestData[refCavityChannel]);
                        foreach (double d in latestData) a.Add(d);
                    }
                    //if (!Environs.Debug) foreach (double d in latestData) a.Add(d);
                    else for (int i = 0; i < ((String)settings["channelList"]).Split(',').Length; i++)
                            a.Add(new Random().NextDouble());
                    return a;
                }
            }
        }
    }
}


