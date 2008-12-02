using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

using ScanMaster.Acquire.Plugin;
using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;

namespace ScanMaster.Acquire.Plugins
{
    [Serializable]
    public class TTLSwitchPlugin : SwitchOutputPlugin
    {
        [NonSerialized]
        private bool state;
        [NonSerialized]
        private Task dot;
        [NonSerialized]
        private DigitalSingleChannelWriter raita;

        protected override void InitialiseSettings()
        {
        }

        public override void AcquisitionStarting()
        {
            dot = new Task("ttlSwitchTask");
            raita = new DigitalSingleChannelWriter(dot.Stream);
            ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["digitalSwitchChannel"]).AddToTask(
                dot);
            dot.Control(TaskAction.Verify);
        }

        public override void ScanStarting()
        {
        }

        public override void ScanFinished()
        {
        }

        public override void AcquisitionFinished()
        {
            dot.Dispose();
        }

        [XmlIgnore]
        public override bool State
        {
            set 
            {
                state = value;
                raita.WriteSingleSampleSingleLine(true, value);
            }
            get { return state; }
        }
    }
}
