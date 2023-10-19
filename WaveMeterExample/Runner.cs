using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using DAQ.Environment;

//Runner of the WavemeterLock client

namespace WavemeterLockServer
{
    static class Runner
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]

        static void Main()
        {
            string hostName = (String)System.Environment.GetEnvironmentVariables()["COMPUTERNAME"];
            EnvironsHelper eHelper = new EnvironsHelper(hostName);
            int channelNumber = eHelper.serverTCPChannel;
            Controller controller = new Controller();

            

            // publish the controller to the remoting system
            TcpChannel clientChannel = new TcpChannel(channelNumber);
            ChannelServices.RegisterChannel(clientChannel, false);
            RemotingServices.Marshal(controller, "controller.rem");

            controller.start();

        }
    }
}
