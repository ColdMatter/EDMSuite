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
	public class YAGFirePatternBuilder : PatternBuilder32 
	{
		private const int FLASH_PULSE_LENGTH = 100;
		private const int Q_PULSE_LENGTH = 100;
		
		public int ShotSequence( int startTime, int sequenceLength, int flashlampPulseInterval,
								 int flashToQ ) 
		{
			int time = startTime;
			for (int i = 0 ; i < sequenceLength ; i++ ) 
			{
				Shot( time, flashToQ );
				time += flashlampPulseInterval;
			}
		
			return time;
		}

		public int Shot( int startTime, int flashToQ )  
		{
			int time1 = Pulse(startTime, 0, FLASH_PULSE_LENGTH,
								((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash"]).BitNumber);

			int time2 = Pulse(startTime, flashToQ, Q_PULSE_LENGTH,
								((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["q"]).BitNumber);
			if (time2 > time1) return time2;
			return time1;

		}
	

	}
		

}
