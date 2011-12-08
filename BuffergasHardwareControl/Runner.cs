using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace BuffergasHardwareControl
{
    static class Runner
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // instantiate the controller
            Controller controller = new Controller();

           

            // hand over to the controller
            controller.Start();

           
           
            
        }
    }
}