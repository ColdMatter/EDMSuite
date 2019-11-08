using System;
using System.Windows.Forms;

using NationalInstruments.DAQmx;

using DAQ.Environment;
using DAQ.HAL;
using EDMConfig;

namespace EDMBlockHead.Acquire.Channels
{
	/// <summary>
	/// A channel to map a modulation to the E-field switches. This channel connects to the
	/// hardware helper and uses it to switch the fields. It also changes one or both of the rf
    /// fm voltages.
	/// </summary>
	public class ESwitchRFChannel : SwitchedChannel
	{
		public bool Invert;
        public enum RFSwitchToModify { rf1/*, rf2, both*/ };    // Code is commented out so don't make a mistake
                                                                // otherwise there is nothing wrong with it
        public RFSwitchToModify rfToModify;
        public double stepSizeRF;
        private double initialCentre1;
        //private double initialCentre2;

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
                    if (rfToModify == RFSwitchToModify.rf1)
                    {
                        AnalogModulation rfMod = (AnalogModulation)(Controller.GetController().Config.GetModulationByName("RF1F"));
                        rfMod.Centre = initialCentre1 + (value ? stepSizeRF : -stepSizeRF);
                    }
                    //else if (rfToModify == RFSwitchToModify.rf2)
                    //{
                    //    AnalogModulation rfMod = (AnalogModulation)(Controller.GetController().Config.GetModulationByName("RF2F"));
                    //    rfMod.Centre = initialCentre2 - (value ? stepSizeRF : -stepSizeRF);
                    //}
                    //else if (rfToModify == RFSwitchToModify.both)
                    //{
                    //    AnalogModulation rfMod1 = (AnalogModulation)(Controller.GetController().Config.GetModulationByName("RF1F"));
                    //    rfMod1.Centre = initialCentre1 - (value ? stepSizeRF : -stepSizeRF);
                    //    AnalogModulation rfMod2 = (AnalogModulation)(Controller.GetController().Config.GetModulationByName("RF2F"));
                    //    rfMod2.Centre = initialCentre2 - (value ? stepSizeRF : -stepSizeRF);
                    //}

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
                if (rfToModify == RFSwitchToModify.rf1)
                {
                    initialCentre1 = ((AnalogModulation)Controller.GetController().Config.GetModulationByName("RF1F")).Centre;
                }
                //else if (rfToModify == RFSwitchToModify.rf2)
                //{
                //    initialCentre2 = ((AnalogModulation)Controller.GetController().Config.GetModulationByName("RF2F")).Centre;
                //}
                //else if (rfToModify == RFSwitchToModify.both)
                //{
                //    initialCentre1 = ((AnalogModulation)Controller.GetController().Config.GetModulationByName("RF1F")).Centre;
                //    initialCentre2 = ((AnalogModulation)Controller.GetController().Config.GetModulationByName("RF2F")).Centre;
                //}
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
