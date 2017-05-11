using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MicrocavityScanner
{
    static class Runner
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //make a new controller
            Controller controller = Controller.GetController();

            // hand over to the controller
            controller.StartApplication();
        }
    }
}
