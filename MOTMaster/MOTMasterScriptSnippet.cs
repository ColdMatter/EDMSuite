using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Pattern;
using DAQ.Analog;

namespace MOTMaster.SnippetLibrary
{
    public abstract class MOTMasterScriptSnippet
    {
        public abstract void AddDigitalSnippet(PatternBuilder32 p, Dictionary<String, Object> parameters);
        public abstract void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters);
    }
}
