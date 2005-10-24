using System;

namespace DAQ.Pattern.Test
{
	public class SupersonicTestPattern : PatternBuilder32 
	{
	
		private int valveChan = PatternBuilder32.ChannelFromNIPort(0,6);
		private int flashChan = PatternBuilder32.ChannelFromNIPort(0,5);
		private int qChan = PatternBuilder32.ChannelFromNIPort(0,2);	
		private int pumpChan = PatternBuilder32.ChannelFromNIPort(0,4);
		private int detectorTrigChan = PatternBuilder32.ChannelFromNIPort(0,3);
	
	
		public int ShotSequence( int startTime, int sequenceLength, int shotEvery, int valvePulseLength,
			int delayToQ, int flashToQ, int delayToPump, int pumpPulseLength, 
			int delayToDetection) 
		{
		
			// HACK: this fixes a bug in triggered pattern gen, where the first line is 
			// output before the trigger arrives. Needs to be improved.
			int time = 1;
		
			for (int i = 0 ; i < sequenceLength ; i++ ) 
			{
				Shot( time, valvePulseLength, delayToQ, flashToQ, delayToPump, pumpPulseLength, delayToDetection );
				time += shotEvery;
			}
		
			return time;
		}

		public int Shot( int startTime, int valvePulseLength, int delayToQ, int flashToQ, int delayToPump,
			int pumpPulseLength, int delayToDetection)  
		{
			int time = 0;
			int tempTime = 0;
			// valve pulse
//			tempTime = Pulse(startTime, 0, valvePulseLength, valveChan);
			for (int z = 0 ; z < 32 ; z++) tempTime = Pulse(startTime, delayToDetection + delayToQ, valvePulseLength, z);
			if (tempTime > time) time = tempTime;
			// Flash pulse
//			tempTime = Pulse(startTime, delayToQ - flashToQ, 20, flashChan);
			if (tempTime > time) time = tempTime;
			// Q pulse
//			tempTime = Pulse(startTime, delayToQ, 20, qChan);
			if (tempTime > time) time = tempTime;
			// Pump beam trigger
//			tempTime = Pulse(startTime, delayToPump + delayToQ, pumpPulseLength, pumpChan);
			if (tempTime > time) time = tempTime;
			// Detector trigger
//			tempTime = Pulse(startTime, delayToDetection + delayToQ, 20, detectorTrigChan);
			if (tempTime > time) time = tempTime;
		
			return time;
		}
	

	}
		

}
