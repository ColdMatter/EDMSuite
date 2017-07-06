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
        private WindfreakTabView castView; // Convenience to avoid lots of casting in methods 
        
        protected override GenericView CreateControl()
        {
            castView = new WindfreakTabView();
            castView.InitializeTriggerModes(Enum.GetValues(typeof(WindfreakSynth.TriggerTypes)));
            return castView;
        }

        public WindfreakTabController(WindfreakSynth device) : base()
        {
            windfreak = device;
            ReadSettings();
        }

        public override Dictionary<string, object> Report()
        {
            Dictionary<string, object> report = new Dictionary<string, object>();
            report.Add("TriggerMode", windfreak.TriggerMode);
            report.Add("Channel A Frequency (MHz)", windfreak.ChannelA.Frequency);
            report.Add("Channel A Amplitude (dBm)", windfreak.ChannelA.Amplitude);
            report.Add("Channel A RFOn", windfreak.ChannelA.RFOn);
            report.Add("Channel B Frequency (MHz)", windfreak.ChannelB.Frequency);
            report.Add("Channel B Amplitude (dBm)", windfreak.ChannelB.Amplitude);
            report.Add("Channel B RFOn", windfreak.ChannelB.RFOn);
            return report;
        }

        public void SetFrequency(double freq, bool channel)
        {
            windfreak.Channel(channel).Frequency = freq;
            SyncFrequency();
        }

        public void SetAmplitude(double amp, bool channel)
        {
            windfreak.Channel(channel).Amplitude = amp;
            SyncAmplitude();
        }

        public void SetOutput(bool state, bool channel)
        {
            windfreak.Channel(channel).RFOn = state;
            SyncOutput();
        }

        public void SetTriggerMode(string value)
        {
            WindfreakSynth.TriggerTypes triggerMode;
            Enum.TryParse<WindfreakSynth.TriggerTypes>(value, out triggerMode);
            windfreak.TriggerMode = triggerMode;
        }

        public void SyncFrequency()
        {
            bool channelBool = castView.GetChannel();
            WindfreakSynth.WindfreakChannel channel = windfreak.Channel(channelBool);
            castView.UpdateFrequency(channel.Frequency);
        }

        public void SyncAmplitude()
        {
            bool channelBool = castView.GetChannel();
            WindfreakSynth.WindfreakChannel channel = windfreak.Channel(channelBool);
            castView.UpdateAmplitude(channel.Amplitude);
        }

        public void SyncOutput()
        {
            bool channelBool = castView.GetChannel();
            WindfreakSynth.WindfreakChannel channel = windfreak.Channel(channelBool);
            castView.UpdateOutput(channel.RFOn);
        }

        public void SyncTriggerMode()
        {
            castView.UpdateTriggerMode(windfreak.TriggerMode);
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
            SyncTriggerMode();
        }
    }
}
