using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace DAQ.HAL
{
    public class ICEBlocDFG : ICEBlocRemote
    {

        #region Initialization

        public ICEBlocDFG()
            : base("192.168.1.223")
        { }

        #endregion

        #region Laser control methods

        public string StartLink(int foo)
        {
            Dictionary<string, object> prmsOut = new Dictionary<string, object>();
            prmsOut.Add("ip_address", my_ip_address);
            Dictionary<string, object> prmsIn = GenericCommand("start_link", prmsOut);

            if (prmsIn.Count > 0) return (string)prmsIn["status"];
            else return "failed";
        }

        public string PingTest(string foo)
        {
            Dictionary<string, object> prmsOut = new Dictionary<string, object>();
            prmsOut.Add("text_in", foo);
            Dictionary<string, object> prmsIn = GenericCommand("ping", prmsOut);
            if (prmsIn.Count != 0) return (string)prmsIn["text_out"];
            else return "failed";
        }

        // Following the manual ....

        // 3.1
        public int wavelength(string beam, double wavelength, bool report)
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            // “visible” or “infrared”
            prms.Add("beam", "mir");
            prms.Add("target", wavelength);
            if (report) prms.Add("report", "finished");

            Dictionary<string, object> rslt = GenericCommand("wavelength", prms, report);
            if (rslt.Count == 0) return -1;
            if (report) return ((int)rslt["report"]);
            else return ((int)rslt["status"]);
        }

        // 3.2
        public Dictionary<string, object> status()
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            Dictionary<string, object> rslt = GenericCommand("status", prms);
            return rslt;
        }

        #endregion
    }
}
