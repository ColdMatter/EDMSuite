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
        private static SolsTiSPlugin controllerInstance;
        public static SolsTiSPlugin GetController()
        {
            if (controllerInstance == null)
            {
                controllerInstance = new SolsTiSPlugin();
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
            string computer_ip = "192.168.1.23";
            solstis = new ICEBlocSolsTiS(computer_ip);
        }

        #endregion

    }
}
