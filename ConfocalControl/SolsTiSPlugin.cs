using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DAQ.HAL;

namespace ConfocalControl
{
    public class SolsTiSPlugin
    {
        #region Class members

        // Dependencies should refer to this instance only 
        private static MultiChannelRasterScan controllerInstance;
        public static MultiChannelRasterScan GetController()
        {
            if (controllerInstance == null)
            {
                controllerInstance = new MultiChannelRasterScan();
            }
            return controllerInstance;
        }

        // Settings
        public PluginSettings Settings {get; set;}

        // Laser
        private ICEBlocSolsTiS solstis;
        public ICEBlocSolsTiS Solstis { get { return solstis; } }

        #endregion

        #region Initialization

        public SolsTiSPlugin()
        {
            string computer_ip = "1.1.1";
            solstis = new ICEBlocSolsTiS(computer_ip);
        }

        #endregion

    }
}
