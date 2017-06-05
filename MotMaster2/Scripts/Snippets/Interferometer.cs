using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster2;



namespace MOTMaster2.SnippetLibrary
{
    public class Interferometer : SequenceStep
    {
        public Interferometer(HSDIOPatternBuilder hs, Dictionary<String,Object> parameters, double startTime):base(hs,parameters,startTime){}
        public Interferometer(AnalogPatternBuilder p, Dictionary<String, Object> parameters, double startTime) : base(p, parameters, startTime) { }
        public Interferometer(MuquansBuilder mu, Dictionary<String, Object> parameters) : base(mu, parameters) { }
        public override void AddDigitalSnippet(PatternBuilder32 hs, Dictionary<String, Object> parameters)
        {
           
            int clock = (int)parameters["HSClockFrequency"];
            int startTime = ConvertToSampleTime(this.SequenceStartTime, clock);
            
          
            


            SetSequenceEndTime(hs.Layout.LastEventTime, clock);
        }

        public override void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            int clock = (int)parameters["AnalogClockFrequency"];
            int startTime = ConvertToSampleTime(this.SequenceStartTime,clock);
         
            SetSequenceEndTime(p.GetLastEventTime(), clock); 
        }

        public override void AddMuquansCommands(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {
            
        }

       
    }
}
