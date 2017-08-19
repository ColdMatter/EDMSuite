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
            report.Add("Channel A Sweep Upper (MHz)", windfreak.ChannelA.SweepUpper);
            report.Add("Channel A Sweep Lower (MHz)", windfreak.ChannelA.SweepLower);
            report.Add("Channel A Sweep Step Size (MHz)", windfreak.ChannelA.SweepStepSize);
            report.Add("Channel A Sweep Step Time (ms)", windfreak.ChannelA.SweepStepTime);
            report.Add("Channel A Sweep Direction", windfreak.ChannelA.SweepDirection);

            report.Add("Channel B Frequency (MHz)", windfreak.ChannelB.Frequency);
            report.Add("Channel B Amplitude (dBm)", windfreak.ChannelB.Amplitude);
            report.Add("Channel B RFOn", windfreak.ChannelB.RFOn);
            report.Add("Channel B Sweep Upper (MHz)", windfreak.ChannelB.SweepUpper);
            report.Add("Channel B Sweep Lower (MHz)", windfreak.ChannelB.SweepLower);
            report.Add("Channel B Sweep Step Size (MHz)", windfreak.ChannelB.SweepStepSize);
            report.Add("Channel B Sweep Step Time (ms)", windfreak.ChannelB.SweepStepTime);
            report.Add("Channel B Sweep Direction", windfreak.ChannelB.SweepDirection);

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

        public void SetSweepUpper(double freq, bool channel)
        {
            windfreak.Channel(channel).SweepUpper = freq;
        }

        public void SetSweepLower(double freq, bool channel)
        {
            windfreak.Channel(channel).SweepLower = freq;
        }

        public void SetSweepStepSize(double freqStep, bool channel)
        {
            windfreak.Channel(channel).SweepStepSize = freqStep;
        }

        public void SetSweepStepTime(double time, bool channel)
        {
            windfreak.Channel(channel).SweepStepTime = time;
        }

        public void SetSweepDirection(bool state, bool channel)
        {
            windfreak.Channel(channel).SweepDirection = state;
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

        public void SyncSweepSettings()
        {
            bool channelBool = castView.GetChannel();
            WindfreakSynth.WindfreakChannel channel = windfreak.Channel(channelBool);
            castView.UpdateSweepUpper(channel.SweepUpper);
            castView.UpdateSweepLower(channel.SweepLower);
            castView.UpdateSweepStepSize(channel.SweepStepSize);
            castView.UpdateSweepStepTime(channel.SweepStepTime);
            castView.UpdateSweepDirection(channel.SweepDirection);
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
            SyncSweepSettings();
        }

        public void ReadSettings()
        {
            windfreak.ReadSettingsFromDevice();
            SyncChannel();
            SyncTriggerMode();
        }
    }
}
