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
        private double sequenceStartTime;
        public double SequenceStartTime
        {
            get { return sequenceStartTime; }
            set { sequenceStartTime = value; }
        }

        private double sequenceEndTime;
        public double SequenceEndTime
        {
            get { return sequenceEndTime; }
            set { sequenceEndTime = value; }
        }
        public virtual void AddDigitalSnippet(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
            throw new NotImplementedException();
        }

        public virtual void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            throw new NotImplementedException();
        }
        public virtual void AddMuquansCommands(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {
            throw new NotImplementedException();
        }

        public SequenceStep(HSDIOPatternBuilder hs, Dictionary<String,Object> parameters, double startTime)
        {
            this.SequenceStartTime = startTime;
            AddDigitalSnippet(hs, parameters);
        }

        public SequenceStep(AnalogPatternBuilder p, Dictionary<String, Object> parameters, double startTime)
        {
            this.SequenceStartTime = startTime;
            AddAnalogSnippet(p, parameters);
        }

        public SequenceStep(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {
            AddMuquansCommands(mu, parameters);
        }

        public SequenceStep()
        {
            throw new Exception("No Parameters or Pattern Builder Passed to Sequence Constructor");
        }

        public int ConvertToSampleTime(double time, int frequency)
        {
            return (int)(time * frequency / 1000);
        }
        public double ConvertToRealTime(int sampleTime, int frequency)
        {
            return sampleTime * 1000.0/frequency;
        }

        public void SetSequenceEndTime(int sampleTime, int frequency)
        {
            double endTime = ConvertToRealTime(sampleTime, frequency);

            if (endTime > this.SequenceEndTime)
                this.SequenceEndTime = endTime;
        }
    }
}
