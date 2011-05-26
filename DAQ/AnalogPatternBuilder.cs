using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace DAQ.Analog
{
    /// <summary>
    /// A class for building analog patterns that can be output by a NI PatternList generator.
    /// This class lets you set a value (AddAnalogValue) and do a linear ramp (AddLinearRamp).
    /// 
    /// IMPORTANT NOTE ABOUT WRITING SCRIPTS: At the moment, THERE IS NO AUTOMATIC TIME ORDERING FOR ANALOG
    /// CHANNELS. IT WILL BUILD A PATTERN FOLLOWING THE ORDER IN WHICH YOU CALL AddAnalogValue / AddLinearRamp!!
    /// ---> Stick to writing out the pattern in the correct time order to avoid weirdo behaviour.
    /// </summary>
    public class AnalogPatternBuilder
    {
        public Dictionary<String,Double[]> AnalogPatterns;
        public int PatternLength;
        public double[,] Pattern;

        public AnalogPatternBuilder(string[] channelNames, int patternLength)
        {
            AnalogPatterns = new Dictionary<string, double[]>();
            PatternLength = patternLength;
            int numberOfChannels = channelNames.GetLength(0);
            for (int i = 0; i < numberOfChannels; i++)
            {
                AddChannel(channelNames[i]);
            }
        }
        public AnalogPatternBuilder(int patternLength)
        {
            AnalogPatterns = new Dictionary<string, double[]>();
            PatternLength = patternLength;
        }

        public void AddChannel(string channelName)
        {
            double[] data = new double[PatternLength];
            for (int j = 0; j < PatternLength; j++)
            {
               data[j] = 0;
            }
            AnalogPatterns[channelName] = data;
        }

        public void AddAnalogValue(string channel, int time, double value)
        {
            if (time < PatternLength)
            {
                for (int i = time; i < PatternLength; i++)
                {
                    ((double[])AnalogPatterns[channel])[i] = value;
                }
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



        public void AddLinearRamp(string channel, int startTime, int steps, double finalValue)
        {
            if (PatternLength > startTime + steps)
            {
                double stepSize = (finalValue - ((double[])AnalogPatterns[channel])[startTime]) / steps;
                for (int i = 0; i < steps; i++)
                {
                    ((double[])AnalogPatterns[channel])[startTime + i] =
                        ((double[])AnalogPatterns[channel])[startTime + i] + stepSize * i;
                }
                AddAnalogValue(channel, startTime + steps, finalValue);
            }
            else
            {
                throw new InsufficientPatternLengthException();
            }
        }

        public double[,] BuildPattern()
        {
            Pattern = new double[AnalogPatterns.Count, PatternLength];
            ICollection<string> keys = AnalogPatterns.Keys;
            int i = 0;
            foreach(string key in keys)
            {
                for (int j = 0; j < PatternLength ; j++)
                {
                    Pattern[i, j] = ((double[])AnalogPatterns[key])[j];
                }
                i++;
            }
            return Pattern;
        }

        public class InsufficientPatternLengthException : ApplicationException { }
        public class PatternBuildException : ApplicationException
        {
            public PatternBuildException(String message) : base(message) { }
        }
    }
}
