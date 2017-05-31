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

namespace MOTMaster2.Sequence
{
    /// <summary>
    /// A class to encapsulate MOTMasterScriptSnippets. This is used so that a full script can be defined with relative timings and step names.
    /// </summary>
    [Serializable]
    public class SequenceStep 
    {
        public string name { get; set; }
        public string description {get; set;}
        public bool enabled { get; set; }
        public double duration { get; set; }
        public TimebaseUnits timebase { get; set; }
        public ObservableDictionary<string, AnalogChannelSelector> analogValueTypes { get; set; }
        public ObservableDictionary<string, DigitalChannelSelector> digitalValueTypes { get; set; }
        public Dictionary<string, AnalogValueArgs> analogData = new Dictionary<string,AnalogValueArgs>();
        public Dictionary<string, bool> digitalData = new Dictionary<string,bool>();

        
        public SequenceStep()
        {
             digitalValueTypes=new ObservableDictionary<string,DigitalChannelSelector>();
             analogValueTypes= new ObservableDictionary<string,AnalogChannelSelector>();
            foreach (string analog in Environs.Hardware.AnalogOutputChannels.Keys.Cast<string>().ToList())
            {
                analogValueTypes[analog] = new AnalogChannelSelector();
                analogData[analog] = new AnalogValueArgs();
            }
            foreach (string digital in Environs.Hardware.DigitalOutputChannels.Keys.Cast<string>().ToList())
            {
                digitalValueTypes[digital] = new DigitalChannelSelector();
                digitalData[digital] = false;
            }
        }

        public SequenceStep Copy()
        {
            SequenceStep copy = new SequenceStep();
            copy.analogData = this.analogData;
            copy.digitalData = this.digitalData;
            copy.enabled = this.enabled;
            copy.duration = this.duration;
            copy.timebase = this.timebase;
            copy.name = this.name;
            copy.description = this.description;

            return copy;
        }
        /// <summary>
        /// Converts a time from milliseconds into number of samples
        /// </summary>
        /// <param name="time"></param>
        /// <param name="frequency"></param>
        /// <returns></returns>
        public int ConvertToSampleTime(double time, int frequency)
        {
            return (int)(time * frequency / 1000);
        }
        public double ConvertToRealTime(int sampleTime, int frequency)
        {
            return sampleTime * 1000.0/frequency;
        }


        public AnalogChannelSelector GetAnalogChannels(string name)
        {
            
            return this.analogValueTypes[name];
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
        Off,
        On
    }

    public struct AnalogValueArgs
    {
        public double value;
        public double[] linearRamp;
        public double[] pulse;
        public double[] function;
    }
}
