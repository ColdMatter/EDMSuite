using System;
using System.Threading;

using DAQ.Pattern.Test;
using DAQ.HAL;

using ScanMaster.GUI;

namespace ScanMaster.Acquire.Test
{
	/// <summary>
	/// Test the pattern generator output. You might want to disconnect anything that
	/// you value from the pattern generator before running this :-)
	/// </summary>
	public class PatternGenTest
	{
//		private int sequenceLength = 1;
//		private int shotEvery = 10000;
//
//		MainWindow m;
//
		public PatternGenTest(ControllerWindow m)
		{
	//		this.m = m;
		}

		public void Start()
		{
//			// grab the output object
//			AxCWDAQControlsLib.AxCWDO pgDO = m.axCWDO1;
//
//			// make a test pattern
//			SupersonicTestPattern p = new SupersonicTestPattern();
//			Console.WriteLine("Building pattern ...");
//			p.Clear();
//			p.ShotSequence(0,sequenceLength,shotEvery,1000,500,235,750,100,2000);
//			p.BuildPattern(sequenceLength * shotEvery);
//			Console.WriteLine("Done ...");
//
//			// configure the pattern board
//			pgDO.Device = 1;
//			pgDO.ChannelString="0:3";
//			pgDO.NPatterns = sequenceLength * shotEvery;
//			pgDO.UpdateClock.Frequency = 1000000;
//			pgDO.Continuous = true;
//			pgDO.AllowRegeneration = true;
//			pgDO.UpdateClock.ClockSourceType = CWDAQControlsLib.CWDIOClockSources.cwdioCSInternalClock;
//			pgDO.Configure();
//			Console.WriteLine(pgDO.NPorts);
//			
//			// start output
//			pgDO.Write(p.PatternAsInt16s, -1);
//			pgDO.Start();
//			Console.WriteLine("Outputting pattern");
//			Thread.Sleep(1000);
//			
//			// update the pattern
//			for (int i = 0 ; i < 50 ; i++)
//			{
//				Thread.Sleep(1);
//				p.Clear();
//				p.ShotSequence(0,sequenceLength,shotEvery,1000 + 50 * i,500,235,750,100,2000);
//				p.BuildPattern(sequenceLength * shotEvery);
//				pgDO.Write(p.PatternAsInt16s, -1);
//			}
//			
//			// stop output
//			Console.WriteLine("Pattern gen finished");
//			pgDO.Reset();
		}
	}
}
