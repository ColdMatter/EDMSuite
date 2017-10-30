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

            List<string> analogInputChannelsKeys = new List<string>();
            foreach (string key in Environs.Hardware.AnalogInputChannels.Keys)
            {
                analogInputChannelsKeys.Add(key);
            }
            analogInputChannelsKeys.Sort();

            List<string> analogOutputChannelsKeys = new List<string>();
            foreach (string key in Environs.Hardware.AnalogOutputChannels.Keys)
            {
                analogOutputChannelsKeys.Add(key);
            }
            analogOutputChannelsKeys.Sort();

            string galvoX = (string)GalvoPairPlugin.GetController().Settings["GalvoXRead"];
            foreach (string key in analogInputChannelsKeys)
            {
                galvo_x_input_channel_set.Items.Add(key);
                if (key == galvoX)
                {
                    galvo_x_input_channel_set.SelectedValue = key;
                }
            }

            string galvoXControl = (string)GalvoPairPlugin.GetController().Settings["GalvoXControl"];
            foreach (string key in analogOutputChannelsKeys)
            {
                galvo_x_output_channel_set.Items.Add(key);
                if (key == galvoXControl)
                {
                    galvo_x_output_channel_set.SelectedValue = key;
                }
            }

            string galvoY = (string)GalvoPairPlugin.GetController().Settings["GalvoYRead"];
            foreach (string key in analogInputChannelsKeys)
            {
                galvo_y_input_channel_set.Items.Add(key);
                if (key == galvoY)
                {
                    galvo_y_input_channel_set.SelectedValue = key;
                }
            }

            string galvoYControl = (string)GalvoPairPlugin.GetController().Settings["GalvoYControl"];
            foreach (string key in analogOutputChannelsKeys)
            {
                galvo_y_output_channel_set.Items.Add(key);
                if (key == galvoYControl)
                {
                    galvo_y_output_channel_set.SelectedValue = key;
                }
            }
        }

        private bool AcceptableSettings()
        {
            if ((string)galvo_x_input_channel_set.SelectedValue == (string)galvo_y_input_channel_set.SelectedValue)
            {
                MessageBox.Show("Input channels settings unacceptable.");
                return false;
            }
            else if ((string)galvo_x_output_channel_set.SelectedValue == (string)galvo_y_output_channel_set.SelectedValue)
            {
                MessageBox.Show("Output channels settings unacceptable.");
                return false;
            }
            else return true;
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            if (AcceptableSettings())
            {
                GalvoPairPlugin.GetController().Settings["GalvoXRead"] = (string)galvo_x_input_channel_set.SelectedValue;
                GalvoPairPlugin.GetController().Settings["GalvoXControl"] = (string)galvo_x_output_channel_set.SelectedValue;
                GalvoPairPlugin.GetController().Settings["GalvoYRead"] = (string)galvo_y_input_channel_set.SelectedValue;
                GalvoPairPlugin.GetController().Settings["GalvoYControl"] = (string)galvo_y_output_channel_set.SelectedValue;
                this.Close();
            }
        }

        private void cancel_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
