using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Analysis.EDM;
using EDMConfig;

namespace SonOfSirCachealot
{
    public class BlockStoreResponse
    {
        public List<BlockStoreBlockResponse> BlockResponses;
    }
    public class BlockStoreBlockResponse
    {
        public int BlockID;
        public BlockConfig Settings;
        public List<BlockStoreDetectorResponse> DetectorResponses;
    }

    public class BlockStoreDetectorResponse
    {
        public string Detector;
        public Dictionary<string, TOFChannel> Channels;
    }
}
