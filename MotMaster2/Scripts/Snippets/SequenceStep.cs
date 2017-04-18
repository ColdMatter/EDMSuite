using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster2;

namespace MOTMaster2.SnippetLibrary
{
    /// <summary>
    /// Base class for MOTMaster sequences
    /// </summary>
    public class SequenceStep : MOTMasterScriptSnippet
    {
        private int analogEndTime;
        public int AnalogEndTime
        {
            get { return analogEndTime; }
            set { analogEndTime = value; }
        }

        private int analogStartTime;
        public int AnalogStartTime
        {
            get { return analogStartTime; }
            set { analogStartTime = value; }
        }
        private int digitalStartTime;
        
        private int digitalEndTime;
        public int DigitalStartTime
        {
            get { return digitalStartTime; }
            set { digitalStartTime = value; }
        }
        public int DigitalEndTime
        {
            get { return digitalEndTime; }
            set { digitalEndTime = value; }
        }

        public virtual void AddDigitalSnippet(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
            throw new NotImplementedException();
        }

        public virtual void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            throw new NotImplementedException();
        }

        public int ConvertToSampleTime(double time, int frequency)
        {
            return (int)(time * frequency / 1000);
        }
    }
}
