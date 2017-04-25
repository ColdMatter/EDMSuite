using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ.HAL
{
    /// <summary>
    /// An interface to communicate with the MSquared ICE-BLOC control module.
    /// </summary>
    public interface ICEBLOCCommunicator
    {
        void Configure(string ipAddress, int port);
        void Enable();
        void Disable();
        void Save();
        ICEBLOCMessage SendMessage(ICEBLOCMessage message);
    }

    public class ICEBLOCMessage
    {
        Dictionary<String, Object> parameters;
        string op;
        string transmission_id;

        public ICEBLOCMessage(string op, string transmission_id,Dictionary<string,object> parameters)
        {
            this.op = op;
            this.transmission_id = transmission_id;
            this.parameters = parameters;
        }
        /// <summary>
        /// Checks a reply message to see if it was correctly received
        /// </summary>
        /// <returns></returns>
        public bool MessageStatus()
        {
            if (this.parameters.ContainsKey("status"))
            {
                if (this.parameters["status"] == "ok")
                    return true;
                else
                    return false;
            }
            else
                return true;
        }

    }
}
