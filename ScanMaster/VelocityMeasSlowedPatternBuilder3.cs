using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

namespace ScanMaster.Acquire.Patterns
{
	/// <summary>
	/// See the documentation for the PumpProbePatternPlugin
	/// </summary>
	public class VelocityMeasSlowedPatternBuilder : DAQ.Pattern.PatternBuilder32
	{
		public VelocityMeasSlowedPatternBuilder()
		{
		}

		//	private const int FLASH_PULSE_LENGTH = 100;
		private const int Q_PULSE_LENGTH = 15;
		private const int DETECTOR_TRIGGER_LENGTH = 20;

		public int ShotSequence(int startTime, int shots, int padStart, int flashlampPulseInterval,
			int valvePulseLength, int valveToQ, int flashToQ, int flashlampPulseLength, int ShutterPulseLength, int delayToDetectorTrigger,
			int ttlSwitchPort, int ttlSwitchLine, int switchLineDuration, int Probe1ShutterDelay,int Probe2ShutterDelay, int shutter1offdelay, int shutterslowdelay, 
			int shutterV1delay, int shutterV2delay, int DurationV0, int DurationV1, int DurationV2, bool modulation)
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


			for (int i = 0; i <shots; i++)
			{

				///// Generate the TTLs sequence for channels ttl1 to tll6 /////
				/// (the pulse is generated only if ttlRepetitions==ShotIndex) Things it does every shot
				


				///// A pulse that is high 2 shots, low 2 shots /////
				/// This allows to easily use this plugin for detecting with 2ccds alternatively
				if (i % 4 == 0)
				{
					int switchChannel = PatternBuilder32.ChannelFromNIPort(ttlSwitchPort, ttlSwitchLine);
					Pulse(time, valveToQ + Probe1ShutterDelay - padStart, ShutterPulseLength, switchChannel); // This is just a digital output ttl. We'll use this as the trigger for one of the newport shutters
					Pulse(time, shutter1offdelay - padStart, ShutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter1off"]).BitNumber); // The newport control box needs a ttl to turn on shutter
					Pulse(time, shutter1offdelay+Probe1ShutterDelay+Probe2ShutterDelay+valveToQ-padStart, shutter1offdelay, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow"]).BitNumber); // Probe shutter (one of the newport ones but with a different control box so doesn't need an off pulse)
				}

				///// ON SHOTS /////
				if (i % 2 == 0)
				{
					// first the pulse with the switch line high
					Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detector");// This triggers data aquisition in shot plugin. This is On Shot

					// The shutter trigger for all even 
					Pulse(time, shutterslowdelay - padStart - 4000, DurationV0 + 4000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow2"]).BitNumber); // Uniblitz shutter for v=0 control
					Pulse(time, shutterslowdelay - padStart - 3000, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber); // This is the AOM off pulse
					Pulse(time, shutterslowdelay - padStart + DurationV0, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber); //Two AOM off pulses as the AOM's default state is on.
					Pulse(time, shutterV1delay - padStart, DurationV1, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv1"]).BitNumber); // This is just the shutter for v1
					Pulse(time, shutterV2delay - padStart, DurationV2, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv2"]).BitNumber); // This is just the shutter for v2
				}


				///// OFF SHOTS /////
				if (i % 2 == 1)
				{
					// The laser triggers
					Pulse(time, shutterV1delay - padStart, DurationV1, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv1"]).BitNumber); // This is just the shutter for v1
					Pulse(time, shutterV2delay - padStart, DurationV2, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv2"]).BitNumber); // This is just the shutter for v2

					// now with the switch line low, if modulation is true (otherwise another with line high)
					if (modulation)
					{
						Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detectorprime");
					}
					else
					{
						Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detector");// This triggers data aquisition in shot plugin. This is On Shot
						Pulse(time, shutterslowdelay - padStart - 4000, DurationV0 + 4000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow2"]).BitNumber); // Uniblitz shutter for v=0 control
						Pulse(time, shutterslowdelay - padStart - 3000, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber); // This is the AOM off pulse
						Pulse(time, shutterslowdelay - padStart + DurationV0, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber); //Two AOM off pulses as the AOM's default state is on.
					}
				}

				time += flashlampPulseInterval;
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
