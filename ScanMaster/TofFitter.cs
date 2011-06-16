using System;
using DAQ.Environment;

namespace ScanMaster.Analyze
{
    /// <summary>
    /// Extends the GaussianFitter class. Uses the time-of-arrival and width information
    /// to return a mean speed and translational temperature in the ParameterReport.
    /// Requires a "sourceToDetect" and a "moleculeMass" in the Hardware.Info.
    /// </summary>
    class TofFitter : ScanMaster.Analyze.GaussianFitter
    {
        public TofFitter()
        {
            Name = "TOF";
            model = gaussian;
        }

        public override string ParameterReport
        {
            get
            {
                string returnString = "n: " + lastFittedParameters[0].ToString("G3") +
                    " q: " + lastFittedParameters[1].ToString("G3") +
                    " c: " + lastFittedParameters[2].ToString("G6") +
                    " w: " + lastFittedParameters[3].ToString("G3");
               
                if (Environs.Hardware.GetInfo("sourceToDetect") != null && Environs.Hardware.GetInfo("moleculeMass") != null)
                {
                    double distance = (double)Environs.Hardware.GetInfo("sourceToDetect");
                    double mass = (double)Environs.Hardware.GetInfo("moleculeMass");
                    double speed = 1000000 * (double)Environs.Hardware.GetInfo("sourceToDetect") / lastFittedParameters[2];
                    double temperature = 2.169 * 10000000 * mass * Math.Pow(distance, 2) * Math.Pow(lastFittedParameters[3], 2) / Math.Pow(lastFittedParameters[2], 4);
                    returnString = returnString + Environment.NewLine + "v: " + speed.ToString("G4") + " m/s" +
                    " T: " + temperature.ToString("G3") + " K";
                }

                return returnString;
            }
        }
    }
}
