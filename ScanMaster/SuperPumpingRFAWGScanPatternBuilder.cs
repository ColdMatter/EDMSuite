using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

namespace ScanMaster.Acquire.Patterns
{
    /// <summary>
    /// A pattern that generates a sequence of two rf pulses separated by some time.
    /// All the rf timings are incorporated in the sequence of waveforms.
    /// </summary>
    public class SuperPumpingRFAWGScanPatternBuilder : PatternBuilder32
    {
        private const int FLASH_PULSE_LENGTH = 100;
        private const int Q_PULSE_LENGTH = 100;
        private const int DETECTOR_TRIGGER_LENGTH = 20;
        private const int RF_TRIGGER_TO_GENERATION_TIME = 3;
        private const int RF_TRIGGER_PULSE_LENGTH = 100;

        int rfSwitchChannel = ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["rfSwitch"]).BitNumber;
        int fmChannel = ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["fmSelect"]).BitNumber;
        int attChannel = ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["attenuatorSelect"]).BitNumber;
        int piChannel = ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["piFlip"]).BitNumber;
        int scramblerChannel = ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["scramblerEnable"]).BitNumber;
        int mwEnableChannel = ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["mwEnable"]).BitNumber;
        int mwSelectPumpChannel = ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["mwSelectPumpChannel"]).BitNumber;
        int mwSelectTopProbeChannel = ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["mwSelectTopProbeChannel"]).BitNumber;
        int mwSelectBottomProbeChannel = ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["mwSelectBottomProbeChannel"]).BitNumber;
        int rfPumpSwitchChannel = ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["pumprfSwitch"]).BitNumber;

        public int ShotSequence(int startTime, int numberOfOnOffShots, int padShots, int flashlampPulseInterval,
            int valvePulseLength, int valveToQ, int flashToQ, int delayToDetectorTrigger, int rfTriggerTime,
            int pumprfCentreTime, int pumprfLength, int pumpmwCentreTime, int pumpmwLength, int bottomProbemwCentreTime, 
            int bottomProbemwLength, int topProbemwCentreTime, int topProbemwLength, bool modulateOn)
        {

            int time = startTime;

            // Disable rf
            AddEdge(rfSwitchChannel, 0, false);
            AddEdge(piChannel, 0, true);

            for (int i = 0; i < numberOfOnOffShots; i++)
            {
                Shot(time, valvePulseLength, valveToQ, flashToQ, delayToDetectorTrigger,
                        rfTriggerTime, pumprfCentreTime, pumprfLength, pumpmwCentreTime, pumpmwLength,
                        bottomProbemwCentreTime, bottomProbemwLength, topProbemwCentreTime, topProbemwLength, true);
                time += flashlampPulseInterval;

                // flip the "switch-scan" TTL line (if we need to)
                if (modulateOn)
                {
                    AddEdge(
                   ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttlSwitch"]).BitNumber,
                    time,
                    true
                    );
                }

                for (int p = 0; p < padShots; p++)
                {
                    FlashlampPulse(time, valveToQ, flashToQ);
                    time += flashlampPulseInterval;
                }
                if (modulateOn)
                {
                    Shot(time, valvePulseLength, valveToQ, flashToQ, delayToDetectorTrigger,
                        rfTriggerTime, pumprfCentreTime, pumprfLength, pumpmwCentreTime, pumpmwLength,
                        bottomProbemwCentreTime, bottomProbemwLength, topProbemwCentreTime, topProbemwLength, false);
                }
                else
                {
                    Shot(time, valvePulseLength, valveToQ, flashToQ, delayToDetectorTrigger,
                        rfTriggerTime, pumprfCentreTime, pumprfLength, pumpmwCentreTime, pumpmwLength,
                        bottomProbemwCentreTime, bottomProbemwLength, topProbemwCentreTime, topProbemwLength, true);
                }
                time += flashlampPulseInterval;

                // flip the "switch-scan" TTL line (if we need to)
                if (modulateOn)
                {
                    AddEdge(
                   ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["ttlSwitch"]).BitNumber,
                    time,
                    false
                    );
                }
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

        public int Shot(int startTime, int valvePulseLength, int valveToQ, int flashToQ,
            int delayToDetectorTrigger, int rfTriggerTime, int pumprfCentreTime, int pumprfLength, 
            int pumpmwCentreTime, int pumpmwLength, int bottomProbemwCentreTime, int bottomProbemwLength, 
            int topProbemwCentreTime, int topProbemwLength, bool modulated)
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

            // pulse pump rf
            if (pumprfLength != 0)
            {
                tempTime = Pulse(startTime, valveToQ + pumprfCentreTime - (pumprfLength / 2), pumprfLength, rfPumpSwitchChannel);
                if (tempTime > time) time = tempTime;
            }

            // enable microwaves for pumping
            if (pumpmwLength != 0)
            {
                tempTime = Pulse(startTime, valveToQ + pumpmwCentreTime - (pumpmwLength / 2), pumpmwLength, mwEnableChannel);
                if (tempTime > time) time = tempTime;
            }

            // throw microwave switch to pump region 
            if (pumpmwLength != 0)
            {
                tempTime = Pulse(startTime, valveToQ + pumpmwCentreTime - (pumpmwLength / 2), pumpmwLength, mwSelectPumpChannel);
                if (tempTime > time) time = tempTime;
            }

            // generate rf sequence
            tempTime = Pulse(startTime, valveToQ + rfTriggerTime - RF_TRIGGER_TO_GENERATION_TIME, RF_TRIGGER_PULSE_LENGTH, rfSwitchChannel);
            if (tempTime > time) time = tempTime;

            // enable microwaves for bottom probe region
            if (bottomProbemwLength != 0)
            {
                tempTime = Pulse(startTime, valveToQ + bottomProbemwCentreTime - (bottomProbemwLength / 2), bottomProbemwLength, mwEnableChannel);
                if (tempTime > time) time = tempTime;
            }

            // throw microwave switch to bottom probe region 
            if (bottomProbemwLength != 0)
            {
                tempTime = Pulse(startTime, valveToQ + bottomProbemwCentreTime - (bottomProbemwLength / 2), bottomProbemwLength, mwSelectBottomProbeChannel);
                if (tempTime > time) time = tempTime;
            }

            // enable microwaves for top probe region
            if (topProbemwLength != 0)
            {
                tempTime = Pulse(startTime, valveToQ + topProbemwCentreTime - (topProbemwLength / 2), topProbemwLength, mwEnableChannel);
                if (tempTime > time) time = tempTime;
            }

            // throw microwave switch to top region 
            if (topProbemwLength != 0)
            {
                tempTime = Pulse(startTime, valveToQ + topProbemwCentreTime - (topProbemwLength / 2), topProbemwLength, mwSelectTopProbeChannel);
                if (tempTime > time) time = tempTime;
            }

            // Detector trigger
            if (modulated)
            {
                tempTime = Pulse(startTime, delayToDetectorTrigger + valveToQ, DETECTOR_TRIGGER_LENGTH,
                    ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["detector"]).BitNumber);
                if (tempTime > time) time = tempTime;
            }
            else
            {
                tempTime = Pulse(startTime, delayToDetectorTrigger + valveToQ, DETECTOR_TRIGGER_LENGTH,
                    ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["detectorprime"]).BitNumber);
                if (tempTime > time) time = tempTime;
            }

            return time;
        }

    }
}
