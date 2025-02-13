using System;
using System.Threading;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;

using NationalInstruments.DAQmx;

using DAQ.Environment;
using DAQ.FakeData;
using DAQ.HAL;
using Data;
using ScanMaster.Acquire.Plugin;
using System.Net.Sockets;
using System.Net;

namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A plugin to capture time of flight data by sampling an analog input
	/// on an E-series board. Deals with the case where shots are to be gathered
	/// synchronously with something that switches. This is done by making use
	/// of both detector trigger inputs on the board. The pattern that modulates 
	/// the switched channel, should also modulate the trigger channel so that this
	/// shot gatherer has the right behaviour.
	/// </summary>
	[Serializable]
	public class CCDModulatedAnalogShotGathererPlugin : ShotGathererPlugin
	{
	
		[NonSerialized]
		private Task inputTask1;
		[NonSerialized]
		private Task inputTask2;
		[NonSerialized]
		private Task counterTask1;
		[NonSerialized]
		private AnalogMultiChannelReader reader1;
		[NonSerialized]
		private AnalogMultiChannelReader reader2;

		[NonSerialized]
		private CounterSingleChannelWriter counter1;
		[NonSerialized]
        private double[,] latestData;

		// add ccd function
        [NonSerialized]
        private csAcq4.CCDController controller;


        protected override void InitialiseBaseSettings()
		{
			settings["gateStartTime"] = 600;
			settings["gateLength"] = 12000;
			settings["ccdEnableStartTime"] = 600;
			settings["ccdEnableLength"] = 10000;
			settings["clockPeriod"] = 1;
			settings["sampleRate"] = 100000;
			settings["channel"] = "detectorA,detectorB";
			settings["cameraChannel"] = "cameraEnabler";
			settings["inputRangeLow"] = -1.0;
			settings["inputRangeHigh"] = 10.0;
		}

		protected override void InitialiseSettings()
		{
            //Set up TCP
            formMain = (csAcq4.FormMain)(Activator.GetObject(typeof(csAcq4.FormMain), "tcp://" + "ULTRACOLDEDM" + ":" + 5555 + "/formMain.rem"));
        }

        public override void AcquisitionStarting()
		{
			//formMain.something(); add the CCD command we want to action here

			// configure the analog input
			inputTask1 = new Task("analog gatherer 1 -" /*+ (string)settings["channel"]*/);
			inputTask2 = new Task("analog gatherer 2 -" /*+ (string)settings["channel"]*/);
			counterTask1 = new Task("CCD enable Task Counter");

			// new analog channel, range -10 to 10 volts
			if (!Environs.Debug)
			{

				string channelList = (string)settings["channel"];
				string[] channels = channelList.Split(new char[] { ',' });

				string camChannel = (string)settings["cameraChannel"];

				foreach (string channel in channels)
				{
					((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
						inputTask1,
						(double)settings["inputRangeLow"],
						(double)settings["inputRangeHigh"]
						);
					((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
					inputTask2,
					(double)settings["inputRangeLow"],
					(double)settings["inputRangeHigh"]
					);
				}

				CounterChannel pulseChannel = ((CounterChannel)Environs.Hardware.CounterChannels[camChannel]);
                // CCD enable TTL pulse
                counterTask1.COChannels.CreatePulseChannelTicks(
					pulseChannel.PhysicalChannel,
					pulseChannel.Name,
					"20MHzTimebase",
					COPulseIdleState.Low,
					0,
					100,
					(20000000/(int)settings["sampleRate"]) * (int)settings["ccdEnableLength"]
					);
				
				//counterTask1.COChannels[0].PulseTerminal = "/DAQ_PXIe_6363/PFI12";
				


				// internal clock, finite acquisition
				inputTask1.Timing.ConfigureSampleClock(
					"",
					(int)settings["sampleRate"],
					SampleClockActiveEdge.Rising,
					SampleQuantityMode.FiniteSamples,
					(int)settings["gateLength"]);

				inputTask2.Timing.ConfigureSampleClock(
					"",
					(int)settings["sampleRate"],
					SampleClockActiveEdge.Rising,
					SampleQuantityMode.FiniteSamples,
					(int)settings["gateLength"]);

				
				counterTask1.Timing.ConfigureImplicit(SampleQuantityMode.FiniteSamples,1);


                // trigger off PFI0 (with the standard routing, that's the same as trig1)
                inputTask1.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
					(string)Environs.Hardware.GetInfo("analogTrigger0"),
					DigitalEdgeStartTriggerEdge.Rising);
				// trigger off PFI1 (with the standard routing, that's the same as trig2)
				inputTask2.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
					(string)Environs.Hardware.GetInfo("analogTrigger1"),
					DigitalEdgeStartTriggerEdge.Rising);				
				// trigger off PFI2 (with the standard routing, that's the same as trig1)
				counterTask1.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
					(string)Environs.Hardware.GetInfo("analogTrigger2"),
					DigitalEdgeStartTriggerEdge.Rising);


				inputTask1.Control(TaskAction.Verify);
				inputTask2.Control(TaskAction.Verify);
				counterTask1.Control(TaskAction.Verify);
			}


			reader1 = new AnalogMultiChannelReader(inputTask1.Stream);
			reader2 = new AnalogMultiChannelReader(inputTask2.Stream);
			counter1 = new CounterSingleChannelWriter(counterTask1.Stream);
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
			inputTask1.Dispose();
			inputTask2.Dispose();
			counterTask1.Dispose();
		}

        //public override void ArmAndWait()
        //{
        //	lock (this)
        //	{
        //		if (!Environs.Debug) 
        //		{
        //			if (config.switchPlugin.State == true)
        //			{
        //				counterTask1.Start();
        //				inputTask1.Start();
        //				latestData = reader1.ReadMultiSample((int)settings["gateLength"]);

        //				inputTask1.Stop();
        //				counterTask1.Stop();

        //			}
        //			else
        //			{
        //				counterTask1.Start();
        //				inputTask2.Start();
        //				latestData = reader2.ReadMultiSample((int)settings["gateLength"]);
        //				inputTask2.Stop();
        //				counterTask1.Stop();
        //			}
        //		}
        //	}
        //}

        private ManualResetEventSlim cameraStarted = new ManualResetEventSlim(false);
        private ManualResetEventSlim cameraFinished = new ManualResetEventSlim(false);

        public override void ArmAndWait()
        {
            lock (this)
            {
                if (!Environs.Debug)
                {
                   
                    Console.WriteLine("Waiting for camera to start capturing...");
                    cameraStarted.Wait();  // Wait until the camera signals frame capture start

                    if (config.switchPlugin.State == true)
                    {
                        counterTask1.Start(); // Start camera trigger
                        inputTask1.Start();
                        latestData = reader1.ReadMultiSample((int)settings["gateLength"]);
                        inputTask1.Stop();
                    }
                    else
                    {
                        counterTask1.Start(); // Start camera trigger
                        inputTask2.Start();
                        latestData = reader2.ReadMultiSample((int)settings["gateLength"]);
                        inputTask2.Stop();
                    }

                    Console.WriteLine("Waiting for camera to finish capturing...");
                    cameraFinished.Wait();  // Wait for the camera to confirm frame completion

                    counterTask1.Stop();
                    cameraStarted.Reset();
                    cameraFinished.Reset();
                    Console.WriteLine("Camera handshake complete, system rearmed.");
                }
            }
        }


        public override Shot Shot
		{
			get 
			{
				lock(this)
				{
					Shot s = new Shot();
					s.SetTimeStamp();
					if (!Environs.Debug)
					{
						for (int i = 0 ; i < inputTask1.AIChannels.Count ; i++)
						{
							TOF t = new TOF();
							t.ClockPeriod = (int)settings["clockPeriod"];
                            t.GateStartTime = (int)settings["gateStartTime"];
							double[] tmp = new double[(int)settings["gateLength"]];
							for (int j = 0 ; j < (int)settings["gateLength"] ; j++)
								tmp[j] = latestData[i,j];
							t.Data = tmp;
							s.TOFs.Add(t);
						}
						return s;
					}
					else 
					{
						Thread.Sleep(50);
						return DataFaker.GetFakeShot((int)settings["gateStartTime"], (int)settings["gateLength"],
							(int)settings["clockPeriod"], 1, 1);
					}
				}
			}
		}
	}
}
