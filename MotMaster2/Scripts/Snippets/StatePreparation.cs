using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster2;



namespace MOTMaster2.SnippetLibrary
{
    public class StatePreparation : SequenceStep
    {
        public StatePreparation(HSDIOPatternBuilder hs, Dictionary<String,Object> parameters, double startTime):base(hs,parameters,startTime){}
        public StatePreparation(AnalogPatternBuilder p, Dictionary<String, Object> parameters, double startTime) : base(p, parameters, startTime) { }
        public StatePreparation(MuquansBuilder mu, Dictionary<String, Object> parameters) : base(mu, parameters) { }
        public override void AddDigitalSnippet(PatternBuilder32 hs, Dictionary<String, Object> parameters)
        {
            //Switch off the magnetic field and wait some time
            int clock = (int)parameters["HSClockFrequency"];
            int startTime = ConvertToSampleTime(this.SequenceStartTime, clock);
            int serialWait = ConvertToSampleTime(1.0, clock);
            int repumpTime = ConvertToSampleTime((double)parameters["PrepRepumpDuration"], clock);
            int pumptime22 = ConvertToSampleTime((double)parameters["22PumpTime"],clock);
            int statePrepPresetTime = ConvertToSampleTime(1.0,clock);
            int fieldSwitchTime = ConvertToSampleTime((double)parameters["BRamanFieldSwitchTime"], clock);
            int mWaveDuration = ConvertToSampleTime((double)parameters["MicrowaveDuration"],clock);

            hs.Pulse(startTime - 40000, 0, 200, "serialPreTrigger");

            //Triggers the muquans to set the laser frequencies for state preparation
            hs.Pulse(startTime+repumpTime, serialWait, 200, "slaveDDSTrig");
            hs.Pulse(startTime+repumpTime, serialWait, 200, "aomDDSTrig");

            //Switches off all the MOT beams
            hs.AddEdge("xaomTTL", startTime + statePrepPresetTime + repumpTime, true);
            hs.AddEdge("yaomTTL", startTime + statePrepPresetTime + repumpTime, true);
            hs.AddEdge("zpaomTTL", startTime +  statePrepPresetTime + repumpTime, true);
            hs.AddEdge("zmaomTTL", startTime +  statePrepPresetTime + repumpTime, true);

            int aomOnTime;
            if ((bool)parameters["Pump|1,0>"]) aomOnTime = pumptime22 + ConvertToSampleTime((double)parameters["10PumpTime"],clock);
            else aomOnTime = pumptime22;
            
            //Pulses the z mot beams to pump the atoms
            hs.DownPulse(startTime+repumpTime+serialWait+statePrepPresetTime,0,aomOnTime,"zpaomTTL");
            hs.DownPulse(startTime + repumpTime + serialWait + statePrepPresetTime, 0, aomOnTime, "zmaomTTL");
            
            //Switches off repump after pumping all atoms to F=2
            hs.AddEdge("mphiTTL", startTime + repumpTime +serialWait, false);
            
            //Adds a 10us pulse to drive 1->0
            hs.Pulse(startTime + repumpTime +serialWait+ pumptime22+statePrepPresetTime,0,ConvertToSampleTime(0.01,clock),"mphiTTL");

            //Pulses the microwave horn after waiting for a time to switch the magnetic field along the Raman axis (2ms)
            int mWaveStartTime = startTime + repumpTime +serialWait+ pumptime22+statePrepPresetTime + ConvertToSampleTime(0.01,clock) + fieldSwitchTime;
            hs.Pulse(mWaveStartTime, 0, mWaveDuration, "microwaveTrigger");

            SetSequenceEndTime(hs.Layout.LastEventTime, clock);
        }

        public override void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            int clock = (int)parameters["AnalogClockFrequency"];
            int startTime = ConvertToSampleTime(this.SequenceStartTime, clock);
            int serialWait = ConvertToSampleTime(1.0, clock);
            int repumpTime = ConvertToSampleTime((double)parameters["PrepRepumpDuration"], clock);
            int pumptime22 = ConvertToSampleTime((double)parameters["22PumpTime"], clock);
            int statePrepPresetTime = ConvertToSampleTime(1.0, clock);
            int fieldSwitchTime = ConvertToSampleTime((double)parameters["BRamanFieldSwitchTime"], clock);
            int mWaveDuration = ConvertToSampleTime((double)parameters["MicrowaveDuration"], clock);

            //Maximises the amount of power in the repump sideband to pump all atoms to F=2
            p.AddAnalogPulse("mphiCTRL", startTime, repumpTime, 0.38, 0.32);
            p.AddAnalogValue("motCTRL", startTime, (double)parameters["MotPower"]);

            //Sets the z bias coil to maximum (this is for state selection)
            p.AddAnalogValue("zbiasCoil", startTime + repumpTime, 9.0);

            //After pumping and before microwave pulse, the magnetic field is varied so that it points along the Raman axis (the yBias coils)
            int bSwitchTime1 = startTime + repumpTime +serialWait+ pumptime22+statePrepPresetTime + ConvertToSampleTime(0.01,clock);
            int bSwitchTime2 = startTime + repumpTime +serialWait+ pumptime22+statePrepPresetTime + ConvertToSampleTime(0.01,clock) + fieldSwitchTime/2;
            p.AddAnalogValue("ybiasCoil",bSwitchTime1,9.0);
            p.AddAnalogValue("zbiasCoil", bSwitchTime2, (double)parameters["ZBias"]);


            SetSequenceEndTime(p.GetLastEventTime(), clock); 
        }

        public override void AddMuquansCommands(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {
            //Sets slave0 on to 2->2 taking account of the extra 1.5 MHz detuning from the Fibre AOMs
            mu.SetFrequency("Slave0", (double)parameters["StatePrepSlavedetuning"]);
            //Puts the mphi sideband at the 1->0 transition
            mu.SetFrequency("mphi", (double)parameters["StatePrepMPhidetuning"]);
        }

       
    }
}
