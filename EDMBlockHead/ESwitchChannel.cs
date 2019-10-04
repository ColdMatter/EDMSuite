using System;
using System.Windows.Forms;

using NationalInstruments.DAQmx;

using DAQ.Environment;
using DAQ.HAL;

namespace EDMBlockHead.Acquire.Channels
{
	/// <summary>
	/// A channel to map a modulation to the E-field switches. This channel connects to the
	/// hardware helper and uses it to switch the fields.
	/// </summary>
	public class ESwitchChannel : SwitchedChannel
	{
		public bool Invert;

		private bool currentState = false;
		static private EDMHardwareControl.Controller hardwareController;

		public override bool State
		{
			get
			{
				return currentState;
			}
			set
			{
				currentState = value;
				try
				{
					hardwareController.SwitchEAndWait(value);
				}
				catch (Exception e)
				{
					MessageBox.Show("Unable to switch E." + Environment.NewLine + e, "Switch error ...");
				}
			}
		}

		public override void AcquisitionStarting()
		{
			try
			{
				if (hardwareController == null)	hardwareController = new EDMHardwareControl.Controller();
			}
			catch (Exception e)
			{
				MessageBox.Show("BlockHead can't connect to the " +
					"hardware controller. It won't be able to switch the electric fields. " +
					"Check to make sure it's running." + Environment.NewLine + e, "Connect error ...");
			}
		}

		public override void AcquisitionFinishing()
		{
			// disconnect from the hardware helper
		}
	}
}
