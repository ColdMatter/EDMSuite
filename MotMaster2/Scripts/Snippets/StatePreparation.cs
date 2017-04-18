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
            

            SetSequenceEndTime(hs.Layout.LastEventTime, clock);
        }

        public override void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            int clock = (int)parameters["AnalogClockFrequency"];
            
            SetSequenceEndTime(p.GetLastEventTime(), clock); 
        }

        public void AddMuquansCommands(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {

        }

        public int ConvertToSampleTime(double time, int frequency)
        {
            return (int)(time * frequency / 1000);
        }
    }
}
