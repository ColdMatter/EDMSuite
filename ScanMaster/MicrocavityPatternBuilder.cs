using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

namespace ScanMaster.Acquire.Patterns
{
	/// <summary>
	/// 
	/// </summary>
	public class MicrocavityPatternBuilder : PatternBuilder32
	{
		private const int DETECTOR_TRIGGER_LENGTH = 1;
        	
		public int ShotSequence( int startTime, int numberOfOnOffShots, int delayToDetectorTrigger) 
		{
		
			int time = startTime;
            	
			for (int i = 0 ; i < numberOfOnOffShots ; i++ ) 
			{
				Shot(time, delayToDetectorTrigger);
			}
		
			return time;
		}

		public int Shot( int startTime, int delayToDetectorTrigger)  
		{
			int time = 0;
			int tempTime = 0;

			// Detector trigger
			tempTime = Pulse(startTime, delayToDetectorTrigger, DETECTOR_TRIGGER_LENGTH,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["AcqTriggerOut"]).BitNumber);
			if (tempTime > time) time = tempTime;
		
			return time;
		}
		
	}
}
