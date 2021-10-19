using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

namespace ScanMaster.Acquire.Patterns
{
    /// <summary>
    /// This is the builder for the NshotsPatternPlugin.
    /// </summary>
    public class NshotsPatternBuilder : DAQ.Pattern.PatternBuilder32
    {
        public NshotsPatternBuilder()
        {
        }

        //	private const int FLASH_PULSE_LENGTH = 100;
        private const int Q_PULSE_LENGTH = 100;
        private const int DETECTOR_TRIGGER_LENGTH = 20;
        private const int CCD_TRIGGER_LENGTH = 5000;

        public int ShotSequence(int startTime, int NumberOfShots, int flashlampPulseInterval, int flashToQ, int flashlampPulseLength,
            int ccd1Start1, int ccd1Start2, int ccd2Start1, int ccd2Start2, int[] ttl1StartTimes, int[] ttl1Durations, int[] ttl1Repetitions,
            int[] ttl2StartTimes, int[] ttl2Durations, int[] ttl2Repetitions, int[] ttl3StartTimes, int[] ttl3Durations, int[] ttl3Repetitions,
            int[] ttl4StartTimes, int[] ttl4Durations, int[] ttl4Repetitions, int[] ttl5StartTimes, int[] ttl5Durations, int[] ttl5Repetitions,
            int[] ttl6StartTimes, int[] ttl6Durations, int[] ttl6Repetitions,
            int delayToDetectorTrigger, int ttlSwitchPort, int ttlSwitchLine, int switchLineDuration, int switchLineDelay, bool modulation)
        {

            int switchChannel = PatternBuilder32.ChannelFromNIPort(ttlSwitchPort, ttlSwitchLine);
            int time = startTime;

            for (int ShotIndex = 0; ShotIndex < NumberOfShots; ShotIndex++)
            {

                ///// Generate the TTLs sequence for channels ttl1 to tll6 /////
                /// (the pulse is generated only if ttlRepetitions==ShotIndex)
                TTLpulses(time, ttl1StartTimes, ttl1Durations, ttl1Repetitions, NumberOfShots, ShotIndex, flashlampPulseInterval, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl1"]).BitNumber);
                TTLpulses(time, ttl2StartTimes, ttl2Durations, ttl2Repetitions, NumberOfShots, ShotIndex, flashlampPulseInterval, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl2"]).BitNumber);
                TTLpulses(time, ttl3StartTimes, ttl3Durations, ttl3Repetitions, NumberOfShots, ShotIndex, flashlampPulseInterval, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl3"]).BitNumber);
                TTLpulses(time, ttl4StartTimes, ttl4Durations, ttl4Repetitions, NumberOfShots, ShotIndex, flashlampPulseInterval, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl4"]).BitNumber);
                TTLpulses(time, ttl5StartTimes, ttl5Durations, ttl5Repetitions, NumberOfShots, ShotIndex, flashlampPulseInterval, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl5"]).BitNumber);
                TTLpulses(time, ttl6StartTimes, ttl6Durations, ttl6Repetitions, NumberOfShots, ShotIndex, flashlampPulseInterval, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["valve"]).BitNumber);


                ///// A pulse that is high 2 shots, low 2 shots /////
                /// This allows to easily use this plugin for detecting with 2ccds alternatively
                if (ShotIndex % 4 == 0)
                {
                    Pulse(time, switchLineDelay, 2 * flashlampPulseInterval - 1000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["probe"]).BitNumber);
                }

                ///// ON SHOTS /////
                if (ShotIndex % 2 == 0)
                {
                    // first the pulse with the switch line high
                    Pulse(time, switchLineDelay, switchLineDuration, switchChannel);
                    Shot(time, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detector");

                    // The CCDs triggers
                    Pulse(time, ccd1Start1, CCD_TRIGGER_LENGTH, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd1"]).BitNumber);
                    Pulse(time, ccd1Start2, CCD_TRIGGER_LENGTH, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd1"]).BitNumber);
                    Pulse(time, ccd2Start1, CCD_TRIGGER_LENGTH, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd2"]).BitNumber);
                    Pulse(time, ccd2Start2, CCD_TRIGGER_LENGTH, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd2"]).BitNumber);
                }


                ///// OFF SHOTS /////
                if (ShotIndex % 2 == 1)
                {
                    // The CCDs triggers
                    Pulse(time, ccd1Start1, CCD_TRIGGER_LENGTH, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd1"]).BitNumber);
                    Pulse(time, ccd1Start2, CCD_TRIGGER_LENGTH, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd1"]).BitNumber);
                    Pulse(time, ccd2Start1, CCD_TRIGGER_LENGTH, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd2"]).BitNumber);
                    Pulse(time, ccd2Start2, CCD_TRIGGER_LENGTH, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd2"]).BitNumber);

                    // now with the switch line low, if modulation is true (otherwise another with line high)
                    if (modulation)
                    {
                        Shot(time, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detectorprime");
                    }
                    else
                    {
                        Pulse(time, switchLineDelay, switchLineDuration, switchChannel);
                        Shot(time, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detector");
                    }
                }

                time += flashlampPulseInterval;
            }


            return time;
        }


            public int TTLpulses(int time, int[] ttlStartTimes, int[] ttlDurations, int[] ttlRepetitions, int numberOfShots, int ShotIndex, int flashlampPulseInterval, int channel)
        {
            for (int j = 0; j < ttlStartTimes.Length; j++)
            {
                if ((ShotIndex % ttlRepetitions[j] == 0) && (time + ttlStartTimes[j] + ttlDurations[j] < numberOfShots * flashlampPulseInterval)) Pulse(time, ttlStartTimes[j], ttlDurations[j], channel);
            }
            return time;
        }


            public int Shot(int startTime, int flashToQ, int flashlampPulseLength, int delayToDetectorTrigger, string detectorTriggerSource)
        {
            int time = 0;
            int tempTime = 0;
            // Flash pulse
            tempTime = Pulse(startTime, 0, flashlampPulseLength,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash"]).BitNumber);
            if (tempTime > time) time = tempTime;
            // Q pulse
            tempTime = Pulse(startTime, flashToQ, Q_PULSE_LENGTH,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["q"]).BitNumber);
            if (tempTime > time) time = tempTime;

            if (tempTime > time) time = tempTime;
            // Detector trigger
            tempTime = Pulse(startTime, delayToDetectorTrigger + flashToQ, DETECTOR_TRIGGER_LENGTH,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[detectorTriggerSource]).BitNumber);
            if (tempTime > time) time = tempTime;


            return time;
        }
    }
}
