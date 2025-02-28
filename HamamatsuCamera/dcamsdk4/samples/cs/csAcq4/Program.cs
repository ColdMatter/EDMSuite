using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using DAQ.Environment;

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

            //FormMain mainwindow = new FormMain();
            //mainwindow.controller = controller;

            //controller.window = mainwindow;

            //Application.Run(mainwindow);

            CCDController controller = new CCDController();

            string thisComputerName = Environment.MachineName;
            EnvironsHelper eHelper = new EnvironsHelper(thisComputerName);
            int serverChannelNumber = eHelper.emccdTCPChannel;
            //int tcpchannelnum = 5555;
            Console.WriteLine(thisComputerName);
            Console.WriteLine(serverChannelNumber);

            // publish the controller to the remoting system
            TcpChannel clientChannel = new TcpChannel(serverChannelNumber);
            ChannelServices.RegisterChannel(clientChannel, false);
            RemotingServices.Marshal(controller, "controller.rem");

            controller.Start();
        }
    }
}
