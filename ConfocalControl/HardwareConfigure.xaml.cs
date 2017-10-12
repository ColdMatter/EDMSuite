using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;

using NationalInstruments;
using NationalInstruments.DAQmx;

using DAQ.Environment;
using DAQ.HAL;

namespace ConfocalControl
{
    /// <summary>
    /// Interaction logic for HardwareConfigure.xaml
    /// </summary>
    public partial class HardwareConfigure : Window
    {
        public HardwareConfigure()
        {
            InitializeComponent();

            string sampleclock = ((CounterChannel)Environs.Hardware.CounterChannels["SampleClock"]).PhysicalChannel;

            foreach (string pc in DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.CI, PhysicalChannelAccess.External))
            {
                if (pc != sampleclock) APD1_channel_set.Items.Add(pc);
            }

            string current = ((CounterChannel)Environs.Hardware.CounterChannels["ConfocalAPD"]).PhysicalChannel;
            APD1_channel_set.SelectedValue = current;
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            ConfocalHardware Confhard = new ConfocalHardware((string)APD1_channel_set.SelectedValue);
            Environs.Hardware = Confhard;
            this.Close();
        }

        private void cancel_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
