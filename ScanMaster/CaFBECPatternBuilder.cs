using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;
using ScanMaster.Acquire.Patterns;

namespace ScanMaster.Acquire.Patterns
{
    public class CaFBECPatternBuilder : DAQ.Pattern.PatternBuilder32
    {

        public CaFBECPatternBuilder()
        {
        }

        private const int Q_PULSE_LENGTH = 100;
        private const int DETECTOR_TRIGGER_LENGTH = 20;
        private const int CAMERA_TRIGGER_LENGTH = 20;

        public int ShotSequence(int startTime, int numberOfOnOffShots, int padShots, int flashlampPulseInterval,
            int flashToQ, int flashlampPulseLength, bool modulation)
        {
            int time = startTime;

            for (int i = 0; i < numberOfOnOffShots; i++)
            {

                // first the pulse with the Q switch triggered
                Shot(time, flashToQ, flashlampPulseLength, true);
                time += flashlampPulseInterval;
                for (int p = 0; p < padShots; p++)
                {
                    FlashlampPulse(time, flashToQ, flashlampPulseLength);
                    time += flashlampPulseInterval;
                }
                // now the pulse with the Q switch not triggered, if modulation is true (otherwise another one as before)
                if (modulation)
                {
                    Shot(time, flashToQ, flashlampPulseLength, false);
                    time += flashlampPulseInterval;
                    for (int p = 0; p < padShots; p++)
                    {
                        FlashlampPulse(time, flashToQ, flashlampPulseLength);
                        time += flashlampPulseInterval;
                    }
                }
                else
                {
                    Shot(time, flashToQ, flashlampPulseLength, true);
                    time += flashlampPulseInterval;
                    for (int p = 0; p < padShots; p++)
                    {
                        FlashlampPulse(time, flashToQ, flashlampPulseLength);
                        time += flashlampPulseInterval;
                    }
                }
            }

            return time;
        }

        public int FlashlampPulse(int startTime, int flashToQ, int flashlampPulseLength)
        {
            return Pulse(startTime, flashToQ, flashlampPulseLength,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash"]).BitNumber);
        }

        public int Shot(int startTime, int flashToQ, int flashlampPulseLength, bool qTrig)
        {
            int time = 0;
            int tempTime = 0;

            // valve pulse
            //tempTime = Pulse(startTime, 0, valvePulseLength,
            //    ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["valve"]).BitNumber);
            //if (tempTime > time) time = tempTime;
            // Flash pulse
            tempTime = Pulse(startTime, startTime, flashlampPulseLength,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash"]).BitNumber);
            if (tempTime > time) time = tempTime;
            // Q pulse
            if (qTrig) tempTime = Pulse(startTime, flashToQ, Q_PULSE_LENGTH,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["q"]).BitNumber);
            if (tempTime > time) time = tempTime;

            //all following pulses have delay: valveToQ + parameterPulseTime to make them happen relative to the yag firing

            // analog pattern trigger pulse
            tempTime = Pulse(startTime, startTime, 100,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["analogPatternTrigger"]).BitNumber);
            if (tempTime > time) time = tempTime;

            if (tempTime > time) time = tempTime;


            return time;
        }


    }
}
