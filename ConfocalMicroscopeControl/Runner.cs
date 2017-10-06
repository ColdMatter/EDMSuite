using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConfocalMicroscopeControl
{
    class Runner
    {
        static void Main()
        {
            //make a new controller
            Controller controller = Controller.GetController();

            // hand over to the controller
            controller.StartApplication();
        }
    }
}
