using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace TransferCavityLock2012
{
    static class Runner
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]

        static void Main(string[] config)
        {
            string cg;

            if (config.Length == 0)
            {
                cg = "defaultcavity";
            }
            else
            {
                cg = config[0];
            };

            Controller controller = new Controller(cg);

            // publish the controller to the remoting system
            TcpChannel channel = new TcpChannel(controller.config.TCPChannel);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(controller, "controller.rem");


            // hand over to the controller
            controller.Start();

            // the application is finishing - close down the remoting channel
            //RemotingServices.Disconnect(controller);
            //ChannelServices.UnregisterChannel(channel);
        }
    }
}
