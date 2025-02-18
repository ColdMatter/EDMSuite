using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace csAcq4
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
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new FormMain());
            //FormMain formMain = new FormMain();
            //Application.Run(formMain);

            CCDController controller = new CCDController();
            
            //FormMain mainwindow = new FormMain();
            //mainwindow.controller = controller;

            //controller.window = mainwindow;

            //Application.Run(mainwindow);

            int tcpchannelnum = 5555;

            // publish the controller to the remoting system
            TcpChannel clientChannel = new TcpChannel(tcpchannelnum);
            ChannelServices.RegisterChannel(clientChannel, false);
            RemotingServices.Marshal(controller, "controller.rem");

            controller.Start();
        }
    }
}
