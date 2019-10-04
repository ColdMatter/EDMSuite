using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

namespace ScanMaster.Acquire.Patterns
{
	/// <summary>
	/// This pattern simply flashes the YAG flashlamps. The q-switch and valve are
	/// not triggered. It is used to keep the laser warm whilst doing other stuff.
	/// </summary>
	public class FlashlampPatternBuilder : PatternBuilder32 
	{
		private const int FLASH_PULSE_LENGTH = 100;
		
		public int ShotSequence( int startTime, int sequenceLength, int flashlampPulseInterval,
									int valveToQ, int flashToQ ) 
		{
			int time = startTime;
			for (int i = 0 ; i < sequenceLength ; i++ ) 
			{
				Shot( time, valveToQ, flashToQ );
				time += flashlampPulseInterval;
			}
		
			return time;
		}

		public int Shot( int startTime, int valveToQ, int flashToQ )  
		{
			return Pulse(startTime, valveToQ - flashToQ, FLASH_PULSE_LENGTH,
					((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash"]).BitNumber);
		}
	

	}
		

}
