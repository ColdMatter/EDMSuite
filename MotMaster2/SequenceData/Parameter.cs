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
            //TODO Check this doesn't cause problems if the paramter needs to be a double
            Name = "";
            Value = 0;
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

        //Equality is only defined if two parameters have the same name. This is to make it easier for overriding them when loading a new sequence
        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Parameter))
            {
                Parameter param = obj as Parameter;
                if (param.Name == this.Name)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return base.Equals(obj);
        }
    }

}
