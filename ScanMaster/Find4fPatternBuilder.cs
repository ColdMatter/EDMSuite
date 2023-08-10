using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

namespace ScanMaster.Acquire.Patterns
{
	/// <summary>
	/// See the documentation for the PumpProbePatternPlugin
	/// </summary>
	public class Find4fPatternBuilder : DAQ.Pattern.PatternBuilder32
	{
		public Find4fPatternBuilder()
		{
		}

		//	private const int FLASH_PULSE_LENGTH = 100;
		private const int Q_PULSE_LENGTH = 15;
		private const int DETECTOR_TRIGGER_LENGTH = 20;

		public int ShotSequence(int startTime, int shots, int padShots, int padStart, int flashlampPulseInterval,
			int valvePulseLength, int valveToQ, int flashToQ, int flashlampPulseLength, int shutterPulseLength, int delayToDetectorTrigger,
			int ttlSwitchPort, int ttlSwitchLine, int switchLineDuration, int DurationV3, int shutterV0delay, int DurationV0, 
			int shutterV1delay, int shutterV2delay, int DurationV2, int DurationV1, bool modulation,int switchLineDelay,int DurationIR,int v3delaytime)
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

					Pulse(time, valveToQ - switchLineDelay, shutterPulseLength, switchChannel); // This is just a digital output ttl. This is used for the opening pulse of the IR shutter. The newport ones need a pulse to turn on and one to turn off

					//v0stuff
					Pulse(time, shutterV0delay - padStart - 3000, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber);//this is the v0 think there something up with this one added time BQ - want shutter to open before the shot fires 9 what about valve to q
					Pulse(time, shutterV0delay - padStart + DurationV0, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber);
					Pulse(time, shutterV0delay - padStart - 4000, DurationV0 + 4000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow2"]).BitNumber);
					//Pulse(time, shutterV0delay - padStart, DurationV0, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber);//v0 trigger N.B: replace with aom trigger
					//v1
					Pulse(time, shutterV1delay - padStart, DurationV1, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv1"]).BitNumber);

					//v2
					Pulse(time, shutterV2delay - padStart, DurationV2, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv2"]).BitNumber);

					//v3
					Pulse(time, v3delaytime, DurationV3, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow"]).BitNumber);//this is v3

					//IR shutter
					Pulse(time, DurationIR, shutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter1off"]).BitNumber); // this is the IR shutter

					Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detector");// how does this work in terms of time - it does though - time has been added from previosu


					time += flashlampPulseInterval;
					for (int p = 0; p < padShots; p++)
					{
						FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
						time += flashlampPulseInterval;
					}
				}
				else //else do a normal shot with the switch line high. e.f 0,2,4,6 it hits this one first
				{

					//v0stuff
					//Pulse(time, shutterV0delay - padStart - 3000, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber);//this is the v0 think there something up with this one added time BQ - want shutter to open before the shot fires 9 what about valve to q
					//Pulse(time, shutterV0delay - padStart + DurationV0, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber);
					//Pulse(time, shutterV0delay - padStart - 4000, DurationV0 + 4000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow2"]).BitNumber);
					//Pulse(time, shutterV0delay - padStart, DurationV0, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber);//v0 trigger N.B: replace with aom trigger

					//v1 
					//Pulse(time, shutterV1delay-padStart, DurationV1, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv1"]).BitNumber);//his is v1

					//v2
					//Pulse(time, shutterV2delay-padStart, DurationV2, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv2"]).BitNumber);//this is v2

					//v3
					//Pulse(time, v3delaytime, DurationV3, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow"]).BitNumber);//this is v3


					Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detectorprime");
					

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
