using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

using Data;

namespace Analysis.EDM
{
    /// <summary>
    /// This class represents an analysis channel. A channel is simply something which has an
    /// on, an off and a difference value, and these values can be of any type. Examples could be a channel
    /// of TOF values, or simply a channel of numbers.
    /// </summary>
    /// 
    public abstract class Channel
    {
        public object On;
        public object Off;
        public object Difference;
    }

    [Serializable]
    [XmlInclude(typeof(TOFChannel))]
    [XmlInclude(typeof(GatedChannel))]
    [XmlInclude(typeof(PointChannel))]
    public class Channel<T> : Channel
    {
        public new T On;
        public new T Off;      
        public new T Difference;
    }
}
