using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOTMaster
{
    public class ParameterEntry
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public Type Type { get; set; }

        public ParameterEntry(string name, string value, Type type)
        {
            Name = name;
            Value = value;
            Type = type;
        }
    }
}
