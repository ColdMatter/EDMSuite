using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

namespace ScanMaster.Acquire.Patterns
{
    /// <summary>
    /// Pattern for controlling the crazy box that accepts two ttl pulses and generates 4 possible analog output levels depending on their states.
    /// There are two ttl lines, and each one is pulsed on twice.
    /// </summary>
    public class AomLevelControlPatternBuilder : DAQ.Pattern.PatternBuilder32
    {
        public AomLevelControlPatternBuilder()
        {
        }

        private const int FLASH_PULSE_LENGTH = 100;
        private const int Q_PULSE_LENGTH = 100;
        private const int DETECTOR_TRIGGER_LENGTH = 20;

        public int ShotSequence(int startTime, int numberOfOnOffShots, int padShots, int flashlampPulseInterval,
            int valvePulseLength, int valveToQ, int flashToQ, int ttl1Start1, int ttl1Duration1, int ttl1Start2, int ttl1Duration2,
            int ttl2Start1, int ttl2Duration1, int ttl2Start2, int ttl2Duration2, int delayToDetectorTrigger)
        {

            int time = startTime;


            for (int i = 0; i < numberOfOnOffShots; i++)
            {
                Shot(startTime, valvePulseLength, valveToQ, flashToQ, ttl1Start1, ttl1Duration1, ttl1Start2, ttl1Duration2,
                 ttl2Start1, ttl2Duration1, ttl2Start2, ttl2Duration2, delayToDetectorTrigger);
                time += flashlampPulseInterval;
                for (int p = 0; p < padShots; p++)
                {
                    FlashlampPulse(time, valveToQ, flashToQ);
                    time += flashlampPulseInterval;
                }
            }

            return time;
        }

        public int FlashlampPulse(int startTime, int valveToQ, int flashToQ)
        {
            return Pulse(startTime, valveToQ - flashToQ, FLASH_PULSE_LENGTH,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash"]).BitNumber);
        }

        public int Shot(int startTime, int valvePulseLength, int valveToQ, int flashToQ, int ttl1Start1, int ttl1Duration1, int ttl1Start2, int ttl1Duration2,
            int ttl2Start1, int ttl2Duration1, int ttl2Start2, int ttl2Duration2, int delayToDetectorTrigger)
        {
            int time = 0;
            int tempTime = 0;

            // valve pulse
            tempTime = Pulse(startTime, 0, valvePulseLength,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["valve"]).BitNumber);
            if (tempTime > time) time = tempTime;
            // Flash pulse
            tempTime = Pulse(startTime, valveToQ - flashToQ, FLASH_PULSE_LENGTH,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash"]).BitNumber);
            if (tempTime > time) time = tempTime;
            // Q pulse
            tempTime = Pulse(startTime, valveToQ, Q_PULSE_LENGTH,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["q"]).BitNumber);
            if (tempTime > time) time = tempTime;
            // ttl 1, pulse 1
            tempTime = Pulse(startTime, ttl1Start1 + valveToQ, ttl1Duration1,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl1"]).BitNumber);
            if (tempTime > time) time = tempTime;
            // ttl 2, pulse 1
            tempTime = Pulse(startTime, ttl2Start1 + valveToQ, ttl2Duration1,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl2"]).BitNumber);
            if (tempTime > time) time = tempTime;
            // ttl 1, pulse 2
            tempTime = Pulse(startTime, ttl1Start2 + valveToQ, ttl1Duration2,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl1"]).BitNumber);
            if (tempTime > time) time = tempTime;
            // ttl 2, pulse 2
            tempTime = Pulse(startTime, ttl2Start2 + valveToQ, ttl2Duration2,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl2"]).BitNumber);
            if (tempTime > time) time = tempTime;
            // Detector trigger
            tempTime = Pulse(startTime, delayToDetectorTrigger + valveToQ, DETECTOR_TRIGGER_LENGTH,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["detector"]).BitNumber);
            if (tempTime > time) time = tempTime;


            return time;
        }

    }
}
