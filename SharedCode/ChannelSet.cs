using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

using EDMConfig;

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
        // The custom comparer fixes the hashtable so that it works with string 
        // arrays in the way we'd expect (see below)
        private Dictionary<string[], Channel<T>> ChannelDictionary 
            = new Dictionary<string[],Channel<T>>(new ChannelComparer());
 
        public Channel<T> GetChannel(string[] switches)
        {
            Array.Sort(switches);
            return ChannelDictionary[switches];
        }

        public void AddChannel(string[] switches, Channel<T> channel)
        {
            Array.Sort(switches);
            ChannelDictionary.Add(switches, channel);
        }

        public List<string[]> Channels
        {
            get
            {
                Dictionary<string[], Channel<T>>.KeyCollection keys = ChannelDictionary.Keys;
                List<string[]> keyArray = new List<string[]>();
                foreach (string[] key in keys) keyArray.Add(key);
                return keyArray;
            }
        }

        // Improbable as it seems, .NET doesn't give a way to get the same hashcode for two
        // seemingly identical arrays i.e. for
        // string[] s1 = new string[] {"SIG"}
        // string[] s2 = new string[] {"SIG"}
        // s1.GetHashCode() will not be the same as s2.GetHashCode().
        // To work around this we have to provide our own equality comparer to the dictionary.
        [Serializable]
        private class ChannelComparer : IEqualityComparer<string[]>
        {
            #region IEqualityComparer<string[]> Members

            public bool Equals(string[] x, string[] y)
            {
                return HashStringArray(x) == HashStringArray(y);
            }

            public int GetHashCode(string[] obj)
            {
                return HashStringArray(obj);
            }

            #endregion

            // This method hashes an array of strings in a way that only depends on the contents.
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
}
