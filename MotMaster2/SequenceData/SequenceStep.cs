using System;
using System.Collections.Generic;
using System.Linq;
using DAQ.Environment;
using NavigatorHardwareControl;
using System.ComponentModel;
using Newtonsoft.Json;

namespace MOTMaster2.SequenceData
{
    /// <summary>
    /// A class to encapsulate MOTMasterScriptSnippets. This is used so that a full script can be defined with relative timings and step names.
    /// </summary>
    [Serializable,JsonObject]
    public class SequenceStep : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Description {get; set;}
        public bool Enabled { get; set; }
        public object Duration { get; set; }
        public TimebaseUnits Timebase { get; set; }
        public bool RS232Commands { get; set; }
        public ObservableDictionary<string, AnalogChannelSelector> AnalogValueTypes {get; set;}
        public ObservableDictionary<string, DigitalChannelSelector> DigitalValueTypes { get; set; }
        [JsonProperty]
        private Dictionary<string, AnalogValueArgs> analogData;
        [JsonProperty]
        private Dictionary<string, bool> digitalData;
        [JsonProperty]
        private List<string> usedAnalogChannels;
        [JsonProperty]
        private List<SerialItem> serialCommands;
        
        public SequenceStep()
        {
            if (analogData == null) analogData = new Dictionary<string, AnalogValueArgs>();
            if (digitalData == null) digitalData = new Dictionary<string, bool>();
            if (usedAnalogChannels == null) usedAnalogChannels = new List<string>();
            if (serialCommands == null) serialCommands = new List<SerialItem>();

            AnalogValueTypes = new ObservableDictionary<string,AnalogChannelSelector>();
            DigitalValueTypes = new ObservableDictionary<string,DigitalChannelSelector>();
            List<string> analogNames = Environs.Hardware.AnalogOutputChannels.Keys.Cast<string>().ToList();
            //analogNames.OrderBy(t=>(DAQ.HAL.AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[t])
            foreach (string analog in Environs.Hardware.AnalogOutputChannels.Keys.Cast<string>().ToList())
            {
                AnalogValueTypes[analog] = new AnalogChannelSelector();
                analogData[analog] = new AnalogValueArgs();
            }
            foreach (string digital in Environs.Hardware.DigitalOutputChannels.Keys.Cast<string>().ToList())
            {
                DigitalValueTypes[digital] = new DigitalChannelSelector();
                digitalData[digital] = false;
            }
        }


        [JsonConstructor] // This forces JsonSerializer to call it instead of the default.
        [Obsolete("Call the default constructor. This is only for JSONserializer", true)]
        protected SequenceStep(bool Do_Not_Call)
        {
           
        }
        //If a property is changed, this will modify the SequenceData object that exists in the Controller
        public static void SequenceStep_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "EnabledRS232") return;
           
            
        }
        public SequenceStep Copy()
        {
            SequenceStep copy = new SequenceStep();
            copy.analogData = this.analogData;
            copy.digitalData = this.digitalData;
            copy.Enabled = this.Enabled;
            copy.Duration = this.Duration;
            copy.Timebase = this.Timebase;
            copy.Name = this.Name;
            copy.Description = this.Description;

            return copy;
        }
      
        public void SetSerialCommands(List<SerialItem> commands)
        {
            serialCommands = commands;
        }
        public AnalogChannelSelector GetAnalogChannelType(string name)
        {
            return AnalogValueTypes[name];
        }

        public List<string> GetUsedAnalogChannels()
        {
             return AnalogValueTypes.Where(t=>(t.Value != AnalogChannelSelector.Continue)).Select(t=>t.Key).ToList();
           
        }

        public List<string> GetUsedDigitalChannels(SequenceStep previousStep)
        {
            List<string> usedDigitalChannels = new List<string>();
            if (previousStep == null)
            {
                foreach (string name in DigitalValueTypes.Keys)
                {
                    if (DigitalValueTypes[name].Value) {usedDigitalChannels.Add(name); digitalData[name] = true;};
                }
            }
            else
            {
                foreach (string name in DigitalValueTypes.Keys)
                {
                    if (DigitalValueTypes[name].Value != previousStep.DigitalValueTypes[name].Value) {
                        usedDigitalChannels.Add(name);
                      //  digitalData[name] = !previousStep.GetDigitalData(name);
                    }
                }
            }
            return usedDigitalChannels;
        }
        public List<AnalogArgItem> GetAnalogData(string name,AnalogChannelSelector type)
        {
            AnalogValueArgs analogArgs = analogData[name];
            //Adds or removes the channel from a list if it is being modified in this step
            if (type != AnalogChannelSelector.Continue && !usedAnalogChannels.Contains(name)) usedAnalogChannels.Add(name);
            else if (usedAnalogChannels.Contains(name)) usedAnalogChannels.Remove(name);

            return analogArgs.GetArgType(type);
        }

        //Modifies the data of a selected analog channel
        public void SetAnalogDataItem(string name, AnalogChannelSelector type, List<AnalogArgItem> data)
        {
            if (type != AnalogChannelSelector.Continue)
            {
                AnalogValueArgs analogArgs = analogData[name];
                analogArgs.SetArgType(type,data);
                
              
            }
            AnalogValueTypes[name] = type;
            
        }
        public bool GetDigitalData(string name)
        {
            return DigitalValueTypes[name].Value;
        }

        # region AnalogValue Access Methods - Could be refactored
        public double GetAnalogStartTime(string name)
        {
            AnalogValueArgs analogArgs = analogData[name];
            return analogArgs.GetStartTime();
        }

        public double GetAnalogDuration(string name)
        {
            AnalogValueArgs analogArgs = analogData[name];
            return analogArgs.GetDuration();
        }

        public double GetAnalogValue(string name)
        {
            AnalogValueArgs analogArgs = analogData[name];
            return analogArgs.GetValue();
        }

        public double GetAnalogFinalValue(string name)
        {
            AnalogValueArgs analogArgs = analogData[name];
            return analogArgs.GetFinalValue();
        }
        public string GetFunction(string name)
        {
            AnalogValueArgs analogArgs = analogData[name];
            return analogArgs.GetFunction();
        }

        public List<double[]> GetXYPairs(string name)
        {
            AnalogValueArgs analogArgs = analogData[name];
            return analogArgs.GetXYPairs();
        }

        public string GetInterpolationType(string name)
        {
            AnalogValueArgs analogArgs = analogData[name];
            return analogArgs.GetInterpolationType();
        }
        #endregion
        public event PropertyChangedEventHandler PropertyChanged;

        internal List<SerialItem> GetSerialData()
        {
            if (serialCommands.Count == 0) serialCommands.Add(new SerialItem("", ""));
            return serialCommands;
        }
    }

    //Enumerates units of time relative to milliseconds
    public enum TimebaseUnits 
    {
        us,
        ms,
        s
    }

    //
    public enum AnalogChannelSelector
    {
        Continue,
        SingleValue,
        LinearRamp,
        Pulse,
        Function,
        XYPairs
    }
    //Enumerates the state of each digital channel. For now, this is either on/off, but we may want to add the option of including pulses within a single step   
    [Serializable,JsonObject]
     public class DigitalChannelSelector
    {

        public bool Value {get; set;}
        public DigitalChannelSelector()
        {
            Value = false;
        }
    }

    //This stores a list of arguments for each analog channel in the sequence step. Nominally these are strings which are parsed to either numbers or Parameter names
    [Serializable,JsonObject]
    public class AnalogValueArgs
    {
        [JsonIgnore]
        public List<AnalogArgItem> Value {get; set;}
        [JsonIgnore]
        public List<AnalogArgItem> LinearRamp { get; set; }
        [JsonIgnore]
        public List<AnalogArgItem> Pulse { get; set; }
        [JsonIgnore]
        public List<AnalogArgItem> Function { get; set; }
        [JsonIgnore]
        public List<AnalogArgItem> XYPairs { get; set; }
        [JsonProperty]
        private List<AnalogArgItem> _selectedItem;

        public AnalogValueArgs()
        {
            CreateArgs();
        }

        public void CreateArgs()
        {
            Value = new List<AnalogArgItem> { new AnalogArgItem("Start Time", ""), new AnalogArgItem("Value", "") };
            LinearRamp = new List<AnalogArgItem> { new AnalogArgItem("Start Time", ""), new AnalogArgItem("Duration", ""), new AnalogArgItem("Final Value", "") };
            Pulse = new List<AnalogArgItem> { new AnalogArgItem("Start Time", ""), new AnalogArgItem("Duration", ""), new AnalogArgItem("Value", ""), new AnalogArgItem("Final Value", "") };
            Function = new List<AnalogArgItem> { new AnalogArgItem("Start Time", ""), new AnalogArgItem("Duration", ""), new AnalogArgItem("Function", "") };
            XYPairs = new List<AnalogArgItem> { new AnalogArgItem("X Values", ""), new AnalogArgItem("Y Values", ""), new AnalogArgItem("Interpolation Type", "") };
        }

        //TODO make this assign _selecteditem to the correct property. the others should take on their default values
        [JsonConstructor]
        [Obsolete("call the default constructor. this is only for jsonserializer", true)]
        public AnalogValueArgs(bool do_not_call)
        {

        }

        public double GetStartTime()
        {
            if (_selectedItem[0].Name != "X Values") return SequenceParser.ParseOrGetParameter(_selectedItem[0].Value);
            else return SequenceParser.ParseOrGetParameter(_selectedItem[0].Value.Split(',')[0]);
            
        }

        public double GetDuration()
        {
            if (_selectedItem == null) return 0.0;
            if (_selectedItem == Value) throw new Exception("Channel arguments do not have a Duration");
            return SequenceParser.ParseOrGetParameter(_selectedItem[1].Value);
        }

        public double GetValue()
        {
            if (_selectedItem.Count == 2) return SequenceParser.ParseOrGetParameter(_selectedItem[1].Value);
            else return SequenceParser.ParseOrGetParameter(_selectedItem[2].Value);
        }

        public string GetFunction()
        {
            if (_selectedItem[2].Name != "Function") throw new Exception("Channel arguments do not have a function string");
            return _selectedItem[2].Value;
        }

        public double GetFinalValue()
        {
            if (_selectedItem != Pulse) throw new Exception("Channel arguments do not have a final value.");
            else return SequenceParser.ParseOrGetParameter(_selectedItem[3].Value);
        }
        public List<double[]> GetXYPairs()
        {
            if (_selectedItem[0].Name == "X Values")
            {
                string[] xstr = _selectedItem[0].Value.Split(',');
                string[] ystr = _selectedItem[1].Value.Split(',');
                if (xstr.Length != ystr.Length) throw new Exception("Length mismatch for XY pairs");
                double[] xvals = new double[xstr.Length];
                double[] yvals = new double[ystr.Length];
                for (int i = 0; i<xvals.Length; i++)
                {
                    xvals[i] = Double.Parse(xstr[i]);
                    yvals[i] = Double.Parse(ystr[i]);
                }
                return new List<double[]>(){xvals,yvals};
            }
            else throw new Exception("Channel arguments not an XY list");
        }
        public string GetInterpolationType()
        {
            if (_selectedItem[0].Name == "X Values")
                return _selectedItem[2].Value;
            else throw new Exception("Channel arguments not an XY list");
        }
        //Sets the argument type of the selected analog channel, as well as the data for that type. This should prevent issues with different types being assigned by value or reference 
        public void SetArgType(AnalogChannelSelector channelType, List<AnalogArgItem> data)
        {

            switch (channelType)
            {
                case AnalogChannelSelector.Continue:
                    break;
                case AnalogChannelSelector.SingleValue:
                    if (data == null) data = new List<AnalogArgItem> { new AnalogArgItem("Start Time", ""), new AnalogArgItem("Value", "") };
                    Value = data;
                    _selectedItem = Value;
                    break;
                case AnalogChannelSelector.LinearRamp:
                    if (data == null) data = new List<AnalogArgItem> { new AnalogArgItem("Start Time", ""), new AnalogArgItem("Duration", ""), new AnalogArgItem("Final Value", "") };
                    LinearRamp = data;
                    _selectedItem = LinearRamp;
                    break;
                case AnalogChannelSelector.Pulse:
                    if (data == null) data = new List<AnalogArgItem> { new AnalogArgItem("Start Time", ""), new AnalogArgItem("Duration", ""), new AnalogArgItem("Value", ""), new AnalogArgItem("Final Value", "") };
                    Pulse = data;
                    _selectedItem = Pulse;
                    break;
                case AnalogChannelSelector.Function:
                    if (data == null) data = new List<AnalogArgItem> { new AnalogArgItem("Start Time", ""), new AnalogArgItem("Duration", ""), new AnalogArgItem("Function", "") };
                    Function = data;
                    _selectedItem = Function;
                    break;
                case AnalogChannelSelector.XYPairs:
                    if (data == null) data = new List<AnalogArgItem> { new AnalogArgItem("X Values", ""), new AnalogArgItem("Y Values", ""), new AnalogArgItem("Interpolation Type", "") };
                    XYPairs = data;
                    _selectedItem = XYPairs;
                    break;
                default:
                    break;
            }
            return;
        }
        public List<AnalogArgItem> GetArgType(AnalogChannelSelector channelType)
        {
            List<AnalogArgItem> data;
            //When deserialised, only _selectedItem is not null. This ensures the correct argument type is returned
            if (Value == null && LinearRamp == null && Pulse == null && Function == null && XYPairs == null)
            {
                CreateArgs();
                SetArgType(channelType);

            }

            switch (channelType)
            {
                case AnalogChannelSelector.Continue:
                    break;
                case AnalogChannelSelector.SingleValue:
                    data = Value;
                    _selectedItem = Value;
                    break;
                case AnalogChannelSelector.LinearRamp:
                    data = LinearRamp;
                    _selectedItem = LinearRamp;
                    break;
                case AnalogChannelSelector.Pulse:
                    data = Pulse;
                    _selectedItem = Pulse;
                    break;
                case AnalogChannelSelector.Function:
                    data = Function;
                    _selectedItem = Function;
                    break;
                case AnalogChannelSelector.XYPairs:
                    data = XYPairs;
                    _selectedItem = XYPairs;
                    break;
                default:
                    break;
            }
            return _selectedItem;
        }
        //Use this when deserliasing so the correct argument type gets assigned the _selectedItem argument
        public void SetArgType(AnalogChannelSelector type)
        {
            SetArgType(type, _selectedItem);
        }
        public List<AnalogArgItem> GetArgItems()
        {
            return _selectedItem;
        }

    }

    //This is a simple class to represent each analog argument. Perhaps it is worth restructuring this so a single class can represent each type of analog command (pulse, value, ramp, arbitrary function)
    [Serializable,JsonObject]
    public class AnalogArgItem
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public AnalogArgItem(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public bool CompatibleArgs(object obj)
        {
            var item = obj as AnalogArgItem;
            if (this.Name == item.Name) return true;
            else return false;
        }
    }
    [Serializable]
    public class SerialItem : AnalogArgItem
    {
        public SerialItem(string name, string value) : base(name, value) { }
    }

    
}
