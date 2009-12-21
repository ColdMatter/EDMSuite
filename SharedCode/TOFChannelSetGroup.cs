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
                return weightedAverageChannelSets(chanSets);
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
                TOFChannelSet onAverage = weightedAverageChannelSets(onChanSets);
                TOFChannelSet offAverage = weightedAverageChannelSets(offChanSets);
                // return the weighted difference - this slightly quirky construction
                // let's me reuse the existing code.
                return weightedAverageChannelSets(
                    new List<TOFChannelSet>() {onAverage, (offAverage * -1.0)});
            }
            // this means that there were not channels with both states of the requested signing.
            else return null;
        }

        // finds the average ChannelSet of a number of ChannelSets, weighting the average
        // by the ChannelSet's Counts. This gives equal weight to the data from each block
        // which is not a crazy thing to do.
        //
        // I wonder if this wouldn't better belong in the TOFChannelSet class?
        private TOFChannelSet weightedAverageChannelSets(List<TOFChannelSet> chanSets)
        {
            if (chanSets.Count > 0)
            {
                // get the counts
                int[] counts = new int[chanSets.Count];
                for (int i = 0; i < chanSets.Count; i++) counts[i] = chanSets[i].Count;
                int total = 0;
                for (int i = 0; i < counts.Length; i++) total += counts[i];
      
                TOFChannelSet tcs = chanSets[0] * (double)counts[0];
                for (int i = 1; i < chanSets.Count; i++) tcs += chanSets[i] * (double)counts[i];
                tcs.Count = total;
                return tcs / total;
            }
            else return null;
        }
    }
}
