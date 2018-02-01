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
using NationalInstruments.NetworkVariable;
using NationalInstruments.NetworkVariable.WindowsForms;
using NationalInstruments.Tdms;
using NationalInstruments.Controls;
using NationalInstruments.Controls.Rendering;
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
using System.Globalization;

namespace ConfocalControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Class members

        private PluginSettings settings = new PluginSettings("foo_main");

        private void InitialiseSettings()
        {
            settings = PluginSaveLoad.LoadSettings("mainWindow");

            if (settings.Keys.Count != 1)
            {
                settings["saveFile"] = "file_name";
            }
        }

        #endregion

        #region Initialization

        public MainWindow()
        {
            InitializeComponent();

            InitialiseSettings();

            try
            {
                galvo_X_set.Text = (string)GalvoPairPlugin.GetController().Settings["GalvoXInit"];
                galvo_Y_set.Text = (string)GalvoPairPlugin.GetController().Settings["GalvoYInit"];
                GalvoPairPlugin.GetController().AcquisitionStarting();
                double xvalue = GalvoPairPlugin.GetController().GetGalvoXSetpoint();
                double yvalue = GalvoPairPlugin.GetController().GetGalvoYSetpoint();
                GalvoPairPlugin.GetController().AcquisitionFinished();
                galvo_X_display.Text = string.Format("{0:0.00000}", xvalue);
                galvo_Y_display.Text = string.Format("{0:0.00000}", yvalue);
            }
            catch (DaqException e1)
            {
                MessageBox.Show("Caught exception: " + e1.Message);
                if (GalvoPairPlugin.GetController().IsRunning()) GalvoPairPlugin.GetController().AcquisitionFinished();
            }
            finally
            {
                fileName_set.Text = (string)settings["saveFile"];

                exposure_set.Value = TimeTracePlugin.GetController().GetExposure();
                int val = TimeTracePlugin.GetController().GetBufferSize();
                buffer_size_set.Value = val;
                binNumber_set.Value = (int)TimeTracePlugin.GetController().Settings["binNumber"];

                TimeTracePlugin.GetController().setTextBox += singleCounter_setTextBox;
                TimeTracePlugin.GetController().setWaveForm += singleCounter_setWaveForm;
                TimeTracePlugin.GetController().DaqProblem += singleCounter_problemHandler;

                FastMultiChannelRasterScan.GetController().LineFinished += rasterScan_setLines;
                FastMultiChannelRasterScan.GetController().ScanFinished += rasterScan_End;
                FastMultiChannelRasterScan.GetController().DaqProblem += rasterScan_problemHandler;

                CounterOptimizationPlugin.GetController().Data += opt_setLines;
                CounterOptimizationPlugin.GetController().OptFinished += opt_End;
                CounterOptimizationPlugin.GetController().DaqProblem += opt_problemHandler;

                scan_x_min_set.Value = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXStart"];
                scan_x_max_set.Value = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXEnd"];
                scan_x_res_set.Value = Convert.ToInt32((double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXRes"]);
                scan_y_min_set.Value = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoYStart"];
                scan_y_max_set.Value = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoYEnd"];
                scan_y_res_set.Value = Convert.ToInt32((double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoYRes"]);

                output_type_box.Items.Add("Counters");
                output_type_box.Items.Add("Analogues");
                output_type_box.SelectedIndex = 0;

                output_type_box_TimeTrace.Items.Add("Counters");
                output_type_box_TimeTrace.Items.Add("Analogues");
                output_type_box_TimeTrace.SelectedIndex = 0;
            }
        }

            #endregion

        #region Galvo control events

        public void galvo_setpoint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GalvoPairPlugin.GetController().AcquisitionStarting();
                double xvalue = GalvoPairPlugin.GetController().GetGalvoXSetpoint();
                double yvalue = GalvoPairPlugin.GetController().GetGalvoYSetpoint();
                GalvoPairPlugin.GetController().AcquisitionFinished();

                galvo_X_display.Text = string.Format("{0:0.00000}", xvalue);
                galvo_Y_display.Text = string.Format("{0:0.00000}", yvalue);
            }
            catch (DaqException e1)
            {
                MessageBox.Show("Caught exception: " + e1.Message);
                if (GalvoPairPlugin.GetController().IsRunning()) GalvoPairPlugin.GetController().AcquisitionFinished();
            }
        }

        private void galvo_X_set_TextChanged(object sender, TextChangedEventArgs e)
        {
            GalvoPairPlugin.GetController().Settings["GalvoXInit"] = galvo_X_set.Text;
        }

        private void galvo_Y_set_TextChanged(object sender, TextChangedEventArgs e)
        {
            GalvoPairPlugin.GetController().Settings["GalvoYInit"] = galvo_Y_set.Text;
        }

        private void galvosSet_Button_Click(object sender, RoutedEventArgs e)
        {
            if (galvo_X_set.Text == "" && galvo_Y_set.Text == "")
            {
                return;
            }
            else
            {
                try
                {
                    double valueX;
                    double valueY;
                    valueX = Convert.ToDouble(galvo_X_set.Text);
                    valueY = Convert.ToDouble(galvo_Y_set.Text);
                    GalvoPairPlugin.GetController().AcquisitionStarting();
                    GalvoPairPlugin.GetController().SetGalvoXSetpoint(valueX);
                    GalvoPairPlugin.GetController().SetGalvoYSetpoint(valueY);
                    double xvalue = GalvoPairPlugin.GetController().GetGalvoXSetpoint();
                    double yvalue = GalvoPairPlugin.GetController().GetGalvoYSetpoint();
                    GalvoPairPlugin.GetController().AcquisitionFinished();

                    galvo_X_display.Text = string.Format("{0:0.00000}", xvalue);
                    galvo_Y_display.Text = string.Format("{0:0.00000}", yvalue);
                }
                catch (FormatException e1)
                {
                    MessageBox.Show("Caught exception: " + e1.Message);
                    if (GalvoPairPlugin.GetController().IsRunning()) GalvoPairPlugin.GetController().AcquisitionFinished();
                }
                catch (DaqException e1)
                {
                    MessageBox.Show("Caught exception: " + e1.Message);
                    if (GalvoPairPlugin.GetController().IsRunning()) GalvoPairPlugin.GetController().AcquisitionFinished();
                }
            }
        }

        public void galvo_X_set_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !GalvoPairPlugin.GetController().IsRunning())
            {
                if (galvo_X_set.Text == "")
                {
                    return;
                }
                else
                {
                    double valueX;
                    double valueY;

                    if (galvo_Y_set.Text == "")
                    {
                        try
                        {
                            valueX = Convert.ToDouble(galvo_X_set.Text);
                            GalvoPairPlugin.GetController().AcquisitionStarting();
                            GalvoPairPlugin.GetController().SetGalvoXSetpoint(valueX);
                            double xvalue = GalvoPairPlugin.GetController().GetGalvoXSetpoint();
                            double yvalue = GalvoPairPlugin.GetController().GetGalvoYSetpoint();
                            GalvoPairPlugin.GetController().AcquisitionFinished();

                            galvo_X_display.Text = string.Format("{0:0.00000}", xvalue);
                            galvo_Y_display.Text = string.Format("{0:0.00000}", yvalue);
                        }
                        catch(FormatException e1)
                        {
                            MessageBox.Show("Caught exception: " + e1.Message);
                            if (GalvoPairPlugin.GetController().IsRunning()) GalvoPairPlugin.GetController().AcquisitionFinished();
                        }
                        catch (DaqException e1)
                        {
                            MessageBox.Show("Caught exception: " + e1.Message);
                            if (GalvoPairPlugin.GetController().IsRunning()) GalvoPairPlugin.GetController().AcquisitionFinished();
                        }
                    }
                    else
                    {
                        try
                        {
                            valueX = Convert.ToDouble(galvo_X_set.Text);
                            valueY = Convert.ToDouble(galvo_Y_set.Text);
                            GalvoPairPlugin.GetController().AcquisitionStarting();
                            GalvoPairPlugin.GetController().SetGalvoXSetpoint(valueX);
                            GalvoPairPlugin.GetController().SetGalvoYSetpoint(valueY);
                            double xvalue = GalvoPairPlugin.GetController().GetGalvoXSetpoint();
                            double yvalue = GalvoPairPlugin.GetController().GetGalvoYSetpoint();
                            GalvoPairPlugin.GetController().AcquisitionFinished();

                            galvo_X_display.Text = string.Format("{0:0.00000}", xvalue);
                            galvo_Y_display.Text = string.Format("{0:0.00000}", yvalue);
                        }
                        catch(FormatException e1)
                        {
                            MessageBox.Show("Caught exception: " + e1.Message);
                            if (GalvoPairPlugin.GetController().IsRunning()) GalvoPairPlugin.GetController().AcquisitionFinished();
                        }
                        catch (DaqException e1)
                        {
                            MessageBox.Show("Caught exception: " + e1.Message);
                            if (GalvoPairPlugin.GetController().IsRunning()) GalvoPairPlugin.GetController().AcquisitionFinished();
                        }
                    }
                }
            }
        }

        public void galvo_Y_set_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !GalvoPairPlugin.GetController().IsRunning())
            {
                if (galvo_Y_set.Text == "")
                {
                    return;
                }
                else
                {
                    double valueX;
                    double valueY;

                    if (galvo_X_set.Text == "")
                    {
                        try
                        {
                            valueY = Convert.ToDouble(galvo_Y_set.Text);
                            GalvoPairPlugin.GetController().AcquisitionStarting();
                            GalvoPairPlugin.GetController().SetGalvoYSetpoint(valueY);
                            double xvalue = GalvoPairPlugin.GetController().GetGalvoXSetpoint();
                            double yvalue = GalvoPairPlugin.GetController().GetGalvoYSetpoint();
                            GalvoPairPlugin.GetController().AcquisitionFinished();

                            galvo_X_display.Text = string.Format("{0:0.00000}", xvalue);
                            galvo_Y_display.Text = string.Format("{0:0.00000}", yvalue);
                        }
                        catch(FormatException e1)
                        {
                            MessageBox.Show("Caught exception: " + e1.Message);
                            if (GalvoPairPlugin.GetController().IsRunning()) GalvoPairPlugin.GetController().AcquisitionFinished();
                        }
                        catch (DaqException e1)
                        {
                            MessageBox.Show("Caught exception: " + e1.Message);
                            if (GalvoPairPlugin.GetController().IsRunning()) GalvoPairPlugin.GetController().AcquisitionFinished();
                        }
                    }
                    else
                    {
                        try
                        {
                            valueX = Convert.ToDouble(galvo_X_set.Text);
                            valueY = Convert.ToDouble(galvo_Y_set.Text);
                            GalvoPairPlugin.GetController().AcquisitionStarting();
                            GalvoPairPlugin.GetController().SetGalvoXSetpoint(valueX);
                            GalvoPairPlugin.GetController().SetGalvoYSetpoint(valueY);
                            double xvalue = GalvoPairPlugin.GetController().GetGalvoXSetpoint();
                            double yvalue = GalvoPairPlugin.GetController().GetGalvoYSetpoint();
                            GalvoPairPlugin.GetController().AcquisitionFinished();

                            galvo_X_display.Text = string.Format("{0:0.00000}", xvalue);
                            galvo_Y_display.Text = string.Format("{0:0.00000}", yvalue);
                        }
                        catch(FormatException e1)
                        {
                            MessageBox.Show("Caught exception: " + e1.Message);
                            if (GalvoPairPlugin.GetController().IsRunning()) GalvoPairPlugin.GetController().AcquisitionFinished();
                            if (TimeTracePlugin.GetController().IsRunning()) TimeTracePlugin.GetController().AcquisitionFinished();
                        }
                        catch (DaqException e1)
                        {
                            MessageBox.Show("Caught exception: " + e1.Message);
                            if (GalvoPairPlugin.GetController().IsRunning()) GalvoPairPlugin.GetController().AcquisitionFinished();
                            if (TimeTracePlugin.GetController().IsRunning()) TimeTracePlugin.GetController().AcquisitionFinished();
                        }
                    }
                }
            }
        }

        #endregion

        #region Timetrace events

        private void output_type_box_TimeTrace_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            output_box_TimeTrace.Items.Clear();
            output_box_TimeTrace.SelectedIndex = -1;

            switch (output_type_box_TimeTrace.SelectedIndex)
            {
                case 0:
                    TimeTracePlugin.GetController().Settings["channel_type"] = "Counters";
                    foreach (string input in TimeTracePlugin.GetController().historicCounterChannels)
                    {
                        output_box_TimeTrace.Items.Add(input);
                    }
                    if (output_box_TimeTrace.Items.Count != 0) output_box_TimeTrace.SelectedIndex = 0;
                    break;

                case 1:
                    TimeTracePlugin.GetController().Settings["channel_type"] = "Analogues";
                    foreach (string input in TimeTracePlugin.GetController().historicAnalogueChannels)
                    {
                        output_box_TimeTrace.Items.Add(input);
                    }
                    if (output_box_TimeTrace.Items.Count != 0) output_box_TimeTrace.SelectedIndex = 0;
                    break;

                default:
                    break;
            }
        }

        private void output_box_TimeTrace_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (output_type_box_TimeTrace.SelectedIndex)
            {
                case 0:
                    if (output_box_TimeTrace.SelectedIndex >= 0)
                    {
                        TimeTracePlugin.GetController().Settings["display_channel_index"] = output_box_TimeTrace.SelectedIndex;
                        TimeTracePlugin.GetController().RequestHistoricData();
                    }
                    else
                    {
                        this.APD_monitor.DataSource = null;
                        this.APD_hist.DataSource = null;
                    }
                    break;

                case 1:
                    if (output_box_TimeTrace.SelectedIndex >= 0)
                    {
                        TimeTracePlugin.GetController().Settings["display_channel_index"] = output_box_TimeTrace.SelectedIndex;
                        TimeTracePlugin.GetController().RequestHistoricData();
                    }
                    else
                    {
                        this.APD_monitor.DataSource = null;
                        this.APD_hist.DataSource = null;
                    }
                    break;

                default:
                    break;
            }
        }

        public void singleCounter_setTextBox(double value)
        {
            // If on a different thread 
            if (Application.Current.Dispatcher.CheckAccess())
            {
                single_photon_counts.Text = value.ToString("G6", CultureInfo.InvariantCulture);
            }

            else
            {
                Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() =>
                        {
                            this.single_photon_counts.Text = value.ToString("G6", CultureInfo.InvariantCulture);
                        }
                ));
            }
        }

        public void singleCounter_setWaveForm(double[] values, Point[] histData)
        {
            // If on a different thread 
            if (Application.Current.Dispatcher.CheckAccess())
            {
                APD_monitor.DataSource = values;
                APD_hist.DataSource = histData;
            }

            else
            {
                Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() =>
                    {
                        this.APD_monitor.DataSource = values;
                        this.APD_hist.DataSource = histData;
                    }
                ));
            }
        }

        public void singleCounter_problemHandler(DaqException e)
        {
            MessageBox.Show("Caught exception: " + e.Message);
            if (TimeTracePlugin.GetController().IsRunning()) TimeTracePlugin.GetController().AcquisitionFinished();

            Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.oscilloscope_switch.Value = false;
                           this.exposure_set.IsEnabled = true;
                           this.rasterScan_switch.IsReadOnly = false;
                           this.timetrace_hardware_MenuItem.IsEnabled = true;
                           this.loadSettings_MenuItem.IsEnabled = true;
                           this.single_photon_counts.Text = "Stopped";
                           this.galvo_setpoint_reader.IsEnabled = true;
                           this.galvosSet_Button.IsEnabled = true;
                           this.galvo_X_set.IsReadOnly = false;
                           this.galvo_Y_set.IsReadOnly = false;
                           this.set_galvos_from_scan.IsEnabled = true;
                           this.optimize_Button.IsEnabled = true;
                       }
                   ));
        }

        private void oscilloscope_Click(object sender, RoutedEventArgs e)
        {
            if (! oscilloscope_switch.Value)
            {
                Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.oscilloscope_switch.Value = true;
                           this.exposure_set.IsEnabled = false;
                           this.rasterScan_switch.IsReadOnly = true;
                           this.timetrace_hardware_MenuItem.IsEnabled = false;
                           this.loadSettings_MenuItem.IsEnabled = false;
                           this.optimize_Button.IsEnabled = false;

                           bool HasGalvoX = ((List<string>)TimeTracePlugin.GetController().Settings["analogueChannels"]).Any(s => s == (string)GalvoPairPlugin.GetController().Settings["GalvoXRead"]);
                           bool HasGalvoY = ((List<string>)TimeTracePlugin.GetController().Settings["analogueChannels"]).Any(s => s == (string)GalvoPairPlugin.GetController().Settings["GalvoYRead"]);

                           if (HasGalvoX || HasGalvoY)
                           {
                               this.galvo_setpoint_reader.IsEnabled = false;
                               this.galvosSet_Button.IsEnabled = false;
                               this.galvo_X_set.IsReadOnly = true;
                               this.galvo_Y_set.IsReadOnly = true;
                               this.set_galvos_from_scan.IsEnabled = false;
                           }
                       }
                   ));

                Thread thread = new Thread(new ThreadStart(TimeTracePlugin.GetController().ContinuousAcquisition));
                thread.IsBackground = true;
                thread.Start();
                output_type_box_TimeTrace_SelectionChanged(null, null);

            }
            else
            {
                TimeTracePlugin.GetController().StopContinuousAcquisition();
                Thread.Sleep(1000);
                Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.oscilloscope_switch.Value = false;
                           this.exposure_set.IsEnabled = true;
                           this.rasterScan_switch.IsReadOnly = false;
                           this.timetrace_hardware_MenuItem.IsEnabled = true;
                           this.loadSettings_MenuItem.IsEnabled = true;
                           this.single_photon_counts.Text = "Stopped";
                           this.galvo_setpoint_reader.IsEnabled = true;
                           this.galvosSet_Button.IsEnabled = true;
                           this.galvo_X_set.IsReadOnly = false;
                           this.galvo_Y_set.IsReadOnly = false;
                           this.set_galvos_from_scan.IsEnabled = true;
                           this.optimize_Button.IsEnabled = true;
                       }
                   ));
            }
        }

        private void exposure_set_ValueChanged(object sender, ValueChangedEventArgs<double> e)
        {
            if (e.NewValue > 0) TimeTracePlugin.GetController().UpdateExposure(e.NewValue);
            else exposure_set.Value = e.OldValue;
        }

        private void buffer_size_set_ValueChanged(object sender, ValueChangedEventArgs<int> e)
        {
            if (e.NewValue > 1)
            {
                TimeTracePlugin.GetController().Settings["bufferSize"] = e.NewValue;
            }
            else buffer_size_set.Value = e.OldValue;
        }

        private void binNumber_set_ValueChanged(object sender, ValueChangedEventArgs<int> e)
        {
            if (e.NewValue > 1)
            {
                TimeTracePlugin.GetController().Settings["binNumber"] = e.NewValue;
            }
            else binNumber_set.Value = e.OldValue;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.APD_monitor.DataSource != null)
            {
                TimeTracePlugin.GetController().DeleteHistoricData();

                if (!TimeTracePlugin.GetController().IsRunning())
                {
                    Application.Current.Dispatcher.BeginInvoke(
                           DispatcherPriority.Background,
                           new Action(() =>
                           {
                           this.APD_monitor.DataSource = null;
                           this.APD_hist.DataSource = null;
                            }
                   ));
                }
            }
        }

        #endregion

        #region RasterScan events

        private void output_type_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            output_box.Items.Clear();
            output_box.SelectedIndex = -1;

            switch (output_type_box.SelectedIndex)
            {
                case 0:
                    foreach (string input in (List<string>)FastMultiChannelRasterScan.GetController().dataOutputHistory.historicSettings["counterChannels"])
                    {
                        output_box.Items.Add(input);
                    }
                    if (output_box.Items.Count != 0) output_box.SelectedIndex = 0;
                    break;
                case 1:
                    foreach (string input in (List<string>)FastMultiChannelRasterScan.GetController().dataOutputHistory.historicSettings["analogueChannels"])
                    {
                        output_box.Items.Add(input);
                    }
                    if (output_box.Items.Count != 0) output_box.SelectedIndex = 0;
                    break;
                default:
                    break;
            }
        }

        private void rasterScan_setLines(MultiChannelData data)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    switch (output_type_box.SelectedIndex)
                    {
                        case 0:
                            if (output_box.SelectedIndex >= 0)
                            {
                                Point3D[] dataSpecific = data.GetCounterData(output_box.SelectedIndex);
                                if (dataSpecific != null)
                                {
                                    this.rasterScan_display.DataSource = dataSpecific;
                                }
                                else this.rasterScan_display.DataSource = null;
                            }
                            else this.rasterScan_display.DataSource = null;
                            break;

                        case 1:
                            if (output_box.SelectedIndex >= 0)
                            {
                                Point3D[] dataSpecific = data.GetAnalogueData(output_box.SelectedIndex);
                                if (dataSpecific != null)
                                {
                                    this.rasterScan_display.DataSource = dataSpecific;
                                }
                                else this.rasterScan_display.DataSource = null;
                            }
                            else this.rasterScan_display.DataSource = null;
                            break;

                        default:
                            this.rasterScan_display.DataSource = null;
                            break;
                    }

                    double range_low = rasterScan_display.ColorScale.Range.Minimum;
                    this.raster_plot_range_low.Text = range_low.ToString("G6", CultureInfo.InvariantCulture);
                    double range_high = rasterScan_display.ColorScale.Range.Maximum;
                    this.raster_plot_range_high.Text = range_high.ToString("G6", CultureInfo.InvariantCulture);
                }
            ));
        }

        private void rasterScan_problemHandler(DaqException e)
        {
            MessageBox.Show("Caught exception: " + e.Message);
            if (FastMultiChannelRasterScan.GetController().IsRunning())
            {
                FastMultiChannelRasterScan.GetController().AcquisitionFinishing();
            }
            
            Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.rasterScan_switch.Value = false;
                           this.oscilloscope_switch.IsReadOnly = false;
                           this.exposure_set.IsEnabled = true;
                           this.galvo_setpoint_reader.IsEnabled = true;
                           this.galvosSet_Button.IsEnabled = true;
                           this.galvo_X_set.IsReadOnly = false;
                           this.galvo_Y_set.IsReadOnly = false;
                           this.scan_x_min_set.IsEnabled = true;
                           this.scan_x_max_set.IsEnabled = true;
                           this.scan_x_res_set.IsEnabled = true;
                           this.scan_y_min_set.IsEnabled = true;
                           this.scan_y_max_set.IsEnabled = true;
                           this.scan_y_res_set.IsEnabled = true;
                           this.set_galvos_from_scan.IsEnabled = true;
                           this.hardware_MenuItem.IsEnabled = true;
                           this.rasterScan_hardware_MenuItem.IsEnabled = true;
                           this.loadSettings_MenuItem.IsEnabled = true;
                           this.rasterScan_lineDisplay.DataSource = null;
                           this.optimize_Button.IsEnabled = true;
                       }
                   ));
        }

        private void rasterScan_Click(object sender, RoutedEventArgs e)
        {
            if (!rasterScan_switch.Value)
            {
                if (!FastMultiChannelRasterScan.GetController().AcceptableSettings())
                {                
                    Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.rasterScan_switch.Value = false;
                       }));
                    return;
                }

                Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.rasterScan_switch.Value = true;
                           this.oscilloscope_switch.IsReadOnly = true;
                           this.exposure_set.IsEnabled = false;
                           this.galvo_setpoint_reader.IsEnabled = false;
                           this.galvosSet_Button.IsEnabled = false;
                           this.galvo_X_set.IsReadOnly = true;
                           this.galvo_Y_set.IsReadOnly = true;
                           this.scan_x_min_set.IsEnabled = false;
                           this.scan_x_max_set.IsEnabled = false;
                           this.scan_x_res_set.IsEnabled = false;
                           this.scan_y_min_set.IsEnabled = false;
                           this.scan_y_max_set.IsEnabled = false;
                           this.scan_y_res_set.IsEnabled = false;
                           this.set_galvos_from_scan.IsEnabled = false;
                           this.hardware_MenuItem.IsEnabled = false;
                           this.rasterScan_hardware_MenuItem.IsEnabled = false;
                           this.loadSettings_MenuItem.IsEnabled = false;
                           this.optimize_Button.IsEnabled = false;

                           double hRange = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXEnd"] - (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXStart"];
                           double vRange = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoYEnd"] - (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoYStart"];
                           
                           double axMin = 0;
                           double axHorizontalMax;
                           double axVerticalMax;

                           if(hRange > vRange)
                           {
                               double scale = hRange / vRange;
                               axHorizontalMax = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXRes"] + 1;
                               axVerticalMax = scale * ((double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoYRes"] + 1);
                           }
                           else
                           {
                               double scale = vRange / hRange;
                               axHorizontalMax = scale * ((double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXRes"] + 1);
                               axVerticalMax = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoYRes"] + 1;
                           }

                           AxisDouble axHorizontal = new AxisDouble { Orientation = Orientation.Horizontal, Range = Range.Create(axMin, axHorizontalMax), Adjuster = null };
                           this.rasterScan_display.HorizontalAxis = axHorizontal;
                           AxisDouble axVertical = new AxisDouble { Orientation = Orientation.Vertical, Range = Range.Create(axMin, axVerticalMax), Adjuster = null };
                           this.rasterScan_display.VerticalAxis = axVertical;
                       }
                   ));

                Thread thread;
                thread = new Thread(new ThreadStart(FastMultiChannelRasterScan.GetController().SynchronousStartScan));
                thread.IsBackground = true;
                thread.Start();
                output_type_box_SelectionChanged(null, null);
            }
            else
            {
                FastMultiChannelRasterScan.GetController().StopScan();
                Thread.Sleep(1000);

                Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.rasterScan_switch.Value = false;
                           this.oscilloscope_switch.IsReadOnly = false;
                           this.exposure_set.IsEnabled = true;
                           this.galvo_setpoint_reader.IsEnabled = true;
                           this.galvosSet_Button.IsEnabled = true;
                           this.galvo_X_set.IsReadOnly = false;
                           this.galvo_Y_set.IsReadOnly = false;
                           this.scan_x_min_set.IsEnabled = true;
                           this.scan_x_max_set.IsEnabled = true;
                           this.scan_x_res_set.IsEnabled = true;
                           this.scan_y_min_set.IsEnabled = true;
                           this.scan_y_max_set.IsEnabled = true;
                           this.scan_y_res_set.IsEnabled = true;
                           this.set_galvos_from_scan.IsEnabled = true;
                           this.hardware_MenuItem.IsEnabled = true;
                           this.rasterScan_hardware_MenuItem.IsEnabled = true;
                           this.loadSettings_MenuItem.IsEnabled = true;
                           this.rasterScan_lineDisplay.DataSource = null;
                           this.optimize_Button.IsEnabled = true;
                       }
                   ));
            }
        }

        private void rasterScan_End()
        {
            Thread.Sleep(1000);
            Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.rasterScan_switch.Value = false;
                           this.oscilloscope_switch.IsReadOnly = false;
                           this.exposure_set.IsEnabled = true;
                           this.galvo_setpoint_reader.IsEnabled = true;
                           this.galvosSet_Button.IsEnabled = true;
                           this.galvo_X_set.IsReadOnly = false;
                           this.galvo_Y_set.IsReadOnly = false;
                           this.scan_x_min_set.IsEnabled = true;
                           this.scan_x_max_set.IsEnabled = true;
                           this.scan_x_res_set.IsEnabled = true;
                           this.scan_y_min_set.IsEnabled = true;
                           this.scan_y_max_set.IsEnabled = true;
                           this.scan_y_res_set.IsEnabled = true;
                           this.set_galvos_from_scan.IsEnabled = true;
                           this.hardware_MenuItem.IsEnabled = true;
                           this.rasterScan_hardware_MenuItem.IsEnabled = true;
                           this.loadSettings_MenuItem.IsEnabled = true;
                           this.rasterScan_lineDisplay.DataSource = null;
                           this.optimize_Button.IsEnabled = true;

                           if ((bool)this.save_automatic.IsChecked)
                           {
                               FastMultiChannelRasterScan.GetController().SaveDataAutomatic(DateTime.Today.ToString("yy-MM-dd") + "_" + DateTime.Now.ToString("HH-mm-ss") + "_" + "rasterScan_" + (string)settings["saveFile"] + ".txt");
                           }
                       }
                   ));
        }

        private void scan_x_min_set_ValueChanged(object sender, ValueChangedEventArgs<double> e)
        {
            FastMultiChannelRasterScan.GetController().scanSettings["GalvoXStart"] = e.NewValue;
        }

        private void scan_x_max_set_ValueChanged(object sender, ValueChangedEventArgs<double> e)
        {
            FastMultiChannelRasterScan.GetController().scanSettings["GalvoXEnd"] = e.NewValue;
        }

        private void scan_x_res_set_ValueChanged(object sender, ValueChangedEventArgs<int> e)
        {
            FastMultiChannelRasterScan.GetController().scanSettings["GalvoXRes"] = (double)e.NewValue;
        }

        private void scan_y_min_set_ValueChanged(object sender, ValueChangedEventArgs<double> e)
        {
            FastMultiChannelRasterScan.GetController().scanSettings["GalvoYStart"] = e.NewValue;
        }

        private void scan_y_max_set_ValueChanged(object sender, ValueChangedEventArgs<double> e)
        {
            FastMultiChannelRasterScan.GetController().scanSettings["GalvoYEnd"] = e.NewValue;
        }

        private void scan_y_res_set_ValueChanged(object sender, ValueChangedEventArgs<int> e)
        {
            FastMultiChannelRasterScan.GetController().scanSettings["GalvoYRes"] = (double)e.NewValue;
        }

        private void up_Button_Click(object sender, RoutedEventArgs e)
        {
            scan_cursor.Index += Convert.ToInt32((double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXRes"]);
            if (set_galvos_from_scan.IsEnabled && (bool)moveAndSet_CheckBox.IsChecked) set_galvos_from_scan_Click(this, null); 
        }

        private void down_Button_Click(object sender, RoutedEventArgs e)
        {
            if (scan_cursor.Index > (Convert.ToInt32((double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXRes"]) - 1))
            {
                scan_cursor.Index -= Convert.ToInt32((double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXRes"]);
                if (set_galvos_from_scan.IsEnabled && (bool)moveAndSet_CheckBox.IsChecked) set_galvos_from_scan_Click(this, null); 
            }
        }

        private void left_Button_Click(object sender, RoutedEventArgs e)
        {
            if ((scan_cursor.Index % Convert.ToInt32((double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXRes"])) != 0)
            {
                scan_cursor.Index -= 1;
                if (set_galvos_from_scan.IsEnabled && (bool)moveAndSet_CheckBox.IsChecked) set_galvos_from_scan_Click(this, null); 
            }
        }

        private void right_Button_Click(object sender, RoutedEventArgs e)
        {
            if ((scan_cursor.Index % Convert.ToInt32((double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXRes"])) != Convert.ToInt32((double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXRes"]) - 1)
            {
                scan_cursor.Index += 1;
                if (set_galvos_from_scan.IsEnabled && (bool)moveAndSet_CheckBox.IsChecked) set_galvos_from_scan_Click(this, null); 
            }
        }

        private void cursor_PositionChanged(object sender, EventArgs e)
        {
            if (scan_cursor.Value.Count > 2 && !CounterOptimizationPlugin.GetController().IsRunning())
            {
                double xVal = Convert.ToDouble(scan_cursor.Value[0]) - 1;
                double yVal = Convert.ToDouble(scan_cursor.Value[1]) - 1;

                double hGridPoints = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXRes"];
                double vGridPoints = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoYRes"];
                double hStart = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXStart"];
                double vStart = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoYStart"];
                double hRange = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXEnd"] - hStart;
                double vRange = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoYEnd"] - vStart;
                double hres = hRange / (hGridPoints - 1);
                double vres = vRange / (vGridPoints - 1);

                if (galvo_x_scan_pos != null && galvo_y_scan_pos != null)
                {
                    galvo_x_scan_pos.Text = string.Format("{0:0.000}", hStart + hres * xVal);
                    galvo_y_scan_pos.Text = string.Format("{0:0.000}", vStart + vres * yVal);
                }

                object value = scan_cursor.Value[2];
                cursor_signal_value.Text = (Convert.ToDouble(value)).ToString("G6", CultureInfo.InvariantCulture);

                Point3D[] fullCurrentData = (Point3D[])rasterScan_display.DataSource;
                if (fullCurrentData != null)
                {
                    int skipPos = Convert.ToInt32(yVal * hGridPoints);

                    Point3D[] trimmedCurrentData;
                    if ((skipPos + hGridPoints) > fullCurrentData.Length) trimmedCurrentData = fullCurrentData.Skip(skipPos).ToArray();
                    else trimmedCurrentData = fullCurrentData.Skip(skipPos).Take(Convert.ToInt32(hGridPoints)).ToArray();

                    Point[] lineCurrentData = new Point[trimmedCurrentData.Length];
                    for (int i = 0; i < trimmedCurrentData.Length; i++)
                    {
                        lineCurrentData[i] = new Point(trimmedCurrentData[i].X, trimmedCurrentData[i].Z);
                    }

                    if (scan_cursor.Value.Count > 2)
                    {
                        double cursorXPos = (double)scan_cursor.Value[0];
                        double cursorZPos = (double)scan_cursor.Value[2];
                        Point[] verticalLine = new Point[] { new Point(cursorXPos, cursorZPos)};
                        Point[][] displayDataSource = new Point[2][];
                        displayDataSource[0] = lineCurrentData; displayDataSource[1] = verticalLine;
                        rasterScan_lineDisplay.DataSource = displayDataSource;
                    }
                    else rasterScan_lineDisplay.DataSource = lineCurrentData;
                }
            }
        }

        private void set_galvos_from_scan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double valueX = Convert.ToDouble(galvo_x_scan_pos.Text);
                double valueY = Convert.ToDouble(galvo_y_scan_pos.Text);

                GalvoPairPlugin.GetController().AcquisitionStarting();

                GalvoPairPlugin.GetController().SetGalvoXSetpoint(valueX);
                GalvoPairPlugin.GetController().SetGalvoYSetpoint(valueY);

                double xvalue = GalvoPairPlugin.GetController().GetGalvoXSetpoint();
                double yvalue = GalvoPairPlugin.GetController().GetGalvoYSetpoint();
                galvo_X_display.Text = string.Format("{0:0.00000}", xvalue);
                galvo_Y_display.Text = string.Format("{0:0.00000}", yvalue);

                GalvoPairPlugin.GetController().AcquisitionFinished();
            }
            catch (DaqException e1)
            {
                MessageBox.Show("Caught exception: " + e1.Message);
                if (GalvoPairPlugin.GetController().IsRunning()) GalvoPairPlugin.GetController().AcquisitionFinished();
            }
            catch (FormatException e2)
            {
                MessageBox.Show("Caught exception: " + e2.Message);
                if (GalvoPairPlugin.GetController().IsRunning()) GalvoPairPlugin.GetController().AcquisitionFinished();
            }
        }

        private void output_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (output_type_box.SelectedIndex)
            {
                case 0:
                    if (output_box.SelectedIndex >= 0)
                    {
                        MultiChannelData data = FastMultiChannelRasterScan.GetController().dataOutputHistory;
                        if (data != null)
                        {
                            Point3D[] dataSpecific = data.GetCounterData(output_box.SelectedIndex);
                            if (dataSpecific != null)
                            {
                                rasterScan_display.DataSource = dataSpecific;
                                double range_low = rasterScan_display.ColorScale.Range.Minimum;
                                raster_plot_range_low.Text = range_low.ToString("G6", CultureInfo.InvariantCulture);
                                double range_high = rasterScan_display.ColorScale.Range.Maximum;
                                raster_plot_range_high.Text = range_high.ToString("G6", CultureInfo.InvariantCulture);
                            }
                            else this.rasterScan_display.DataSource = null;
                        }
                    }
                    else this.rasterScan_display.DataSource = null;
                    break;

                case 1:
                    if (output_box.SelectedIndex >= 0)
                    {
                        MultiChannelData data = FastMultiChannelRasterScan.GetController().dataOutputHistory;
                        if (data != null)
                        {
                            Point3D[] dataSpecific = data.GetAnalogueData(output_box.SelectedIndex);
                            if (dataSpecific != null)
                            {
                                rasterScan_display.DataSource = dataSpecific;
                                double range_low = rasterScan_display.ColorScale.Range.Minimum;
                                raster_plot_range_low.Text = range_low.ToString("G6", CultureInfo.InvariantCulture);
                                double range_high = rasterScan_display.ColorScale.Range.Maximum;
                                raster_plot_range_high.Text = range_high.ToString("G6", CultureInfo.InvariantCulture);
                            }
                            else this.rasterScan_display.DataSource = null;
                        }
                    }
                    else this.rasterScan_display.DataSource = null;
                    break;

                default:
                    this.rasterScan_display.DataSource = null;
                    break;
            }
        }

        private void setStartScan_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                scan_x_min_set.Value = Convert.ToDouble(galvo_x_scan_pos.Text);
                scan_y_min_set.Value = Convert.ToDouble(galvo_y_scan_pos.Text);
            }
            catch (FormatException e2) { MessageBox.Show("Caught exception: " + e2.Message); }
        }

        private void setStopScan_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                scan_x_max_set.Value = Convert.ToDouble(galvo_x_scan_pos.Text);
                scan_y_max_set.Value = Convert.ToDouble(galvo_y_scan_pos.Text);
            }
            catch (FormatException e2) { MessageBox.Show("Caught exception: " + e2.Message); }
        }

        #endregion

        #region Optimization events

        private int cursorIndexFromGalvoPosition(double galvXPos, double galvYPos)
        {
            double hGridPoints = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXRes"];
            double vGridPoints = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoYRes"];
            double hStart = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXStart"];
            double vStart = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoYStart"];
            double hRange = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXEnd"] - hStart;
            double vRange = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoYEnd"] - vStart;
            double hres = hRange / hGridPoints;
            double vres = vRange / vGridPoints;

            int xVal = Convert.ToInt32((galvXPos - hStart) / hres);
            int yVal = Convert.ToInt32((galvYPos - vStart) / vres);

            if (xVal < 0 || xVal >= hGridPoints) return -1;
            else return yVal * Convert.ToInt32(hGridPoints) + xVal;
        }

        private void opt_setLines(Point3D[] ptns)
        {
            double xLast = ptns[ptns.Length - 1].X;
            double yLast = ptns[ptns.Length - 1].Y;
            double zLast = ptns[ptns.Length - 1].Z;
            Point[] optHist = new Point[ptns.Length];
            for (int i = 0; i < ptns.Length; i++)
			{
                optHist[i] = new Point(i, ptns[i].Z);
			}

            int cursorIndex = cursorIndexFromGalvoPosition(xLast, yLast);

            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    this.rasterScan_lineDisplay.DataSource = optHist;
                    this.cursor_signal_value.Text = zLast.ToString("G6", CultureInfo.InvariantCulture);
                    this.galvo_x_scan_pos.Text = xLast.ToString("G6", CultureInfo.InvariantCulture);
                    this.galvo_y_scan_pos.Text = yLast.ToString("G6", CultureInfo.InvariantCulture);

                    if (cursorIndex >= 0 && cursorIndex < ((double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXRes"] * (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoYRes"]))
                    {
                        this.scan_cursor.Index = cursorIndex;
                    }
                }
            ));
        }

        private void opt_End()
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    this.optimize_Button.Content = "Optimize";
                    this.rasterScan_switch.Value = false;
                    this.oscilloscope_switch.IsReadOnly = false;
                    this.exposure_set.IsEnabled = true;
                    this.galvo_setpoint_reader.IsEnabled = true;
                    this.galvosSet_Button.IsEnabled = true;
                    this.galvo_X_set.IsReadOnly = false;
                    this.galvo_Y_set.IsReadOnly = false;
                    this.scan_x_min_set.IsEnabled = true;
                    this.scan_x_max_set.IsEnabled = true;
                    this.scan_x_res_set.IsEnabled = true;
                    this.scan_y_min_set.IsEnabled = true;
                    this.scan_y_max_set.IsEnabled = true;
                    this.scan_y_res_set.IsEnabled = true;
                    this.set_galvos_from_scan.IsEnabled = true;
                    this.hardware_MenuItem.IsEnabled = true;
                    this.rasterScan_hardware_MenuItem.IsEnabled = true;
                    this.loadSettings_MenuItem.IsEnabled = true;
                    this.rasterScan_switch.IsEnabled = true;
                    this.up_Button.IsEnabled = true;
                    this.down_Button.IsEnabled = true;
                    this.right_Button.IsEnabled = true;
                    this.left_Button.IsEnabled = true;
                }
            ));
        }

        private void opt_problemHandler(DaqException e)
        {
            MessageBox.Show("Caught exception: " + e.Message);
            if (CounterOptimizationPlugin.GetController().IsRunning()) CounterOptimizationPlugin.GetController().StopOptimizing();

            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    MessageBox.Show(e.Message);
                    this.optimize_Button.Content = "Optimize";
                    this.rasterScan_switch.Value = false;
                    this.oscilloscope_switch.IsReadOnly = false;
                    this.exposure_set.IsEnabled = true;
                    this.galvo_setpoint_reader.IsEnabled = true;
                    this.galvosSet_Button.IsEnabled = true;
                    this.galvo_X_set.IsReadOnly = false;
                    this.galvo_Y_set.IsReadOnly = false;
                    this.scan_x_min_set.IsEnabled = true;
                    this.scan_x_max_set.IsEnabled = true;
                    this.scan_x_res_set.IsEnabled = true;
                    this.scan_y_min_set.IsEnabled = true;
                    this.scan_y_max_set.IsEnabled = true;
                    this.scan_y_res_set.IsEnabled = true;
                    this.set_galvos_from_scan.IsEnabled = true;
                    this.hardware_MenuItem.IsEnabled = true;
                    this.rasterScan_hardware_MenuItem.IsEnabled = true;
                    this.loadSettings_MenuItem.IsEnabled = true;
                    this.rasterScan_switch.IsEnabled = true;
                    this.up_Button.IsEnabled = true;
                    this.down_Button.IsEnabled = true;
                    this.right_Button.IsEnabled = true;
                    this.left_Button.IsEnabled = true;
                }
            ));
        }

        private void optimize_Button_Click(object sender, RoutedEventArgs e)
        {
            if (output_type_box.SelectedIndex == 0 && output_box.SelectedIndex >= 0 && scan_cursor.Value.Count > 2 && (string)optimize_Button.Content == "Optimize")
            {
                Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() =>
                    {
                        this.rasterScan_switch.Value = true;
                        this.oscilloscope_switch.IsReadOnly = true;
                        this.exposure_set.IsEnabled = false;
                        this.galvo_setpoint_reader.IsEnabled = false;
                        this.galvosSet_Button.IsEnabled = false;
                        this.galvo_X_set.IsReadOnly = true;
                        this.galvo_Y_set.IsReadOnly = true;
                        this.scan_x_min_set.IsEnabled = false;
                        this.scan_x_max_set.IsEnabled = false;
                        this.scan_x_res_set.IsEnabled = false;
                        this.scan_y_min_set.IsEnabled = false;
                        this.scan_y_max_set.IsEnabled = false;
                        this.scan_y_res_set.IsEnabled = false;
                        this.set_galvos_from_scan.IsEnabled = false;
                        this.hardware_MenuItem.IsEnabled = false;
                        this.rasterScan_hardware_MenuItem.IsEnabled = false;
                        this.loadSettings_MenuItem.IsEnabled = false;
                        this.rasterScan_switch.IsEnabled = false;
                        this.up_Button.IsEnabled = false;
                        this.down_Button.IsEnabled = false;
                        this.right_Button.IsEnabled = false;
                        this.left_Button.IsEnabled = false;
                    }
                ));

                double xVal = Convert.ToDouble(scan_cursor.Value[0]) - 1;
                double yVal = Convert.ToDouble(scan_cursor.Value[1]) - 1;
                double zVal = (double)scan_cursor.Value[2];

                double hGridPoints = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXRes"];
                double vGridPoints = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoYRes"];
                double hStart = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXStart"];
                double vStart = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoYStart"];
                double hRange = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXEnd"] - hStart;
                double vRange = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoYEnd"] - vStart;
                double hres = hRange / hGridPoints;
                double vres = vRange / vGridPoints; 

                CounterOptimizationPlugin.GetController().OptimizationStarting((string)output_box.SelectedValue);

                Thread thread;
                thread = new Thread(() => CounterOptimizationPlugin.GetController().FindOptimum(hStart + hres * xVal, vStart + vres * yVal, zVal));
                thread.IsBackground = true;
                thread.Start();

                optimize_Button.Content = "Stop";
            }

            else if ((string)optimize_Button.Content == "Stop") CounterOptimizationPlugin.GetController().StopOptimizing();
        }

        #endregion

        #region Menu events

        private void save_settings_Click(object sender, RoutedEventArgs e)
        {
            settings.Save();
            GalvoPairPlugin.GetController().Settings.Save();
            TimeTracePlugin.GetController().Settings.Save();
            FastMultiChannelRasterScan.GetController().scanSettings.Save();
        }

        private void load_settings_Click(object sender, RoutedEventArgs e)
        {
            settings = PluginSaveLoad.LoadSettings("mainWindow");
            fileName_set.Text = (string)settings["saveFile"];

            GalvoPairPlugin.GetController().LoadSettings();
            galvo_X_set.Text = (string)GalvoPairPlugin.GetController().Settings["GalvoXInit"];
            galvo_Y_set.Text = (string)GalvoPairPlugin.GetController().Settings["GalvoYInit"];

            TimeTracePlugin.GetController().LoadSettings();
            exposure_set.Value = TimeTracePlugin.GetController().GetExposure();
            int val = TimeTracePlugin.GetController().GetBufferSize();
            buffer_size_set.Value = val;
            binNumber_set.Value = (int)TimeTracePlugin.GetController().Settings["binNumber"];

            FastMultiChannelRasterScan.GetController().LoadSettings();
            scan_x_min_set.Value = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXStart"];
            scan_x_max_set.Value = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXEnd"];
            scan_x_res_set.Value = Convert.ToInt32((double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXRes"]);
            scan_y_min_set.Value = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoYStart"];
            scan_y_max_set.Value = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoYEnd"];
            scan_y_res_set.Value = Convert.ToInt32((double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoYRes"]);
        }

        private void open_HardwareConfigure(object sender, RoutedEventArgs e)
        {
            HardwareConfigure window = new HardwareConfigure();
            window.ShowDialog();
        }

        private void open_TimeTraceHardwareConfigure(object sender, RoutedEventArgs e)
        {
            TimeTraceHardwareConfigure window = new TimeTraceHardwareConfigure();
            window.ShowDialog();
        }

        private void rasterScan_hardware_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            RasterScanHardwareConfigure window = new RasterScanHardwareConfigure();
            window.ShowDialog();
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void fileName_set_TextChanged(object sender, TextChangedEventArgs e)
        {
            settings["saveFile"] = fileName_set.Text;
        }

        private void solstis_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            LaserControl.GetWindow().Show();
            LaserControl.GetWindow().Activate();
        }

        private void DFG_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DFGControl.GetWindow().Show();
            DFGControl.GetWindow().Activate();
        }

        #endregion 

        #region Save events

        private void save_raster_scan_Click(object sender, RoutedEventArgs e)
        {
            if (this.rasterScan_display.DataSource != null)
            {
                FastMultiChannelRasterScan.GetController().SaveData(DateTime.Today.ToString("yy-MM-dd") + "_" + DateTime.Now.ToString("HH-mm-ss") + "_rasterScan_" + (string)settings["saveFile"] + ".txt");
            }
        }

        private void save_timetrace_Click(object sender, RoutedEventArgs e)
        {
            if (this.APD_monitor.DataSource != null)
            {
                TimeTracePlugin.GetController().SaveData(DateTime.Today.ToString("yy-MM-dd") + "_" + DateTime.Now.ToString("HH-mm-ss") + "_timeTrace_" + (string)settings["saveFile"] + ".txt");
            }
        }

        #endregion

        #region Closing event

        private void main_window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (CounterOptimizationPlugin.GetController().IsRunning())
            {
                CounterOptimizationPlugin.GetController().StopOptimizing();
                Thread.Sleep(1000);
            }

            if (FastMultiChannelRasterScan.GetController().IsRunning())
            {
                FastMultiChannelRasterScan.GetController().StopScan();
                Thread.Sleep(1000);
            }

            if (TimeTracePlugin.GetController().IsRunning())
            {
                TimeTracePlugin.GetController().StopContinuousAcquisition();
                Thread.Sleep(1000);
            }

            if (GalvoPairPlugin.GetController().IsRunning())
            {
                TimeTracePlugin.GetController().AcquisitionFinished();
                Thread.Sleep(1000);
            }
        }

        #endregion 

    }
}
