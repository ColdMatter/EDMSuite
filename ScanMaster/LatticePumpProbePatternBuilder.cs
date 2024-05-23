using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

namespace ScanMaster.Acquire.Patterns
{
	/// <summary>
	/// See the documentation for the PumpProbePatternPlugin
	/// </summary>
	public class LatticePumpProbePatternBuilder : DAQ.Pattern.PatternBuilder32
	{
		public LatticePumpProbePatternBuilder()
		{
		}

    //	private const int FLASH_PULSE_LENGTH = 100;
		private const int Q_PULSE_LENGTH = 15;
		private const int DETECTOR_TRIGGER_LENGTH = 20;
	
		public int ShotSequence(int startTime, int shots, int padShots, int padStart, int flashlampPulseInterval,
			int valvePulseLength, int valveToQ, int flashToQ, int flashlampPulseLength, int shutterPulseLength, int delayToDetectorTrigger,
			int ttlSwitchPort, int ttlSwitchLine, int switchLineDuration, int shutteroffdelay, int shutterslowdelay, int DurationV0,
			int shutterV1delay, int shutterV2delay, int DurationV2, int DurationV1, bool modulation, int switchLineDelay, int shutter1offdelay, int v3delaytime, int repumpDuration, int repumpDelay) 
		{
			int padEnd = padStart;
			int time;
            if (padStart == 0)
            {
                time = startTime;
            }
            else
            {
                time = startTime + padStart;
            }
			for (int i = 0 ; i < shots ; i++ ) 
			{
				///ON shot open slowing beam

				//STEVE shutter
				int switchChannel = PatternBuilder32.ChannelFromNIPort(ttlSwitchPort, ttlSwitchLine);
				//int IRDelay = valveToQ - switchLineDelay - padStart; Usual trigger but outdated.
				//Steve1
				Pulse(time, repumpDelay + 560 + 160 - 2680 - padStart, shutterPulseLength, switchChannel); // This is used for the opening pulse of the STEVE shutter. The newport ones need a pulse to turn on and one to turn off
				Pulse(time, repumpDelay + 5000 - padStart + 13500, shutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterSTEVE1off"]).BitNumber); // this is the V1/V2 STEVE shutter
				//Steve2
				Pulse(time, repumpDelay + 560 - 2680 + repumpDuration - padStart, 21000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterSTEVE2"]).BitNumber);

				//V0 Slowing
				Pulse(time, shutterslowdelay - padStart - 11800 - 3000, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber); //V0 AOM
				Pulse(time, shutterslowdelay - padStart - 11800 + DurationV0, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber);//V0 AOM
				Pulse(time, shutterslowdelay - padStart - 11800 - 4000, DurationV0 + 4000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow2"]).BitNumber);//V0 Uniblitz Shutter. Which takes approximately 2ms (2000 us) to respond and up to 1ms to change state. 
																																																			 //V1 and V2
				//Pulse(time, shutterV1delay - padStart - 11800, DurationV1 - GlobalDurationCutoff, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv1/v2"]).BitNumber); //THorlabs shutter					
				//Pulse(time, shutterV2delay - padStart - 11800, DurationV2 - GlobalDurationCutoff, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv2"]).BitNumber); //V2 AOM (not used in practice)
																																														   //Camera
				//Pulse(time, CameraTrigger, shutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["camerashutter"]).BitNumber);//Guanchen added camera trigger for the molecule image
				//Pulse(time, BgTrigger, shutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["camerashutter"]).BitNumber); //Guanchen added camera trigger for the light background image
				Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detector");// how does this work in terms of time - it does though - time has been added from previosu



				time += flashlampPulseInterval;
				
				for (int p = 0 ; p < padShots ; p++)
				{
                    FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
					time += flashlampPulseInterval;
				}
				// now with the switch line low, if modulation is true (otherwise another with line high)
                if (modulation)
                {

					///OFF Shot closes Slowing beam


					//int switchChannel = PatternBuilder32.ChannelFromNIPort(ttlSwitchPort,ttlSwitchLine);
					// first the pulse with the switch line high
					//STEVE shutter
					//Pulse(time, valveToQ - switchLineDelay, shutterPulseLength, switchChannel); // This is just a digital output ttl. This is used for the opening pulse of the STEVE shutter. The newport ones need a pulse to turn on and one to turn off

					/*if (DurationIR != 0) //To not use shutter when not desired set duratiopn to 0
					{
						Pulse(time, DurationIR, shutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterSTEVEoff"]).BitNumber); // this is the V1/V2 STEVE shutter
					}*/
					//Pulse(time, v3delaytime, shutteroffdelay, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow"]).BitNumber);//this line seems to not work

					/*
	Pulse(time, shutterslowdelay - padStart - 3000, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber); //V0 AOM
	Pulse(time, shutterslowdelay - padStart + DurationV0, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber);//V0 AOM
	*/


					/*//Pulse(time, shutterV1delay - padStart, DurationV1, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv1/v2"]).BitNumber);
					//Pulse(time, shutterV2delay - padStart, DurationV2, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv2"]).BitNumber); //This is V2 aom, not used in practice
					*/

					//Pulse(time, CameraTrigger, shutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["camerashutter"]).BitNumber);//Guanchen added camera trigger for the molecule image
					//Pulse(time, BgTrigger, shutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["camerashutter"]).BitNumber); //Guanchen added camera trigger for the light background image

					Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detectorprime");



					time += flashlampPulseInterval;
                    for (int p = 0; p < padShots; p++)
                    {
                        FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
                        time += flashlampPulseInterval;
                    }
                }
                else
                {
					///ON shot open slowing beam
					//int IRDelay = valveToQ - switchLineDelay - padStart; Usual trigger but outdated.

					//Steve1
					Pulse(time, repumpDelay +160 + 560 - 2680 - padStart, shutterPulseLength, switchChannel); // This is used for the opening pulse of the STEVE shutter. The newport ones need a pulse to turn on and one to turn off
					Pulse(time, repumpDelay + 5000 - padStart + 13500, shutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterSTEVE1off"]).BitNumber); // this is the V1/V2 STEVE shutter
					//Steve2
					Pulse(time, repumpDelay + 560 - 2680 + repumpDuration - padStart, 21000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterSTEVE2"]).BitNumber);


					//V0 Slowing
					Pulse(time, shutterslowdelay - padStart - 11800 - 3000, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber); //V0 AOM
					Pulse(time, shutterslowdelay - padStart - 11800 + DurationV0, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber);//V0 AOM
					Pulse(time, shutterslowdelay - padStart - 11800 - 4000, DurationV0 + 4000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow2"]).BitNumber);//V0 Uniblitz Shutter. Which takes approximately 2ms (2000 us) to respond and up to 1ms to change state. 
																																																				 //V1 and V2
					//Pulse(time, shutterV1delay - padStart - 11800, DurationV1, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv1/v2"]).BitNumber); //THorlabs shutter					
					//Pulse(time, shutterV2delay - padStart - 11800, DurationV2, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv2"]).BitNumber); //V2 AOM (not used in practice)
																																															   //Camera
					//Pulse(time, CameraTrigger, shutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["camerashutter"]).BitNumber);//Guanchen added camera trigger for the molecule image
					//Pulse(time, BgTrigger, shutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["camerashutter"]).BitNumber); //Guanchen added camera trigger for the light background image
					Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detector");// how does this work in terms of time - it does though - time has been added from previosu


					//// Repeat OFF shot if there is no modulation.

					//int switchChannel = PatternBuilder32.ChannelFromNIPort(ttlSwitchPort,ttlSwitchLine);
					// first the pulse with the switch line high
					//STEVE shutter
					//Pulse(time, valveToQ - switchLineDelay, shutterPulseLength, switchChannel); // This is just a digital output ttl. This is used for the opening pulse of the STEVE shutter. The newport ones need a pulse to turn on and one to turn off

					/*if (DurationIR != 0) //To not use shutter when not desired set duratiopn to 0
					{
						Pulse(time, DurationIR, shutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterSTEVEoff"]).BitNumber); // this is the V1/V2 STEVE shutter
					}*/
					//Pulse(time, v3delaytime, shutteroffdelay, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow"]).BitNumber);//this line seems to not work

					/*
					Pulse(time, shutterslowdelay - padStart - 3000, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber); //V0 AOM
					Pulse(time, shutterslowdelay - padStart + DurationV0, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber);//V0 AOM
					*/


					/*// Pulse(time, shutterV1delay - padStart, DurationV1, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv1/v2"]).BitNumber);
					//Pulse(time, shutterV2delay - padStart, DurationV2, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv2"]).BitNumber); //This is V2 aom, not used in practice
					*/

					//Pulse(time, CameraTrigger, shutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["camerashutter"]).BitNumber);//Guanchen added camera trigger for the molecule image
					//Pulse(time, BgTrigger, shutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["camerashutter"]).BitNumber); //Guanchen added camera trigger for the light background image
					
					//Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detector");

					time += flashlampPulseInterval;
                    for (int p = 0; p < padShots; p++)
                    {
                        FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
                        time += flashlampPulseInterval;
                    }
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
