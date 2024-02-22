using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAQ.HAL;
using DAQ.Environment;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

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

            //Application.ThreadException +=;
            //AppDomain.CurrentDomain.UnhandledException += controller.ExceptionHandler;

            TcpChannel tcpChannel = new TcpChannel(1172);
            ChannelServices.RegisterChannel(tcpChannel, false);
            RemotingServices.Marshal(controller, "controller.rem");

            controller.Start();
        }

    }
}
