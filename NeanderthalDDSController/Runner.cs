using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace NeanderthalDDSController
{
    internal static class Runner
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Controller controller = new Controller();

            // publish the controller to the remoting system
            TcpChannel clientChannel = new TcpChannel(1818);
            ChannelServices.RegisterChannel(clientChannel, false);
            RemotingServices.Marshal(controller, "controller.rem");


            controller.readCard();
            controller.start();

        }
    }
}