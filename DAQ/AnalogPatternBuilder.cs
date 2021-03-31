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
    public class AnalogPatternBuilder
    {
        [DataMember]
        public Dictionary<String, Dictionary<Int32, Double>> AnalogPatterns;
        public int PatternLength;
        public double[,] Pattern;
        private static Hashtable calibrations = Environs.Hardware.Calibrations;

        public AnalogPatternBuilder(string[] channelNames, int patternLength)
        {
            AnalogPatterns = new Dictionary<string, Dictionary<int, double>>();
            PatternLength = patternLength;
            int numberOfChannels = channelNames.GetLength(0);
            for (int i = 0; i < numberOfChannels; i++)
            {
                AddChannel(channelNames[i]);
            }
        }
        public AnalogPatternBuilder(int patternLength)
        {
            AnalogPatterns = new Dictionary<string, Dictionary<int, double>>();
            PatternLength = patternLength;
        }

        public void AddChannel(string channelName)
        {
            Dictionary<int, double> d = new Dictionary<int, double>();
            AnalogPatterns.Add(channelName, d);

        }

        public void AddAnalogValue(string channel, int time, double value)
        {
            if (time < PatternLength)
            {
                AnalogPatterns[channel][time] = value;
            }
            else
            {
                throw new InsufficientPatternLengthException();
            }
        }

        //value is the voltage during the pulse
        //finalValue is the voltage AFTER the pulse.
        public void AddAnalogPulse(string channel, int startTime, int duration, double value, double finalValue)
        {
            if (startTime + duration < PatternLength)
            {
                AddAnalogValue(channel, startTime, value);
                AddAnalogValue(channel, startTime + duration, finalValue);
            }
            else
            {
                throw new InsufficientPatternLengthException();
            }
        }

        private List<int> getSortedListOfEvents(string channel)
        {
            List<int> ints = new List<int>(AnalogPatterns[channel].Keys);
            ints.Sort();

            return ints;
        }
        public double GetValue(string channel, int time)
        {
            List<int> events = getSortedListOfEvents(channel);
            double val = 0.0;
            for (int i = 0; i < AnalogPatterns[channel].Count; i++)
            {
                if (events[i] <= time)
                {
                    val = AnalogPatterns[channel][events[i]];
                }
            }
            return val;
        }
        public void AddLinearRamp(string channel, int startTime, int steps, double finalValue)
        {
            if (PatternLength > startTime + steps)
            {
                double startValue = GetValue(channel, startTime);
                double stepSize = (finalValue - startValue) / steps;
                for (int i = 0; i < steps; i++)
                {
                    if (AnalogPatterns[channel].ContainsKey(startTime + i) == false)
                    {
                        AddAnalogValue(channel, startTime + i, startValue + (i + 1) * stepSize);
                    }
                    else
                    {
                        throw new ConflictInPatternException();
                    }
                }
            }
            else
            {
                throw new InsufficientPatternLengthException();
            }
        }
        public void AddPolynomialRamp(string channel, int startTime, int stopTime,
            double finalValue, double upperThresholdValue, double lowerThresholdValue,
            double weight1, double weight2, double weight3, double weight4)
        {
            if (PatternLength > stopTime)
            {
                double startValue = GetValue(channel, startTime);
                int steps = stopTime - startTime;
                for (int i = 0; i < steps; i++)
                {
                    if (AnalogPatterns[channel].ContainsKey(startTime + i) == false)
                    {
                        double t = 1.0 * steps;
                        double it = 1.0 * i;
                        double first = weight1 * (it / t);
                        double second = weight2 * (it / t) * (it / t);
                        double third = weight3 * (it / t) * (it / t) * (it / t);
                        double fourth = weight4 * (it / t) * (it / t) * (it / t) * (it / t);
                        double norm = startValue + weight1 + weight2 + weight3 + weight4;
                        double value = finalValue * (startValue + first + second + third + fourth) / norm;
                        if (value > upperThresholdValue)
                        {
                            AddAnalogValue(channel, startTime + i, upperThresholdValue);
                        }
                        else if (value < lowerThresholdValue)
                        {
                            AddAnalogValue(channel, startTime + i, lowerThresholdValue);
                        }
                        else
                        {
                            AddAnalogValue(channel, startTime + i, value);
                        }
                    }
                    else
                    {
                        throw new ConflictInPatternException();
                    }
                }
            }
            else
            {
                throw new InsufficientPatternLengthException();
            }
        }

        //For a single channel, gets a sequence of events (changes to the output value) and builds a pattern.
        private double[] buildSinglePattern(string channel)
        {
            double[] d = new double[PatternLength];
            List<int> events = getSortedListOfEvents(channel);
            int timeUntilNextEvent = 0;
            events.Add(PatternLength);
            if (calibrations.ContainsKey(channel))
            {
                for (int i = 0; i < events.Count - 1; i++)
                {
                    timeUntilNextEvent = events[i + 1] - events[i];
                    double dval = AnalogPatterns[channel][events[i]];
                    for (int j = 0; j < timeUntilNextEvent; j++)
                    {
                        d[events[i] + j] = ((Calibration)calibrations[channel]).Convert(dval);
                    }

                }
            }
            else
            {
                for (int i = 0; i < events.Count - 1; i++)
                {
                    timeUntilNextEvent = events[i + 1] - events[i];
                    double dval = AnalogPatterns[channel][events[i]];
                    for (int j = 0; j < timeUntilNextEvent; j++)
                    {
                        d[events[i] + j] = dval;
                    }

                }
            }
            

            return d;
        }


        public double[,] BuildPattern()
        {
            Pattern = new double[AnalogPatterns.Count, PatternLength];
            ICollection<string> keys = AnalogPatterns.Keys;
            int i = 0;
            foreach (string key in keys)
            {
                double[] d = buildSinglePattern(key);
                for (int j = 0; j < PatternLength; j++)
                {
                    Pattern[i, j] = d[j];
                }
                i++;
            }

            return Pattern;
        }

        public void SwitchOffAtEndOfPattern(string channel)
        {
            AddAnalogValue(channel, PatternLength - 1, 0.0);
        }
        public void SwitchAllOffAtEndOfPattern()
        {
            ICollection<string> keys = AnalogPatterns.Keys;
            foreach (string key in keys)
            {
                AddAnalogValue(key, PatternLength - 1, 0.0);
            }
        }

        public class ConflictInPatternException : ApplicationException { }
        public class InsufficientPatternLengthException : ApplicationException { }
        public class PatternBuildException : ApplicationException
        {
            public PatternBuildException(String message) : base(message) { }
        }
    }
}
