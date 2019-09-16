using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SonOfSirCachealot
{
    public class BlockStoreQuery
    {
        public int[] BlockIDs;
        public BlockStoreBlockQuery BlockQuery;
    }
    public class BlockStoreBlockQuery
    {
        public BlockStoreDetectorQuery[] DetectorQueries;
    }
    public class BlockStoreDetectorQuery
    {
        public string[] Channels;
        public string Detector;
    }
}
