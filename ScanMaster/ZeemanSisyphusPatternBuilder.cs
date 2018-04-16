using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

namespace ScanMaster.Acquire.Patterns
{
    /// <summary>
    /// A pattern for making a MOT. See the MOTPatternPlugin.
    /// </summary>

    public class ZeemanSisyphusPatternBuilder : DAQ.Pattern.PatternBuilder32
    {

        public ZeemanSisyphusPatternBuilder()
        {
        }


        private const int Q_PULSE_LENGTH = 100;
        private const int DETECTOR_TRIGGER_LENGTH = 20;
        private const int CAMERA_TRIGGER_LENGTH = 20;

        public int ShotSequence(int startTime, int numberOfOnOffShots, int padShots, int flashlampPulseInterval,
            int valvePulseLength, int valveToQ, int flashToQ, int flashlampPulseLength,  int cameraTrigger, bool modulation)
        {
            int time = startTime;

            for (int i = 0; i < numberOfOnOffShots; i++)
            {
               
                // first the pulse with the Q switch triggered
                Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, cameraTrigger, true);
                time += flashlampPulseInterval;
                for (int p = 0; p < padShots; p++)
                {
                    FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
                    time += flashlampPulseInterval;
                }
                // now the pulse with the Q switch not triggered, if modulation is true (otherwise another one as before)
                if (modulation)
                {
                    Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, cameraTrigger, false);
                    time += flashlampPulseInterval;
                    for (int p = 0; p < padShots; p++)
                    {
                        FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
                        time += flashlampPulseInterval;
                    }
                }
                else
                {
                    Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, cameraTrigger, true);
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

        public int Shot(int startTime, int valvePulseLength, int valveToQ, int flashToQ, int flashlampPulseLength, int cameraTrigger, bool qTrig)
        {
            int time = 0;
            int tempTime = 0;

            // valve pulse
            //tempTime = Pulse(startTime, 0, valvePulseLength,
            //    ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["valve"]).BitNumber);
            //if (tempTime > time) time = tempTime;
            // Flash pulse
            tempTime = Pulse(startTime, valveToQ - flashToQ, flashlampPulseLength,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash"]).BitNumber);
            if (tempTime > time) time = tempTime;
            // Q pulse
            if (qTrig) tempTime = Pulse(startTime, valveToQ, Q_PULSE_LENGTH,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["q"]).BitNumber);
            if (tempTime > time) time = tempTime;
           
            //all following pulses have delay: valveToQ + parameterPulseTime to make them happen relative to the yag firing
       
            // analog pattern trigger pulse
            tempTime = Pulse(startTime, valveToQ, 100,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["analogPatternTrigger"]).BitNumber);
            if (tempTime > time) time = tempTime;

            if (tempTime > time) time = tempTime;


            return time;
        }

    }
}
