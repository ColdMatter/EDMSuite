using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MOTMaster2.SequenceData
{
    [Serializable,JsonObject]
    public class Sequence
    {
        public List<SequenceStep> Steps { get; set; }
        public List<Parameter> Parameters { get; set; }

        public Dictionary<string,object> CreateParameterDictionary()
        {
            Dictionary<string,object> paramDict = new Dictionary<string,object>();
            foreach (Parameter p in Parameters)
            {
                //Converts a 64-bit int to 32-bit
                if (p.Value.GetType() == typeof(Int64)) p.Value = Convert.ToInt32(p.Value);
                paramDict[p.Name] = p.Value;
            }
            return paramDict;
        }

        public void CreateParameterList(Dictionary<string,object> paramDict)
        {
            foreach (KeyValuePair<string, object> entry in paramDict)
                Parameters.Add(new Parameter(entry.Key, "", entry.Value));
        }

        public Sequence()
        {
            Steps = new List<SequenceStep>();
            Parameters = new List<Parameter>();
        }
    }
}
