using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAQ.Environment;

namespace WavemeterViewer
{
    static class Runner
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            //Connect to wavemeter lock server
            string thisComputerName = Environment.MachineName;
            EnvironsHelper eHelper = new EnvironsHelper(thisComputerName);
            string hostName = eHelper.viewerServerComputerName;
            int hostChannelNumber = eHelper.viewerServerTCPChannel;


            Controller controller = new Controller(hostName, hostChannelNumber);
            controller.start();
        }
    }
}
