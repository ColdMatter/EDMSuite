using System;
using System.Runtime.Remoting;

using EDMConfig;

namespace EDMRemotingHelper
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class RemotingHelper
	{
		ScanMaster.Controller scanMaster;
		BlockHead.Controller blockHead;
		EDMHardwareControl.Controller hardwareControl;

		public RemotingHelper()
		{
			// register the types
			RemotingConfiguration.RegisterWellKnownClientType(
				Type.GetType("ScanMaster.Controller, ScanMaster"),
				"tcp://localhost:1170/controller.rem"
				);

			RemotingConfiguration.RegisterWellKnownClientType(
				Type.GetType("BlockHead.Controller, BlockHead"),
				"tcp://localhost:1171/controller.rem"
				);

			RemotingConfiguration.RegisterWellKnownClientType(
				Type.GetType("EDMHardwareControl.Controller, EDMHardwareControl"),
				"tcp://localhost:1172/controller.rem"
				);

			// create the proxy objects
			scanMaster = new ScanMaster.Controller();
			blockHead = new BlockHead.Controller();
			hardwareControl = new EDMHardwareControl.Controller();
		}

		public ScanMaster.Controller ScanMaster
		{
			get { return scanMaster; }
		}

		public BlockHead.Controller BlockHead
		{
			get { return blockHead; }
		}

		public EDMHardwareControl.Controller HardwareControl
		{
			get { return hardwareControl; }
		}

		public void Test()
		{
			Console.WriteLine("Hello, Python !");
		}

	}
}
