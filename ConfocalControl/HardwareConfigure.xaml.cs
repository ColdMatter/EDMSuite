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

            string galvoX = (string)GalvoPairPlugin.GetController().Settings["GalvoXRead"];
            foreach (string key in Environs.Hardware.AnalogInputChannels.Keys)
            {
                string pc = ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[key]).PhysicalChannel;
                galvo_x_input_channel_set.Items.Add(pc);
                if (key == galvoX)
                {
                    galvo_x_input_channel_set.SelectedValue = pc;
                }
            }

            string galvoXControl = (string)GalvoPairPlugin.GetController().Settings["GalvoXControl"];
            foreach (string key in Environs.Hardware.AnalogOutputChannels.Keys)
            {
                string pc = ((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[key]).PhysicalChannel;
                galvo_x_output_channel_set.Items.Add(pc);
                if (key == galvoXControl)
                {
                    galvo_x_output_channel_set.SelectedValue = pc;
                }
            }

            string galvoY = (string)GalvoPairPlugin.GetController().Settings["GalvoYRead"];
            foreach (string key in Environs.Hardware.AnalogInputChannels.Keys)
            {
                string pc = ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[key]).PhysicalChannel;
                galvo_y_input_channel_set.Items.Add(pc);
                if (key == galvoY)
                {
                    galvo_y_input_channel_set.SelectedValue = pc;
                }
            }

            string galvoYControl = (string)GalvoPairPlugin.GetController().Settings["GalvoYControl"];
            foreach (string key in Environs.Hardware.AnalogOutputChannels.Keys)
            {
                string pc = ((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[key]).PhysicalChannel;
                galvo_y_output_channel_set.Items.Add(pc);
                if (key == galvoYControl)
                {
                    galvo_y_output_channel_set.SelectedValue = pc;
                }
            }
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cancel_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
