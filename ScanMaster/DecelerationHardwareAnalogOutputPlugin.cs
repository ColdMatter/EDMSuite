#if DECELERATOR

using System;
using System.Threading;
using System.Xml.Serialization;

using NationalInstruments.DAQmx;

using DAQ.Environment;
using DAQ.HAL;
using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
    /// <summary>
    /// A plugin to scan an analog output provided the DecelerationHardwareControl allows it
    /// </summary>
    [Serializable]
    public class DecelerationHardwareAnalogOutputPlugin : ScanOutputPlugin
    {
        [NonSerialized]
        private double scanParameter = 0;

        [NonSerialized]
        private Task outputTask;
        [NonSerialized]
        private AnalogSingleChannelWriter writer;
        [NonSerialized]
        private DecelerationHardwareControl.Controller hardwareControl;

        protected override void InitialiseSettings()
        {
            settings["channel"] = "laser";
            settings["rampToZero"] = true;
            settings["rampSteps"] = 50;
            settings["rampDelay"] = 20;
        }


        public override void AcquisitionStarting()
        {
            //connect to the hardware controller
            hardwareControl = new DecelerationHardwareControl.Controller();
            
            // initialise the output hardware, full scale -10 to 10 volts
            outputTask = new Task("analog output");
            if (!Environs.Debug)
            {
                AnalogOutputChannel oc =
                    (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[(string)settings["channel"]];
                oc.AddToTask(outputTask, -10, 10);
                writer = new AnalogSingleChannelWriter(outputTask.Stream);
                if (!Blocked()) writer.WriteSingleSample(true, 0);
            }
            scanParameter = 0;
			//go gently to the correct start position
			if (((string)settings["scanMode"] == "up" || (string)settings["scanMode"] == "updown") && !Blocked()) 
				rampOutputToVoltage((double)settings["start"]);
			if (((string)settings["scanMode"] == "down" || (string)settings["scanMode"] == "downup") && !Blocked()) 
				rampOutputToVoltage((double)settings["end"]);
        }

        public override void ScanStarting()
        {
			// do nothing
        }

        public override void ScanFinished()
        {
			if ((string)settings["scanMode"] == "up" && !Blocked()) rampOutputToVoltage((double)settings["start"]);
			if ((string)settings["scanMode"] == "down" && !Blocked()) rampOutputToVoltage((double)settings["end"]);
			// all other cases, do nothing
        }

        public override void AcquisitionFinished()
        {
            if (!Blocked()) rampOutputToVoltage(0);
            outputTask.Dispose();
        }

        [XmlIgnore]
        public override double ScanParameter
        {
            set
            {
                scanParameter = value;
                if (!Blocked())
                {
                    
                    if (!Environs.Debug) writer.WriteSingleSample(true, value);
                    hardwareControl.LaserFrequencyControlVoltage = value;
                }
            }
            get { return scanParameter; }
        }

        private void rampOutputToVoltage(double voltage)
        {
            for (int i = 1; i <= (int)settings["rampSteps"]; i++)
            {
                if (!Environs.Debug) writer.WriteSingleSample(true,
                                         scanParameter - (i * (scanParameter - voltage) / (int)settings["rampSteps"]));
                Thread.Sleep((int)settings["rampDelay"]);
            }
            scanParameter = voltage;
            hardwareControl.LaserFrequencyControlVoltage = voltage;
        }

        private bool Blocked()
        {
            return hardwareControl.GetAnalogOutputBlockedStatus((string)settings["channel"]);
        }
    }
}

#endif