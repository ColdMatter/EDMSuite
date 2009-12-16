using System;
using System.Collections.Generic;
using System.Text;

using Data;

namespace Analysis.EDM
{
    [Serializable]
    public class TOFChannelSetGroup : ChannelSetGroup<TOFWithError>
    {
        public TOFChannelSet AverageChannelSetSignedByMachineState(bool eSign, bool bSign, bool rfSign)
        {
            // deal with the (false, false, false) special case
            if (!(eSign || bSign || rfSign))
            {
                List<TOFChannelSet> chanSets = new List<TOFChannelSet>();
                foreach (TOFChannelSet tcs in ChannelSets) if (tcs != null) chanSets.Add(tcs);
                return averageChannelSets(chanSets);
            }

            // divide the chan sets into two groups, on and off
            List<TOFChannelSet> onChanSets = new List<TOFChannelSet>();
            List<TOFChannelSet> offChanSets = new List<TOFChannelSet>();
            for (int i = 0; i < ChannelSets.Length; i++)
            {
                if ((eSign && eState(i)) ^
                    (bSign && bState(i)) ^
                    (rfSign && rfState(i)))
                {
                    if (ChannelSets[i] != null) onChanSets.Add((TOFChannelSet)ChannelSets[i]);
                }
                else
                {
                    if (ChannelSets[i] != null) offChanSets.Add((TOFChannelSet)ChannelSets[i]);
                }
            }

            // check whether both groups have members
            if ((onChanSets.Count > 0) && (offChanSets.Count > 0))
            {
                // find the average for each group
                TOFChannelSet onAverage = averageChannelSets(onChanSets);
                TOFChannelSet offAverage = averageChannelSets(offChanSets);
                // return the difference
                return onAverage - offAverage;
            }
            // this means that there were not channels with both states of the requested signing.
            else return null;
        }

        private TOFChannelSet averageChannelSets(List<TOFChannelSet> chanSets)
        {
            if (chanSets.Count > 0)
            {
                TOFChannelSet tcs = chanSets[0];
                for (int i = 1; i < chanSets.Count; i++) tcs += chanSets[i];
                return tcs / chanSets.Count;
            }
            else return null;
        }
    }
}
