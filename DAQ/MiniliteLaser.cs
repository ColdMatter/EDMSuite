using System;

namespace DAQ.HAL
{
	/// <summary>
	/// A class that could be used for communicating with a Minilite laser.
	/// </summary>
	public class MiniliteLaser: DAQ.HAL.YAGLaser
	{
		public void StartFlashlamps(bool internalClock)
		{
		}

		public void StopFlashlamps()
		{
		}

		public void EnableQSwitch()
		{}

		public void DisableQSwitch()
		{
		}

		public void PatternChangeStartingHandler(object source, EventArgs e)
		{}

		public void PatternChangeEndedHandler(object source, EventArgs e)
		{}


	}
}
