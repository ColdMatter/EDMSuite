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
        public virtual AnalogStaticBuilder GetAnalogStatic()
        {
            return new AnalogStaticBuilder();
        }
        public Dictionary<String,Object> Parameters;

        public Dictionary<string, List<object>> switchConfiguration = new Dictionary<string, List<object>> { };

        public MOTMasterSequence GetSequence()
        {
            MOTMasterSequence s = new MOTMasterSequence();
            s.DigitalPattern = GetDigitalPattern();
            s.AnalogPattern = GetAnalogPattern();
            s.AnalogStatic = GetAnalogStatic();
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
