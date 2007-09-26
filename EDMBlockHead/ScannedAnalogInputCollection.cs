using System;
using System.Collections;

namespace EDMBlockHead.Acquire.Input
{
	/// <summary>
	/// This class holds configuration information for the scanned analog input. The parameters
	/// in here control the acual physical scan. The data is then divided up according to the
	/// parameters in the individual ScannedAnalogInput channels.
	/// </summary>
	public class ScannedAnalogInputCollection
	{
		public int RawSampleRate;
		public int GateStartTime;
		public int GateLength;

		public ArrayList Channels = new ArrayList();
	}
}
