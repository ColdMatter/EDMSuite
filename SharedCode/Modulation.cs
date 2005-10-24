using System;

namespace EDMConfig
{
	/// <summary>
	/// Modulations are the heart of the edm data acquistion. All BlockHead does is modulate
	/// a bunch of channels and record the signal each time.
	/// 
	/// Modulation is the base class for all of the modulations. There are more specialised
	/// subclasses for particular types of modulations (like analog, digital).
	/// 
	/// Remember that the type of modulation is primarily an analysis thing and doesn't
	/// necessarily correspond to the lab implementation of the modulation.
	/// </summary>
	[Serializable]
	public class Modulation
	{
		public String Name;
		public Waveform Waveform;
		public int DelayAfterSwitch;
		public int DelayBeforeSwitch;
	}
}
