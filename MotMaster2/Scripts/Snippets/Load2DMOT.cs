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
    public class Load2DMOT : MOTMasterScriptSnippet
    {
        public Load2DMOT(HSDIOPatternBuilder hs, Dictionary<String,Object> parameters)
        {
            AddDigitalSnippet(hs, parameters);
        }
        public Load2DMOT(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            AddAnalogSnippet(p, parameters);
        }
        public Load2DMOT(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {
            AddMuquansCommands(mu, parameters);
        }
        public Load2DMOT()
        {
            Console.WriteLine("No Parameters");
        }
        public void AddDigitalSnippet(PatternBuilder32 hs, Dictionary<String, Object> parameters)
        {
            int clock = (int)parameters["HSClockFrequency"];
            int loadtime2D = ConvertToSampleTime((double)parameters["2DLoadTime"],clock);
           
            
            //Pulse push beam for the duration of the 2D mot loading time
            hs.DownPulse(4, 0, loadtime2D, "pushaomTTL");

        }
        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {

        }

        public void AddMuquansCommands(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {
           
        }

        public int ConvertToSampleTime(double time, int frequency)
        {
            return (int)(time * frequency);
        }
    }
}
