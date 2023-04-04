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
    public class AnalogStaticBuilder
    {
        [DataMember]
        public string[] ChannelNames;
        public int PatternLength;
        private Dictionary<string, AnalogStaticBuilderSingleBoard> boards = new Dictionary<string, AnalogStaticBuilderSingleBoard>();
        public Dictionary<string, AnalogStaticBuilderSingleBoard> Boards
        {
            get { return boards; }
            set { boards = value; }
        }

        public AnalogStaticBuilder(int length)
        {
            PatternLength = length;
        }

        public AnalogStaticBuilder()
        {

        }

        public void AddBoard(string address)
        {
            Boards.Add(address, new AnalogStaticBuilderSingleBoard());
        }

        public AnalogStaticBuilderSingleBoard GetBoard(AnalogOutputChannel channel)
        {
            string boardName = channel.Device;
            if (!Boards.ContainsKey(boardName))
            {
                AddBoard(boardName);
            }
            return Boards[boardName];
        }

        public void AddStaticChannel(string channelName)
        {

            AnalogOutputChannel channel = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[channelName];
            GetBoard(channel).AddStaticChannel(channelName);
        }

        public void AddStaticAnalogValue(string channelName, double value)
        {
            AnalogOutputChannel channel = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[channelName];
            GetBoard(channel).AddStaticAnalogValue(channelName, value);
        }

        public void BuildPattern()
        {
            foreach (AnalogStaticBuilderSingleBoard board in Boards.Values)
            {
                board.BuildPattern();
            }
        }

        
    }
}
