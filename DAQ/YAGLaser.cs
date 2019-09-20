using System;

namespace DAQ.HAL
{
	/// <summary>
	/// Interface to a YAG laser.
	/// </summary>
	public interface YAGLaser
	{
		void StartFlashlamps(bool internalClock);
		void StopFlashlamps();
		void EnableQSwitch();
		void DisableQSwitch();
	}
}
