using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

using EDMConfig;
using MongoDB.Bson.Serialization.Attributes;

namespace Analysis.EDM
{
    /// <summary>
    /// This class represents a collection of Channels. The type is parameterised by the
    /// type that the channels contain. Note that you can subclass the Channel class
    /// to give different types of channel specific behaviours.
    /// 
    /// It is used to carry the results of demodulating a single detector from a Block into its channels.
    /// </summary>
    /// 
    [Serializable]
    public class ChannelSet<T>
    {
        private readonly Dictionary<string, T> _channelDictionary = new Dictionary<string, T>();

        public T GetChannel(string channelName)
        {
            return _channelDictionary[CanonicalChannelString(channelName)];
        }

        // sometimes its more convenient to use a list of switches rather than a channel name
        public T GetChannel(string[] switches)
        {
            return _channelDictionary[CanonicalChannelString(switches)];
        }
        
        public void AddChannel(string channelName, T channel)
        {
            _channelDictionary.Add(CanonicalChannelString(channelName), channel);
        }

        // sometimes its more convenient to use a list of switches rather than a channel name
        public void AddChannel(string[] switches, T channel)
        {
            _channelDictionary.Add(CanonicalChannelString(switches), channel);
        }
        
        // this sorts the channel in a channel string, giving its canonical representation.
        // It is this representation that is used as the key to the dictionary of channels
        private string CanonicalChannelString(string channelName)
        {
            string[] switches = channelName.Split(new char[] { '.' });
            return CanonicalChannelString(switches);
        }
        private string CanonicalChannelString(string[] switches)
        {
            Array.Sort(switches);
            return ArrayToDottedString(switches);
        }

        private string ArrayToDottedString(string[] switches)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < switches.Length - 1; i++) {
                sb.Append(switches[i]);
                sb.Append(".");
            }
            sb.Append(switches[switches.Length - 1]);
            return sb.ToString();
        }

        static public ChannelSet<T> operator +(ChannelSet<T> cs1, ChannelSet<T> cs2)
        {
            var csNew = new ChannelSet<T>();
            foreach (string channelName in cs1.Channels)
            {
                csNew.AddChannel(channelName, cs1.GetChannel(channelName));
            }
            foreach(string channelName in cs2.Channels)
            {
                csNew.AddChannel(channelName, cs2.GetChannel(channelName));
            }
            return csNew;
        }

        public List<string> Channels
        {
            get
            {
                Dictionary<string, T>.KeyCollection keys = _channelDictionary.Keys;
                List<string> keyArray = new List<string>();
                foreach (string key in keys) keyArray.Add(key);
                return keyArray;
            }
        }

    }
}
