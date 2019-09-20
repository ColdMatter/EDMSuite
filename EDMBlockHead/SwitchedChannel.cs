using System;

using EDMConfig;

namespace EDMBlockHead.Acquire.Channels
{
	/// <summary>
	/// This class is the base for all the switched channels. A switched channel is the mapping
	/// between a modulation and an actual physical output (be it ttl, analog, gpib etc).
	/// 
	/// Whereas a modulation describes the type of modulation being performed (i.e. analog,
	/// digital) the switched channel corresponds to how that modulation is performed. For
	/// example it's perfectly reasonable to have an analog modulation associated with a 
	/// TTLSwitchedChannel if the analog current is switched by a digitally driven box.
	/// </summary>
	public abstract class SwitchedChannel
	{
		public Modulation Modulation;

		public abstract bool State
		{
			set;
			get;
		}

		public abstract void AcquisitionStarting();

		public abstract void AcquisitionFinishing();

	}
}
