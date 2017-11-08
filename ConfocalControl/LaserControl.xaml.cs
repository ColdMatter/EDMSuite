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
        }

        #endregion

        private void connect_Button_Click(object sender, RoutedEventArgs e)
        {
            if (true)
            {
                SolsTiSPlugin.GetController().Solstis.Connect();
                string reply = SolsTiSPlugin.GetController().Solstis.StartLink(0);
                MessageBox.Show(reply);
            }
        }

        private void foo_action_Button_Click(object sender, RoutedEventArgs e)
        {
            SolsTiSPlugin.GetController().Solstis.Connect();
            SolsTiSPlugin.GetController().Solstis.StartLink();

            string pong = SolsTiSPlugin.GetController().Solstis.PingTest("HELLOworld");

            MessageBox.Show(pong);

            SolsTiSPlugin.GetController().Solstis.Disconnect();
        }

        #region Closing event

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            windowInstance = null;
        }

        #endregion

    }
}
