using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

namespace ScanMaster.Acquire.Patterns
{
    /// <summary>
    /// A pattern for making a MOT. See the MOTPatternPlugin.
    /// </summary>

   public class MOTPatternBuilder : DAQ.Pattern.PatternBuilder32
        {

            public MOTPatternBuilder()
            {
            }
            
           
            private const int Q_PULSE_LENGTH = 100;
            private const int DETECTOR_TRIGGER_LENGTH = 20;
            private const int MOT_RAMP_TRIGGER_LENGTH = 20;
        
            private const int CAMERA_TRIGGER_LENGTH = 20;

            public int ShotSequence(int startTime, int numberOfOnOffShots, int padShots, int padStart, int flashlampPulseInterval,
                int valvePulseLength, int valveToQ, int flashToQ, int flashlampPulseLength, int slowingAOMStart1, int slowingAOMDuration1,
                int slowingAOMStart2, int slowingAOMDuration2, int slowingRepumpAOMDuration1, int motAOMStart, int motAOMDuration, int motRampStart, int motAOMReStart,
                int bTrigger,int bDuration, int cameraTrigger, int delayToDetectorTrigger,
                int chirpStart, int chirpDuration, bool modulation)
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
                for (int i = 0; i < numberOfOnOffShots; i++)
                {
                   
                    // first the pulse with the Q switch triggered
                    Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, slowingAOMStart1, slowingAOMDuration1, slowingAOMStart2, slowingAOMDuration2, slowingRepumpAOMDuration1, motAOMStart, motAOMDuration, motRampStart, motAOMReStart,
                        bTrigger, bDuration, cameraTrigger, delayToDetectorTrigger, chirpStart, chirpDuration, "detector", true);
                    time += flashlampPulseInterval;
                    for (int p = 0; p < padShots; p++)
                    {
                        FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
                        time += flashlampPulseInterval;
                    }
                    // now the pulse with the Q switch not triggered, if modulation is true (otherwise another one as before)
                    if (modulation)
                    {
                        Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, slowingAOMStart1, slowingAOMDuration1, slowingAOMStart2, slowingAOMDuration2, slowingRepumpAOMDuration1, motAOMStart, motAOMDuration, motRampStart, motAOMReStart,
                       bTrigger, bDuration, cameraTrigger, delayToDetectorTrigger, chirpStart, chirpDuration, "detectorprime", false);
                        time += flashlampPulseInterval;
                        for (int p = 0; p < padShots; p++)
                        {
                            FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
                            time += flashlampPulseInterval;
                        }
                    }
                    else
                    {
                        Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, slowingAOMStart1, slowingAOMDuration1, slowingAOMStart2, slowingAOMDuration2, slowingRepumpAOMDuration1, motAOMStart, motAOMDuration, motRampStart, motAOMReStart,
                       bTrigger, bDuration, cameraTrigger, delayToDetectorTrigger, chirpStart, chirpDuration, "detector", true);
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

            public int Shot(int startTime, int valvePulseLength, int valveToQ, int flashToQ, int flashlampPulseLength, int slowingAOMStart1, int slowingAOMDuration1,
                int slowingAOMStart2, int slowingAOMDuration2, int slowingRepumpAOMDuration1, 
                int motAOMStart, int motAOMDuration, int motRampStart, int motAOMReStart,
                int bTrigger, int bDuration, int cameraTrigger,
                int delayToDetectorTrigger, int chirpStart, int chirpDuration, string detectorTriggerSource, bool qTrig)
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
                if (qTrig) tempTime = Pulse(startTime, valveToQ, Q_PULSE_LENGTH,
                    ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["q"]).BitNumber);
                if (tempTime > time) time = tempTime;
                // chirp pulse
                tempTime = Pulse(startTime, valveToQ + chirpStart, chirpDuration,
                    ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["chirpTrigger"]).BitNumber);
                if (tempTime > time) time = tempTime;
                // slowing aom pulse 1
                tempTime = Pulse(startTime, slowingAOMStart1 + valveToQ, slowingAOMDuration1,
                    ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["aom"]).BitNumber);
                // slowing repump aom pulse 1
                tempTime = Pulse(startTime, slowingAOMStart1 + valveToQ, slowingRepumpAOMDuration1,
                    ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["aom2"]).BitNumber);
                if (tempTime > time) time = tempTime;
                // slowing aom pulse 2
                tempTime = Pulse(startTime, slowingAOMStart2 + valveToQ, slowingAOMDuration2,
                    ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["aom"]).BitNumber);
                // alowing repump aom pulse 2
                tempTime = Pulse(startTime, slowingAOMStart2 + valveToQ, slowingAOMDuration2,
                    ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["aom2"]).BitNumber);
                if (tempTime > time) time = tempTime;
                // MOT off pulse
                tempTime = Pulse(startTime, valveToQ + motAOMStart, motAOMDuration,
                    ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["motAOM"]).BitNumber);
                if (tempTime > time) time = tempTime;
                // MOT ramp pulse
                tempTime = Pulse(startTime, valveToQ + motRampStart, MOT_RAMP_TRIGGER_LENGTH,
                    ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["motRampTrigger"]).BitNumber);
                if (tempTime > time) time = tempTime;
                // B-field trigger pulse
                tempTime = Pulse(startTime, valveToQ + bTrigger, bDuration,
                    ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["bTrigger"]).BitNumber);
                if (tempTime > time) time = tempTime;
                // camera trigger pulse
                tempTime = Pulse(startTime, valveToQ + cameraTrigger, CAMERA_TRIGGER_LENGTH,
                    ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["cameraTrigger"]).BitNumber);
                if (tempTime > time) time = tempTime;
                // Detector trigger
                tempTime = Pulse(startTime, delayToDetectorTrigger + valveToQ, DETECTOR_TRIGGER_LENGTH,
                    ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[detectorTriggerSource]).BitNumber);
                if (tempTime > time) time = tempTime;


                return time;
            }

        }
    }
