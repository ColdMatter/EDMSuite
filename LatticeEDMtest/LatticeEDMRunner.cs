using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Windows.Forms;

namespace LatticeHardwareControl
{
	/// <summary>
	/// Summary description for Runner.
	/// </summary>
	public class Runner
	{
		[STAThread]
		static void Main()
		{

			Program controller = new Program();
			controller = new Program();

			// publish the controller to the remoting system
			TcpChannel channel = new TcpChannel(1197);
			ChannelServices.RegisterChannel(channel, false);
			RemotingServices.Marshal(controller, "controller.rem");

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			controller.Start();

			// the application is finishing - close down the remoting channel
			RemotingServices.Disconnect(controller);
			ChannelServices.UnregisterChannel(channel);
		}

	}
}