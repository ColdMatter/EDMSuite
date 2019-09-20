using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

namespace ScanMaster.Acquire.Patterns
{
    /// <summary>
    /// See the documentation for the PumpProbePatternPlugin
    /// </summary>
    public class ImagingPatternBuilder : DAQ.Pattern.PatternBuilder32
    {
        public ImagingPatternBuilder()
        {
            // 
            // TODO: Add constructor logic here
            //
        }

        private const int FLASH_PULSE_LENGTH = 100;
        private const int Q_PULSE_LENGTH = 100;
        private const int DETECTOR_TRIGGER_LENGTH = 20;

        public int ShotSequence(int startTime, int numberOfOnOffShots, int padShots, int flashlampPulseInterval,
            int valvePulseLength, int valveToQ, int flashToQ, int aomStart1, int aomDuration1,
            int aomStart2, int aomDuration2, int probeStart, int probeDuration, int shutterStart, int shutterDuration, int delayToDetectorTrigger,
            int ttlSwitchPort, int ttlSwitchLine, int switchLineDuration, int switchLineDelay, bool modulation)
        {

            int time = startTime;


            for (int i = 0; i < numberOfOnOffShots; i++)
            {
                // first the pulse with the YAG
                Shot(time, valvePulseLength, valveToQ, flashToQ, probeStart, probeDuration, 
                    shutterStart, shutterDuration, delayToDetectorTrigger, "detector", "shutterTrig1", true);
                time += flashlampPulseInterval;
                for (int p = 0; p < padShots; p++)
                {
                    FlashlampPulse(time, valveToQ, flashToQ);
                    time += flashlampPulseInterval;
                }
                // now without the YAG, if modulation is true (otherwise another with the YAG)
                if (modulation)
                {
                    Shot(time, valvePulseLength, valveToQ, flashToQ, probeStart, probeDuration,
                         shutterStart, shutterDuration, delayToDetectorTrigger, "detectorprime", "shutterTrig2", false);
                    time += flashlampPulseInterval;
                    for (int p = 0; p < padShots; p++)
                    {
                        FlashlampPulse(time, valveToQ, flashToQ);
                        time += flashlampPulseInterval;
                    }
                }
                else
                {
                    Shot(time, valvePulseLength, valveToQ, flashToQ, probeStart, probeDuration,
                    shutterStart, shutterDuration, delayToDetectorTrigger, "detector", "shutterTrig1", true);
                    time += flashlampPulseInterval;
                    for (int p = 0; p < padShots; p++)
                    {
                        FlashlampPulse(time, valveToQ, flashToQ);
                        time += flashlampPulseInterval;
                    }
                }
            }

            return time;
        }

        public int FlashlampPulse(int startTime, int valveToQ, int flashToQ)
        {
            return Pulse(startTime, valveToQ - flashToQ, FLASH_PULSE_LENGTH,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash"]).BitNumber);
        }

        public int Shot(int startTime, int valvePulseLength, int valveToQ, int flashToQ, 
            int probeStart, int probeDuration, int shutterStart, int shutterDuration, 
            int delayToDetectorTrigger, string detectorTriggerSource, string cameraTriggerSource, bool yag)
        {
            int time = 0;
            int tempTime = 0;
            
            // valve pulse
            tempTime = Pulse(startTime, 0, valvePulseLength,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["valve"]).BitNumber);
            if (tempTime > time) time = tempTime;
            if (yag)
            {
                // Flash pulse
                tempTime = Pulse(startTime, valveToQ - flashToQ, FLASH_PULSE_LENGTH,
                    ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash"]).BitNumber);
                if (tempTime > time) time = tempTime;
                // Q pulse
                tempTime = Pulse(startTime, valveToQ, Q_PULSE_LENGTH,
                    ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["q"]).BitNumber);
                if (tempTime > time) time = tempTime;
            }
            // probe pulse
            tempTime = Pulse(startTime, probeStart + valveToQ, probeDuration,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["probe"]).BitNumber);
            if (tempTime > time) time = tempTime;
            // shutter pulse
            tempTime = Pulse(startTime, shutterStart + valveToQ, shutterDuration,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[cameraTriggerSource]).BitNumber);
            if (tempTime > time) time = tempTime;
            // Detector trigger
            tempTime = Pulse(startTime, delayToDetectorTrigger + valveToQ, DETECTOR_TRIGGER_LENGTH,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[detectorTriggerSource]).BitNumber);
            if (tempTime > time) time = tempTime;


            return time;
        }

    }
}
