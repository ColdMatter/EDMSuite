using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOTMaster2.SequenceData
{
    //A parameter class which adds more functionality to the parameter dictionary used in a MOTMasterScript
    public class Parameter 
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public string Description { get; set; }
        public bool IsHidden { get; set; }

        public Parameter()
        {
            Name = "";
            Value = "";
            Description = "";
            IsHidden = false;
        }
        public Parameter(string name, string description, object value)
        {
            Name = name;
            Value = value;
            Description = description;
            IsHidden = false;
        }

        public Parameter Copy()
        {
            Parameter newParam = new Parameter(this.Name,this.Description,this.Value);
            newParam.IsHidden = this.IsHidden;
            return newParam;

        }
    }

}
