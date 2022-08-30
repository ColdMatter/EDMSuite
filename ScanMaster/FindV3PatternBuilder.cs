using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

namespace ScanMaster.Acquire.Patterns
{
	/// <summary>
	/// See the documentation for the PumpProbePatternPlugin
	/// </summary>
	public class FindV3PatternBuilder : DAQ.Pattern.PatternBuilder32
	{
		public FindV3PatternBuilder()
		{
		}

		//	private const int FLASH_PULSE_LENGTH = 100;
		private const int Q_PULSE_LENGTH = 15;
		private const int DETECTOR_TRIGGER_LENGTH = 20;

		public int ShotSequence(int startTime, int shots, int padShots, int padStart, int flashlampPulseInterval,
			int valvePulseLength, int valveToQ, int flashToQ, int flashlampPulseLength, int ShutterPulseLength, int delayToDetectorTrigger,
			int ttlSwitchPort, int ttlSwitchLine, int switchLineDuration, int shutteroffdelay, int shutterslowdelay, int DurationV0, 
			int shutterV1delay, int shutterV2delay, int DurationV2, int DurationV1, bool modulation,int switchLineDelay,int shutter1offdelay,int v3delaytime)
		{
			int time;
			if (padStart == 0)
			{
				time = startTime;
			}
			else
			{
				time = startTime + padStart;
			}
			for (int i = 0; i < shots; i++)
			{
				// if we're on an odd number of shots and we're modulating the switch line do this:
				if (modulation & (i % 2 != 0))//e.g 1,3,5 etc
				{
					int switchChannel = PatternBuilder32.ChannelFromNIPort(ttlSwitchPort, ttlSwitchLine);
					// first the pulse with the switch line high
					//Pulse(time, valveToQ - switchLineDelay, ShutterPulseLength, switchChannel); // This is just a digital output ttl. We'll use this as the trigger for one of the shutters - 
					//Pulse(time, 0, shutteroffdelay, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow"]).BitNumber);//this line seems to not work

					Pulse(time, valveToQ - switchLineDelay, ShutterPulseLength, switchChannel); // This is just a digital output ttl. We'll use this as the trigger for one of the shutters - 
					Pulse(time, shutter1offdelay, ShutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter1off"]).BitNumber);

					//v0stuff
					Pulse(time, shutterslowdelay - padStart - 3000, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber);//v0 aom//turns it off 3ms before (for 3ms) v0 is on then off after v0 is pulse and this is the v0 think there something up with this one added time BQ - want shutter to open before the shot fires 9 what about valve to q
					Pulse(time, shutterslowdelay - padStart + DurationV0, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber);//v0 aom
					Pulse(time, shutterslowdelay - padStart - 4000, DurationV0 + 4000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow2"]).BitNumber);//remember shutter has a delay//v0shutter


					Pulse(time, shutterV1delay - padStart, DurationV1, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv1"]).BitNumber);
					Pulse(time, shutterV2delay - padStart, DurationV2, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv2"]).BitNumber);
					//Pulse(time, shutterslowdelay - padStart-3000, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber);//this is the v0 aom - high is off//think there something up with this one added time BQ - want shutter to open before the shot fires 9 what about valve to q
					//Pulse(time, shutterslowdelay - padStart+ DurationV0, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber);//the 1000 is to account for the delay in the shutter
					//Pulse(time, shutterslowdelay - padStart-4000, DurationV0+4000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow2"]).BitNumber);//turns on shutter for v0 2ms before and 2ms longer
					//Pulse(time, shutterV1delay - padStart, DurationV1, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv1"]).BitNumber);
					Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detector");// how does this work in terms of time - it does though - time has been added from previosu
					//Pulse(time, shutter1offdelay, ShutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter1off"]).BitNumber);
					//Pulse(time, shutterslowdelay, ShutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber);
					//Pulse(time, DurationV0 + shutterslowdelay, ShutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2off"]).BitNumber);

					time += flashlampPulseInterval;
					for (int p = 0; p < padShots; p++)
					{
						FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
						time += flashlampPulseInterval;
					}
				}
				else //else do a normal shot with the switch line high. e.f 0,2,4,6 it hits this one first
				{
					Pulse(time, v3delaytime, shutteroffdelay, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow"]).BitNumber);//this is v3 this line seems to not work
					
					//v0stuff
					Pulse(time, shutterslowdelay-padStart-3000, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber);//v0 aom//turns it off 3ms before (for 3ms) v0 is on then off after v0 is pulse and this is the v0 think there something up with this one added time BQ - want shutter to open before the shot fires 9 what about valve to q
					Pulse(time, shutterslowdelay - padStart+ DurationV0,3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber);//v0 aom
					Pulse(time, shutterslowdelay - padStart - 4000, DurationV0 + 4000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow2"]).BitNumber);//v0shutter



					Pulse(time, shutterV1delay-padStart, DurationV1, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv1"]).BitNumber);
					Pulse(time, shutterV2delay-padStart, DurationV2, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv2"]).BitNumber);
					Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detectorprime");
					//Pulse(time, ShutterslowPulseLength+shutterslowdelay-padStart, ShutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2off"]).BitNumber);//seems to turn off at the next pattern needs to be negative not sure why
					//Pulse(startTime, 0, shutteroffdelay, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow"]).BitNumber);//this line seems to not work

					time += flashlampPulseInterval;
					for (int p = 0; p < padShots; p++)
					{
						FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
						time += flashlampPulseInterval;
					}
					// now with the switch line low, if modulation is true (otherwise another with line high)
				}
		
			}

			return time;
		}

		public int FlashlampPulse(int startTime, int valveToQ, int flashToQ, int flashlampPulseLength)
		{
			return Pulse(startTime, valveToQ - flashToQ, flashlampPulseLength,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash"]).BitNumber);
		}

		public int Shot(int startTime, int valvePulseLength, int valveToQ, int flashToQ, int flashlampPulseLength, int delayToDetectorTrigger, string detectorTriggerSource)
		{
			int time = 0;
			int tempTime = 0;

			// valve pulse
			tempTime = Pulse(startTime, 0, valvePulseLength,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["valve"]).BitNumber);
			if (tempTime > time) time = tempTime;
			// Flash pulse
			tempTime = Pulse(startTime, valveToQ - flashToQ, flashlampPulseLength,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash"]).BitNumber);
			if (tempTime > time) time = tempTime;
			// Q pulse
			tempTime = Pulse(startTime, valveToQ, Q_PULSE_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["q"]).BitNumber);
			if (tempTime > time) time = tempTime;
			// Detector trigger
			tempTime = Pulse(startTime, delayToDetectorTrigger + valveToQ, DETECTOR_TRIGGER_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[detectorTriggerSource]).BitNumber);
			if (tempTime > time) time = tempTime;


			return time;
		}

	}
}
