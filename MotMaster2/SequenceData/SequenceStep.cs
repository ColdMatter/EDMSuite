using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Environment;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster2;
using NavigatorHardwareControl;
using System.Collections.ObjectModel;
using dotMath;
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
        public double Duration { get; set; }
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
            //serialCommands.Add(new SerialItem("Slave", ""));
            //serialCommands.Add(new SerialItem("AOM", ""));
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
            return usedAnalogChannels;
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
                    if (DigitalValueTypes[name] != previousStep.DigitalValueTypes[name]) {
                        usedDigitalChannels.Add(name);
                        digitalData[name] = !previousStep.GetDigitalData(name);
                    }
                }
            }
            return usedDigitalChannels;
        }
        public List<AnalogArgItem> GetAnalogData(string name,AnalogChannelSelector type)
        {
            AnalogValueArgs analogArgs = analogData[name];
            //Adds or removes the channel from a list if it is being modified in this step
            if (type != AnalogChannelSelector.Continue) usedAnalogChannels.Add(name);
            else if (usedAnalogChannels.Contains(name)) usedAnalogChannels.Remove(name);

            //Sets the argument type for use elsewhere
            analogArgs.SetArgType(type);
            return analogArgs.GetArgItems();
        }
        public void SetAnalogDataItem(string name, AnalogChannelSelector type, object data)
        {
            if (type != AnalogChannelSelector.Continue)
            {
                AnalogValueArgs analogArgs = analogData[name];
                analogArgs.SetArgumentData(data);
            }
        }
        public bool GetDigitalData(string name)
        {
            return digitalData[name];
        }
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
    
        public event PropertyChangedEventHandler PropertyChanged;

        internal List<SerialItem> GetSerialData()
        {
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
        Function
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
        public List<AnalogArgItem> Value {get; set;}
        public List<AnalogArgItem> LinearRamp { get; set; }
        public List<AnalogArgItem> Pulse { get; set; }
        public List<AnalogArgItem> Function { get; set; }
        [JsonProperty]
        private List<AnalogArgItem> _selectedItem;

        public AnalogValueArgs()
        {
            Value = new List<AnalogArgItem>{new AnalogArgItem("Start Time",""),new AnalogArgItem("Value","")};
            LinearRamp = new List<AnalogArgItem> { new AnalogArgItem("Start Time", ""), new AnalogArgItem("Duration", ""), new AnalogArgItem("Final Value", "") };
            Pulse = new List<AnalogArgItem> { new AnalogArgItem("Start Time", ""), new AnalogArgItem("Duration", ""), new AnalogArgItem("Value", ""), new AnalogArgItem("Final Value","") };
            Function = new List<AnalogArgItem> {new AnalogArgItem("Start Time",""), new AnalogArgItem("Duration", ""), new AnalogArgItem("Function", "") };
        }

        [JsonConstructor]
        [Obsolete("Call the default constructor. This is only for JSONserializer", true)]
        public AnalogValueArgs(bool Do_Not_Call)
        {
          
        }

        public double ParseOrGetParameter(string value)
        {
            double number = 0.0;
            bool result = Double.TryParse(value,out number);
            if (result) return number;
            else return (double)Controller.sequenceData.Parameters.Where(t=>t.Name==value).Select(t=>t.Value).First();
        }
        public double GetStartTime()
        {
            return ParseOrGetParameter(_selectedItem[0].Value);
        }

        public double GetDuration()
        {
            if (_selectedItem == null) return 0.0;
            if (_selectedItem == Value) throw new Exception("Channel arguments do not have a Duration");
            return ParseOrGetParameter(_selectedItem[1].Value);
        }

        public double GetValue()
        {
            if (_selectedItem.Count == 2) return ParseOrGetParameter(_selectedItem[1].Value);
            else return ParseOrGetParameter(_selectedItem[2].Value);
        }

        public string GetFunction()
        {
            if (_selectedItem != Function) throw new Exception("Channel arguments do not have a function string");
            return _selectedItem[2].Value;
        }

        public double GetFinalValue()
        {
            if (_selectedItem != Pulse) throw new Exception("Channel arguments do not have a final value.");
            else return ParseOrGetParameter(_selectedItem[3].Value);
        }

        public void SetArgType(AnalogChannelSelector channelType)
        {
            switch (channelType)
            {
                case AnalogChannelSelector.Continue:
                    break;
                case AnalogChannelSelector.SingleValue:
                    _selectedItem = Value;
                    break;
                case AnalogChannelSelector.LinearRamp:
                    _selectedItem = LinearRamp;
                    break;
                case AnalogChannelSelector.Pulse:
                    _selectedItem = Pulse;
                    break;
                case AnalogChannelSelector.Function:
                    _selectedItem = Function;
                    break;
                default:
                    break;
            }
            return;
        }

        public void SetArgumentData(object data)
        {
            if (data.GetType() != typeof(List<AnalogArgItem>)) throw new Exception("Incorrect Analog Argument Data Type");
            _selectedItem = (List<AnalogArgItem>)data;
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
    }
    [Serializable]
    public class SerialItem : AnalogArgItem
    {
        public SerialItem(string name, string value) : base(name, value) { }
    }

    
}
