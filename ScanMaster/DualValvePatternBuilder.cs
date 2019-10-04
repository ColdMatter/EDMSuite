using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

namespace ScanMaster.Acquire.Patterns
{
    /// <summary>
    /// See documentation for DualValvePatternPlugin
    /// </summary>
    public class DualValvePatternBuilder : DAQ.Pattern.PatternBuilder32
    {
        
        public DualValvePatternBuilder()
        {

        }

        private const int DETECTOR_TRIGGER_LENGTH = 20;

        public int ShotSequence(int startTime, int numberOfOnOffShots, int pulseInterval, int dischargeLength,
            int valve1PulseLength, int valve2PulseLength, int dischargeToValve1, int valve1ToValve2,
            int delayToDetectorTrigger)
        {

            int time = startTime;

            for (int i = 0; i < numberOfOnOffShots; i++)
            {
                
                Shot(time, dischargeLength, valve1PulseLength, valve2PulseLength, dischargeToValve1, valve1ToValve2, delayToDetectorTrigger, "detector");
                time += pulseInterval;
                
            }
            
            return time;
        }


        public int Shot(int startTime, int dischargeLength, int valve1PulseLength, 
            int valve2PulseLength, int dischargeToValve1, int valve1ToValve2, 
            int delayToDetectorTrigger, string detectorTriggerSource)
        {
            int time = 0;
            int tempTime = 0;

            // discharge pulse
            tempTime = Pulse(startTime, 0, dischargeLength,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["discharge"]).BitNumber);
            if (tempTime > time) time = tempTime;
            // valve1 pulse
            tempTime = Pulse(startTime, dischargeToValve1, valve1PulseLength,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["valve"]).BitNumber);
            if (tempTime > time) time = tempTime;
            // valve2 pulse
            tempTime = Pulse(startTime, dischargeToValve1 + valve1ToValve2, valve2PulseLength,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["valve2"]).BitNumber);
            if (tempTime > time) time = tempTime;
          
            // Detector trigger
            tempTime = Pulse(startTime, delayToDetectorTrigger, DETECTOR_TRIGGER_LENGTH,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[detectorTriggerSource]).BitNumber);
            if (tempTime > time) time = tempTime;


            return time;
        }

    }
}
