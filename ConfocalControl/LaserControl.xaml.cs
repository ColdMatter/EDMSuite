using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ConfocalControl
{
    /// <summary>
    /// Interaction logic for LaserControl.xaml
    /// </summary>
    public partial class LaserControl : Window
    {
        #region Window members

        // Dependencies should refer to this instance only 
        private static LaserControl windowInstance;
        public static LaserControl GetWindow()
        {
            if (windowInstance == null)
            {
                windowInstance = new LaserControl();
            }
            return windowInstance;
        }

        #endregion

        #region Initialization

        public LaserControl()
        {
            InitializeComponent();

            checkConnection_Button_Click(null, null);

            wavelengthSet_Numeric.Value = (double)SolsTiSPlugin.GetController().Settings["wavelength"];

            SolsTiSPlugin.GetController().WavemeterData += wavemeterScanData;
            SolsTiSPlugin.GetController().WavemeterScanFinished += wavemeterScanFinished;
            SolsTiSPlugin.GetController().WavemeterScanProblem += wavemeterScanProblem;

            SolsTiSPlugin.GetController().ResonatorData += resonatorScanData;
            SolsTiSPlugin.GetController().ResonatorScanFinished += resonatorScanFinished;
            SolsTiSPlugin.GetController().ResonatorScanProblem += resonatorScanProblem;

            output_type_box.Items.Add("Counters");
            output_type_box.Items.Add("Analogues");
            output_type_box.SelectedIndex = 0;
        }

        #endregion

        #region TCP connection events

        private void connect_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!SolsTiSPlugin.GetController().Solstis.Connected)
            {
                try
                {
                    SolsTiSPlugin.GetController().Solstis.Connect();
                    string reply = SolsTiSPlugin.GetController().Solstis.StartLink(0);
                    MessageBox.Show(reply);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
            else
            {
                MessageBox.Show("already connected");
            }

            checkConnection_Button_Click(null, null);
        }

        private void disconnect_Button_Click(object sender, RoutedEventArgs e)
        {
            if (SolsTiSPlugin.GetController().Solstis.Connected) SolsTiSPlugin.GetController().Solstis.Disconnect();
            else MessageBox.Show("already disconnected");

            checkConnection_Button_Click(null, null);
            etalonCheck_Reader.Text = "";
            wavelength_Read.Text = "";
        }

        private void checkConnection_Button_Click(object sender, RoutedEventArgs e)
        {
            if (SolsTiSPlugin.GetController().Solstis.Connected) checkConnection_Reader.Text = "Active";
            else checkConnection_Reader.Text = "Inactive";
        }

        private void ping_Button_Click(object sender, RoutedEventArgs e)
        {
            if (SolsTiSPlugin.GetController().Solstis.Connected)
            {
                string pong = SolsTiSPlugin.GetController().Solstis.PingTest("PONG");
                MessageBox.Show(pong);
            }
            else MessageBox.Show("not connected");
        }

        #endregion

        #region Etalon events

        private void etalonCheck_Button_Click(object sender, RoutedEventArgs e)
        {
            if (SolsTiSPlugin.GetController().Solstis.Connected)
            {
                Dictionary<string, object> reply = SolsTiSPlugin.GetController().Solstis.etalon_lock_status();

                if (reply.Count == 0)
                {
                    MessageBox.Show("empty reply");
                }
                else
                {
                    switch ((int)reply["status"])
                    {
                        case 0:
                            etalonCheck_Reader.Text = (string)reply["condition"];
                            break;

                        case 1:
                            MessageBox.Show("task failed");
                            break;

                        default:
                            MessageBox.Show("did not understand reply");
                            break;
                    }
                }
            }
            else MessageBox.Show("not connected");
        }

        private void etalonLock_Button_Click(object sender, RoutedEventArgs e)
        {
            if (SolsTiSPlugin.GetController().Solstis.Connected)
            {
                int reply = SolsTiSPlugin.GetController().Solstis.etalon_lock(true, true);
                
                switch (reply)
                {
                    case -1:
                        MessageBox.Show("empty reply");
                        break;

                    case 0:
                        MessageBox.Show("task completed");
                        break;

                    case 1:
                        MessageBox.Show("task failed");
                        break;

                    default:
                        MessageBox.Show("did not understand reply");
                        break;
                }

                etalonCheck_Button_Click(null, null);
            }
            else MessageBox.Show("not connected");
        }

        private void etalonUnLock_Button_Click(object sender, RoutedEventArgs e)
        {
            if (SolsTiSPlugin.GetController().Solstis.Connected)
            {
                int reply = SolsTiSPlugin.GetController().Solstis.etalon_lock(false, true);

                switch (reply)
                {
                    case -1:
                        MessageBox.Show("empty reply");
                        break;

                    case 0:
                        MessageBox.Show("task completed");
                        break;

                    case 1:
                        MessageBox.Show("task failed");
                        break;

                    default:
                        MessageBox.Show("did not understand reply");
                        break;
                }

                etalonCheck_Button_Click(null, null);
            }
            else MessageBox.Show("not connected");
        }

        #endregion

        #region Wavelength events

        private void wavelengthSet_Numeric_ValueChanged(object sender, NationalInstruments.Controls.ValueChangedEventArgs<double> e)
        {
            SolsTiSPlugin.GetController().Settings["wavelength"] = e.NewValue;
        }

        private void wavelengthRead_Button_Click(object sender, RoutedEventArgs e)
        {
            if (SolsTiSPlugin.GetController().Solstis.Connected)
            {
                Dictionary<string, object> reply = SolsTiSPlugin.GetController().Solstis.poll_move_wave_t();

                if (reply.Count == 0)
                {
                    MessageBox.Show("empty reply");
                }
                else
                {
                    switch ((int)reply["status"])
                    {
                        case 0:
                            wavelength_Read.Text = (Convert.ToDouble(reply["current_wavelength"])).ToString();
                            break;

                        case 1:
                            MessageBox.Show("tuning in progress");
                            wavelength_Read.Text = (Convert.ToDouble(reply["current_wavelength"])).ToString();
                            break;

                        case 2:
                            MessageBox.Show("tuning operation failed");
                            break;

                        default:
                            MessageBox.Show("did not understand reply");
                            break;
                    }
                }
            }
            else MessageBox.Show("not connected");
        }

        //private void wavelengthRead_Button_Click(object sender, RoutedEventArgs e)
        //{
        //    if (SolsTiSPlugin.GetController().Solstis.Connected)
        //    {
        //        Dictionary<string, object> reply = SolsTiSPlugin.GetController().Solstis.poll_wave_m();

        //        if (reply.Count == 0)
        //        {
        //            MessageBox.Show("empty reply");
        //        }
        //        else
        //        {
        //            switch ((int)reply["status"])
        //            {
        //                case 0:
        //                    MessageBox.Show("tuning software not active");
        //                    break;

        //                case 1:
        //                    MessageBox.Show("no link to wavelength meter or no meter configured");
        //                    break;

        //                case 2:
        //                    MessageBox.Show("tuning in progress");
        //                    wavelength_Read.Text = (Convert.ToDouble(reply["current_wavelength"])).ToString();
        //                    break;

        //                case 3:
        //                    wavelength_Read.Text = (Convert.ToDouble(reply["current_wavelength"])).ToString();
        //                    break;

        //                default:
        //                    MessageBox.Show("did not understand reply");
        //                    break;
        //            }
        //        }
        //    }
        //    else MessageBox.Show("not connected");
        //}

        private void wavelengthSet_Button_Click(object sender, RoutedEventArgs e)
        {
            if (SolsTiSPlugin.GetController().Solstis.Connected)
            {
                double wavelength = (double)SolsTiSPlugin.GetController().Settings["wavelength"];
                int reply = SolsTiSPlugin.GetController().Solstis.move_wave_t(wavelength, true);
                switch (reply)
                {
                    case -1:
                        MessageBox.Show("empty reply");
                        break;

                    case 0:
                        MessageBox.Show("task completed");
                        break;

                    case 1:
                        MessageBox.Show("task failed");
                        break;

                    default:
                        MessageBox.Show("did not understand reply");
                        break;
                }

                wavelengthRead_Button_Click(null, null);
            }
            else MessageBox.Show("not connected");
        }

        //private void wavelengthSet_Button_Click(object sender, RoutedEventArgs e)
        //{
        //    if (SolsTiSPlugin.GetController().Solstis.Connected)
        //    {
        //        Dictionary<string, object> reply = SolsTiSPlugin.GetController().Solstis.poll_wave_m();

        //        if (reply.Count == 0)
        //        {
        //            MessageBox.Show("empty reply");
        //        }
        //        else
        //        {
        //            switch ((int)reply["status"])
        //            {
        //                case 0:
        //                    MessageBox.Show("tuning software not active");
        //                    break;

        //                case 1:
        //                    MessageBox.Show("no link to wavelength meter or no meter configured");
        //                    break;

        //                case 2:
        //                case 3:
        //                    double wavelength = (double)SolsTiSPlugin.GetController().Settings["wavelength"];
        //                    int set_reply = SolsTiSPlugin.GetController().Solstis.set_wave_m(wavelength, true);

        //                    switch (set_reply)
        //                    {
        //                        case -1:
        //                            MessageBox.Show("empty reply");
        //                            break;

        //                        case 0:
        //                            MessageBox.Show("task completed");
        //                            break;

        //                        case 1:
        //                            MessageBox.Show("task failed");
        //                            break;

        //                        default:
        //                            MessageBox.Show("did not understand reply");
        //                            break;
        //                    }

        //                    wavelengthRead_Button_Click(null, null);
        //                    break;

        //                default:
        //                    MessageBox.Show("did not understand reply");
        //                    break;
        //            }
        //        }
        //    }
        //    else MessageBox.Show("not connected");
        //}

        #endregion

        #region Wavemeter Scan events

        private void wavemeterScanStart_Set_ValueChanged(object sender, NationalInstruments.Controls.ValueChangedEventArgs<double> e)
        {
            if (e.NewValue < 700 || e.NewValue > 1000) wavemeterScanStart_Set.Value = e.OldValue;
            else SolsTiSPlugin.GetController().Settings["wavemeterScanStart"] = e.NewValue;
        }

        private void wavemeterScanStop_Set_ValueChanged(object sender, NationalInstruments.Controls.ValueChangedEventArgs<double> e)
        {
            if (e.NewValue < 700 || e.NewValue > 1000) wavemeterScanStop_Set.Value = e.OldValue;
            else SolsTiSPlugin.GetController().Settings["wavemeterScanStop"] = e.NewValue;
        }

        private void wavemeterScanRes_Set_ValueChanged(object sender, NationalInstruments.Controls.ValueChangedEventArgs<double> e)
        {
            if (e.NewValue <= 0) wavemeterScanRes_Set.Value = e.OldValue;
            else SolsTiSPlugin.GetController().Settings["wavemeterScanPoints"] = Convert.ToInt32(e.NewValue);
        }

        private void wavemeterScanProblem(Exception e)
        {
            MessageBox.Show(e.Message);
            if (SolsTiSPlugin.GetController().IsRunning())
            {
                SolsTiSPlugin.GetController().WavemeterAcquisitionFinishing();
            }
            Application.Current.Dispatcher.BeginInvoke(
                   DispatcherPriority.Background,
                   new Action(() =>
                   {
                       EnableNoneScanCommands();
                       this.wavemeterScan_Switch.Value = false;
                       this.resonatorScan_Switch.IsEnabled = true;
                       this.wavemeterScanStart_Set.IsEnabled = true;
                       this.wavemeterScanStop_Set.IsEnabled = true;
                       this.wavemeterScanRes_Set.IsEnabled = true;
                   }));
        }

        private void wavemeterScanFinished()
        {
            Application.Current.Dispatcher.BeginInvoke(
                   DispatcherPriority.Background,
                   new Action(() =>
                   {
                       EnableNoneScanCommands();
                       this.wavemeterScan_Switch.Value = false;
                       this.resonatorScan_Switch.IsEnabled = true;
                       this.wavemeterScanStart_Set.IsEnabled = true;
                       this.wavemeterScanStop_Set.IsEnabled = true;
                       this.wavemeterScanRes_Set.IsEnabled = true;
                   }));
        }

        private void wavemeterScanData(Point[] data)
        {
            Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.wavemeterScan_Display.DataSource = data;
                       }));
        }

        private void wavemeterScan_Switch_Click(object sender, RoutedEventArgs e)
        {
            if (!wavemeterScan_Switch.Value)
            {
                if (!SolsTiSPlugin.GetController().WavemeterAcceptableSettings())
                {
                    Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.wavemeterScan_Switch.Value = false;
                       }));
                    return;
                }

                if (!SolsTiSPlugin.GetController().Solstis.Connected)
                {
                    MessageBox.Show("not connected");
                    Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.wavemeterScan_Switch.Value = false;
                       }));
                    return;
                }

                Application.Current.Dispatcher.BeginInvoke(
                   DispatcherPriority.Background,
                   new Action(() => 
                   {
                       DissableNoneScanCommands();
                       this.wavemeterScan_Switch.Value = true;
                       this.resonatorScan_Switch.IsEnabled = false;
                       this.wavemeterScanStart_Set.IsEnabled = false;
                       this.wavemeterScanStop_Set.IsEnabled = false;
                       this.wavemeterScanRes_Set.IsEnabled = false;
                   }));

                Thread thread = new Thread(new ThreadStart(SolsTiSPlugin.GetController().WavemeterSynchronousStartScan));
                thread.IsBackground = true;
                thread.Start();
            }
        }

        #endregion

        #region Resonator Scan events

        private void resonatorScanStart_Set_ValueChanged(object sender, NationalInstruments.Controls.ValueChangedEventArgs<double> e)
        {
            if (e.NewValue < 0 || e.NewValue > 100) resonatorScanStart_Set.Value = e.OldValue;
            else SolsTiSPlugin.GetController().Settings["resonatorScanStart"] = e.NewValue;
        }

        private void resonatorScanStop_Set_ValueChanged(object sender, NationalInstruments.Controls.ValueChangedEventArgs<double> e)
        {
            if (e.NewValue < 0 || e.NewValue > 100) resonatorScanStop_Set.Value = e.OldValue;
            else SolsTiSPlugin.GetController().Settings["resonatorScanStop"] = e.NewValue;
        }

        private void resonatorScanRes_Set_ValueChanged(object sender, NationalInstruments.Controls.ValueChangedEventArgs<double> e)
        {
            if (e.NewValue <= 0) resonatorScanRes_Set.Value = e.OldValue;
            else SolsTiSPlugin.GetController().Settings["resonatorScanPoints"] = Convert.ToInt32(e.NewValue);
        }

        private void resonatorScanProblem(Exception e)
        {
            MessageBox.Show(e.Message);
            if (SolsTiSPlugin.GetController().IsRunning())
            {
                SolsTiSPlugin.GetController().ResonatorAcquisitionFinishing();
            }
            Application.Current.Dispatcher.BeginInvoke(
                   DispatcherPriority.Background,
                   new Action(() =>
                   {
                       EnableNoneScanCommands();
                       this.resonatorScan_Switch.Value = false;
                       this.wavemeterScan_Switch.IsEnabled = true;
                       this.resonatorScanStart_Set.IsEnabled = true;
                       this.resonatorScanStop_Set.IsEnabled = true;
                       this.resonatorScanRes_Set.IsEnabled = true;
                   }));
        }

        private void resonatorScanFinished()
        {
            Application.Current.Dispatcher.BeginInvoke(
                   DispatcherPriority.Background,
                   new Action(() =>
                   {
                       EnableNoneScanCommands();
                       this.resonatorScan_Switch.Value = false;
                       this.wavemeterScan_Switch.IsEnabled = true;
                       this.resonatorScanStart_Set.IsEnabled = true;
                       this.resonatorScanStop_Set.IsEnabled = true;
                       this.resonatorScanRes_Set.IsEnabled = true;
                   }));
        }

        private void resonatorScanData(Point[] data)
        {
            Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.resonatorScan_Display.DataSource = data;
                       }));
        }

        private void resonatorScan_Switch_Click(object sender, RoutedEventArgs e)
        {
            if (!resonatorScan_Switch.Value)
            {
                if (!SolsTiSPlugin.GetController().ResonatorAcceptableSettings())
                {
                    Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.resonatorScan_Switch.Value = false;
                       }));
                    return;
                }

                if (!SolsTiSPlugin.GetController().Solstis.Connected)
                {
                    MessageBox.Show("not connected");
                    Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.resonatorScan_Switch.Value = false;
                       }));
                    return;
                }

                Application.Current.Dispatcher.BeginInvoke(
                   DispatcherPriority.Background,
                   new Action(() => 
                   {
                       DissableNoneScanCommands(); 
                       this.resonatorScan_Switch.Value = true;
                       this.wavemeterScan_Switch.IsEnabled = false;
                       this.resonatorScanStart_Set.IsEnabled = false;
                       this.resonatorScanStop_Set.IsEnabled = false;
                       this.resonatorScanRes_Set.IsEnabled = false;
                   }));

                Thread thread = new Thread(new ThreadStart(SolsTiSPlugin.GetController().ResonatorSynchronousStartScan));
                thread.IsBackground = true;
                thread.Start();
            }
        }

        #endregion

        #region Other methods

        private void output_type_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            output_box.Items.Clear();
            output_box.SelectedIndex = -1;

            switch (output_type_box.SelectedIndex)
            {
                case 0:
                    foreach (string input in (List<string>)SolsTiSPlugin.GetController().Settings["counterChannels"])
                    {
                        output_box.Items.Add(input);
                    }
                    if (output_box.Items.Count != 0) output_box.SelectedIndex = 0;
                    break;
                case 1:
                    foreach (string input in (List<string>)SolsTiSPlugin.GetController().Settings["analogueChannels"])
                    {
                        output_box.Items.Add(input);
                    }
                    if (output_box.Items.Count != 0) output_box.SelectedIndex = 0;
                    break;
                default:
                    break;
            }
        }

        private void output_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (output_type_box.SelectedIndex)
            {
                case 0:
                    if (output_box.SelectedIndex >= 0)
                    {
                        SolsTiSPlugin.GetController().Settings["display_channel_index"] = output_box.SelectedIndex;
                        SolsTiSPlugin.GetController().RequestWavemeterHistoricData();
                        SolsTiSPlugin.GetController().RequestResonatorHistoricData();
                    }
                    else
                    {
                        this.resonatorScan_Display.DataSource = null;
                        this.wavemeterScan_Display.DataSource = null;
                    }
                    break;

                case 1:
                    if (output_box.SelectedIndex >= 0)
                    {
                        SingleCounterPlugin.GetController().Settings["display_channel_index"] = output_box.SelectedIndex;
                        SingleCounterPlugin.GetController().RequestHistoricData();
                    }
                    else
                    {
                        this.resonatorScan_Display.DataSource = null;
                        this.wavemeterScan_Display.DataSource = null;
                    }
                    break;

                default:
                    break;
            }
        }

        private void DissableNoneScanCommands()
        {
            this.connect_Button.IsEnabled = false;
            this.checkConnection_Button.IsEnabled = false;
            this.ping_Button.IsEnabled = false;
            this.etalonLock_Button.IsEnabled = false;
            this.etalonCheck_Button.IsEnabled = false;
            this.etalonUnLock_Button.IsEnabled = false;
            this.wavelengthSet_Button.IsEnabled = false;
            this.wavelengthRead_Button.IsEnabled = false;
        }

        private void EnableNoneScanCommands()
        {
            this.connect_Button.IsEnabled = true;
            this.checkConnection_Button.IsEnabled = true;
            this.ping_Button.IsEnabled = true;
            this.etalonLock_Button.IsEnabled = true;
            this.etalonCheck_Button.IsEnabled = true;
            this.etalonUnLock_Button.IsEnabled = true;
            this.wavelengthSet_Button.IsEnabled = true;
            this.wavelengthRead_Button.IsEnabled = true;
        }

        #endregion

        #region Closing event

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (SolsTiSPlugin.GetController().Solstis.Connected) SolsTiSPlugin.GetController().Solstis.Disconnect();
            windowInstance = null;
        }

        #endregion

    }
}
