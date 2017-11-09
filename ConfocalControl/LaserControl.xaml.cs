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
                            wavelength_Read.Text = ((double)reply["current_wavelength"]).ToString();
                            break;

                        case 3:
                            wavelength_Read.Text = ((double)reply["current_wavelength"]).ToString();
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
                double wavelength = wavelengthSet_Numeric.Value;

                int reply = SolsTiSPlugin.GetController().Solstis.set_wave_m(wavelength, true);

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

        private void wavelengthStop_Button_Click(object sender, RoutedEventArgs e)
        {
            if (SolsTiSPlugin.GetController().Solstis.Connected)
            {
                Dictionary<string, object> reply = SolsTiSPlugin.GetController().Solstis.stop_wave_m();

                if (reply.Count == 0)
                {
                    MessageBox.Show("empty reply");
                }
                else
                {
                    switch ((int)reply["status"])
                    {
                        case 0:
                            wavelength_Read.Text = ((double)reply["current_wavelength"]).ToString();
                            break;

                        case 1:
                            MessageBox.Show("no link to wavelength meter");
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

        #region Closing event

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            windowInstance = null;
        }

        #endregion

    }
}
