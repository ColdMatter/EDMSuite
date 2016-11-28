using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ;
using DAQ.Pattern;
using MOTMaster;

public class Patterns : MOTMasterScript
{
    public Patterns()
    {
        Parameters =  new Dictionary<string,object>();
    }

    public override Dictionary<string, PatternBuilder32> GetDigitalPatterns()
    {
        throw new NotImplementedException();
    }

    public override HSDIOPatternBuilder GetHSDIOPattern()
    {
       throw new NotImplementedException();
    }

    public override DAQ.Analog.AnalogPatternBuilder GetAnalogPattern()
    {
        throw new NotImplementedException();
    }
}

