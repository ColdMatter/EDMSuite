using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAQ.HAL;
using DAQ.Environment;

namespace AlFHardwareControl
{
    static class Program
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            AlFController controller = new AlFController();
            controller.Start();
        }

    }
}
