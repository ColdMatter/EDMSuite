using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDMConfig;

namespace Analysis.EDM.Database
{
    public class BlockDBEntry
    {
        public BlockConfig Config;
        public HashSet<String> Tags = new HashSet<string>();
        public Dictionary<String, String> TCSPaths = new Dictionary<string,string>();

    }
}
