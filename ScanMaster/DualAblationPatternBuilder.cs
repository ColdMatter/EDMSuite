using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

namespace ScanMaster.Acquire.Patterns
{
    /// <summary>
    /// See documentation for DualAblationPatternPlugin
    /// </summary>
    public class DualAblationPatternBuilder : DAQ.Pattern.PatternBuilder32
    {
        public DualAblationPatternBuilder()
        {
            
        }

        private const int FLASH_PULSE_LENGTH = 100;
        private const int Q_PULSE_LENGTH = 100;
        private const int DETECTOR_TRIGGER_LENGTH = 20;

        public int ShotSequence(int startTime, int numberOfOnOffShots, int flashlampPulseInterval,
            int valvePulseLength, int valveToQ, int flashToQ, int flash2ToQ2, int qToQ2, int delayToDetectorTrigger, bool modulation)
        {

            int time = startTime;
            
            for (int i = 0; i < numberOfOnOffShots; i++)
            {
                // first the pulse with both ablation pulses
                Shot(time, valvePulseLength, valveToQ, flashToQ, flash2ToQ2, qToQ2, delayToDetectorTrigger, "detector", true);
                time += flashlampPulseInterval;
                // now with only one ablation laser, if modulation is true (otherwise another with both lasers)
                if (modulation)
                {
                    Shot(time, valvePulseLength, valveToQ, flashToQ, flash2ToQ2, qToQ2, delayToDetectorTrigger, "detectorprime", false);
                    time += flashlampPulseInterval;
                }
                else
                {
                    Shot(time, valvePulseLength, valveToQ, flashToQ, flash2ToQ2, qToQ2, delayToDetectorTrigger, "detector", true);
                    time += flashlampPulseInterval;
                }
            }

            return time;
        }

        
        public int Shot(int startTime, int valvePulseLength, int valveToQ, int flashToQ, int flash2ToQ2, int qToQ2,
            int delayToDetectorTrigger, string detectorTriggerSource, bool both)
        {
            int time = 0;
            int tempTime = 0;

            // valve pulse
            tempTime = Pulse(startTime, 0, valvePulseLength,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["valve"]).BitNumber);
            if (tempTime > time) time = tempTime;
            // Flash pulse 1
            tempTime = Pulse(startTime, valveToQ - flashToQ, FLASH_PULSE_LENGTH,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash"]).BitNumber);
            if (tempTime > time) time = tempTime;
            // Flash pulse 2
            if (both)
            {
                tempTime = Pulse(startTime, valveToQ + qToQ2 - flash2ToQ2, FLASH_PULSE_LENGTH,
                     ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash2"]).BitNumber);
                if (tempTime > time) time = tempTime;
            }
            // Q pulse 1
            tempTime = Pulse(startTime, valveToQ, Q_PULSE_LENGTH,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["q"]).BitNumber);
            if (tempTime > time) time = tempTime;
            // Q pulse 2
            if (both)
            {
                tempTime = Pulse(startTime, valveToQ + qToQ2, Q_PULSE_LENGTH,
                    ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["q2"]).BitNumber);
                if (tempTime > time) time = tempTime;
            }
            // Detector trigger
            tempTime = Pulse(startTime, delayToDetectorTrigger + valveToQ, DETECTOR_TRIGGER_LENGTH,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[detectorTriggerSource]).BitNumber);
            if (tempTime > time) time = tempTime;


            return time;
        }

    }
}
