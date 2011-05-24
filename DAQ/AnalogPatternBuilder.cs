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
        public ArrayList PatternList;
        public ArrayList ChannelNames;
        public int PatternLength;
        public double[,] Pattern;

        public AnalogPatternBuilder(string[] channelNames, int patternLength)
        {
            PatternList = new ArrayList();
            ChannelNames = new ArrayList();
            PatternLength = patternLength;
            int numberOfChannels = channelNames.GetLength(0);
            for (int i = 0; i < numberOfChannels; i++)
            {
                AddChannel(channelNames[i]);
            }
        }
        public AnalogPatternBuilder(int patternLength)
        {
            PatternList = new ArrayList();
            ChannelNames = new ArrayList();
            PatternLength = patternLength;
        }

        public void AddChannel(string channelName)
        {
            double[] data = new double[PatternLength];
                for (int j = 0; j < PatternLength; j++)
                {
                    data[j] = 0;
                }
            PatternList.Add(data);
            ChannelNames.Add(channelName);
        }

        public void AddAnalogValue(string channel, int time, double value)
        {
            if (time < ((double[])PatternList[SearchPatternIndex(channel)]).GetLength(0))
            {
                for (int i = time; i < ((double[])PatternList[SearchPatternIndex(channel)]).GetLength(0); i++)
                {
                    ((double[])PatternList[SearchPatternIndex(channel)])[i] = value;
                }
            }
            else
            {
                throw new InsufficientPatternLengthException();
            }
        }

        


        public void AddLinearRamp(string channel, int startTime, int steps, double finalValue)
        {
            int targetIndex = SearchPatternIndex(channel);
            if (PatternLength > startTime + steps)
            {
                double stepSize = (finalValue - ((double[])PatternList[targetIndex])[startTime]) / steps;
                for (int i = 0; i < steps; i++)
                {
                    ((double[])PatternList[targetIndex])[startTime + i] =
                        ((double[])PatternList[targetIndex])[startTime + i] + stepSize * i;
                }
                AddAnalogValue(channel, startTime + steps, finalValue);
            }
            else
            {
                throw new InsufficientPatternLengthException();
            }
        }

        public int SearchPatternIndex(string channel)
        {
            return ChannelNames.BinarySearch((Object)channel);
        }

        public double[,] BuildPattern()
        {
            Pattern = new double[PatternList.Count,PatternLength];
            for (int i = 0; i < PatternList.Count; i++)
            {
                for (int j = 0; j < PatternLength; j++)
                {
                    Pattern[i, j] = ((double[])PatternList[i])[j];
                }
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
