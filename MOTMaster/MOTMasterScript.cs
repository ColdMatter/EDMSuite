using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Pattern;
using DAQ.Analog;

namespace MOTMaster
{
    public abstract class MOTMasterScript
    {
        public abstract PatternBuilder32 GetDigitalPattern();
        public abstract AnalogPatternBuilder GetAnalogPattern();

        public abstract Dictionary<string, PatternBuilder32> GetDigitalPatterns();
        public abstract Dictionary<string, AnalogPatternBuilder> GetAnalogPatterns();
        public abstract Dictionary<string, HSDIOPatternBuilder> GetHSDIOPatterns();

        public Dictionary<String,Object> Parameters;

        public MOTMasterSequence GetSequence()
        {
            MOTMasterSequence s = new MOTMasterSequence();
            s.DigitalPattern = GetDigitalPattern();
            s.AnalogPattern = GetAnalogPattern();
            return s;
        }
        public MOTMasterSequence GetMultiSequence()
        {
            MOTMasterSequence s = new MOTMasterSequence();
            s.multipleCards = true;
            s.DigitalPatterns = GetDigitalPatterns();
            s.AnalogPatterns = GetAnalogPatterns();
            s.HSDIOPatterns = GetHSDIOPatterns();
            return s;
        }

        public void EditDictionary(Dictionary<String, Object> dictionary)
        {
            foreach (KeyValuePair<string, object> k in dictionary)
            {
                if (Parameters.ContainsKey(k.Key))
                {
                    Parameters[k.Key] = k.Value;
                }
                else
                {
                    throw new ParameterNotInOriginalDictionaryException();
                }
            }
        }
        public class ParameterNotInOriginalDictionaryException : ApplicationException { }
    }
}
