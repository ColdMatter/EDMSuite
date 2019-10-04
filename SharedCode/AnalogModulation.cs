using System;

namespace EDMConfig
{
	/// <summary>
	/// An analog modulation - it modulates between two analog values (represented as
	/// a centre and a step).
    /// 
    /// Remember that the type of modulation is primarily an analysis thing and doesn't
    /// necessarily correspond to the lab implementation of the modulation.
    ///
    /// Two types of values can be stored: the control values and the corresponding "physical"
    /// values. Sometimes it's interesting to know what was sent into a box, sometimes what
    /// comes out! (i.e. a VCO might have a control voltage - the CV is recorded in the control
    /// parameters, and the output frequency in the physical parameters).
    /// 
    /// Really this is a bit of a hack. These values should be stored in the SwitchedChannels, 
    /// but they are not serialized as part of the block.
    /// 
    /// By convention, if an analog modulation is
    /// switched by an analog channel then the control values should be stored in Centre and Step and
    /// the corresponding physical values, if available, should be stored in the physical parameters.
    /// If the modulation is switched by a digital switch then the physical values should be stored in
    /// both the control and physical parameters. This is confusing, but it's the only way I can think
    /// of to add the ability to store physical and control values without breaking backward
    /// compatibility.
    /// 
	/// </summary>
	[Serializable]
	public class AnalogModulation : Modulation
	{
        // these are the physical values
		public double Centre;
		public double Step;
        // and these are the control values.
        public double PhysicalCentre;
        public double PhysicalStep;
	}
}
