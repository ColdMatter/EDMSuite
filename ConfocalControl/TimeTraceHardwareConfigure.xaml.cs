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

using DAQ.Environment;
using DAQ.HAL;

namespace ConfocalControl
{
    /// <summary>
    /// Interaction logic for TimeTraceHardwareConfigure.xaml
    /// </summary>
    public partial class TimeTraceHardwareConfigure : Window
    {
        private List<string> analogInputChannelsKeys;
        private List<string> counterInputChannelsKeys;

        public TimeTraceHardwareConfigure()
        {
            InitializeComponent();

            analogInputChannelsKeys = new List<string>();
            foreach (string key in Environs.Hardware.AnalogInputChannels.Keys)
            {
                analogInputChannelsKeys.Add(key);
            }
            analogInputChannelsKeys.Sort();

            counterInputChannelsKeys = new List<string>();
            foreach (string key in Environs.Hardware.CounterChannels.Keys)
            {
                if (key != "SampleClock") counterInputChannelsKeys.Add(key);
            }
            counterInputChannelsKeys.Sort();

            output_type_box.Items.Add("Counters");
            output_type_box.Items.Add("Analogues");
            output_type_box.SelectedValue = (string)SingleCounterPlugin.GetController().Settings["channel_type"];
            output_box.SelectedValue = (string)SingleCounterPlugin.GetController().Settings["channel"];
        }

        private void ok_button_Click(object sender, RoutedEventArgs e)
        {
            SingleCounterPlugin.GetController().Settings["channel_type"] = (string)output_type_box.SelectedValue;
            SingleCounterPlugin.GetController().Settings["channel"] = (string)output_box.SelectedValue;
            this.Close();
        }

        private void cancel_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void output_type_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            output_box.Items.Clear();
            output_box.SelectedIndex = -1;

            switch (output_type_box.SelectedIndex)
            {
                case 0:
                    foreach (string input in counterInputChannelsKeys)
                    {
                        output_box.Items.Add(input);
                    }
                    if (output_box.Items.Count != 0) output_box.SelectedIndex = 0;
                    break;

                case 1:
                    foreach (string input in analogInputChannelsKeys)
                    {
                        output_box.Items.Add(input);
                    }
                    if (output_box.Items.Count != 0) output_box.SelectedIndex = 0;
                    break;

                default:
                    break;
            }
        }
    }
}
