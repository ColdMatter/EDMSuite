using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using NationalInstruments.DAQmx;

namespace NavigatorHardwareControl
{
    /// <summary>
    /// Interaction logic for ChannelManagerForm.xaml
    /// </summary>
    public partial class ChannelManagerForm : Window
    {
        //TODO modify this to load the hardware class defined in DAQ.Environs.Hardware
        private List<String> channels = new List<String>();
        private List<string> AIchannels = DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.AI, PhysicalChannelAccess.External).ToList<string>();
        private List<string> AOchannels = DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.AO, PhysicalChannelAccess.External).ToList<string>();
        public ChannelManagerForm()
        {
            InitializeComponent();
        }

        private void addChannel_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            channels.Add("");
            channelGrid.DataContext = channels;
            channelGrid.ItemsSource = channels;
            channelGrid.UpdateLayout();

        }

        private void removeChannel_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            channels.Remove((String)channelGrid.SelectedItem);
            channelGrid.DataContext = channels;
            channelGrid.ItemsSource = channels;
            channelGrid.UpdateLayout();
        }
    }
}
