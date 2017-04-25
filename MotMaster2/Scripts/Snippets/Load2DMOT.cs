using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster2.SnippetLibrary;
using MOTMaster2;

namespace MOTMaster2.SnippetLibrary
{
    public class Load2DMOT : SequenceStep
    {
        public Load2DMOT(HSDIOPatternBuilder hs, Dictionary<String,Object> parameters,double startTime):base (hs,parameters,startTime)
        {
        }
        public Load2DMOT(AnalogPatternBuilder p, Dictionary<String, Object> parameters,double startTime):base (p,parameters,startTime)
        {
        }
        public Load2DMOT(MuquansBuilder mu, Dictionary<String, Object> parameters):base(mu,parameters)
        {
        }

        public override void AddDigitalSnippet(PatternBuilder32 hs, Dictionary<String, Object> parameters)
        {
            int clock = (int)parameters["HSClockFrequency"];
            int loadtime2D = ConvertToSampleTime((double)parameters["2DLoadTime"],clock);

            
            //Pulse push beam for the duration of the 2D mot loading time
            hs.DownPulse(4, 0, loadtime2D, "pushaomTTL");
            SetSequenceEndTime(hs.Layout.LastEventTime, clock);

        }
        public override void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {

        }

        public override void AddMuquansCommands(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {
           
        }

        public int ConvertToSampleTime(double time, int frequency)
        {
            return (int)(time * frequency/1000);
        }
    }
}
