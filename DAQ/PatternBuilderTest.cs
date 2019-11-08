using System;

namespace DAQ.Pattern.Test
{
	public class PatternBuilderTest
	{

		[STAThread]
		public static void Main() 
		{
			SupersonicTestPattern p = new SupersonicTestPattern();
			Console.WriteLine("Building pattern ...");
			p.Clear();
			p.ShotSequence(0,100,100000,350,235,500,750,100,15000);
			p.BuildPattern(p.GetMinimumLength());
			Console.WriteLine("Done ...");
			Console.WriteLine("Pattern length is " + p.GetMinimumLength() + " long words.");
			Console.WriteLine(p.LayoutToString());
			Console.WriteLine(p.ArrayToString());
		}

	}
}
