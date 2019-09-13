using System;

using NationalInstruments.Visa;
using NationalInstruments.ModularInstruments.NIRfsg;
using NationalInstruments.ModularInstruments.SystemServices.DeviceServices;

using DAQ.Environment;

using System.Windows.Forms;

namespace DAQ.HAL
{
	/// <summary>
	/// Represents a class to use the NI PXI-5670 arbitrary waveform generator.
    /// All PXI-5670-specific functions should go here, otherwise they should go into the parent class.
	/// </summary>
	public class NIPXI5670 : NIRfsgInstrument
	{
		public NIPXI5670(String address)
            : base(address)
		{
		}
	}
}
