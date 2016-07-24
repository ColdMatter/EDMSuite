using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MOTMaster;
using MOTMaster.SnippetLibrary;

using DAQ.Pattern;
using DAQ.Analog;

public class Patterns : MOTMasterScript
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
 
    }
    public HSDIOPatternBuilder GetDigitalPattern()
    {
        HSDIOPatternBuilder p = new HSDIOPatternBuilder();
        p.Pulse(0, 0, 1, "AnalogPatternTrigger");

        return p;
    }
    public override AnalogPatternBuilder GetAnalogPattern()
    {
        throw new NotImplementedException();
    }
}
