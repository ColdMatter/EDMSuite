using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Windows.Forms;

namespace ScanMaster
{
	/// <summary>
	/// Summary description for Runner.
	/// </summary>
	public class Runner
	{
  		// This is the entry point to ScanMaster. Make a controller and pass
		// execution to it. The controller is published to the remoting system here.
		[STAThread]
		static void Main() 
		{
			// instantiate the controller
			Controller controller = Controller.GetController();
			
			// publish the controller to the remoting system
			TcpChannel channel = new TcpChannel(1170);
			ChannelServices.RegisterChannel(channel, false);
			RemotingServices.Marshal(controller, "controller.rem");

			// hand over to the controller
            Application.EnableVisualStyles();
			controller.StartApplication();

			// the application is finishing - close down the remoting channel
			RemotingServices.Disconnect(controller);
			ChannelServices.UnregisterChannel(channel);
		}
	}
}
