using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

namespace ScanMaster.Acquire.Patterns
{
    public class CaFBECPatternBuilder : DAQ.Pattern.PatternBuilder32
    {

        public CaFBECPatternBuilder()
        {
        }

        //  private const int FLASH_PULSE_LENGTH = 100;
        private const int Q_PULSE_LENGTH = 100;
        private const int DETECTOR_TRIGGER_LENGTH = 20;
        private const int SWITCHLINEDURARION = 100;

        public int ShotSequence(int startTime, int NumberOfShots, int flashlampPulseInterval, int flashToQ, int flashlampPulseLength, 
            int[] ttl1StartTimes, int[] ttl1Durations, int[] ttl1Repetitions,
            int[] ttl2StartTimes, int[] ttl2Durations, int[] ttl2Repetitions, 
            int delayToDetectorTrigger, int ttlSwitchPort, int ttlSwitchLine, int switchLineDuration, int switchLineDelay, int chirpStart, int chirpDuration, bool modulation)
        {

            int switchChannel = PatternBuilder32.ChannelFromNIPort(ttlSwitchPort, ttlSwitchLine);
            int time = startTime;
            
            for (int ShotIndex = 0; ShotIndex < NumberOfShots; ShotIndex++)
            {

                //// Not in use now. Later can be editted for AOMs.
                // TTLpulses(time, ttl1StartTimes, ttl1Durations, ttl1Repetitions, NumberOfShots, ShotIndex, flashlampPulseInterval, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl1"]).BitNumber);
                // TTLpulses(time, ttl2StartTimes, ttl2Durations, ttl2Repetitions, NumberOfShots, ShotIndex, flashlampPulseInterval, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl2"]).BitNumber);

                ///// A pulse that is high 2 shots, low 2 shots /////
               
                /*
                if ((ShotIndex % 4 == 0) && (NumberOfShots % 4 == 0))
                {
                    Pulse(time, switchLineDelay, 2 * flashlampPulseInterval - 1000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["probe"]).BitNumber);
                }
               */

                ///// ON SHOTS /////
                if (ShotIndex % 2 == 0)
                {
                    // Console.WriteLine("High");
                    // first the pulse with the switch line high

                    // Pulse(time, switchLineDelay, switchLineDuration, switchChannel);
                    Pulse(time, switchLineDelay, switchLineDuration,
                        ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["probe"]).BitNumber);
                    Shot(time, flashToQ, flashlampPulseLength, delayToDetectorTrigger, chirpStart, chirpDuration,  "analogPatternTrigger");
                }


                ///// OFF SHOTS /////
                if (ShotIndex % 2 == 1)
                {
                    Shot(time, flashToQ, flashlampPulseLength, delayToDetectorTrigger, chirpStart, chirpDuration, "analogPatternTrigger");
                    
                    // now with the switch line low, if modulation is true (otherwise another with line high)
                    if (modulation)
                    {
                        // Console.WriteLine("Low");
                        Shot(time, flashToQ, flashlampPulseLength, delayToDetectorTrigger, chirpStart, chirpDuration, "analogPatternTrigger");
                    }
                    else
                    {
                        // Console.WriteLine("Bad High");
                        Pulse(time, switchLineDelay, SWITCHLINEDURARION, switchChannel);
                        Shot(time, flashToQ, flashlampPulseLength, delayToDetectorTrigger, chirpStart, chirpDuration, "analogPatternTrigger");
                    }
                }

                time += flashlampPulseInterval;
            }

            return time;
        }
        /*
            public int TTLpulses(int time, int[] ttlStartTimes, int[] ttlDurations, int[] ttlRepetitions, int numberOfShots, int ShotIndex, int flashlampPulseInterval, int channel)
        {
            for (int j = 0; j < ttlStartTimes.Length; j++)
            {
                if ((ShotIndex % ttlRepetitions[j] == 0) && (time + ttlStartTimes[j] + ttlDurations[j] < numberOfShots * flashlampPulseInterval)) Pulse(time, ttlStartTimes[j], ttlDurations[j], channel);
            }
            return time;
        }
        */

        public int Shot(int startTime, int flashToQ, int flashlampPulseLength, int delayToDetectorTrigger, int chirpStart, int chirpDuration, string detectorTriggerSource)
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
            tempTime = Pulse(startTime, chirpStart, chirpDuration,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["chirpTrigger"]).BitNumber);
            
            if (tempTime > time) time = tempTime;
            // Detector trigger
            tempTime = Pulse(startTime, delayToDetectorTrigger + flashToQ, DETECTOR_TRIGGER_LENGTH,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[detectorTriggerSource]).BitNumber);
            if (tempTime > time) time = tempTime;


            return time;
        }

    }
}
