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
    /// A class to encapsulate MOTMasterScriptSnippets. This is used so that a full script can be defined with relative timings and step names.
    /// </summary>
    public class SequenceStep 
    {
        private MOTMasterScriptSnippet snippet;
        private string name;
        private string description;
        private double sequenceStartTime;
        public bool enabled { get; set; }
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

        /// <summary>
        /// Constructs a SequenceStep
        /// </summary>
        /// <param name="name">Name of the step</param>
        /// <param name="description">Description of the step</param>
        /// <param name="startTime">Starting time in milliseconds</param>
        /// <param name="parameters">Dictionary of parameters used for the step</param>
        public SequenceStep(string name, string description, double startTime, Dictionary<string,object> parameters,List<string>channels)
        {
            this.name = name;
            this.description = description;
            this.sequenceStartTime = startTime;   
        }
        

        public SequenceStep()
        {
            throw new Exception("No Parameters or Pattern Builder Passed to Sequence Constructor");
        }

        /// <summary>
        /// Converts a time from milliseconds into number of samples
        /// </summary>
        /// <param name="time"></param>
        /// <param name="frequency"></param>
        /// <returns></returns>
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
