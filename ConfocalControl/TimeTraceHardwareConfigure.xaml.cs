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

        private Dictionary<string, double[]> analogHighLowsIncluded;

        public TimeTraceHardwareConfigure()
        {
            InitializeComponent();

            analogHighLowsIncluded = (Dictionary<string, double[]>)SingleCounterPlugin.GetController().Settings["analogueLowHighs"];

            List<string> includedAnalogInputChannelsKeys = new List<string>();
            foreach (string key in (List<string>)SingleCounterPlugin.GetController().Settings["analogueChannels"])
            {
                includedAnalogInputChannelsKeys.Add(key);

            }
            includedAnalogInputChannelsKeys.Sort();

            foreach (string key in includedAnalogInputChannelsKeys)
            {
                analog_included.Items.Add(key);
            }

            List<string> analogInputChannelsKeys = new List<string>();
            foreach (string key in Environs.Hardware.AnalogInputChannels.Keys)
            {
                analogInputChannelsKeys.Add(key);
            }
            analogInputChannelsKeys.Sort();

            foreach (string key in analogInputChannelsKeys)
            {
                if (!includedAnalogInputChannelsKeys.Contains(key)) analog_not_included.Items.Add(key);
            }

            List<string> includedCounterChannelsKeys = new List<string>();
            foreach (string key in (List<string>)SingleCounterPlugin.GetController().Settings["counterChannels"])
            {
                includedCounterChannelsKeys.Add(key);
            }
            includedCounterChannelsKeys.Sort();

            foreach (string key in includedCounterChannelsKeys)
            {
                counters_included.Items.Add(key);
            }

            List<string> counterChannelsKeys = new List<string>();
            foreach (string key in Environs.Hardware.CounterChannels.Keys)
            {
                counterChannelsKeys.Add(key);
            }
            counterChannelsKeys.Sort();

            foreach (string key in counterChannelsKeys)
            {
                if (!includedCounterChannelsKeys.Contains(key) && !(key == "SampleClock"))
                    counters_not_included.Items.Add(key);
            }
        }

        private void analog_included_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (analog_included.SelectedIndex >= 0)
            {
                string key = (string)analog_included.SelectedValue;
                analogLow_reader.Text = analogHighLowsIncluded[key][0].ToString(); ;
                analogHigh_reader.Text = analogHighLowsIncluded[key][1].ToString();
            }
        }

        private void add_analog_Click(object sender, RoutedEventArgs e)
        {
            if (analog_not_included.SelectedIndex >= 0)
            {
                double lowInput = analogLow_input.Value;
                double highInput = analogHigh_input.Value;

                if (lowInput >= highInput)
                {
                    MessageBox.Show("Analogue bounds unacceptable.");
                }
                else
                {
                    string key = (string)analog_not_included.SelectedValue;
                    analog_not_included.Items.Remove(key);
                    analog_included.Items.Add(key);
                    analogHighLowsIncluded[key] = new double[] { analogLow_input.Value, analogHigh_input.Value };
                    analogLow_input.Value = 0; analogHigh_input.Value = 0;
                }
            }
        }

        private void remove_analog_Click(object sender, RoutedEventArgs e)
        {
            if (analog_included.SelectedIndex >= 0)
            {
                string key = (string)analog_included.SelectedValue;
                analog_included.Items.Remove(key);
                analogHighLowsIncluded.Remove(key);
                analog_not_included.Items.Add(key);
            }
        }

        private void add_counters_Click(object sender, RoutedEventArgs e)
        {
            if (counters_not_included.SelectedIndex >= 0)
            {
                string key = (string)counters_not_included.SelectedValue;
                counters_not_included.Items.Remove(key);
                counters_included.Items.Add(key);
            }
        }

        private void remove_counters_Click(object sender, RoutedEventArgs e)
        {
            if (counters_included.SelectedIndex >= 0)
            {
                string key = (string)counters_included.SelectedValue;
                counters_included.Items.Remove(key);
                counters_not_included.Items.Add(key);
            }
        }

        private void cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ok_Button_Click(object sender, RoutedEventArgs e)
        {
            if (counters_included.Items.Count < 1)
            {
                MessageBox.Show("Need at least one counter");
                return;
            }

            List<string> new_analog = new List<string>();
            foreach (string key in analog_included.Items)
            {
                new_analog.Add(key);
            }
            SingleCounterPlugin.GetController().Settings["analogueChannels"] = new_analog;
            SingleCounterPlugin.GetController().Settings["analogueLowHighs"] = analogHighLowsIncluded;

            List<string> new_counters = new List<string>();
            foreach (string key in counters_included.Items)
            {
                new_counters.Add(key);
            }
            SingleCounterPlugin.GetController().Settings["counterChannels"] = new_counters;

            this.Close();
        }
    }
}
