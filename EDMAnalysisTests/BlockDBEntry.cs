using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDMConfig;

namespace EDMAnalysisTests
{
    class BlockDBEntry
    {
        public HashSet<string> Tags = new HashSet<string>();

        public Dictionary<string, string> TOFChannelSets = new Dictionary<string, string>();

        public BlockConfig Config;
        public string Cluster;
        public int ClusterIndex;
        public bool EState;
        public bool BState;
        public bool RFState;

        public BlockDBEntry(BlockConfig b)
        {
            this.Config = b;

            Cluster = (string)Config.Settings["cluster"];
            ClusterIndex = (int)Config.Settings["clusterIndex"];
            EState = (bool)Config.Settings["eState"];
            BState = (bool)Config.Settings["bState"];
            RFState = (bool)Config.Settings["rfState"];
        }

    }
}
