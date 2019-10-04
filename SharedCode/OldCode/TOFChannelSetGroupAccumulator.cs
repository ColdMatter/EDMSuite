//using System;
//using System.Collections.Generic;
//using System.Text;

//using Data;

//namespace Analysis.EDM
//{
//    public class TOFChannelSetGroupAccumulator : ChannelSetGroup<TOFAccumulator>
//    {
//        public TOFChannelSetGroupAccumulator()
//        {
//            ChannelSets = new TOFChannelSetAccumulator[Length];
//            for (int i = 0; i < Length; i++) ChannelSets[i] = new TOFChannelSetAccumulator();
//        }

//        public void Add(TOFChannelSet val)
//        {
//            int index = machineStateIndex(
//                (bool)val.Config.Settings["eState"], 
//                (bool)val.Config.Settings["bState"],
//                (bool)val.Config.Settings["rfState"]);
//            ((TOFChannelSetAccumulator)ChannelSets[index]).Add(val);
//        }

//        public TOFChannelSetGroup GetResult()
//        {
//            TOFChannelSetGroup tcsg = new TOFChannelSetGroup();
//            TOFChannelSet[] tcsa = new TOFChannelSet[Length];
//            for (int i = 0; i < Length; i++)
//                // watch out for ChannelSets that have not had any items added
//                // would be better if there was a cleaner way to see if a ChannelSet was initialised
//                if (ChannelSets[i].Channels.Count == 0) tcsa[i] = null;
//                else
//                    tcsa[i] = (TOFChannelSet)(((TOFChannelSetAccumulator)ChannelSets[i]).GetResult());
//            tcsg.ChannelSets = tcsa;
//            return tcsg;
//        }
//    }
//}
