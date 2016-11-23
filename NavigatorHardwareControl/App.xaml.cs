using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace NavigatorHardwareControl
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Controller controller;
        [STAThread]
        public static void Main()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += On_UnhandledException;
            
            controller = new Controller();
           
            // publish the controller to the remoting system
            TcpChannel channel = new TcpChannel(1172);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(controller, "controller.rem");
            //Starts the application. Inside the controlWindow, the controller is started
            var application = new App();
            application.InitializeComponent();
            application.Run();
            // the application is finishing - close down the remoting channel
            RemotingServices.Disconnect(controller);
            ChannelServices.UnregisterChannel(channel);
        }

        private static void On_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            controller.hsdio.ReleaseHardware();
        }
      
    }


}
