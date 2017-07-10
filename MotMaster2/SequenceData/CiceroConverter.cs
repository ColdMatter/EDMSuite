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
            return true;
        }
        // Overrides the ConvertFrom method of TypeConverter.
        public override object ConvertFrom(ITypeDescriptorContext context,
           CultureInfo culture, object value)
        {
            if (value is DataStructures.SequenceData)
            {
                ciceroSequence = value as DataStructures.SequenceData;
                foreach (TimeStep step in ciceroSequence.TimeSteps)
                {
                    SequenceStep mmStep = new SequenceStep();

                    InitialiseSequenceStep(step, mmStep);

                    ConvertAnalogChannelData(step, mmStep);
                    ConvertDigitalChannelData(step, mmStep);
                    ConvertSerialChannelData(step, mmStep);

                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        private static void InitialiseSequenceStep(TimeStep step, SequenceStep mmStep)
        {
            mmStep.Name = step.StepName;
            mmStep.Description = step.Description;
            mmStep.Duration = step.StepDuration.ParameterValue;
            mmStep.Enabled = step.StepEnabled;
            Units timeUnits = step.StepDuration.ParameterUnits;

            //Checks the units of the timestep and converts it to the equivalent object in the MotMaster Sequence step

            if (timeUnits.toLongString() == "ms") mmStep.Timebase = TimebaseUnits.ms;
            else if (timeUnits.toLongString() == "us") mmStep.Timebase = TimebaseUnits.us;
            else if (timeUnits.toLongString() == "s")
                mmStep.Timebase = TimebaseUnits.s;
            else throw new Exception("Incorrect Cicero TimeStep units");

            
            mmStep.RS232Commands = (step.rs232Group.ChannelDatas != null);
        }

        /// <summary>
        /// Goes through the analog data for this step and assigns the data to the equivalent channel in the MOTMaster Sequence. Checks the channel ids are the same and refer to the same hardware channel
        /// </summary>
        /// <param name="ciceroStep"></param>
        /// <param name="mmStep"></param>
        private static void ConvertAnalogChannelData(TimeStep ciceroStep, SequenceStep mmStep)
        {
            Dictionary<int, AnalogGroupChannelData> ciceroChannelData = ciceroStep.AnalogGroup.ChannelDatas;
            
            foreach (int id in ciceroChannelData.Keys)
            {
               LogicalChannel analog = ciceroSettings.logicalChannelManager.Analogs[id];
               AnalogGroupChannelData chan = ciceroChannelData[id];
               if (chan.waveform.YValues.Count == 1) mmStep.SetAnalogDataItem(analog.Name, AnalogChannelSelector.SingleValue, chan.waveform.YValues[0].ToString());
               else if (chan.waveform.EquationString != "") mmStep.SetAnalogDataItem(analog.Name, AnalogChannelSelector.Function, chan.waveform.EquationString);
               else if (chan.waveform.interpolationType == Waveform.InterpolationType.Linear)
                   continue;
            
            }
        }

        private static void ConvertDigitalChannelData(TimeStep ciceroStep, SequenceStep mmStep)
        {

        }

        private static void ConvertSerialChannelData(TimeStep ciceroStep, SequenceStep mmStep)
        {

        }
        // Overrides the ConvertTo method of TypeConverter.
        public override object ConvertTo(ITypeDescriptorContext context,
           CultureInfo culture, object value, Type destinationType)
        {
            return base.ConvertTo(context, culture, value,destinationType);
        }
    }
}
