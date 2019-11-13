using System;
//using System.Runtime.Remoting;
//using System.Runtime.Remoting.Channels;
//using System.Runtime.Remoting.Channels.Tcp;
using System.Windows.Forms;

namespace UEDMHardwareControl
{
	/// <summary>
	/// Summary description for Runner.
	/// </summary>
	public class Runner
	{
		[STAThread]
		static void Main() 
		{
			// instantiate the controller
			UEDMController controller = new UEDMController();

			// publish the controller to the remoting system
			//TcpChannel channel = new TcpChannel(1172);
			//ChannelServices.RegisterChannel(channel, false);
			//RemotingServices.Marshal(controller, "controller.rem");

			// hand over to the controller
            Application.EnableVisualStyles();
			controller.Start();

			// the application is finishing - close down the remoting channel
			//RemotingServices.Disconnect(controller);
			//ChannelServices.UnregisterChannel(channel);
		}
	}
}