using NationalInstruments.Net;
using NationalInstruments.Analysis;
using NationalInstruments.Analysis.Conversion;
using NationalInstruments.Analysis.Dsp;
using NationalInstruments.Analysis.Dsp.Filters;
using NationalInstruments.Analysis.Math;
using NationalInstruments.Analysis.Monitoring;
using NationalInstruments.Analysis.SignalGeneration;
using NationalInstruments.Analysis.SpectralMeasurements;
using NationalInstruments;
using NationalInstruments.DAQmx;
using NationalInstruments.NI4882;
using NationalInstruments.NetworkVariable;
using NationalInstruments.NetworkVariable.WindowsForms;
using NationalInstruments.Tdms;
using NationalInstruments.Controls;
using NationalInstruments.Controls.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ConfocalControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double[,] reading = GalvoPairPlugin.GetController().GetSingleGalvoPairSetpoint();

            double xvalue = reading[0, 0];
            double yvalue = reading[1, 0];

            galvo_X_display.Text = Convert.ToString(xvalue);
            galvo_Y_display.Text = Convert.ToString(yvalue);
        }

        public void galvo_setpoint_Click(object sender, RoutedEventArgs e)
        {
            double[,] reading = GalvoPairPlugin.GetController().GetSingleGalvoPairSetpoint();

            double xvalue = reading[0, 0];
            double yvalue = reading[1, 0];

            galvo_X_display.Text = Convert.ToString(xvalue);
            galvo_Y_display.Text = Convert.ToString(yvalue);
        }

        public void galvo_X_set_TextChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                double valueX;
                double valueY;

                if (galvo_Y_set.Text == "")
                {
                    valueX = Convert.ToDouble(galvo_X_set.Text);
                    valueY = Convert.ToDouble(galvo_Y_display.Text);
                }
                else
                {
                    valueX = Convert.ToDouble(galvo_X_set.Text);
                    valueY = Convert.ToDouble(galvo_Y_set.Text);
                    galvo_Y_set.Text = "";

                }
                galvo_X_set.Text = "";

                GalvoPairPlugin.GetController().SetSingleGalvoPairSetpoint(new double[,] { { valueX} , { valueY } });

                double[,] reading = GalvoPairPlugin.GetController().GetSingleGalvoPairSetpoint();
                double xvalue = reading[0, 0];
                double yvalue = reading[1, 0];

                galvo_X_display.Text = Convert.ToString(xvalue);
                galvo_Y_display.Text = Convert.ToString(yvalue);
            }
        }

        public void galvo_Y_set_TextChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                double valueX;
                double valueY;

                if (galvo_X_set.Text == "")
                {
                    valueX = Convert.ToDouble(galvo_X_display.Text);
                    valueY = Convert.ToDouble(galvo_Y_set.Text);
                }
                else
                {
                    valueX = Convert.ToDouble(galvo_X_set.Text);
                    valueY = Convert.ToDouble(galvo_Y_set.Text);
                    galvo_X_set.Text = "";
                }

                galvo_Y_set.Text = "";

                GalvoPairPlugin.GetController().SetSingleGalvoPairSetpoint(new double[,] { { valueX }, { valueY } });

                double[,] reading = GalvoPairPlugin.GetController().GetSingleGalvoPairSetpoint();
                double xvalue = reading[0, 0];
                double yvalue = reading[1, 0];

                galvo_X_display.Text = Convert.ToString(xvalue);
                galvo_Y_display.Text = Convert.ToString(yvalue);
            }
        }
    }
}
