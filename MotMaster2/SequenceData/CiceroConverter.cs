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

namespace MOTMaster2.SequenceData
{
    /// <summary>
    /// Converts a Cicero SequenceData object into a MOTMaster Sequence object
    /// </summary>
    class CiceroConverter : TypeConverter
    {
        private static DataStructures.SequenceData ciceroSequence;
        private static DataStructures.SettingsData ciceroSettings;
<<<<<<< HEAD
        private static Sequence mmSequence;
=======
>>>>>>> 15668b9c2cbb0d64b4dd583bb3c87929c4f9db7f
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
<<<<<<< HEAD
        
        public void InitMMSequence(Sequence initSequence)
        {
            mmSequence = initSequence;
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
=======
        public bool CheckValidHardwareChannels()
        {
            //Checks all the channel names and device routes are named the same
            foreach (HardwareChannel channel in ciceroSettings.logicalChannelManager.AssignedHardwareChannels)
            {
                string ciceroName = channel.ChannelName;
                string ciceroDevice = channel.physicalChannelName();

                if (!Environs.Hardware.AnalogOutputChannels.ContainsKey(ciceroName)) {Console.WriteLine(string.Format("Cicero channel {0} not found in MOTMaster Hardware", ciceroName));
                    }

                AnalogOutputChannel motDevice = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[ciceroName];
                if (motDevice.Name != ciceroDevice) throw new Exception(string.Format("The cicero channel {0} does not map to the same hardware channel as MOTMaster", ciceroName));
                
            }
>>>>>>> 15668b9c2cbb0d64b4dd583bb3c87929c4f9db7f
            return true;
        }
        // Overrides the ConvertFrom method of TypeConverter.
        public override object ConvertFrom(ITypeDescriptorContext context,
           CultureInfo culture, object value)
        {
<<<<<<< HEAD
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
=======
            if (value is DataStructures.SequenceData)
            {
                ciceroSequence = value as DataStructures.SequenceData;
>>>>>>> 15668b9c2cbb0d64b4dd583bb3c87929c4f9db7f
                foreach (TimeStep step in ciceroSequence.TimeSteps)
                {
                    SequenceStep mmStep = new SequenceStep();

                    InitialiseSequenceStep(step, mmStep);

                    ConvertAnalogChannelData(step, mmStep);
                    ConvertDigitalChannelData(step, mmStep);
                    ConvertSerialChannelData(step, mmStep);
<<<<<<< HEAD
                    mmSequence.Steps.Add(mmStep);
                }
                return mmSequence;
            }
            else
            {
                return base.ConvertFrom(context, culture, value);
            }
=======

                }
            }
            return base.ConvertFrom(context, culture, value);
>>>>>>> 15668b9c2cbb0d64b4dd583bb3c87929c4f9db7f
        }

        private static void InitialiseSequenceStep(TimeStep step, SequenceStep mmStep)
        {
            mmStep.Name = step.StepName;
            mmStep.Description = step.Description;
<<<<<<< HEAD
            mmStep.Duration = step.StepDuration.parameter.ManualValue;
            mmStep.Enabled = step.StepEnabled;
            string timeUnits = step.StepDuration.ParameterString.Split(' ')[1];

            //Checks the units of the timestep and converts it to the equivalent object in the MotMaster Sequence step

            if (timeUnits == "ms") mmStep.Timebase = TimebaseUnits.ms;
            else if (timeUnits == "us") mmStep.Timebase = TimebaseUnits.us;
            else if (timeUnits == "s")
=======
            mmStep.Duration = step.StepDuration.ParameterValue;
            mmStep.Enabled = step.StepEnabled;
            Units timeUnits = step.StepDuration.ParameterUnits;

            //Checks the units of the timestep and converts it to the equivalent object in the MotMaster Sequence step

            if (timeUnits.toLongString() == "ms") mmStep.Timebase = TimebaseUnits.ms;
            else if (timeUnits.toLongString() == "us") mmStep.Timebase = TimebaseUnits.us;
            else if (timeUnits.toLongString() == "s")
>>>>>>> 15668b9c2cbb0d64b4dd583bb3c87929c4f9db7f
                mmStep.Timebase = TimebaseUnits.s;
            else throw new Exception("Incorrect Cicero TimeStep units");

            
<<<<<<< HEAD
            mmStep.RS232Commands = (step.rs232Group != null);
=======
            mmStep.RS232Commands = (step.rs232Group.ChannelDatas != null);
>>>>>>> 15668b9c2cbb0d64b4dd583bb3c87929c4f9db7f
        }

        /// <summary>
        /// Goes through the analog data for this step and assigns the data to the equivalent channel in the MOTMaster Sequence. Checks the channel ids are the same and refer to the same hardware channel
        /// </summary>
        /// <param name="ciceroStep"></param>
        /// <param name="mmStep"></param>
        private static void ConvertAnalogChannelData(TimeStep ciceroStep, SequenceStep mmStep)
        {
<<<<<<< HEAD
            if (ciceroStep.AnalogGroup == null)
            {
                return;
            }
=======
>>>>>>> 15668b9c2cbb0d64b4dd583bb3c87929c4f9db7f
            Dictionary<int, AnalogGroupChannelData> ciceroChannelData = ciceroStep.AnalogGroup.ChannelDatas;
            
            foreach (int id in ciceroChannelData.Keys)
            {
<<<<<<< HEAD
              
               LogicalChannel analog = ciceroSettings.logicalChannelManager.Analogs[id];
               if (analog.HardwareChannel.physicalChannelName() == "Unassigned") continue;
               AnalogGroupChannelData chan = ciceroChannelData[id];
               if (chan.waveform.YValues.Count == 1)
               {
                   List<AnalogArgItem> value = new List<AnalogArgItem>{new AnalogArgItem("Start Time",""),new AnalogArgItem("Value",chan.waveform.YValues[0].ParameterValue.ToString())};
                   mmStep.SetAnalogDataItem(analog.Name, AnalogChannelSelector.SingleValue,value );
               }
               else if (chan.waveform.EquationString != "")
               {
                   List<AnalogArgItem> function = new List<AnalogArgItem> { new AnalogArgItem("Start Time", ""), new AnalogArgItem("Duration", ""), new AnalogArgItem("Function", chan.waveform.EquationString) };
                   mmStep.SetAnalogDataItem(analog.Name, AnalogChannelSelector.Function, function);
               }
=======
               LogicalChannel analog = ciceroSettings.logicalChannelManager.Analogs[id];
               AnalogGroupChannelData chan = ciceroChannelData[id];
               if (chan.waveform.YValues.Count == 1) mmStep.SetAnalogDataItem(analog.Name, AnalogChannelSelector.SingleValue, chan.waveform.YValues[0].ToString());
               else if (chan.waveform.EquationString != "") mmStep.SetAnalogDataItem(analog.Name, AnalogChannelSelector.Function, chan.waveform.EquationString);
>>>>>>> 15668b9c2cbb0d64b4dd583bb3c87929c4f9db7f
               else if (chan.waveform.interpolationType == Waveform.InterpolationType.Linear)
                   continue;
            
            }
        }

        private static void ConvertDigitalChannelData(TimeStep ciceroStep, SequenceStep mmStep)
        {
<<<<<<< HEAD
            foreach (KeyValuePair<int,DigitalDataPoint> ddata in ciceroStep.DigitalData)
            {
                LogicalChannel digital = ciceroSettings.logicalChannelManager.Digitals[ddata.Key];
                DigitalChannelSelector digitalSelector = new DigitalChannelSelector();
                if (ddata.Value.ManualValue) digitalSelector.Value = true;
                mmStep.DigitalValueTypes[digital.Name] = digitalSelector;
            }
=======

>>>>>>> 15668b9c2cbb0d64b4dd583bb3c87929c4f9db7f
        }

        private static void ConvertSerialChannelData(TimeStep ciceroStep, SequenceStep mmStep)
        {
<<<<<<< HEAD
            if (ciceroStep.rs232Group != null)
            {
                List<SerialItem> serialList = new List<SerialItem>();
                foreach (KeyValuePair<int, RS232GroupChannelData> rs232 in ciceroStep.rs232Group.ChannelDatas)
                {
                    LogicalChannel serial = ciceroSettings.logicalChannelManager.RS232s[rs232.Key];
                    string valuestring;
                    if (rs232.Value.RawString != "" || rs232.Value.StringParameterStrings == null) valuestring = rs232.Value.RawString;
                    else valuestring = string.Join("",rs232.Value.StringParameterStrings);
                    SerialItem serialData = new SerialItem(serial.Name, valuestring);
                    serialList.Add(serialData);
                }
                mmStep.SetSerialCommands(serialList);
            }
=======

>>>>>>> 15668b9c2cbb0d64b4dd583bb3c87929c4f9db7f
        }
        // Overrides the ConvertTo method of TypeConverter.
        public override object ConvertTo(ITypeDescriptorContext context,
           CultureInfo culture, object value, Type destinationType)
        {
            return base.ConvertTo(context, culture, value,destinationType);
        }
    }
}
