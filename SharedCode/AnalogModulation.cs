using System;

namespace EDMConfig
{
	/// <summary>
	/// An analog modulation - it modulates between two analog values (represented as
	/// a centre and a step).
	/// 
	/// Remember that the type of modulation is primarily an analysis thing and doesn't
	/// necessarily correspond to the lab implementation of the modulation.
	/// </summary>
	[Serializable]
	public class AnalogModulation : Modulation
	{
		public double Centre;
		public double Step;
	}
}
