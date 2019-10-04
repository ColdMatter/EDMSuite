using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace EDMBlockHead
{
	/// <summary>
	/// Summary description for Runner.
	/// </summary>
	public class Runner
	{
		// This is the entry point to BlockHead.
		[STAThread]
		static void Main() 
		{
			// instantiate the controller
			Controller controller = Controller.GetController();
			
			// publish the controller to the remoting system
			TcpChannel channel = new TcpChannel(1181);
			ChannelServices.RegisterChannel(channel, false);
			RemotingServices.Marshal(controller, "controller.rem");

			// hand over to the controller
			controller.StartApplication();

			// the application is finishing - close down the remoting channel
			RemotingServices.Disconnect(controller);
			ChannelServices.UnregisterChannel(channel);
		}
	}
}
