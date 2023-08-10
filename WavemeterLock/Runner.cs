using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAQ.Environment;

namespace WavemeterLock
{
    static class Runner
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string thisComputerName = Environment.MachineName;
            EnvironsHelper eHelper = new EnvironsHelper(thisComputerName);
            string hostName = eHelper.serverComputerName;
            int serverChannelNumber = eHelper.wavemeterLockTCPChannel;
            int hostChannelNumber = eHelper.serverTCPChannel;

            // publish the controller to the remoting system
            TcpChannel clientChannel = new TcpChannel(serverChannelNumber);

            Controller controller = new Controller("Default", hostName, hostChannelNumber);
            ChannelServices.RegisterChannel(clientChannel, false);
            RemotingServices.Marshal(controller, "controller.rem");
            
            controller.start();
        }
    }
}
