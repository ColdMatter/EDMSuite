//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Xml.Serialization;

//namespace Analysis.EDM
//{
//    /// <summary>
//    /// This class holds a number of ChannelSets, one for each manual state. Instances of this
//    /// class are usually assembled by accumulation. It has slots for reporting how many
//    /// channel sets were accumulated for each machine state.
//    /// </summary>
//    [Serializable]
//    [XmlInclude(typeof(TOFChannelSetGroup))]
//    public class ChannelSetGroup<T>
//    {
//        public const int Length = 8;
//        protected const int rfIndex = 1;
//        protected const int bIndex = 2;
//        protected const int eIndex = 4;

//        public ChannelSet<T>[] ChannelSets;

//        public ChannelSet<T> ChannelSetForMachineState(bool eState, bool bState, bool rfState)
//        {
//            return ChannelSets[machineStateIndex(eState, bState, rfState)];
//        }

//        protected int machineStateIndex(bool eState, bool bState, bool rfState)
//        {
//            int index = 0;
//            if (rfState) index += rfIndex;
//            if (bState)  index += bIndex;
//            if (eState)  index += eIndex;
//            return index;
//        }

//        protected bool eState(int index)
//        {
//            return ((index & eIndex) != 0);
//        }

//        protected bool bState(int index)
//        {
//            return ((index & bIndex) != 0);
//        }

//        protected bool rfState(int index)
//        {
//            return ((index & rfIndex) != 0);
//        }
//    }
//}
