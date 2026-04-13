using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.HAL;

namespace MoleculeMOTHardwareControl.Controls
{
    public class HPSynthTabController : GenericController
    {
        protected HP8656BSynth synth;
        private HPSynthTabView castView; // Convenience to avoid lots of casting in methods 

        protected override GenericView CreateControl()
        {
            castView = new HPSynthTabView(this);
            return castView;
        }

        public HPSynthTabController(HP8656BSynth device)
            : base()
        {
            synth = device;
            SyncSettings();
        }

        public override Dictionary<string, object> Report()
        {
            Dictionary<string, object> report = new Dictionary<string, object>();
            report.Add("Frequency (MHz)", castView.GetFrequency());
            report.Add("Amplitude (dBm)", castView.GetAmplitude());
            report.Add("RFOn", castView.GetOutput());
            return report;
        }

        public void SetFrequency(double freq)
        {
            synth.Connect();
            synth.Frequency = freq;
            synth.Disconnect();
            castView.UpdateFrequency(freq);
        }

        public void SetAmplitude(double amp)
        {
            synth.Connect();
            synth.Amplitude = amp;
            synth.Disconnect();
            castView.UpdateAmplitude(amp);
        }

        public void SetOutput(bool state)
        {
            synth.Connect();
            synth.Enabled = state;
            synth.Disconnect();
            castView.UpdateOutput(state);
        }

        public void SyncSettings()
        {
            synth.Connect();
            synth.Frequency = castView.GetFrequency();
            synth.Amplitude = castView.GetAmplitude();
            synth.Enabled = castView.GetOutput();
            synth.Disconnect();
        }
    }
}
