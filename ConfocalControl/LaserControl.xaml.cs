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

            SolsTiSPlugin.GetController().FastData += fastScanData;
            SolsTiSPlugin.GetController().FastScanFinished += fastScanFinished;
            SolsTiSPlugin.GetController().FastScanProblem += fastScanProblem;

            output_type_box.Items.Add("Counters");
            output_type_box.Items.Add("Analogues");
            output_type_box.SelectedIndex = 0;

            fastScanType_ComboBox.Items.Add("etalon_continuous");
            fastScanType_ComboBox.Items.Add("resonator_continuous");
            fastScanType_ComboBox.SelectedIndex = 0;
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
                    etalonCheck_Button_Click(null, null);
                    wavelengthRead_Button_Click(null, null);

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

        //private void wavelengthRead_Button_Click(object sender, RoutedEventArgs e)
        //{
        //    if (SolsTiSPlugin.GetController().Solstis.Connected)
        //    {
        //        Dictionary<string, object> reply = SolsTiSPlugin.GetController().Solstis.poll_move_wave_t();

        //        if (reply.Count == 0)
        //        {
        //            MessageBox.Show("empty reply");
        //        }
        //        else
        //        {
        //            switch ((int)reply["status"])
        //            {
        //                case 0:
        //                    wavelength_Read.Text = (Convert.ToDouble(reply["current_wavelength"])).ToString();
        //                    break;

        //                case 1:
        //                    MessageBox.Show("tuning in progress");
        //                    wavelength_Read.Text = (Convert.ToDouble(reply["current_wavelength"])).ToString();
        //                    break;

        //                case 2:
        //                    MessageBox.Show("tuning operation failed");
        //                    break;

        //                default:
        //                    MessageBox.Show("did not understand reply");
        //                    break;
        //            }
        //        }
        //    }
        //    else MessageBox.Show("not connected");
        //}

        private void wavelengthRead_Button_Click(object sender, RoutedEventArgs e)
        {
            if (SolsTiSPlugin.GetController().Solstis.Connected)
            {
                Dictionary<string, object> reply = SolsTiSPlugin.GetController().Solstis.poll_wave_m();

                if (reply.Count == 0)
                {
                    MessageBox.Show("empty reply");
                }
                else
                {
                    switch ((int)reply["status"])
                    {
                        case 0:
                            MessageBox.Show("tuning software not active");
                            break;

                        case 1:
                            MessageBox.Show("no link to wavelength meter or no meter configured");
                            break;

                        case 2:
                            MessageBox.Show("tuning in progress");
                            wavelength_Read.Text = (Convert.ToDouble(reply["current_wavelength"])).ToString();
                            break;

                        case 3:
                            wavelength_Read.Text = (Convert.ToDouble(reply["current_wavelength"])).ToString();
                            break;

                        default:
                            MessageBox.Show("did not understand reply");
                            break;
                    }
                }
            }
            else MessageBox.Show("not connected");
        }

        //private void wavelengthSet_Button_Click(object sender, RoutedEventArgs e)
        //{
        //    if (SolsTiSPlugin.GetController().Solstis.Connected)
        //    {
        //        double wavelength = (double)SolsTiSPlugin.GetController().Settings["wavelength"];
        //        int reply = SolsTiSPlugin.GetController().Solstis.move_wave_t(wavelength, true);
        //        switch (reply)
        //        {
        //            case -1:
        //                MessageBox.Show("empty reply");
        //                break;

        //            case 0:
        //                MessageBox.Show("task completed");
        //                break;

        //            case 1:
        //                MessageBox.Show("task failed");
        //                break;

        //            default:
        //                MessageBox.Show("did not understand reply");
        //                break;
        //        }

        //        wavelengthRead_Button_Click(null, null);
        //    }
        //    else MessageBox.Show("not connected");
        //}

        private void wavelengthSet_Button_Click(object sender, RoutedEventArgs e)
        {
            if (SolsTiSPlugin.GetController().Solstis.Connected)
            {
                Dictionary<string, object> reply = SolsTiSPlugin.GetController().Solstis.poll_wave_m();

                if (reply.Count == 0)
                {
                    MessageBox.Show("empty reply");
                }
                else
                {
                    switch ((int)reply["status"])
                    {
                        case 0:
                            MessageBox.Show("tuning software not active");
                            break;

                        case 1:
                            MessageBox.Show("no link to wavelength meter or no meter configured");
                            break;

                        case 2:
                        case 3:
                            double wavelength = (double)SolsTiSPlugin.GetController().Settings["wavelength"];
                            int set_reply = SolsTiSPlugin.GetController().Solstis.set_wave_m(wavelength, true);

                            switch (set_reply)
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
                            break;

                        default:
                            MessageBox.Show("did not understand reply");
                            break;
                    }
                }
            }
            else MessageBox.Show("not connected");
        }

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
                       this.fastScan_Switch.IsEnabled = true;
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
                       this.fastScan_Switch.IsEnabled = true;
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
                       this.fastScan_Switch.IsEnabled = false;
                       this.wavemeterScanStart_Set.IsEnabled = false;
                       this.wavemeterScanStop_Set.IsEnabled = false;
                       this.wavemeterScanRes_Set.IsEnabled = false;
                   }));

                Thread thread = new Thread(new ThreadStart(SolsTiSPlugin.GetController().WavemeterSynchronousStartScan));
                thread.IsBackground = true;
                thread.Start();
            }

            else
            {
                if (SolsTiSPlugin.GetController().IsRunning())
                {
                    SolsTiSPlugin.GetController().StopAcquisition();
                }
            }
        }

        private void output_type_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            output_box.Items.Clear();
            output_box.SelectedIndex = -1;

            switch (output_type_box.SelectedIndex)
            {
                case 0:
                    SolsTiSPlugin.GetController().Settings["wavemeter_channel_type"] = "Counters";
                    foreach (string input in (List<string>)SolsTiSPlugin.GetController().Settings["counterChannels"])
                    {
                        output_box.Items.Add(input);
                    }
                    if (output_box.Items.Count != 0) output_box.SelectedIndex = 0;
                    break;
                case 1:
                    SolsTiSPlugin.GetController().Settings["wavemeter_channel_type"] = "Analogues";
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
                        SolsTiSPlugin.GetController().Settings["wavemeter_display_channel_index"] = output_box.SelectedIndex;
                        SolsTiSPlugin.GetController().RequestWavemeterHistoricData();
                    }
                    else
                    {
                        this.fastScan_Display.DataSource = null;
                        this.wavemeterScan_Display.DataSource = null;
                    }
                    break;

                case 1:
                    if (output_box.SelectedIndex >= 0)
                    {
                        SolsTiSPlugin.GetController().Settings["wavemeter_display_channel_index"] = output_box.SelectedIndex;
                        SolsTiSPlugin.GetController().RequestWavemeterHistoricData();
                    }
                    else
                    {
                        this.fastScan_Display.DataSource = null;
                        this.wavemeterScan_Display.DataSource = null;
                    }
                    break;

                default:
                    break;
            }
        }

        #endregion

        #region Fast Scan events

        private void fastScanType_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (fastScanType_ComboBox.SelectedIndex)
            {
                case 0:
                    SolsTiSPlugin.GetController().Settings["fastScanType"] = "etalon_continuous";
                    break;
                case 1:
                    SolsTiSPlugin.GetController().Settings["fastScanType"] = "resonator_continuous";
                    break;
                default:
                    break;
            }
        }

        private void fastScanWidth_Set_ValueChanged(object sender, NationalInstruments.Controls.ValueChangedEventArgs<double> e)
        {
            if (e.NewValue <= 0.01) fastScanWidth_Set.Value = e.OldValue;
            else SolsTiSPlugin.GetController().Settings["fastScanWidth"] = e.NewValue;
        }

        private void fastScanTime_Set_ValueChanged(object sender, NationalInstruments.Controls.ValueChangedEventArgs<double> e)
        {
            if (e.NewValue <= 0.01 || e.NewValue >= 10000) fastScanTime_Set.Value = e.OldValue;
            else SolsTiSPlugin.GetController().Settings["fastScanTime"] = e.NewValue;
        }

        private void fastScanProblem(Exception e)
        {
            MessageBox.Show(e.Message);
            if (SolsTiSPlugin.GetController().IsRunning())
            {
                SolsTiSPlugin.GetController().FastAcquisitionFinishing();
            }
        }

        private void fastScanFinished()
        {
            Application.Current.Dispatcher.BeginInvoke(
                   DispatcherPriority.Background,
                   new Action(() =>
                   {
                       EnableNoneScanCommands();
                       this.fastScan_Switch.Value = false;
                       this.wavemeterScan_Switch.IsEnabled = true;
                       this.fastScanType_ComboBox.IsEnabled = true;
                       this.fastScanWidth_Set.IsEnabled = true;
                       this.fastScanTime_Set.IsEnabled = true;
                   }));
        }

        private void fastScanData(Point[] data)
        {
            Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.fastScan_Display.DataSource = data;
                       }));
        }

        private void fastScan_Switch_Click(object sender, RoutedEventArgs e)
        {
            if (!fastScan_Switch.Value)
            {
                if (!SolsTiSPlugin.GetController().FastScanAcceptableSettings())
                {
                    Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.fastScan_Switch.Value = false;
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
                           this.fastScan_Switch.Value = false;
                       }));
                    return;
                }

                Application.Current.Dispatcher.BeginInvoke(
                   DispatcherPriority.Background,
                   new Action(() => 
                   {
                       DissableNoneScanCommands(); 
                       this.fastScan_Switch.Value = true;
                       this.wavemeterScan_Switch.IsEnabled = false;
                       this.fastScanType_ComboBox.IsEnabled = false;
                       this.fastScanWidth_Set.IsEnabled = false;
                       this.fastScanTime_Set.IsEnabled = false;
                   }));

                Thread thread = new Thread(new ThreadStart(SolsTiSPlugin.GetController().FastSynchronousStartScan));
                thread.IsBackground = true;
                thread.Start();
            }

            else
            {
                if (SolsTiSPlugin.GetController().IsRunning())
                {
                    SolsTiSPlugin.GetController().StopAcquisition();
                }
            }
        }

        #endregion

        #region Other methods

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

        #region Save events

        private void save_wavemeterScan_Click(object sender, RoutedEventArgs e)
        {
            if (this.wavemeterScan_Display.DataSource != null)
            {
                SolsTiSPlugin.GetController().SaveWavemeterData(DateTime.Today.ToString("yy-MM-dd") + "_" + DateTime.Now.ToString("HH-mm-ss") + "_wavemeterScan.txt", false);
            }
        }

        private void save_fastScan_Click(object sender, RoutedEventArgs e)
        {
            if (this.fastScan_Display.DataSource != null)
            {
                SolsTiSPlugin.GetController().SaveFastData(DateTime.Today.ToString("yy-MM-dd") + "_" + DateTime.Now.ToString("HH-mm-ss") + "_fastScan.txt", false);
            }
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
