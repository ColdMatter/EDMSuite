using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Collections;
using System.Linq;
using DAQ.HAL;
using DAQ.Environment;
using DAQ.Pattern;

namespace DAQ.Analog
{
    /// <summary>
    /// thin wrapper around AnalogPatternBuilderSingleBoard
    /// </summary>

    [Serializable, DataContract]
    public class AnalogPatternBuilder
    {
        [DataMember]
        public int PatternLength;
        public string[] ChannelNames;
        private Dictionary<string, AnalogPatternBuilderSingleBoard> boards = new Dictionary<string, AnalogPatternBuilderSingleBoard>();
        public Dictionary<string, AnalogPatternBuilderSingleBoard> Boards
        {
            get { return boards; }
            set { boards = value; }
        }

        public AnalogPatternBuilder(string[] channelNames, int patternLength)
        {
            ChannelNames = channelNames;
            PatternLength = patternLength + LinkedLengthExtension();
        }

        public AnalogPatternBuilder(int patternLength)
        {
            PatternLength = patternLength + LinkedLengthExtension();
        }

        // Inside a linked run the secondary's analog pattern is padded so the shifted events fit; otherwise 0.
        private static int LinkedLengthExtension()
        {
            LinkRouting routing = LinkRouting.Current;
            return routing == null ? 0 : routing.AnalogLengthExtension;
        }

        public void AddBoard(string address)
        {
            Boards.Add(address, new AnalogPatternBuilderSingleBoard(PatternLength));
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
            LinkRouting routing = LinkRouting.Current;
            if (routing != null && !routing.IsLocalAnalog(channelName)) return;
            AnalogOutputChannel channel = LinkRouting.LookupAnalog(channelName);
            GetBoard(channel).AddChannel(channelName);
        }

        public void AddAnalogValue(string channelName, int time, double value)
        {
            LinkRouting routing = LinkRouting.Current;
            if (routing != null && !routing.IsLocalAnalog(channelName)) return;
            if (routing != null) time = routing.MapAnalogPoint(channelName, time);
            AnalogOutputChannel channel = LinkRouting.LookupAnalog(channelName);
            GetBoard(channel).AddAnalogValue(channelName, time, value);
        }

        //value is the voltage during the pulse
        //finalValue is the voltage AFTER the pulse.
        public void AddAnalogPulse(string channelName, int startTime, int duration, double value, double finalValue)
        {
            LinkRouting routing = LinkRouting.Current;
            if (routing != null && !routing.IsLocalAnalog(channelName)) return;
            if (routing != null) startTime = routing.MapAnalogSpan(channelName, startTime);
            AnalogOutputChannel channel = LinkRouting.LookupAnalog(channelName);
            GetBoard(channel).AddAnalogPulse(channelName, startTime, duration, value, finalValue);
        }

        public double GetValue(string channelName, int time)
        {
            LinkRouting routing = LinkRouting.Current;
            if (routing != null)
            {
                if (!routing.IsLocalAnalog(channelName))
                    throw new LinkRoutingException("GetValue('" + channelName + "') reads a channel that belongs to the other system and is not supported in a linked run");
                time = Math.Max(0, routing.ShiftAnalog(time));
            }
            AnalogOutputChannel channel = LinkRouting.LookupAnalog(channelName);
            return GetBoard(channel).GetValue(channelName, time);
        }
        
        public void AddLinearRamp(string channelName, int startTime, int steps, double finalValue)
        {
            LinkRouting routing = LinkRouting.Current;
            if (routing != null && !routing.IsLocalAnalog(channelName)) return;
            if (routing != null) startTime = routing.MapAnalogSpan(channelName, startTime);
            AnalogOutputChannel channel = LinkRouting.LookupAnalog(channelName);
            GetBoard(channel).AddLinearRamp(channelName, startTime, steps, finalValue);
        }

        public void AddPolynomialRamp(string channelName, int startTime, int stopTime,
            double finalValue, double upperThresholdValue, double lowerThresholdValue,
            double weight1, double weight2, double weight3, double weight4)
        {
            LinkRouting routing = LinkRouting.Current;
            if (routing != null && !routing.IsLocalAnalog(channelName)) return;
            if (routing != null)
            {
                int mappedStart = routing.MapAnalogSpan(channelName, startTime);
                stopTime += mappedStart - startTime;
                startTime = mappedStart;
            }
            AnalogOutputChannel channel = LinkRouting.LookupAnalog(channelName);
            GetBoard(channel).AddPolynomialRamp(channelName, startTime, stopTime,
                finalValue, upperThresholdValue, lowerThresholdValue,
                weight1, weight2, weight3, weight4);
        }

        public void BuildPattern()
        {
            foreach (AnalogPatternBuilderSingleBoard board in Boards.Values)
            {
                board.BuildPattern();
            }
        }

        public void SwitchOffAtEndOfPattern(string channelName)
        {
            LinkRouting routing = LinkRouting.Current;
            if (routing != null && !routing.IsLocalAnalog(channelName)) return;
            AnalogOutputChannel channel = LinkRouting.LookupAnalog(channelName);
            GetBoard(channel).SwitchOffAtEndOfPattern(channelName);
        }
        public void SwitchAllOffAtEndOfPattern()
        {
            foreach (AnalogPatternBuilderSingleBoard board in Boards.Values)
            {
                board.SwitchAllOffAtEndOfPattern();
            }
        }
    }
}
