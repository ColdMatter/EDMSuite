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
            wavemeterScanStart_Set.Value = (double)SolsTiSPlugin.GetController().Settings["wavemeterScanStart"];
            wavemeterScanStop_Set.Value = (double)SolsTiSPlugin.GetController().Settings["wavemeterScanStop"];
            wavemeterScanRes_Set.Value = (int)SolsTiSPlugin.GetController().Settings["wavemeterScanPoints"];
            fastScanWidth_Set.Value = (double)SolsTiSPlugin.GetController().Settings["fastScanWidth"];
            fastScanTime_Set.Value = (double)SolsTiSPlugin.GetController().Settings["fastScanTime"];

            SolsTiSPlugin.GetController().WavemeterData += wavemeterScanData;
            SolsTiSPlugin.GetController().WavemeterScanFinished += wavemeterScanFinished;
            SolsTiSPlugin.GetController().WavemeterScanProblem += wavemeterScanProblem;

            SolsTiSPlugin.GetController().FastData += fastScanData;
            SolsTiSPlugin.GetController().FastScanFinished += fastScanFinished;
            SolsTiSPlugin.GetController().FastScanProblem += fastScanProblem;

            SolsTiSPlugin.GetController().TeraData += teraScanData;
            SolsTiSPlugin.GetController().TeraTotalOnlyData += teraScanTotalOnlyData;
            SolsTiSPlugin.GetController().TeraSegmentOnlyData += teraScanSegmentOnlyData;
            SolsTiSPlugin.GetController().TeraSegmentScanFinished += teraScanSegmentFinished;
            SolsTiSPlugin.GetController().TeraScanFinished += teraScanFinished;
            SolsTiSPlugin.GetController().TeraScanProblem += teraScanProblem;

            output_type_box.Items.Add("Counters");
            output_type_box.Items.Add("Analogues");
            output_type_box.SelectedIndex = 0;

            fastScanType_ComboBox.Items.Add("etalon_continuous");
            fastScanType_ComboBox.Items.Add("resonator_continuous");
            fastScanType_ComboBox.SelectedItem = (string)SolsTiSPlugin.GetController().Settings["fastScanType"];
            fastScan_output_type_box.Items.Add("Counters");
            fastScan_output_type_box.Items.Add("Analogues");
            fastScan_output_type_box.SelectedIndex = 0;

            teraScan_output_type_box.Items.Add("Lambda");
            teraScan_output_type_box.Items.Add("Counters");
            teraScan_output_type_box.Items.Add("Analogues");
            teraScan_output_type_box.SelectedIndex = 0;
            teraScan_segmentDisplay_ComboBox.Items.Add("Current");
            teraScan_segmentDisplay_ComboBox.SelectedIndex = 0;
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
            if (e.NewValue < 700 || e.NewValue > 1000) wavemeterScanStart_Set.Value = e.OldValue;
            else SolsTiSPlugin.GetController().Settings["wavelength"] = e.NewValue;
        }

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

        private void wavelengthSet_Button_Click(object sender, RoutedEventArgs e)
        {
            if (SolsTiSPlugin.GetController().Solstis.Connected)
            {
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
            if (SolsTiSPlugin.GetController().IsRunning() && SolsTiSPlugin.GetController().WavemeterScanIsRunning())
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
                output_type_box_SelectionChanged(null, null);
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
                    foreach (string input in (List<string>)SolsTiSPlugin.GetController().wavemeterHistoricSettings["counterChannels"])
                    {
                        output_box.Items.Add(input);
                    }
                    if (output_box.Items.Count != 0) output_box.SelectedIndex = 0;
                    break;
                case 1:
                    SolsTiSPlugin.GetController().Settings["wavemeter_channel_type"] = "Analogues";
                    foreach (string input in (List<string>)SolsTiSPlugin.GetController().wavemeterHistoricSettings["analogueChannels"])
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
                        if (!SolsTiSPlugin.GetController().WavemeterScanIsRunning())  SolsTiSPlugin.GetController().RequestWavemeterHistoricData();
                    }
                    else
                    {
                        this.wavemeterScan_Display.DataSource = null;
                    }
                    break;

                case 1:
                    if (output_box.SelectedIndex >= 0)
                    {
                        SolsTiSPlugin.GetController().Settings["wavemeter_display_channel_index"] = output_box.SelectedIndex;
                        if (!SolsTiSPlugin.GetController().WavemeterScanIsRunning()) SolsTiSPlugin.GetController().RequestWavemeterHistoricData();
                    }
                    else
                    {
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

        private void fastScan_output_type_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            fastScan_output_box.Items.Clear();
            fastScan_output_box.SelectedIndex = -1;

            switch (fastScan_output_type_box.SelectedIndex)
            {
                case 0:
                    SolsTiSPlugin.GetController().Settings["fast_channel_type"] = "Counters";
                    foreach (string input in (List<string>)SolsTiSPlugin.GetController().Settings["counterChannels"])
                    {
                        fastScan_output_box.Items.Add(input);
                    }
                    if (fastScan_output_box.Items.Count != 0) fastScan_output_box.SelectedIndex = 0;
                    break;
                case 1:
                    SolsTiSPlugin.GetController().Settings["fast_channel_type"] = "Analogues";
                    foreach (string input in (List<string>)SolsTiSPlugin.GetController().Settings["analogueChannels"])
                    {
                        fastScan_output_box.Items.Add(input);
                    }
                    if (fastScan_output_box.Items.Count != 0) fastScan_output_box.SelectedIndex = 0;
                    break;
                default:
                    break;
            }
        }

        private void fastScan_output_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (fastScan_output_type_box.SelectedIndex)
            {
                case 0:
                    if (fastScan_output_box.SelectedIndex >= 0)
                    {
                        SolsTiSPlugin.GetController().Settings["fast_display_channel_index"] = fastScan_output_box.SelectedIndex;
                        if (!SolsTiSPlugin.GetController().FastScanIsRunning()) SolsTiSPlugin.GetController().RequestFastHistoricData();
                    }
                    else
                    {
                        this.fastScan_Display.DataSource = null;
                    }
                    break;

                case 1:
                    if (fastScan_output_box.SelectedIndex >= 0)
                    {
                        SolsTiSPlugin.GetController().Settings["fast_display_channel_index"] = fastScan_output_box.SelectedIndex;
                        if (!SolsTiSPlugin.GetController().FastScanIsRunning()) SolsTiSPlugin.GetController().RequestFastHistoricData();
                    }
                    else
                    {
                        this.fastScan_Display.DataSource = null;
                    }
                    break;

                default:
                    break;
            }
        }

        private void fastScanProblem(Exception e)
        {
            MessageBox.Show(e.Message);
            if (SolsTiSPlugin.GetController().IsRunning() && SolsTiSPlugin.GetController().FastScanIsRunning())
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
                bool ready = false;

                if (!ready)
                {
                    MessageBox.Show("Not ready yet.");
                    Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.fastScan_Switch.Value = false;
                       }));
                    return;
                }

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

        #region Tera Scan events

        private void teraScan_output_type_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            teraScan_output_box.Items.Clear();
            teraScan_output_box.SelectedIndex = -1;

            switch (teraScan_output_type_box.SelectedIndex)
            {
                case 0:
                    SolsTiSPlugin.GetController().Settings["tera_channel_type"] = "Lambda";
                    if (!SolsTiSPlugin.GetController().TeraScanIsRunning()) SolsTiSPlugin.GetController().RequestTeraHistoricData();
                    teraScan_segmentDisplay_ComboBox_SelectionChanged(null, null);
                    break;
                case 1:
                    SolsTiSPlugin.GetController().Settings["tera_channel_type"] = "Counters";
                    foreach (string input in (List<string>)SolsTiSPlugin.GetController().teraScanBufferAccess.historicSettings["counterChannels"])
                    {
                        teraScan_output_box.Items.Add(input);
                    }
                    if (teraScan_output_box.Items.Count != 0) teraScan_output_box.SelectedIndex = 0;
                    break;
                case 2:
                    SolsTiSPlugin.GetController().Settings["tera_channel_type"] = "Analogues";
                    foreach (string input in (List<string>)SolsTiSPlugin.GetController().teraScanBufferAccess.historicSettings["analogueChannels"])
                    {
                        teraScan_output_box.Items.Add(input);
                    }
                    if (teraScan_output_box.Items.Count != 0) teraScan_output_box.SelectedIndex = 0;
                    break;
                default:
                    break;
            }
        }

        private void teraScan_output_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SolsTiSPlugin.GetController().Settings["tera_display_channel_index"] = teraScan_output_box.SelectedIndex;
            if (!SolsTiSPlugin.GetController().TeraScanIsRunning()) SolsTiSPlugin.GetController().RequestTeraHistoricData();
            teraScan_segmentDisplay_ComboBox_SelectionChanged(null, null);
        }

        private void teraScanProblem(Exception e)
        {
            MessageBox.Show(e.Message);
            if (SolsTiSPlugin.GetController().IsRunning() && SolsTiSPlugin.GetController().TeraScanIsRunning())
            {
                if (SolsTiSPlugin.GetController().TeraSegmentIsRunning())
                {
                    SolsTiSPlugin.GetController().TeraScanSegmentAcquisitionEnd();
                }
                SolsTiSPlugin.GetController().TeraScanAcquisitionStopping();
            }
        }

        private void teraScanFinished()
        {
            Application.Current.Dispatcher.BeginInvoke(
                   DispatcherPriority.Background,
                   new Action(() =>
                   {
                       EnableNoneScanCommands();
                       this.teraScan_Switch.Value = false;

                       this.fastScan_Switch.IsEnabled = true;
                       this.wavemeterScan_Switch.IsEnabled = true;
                       this.teraScan_configure_Button.IsEnabled = true;
                   }));

            MessageBoxResult result = MessageBox.Show("Restart TeraScan from " + SolsTiSPlugin.GetController().latestLambda + "nm to " + SolsTiSPlugin.GetController().Settings["TeraScanStop"] + "nm?", "Repeat TeraScan", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Dispatcher.BeginInvoke(
                   DispatcherPriority.Background,
                   new Action(() =>
                   {
                       this.teraScan_Switch.Value = true;

                       DissableNoneScanCommands();
                       this.fastScan_Switch.IsEnabled = false;
                       this.wavemeterScan_Switch.IsEnabled = false;
                       this.teraScan_configure_Button.IsEnabled = false;

                       this.teraScan_segmentDisplay_ComboBox.Items.Clear();
                       this.teraScan_segmentDisplay_ComboBox.Items.Add("Current");
                       this.teraScan_segmentDisplay_ComboBox.SelectedIndex = 0;
                   }));

                SolsTiSPlugin.GetController().Settings["TeraScanStart"] = SolsTiSPlugin.GetController().latestLambda;
                Thread thread = new Thread(new ThreadStart(SolsTiSPlugin.GetController().ResumeTeraScan));
                thread.IsBackground = true;
                thread.Start();
            }
        }

        private void teraScanData(double[] data, double[] dataSeg)
        {
            Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.teraScan_Display.DataSource = data;
                           this.segmentScanDisplay.DataSource = dataSeg;
                       }));
        }

        private void teraScanTotalOnlyData(double[] data)
        {
            Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.teraScan_Display.DataSource = data;
                       }));
        }

        private void teraScanSegmentOnlyData(double[] data)
        {
            Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.segmentScanDisplay.DataSource = data;
                       }));
        }

        private void teraScanSegmentFinished()
        {
            Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.teraScan_segmentDisplay_ComboBox.Items.Add("Segment: " + Convert.ToString(SolsTiSPlugin.GetController().teraScanBufferAccess.currentSegmentIndex));
                       }));
        }

        private void teraScan_configure_Button_Click(object sender, RoutedEventArgs e)
        {
            TeraScanConfigure window = new TeraScanConfigure();
            window.ShowDialog();
        }

        private void teraScan_Switch_Click(object sender, RoutedEventArgs e)
        {
            if (!teraScan_Switch.Value)
            {
                if (!SolsTiSPlugin.GetController().TeraScanAcceptableSettings())
                {
                    MessageBox.Show("TeraScan parameters unacceptable");
                    Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.teraScan_Switch.Value = false;
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
                           this.teraScan_Switch.Value = false;
                       }));
                    return;
                }

                Application.Current.Dispatcher.BeginInvoke(
                   DispatcherPriority.Background,
                   new Action(() => 
                   {
                       DissableNoneScanCommands(); 
                       this.fastScan_Switch.IsEnabled = false;
                       this.wavemeterScan_Switch.IsEnabled = false;
                       this.teraScan_configure_Button.IsEnabled = false;

                       this.teraScan_segmentDisplay_ComboBox.Items.Clear();
                       this.teraScan_segmentDisplay_ComboBox.Items.Add("Current");
                       this.teraScan_segmentDisplay_ComboBox.SelectedIndex = 0;
                   }));

                Thread thread = new Thread(new ThreadStart(SolsTiSPlugin.GetController().StartTeraScan));
                thread.IsBackground = true;
                thread.Start();
                teraScan_output_type_box_SelectionChanged(null, null);
            }

            else
            {
                if (SolsTiSPlugin.GetController().IsRunning())
                {
                    SolsTiSPlugin.GetController().StopAcquisition();
                }
            }
        }

        private void teraScan_segmentDisplay_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int displayIndex = teraScan_segmentDisplay_ComboBox.SelectedIndex;

            if ( displayIndex < 1 )
            {
                SolsTiSPlugin.GetController().Settings["tera_display_current_segment"] = true;
            }
            else
            {
                SolsTiSPlugin.GetController().Settings["tera_display_current_segment"] = false;
                SolsTiSPlugin.GetController().RequestSegmentData(displayIndex - 1);
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

        private void change_settings_Button_Click(object sender, RoutedEventArgs e)
        {
            LaserHardwareConfigure window = new LaserHardwareConfigure();
            window.ShowDialog();
        }

        private void save_settings_Button_Click(object sender, RoutedEventArgs e)
        {
            SolsTiSPlugin.GetController().Settings.Save();
        }

        private void load_settings_Button_Click(object sender, RoutedEventArgs e)
        {
            SolsTiSPlugin.GetController().LoadSettings();
            wavelengthSet_Numeric.Value = (double)SolsTiSPlugin.GetController().Settings["wavelength"];
            wavemeterScanStart_Set.Value = (double)SolsTiSPlugin.GetController().Settings["wavemeterScanStart"];
            wavemeterScanStop_Set.Value = (double)SolsTiSPlugin.GetController().Settings["wavemeterScanStop"];
            wavemeterScanRes_Set.Value = (int)SolsTiSPlugin.GetController().Settings["wavemeterScanPoints"];
            fastScanType_ComboBox.SelectedItem = (string)SolsTiSPlugin.GetController().Settings["fastScanType"];
            fastScanWidth_Set.Value = (double)SolsTiSPlugin.GetController().Settings["fastScanWidth"];
            fastScanTime_Set.Value = (double)SolsTiSPlugin.GetController().Settings["fastScanTime"];
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

        private void teraScan_save_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.teraScan_Display.DataSource != null)
            {
                SolsTiSPlugin.GetController().SaveTeraScanData(DateTime.Today.ToString("yy-MM-dd") + "_" + DateTime.Now.ToString("HH-mm-ss") + "_teraScan.txt", false);
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
