//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Xml.Serialization;

//using Data;

//namespace Analysis.EDM
//{
//    /// <summary>
//    /// This class represents an analysis channel. A channel is simply something which has an
//    /// on, an off and a difference value, and these values can be of any type. Examples could be a channel
//    /// of TOF values, or simply a channel of numbers.
//    /// </summary>
//    /// 
//    [Serializable]
//    [XmlInclude(typeof(GatedChannel))]
//    [XmlInclude(typeof(PointChannel))]
//    [XmlInclude(typeof(TOFChannel))]
//    [XmlInclude(typeof(TOFWithErrorChannel))]
//    public abstract class Channel
//    {
//        public object On;
//        public object Off;
//        public object Difference;
//    }

//    [Serializable]
//    [XmlInclude(typeof(TOFChannel))]
//    [XmlInclude(typeof(GatedChannel))]
//    [XmlInclude(typeof(PointChannel))]
//    [XmlInclude(typeof(TOFWithErrorChannel))]
//    public class Channel<T> : Channel
//    {
//        private T _on;
//        private T _off;
//        private T _difference;
//        public new T On 
//        {
//            get
//            {
//                return _on;
//            }
//            set
//            {
//                _on = value;
//                base.On = value;
//            }
//        }

//        public new T Off
//        {
//            get
//            {
//                return _off;
//            }
//            set
//            {
//                _off = value;
//                base.Off = value;
//            }
//        }

//        public new T Difference
//        {
//            get
//            {
//                return _difference;
//            }
//            set
//            {
//                _difference = value;
//                base.Difference = value;
//            }
//        }
//    }
//}
