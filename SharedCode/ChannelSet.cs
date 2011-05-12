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
    /// It is used to carry the results of demodulating a single detector form a Block into its channels.
    /// </summary>
    [Serializable]
    [XmlInclude(typeof(TOFChannelSet))]
    public class ChannelSet<T>
    {
        [BsonElement("csd")]
        private Dictionary<string, Channel<T>> ChannelDictionary = new Dictionary<string, Channel<T>>();
 
        public Channel<T> GetChannel(string channelName)
        {
            return ChannelDictionary[CanonicalChannelString(channelName)];
        }

        // sometimes its more convenient to use a list of switches rather than a channel name
        public Channel<T> GetChannel(string[] switches)
        {
            return ChannelDictionary[CanonicalChannelString(switches)];
        }
        
        public void AddChannel(string channelName, Channel<T> channel)
        {
            ChannelDictionary.Add(CanonicalChannelString(channelName), channel);
        }

        // sometimes its more convenient to use a list of switches rather than a channel name
        public void AddChannel(string[] switches, Channel<T> channel)
        {
            ChannelDictionary.Add(CanonicalChannelString(switches), channel);
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

        public List<string> Channels
        {
            get
            {
                Dictionary<string, Channel<T>>.KeyCollection keys = ChannelDictionary.Keys;
                List<string> keyArray = new List<string>();
                foreach (string key in keys) keyArray.Add(key);
                return keyArray;
            }
        }

       // Improbable as it seems, .NET doesn't give a way to get the same hashcode for two
        // seemingly identical arrays i.e. for
        // string[] s1 = new string[] {"SIG"}
        // string[] s2 = new string[] {"SIG"}
        // s1.GetHashCode() will not be the same as s2.GetHashCode().
        // To work around this we have to provide our hash method that has this property
        private int HashStringArray(string[] channel)
        {
            int hash = 0x76BC45AD;
            foreach (var sw in channel)
            {
                int swHash = sw.GetHashCode();
                hash = swHash ^ ((hash << 5) + hash);
            }
            return hash;
        }
    }
}
