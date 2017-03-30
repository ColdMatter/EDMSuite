using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Pattern;
using DAQ.Analog;

namespace MOTMaster2
{
    public abstract class MOTMasterScript
    {
        public abstract PatternBuilder32 GetDigitalPattern();
        public abstract AnalogPatternBuilder GetAnalogPattern();
        public Dictionary<String,Object> Parameters;
        public abstract MMAIConfiguration GetAIConfiguration();
        public abstract HSDIOPatternBuilder GetHSDIOPattern();
        public abstract MuquansBuilder GetMuquansCommands();


        public MOTMasterSequence GetSequence()
        {
            return GetSequence(false,false);
        }

        public MOTMasterSequence GetSequence(bool hsdio,bool muquans)
        {
            MOTMasterSequence s = new MOTMasterSequence();
            if (hsdio) s.DigitalPattern = GetHSDIOPattern();
            else s.DigitalPattern = GetDigitalPattern();
            s.AnalogPattern = GetAnalogPattern();
            s.AIConfiguration = GetAIConfiguration();
            if (muquans)
                s.MuquansPattern = GetMuquansCommands();
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
