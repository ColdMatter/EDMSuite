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

        #region Closing event

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            windowInstance = null;
        }

        #endregion
    }
}
