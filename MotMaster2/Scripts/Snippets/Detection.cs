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

        }

        public override void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {

        }

        public void AddMuquansCommands(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {
           
        }

      
    }
}
