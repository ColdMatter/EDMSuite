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

namespace ConfocalControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Initialization

        public MainWindow()
        {
            InitializeComponent();

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
            }
            finally
            {
                exposure_set.Value = SingleCounterPlugin.GetController().GetExposure();
                int val = SingleCounterPlugin.GetController().GetBufferSize();
                buffer_size_set.Value = val;

                SingleCounterPlugin.GetController().setTextBox += singleCounter_setTextBox;
                SingleCounterPlugin.GetController().setWaveForm += singleCounter_setWaveForm;
                SingleCounterPlugin.GetController().DaqProblem += singleCounter_problemHandler;

                ConfocalRasterScan.GetController().Data += rasterScan_setArray;
                ConfocalRasterScan.GetController().LineFinished += rasterScan_setLine;
                ConfocalRasterScan.GetController().ScanFinished += rasterScan_End;
                ConfocalRasterScan.GetController().DaqProblem += rasterScan_problemHandler;

                scan_x_min_set.Value = (double)ConfocalRasterScan.GetController().scanSettings["GalvoXStart"];
                scan_x_max_set.Value = (double)ConfocalRasterScan.GetController().scanSettings["GalvoXEnd"];
                scan_x_res_set.Value = Convert.ToInt32((double)ConfocalRasterScan.GetController().scanSettings["GalvoXRes"]);
                scan_y_min_set.Value = (double)ConfocalRasterScan.GetController().scanSettings["GalvoYStart"];
                scan_y_max_set.Value = (double)ConfocalRasterScan.GetController().scanSettings["GalvoYEnd"];
                scan_y_res_set.Value = Convert.ToInt32((double)ConfocalRasterScan.GetController().scanSettings["GalvoYRes"]);

                set_galvos_from_scan.IsEnabled = false;
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
                if (SingleCounterPlugin.GetController().IsRunning()) SingleCounterPlugin.GetController().AcquisitionFinished();
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

        public void galvo_X_set_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
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
                            if (SingleCounterPlugin.GetController().IsRunning()) SingleCounterPlugin.GetController().AcquisitionFinished();
                        }
                        catch (DaqException e1)
                        {
                            MessageBox.Show("Caught exception: " + e1.Message);
                            if (GalvoPairPlugin.GetController().IsRunning()) GalvoPairPlugin.GetController().AcquisitionFinished();
                            if (SingleCounterPlugin.GetController().IsRunning()) SingleCounterPlugin.GetController().AcquisitionFinished();
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
                            if (SingleCounterPlugin.GetController().IsRunning()) SingleCounterPlugin.GetController().AcquisitionFinished();
                        }
                        catch (DaqException e1)
                        {
                            MessageBox.Show("Caught exception: " + e1.Message);
                            if (GalvoPairPlugin.GetController().IsRunning()) GalvoPairPlugin.GetController().AcquisitionFinished();
                            if (SingleCounterPlugin.GetController().IsRunning()) SingleCounterPlugin.GetController().AcquisitionFinished();
                        }
                    }
                }
            }
        }

        public void galvo_Y_set_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
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
                            if (SingleCounterPlugin.GetController().IsRunning()) SingleCounterPlugin.GetController().AcquisitionFinished();
                        }
                        catch (DaqException e1)
                        {
                            MessageBox.Show("Caught exception: " + e1.Message);
                            if (GalvoPairPlugin.GetController().IsRunning()) GalvoPairPlugin.GetController().AcquisitionFinished();
                            if (SingleCounterPlugin.GetController().IsRunning()) SingleCounterPlugin.GetController().AcquisitionFinished();
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
                            if (SingleCounterPlugin.GetController().IsRunning()) SingleCounterPlugin.GetController().AcquisitionFinished();
                        }
                        catch (DaqException e1)
                        {
                            MessageBox.Show("Caught exception: " + e1.Message);
                            if (GalvoPairPlugin.GetController().IsRunning()) GalvoPairPlugin.GetController().AcquisitionFinished();
                            if (SingleCounterPlugin.GetController().IsRunning()) SingleCounterPlugin.GetController().AcquisitionFinished();
                        }
                    }
                }
            }
        }

        #endregion

        #region Timetrace events

        public void singleCounter_setTextBox(int value)
        {
            // If on a different thread 
            if (Application.Current.Dispatcher.CheckAccess())
            {
                single_photon_counts.Text = value.ToString();
            }

            else
            {
                Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() =>
                        {
                            this.single_photon_counts.Text = value.ToString();
                        }
                ));
            }
        }

        public void singleCounter_setWaveForm(int[] values, Point[] histData)
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
            if (GalvoPairPlugin.GetController().IsRunning()) GalvoPairPlugin.GetController().AcquisitionFinished();
            if (SingleCounterPlugin.GetController().IsRunning()) SingleCounterPlugin.GetController().AcquisitionFinished();

            Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.oscilloscope_switch.Value = false;
                           this.exposure_set.IsEnabled = true;
                           this.rasterScan_switch.IsReadOnly = false;
                           this.hardware_MenuItem.IsEnabled = true;
                       }
                   ));

            single_photon_counts.Text = "Stopped";
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
                           this.hardware_MenuItem.IsEnabled = false;
                       }
                   ));

                Thread thread = new Thread(new ThreadStart(SingleCounterPlugin.GetController().ContinuousAcquisition));
                thread.IsBackground = true;
                thread.Start();

            }
            else
            {
                SingleCounterPlugin.GetController().StopContinuousAcquisition();
                Thread.Sleep(1000);
                Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.oscilloscope_switch.Value = false;
                           this.exposure_set.IsEnabled = true;
                           this.rasterScan_switch.IsReadOnly = false;
                           this.hardware_MenuItem.IsEnabled = true;
                       }
                   ));

                single_photon_counts.Text = "Stopped";
            }
        }

        private void exposure_set_ValueChanged(object sender, ValueChangedEventArgs<double> e)
        {
            if (e.NewValue > 0) SingleCounterPlugin.GetController().UpdateExposure(e.NewValue);
            else exposure_set.Value = e.OldValue;
        }

        private void buffer_size_set_ValueChanged(object sender, ValueChangedEventArgs<int> e)
        {
            if (e.NewValue > 0)
            {
                SingleCounterPlugin.GetController().Settings["bufferSize"] = e.NewValue;
            }
            else buffer_size_set.Value = e.OldValue;
        }

        #endregion

        #region RasterScan events

        public void rasterScan_setArray(Point3D[] pnts)
        {
            // If on a different thread 
            if (Application.Current.Dispatcher.CheckAccess())
            {
                rasterScan_display.DataSource = pnts;
            }

            else
            {
                Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() =>
                    {
                        this.rasterScan_display.DataSource = pnts;
                    }
                ));
            }
        }

        public void rasterScan_setLine(Point[] pnts)
        {
            // If on a different thread 
            if (Application.Current.Dispatcher.CheckAccess())
            {
                rasterScan_lineDisplay.DataSource = pnts;
            }

            else
            {
                Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() =>
                    {
                        this.rasterScan_lineDisplay.DataSource = pnts;
                    }
                ));
            }
        }

        public void rasterScan_problemHandler(DaqException e)
        {
            MessageBox.Show("Caught exception: " + e.Message);
            if (GalvoPairPlugin.GetController().IsRunning()) GalvoPairPlugin.GetController().AcquisitionFinished();
            if (SingleCounterPlugin.GetController().IsRunning()) SingleCounterPlugin.GetController().AcquisitionFinished();

            Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.rasterScan_switch.Value = false;
                           this.oscilloscope_switch.IsReadOnly = false;
                           this.exposure_set.IsEnabled = true;
                           this.galvo_setpoint_reader.IsEnabled = true;
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
                       }
                   ));
        }

        public void rasterScan_Click(object sender, RoutedEventArgs e)
        {
            if (!rasterScan_switch.Value)
            {
                Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.rasterScan_switch.Value = true;
                           this.oscilloscope_switch.IsReadOnly = true;
                           this.exposure_set.IsEnabled = false;
                           this.galvo_setpoint_reader.IsEnabled = false;
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

                           double hRange = (double)ConfocalRasterScan.GetController().scanSettings["GalvoXEnd"] - (double)ConfocalRasterScan.GetController().scanSettings["GalvoXStart"];
                           double vRange = (double)ConfocalRasterScan.GetController().scanSettings["GalvoYEnd"] - (double)ConfocalRasterScan.GetController().scanSettings["GalvoYStart"];
                           
                           double axMin = 0;
                           double axHorizontalMax;
                           double axVerticalMax;

                           if(hRange > vRange)
                           {
                               double scale = hRange / vRange;
                               axHorizontalMax = (double)ConfocalRasterScan.GetController().scanSettings["GalvoXRes"] + 1;
                               axVerticalMax = scale * ((double)ConfocalRasterScan.GetController().scanSettings["GalvoYRes"] + 1);
                           }
                           else
                           {
                               double scale = vRange / hRange;
                               axHorizontalMax = scale * ((double)ConfocalRasterScan.GetController().scanSettings["GalvoXRes"] + 1);
                               axVerticalMax = (double)ConfocalRasterScan.GetController().scanSettings["GalvoYRes"] + 1;
                           }

                           AxisDouble axHorizontal = new AxisDouble { Orientation = Orientation.Horizontal, Range = Range.Create(axMin, axHorizontalMax), Adjuster = null };
                           this.rasterScan_display.HorizontalAxis = axHorizontal;
                           AxisDouble axVertical = new AxisDouble { Orientation = Orientation.Vertical, Range = Range.Create(axMin, axVerticalMax), Adjuster = null };
                           this.rasterScan_display.VerticalAxis = axVertical;
                       }
                   ));

                Thread thread = new Thread(new ThreadStart(ConfocalRasterScan.GetController().StartScan));
                thread.IsBackground = true;
                thread.Start();
            }
            else
            {
                ConfocalRasterScan.GetController().StopScan();
                Thread.Sleep(1000);

                Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.rasterScan_switch.Value = false;
                           this.oscilloscope_switch.IsReadOnly = false;
                           this.exposure_set.IsEnabled = true;
                           this.galvo_setpoint_reader.IsEnabled = true;
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

                           if ((bool)this.save_automatic.IsChecked)
                           {
                               ConfocalRasterScan.GetController().SaveDataAutomatic();
                           }
                       }
                   ));
            }
        }

        public void rasterScan_End()
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

                           if ((bool)this.save_automatic.IsChecked)
                           {
                               ConfocalRasterScan.GetController().SaveDataAutomatic();
                           }
                       }
                   ));
        }

        private void scan_x_min_set_ValueChanged(object sender, ValueChangedEventArgs<double> e)
        {
            ConfocalRasterScan.GetController().scanSettings["GalvoXStart"] = e.NewValue;
        }

        private void scan_x_max_set_ValueChanged(object sender, ValueChangedEventArgs<double> e)
        {
            ConfocalRasterScan.GetController().scanSettings["GalvoXEnd"] = e.NewValue;
        }

        private void scan_x_res_set_ValueChanged(object sender, ValueChangedEventArgs<int> e)
        {
            ConfocalRasterScan.GetController().scanSettings["GalvoXRes"] = (double)e.NewValue;
        }

        private void scan_y_min_set_ValueChanged(object sender, ValueChangedEventArgs<double> e)
        {
            ConfocalRasterScan.GetController().scanSettings["GalvoYStart"] = e.NewValue;
        }

        private void scan_y_max_set_ValueChanged(object sender, ValueChangedEventArgs<double> e)
        {
            ConfocalRasterScan.GetController().scanSettings["GalvoYEnd"] = e.NewValue;
        }

        private void scan_y_res_set_ValueChanged(object sender, ValueChangedEventArgs<int> e)
        {
            ConfocalRasterScan.GetController().scanSettings["GalvoYRes"] = (double)e.NewValue;
        }

        private void cursor_PositionChanged(object sender, EventArgs e)
        {
            Point pnt = scan_cursor.GetRelativePosition();
            double xVal = pnt.X;
            double yVal = pnt.Y;

            double hStart = (double)ConfocalRasterScan.GetController().scanSettings["GalvoXStart"];
            double vStart = (double)ConfocalRasterScan.GetController().scanSettings["GalvoYStart"];
            double hRange = (double)ConfocalRasterScan.GetController().scanSettings["GalvoXEnd"] - hStart;
            double vRange = (double)ConfocalRasterScan.GetController().scanSettings["GalvoYEnd"] - vStart;

            if (galvo_x_scan_pos != null && galvo_y_scan_pos != null)
            {
                galvo_x_scan_pos.Text = string.Format("{0:0.000}", hStart + hRange * xVal);
                galvo_y_scan_pos.Text = string.Format("{0:0.000}", vStart + vRange * yVal);
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
                if (SingleCounterPlugin.GetController().IsRunning()) SingleCounterPlugin.GetController().AcquisitionFinished();
            }
        }

        #endregion

        #region Menu events

        private void save_settings_Click(object sender, RoutedEventArgs e)
        {
            GalvoPairPlugin.GetController().Settings.SaveSettings();
            SingleCounterPlugin.GetController().Settings.SaveSettings();
            ConfocalRasterScan.GetController().scanSettings.SaveSettings();
        }

        private void load_settings_Click(object sender, RoutedEventArgs e)
        {
            GalvoPairPlugin.GetController().Settings.LoadSettings();
            galvo_X_set.Text = (string)GalvoPairPlugin.GetController().Settings["GalvoXInit"];
            galvo_Y_set.Text = (string)GalvoPairPlugin.GetController().Settings["GalvoYInit"];

            SingleCounterPlugin.GetController().Settings.LoadSettings();
            exposure_set.Value = SingleCounterPlugin.GetController().GetExposure();
            int val = SingleCounterPlugin.GetController().GetBufferSize();
            buffer_size_set.Value = val;

            ConfocalRasterScan.GetController().scanSettings.LoadSettings();
            scan_x_min_set.Value = (double)ConfocalRasterScan.GetController().scanSettings["GalvoXStart"];
            scan_x_max_set.Value = (double)ConfocalRasterScan.GetController().scanSettings["GalvoXEnd"];
            scan_x_res_set.Value = Convert.ToInt32((double)ConfocalRasterScan.GetController().scanSettings["GalvoXRes"]);
            scan_y_min_set.Value = (double)ConfocalRasterScan.GetController().scanSettings["GalvoYStart"];
            scan_y_max_set.Value = (double)ConfocalRasterScan.GetController().scanSettings["GalvoYEnd"];
            scan_y_res_set.Value = Convert.ToInt32((double)ConfocalRasterScan.GetController().scanSettings["GalvoYRes"]);
        }

        private void open_HardwareConfigure(object sender, RoutedEventArgs e)
        {
            HardwareConfigure win = new HardwareConfigure();
            win.Show();
        }

        private void open_FileNameConfigure(object sender, RoutedEventArgs e)
        {
            FileNameConfigure win = new FileNameConfigure();
            win.Show();
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion 

        #region Save events

        private void save_raster_scan_Click(object sender, RoutedEventArgs e)
        {
            ConfocalRasterScan.GetController().SaveData();
        }

        private void save_timetrace_Click(object sender, RoutedEventArgs e)
        {
            SingleCounterPlugin.GetController().SaveData();
        }

        private void save_hist_Click(object sender, RoutedEventArgs e)
        {
            SingleCounterPlugin.GetController().SaveHistogram();
        }

        #endregion
    }
}
