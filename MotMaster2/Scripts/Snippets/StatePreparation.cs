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
            hs.Pulse(startTime - 40000, 0, 200, "serialPreTrigger");

            hs.Pulse(startTime, serialWait, 200, "slaveDDSTrig");
            hs.Pulse(startTime, serialWait, 200, "aomDDSTrig");

            hs.AddEdge("xaomTTL", startTime + repumpTime, true);
            hs.AddEdge("yaomTTL", startTime + repumpTime, true);
            hs.AddEdge("zpaomTTL", startTime + repumpTime, true);
            hs.AddEdge("zmaomTTL", startTime + repumpTime, true);

            hs.DownPulse(startTime+repumpTime+serialWait,0,pumptime22,"zpaomTTL");
            hs.DownPulse(startTime + repumpTime + serialWait, 0, pumptime22, "zmaomTTL");

            hs.AddEdge("motTTL", startTime + repumpTime +serialWait+ pumptime22, false);
            hs.AddEdge("mphiTTL", startTime + repumpTime +serialWait+ pumptime22, false);
            


            SetSequenceEndTime(hs.Layout.LastEventTime, clock);
        }

        public override void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            int clock = (int)parameters["AnalogClockFrequency"];
            int startTime = ConvertToSampleTime(this.SequenceStartTime,clock);
            int repumpTime = ConvertToSampleTime((double)parameters["PrepRepumpDuration"],clock);
            p.AddAnalogPulse("mphiCTRL", startTime, repumpTime, 0.35, (double)parameters["RepumpPower"]);
            p.AddAnalogValue("motCTRL", startTime, (double)parameters["MotPower"]);
            SetSequenceEndTime(p.GetLastEventTime(), clock); 
        }

        public override void AddMuquansCommands(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {
            //Sets slave0 on to 2->2 taking account of the extra 1.5 MHz detuning from the Fibre AOMs
            mu.SetFrequency("Slave0", -268.151);
            //Puts the mphi sideband at the 1->0 transition
            mu.SetFrequency("mphi", -37.421);
        }

       
    }
}
