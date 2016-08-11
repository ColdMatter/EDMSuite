using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace MOTMaster
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
            TcpChannel channel = new TcpChannel(1187);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(controller, "controller.rem");


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            //Application.run(new MOTMasterWindow());
            controller.StartApplication();

            // the application is finishing - close down the remoting channel
            RemotingServices.Disconnect(controller);
            ChannelServices.UnregisterChannel(channel);
        }
    }
}
