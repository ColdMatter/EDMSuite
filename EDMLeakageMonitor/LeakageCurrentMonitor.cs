//using System;
//using NationalInstruments.DAQmx;
//
//namespace EDMLeakageMonitor
//{
//	/// <summary>
//	/// Contains methods for interfacing with a home-built leakage current monitor. The current monitor
//	/// provides a frequency output that is proportional to the measured current.
//	/// The measurement method is 'low frequency single counter'. The frequency is measured by routing a high
//	/// frequency internal timebase to the source pin of the counter, and gating the counter with the signal
//	/// to be measured (which must be a much lower frequency than the internal timebase).
//	/// </summary>
//	public class LeakageCurrentMonitor
//	{
//		private Task countingTask = null;
//		private CounterReader counterReader;
//		
//		// Settings
//		private int minimumFrequency = 10;
//		private int maximumFrequency = 50000;
//		private int maxCurrent = 500;
//		private int timeout = 200;
//		private int offset = 5000;
//		private int cal = 100;
//		private int samplesPerPoint = 10;
//		private bool rollover = false; // should be assigned to true if the counter overflows
//		private bool debug = true;
//
//		public LeakageCurrentMonitor(String counter)
//		{
//			this.samplesPerPoint = samplesPerPoint;
//			countingTask = new Task("count" + id.ToString());
//			ch = countingTask.CIChannels.CreateFrequencyChannel(counter, "Frequency", minimumFrequency, maximumFrequency, CIFrequencyStartingEdge.Rising, CIFrequencyMeasurementMethod.LowFrequencyOneCounter, 0.001, 4, CIFrequencyUnits.Hertz);
//			countingTask.Timing.ConfigureImplicit(SampleQuantityMode.FiniteSamples, samplesPerPoint);
//			countingTask.Stream.Timeout = timeout;
//			counterReader = new CounterReader(countingTask.Stream);
//		}
//
//		public void Dispose()
//		{
//			if (countingTask != null) countingTask.Dispose();
//		}
//
//		
//		/// <summary>
//		/// Reads the value of the current
//		/// </summary>
//		/// <returns>the current</returns>
//		public double Read()
//		{
//			double[] results;
//			double av = 0;
//
//			try
//			{
//				countingTask.Start();
//				results = counterReader.ReadMultiSampleDouble(samplesPerPoint);
//				if (ch.TerminalCountReached) rollover = true;
//				countingTask.Stop();
//					
//				if (!rollover)
//				{
//					// average the samples, excluding the first sample which will be bogus (since there is no synchronized trigger)
//					av = 0;
//					for (int i = 1; i < results.Length; i++) av = av + results[i];
//					av = av / (results.Length - 1);
//				}
//				else
//				{
//					rollover = false;
//					if (debug) Console.WriteLine("Counter 1 rolled over");
//				}
//			}
//			catch (DaqException de) 
//			{
//				av = 0.0;
//				countingTask.Stop();
//				if (debug) Console.WriteLine(de.Message);
//			}
//			double ans = cal*(av - offset)/1000;
//			if (ans < maxCurrent) return ans; //prevent ammeters from returning nonsensical results
//			else return maxCurrent;
//		}
//		
//	}
//}
