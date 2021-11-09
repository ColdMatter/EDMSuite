using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

namespace ScanMaster.Acquire.Patterns
{
    /// <summary>
    /// See the documentation for the PumpProbePatternPlugin
    /// </summary>
    public class LeakTestWithDyePatternBuilder : DAQ.Pattern.PatternBuilder32
    {
        public LeakTestWithDyePatternBuilder()
        {
        }

        //	private const int FLASH_PULSE_LENGTH = 100;
        private const int Q_PULSE_LENGTH = 100;
        private const int DETECTOR_TRIGGER_LENGTH = 20;

        public int ShotSequence(int startTime, int numberOfOnOffShots, int padShots, int padStart, int flashlampPulseInterval,
            int valvePulseLength, int valveToQ, int flashToQ, int flashlampPulseLength, int ccd1Start1, int ccd1Start2, int ccd2Start1, int ccd2Start2, int delayToDetectorTrigger,
            int ttlSwitchPort, int ttlSwitchLine, int switchLineDuration, int switchLineDelay, int ttl1Delay, bool modulation)
        {
            int time;
            if (padStart == 0)
            {
                time = startTime;
            }
            else
            {
                time = startTime + 0 * padStart;
            }
            for (int i = 0; i < numberOfOnOffShots; i++)
            {

                int switchChannel = PatternBuilder32.ChannelFromNIPort(ttlSwitchPort, ttlSwitchLine);
                int halfFlashlampPulseInterval = flashlampPulseInterval / 2;
                // first the pulse with the switch line high
                Pulse(time, valveToQ + switchLineDelay, switchLineDuration, switchChannel);
                Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detector");

                if (i < 3)
                {
                    //AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl1"]).BitNumber, time + switchLineDelay, true);
                    //AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl1"]).BitNumber, time + 2 * flashlampPulseInterval + switchLineDelay - 1000, false);
                    //AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl3"]).BitNumber, time + ttl1Delay, true);
                    //AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl3"]).BitNumber, time + 2 * flashlampPulseInterval + switchLineDelay - 1000, false);
                    //AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd1"]).BitNumber, time + valveToQ + 0 * flashlampPulseInterval + ccd1Start1, true);
                    //AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd1"]).BitNumber, time + valveToQ + 0 * flashlampPulseInterval + ccd1Start1 + 5000, false);
                    //AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd1"]).BitNumber, time + valveToQ + 0 * flashlampPulseInterval + ccd1Start2, true);
                    //AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd1"]).BitNumber, time + valveToQ + 0 * flashlampPulseInterval + ccd1Start2 + 5000, false);
                    //AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd1"]).BitNumber, time + valveToQ + 1 * flashlampPulseInterval + ccd1Start1, true);
                    //AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd1"]).BitNumber, time + valveToQ + 1 * flashlampPulseInterval + ccd1Start1 + 5000, false);
                    //AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd1"]).BitNumber, time + valveToQ + 1 * flashlampPulseInterval + ccd1Start2, true);
                    //AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd1"]).BitNumber, time + valveToQ + 1 * flashlampPulseInterval + ccd1Start2 + 5000, false);

                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl1"]).BitNumber, time + switchLineDelay + 0 * flashlampPulseInterval, true);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl1"]).BitNumber, time + switchLineDelay + 1 * flashlampPulseInterval - 1000, false);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl3"]).BitNumber, time + switchLineDelay + ttl1Delay, true);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl3"]).BitNumber, time + 2 * flashlampPulseInterval + switchLineDelay - 1000, false);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd1"]).BitNumber, time + valveToQ + 0 * flashlampPulseInterval + ccd1Start1, true);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd1"]).BitNumber, time + valveToQ + 0 * flashlampPulseInterval + ccd1Start1 + 5000, false);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd1"]).BitNumber, time + valveToQ + 0 * flashlampPulseInterval + ccd1Start2, true);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd1"]).BitNumber, time + valveToQ + 0 * flashlampPulseInterval + ccd1Start2 + 5000, false);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd1"]).BitNumber, time + valveToQ + 1 * flashlampPulseInterval + ccd1Start1, true);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd1"]).BitNumber, time + valveToQ + 1 * flashlampPulseInterval + ccd1Start1 + 5000, false);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd1"]).BitNumber, time + valveToQ + 1 * flashlampPulseInterval + ccd1Start2, true);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd1"]).BitNumber, time + valveToQ + 1 * flashlampPulseInterval + ccd1Start2 + 5000, false);
                }
                else
                {
                    //AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl2"]).BitNumber, time + switchLineDelay, true);
                    //AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl2"]).BitNumber, time + 2 * flashlampPulseInterval + switchLineDelay - 1000, false);
                    //AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd2"]).BitNumber, time + valveToQ + 0 * flashlampPulseInterval + ccd2Start1, true);
                    //AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd2"]).BitNumber, time + valveToQ + 0 * flashlampPulseInterval + ccd2Start1 + 5000, false);
                    //AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd2"]).BitNumber, time + valveToQ + 0 * flashlampPulseInterval + ccd2Start2, true);
                    //AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd2"]).BitNumber, time + valveToQ + 0 * flashlampPulseInterval + ccd2Start2 + 5000, false);
                    //AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd2"]).BitNumber, time + valveToQ + 1 * flashlampPulseInterval + ccd2Start1, true);
                    //AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd2"]).BitNumber, time + valveToQ + 1 * flashlampPulseInterval + ccd2Start1 + 5000, false);
                    //AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd2"]).BitNumber, time + valveToQ + 1 * flashlampPulseInterval + ccd2Start2, true);
                    //AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd2"]).BitNumber, time + valveToQ + 1 * flashlampPulseInterval + ccd2Start2 + 5000, false);

                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl1"]).BitNumber, time + switchLineDelay + 0 * flashlampPulseInterval, true);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl1"]).BitNumber, time + switchLineDelay + 1 * flashlampPulseInterval - 1000, false);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl2"]).BitNumber, time + ttl1Delay, true);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl2"]).BitNumber, time + 2 * flashlampPulseInterval + switchLineDelay - 1000, false);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl3"]).BitNumber, time + switchLineDelay + ttl1Delay, true);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttl3"]).BitNumber, time + switchLineDelay + ttl1Delay + switchLineDuration, false);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd2"]).BitNumber, time + valveToQ + 0 * flashlampPulseInterval + ccd2Start1, true);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd2"]).BitNumber, time + valveToQ + 0 * flashlampPulseInterval + ccd2Start1 + 5000, false);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd2"]).BitNumber, time + valveToQ + 0 * flashlampPulseInterval + ccd2Start2, true);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd2"]).BitNumber, time + valveToQ + 0 * flashlampPulseInterval + ccd2Start2 + 5000, false);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd2"]).BitNumber, time + valveToQ + 1 * flashlampPulseInterval + ccd2Start1, true);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd2"]).BitNumber, time + valveToQ + 1 * flashlampPulseInterval + ccd2Start1 + 5000, false);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd2"]).BitNumber, time + valveToQ + 1 * flashlampPulseInterval + ccd2Start2, true);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ccd2"]).BitNumber, time + valveToQ + 1 * flashlampPulseInterval + ccd2Start2 + 5000, false);

                }
                time += flashlampPulseInterval;

                for (int p = 0; p < padShots; p++)
                {
                    FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
                    time += flashlampPulseInterval;
                }
                // now with the switch line low, if modulation is true (otherwise another with line high)
                if (modulation)
                {
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
                    Pulse(time, valveToQ + switchLineDelay, switchLineDuration, switchChannel);
                    Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detector");
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
            //if(shutter2) Pulse(time, valveToQ + switchLineDelay, 2 * flashlampPulseInterval - 10, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["aom2"]).BitNumber);
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

            if (tempTime > time) time = tempTime;
            // Detector trigger
            tempTime = Pulse(startTime, delayToDetectorTrigger + valveToQ, DETECTOR_TRIGGER_LENGTH,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[detectorTriggerSource]).BitNumber);
            if (tempTime > time) time = tempTime;


            return time;
        }
    }
}
