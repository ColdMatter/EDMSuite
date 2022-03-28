using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Collections;
using System.Linq;
using DAQ.HAL;
using DAQ.Environment;

namespace DAQ.Analog
{
    /// <summary>
    /// thin wrapper around AnalogPatternBuilderSingleBoard
    /// </summary>

    [Serializable, DataContract]
    public class AnalogPatternBuilder
    {
        [DataMember]
        private Dictionary<string, AnalogPatternBuilderSingleBoard> boards = new Dictionary<string, AnalogPatternBuilderSingleBoard>();
        public Dictionary<string, AnalogPatternBuilderSingleBoard> Boards
        {
            get { return boards; }
            set { boards = value; }
        }

        public AnalogPatternBuilder()
        {

        }

        public void AddBoard(string address)
        {
            Boards.Add(address, new AnalogPatternBuilderSingleBoard());
        }

        public AnalogPatternBuilderSingleBoard GetBoard(AnalogOutputChannel channel)
        {
            string boardName = channel.Device;
            if (!Boards.ContainsKey(boardName))
            {
                AddBoard(boardName);
            }
            return Boards[boardName];
        }

        public void AddChannel(string channelName)
        {
            
            AnalogOutputChannel channel = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannel[channelName];
            GetBoard(channel).AddChannel(channel.BitNumber);

        }

        public void AddAnalogValue(string channelName, int time, double value)
        {
            AnalogOutputChannel channel = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannel[channelName];
            GetBoard(channel).AddAnalogValue(channel.BitNumber, time, value);
        }

        //value is the voltage during the pulse
        //finalValue is the voltage AFTER the pulse.
        public void AddAnalogPulse(string channelName, int startTime, int duration, double value, double finalValue)
        {
            AnalogOutputChannel channel = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannel[channelName];
            GetBoard(channel).AddAnalogPulse(channel.BitNumber, startTime, duration, value, finalValue);
        }

        /** Return the minimum length array of the longest pattern. */
        public int GetMinimumLength()
        {
            AnalogPatternBuilderSingleBoard[] boardsList = Boards.Values.ToArray();
            int numBoards = boardsList.Count();
            int[] minLengths = new int[numBoards];
            for (int i = 0; i < numBoards; i++)
            {
                minLengths[i] = boardsList[i].GetMinimumLength();
            }
            return minLengths.Max();
        }
        
        public double GetValue(string channelName, int time)
        {
            AnalogOutputChannel channel = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannel[channelName];
            return GetBoard(channel).GetValue(channel.BitNumber, time);
        }
        
        public void AddLinearRamp(string channelName, int startTime, int steps, double finalValue)
        {
            AnalogOutputChannel channel = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannel[channelName];
            GetBoard(channel).AddLinearRamp(channel.BitNumber, startTime, steps, finalValue);
        }

        public void AddPolynomialRamp(string channelName, int startTime, int stopTime,
            double finalValue, double upperThresholdValue, double lowerThresholdValue,
            double weight1, double weight2, double weight3, double weight4)
        {
            AnalogOutputChannel channel = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannel[channelName];
            GetBoard(channel).AddPolynomialRamp(channel.BitNumber, startTime, stopTime,
                finalValue, upperThresholdValue, lowerThresholdValue,
                weight1, weight2, weight3, weight4);
        }

        public double[,] BuildPattern()
        {
            Pattern = new double[AnalogPatterns.Count, PatternLength];
            ICollection<string> keys = AnalogPatterns.Keys; // will it send all the keys?
            int i = 0;
            foreach (AnalogPatternBuilderSingleBoard board in Boards.Values)
            {
                foreach (string key in keys)
                {
                    double[] d = board.buildSinglePattern(key);
                    for (int j = 0; j < PatternLength; j++)
                    {
                        Pattern[i, j] = d[j];
                    }
                    i++;
                }
            }

            return Pattern;
        }

        public void SwitchOffAtEndOfPattern(string channelName)
        {
            AnalogOutputChannel channel = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannel[channelName];
            GetBoard(channel).SwitchOffAtEndOfPattern(channel.BitNumber);
        }
        public void SwitchAllOffAtEndOfPattern()
        {
            AnalogOutputChannel channel = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannel[channelName];
            GetBoard(channel).SwitchAllOffAtEndOfPattern();
        }
    }
}
