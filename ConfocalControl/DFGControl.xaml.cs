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
    /// Interaction logic for DFGControl.xaml
    /// </summary>
    public partial class DFGControl : Window
    {
        #region Window members

        // Dependencies should refer to this instance only 
        private static DFGControl windowInstance;
        public static DFGControl GetWindow()
        {
            if (windowInstance == null)
            {
                windowInstance = new DFGControl();
            }
            return windowInstance;
        }

        #endregion

        #region Initialization

        public DFGControl()
        {
            InitializeComponent();

            checkConnection_Button_Click(null, null);

            wavelengthSet_Numeric.Value = (double)DFGPlugin.GetController().Settings["wavelength"];
            wavemeterScanStart_Set.Value = (double)DFGPlugin.GetController().Settings["wavemeterScanStart"];
            wavemeterScanStop_Set.Value = (double)DFGPlugin.GetController().Settings["wavemeterScanStop"];
            wavemeterScanRes_Set.Value = (int)DFGPlugin.GetController().Settings["wavemeterScanPoints"];

            DFGPlugin.GetController().WavemeterData += wavemeterScanData;
            DFGPlugin.GetController().WavemeterScanFinished += wavemeterScanFinished;
            DFGPlugin.GetController().WavemeterScanProblem += wavemeterScanProblem;

            output_type_box.Items.Add("Counters");
            output_type_box.Items.Add("Analogues");
            output_type_box.SelectedIndex = 0;

            tripletScanStart_Set.Value = (double)DFGPlugin.GetController().Settings["tripletStart"];
            tripletScanStop_Set.Value = (double)DFGPlugin.GetController().Settings["tripletStop"];
            tripletScanRes_Set.Value = (int)DFGPlugin.GetController().Settings["tripletScanPoints"];
            tripletScanInt_Set.Value = (double)DFGPlugin.GetController().Settings["tripletInt"];
            tripletScanRate_Set.Value = (double)DFGPlugin.GetController().Settings["tripletRate"];

            DFGPlugin.GetController().TripletScanProblem += tripletScanProblem;
            DFGPlugin.GetController().TripletScanFinished += tripletScanFinished;
            DFGPlugin.GetController().TripletData += tripletScanData;
            DFGPlugin.GetController().TripletDFTData += tripletDFTData;

            triplet_output_type_box.Items.Add("Counters");
            triplet_output_type_box.Items.Add("Analogues");
            triplet_output_type_box.SelectedIndex = 0;
        }

        #endregion

        #region TCP connection events

        private void connect_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!DFGPlugin.GetController().DFG.Connected)
            {
                try
                {
                    DFGPlugin.GetController().DFG.Connect();
                    string reply = DFGPlugin.GetController().DFG.StartLink(0);
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
            if (DFGPlugin.GetController().DFG.Connected) DFGPlugin.GetController().DFG.Disconnect();
            else MessageBox.Show("already disconnected");

            checkConnection_Button_Click(null, null);
        }

        private void checkConnection_Button_Click(object sender, RoutedEventArgs e)
        {
            if (DFGPlugin.GetController().DFG.Connected) checkConnection_Reader.Text = "Active";
            else checkConnection_Reader.Text = "Inactive";
        }

        private void ping_Button_Click(object sender, RoutedEventArgs e)
        {
            if (DFGPlugin.GetController().DFG.Connected)
            {
                string pong = DFGPlugin.GetController().DFG.PingTest("PONG");
                MessageBox.Show(pong);
            }
            else MessageBox.Show("not connected");
        }

        #endregion

        #region Wavelength events

        private void wavelengthSet_Numeric_ValueChanged(object sender, NationalInstruments.Controls.ValueChangedEventArgs<double> e)
        {
            if (e.NewValue < 1100 || e.NewValue > 1650) wavemeterScanStart_Set.Value = e.OldValue;
            else DFGPlugin.GetController().Settings["wavelength"] = e.NewValue;
        }

        private void wavelengthSet_Button_Click(object sender, RoutedEventArgs e)
        {
            if (DFGPlugin.GetController().DFG.Connected)
            {
                double wavelength = (double)DFGPlugin.GetController().Settings["wavelength"];
                int set_reply = DFGPlugin.GetController().DFG.wavelength("infrared", wavelength, true);

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
            }
            else MessageBox.Show("not connected");
        }

        #endregion

        #region Wavemeter Scan events

        private void wavemeterScanStart_Set_ValueChanged(object sender, NationalInstruments.Controls.ValueChangedEventArgs<double> e)
        {
            if (e.NewValue < 1100 || e.NewValue > 1650) wavemeterScanStart_Set.Value = e.OldValue;
            else DFGPlugin.GetController().Settings["wavemeterScanStart"] = e.NewValue;
        }

        private void wavemeterScanStop_Set_ValueChanged(object sender, NationalInstruments.Controls.ValueChangedEventArgs<double> e)
        {
            if (e.NewValue < 1100 || e.NewValue > 1650) wavemeterScanStop_Set.Value = e.OldValue;
            else DFGPlugin.GetController().Settings["wavemeterScanStop"] = e.NewValue;
        }

        private void wavemeterScanRes_Set_ValueChanged(object sender, NationalInstruments.Controls.ValueChangedEventArgs<int> e)
        {
            if (e.NewValue <= 1) wavemeterScanRes_Set.Value = e.OldValue;
            else DFGPlugin.GetController().Settings["wavemeterScanPoints"] = e.NewValue;
        }

        private void wavemeterScanProblem(Exception e)
        {
            MessageBox.Show(e.Message);
            if (DFGPlugin.GetController().IsRunning() && DFGPlugin.GetController().WavemeterScanIsRunning())
            {
                DFGPlugin.GetController().WavemeterAcquisitionFinishing();
            }
            Application.Current.Dispatcher.BeginInvoke(
                   DispatcherPriority.Background,
                   new Action(() =>
                   {
                       EnableNoneScanCommands();
                       this.wavemeterScan_Switch.Value = false;
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
                if (!DFGPlugin.GetController().WavemeterAcceptableSettings())
                {
                    Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.wavemeterScan_Switch.Value = false;
                       }));
                    return;
                }

                if (!DFGPlugin.GetController().DFG.Connected)
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
                       this.wavemeterScanStart_Set.IsEnabled = false;
                       this.wavemeterScanStop_Set.IsEnabled = false;
                       this.wavemeterScanRes_Set.IsEnabled = false;
                   }));

                Thread thread = new Thread(new ThreadStart(DFGPlugin.GetController().WavemeterSynchronousStartScan));
                thread.IsBackground = true;
                thread.Start();
                output_type_box_SelectionChanged(null, null);
            }

            else
            {
                if (DFGPlugin.GetController().IsRunning())
                {
                    DFGPlugin.GetController().StopAcquisition();
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
                    DFGPlugin.GetController().Settings["wavemeter_channel_type"] = "Counters";
                    foreach (string input in (List<string>)DFGPlugin.GetController().wavemeterHistoricSettings["counterChannels"])
                    {
                        output_box.Items.Add(input);
                    }
                    if (output_box.Items.Count != 0) output_box.SelectedIndex = 0;
                    break;
                case 1:
                    DFGPlugin.GetController().Settings["wavemeter_channel_type"] = "Analogues";
                    foreach (string input in (List<string>)DFGPlugin.GetController().wavemeterHistoricSettings["analogueChannels"])
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
                        DFGPlugin.GetController().Settings["wavemeter_display_channel_index"] = output_box.SelectedIndex;
                        if (!DFGPlugin.GetController().WavemeterScanIsRunning()) DFGPlugin.GetController().RequestWavemeterHistoricData();
                    }
                    else
                    {
                        this.wavemeterScan_Display.DataSource = null;
                    }
                    break;

                case 1:
                    if (output_box.SelectedIndex >= 0)
                    {
                        DFGPlugin.GetController().Settings["wavemeter_display_channel_index"] = output_box.SelectedIndex;
                        if (!DFGPlugin.GetController().WavemeterScanIsRunning()) DFGPlugin.GetController().RequestWavemeterHistoricData();
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

        #region Triplet Scan events

        private void tripletScanStart_Set_ValueChanged(object sender, NationalInstruments.Controls.ValueChangedEventArgs<double> e)
        {
            if (e.NewValue < 1100 || e.NewValue > 1650) tripletScanStart_Set.Value = e.OldValue;
            else DFGPlugin.GetController().Settings["tripletStart"] = e.NewValue;
        }

        private void tripletScanStop_Set_ValueChanged(object sender, NationalInstruments.Controls.ValueChangedEventArgs<double> e)
        {
            if (e.NewValue < 1100 || e.NewValue > 1650) tripletScanStop_Set.Value = e.OldValue;
            else DFGPlugin.GetController().Settings["tripletStop"] = e.NewValue;

        }

        private void tripletScanRes_Set_ValueChanged(object sender, NationalInstruments.Controls.ValueChangedEventArgs<int> e)
        {
            if (e.NewValue <= 1) tripletScanRes_Set.Value = e.OldValue;
            else DFGPlugin.GetController().Settings["tripletScanPoints"] = e.NewValue;
        }

        private void tripletScanInt_Set_ValueChanged(object sender, NationalInstruments.Controls.ValueChangedEventArgs<double> e)
        {
            if (e.NewValue <= 0) tripletScanInt_Set.Value = e.OldValue;
            else DFGPlugin.GetController().Settings["tripletInt"] = e.NewValue;
        }

        private void tripletScanRate_Set_ValueChanged(object sender, NationalInstruments.Controls.ValueChangedEventArgs<double> e)
        {
            if (e.NewValue <= 0) tripletScanRate_Set.Value = e.OldValue;
            else DFGPlugin.GetController().Settings["tripletRate"] = e.NewValue;
        }

        private void tripletScanProblem(Exception e)
        {
            MessageBox.Show(e.Message);
            if (DFGPlugin.GetController().IsRunning() && DFGPlugin.GetController().TripletScanIsRunning())
            {
                DFGPlugin.GetController().TripletAcquisitionFinishing();
            }
            Application.Current.Dispatcher.BeginInvoke(
                   DispatcherPriority.Background,
                   new Action(() =>
                   {
                       this.tripletScan_Switch.Value = false;
                   }));
        }

        private void tripletScanFinished()
        {
            Application.Current.Dispatcher.BeginInvoke(
                   DispatcherPriority.Background,
                   new Action(() =>
                   {
                       this.tripletScan_Switch.Value = false;
                   }));
        }

        private void tripletScanData(double[] data)
        {
            Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.tripletScan_Display.DataSource = data;
                       }));
        }

        private void tripletDFTData(double[] data)
        {
            Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.tripletScanDFT_Display.DataSource = data;
                       }));
        }

        private void tripletScan_Switch_Click(object sender, RoutedEventArgs e)
        {
            if (!tripletScan_Switch.Value)
            {
                if (!DFGPlugin.GetController().TripletAcceptableSettings())
                {
                    Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.tripletScan_Switch.Value = false;
                       }));
                    return;
                }

                if (!DFGPlugin.GetController().DFG.Connected)
                {
                    MessageBox.Show("not connected");
                    Application.Current.Dispatcher.BeginInvoke(
                       DispatcherPriority.Background,
                       new Action(() =>
                       {
                           this.tripletScan_Switch.Value = false;
                       }));
                    return;
                }

                Application.Current.Dispatcher.BeginInvoke(
                   DispatcherPriority.Background,
                   new Action(() =>
                   {
                       DissableNoneScanCommands();
                       this.tripletScan_Switch.Value = true;
                   }));

                Thread thread = new Thread(new ThreadStart(DFGPlugin.GetController().TripletStartScan));
                thread.IsBackground = true;
                thread.Start();
                triplet_output_type_box_SelectionChanged(null, null);
            }

            else
            {
                if (DFGPlugin.GetController().IsRunning())
                {
                    DFGPlugin.GetController().StopAcquisition();
                }
            }
        }

        private void triplet_output_type_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            triplet_output_box.Items.Clear();
            triplet_output_box.SelectedIndex = -1;

            switch (triplet_output_type_box.SelectedIndex)
            {
                case 0:
                    DFGPlugin.GetController().Settings["triplet_channel_type"] = "Counters";
                    foreach (string input in (List<string>)DFGPlugin.GetController().tripletHistoricSettings["counterChannels"])
                    {
                        triplet_output_box.Items.Add(input);
                    }
                    if (triplet_output_box.Items.Count != 0) triplet_output_box.SelectedIndex = 0;
                    break;
                case 1:
                    DFGPlugin.GetController().Settings["triplet_channel_type"] = "Analogues";
                    foreach (string input in (List<string>)DFGPlugin.GetController().tripletHistoricSettings["analogueChannels"])
                    {
                        triplet_output_box.Items.Add(input);
                    }
                    if (triplet_output_box.Items.Count != 0) triplet_output_box.SelectedIndex = 0;
                    break;
                default:
                    break;
            }
        }

        private void triplet_output_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (triplet_output_type_box.SelectedIndex)
            {
                case 0:
                    if (triplet_output_box.SelectedIndex >= 0)
                    {
                        DFGPlugin.GetController().Settings["triplet_display_channel_index"] = triplet_output_box.SelectedIndex;
                        DFGPlugin.GetController().RequestTripletHistoricData();
                    }
                    else
                    {
                        this.tripletScan_Display.DataSource = null;
                    }
                    break;

                case 1:
                    if (triplet_output_box.SelectedIndex >= 0)
                    {
                        DFGPlugin.GetController().Settings["triplet_display_channel_index"] = triplet_output_box.SelectedIndex;
                        DFGPlugin.GetController().RequestTripletHistoricData();
                    }
                    else
                    {
                        this.tripletScan_Display.DataSource = null;
                    }
                    break;

                default:
                    break;
            }
        }

        #endregion

        #region Other methods

        private void DissableNoneScanCommands()
        {
            this.connect_Button.IsEnabled = false;
            this.checkConnection_Button.IsEnabled = false;
            this.ping_Button.IsEnabled = false;
            this.wavelengthSet_Button.IsEnabled = false;
        }

        private void EnableNoneScanCommands()
        {
            this.connect_Button.IsEnabled = true;
            this.checkConnection_Button.IsEnabled = true;
            this.ping_Button.IsEnabled = true;
            this.wavelengthSet_Button.IsEnabled = true;
        }

        private void change_settings_Button_Click(object sender, RoutedEventArgs e)
        {
            DFGHardwareConfigure window = new DFGHardwareConfigure();
            window.ShowDialog();
        }

        private void save_settings_Button_Click(object sender, RoutedEventArgs e)
        {
            DFGPlugin.GetController().Settings.Save();
        }

        private void load_settings_Button_Click(object sender, RoutedEventArgs e)
        {
            DFGPlugin.GetController().LoadSettings();

            wavelengthSet_Numeric.Value = (double)DFGPlugin.GetController().Settings["wavelength"];
            wavemeterScanStart_Set.Value = (double)DFGPlugin.GetController().Settings["wavemeterScanStart"];
            wavemeterScanStop_Set.Value = (double)DFGPlugin.GetController().Settings["wavemeterScanStop"];
            wavemeterScanRes_Set.Value = (int)DFGPlugin.GetController().Settings["wavemeterScanPoints"];

            tripletScanStart_Set.Value = (double)DFGPlugin.GetController().Settings["tripletStart"];
            tripletScanStop_Set.Value = (double)DFGPlugin.GetController().Settings["tripletStop"];
            tripletScanRes_Set.Value = (int)DFGPlugin.GetController().Settings["tripletScanPoints"];
            tripletScanInt_Set.Value = (double)DFGPlugin.GetController().Settings["tripletInt"];
            tripletScanRate_Set.Value = (double)DFGPlugin.GetController().Settings["tripletRate"];
        }

        #endregion

        #region Save events

        private void save_wavemeterScan_Click(object sender, RoutedEventArgs e)
        {
            if (this.wavemeterScan_Display.DataSource != null)
            {
                DFGPlugin.GetController().SaveWavemeterData(DateTime.Today.ToString("yy-MM-dd") + "_" + DateTime.Now.ToString("HH-mm-ss") + "_wavemeterScan.txt", false);
            }
        }

        #endregion

        #region Closing event

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DFGPlugin.GetController().DFG.Connected) DFGPlugin.GetController().DFG.Disconnect();
            windowInstance = null;
        }

        #endregion

    }
}
