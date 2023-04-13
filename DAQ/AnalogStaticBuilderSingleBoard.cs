using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Collections;
using DAQ.HAL;
using DAQ.Environment;

namespace DAQ.Analog
{
    /// <summary>
    /// A class for building analog patterns that can be output by a NI PatternList generator.
    /// This class lets you set a value (AddAnalogValue), do a linear ramp (AddLinearRamp) and have 
    /// an analog pulse (AddAnalogPulse).
    /// 
    /// </summary>

    [Serializable, DataContract]
    public class AnalogStaticBuilderSingleBoard
    {
        [DataMember]
        public Dictionary<String, double> StaticAnalogValues;
        private static Hashtable calibrations = Environs.Hardware.Calibrations;
        public double[] StaticPattern;
        public int PatternLength;

        public AnalogStaticBuilderSingleBoard(int length)
        {
            StaticAnalogValues = new Dictionary<string, double>();
            PatternLength = length;
        }
        public AnalogStaticBuilderSingleBoard()
        {
            StaticAnalogValues = new Dictionary<string, double>();
        }

        public void AddStaticChannel(string channelName)
        {
            double d = 0.0;
            StaticAnalogValues.Add(channelName, d);
        }

        public void AddStaticAnalogValue(string channel, double value)
        {
            StaticAnalogValues[channel] = value;
        }
        

        public double[] BuildPattern()
        {
            StaticPattern = new double[StaticAnalogValues.Count];
            ICollection<string> keys = StaticAnalogValues.Keys;
            int i = 0;
            foreach (string key in keys)
            {
                StaticPattern[i] = StaticAnalogValues[key];
                i++;
            }

            return StaticPattern;
        }

        public class ConflictInPatternException : ApplicationException { }
        public class InsufficientPatternLengthException : ApplicationException { }
        public class PatternBuildException : ApplicationException
        {
            public PatternBuildException(String message) : base(message) { }
        }
    }
}
