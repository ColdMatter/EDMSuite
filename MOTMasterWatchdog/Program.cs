using System;
using System.Threading;
using System.Windows.Forms;

namespace MOTMasterWatchdog
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Two watchdogs would race each other for the same channels.
            bool firstInstance;
            using (Mutex mutex = new Mutex(true, "EDMSuite.MOTMasterWatchdog", out firstInstance))
            {
                if (!firstInstance)
                {
                    MessageBox.Show("The MOTMaster watchdog is already running.", "MOTMaster watchdog");
                    return;
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new WatchdogForm());
            }
        }
    }
}
