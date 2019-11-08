using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace EDMPhaseLock
{
	/// <summary>
	/// Summary description for Runner.
	/// </summary>
	public class Runner
	{
		// This is the entry point to PhaseLock. The application is published to the remoting system here.
		[STAThread]
		static void Main() 
		{
			// instantiate the controller
			MainForm form = new MainForm();
			
			// publish the controller to the remoting system
			TcpChannel channel = new TcpChannel(1175);
			ChannelServices.RegisterChannel(channel, false);
			RemotingServices.Marshal(form, "controller.rem");

			// hand over to the controller
			form.StartApplication();

			// the application is finishing - close down the remoting channel
			RemotingServices.Disconnect(form);
			ChannelServices.UnregisterChannel(channel);
		}
	}
}
