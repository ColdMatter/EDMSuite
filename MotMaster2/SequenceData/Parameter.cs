using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.ComponentModel;
using dotMath;
using UtilsNS;

namespace MOTMaster2.SequenceData
{
    //A parameter class which adds more functionality to the parameter dictionary used in a MOTMasterScript
    public class Parameter : TypeConverter,INotifyPropertyChanged 
    {
        public string Name { get; set; }
        private object _value;
        public object Value 
        {
            get 
            { 
                if (IsScannable()) return _value;
                else return CompileParameter(Description);
            }
            set {_value = value;}
        }
        public string Description { get; set; }
        public bool IsHidden { get; set; }
        public bool IsScannable() 
        {
            if (Description == "" || Description == null) return true;
            return !Description[0].Equals('=');
        } 
        //Flags if the variable is used to modify a sequence
        public bool SequenceVariable { get; set; }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { PropertyChanged += value; }
            remove { PropertyChanged -= value; }
        }

        protected virtual event PropertyChangedEventHandler PropertyChanged;
        public Parameter()
        {
            //TODO Check this doesn't cause problems if the paramter needs to be a double
            Name = "";
            Value = 0.0;
            Description = "";
            IsHidden = false;
            SequenceVariable = true;
        }
        public Parameter(string name, string description, object value,bool isHidden = false, bool sequenceVar = true)
        {
            Name = name;
            Value = value;
            Description = description;
            IsHidden = isHidden;
            SequenceVariable = sequenceVar;
        }
        public Parameter Copy()
        {
            Parameter newParam = new Parameter(this.Name,this.Description,this.Value,this.IsHidden,this.SequenceVariable);
            return newParam;
        }

        //Equality is only defined if two parameters have the same name. This is to make it easier for overriding them when loading a new sequence
        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Parameter))
            {
                Parameter param = obj as Parameter;
                if (param.Name == this.Name && param.Value.ToString() == this.Value.ToString() && param.SequenceVariable == this.SequenceVariable)
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

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is int || value is double || value is string) return new Parameter("", "", value);
            else return null;
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) 
        {
            Parameter vl = (Parameter) value;
            if (destinationType == typeof(double)) return (double)vl.Value;
            if (destinationType == typeof(int)) return (int)vl.Value;
            else return null;
        }

        private double CompileParameter(string function)
        {
            if (Utils.isNull(Controller.sequenceData)) return Double.NaN;
            if (Utils.isNull(Controller.sequenceData.Parameters)) return Double.NaN;
            string func = function.TrimStart('=');
            EqCompiler compiler = new EqCompiler(func, true);
            compiler.Compile();

            //Checks all variables to use values in parameter dictionary
            foreach (string variable in compiler.GetVariableList())
            {
                if (Controller.sequenceData.Parameters.Keys.Contains(variable))
                {
                    compiler.SetVariable(variable, Convert.ToDouble(Controller.sequenceData.Parameters[variable].Value));
                }
                else throw new Exception(string.Format("Variable {0} not found in parameters.", variable));
                if(!Controller.sequenceData.Parameters[variable].IsScannable())
                    throw new Exception(string.Format("Variable {0} is derivative (non-scannable) - not allowed!", variable));
            }
            return compiler.Calculate();
        }
    }


}
