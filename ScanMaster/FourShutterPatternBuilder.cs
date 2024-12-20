using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

namespace ScanMaster.Acquire.Patterns
{
	/// <summary>
	/// See the documentation for the PumpProbePatternPlugin
	/// </summary>
	public class FourShutterPatternBuilder : DAQ.Pattern.PatternBuilder32
	{
		public FourShutterPatternBuilder()
		{
		}

		//	private const int FLASH_PULSE_LENGTH = 100;
		private const int Q_PULSE_LENGTH = 15;
		private const int DETECTOR_TRIGGER_LENGTH = 20;

		public int ShotSequence(int startTime, int shots, int padShots, int padStart, int flashlampPulseInterval,
			int valvePulseLength, int valveToQ, int flashToQ, int flashlampPulseLength, int ShutterPulseLength, int delayToDetectorTrigger,
			int ttlSwitchPort, int ttlSwitchLine, int switchLineDuration, int switchLineDelay, int shutteroffdelay, int shutter1offdelay, int shutterslowdelay, int ShutterslowPulseLength, 
			int shutterV1delay, int shutterV2delay, int V2OnTime, int DurationV1, bool modulation)
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
					Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detector");// how does this work in terms of time - it does though - time has been added from previosu
					//Pulse(time, shutter1offdelay, ShutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterSTEVEoff"]).BitNumber);
					//Pulse(time, shutterV1delay-padStart, V1OnTime, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv1/v2"]).BitNumber);
					//Pulse(time, shutterV2delay-padStart, V2OnTime, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv2"]).BitNumber);
					//Pulse(time, shutterslowdelay, ShutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber);
					//Pulse(time, ShutterslowPulseLength + shutterslowdelay, ShutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2off"]).BitNumber);

					time += flashlampPulseInterval;
					for (int p = 0; p < padShots; p++)
					{
						FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
						time += flashlampPulseInterval;
					}
				}
				else //else do a normal shot with the switch line high. e.f 0,2,4,6 it hits this one first
				{
					Pulse(time, 0, shutteroffdelay, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow"]).BitNumber);//this line seems to not work
					Pulse(time, shutterslowdelay-padStart, ShutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber);//think there something up with this one added time BQ - want shutter to open before the shot fires 9 what about valve to q
					Pulse(time, shutterV1delay-padStart, DurationV1, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv1/v2"]).BitNumber);
					Pulse(time, shutterV2delay-padStart, V2OnTime, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv2"]).BitNumber);
					Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detectorprime");
					Pulse(time, ShutterslowPulseLength+shutterslowdelay-padStart, ShutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2off"]).BitNumber);//seems to turn off at the next pattern needs to be negative not sure why
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
