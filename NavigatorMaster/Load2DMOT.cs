using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster.SnippetLibrary;
using MOTMaster;

namespace NavigatorMaster
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

        public void AddDigitalSnippet(PatternBuilder32 hs, Dictionary<String, Object> parameters)
        {
            int loadtime2D = (int)parameters["2DLoadTime"] * (int)parameters["ScaleFactor"];
            int pushtime = (int)parameters["PushTime"] * (int)parameters["ScaleFactor"];
            
            //Pulse push beam
            hs.DownPulse(loadtime2D, 0, pushtime, "pushaomTTL");

        }
        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {

        }

        public void AddMuquansCommands(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {
           
        }
    }
}
