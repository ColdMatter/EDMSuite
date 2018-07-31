using System;
using System.Runtime.Remoting;
using System.Threading;
using System.Xml.Serialization;

using DAQ.Environment;
using DAQ.HAL;
using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A plugin that scans a synth's frequency.
	/// </summary>
	[Serializable]
	public class HardwareControllerOutputPlugin : ScanOutputPlugin
	{
	
		[NonSerialized]
		private double scanParameter;
        private string scannedParameter;

        [NonSerialized]
        EDMHardwareControl.Controller hardwareController;

		protected override void InitialiseSettings()
		{
			settings["scannedParameter"] = "rf1frequency";
            settings["dummy"] = "chris";
		}

		public override void AcquisitionStarting()
		{
            // connect to the hardware controller
            try
            {
                if (hardwareController == null) hardwareController = new EDMHardwareControl.Controller();
            }
            catch (Exception e)
            {
                throw new Exception("Unable to create instance of hardware controller!", e);
            }

            if (settings["scannedParameter"] == null) scannedParameter = "rf1frequency";
            else scannedParameter = (String)settings["scannedParameter"];

            hardwareController.ConnectRfAWG();
            hardwareController.EnableRfAWGPulsedGeneration();

		}

		public override void ScanStarting()
        {
		}

		public override void ScanFinished()
		{
		}

		public override void AcquisitionFinished()
		{
            hardwareController.DisableRfAWGPulsedGeneration();
		}

		[XmlIgnore]
		public override double ScanParameter
		{
			set
			{
				scanParameter = value;

                switch (scannedParameter)
                {
                    case "rf1frequency":
                        hardwareController.RfAWGRf1Frequency = scanParameter;
                        break;
                    case "rf2frequency":
                        hardwareController.RfAWGRf2Frequency = scanParameter;
                        break;
                    case "rf1amplitude":
                        hardwareController.RfAWGRf1Amplitude = scanParameter;
                        break;
                    case "rf2amplitude":
                        hardwareController.RfAWGRf2Amplitude = scanParameter;
                        break;
                    default:
                        throw new NotImplementedException("Parameter scanned isn't implemented!");
                }

                hardwareController.UpdateRfAWGPulsedGeneration();

			}
			get { return scanParameter; }
		}

		
	}
}
