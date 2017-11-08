using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace DAQ.HAL
{
    public class ICEBlocSolsTiS : ICEBlocRemote
    {

        #region Class members

        private new string my_ip_address;
        private new byte[] my_byte_ip_address;
        private new string M2_ip_address;

        #endregion

        #region Initialization

        public ICEBlocSolsTiS(string computer_ip_address)
        {
            my_ip_address = computer_ip_address;
            string[] ip_split = my_ip_address.Split('.');
            my_byte_ip_address = new byte[ip_split.Length];
            for (int i = 0; i < ip_split.Length; i++)
            {
                my_byte_ip_address[i] = Convert.ToByte(ip_split[i]);
            }
            
            M2_ip_address = "192.168.1.222";
        }

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
            else return "Failed";
        }

        // Following the manual ....
        // 3.1

        public int set_wave_m(double wavelength, bool report)
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            // implement exception
            prms.Add("wavelength", wavelength);
            if (report) prms.Add("report", "finished");

            Dictionary<string, object> rslt = GenericCommand("tune_resonator", prms, report);
            if (rslt.Count == 0) return 2;
            if (report) return ((int)rslt["report"]);
            else return ((int)rslt["status"]);
        }

        // 3.4
        public int stop_wave_m()
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            Dictionary<string, object> rslt = GenericCommand("tune_resonator", prms);
            if (rslt.Count == 0) return 2;
            else return ((int)rslt["status"]);
        }

        #endregion

    }
}
