using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace SympatheticHardwareControl
{
    static class Runner
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // instantiate the controller
            Controller controller = new Controller();

            // publish the controller to the remoting system
            TcpChannel channel = new TcpChannel(1172);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(controller, "controller.rem");

            // hand over to the controller
            controller.Start();

            // the application is finishing - close down the remoting channel
            RemotingServices.Disconnect(controller);
            ChannelServices.UnregisterChannel(channel);
        }
    }
}