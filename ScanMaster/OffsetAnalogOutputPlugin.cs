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
	/// An plugin to scan an analog output on an E-series board. This moves the scan to the start of the scan and 
    /// fires a TTL to hold the microcavity locking output in place. 
	/// </summary>
	[Serializable]
	public class OffsetAnalogOutputPlugin : ScanOutputPlugin
	{
		
		[NonSerialized]
		private double scanParameter = 0;

		[NonSerialized]
		private Task outputTask;
        [NonSerialized]
        private Task sampleHoldTriggerTask;
		[NonSerialized]
		private AnalogSingleChannelWriter writer;
        [NonSerialized]
        private DigitalSingleChannelWriter triggerWriter;

		protected override void InitialiseSettings()
		{
			settings["channel"] = "laser";
			settings["rampToZero"] = true;
            settings["flyback"] = "normal";
			settings["rampSteps"] = 50;
			settings["rampDelay"] = 20;
            settings["overshootVoltage"] = 0.0;
            settings["scanPause"] = 10000000;
            settings["lockReturnFactor"] = -0.01;
		}


		public override void AcquisitionStarting()
		{
			// initialise the output hardware
			outputTask = new Task("analog output");
			if (!Environs.Debug)
			{
				AnalogOutputChannel oc = 
					(AnalogOutputChannel) Environs.Hardware.AnalogOutputChannels[(string)settings["channel"]];
				oc.AddToTask(outputTask, oc.RangeLow, oc.RangeHigh);
				writer = new AnalogSingleChannelWriter(outputTask.Stream);
				writer.WriteSingleSample(true, 0);
			}

            //verify the task
            outputTask.Control(TaskAction.Verify);
            
            sampleHoldTriggerTask = new Task("sampleHold Trigger");
            if (!Environs.Debug)
            {
                DigitalOutputChannel tc =
                    (DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["sampleAndHoldTriggerOut"];
                tc.AddToTask(sampleHoldTriggerTask);
                triggerWriter = new DigitalSingleChannelWriter(sampleHoldTriggerTask.Stream);
            }
			scanParameter = 0;

            //verify
            sampleHoldTriggerTask.Control(TaskAction.Verify);
			
		}

		public override void ScanStarting()
		{
            //TTL pulse to sample and hold
            triggerWriter.WriteSingleSampleSingleLine(true, true);

            //start output task
            outputTask.Start();
            
            //go gently to the correct start position
            if ((string)settings["scanMode"] == "up" || (string)settings["scanMode"] == "updown")
            {
                if ((string)settings["flyback"] == "overshoot") rampOutputToVoltage((double)settings["overshootVoltage"]);
                rampOutputToVoltage((double)settings["start"]);
            }
            if ((string)settings["scanMode"] == "down" || (string)settings["scanMode"] == "downup")
            {
                if ((string)settings["flyback"] == "overshoot") rampOutputToVoltage((double)settings["overshootVoltage"]);
                rampOutputToVoltage((double)settings["end"]);
            }
		}

		public override void ScanFinished()
		{
            rampOutputToVoltage(((double)settings["end"]-(double)settings["start"])*(double)settings["lockReturnFactor"]);
            outputTask.Stop();

            //pause to let lock loop sense new value
            Thread.Sleep(3000);

            //Set sample and hold TTL to 0
            triggerWriter.WriteSingleSampleSingleLine(true, false);

            //pause to let microcavity lock settle
            Thread.Sleep(((int)settings["scanPause"]) / 1000);
		}

		public override void AcquisitionFinished()
		{
			rampOutputToVoltage(0);
            triggerWriter.WriteSingleSampleSingleLine(true, false);
			outputTask.Dispose();
            sampleHoldTriggerTask.Dispose();
		}
		
		[XmlIgnore]
		public override double ScanParameter
		{
			set
			{
                scanParameter = value;
                if (!Environs.Debug) writer.WriteSingleSample(true, value);
			}
			get { return scanParameter; }
		}

		private void rampOutputToVoltage(double voltage)
		{
			for (int i = 1 ; i <= (int)settings["rampSteps"] ; i++ ) 
			{
				if (!Environs.Debug) writer.WriteSingleSample(true,
										 scanParameter - (i*(scanParameter - voltage) / (int)settings["rampSteps"]));
				Thread.Sleep( (int)settings["rampDelay"] );
			}
			scanParameter = voltage;
		}

        
		
	}
}
