using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster2;



namespace MOTMaster2.SnippetLibrary
{
    public class Detection : SequenceStep
    {

        public Detection(HSDIOPatternBuilder hs, Dictionary<String,Object> parameters, double startTime):base(hs,parameters,startTime){}
        public Detection(AnalogPatternBuilder p, Dictionary<String, Object> parameters, double startTime) : base(p, parameters, startTime) { }
        public Detection(MuquansBuilder mu, Dictionary<String, Object> parameters) : base(mu, parameters) { }
        public override void AddDigitalSnippet(PatternBuilder32 hs, Dictionary<String, Object> parameters)
        {
            int clock = (int)parameters["HSClockFrequency"];
            int startTime = ConvertToSampleTime(this.SequenceStartTime, clock);
            int dwellTIme = ConvertToSampleTime(0.25, clock);
            int msWait = ConvertToSampleTime(1.0, clock);

            //Image atoms in N2 and Ntot
            hs.Pulse(startTime, 0, dwellTIme, "acquisitionTrigger");
            hs.DownPulse(startTime + dwellTIme, 0, msWait, "zpaomTTL");
            hs.DownPulse(startTime + dwellTIme, 0, msWait, "zmaomTTL");
            hs.Pulse(startTime + dwellTIme + msWait / 2, 0, msWait / 2, "mphiTTL");
            //Blow away for background
            hs.DownPulse(startTime + dwellTIme + msWait + dwellTIme, 0, 3 * msWait, "zpaomTTL");
            startTime += dwellTIme + msWait + dwellTIme + 3* msWait;

            hs.DownPulse(startTime + dwellTIme, 0, msWait, "zpaomTTL");
            hs.DownPulse(startTime + dwellTIme, 0, msWait, "zmaomTTL");
            hs.Pulse(startTime + dwellTIme + msWait / 2, 0, msWait / 2, "mphiTTL");
            startTime += dwellTIme + msWait + dwellTIme;
            //Take reference level
            hs.DownPulse(startTime + dwellTIme, 0, msWait / 2, "zpaomTTL"); 
            hs.DownPulse(startTime + dwellTIme, 0, msWait / 2, "zmaomTTL");

            SetSequenceEndTime(hs.Layout.LastEventTime, clock);

        }

        public override void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            int clock = (int)parameters["AnalogClockFrequency"];
            int startTime = ConvertToSampleTime(this.SequenceStartTime, clock);

            p.AddAnalogValue("motCTRL", startTime, 0.48);

            SetSequenceEndTime(p.GetLastEventTime(), clock); 
            
        }

        public void AddMuquansCommands(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {
            mu.SetFrequency("slave0",1.5);
            mu.SetFrequency("mphi", 0.0);
        }

      
    }
}
