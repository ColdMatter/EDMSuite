using System;
using System.Collections.Generic;

namespace MOTMaster
{
    [Serializable]
    public class DDSSequence
    {
        public Dictionary<string, List<List<double>>> Commands { get; set; }

        public DDSSequence()
        {
            Commands = new Dictionary<string, List<List<double>>>();
        }
    }
}