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

namespace MOTMaster2.SequenceData
{
    /// <summary>
    /// A class to encapsulate MOTMasterScriptSnippets. This is used so that a full script can be defined with relative timings and step names.
    /// </summary>
    [Serializable]
    public class SequenceStep 
    {
        public string Name { get; set; }
        public string Description {get; set;}
        public bool Enabled { get; set; }
        public double Duration { get; set; }
        public TimebaseUnits Timebase { get; set; }
        public ObservableDictionary<string, AnalogChannelSelector> AnalogValueTypes {get; set;}
        public ObservableDictionary<string, DigitalChannelSelector> DigitalValueTypes { get; set; } 
        private Dictionary<string, AnalogValueArgs> analogData = new Dictionary<string,AnalogValueArgs>();
        private Dictionary<string, bool> digitalData = new Dictionary<string,bool>();
        private List<string> usedAnalogChannels = new List<string>();

        
        public SequenceStep()
        {
            AnalogValueTypes = new ObservableDictionary<string,AnalogChannelSelector>();
            DigitalValueTypes = new ObservableDictionary<string,DigitalChannelSelector>();
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
                    if (DigitalValueTypes[name] == DigitalChannelSelector.On) {usedDigitalChannels.Add(name); digitalData[name] = true;};
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
     public enum DigitalChannelSelector
    {
        On,
        Off
    }

    //This stores a list of arguments for each analog channel in the sequence step. Nominally these are strings which are parsed to either numbers or Parameter names
    public class AnalogValueArgs
    {
        public List<AnalogArgItem> Value {get; set;}
        public List<AnalogArgItem> LinearRamp { get; set; }
        public List<AnalogArgItem> Pulse { get; set; }
        public List<AnalogArgItem> Function { get; set; }
        private List<AnalogArgItem> _selectedItem;

        public AnalogValueArgs()
        {
            Value = new List<AnalogArgItem>{new AnalogArgItem("Start Time","1"),new AnalogArgItem("Value","2")};
            LinearRamp = new List<AnalogArgItem> { new AnalogArgItem("Start Time", "3"), new AnalogArgItem("Duration", "4"), new AnalogArgItem("Final Value", "5") };
            Pulse = new List<AnalogArgItem> { new AnalogArgItem("Start Time", "6"), new AnalogArgItem("Duration", "7"), new AnalogArgItem("Value", "8"), new AnalogArgItem("Final Value","9") };
            Function = new List<AnalogArgItem> {new AnalogArgItem("Start Time","1"), new AnalogArgItem("Function", "8") };
        }

        public double GetStartTime()
        {
            return Double.Parse(_selectedItem[0].Value);
        }

        public double GetDuration()
        {
            if (_selectedItem == Value || _selectedItem == Function) throw new Exception("Channel arguments do not have a Duration");
            return Double.Parse(_selectedItem[1].Value);
        }

        public double GetValue()
        {
            if (_selectedItem.Count == 2) return Double.Parse(_selectedItem[1].Value);
            else return Double.Parse(_selectedItem[2].Value);
        }

        public string GetFunction()
        {
            if (_selectedItem != Function) throw new Exception("Channel arguments do not have a function string");
            return _selectedItem[1].Value;
        }

        public double GetFinalValue()
        {
            if (_selectedItem != Pulse) throw new Exception("Channel arguments do not have a final value.");
            else return Double.Parse(_selectedItem[3].Value);
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

        public List<AnalogArgItem> GetArgItems()
        {
            return _selectedItem;
        }

    }

    //This is a simple class to represent each analog argument. Perhaps it is worth restructuring this so a single class can represent each type of analog command (pulse, value, ramp, arbitrary function)
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

    
}
