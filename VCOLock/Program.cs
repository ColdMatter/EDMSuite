using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace VCOLock
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // instantiate the controller
            Controller controller = new Controller();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // hand over to the controller
            controller.Start();
        }
    }
}
