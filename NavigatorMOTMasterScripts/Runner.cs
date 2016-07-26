using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace NavMaster 
{
 
    static class Runner
    {
        /// <summary>
        /// The main entry point for the application
        /// </summary>
        [STAThread]
        static void Main()
        {
            //instantiate the controller
            Controller controller = new Controller();
            // publish the controller to the remoting system
            TcpChannel channel = new TcpChannel(1187);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(controller, "controller.rem");
        }
    }
}
