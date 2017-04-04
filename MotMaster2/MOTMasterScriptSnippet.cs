using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Pattern;
using DAQ.Analog;

namespace MOTMaster2.SnippetLibrary
{
    public interface MOTMasterScriptSnippet
    {
        void AddDigitalSnippet(PatternBuilder32 p, Dictionary<String, Object> parameters);
        void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters);


    }


}
