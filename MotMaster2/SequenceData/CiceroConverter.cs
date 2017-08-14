using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Globalization;
using DataStructures;
using DAQ.HAL;
using DAQ.Environment;
using dotMath;

namespace MOTMaster2.SequenceData
{
    /// <summary>
    /// Converts a Cicero SequenceData object into a MOTMaster Sequence object
    /// </summary>
    class CiceroConverter : TypeConverter
    {
        private static DataStructures.SequenceData ciceroSequence;
        private static DataStructures.SettingsData ciceroSettings;
        private static Sequence mmSequence;
        private Dictionary<int,string> channelIDs;
        
        public override bool CanConvertFrom(ITypeDescriptorContext context,
      Type sourceType)
        {
            if (sourceType == typeof(DataStructures.SequenceData))
            {
                
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public void SetSettingsData(DataStructures.SettingsData settings)
        {
            ciceroSettings = settings;
        }
        
        public void InitMMSequence(Sequence initSequence)
        {
            if (initSequence != null) mmSequence = initSequence;
            else mmSequence = new Sequence();
        }
        public bool CheckValidHardwareChannels()
        {
            //Checks all the Analog hardware channel names map to the same device ones
            foreach (LogicalChannel channel in ciceroSettings.logicalChannelManager.Analogs.Values)
            {
                string ciceroName = channel.Name;
                string ciceroDevice = channel.HardwareChannel.physicalChannelName();

                if (!Environs.Hardware.AnalogOutputChannels.ContainsKey(ciceroName))
                {
                    //Ignores unassigned channels
                    if (ciceroDevice == "Unassigned")
                    { continue; }
                    else
                    {
                        Console.WriteLine(string.Format("Cicero channel {0} not found in MOTMaster Hardware", ciceroName));
                    }

                    }

                AnalogOutputChannel motDevice = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[ciceroName];
                if (motDevice.PhysicalChannel != "/" + ciceroDevice) throw new Exception(string.Format("The cicero channel {0} does not map to the same hardware channel as MOTMaster", ciceroName));
                
            }
            foreach (LogicalChannel channel in ciceroSettings.logicalChannelManager.Digitals.Values)
            {
                string ciceroName = channel.Name;
                string ciceroDevice = channel.HardwareChannel.physicalChannelName();

                if (!Environs.Hardware.DigitalOutputChannels.ContainsKey(ciceroName))
                {
                    Console.WriteLine(string.Format("Cicero Digital Channel {0} not found in MOTMaster Hardware", ciceroName));
                }
            }
            return true;
        }
        // Overrides the ConvertFrom method of TypeConverter.
        public override object ConvertFrom(ITypeDescriptorContext context,
           CultureInfo culture, object value)
        {
            mmSequence.Steps = new List<SequenceStep>();
           
            if (value is DataStructures.SequenceData)
            {
                ciceroSequence = value as DataStructures.SequenceData;
                List<string> paramNames = mmSequence.Parameters.Select(t => t.Name).ToList();
                foreach (Variable var in ciceroSequence.Variables)
                {
                    
                    Parameter param = new Parameter(var.VariableName, var.Description, var.VariableValue);
                    if (mmSequence.Parameters.Contains(param))
                    {
                        //Two Parameters are equal if they have the same name, so this reassigns the value of the parameter in question
                        mmSequence.Parameters.Remove(param);
                        
                    }
                    mmSequence.Parameters.Add(param);
                }
                foreach (TimeStep step in ciceroSequence.TimeSteps)
                {
                    SequenceStep mmStep = new SequenceStep();

                    InitialiseSequenceStep(step, mmStep);

                    ConvertAnalogChannelData(step, mmStep);
                    ConvertDigitalChannelData(step, mmStep);
                    ConvertSerialChannelData(step, mmStep);
                    mmSequence.Steps.Add(mmStep);
                }
                return mmSequence;
            }
            else
            {
                return base.ConvertFrom(context, culture, value);
            }
        }

        private static void InitialiseSequenceStep(TimeStep step, SequenceStep mmStep)
        {
            mmStep.Name = step.StepName;
            mmStep.Description = step.Description;
            mmStep.Duration = step.StepDuration.parameter.ManualValue;
            mmStep.Enabled = step.StepEnabled;
            string timeUnits = step.StepDuration.ParameterString.Split(' ')[1];

            //Checks the units of the timestep and converts it to the equivalent object in the MotMaster Sequence step

            if (timeUnits == "ms") mmStep.Timebase = TimebaseUnits.ms;
            else if (timeUnits == "us") mmStep.Timebase = TimebaseUnits.us;
            else if (timeUnits == "s")
                mmStep.Timebase = TimebaseUnits.s;
            else throw new Exception("Incorrect Cicero TimeStep units");

            
            mmStep.RS232Commands = (step.rs232Group != null);
        }

        /// <summary>
        /// Goes through the analog data for this step and assigns the data to the equivalent channel in the MOTMaster Sequence. Checks the channel ids are the same and refer to the same hardware channel
        /// </summary>
        /// <param name="ciceroStep"></param>
        /// <param name="mmStep"></param>
        private static void ConvertAnalogChannelData(TimeStep ciceroStep, SequenceStep mmStep)
        {
            if (ciceroStep.AnalogGroup == null)
            {
                return;
            }
            Dictionary<int, AnalogGroupChannelData> ciceroChannelData = ciceroStep.AnalogGroup.ChannelDatas;
            
            foreach (int id in ciceroChannelData.Keys)
            {
              
               LogicalChannel analog = ciceroSettings.logicalChannelManager.Analogs[id];
               if (analog.HardwareChannel.physicalChannelName() == "Unassigned") continue;
               AnalogGroupChannelData chan = ciceroChannelData[id];
               if (!chan.ChannelEnabled) continue;

               if (chan.waveform.YValues.Count == 1)
               {
                   if (chan.waveform.YValues[0].myParameter.variable != null)
                   {
                       List<AnalogArgItem> function = new List<AnalogArgItem> { new AnalogArgItem("Start Time", ""), new AnalogArgItem("Duration", ""), new AnalogArgItem("Function", chan.waveform.YValues[0].myParameter.variable.VariableName) };
                       mmStep.SetAnalogDataItem(analog.Name, AnalogChannelSelector.Function, function);
                   }
                   else
                   {
                       List<AnalogArgItem> value = new List<AnalogArgItem> { new AnalogArgItem("Start Time", ""), new AnalogArgItem("Value", chan.waveform.YValues[0].ParameterValue.ToString()) };
                       mmStep.SetAnalogDataItem(analog.Name, AnalogChannelSelector.SingleValue, value);
                   }
               }
               else if (chan.waveform.EquationString != "")
               {
                   List<AnalogArgItem> function = new List<AnalogArgItem> { new AnalogArgItem("Start Time", ""), new AnalogArgItem("Duration", ""), new AnalogArgItem("Function", chan.waveform.EquationString) };
                   mmStep.SetAnalogDataItem(analog.Name, AnalogChannelSelector.Function, function);
               }
               else if (chan.waveform.YValues.Count > 1)
                {
                    int timeMultiplier = Convert.ToInt32(Math.Pow(10,6-3*Convert.ToInt32(mmStep.Timebase)));
                  string xstr = string.Join(",", chan.waveform.XValues.Select(t => (t.ParameterValue*timeMultiplier).ToString()));
                  string ystr = string.Join(",", chan.waveform.YValues.Select(t => t.ParameterValue.ToString()));
                 List<AnalogArgItem> xypairs = new List<AnalogArgItem> { new AnalogArgItem("X Values", xstr), new AnalogArgItem("Y Values", ystr), new AnalogArgItem("Interpolation Type",chan.waveform.interpolationType.ToString())};
                 mmStep.SetAnalogDataItem(analog.Name, AnalogChannelSelector.XYPairs, xypairs);
                
               }
               else
               {
                    continue;
               }
            
            }
        }

        private static void ConvertDigitalChannelData(TimeStep ciceroStep, SequenceStep mmStep)
        {
            foreach (KeyValuePair<int,DigitalDataPoint> ddata in ciceroStep.DigitalData)
            {
                try
                {
                    if (!ciceroSettings.logicalChannelManager.Digitals.ContainsKey(ddata.Key)) continue;
                    LogicalChannel digital = ciceroSettings.logicalChannelManager.Digitals[ddata.Key];
                    DigitalChannelSelector digitalSelector = new DigitalChannelSelector();
                    if (ddata.Value.ManualValue) digitalSelector.Value = true;
                    mmStep.DigitalValueTypes[digital.Name] = digitalSelector;
                }
                catch
                {
                    Console.WriteLine(String.Format("Digital ID key {0} not found in cicero channels. Perhaps it has been removed accidentally", ddata.Key));
                }
            }
        }

        private static void ConvertSerialChannelData(TimeStep ciceroStep, SequenceStep mmStep)
        {
            if (ciceroStep.rs232Group != null)
            {
                List<SerialItem> serialList = new List<SerialItem>();
                foreach (KeyValuePair<int, RS232GroupChannelData> rs232 in ciceroStep.rs232Group.ChannelDatas)
                {
                    if (!rs232.Value.Enabled) continue;
                    LogicalChannel serial = ciceroSettings.logicalChannelManager.RS232s[rs232.Key];
                    string valuestring;
                    if (rs232.Value.RawString != "" || rs232.Value.StringParameterStrings == null) valuestring = rs232.Value.RawString;
                    else valuestring = string.Join("",rs232.Value.StringParameterStrings);
                    SerialItem serialData = new SerialItem(serial.Name, valuestring);
                    serialList.Add(serialData);
                }
                mmStep.SetSerialCommands(serialList);
            }
        }
        // Overrides the ConvertTo method of TypeConverter.
        public override object ConvertTo(ITypeDescriptorContext context,
           CultureInfo culture, object value, Type destinationType)
        {
            return base.ConvertTo(context, culture, value,destinationType);
        }
    }
}
