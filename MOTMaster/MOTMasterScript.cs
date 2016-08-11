using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Pattern;
using DAQ.Analog;
using Newtonsoft.Json;

namespace MOTMaster
{
    public abstract class MOTMasterScript
    {
        public abstract PatternBuilder32 GetDigitalPattern();
        public abstract AnalogPatternBuilder GetAnalogPattern();
        //If using multiple cards, make sure that the following methods are overidden as required.
        public bool multipleCards = false;
        public virtual List<PatternBuilder32> GetDigitalPatterns()
        {
            throw new NullSequenceException("The loaded script does not contain the expected collection of digital patterns. Check GetDigitalPatterns() is overidden.");
        }
        public virtual List<AnalogPatternBuilder> GetAnalogPatterns()
        {
            throw new NullSequenceException("The loaded script does not contain the expected collection of analogOutput patterns. Check GetAnalogPatterns() is overidden.");
        }

        
        public Dictionary<String,Object> Parameters;

        public MOTMasterSequence GetSequence()
        {
            MOTMasterSequence s = new MOTMasterSequence();
            if (multipleCards)
            {
                s.multipleCards = true;
                s.DigitalPatterns = GetDigitalPatterns();
                s.AnalogPatterns = GetAnalogPatterns();
                return s;
            }
            else
            {
                s.DigitalPattern = GetDigitalPattern();
                s.AnalogPattern = GetAnalogPattern();
                return s;
            }
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
        public class NullSequenceException : ApplicationException
        {
            public NullSequenceException(string message)
            {
                throw new ApplicationException(message);
            }
        }

    }
}