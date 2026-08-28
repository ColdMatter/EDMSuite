using System.Collections.Generic;

namespace MOTMaster
{
    public class ParameterGroup
    {
        public string Name { get; set; }
        public List<ParameterEntry> Parameters { get; set; }

        public ParameterGroup(string name)
        {
            Name = name;
            Parameters = new List<ParameterEntry>();
        }
    }
}
