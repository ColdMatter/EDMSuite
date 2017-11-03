using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaddlePolStabiliser
{
    class Runner
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Controller controller = Controller.GetController();
            Thread ContThread = new Thread(new ThreadStart(controller.Start));
            ContThread.Name = "Controller Thread";

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // hand over to the controller
            ContThread.Start();
        }
    }
}
