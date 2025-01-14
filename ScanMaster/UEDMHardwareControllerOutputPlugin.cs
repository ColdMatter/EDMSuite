#if ultracoldEDM
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
	/// A plugin that scans a hardware controller parameter.
	/// </summary>
	[Serializable]
	public class UEDMHardwareControllerOutputPlugin : ScanOutputPlugin
	{
	
		[NonSerialized]
		private double scanParameter;
        private string scannedParameter;

        [NonSerialized]
        UEDMHardwareControl.UEDMController hardwareController;

		protected override void InitialiseSettings()
		{
			settings["scannedParameter"] = "efield";
            settings["dummy"] = "fred";
		}

		public override void AcquisitionStarting()
		{
            // connect to the hardware controller
            try
            {
                if (hardwareController == null) hardwareController = new UEDMHardwareControl.UEDMController();
            }
            catch (Exception e)
            {
                throw new Exception("Unable to create instance of hardware controller!", e);
            }

            if (settings["scannedParameter"] == null) scannedParameter = "efield";
            else scannedParameter = (String)settings["scannedParameter"];

		}

		public override void ScanStarting()
        {
		}

		public override void ScanFinished()
		{
		}

		public override void AcquisitionFinished()
		{
		}

		[XmlIgnore]
		public override double ScanParameter
		{
			set
			{
				scanParameter = value;

                switch (scannedParameter)
                {
                    case "efield":
                        hardwareController.SetCPlusVoltage(scanParameter);
						hardwareController.SetCMinusVoltage(scanParameter);
                        break;
                    default:
                        throw new NotImplementedException("Parameter scanned isn't implemented!");
                }

                hardwareController.UpdateVoltages();

			}
			get { return scanParameter; }
		}

		
	}
}
#endif