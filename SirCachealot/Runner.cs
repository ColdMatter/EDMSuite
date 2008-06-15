using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SirCachealot
{
    static class Runner
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainWindow window = new MainWindow();
            Controller controller = new Controller();
            controller.Initialise();
            window.controller = controller;
            controller.mainWindow = window;
            Application.Run(window);
        }
    }
}