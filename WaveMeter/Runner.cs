using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WaveMeter
{
    static class Runner
    {
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
