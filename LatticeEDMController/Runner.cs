using System;
//using System.Runtime.Remoting;
//using System.Runtime.Remoting.Channels;
//using System.Runtime.Remoting.Channels.Tcp;
using System.Windows.Forms;

namespace LatticeEDMController
{
	/// <summary>
	/// Summary description for Runner.
	/// </summary>
	public class Runner
	{
		[STAThread]
		static void Main()
		{

			LatticeController controller = new LatticeController();
			controller = new LatticeController();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			controller.Start();
		}

	}
}