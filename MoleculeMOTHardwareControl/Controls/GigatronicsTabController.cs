using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.HAL;

namespace MoleculeMOTHardwareControl.Controls
{
    public class GigatronicsTabController : GenericController
    {
        protected Gigatronics7100Synth synth;
        private GigatronicsTabView castView; // Convenience to avoid lots of casting in methods 

        protected override GenericView CreateControl()
        {
            castView = new GigatronicsTabView(this);
            return castView;
        }

        public GigatronicsTabController(Gigatronics7100Synth device)
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
        }

        public void SetAmplitude(double amp)
        {
            synth.Connect();
            synth.Amplitude = amp;
            synth.Disconnect();
        }

        public void SetOutput(bool state)
        {
            synth.Connect();
            synth.Enabled = state;
            synth.Disconnect();
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
