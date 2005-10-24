using System;
using System.Threading;

using NationalInstruments.DAQmx;

namespace DAQ.HAL
{
	/// <summary>
	/// A class to control the pattern generator using DAQMx. This class is not horrible, doesn't
	/// suffer from huge memory leaks, and doesn't frequently crash the computer. W00t.
	/// </summary>
	public class DAQMxPatternGenerator : PatternGenerator
	{
		private Task pgTask;
		private String device;
		private DigitalSingleChannelWriter writer;
		private double clockFrequency;
		private double realClockFrequency;
		private int length;

		private const double EXTERNAL_CLOCK_RATE = 1000000;

		public DAQMxPatternGenerator(String device)
		{
			this.device = device;
		}

		// use this method to output a pattern to the whole pattern generator
		public void OutputPattern(UInt32[] pattern)
		{
			writer.WriteMultiSamplePort(true, pattern);
			// This Sleep is important (or at least it may be). It's here to guarantee that the correct pattern is
			// being output by the time this call returns. This is needed to make the tweak
			// and pg scans work correctly. It has the side effect that you have to wait for
			// at least one copy of the pattern to output before you can do anything. This means
			// pg scans are slowed down by a factor of two. I can't think of a better way to do
			// it at the moment.
			// It might be possible to speed it up by understanding the timing of the above call
			// - when does it return ?
			SleepOnePattern();
		}

		// use this method to output a pattern to half of the pattern generator
		public void OutputPattern(Int16[] pattern)
		{
			writer.WriteMultiSamplePort(true, pattern);
			// see above
			SleepOnePattern();
		}
		
		private void SleepOnePattern()
		{
			int sleepTime = (int)((length * 1000) / realClockFrequency);
			Thread.Sleep(sleepTime);
		}

		public void Configure( double clockFrequency, bool loop, bool fullWidth, bool lowGroup, int length )
		{	
			this.clockFrequency = clockFrequency;
			this.length = length;

			pgTask = new Task("pgTask");

			// The underscore notation is the way to address more than 8 of the pattern generator
			// lines at once. This is really buried in the NI-DAQ documentation !
			String chanString = "";
			if (fullWidth) chanString = device + "/port0_32";
			else
			{
				if (lowGroup) chanString = device + "/port0_16";
				else chanString = device + "/port3_16";
			}

			DOChannel doChan = pgTask.DOChannels.CreateChannel(
				chanString,
				"pg",
				ChannelLineGrouping.OneChannelForAllLines
				);

			String clockSource;
			if (clockFrequency == -1) 
			{
				clockSource = device + "/PFI2";
				realClockFrequency = EXTERNAL_CLOCK_RATE;
			} 
			else
			{
				clockSource = "";
				realClockFrequency = clockFrequency;
			}
			SampleQuantityMode sqm;
			if (loop)
			{
				sqm = SampleQuantityMode.ContinuousSamples;
				pgTask.Stream.WriteRegenerationMode = WriteRegenerationMode.AllowRegeneration;
			}
			else
			{
				sqm = SampleQuantityMode.FiniteSamples;
				pgTask.Stream.WriteRegenerationMode = WriteRegenerationMode.DoNotAllowRegeneration;
			}

			pgTask.Timing.ConfigureSampleClock(
				clockSource,
				clockFrequency,
				SampleClockActiveEdge.Rising,
				sqm,
				length
				);
			
			// these lines are critical - without them DAQMx copies the data you provide
			// as many times as it can into the on board FIFO (the cited reason being stability).
			// This has the annoying side effect that you have to wait for the on board buffer
			// to stream out before you can update the patterns - this takes ~6 seconds at 1MHz.
			// These lines tell the board and the software to use buffers as close to the size of
			// the pattern as possible (on board buffer size is coerced to be related to a power of
			// two, so you don't quite get what you ask for).
			pgTask.Stream.Buffer.OutputBufferSize = length;
			pgTask.Stream.Buffer.OutputOnBoardBufferSize = length;
			pgTask.Control(TaskAction.Commit);

			writer = new DigitalSingleChannelWriter(pgTask.Stream);
		}
		
		public void StopPattern()
		{
			pgTask.Dispose();
		}
	}
}
