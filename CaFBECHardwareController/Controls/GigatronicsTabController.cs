using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.HAL;
using DAQ.Environment;

namespace CaFBECHardwareController.Controls
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

        public GigatronicsTabController()
            : base()
        {
            synth = (Gigatronics7100Synth)Environs.Hardware.Instruments["Gigatronics"];
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

        public double GetFrequency()
        {
            return castView.GetFrequency();
        }

        public void SetAmplitude(double amp)
        {
            synth.Connect();
            synth.Amplitude = amp;
            synth.Disconnect();
            castView.UpdateAmplitude(amp);
        }

        public double GetAmplitude()
        {
            return castView.GetAmplitude();
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
