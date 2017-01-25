using System;
using System.Threading;

using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;
using System.Collections.Generic;

namespace DAQ.TransferCavityLock2012
{
	public class DAQMxTCL2012ExtTriggeredMultiReadHelperFake : TransferCavity2012Lockable
	{
		//In this helper, the cavity is scanned externally (not from the computer). The software waits for a trigger pulse to start scanning.
		//An additional AI read is added to this helper for reading off the cavity voltage.
		//This helper doesn't deal with locking the laser.

		private string[] analogInputs;
		private string trigger;

		private Task readAIsTask;
		private Dictionary<string, AnalogInputChannel> channels;

		private AnalogMultiChannelReader analogReader;


		public DAQMxTCL2012ExtTriggeredMultiReadHelperFake(string[] inputs)
		{
			this.analogInputs = inputs;
		}


		#region Methods for configuring the hardware

		//The photodiode inputs have been bundled into one task. We never read one photodiode without reading
		//the other.
		public void ConfigureReadAI(int numberOfMeasurements, double sampleRate, bool autostart) //AND CAVITY VOLTAGE!!! 
		{
			readAIsTask = new Task();

			channels = new Dictionary<string, AnalogInputChannel>();
			foreach (string s in analogInputs)
			{
				AnalogInputChannel channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels[s];
				channels.Add(s, channel);
			}

/*
			foreach (KeyValuePair<string, AnalogInputChannel> pair in channels)
			{
				pair.Value.AddToTask(readAIsTask, 0, 10);
			}

			if (autostart == false)
			{
				readAIsTask.Timing.ConfigureSampleClock(
				   "",
				   sampleRate,
				   SampleClockActiveEdge.Rising,
				   SampleQuantityMode.FiniteSamples, numberOfMeasurements);

				readAIsTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
					trigger,
					DigitalEdgeStartTriggerEdge.Rising);
			}
//			readAIsTask.Control(TaskAction.Verify);
//			analogReader = new AnalogMultiChannelReader(readAIsTask.Stream);
 */ 
		}


		#endregion

		#region Methods for controlling hardware

		public double[,] ReadAI(int numberOfMeasurements)
		{
			double N = (double)numberOfMeasurements;
			double[,] data = new double[analogInputs.Length, numberOfMeasurements];//Cheezy Bugfix

			for (int channel = 0; channel < analogInputs.Length; channel++)
			{
				for (int n = 0; n < numberOfMeasurements; n++)
				{
					switch (analogInputs[channel])
					{
						case "master":							// reference laser
							data[channel, n] = Lorentzian(n, N / 2, N / 10);
							break;
						case "cavity":							// cavity ramp monitor channel
							data[channel, n] = n/N;
							break;
						default:								// anything else
							data[channel, n] = Lorentzian(n, N / 3, N / 10);
							break;
					}
				}
			}

			return data;
		}

		private double Lorentzian(double x, double xo, double Gamma)
		{
			return (Gamma/2) / ((x - xo)*(x-xo) + (Gamma/2)*(Gamma/2));
		}


		public void DisposeAITask()
		{
//			readAIsTask.Dispose();
		}

		#endregion

	}
}
