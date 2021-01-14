using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace EDMPhaseLock2020
{
	static class Runner
	{
		// This is the entry point to PhaseLock. The application is published to the remoting system here.
		[STAThread]
		static void Main()
		{
			// instantiate the controller
			ControlWindow window = new ControlWindow();

			// publish the controller to the remoting system
			TcpChannel channel = new TcpChannel(1175);
			ChannelServices.RegisterChannel(channel, false);
			RemotingServices.Marshal(window, "controller.rem");

			// hand over to the controller
			window.StartApplication();

			// the application is finishing - close down the remoting channel
			RemotingServices.Disconnect(window);
			ChannelServices.UnregisterChannel(channel);

		}
	}
}
