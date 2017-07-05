using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.HAL;

namespace MoleculeMOTHardwareControl.Controls
{
    public class WindfreakTabController : GenericController
    {
        protected WindfreakSynth windfreak;
        
        protected override GenericView CreateControl()
        {
            return new WindfreakTabView();
        }

        public WindfreakTabController(WindfreakSynth device) : base()
        {
            windfreak = device;
        }

        public void SetFreqAmp()
        {
            WindfreakTabView castView = (WindfreakTabView)view;
            double freq = castView.GetFrequency();
            double amp = castView.GetAmplitude();
            bool channelBool = castView.GetChannel();
            WindfreakSynth.WindfreakChannel channel = windfreak.Channel(channelBool);
            channel.SetFrequency(freq);
            channel.SetAmplitude(amp);
        }

        public void SetOutput(bool outputIsOn)
        {
            WindfreakTabView castView = (WindfreakTabView)view;
            bool channelBool = castView.GetChannel();
            WindfreakSynth.WindfreakChannel channel = windfreak.Channel(channelBool);
            channel.SetRF(outputIsOn);
        }

        public void SetTriggerMode(string value)
        {
            WindfreakSynth.TriggerMode triggerMode;
            Enum.TryParse<WindfreakSynth.TriggerMode>(value, out triggerMode);
            windfreak.SetTriggerMode(triggerMode);
        }

        public void SyncFrequency()
        {
            WindfreakTabView castView = (WindfreakTabView)view;
            bool channelBool = castView.GetChannel();
            WindfreakSynth.WindfreakChannel channel = windfreak.Channel(channelBool);
            castView.UpdateFrequency(channel.GetFrequency());
        }

        public void SyncAmplitude()
        {
            WindfreakTabView castView = (WindfreakTabView)view;
            bool channelBool = castView.GetChannel();
            WindfreakSynth.WindfreakChannel channel = windfreak.Channel(channelBool);
            castView.UpdateAmplitude(channel.GetAmplitude());
        }

        public void SyncOutput()
        {
            WindfreakTabView castView = (WindfreakTabView)view;
            bool channelBool = castView.GetChannel();
            WindfreakSynth.WindfreakChannel channel = windfreak.Channel(channelBool);
            castView.UpdateOutput(channel.RFOn());
        }

        public void SyncChannel()
        {
            SyncFrequency();
            SyncAmplitude();
            SyncOutput();
        }

        public void ReadSettings()
        {
            windfreak.ReadSettingsFromDevice();
            SyncChannel();
        }
    }
}
