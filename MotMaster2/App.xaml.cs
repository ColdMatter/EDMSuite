using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace MOTMaster2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //TODO add all of this later
       
        //void Startup(object sender,StartupEventArgs e)
        //{
        //    base.OnStartup(e);
        //    AppDomain currentDomain = AppDomain.CurrentDomain;
            


        //    // publish the controller to the remoting system
        //    TcpChannel channel = new TcpChannel(1187);
        //    ChannelServices.RegisterChannel(channel, false);
        //    RemotingServices.Marshal(controller, "controller.rem");


        //    var application = new App();
            
        //    //application.InitializeComponent();
        //    controller.StartApplication();

        //    application.Run(new MainWindow(controller));
           
        //    //Application.run(new MOTMasterWindow());
            

        //    // the application is finishing - close down the remoting channel
        //    RemotingServices.Disconnect(controller);
        //    ChannelServices.UnregisterChannel(channel);
        //}
    }
}
