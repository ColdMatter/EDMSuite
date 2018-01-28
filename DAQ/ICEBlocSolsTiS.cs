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
            else return "failed";
        }

        // Following the manual ....

        // 3.1
        public int set_wave_m(double wavelength, bool report)
        {
            if (wavelength < 700 || wavelength > 1000) throw new Exception("wavelength out of range");

            Dictionary<string, object> prms = new Dictionary<string, object>();
            prms.Add("wavelength", wavelength);
            if (report) prms.Add("report", "finished");

            Dictionary<string, object> rslt = GenericCommand("set_wave_m", prms, report);
            if (rslt.Count == 0) return -1;
            if (report) return ((int)rslt["report"]);
            else return ((int)rslt["status"]);
        }

        // 3.2
        public Dictionary<string, object> poll_wave_m()
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            Dictionary<string, object> rslt = GenericCommand("poll_wave_m", prms);
            return rslt;
        }

        // 3.3
        public int lock_wave_m(bool request_condition)
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            if (request_condition) prms.Add("operation", "on");
            else prms.Add("operation", "off");

            Dictionary<string, object> rslt = GenericCommand("etalon_lock", prms);
            if (rslt.Count == 0) return -1;
            else return ((int)rslt["status"]);
        }

        // 3.4
        public Dictionary<string, object> stop_wave_m()
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            Dictionary<string, object> rslt = GenericCommand("stop_wave_m", prms);
            return rslt;
        }

        // 3.5
        public int move_wave_t(double wavelength, bool report)
        {
            if (wavelength < 700 || wavelength > 1000) throw new Exception("wavelength out of range");

            Dictionary<string, object> prms = new Dictionary<string, object>();
            prms.Add("wavelength", wavelength);
            if (report) prms.Add("report", "finished");

            Dictionary<string, object> rslt = GenericCommand("move_wave_t", prms, report);
            if (rslt.Count == 0) return -1;
            if (report) return ((int)rslt["report"]);
            else return ((int)rslt["status"]);
        }

        // 3.6
        public Dictionary<string, object> poll_move_wave_t()
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            Dictionary<string, object> rslt = GenericCommand("poll_move_wave_t", prms);
            return rslt;
        }

        // 3.7
        public Dictionary<string, object> stop_move_wave_t()
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            Dictionary<string, object> rslt = GenericCommand("stop_move_wave_t", prms);
            return rslt;
        }

        // 3.11
        public int tune_resonator(double percentage, bool report)
        {
            if (percentage < 0 || percentage > 100) throw new Exception("resonator setting out of range");

            Dictionary<string, object> prms = new Dictionary<string, object>();
            prms.Add("setting", percentage);
            if (report) prms.Add("report", "finished");

            Dictionary<string, object> rslt = GenericCommand("tune_resonator", prms, report);
            if (rslt.Count == 0) return -1;
            if (report) return ((int)rslt["report"]);
            else return ((int)rslt["status"]);
        }

        // 3.13
        public int etalon_lock(bool request_condition, bool report)
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            if (request_condition) prms.Add("operation", "on");
            else prms.Add("operation", "off");
            if (report) prms.Add("report", "finished");

            Dictionary<string, object> rslt = GenericCommand("etalon_lock", prms, report);
            if (rslt.Count == 0) return -1;
            if (report) return ((int)rslt["report"]);
            else return ((int)rslt["status"]);
        }

        // 3.14
        public Dictionary<string, object> etalon_lock_status()
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            Dictionary<string, object> rslt = GenericCommand("etalon_lock_status", prms);
            return rslt;
        }

        // 3.19
        public int monitor_a(int signal, bool report)
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            prms.Add("signal", signal);
            if (report) prms.Add("report", "finished");

            Dictionary<string, object> rslt = GenericCommand("monitor_a", prms, report);
            if (rslt.Count == 0) return -1;
            if (report) return ((int)rslt["report"]);
            else return ((int)rslt["status"]);
        }

        // 3.27
        public int scan_stitch_initialise(string type, double start, double stop, int rate, string units)
        {
            if (start < 700 || start > 1000) throw new Exception("wavelength out of range");
            if (stop < 700 || stop > 1000) throw new Exception("wavelength out of range");

            Dictionary<string, object> prms = new Dictionary<string, object>();
            switch (type)
            {
                case "medium":
                    prms.Add("scan", type);
                    prms.Add("start", start);
                    prms.Add("stop", stop); 
                    switch (units)
                    {
                        case "GHz/s":
                            if (rate == 100 || rate == 50 || rate == 20 || rate == 15 || rate == 10 || rate == 5 || rate == 2 || rate == 1) 
                            {
                                prms.Add("rate", rate);
                            }
                            else throw new Exception("Tera scan rate not accepted");
                            prms.Add("units", units);
                            break;
                        default:
                            throw new Exception("Cannot understand tera scan rate");
                    }
                    break;
                case "fine":
                    prms.Add("scan", type);
                    prms.Add("start", start);
                    prms.Add("stop", stop); 
                    switch (units)
                    {
                        case "GHz/s":
                            if (rate == 20 || rate == 15 || rate == 10 || rate == 5 || rate == 2 || rate == 1) 
                            {
                                prms.Add("rate", rate);
                            }
                            else throw new Exception("Tera scan rate not accepted");
                            prms.Add("units", units);
                            break;
                        case "MHz/s":
                            if (rate == 500 || rate == 200 || rate == 100 || rate == 50 || rate == 20 || rate == 15 || rate == 10 || rate == 5 || rate == 2 || rate == 1) 
                            {
                                prms.Add("rate", rate);
                            }
                            else throw new Exception("Tera scan rate not accepted");
                            prms.Add("units", units);
                            break;
                        default:
                            throw new Exception("Cannot understand tera scan rate");
                    }
                    break;
                default:
                    throw new Exception("Cannot understand tera scan type");
            }

            Dictionary<string, object> rslt = GenericCommand("scan_stitch_initialise", prms);
            if (rslt.Count == 0) return -1;
            else return ((int)rslt["status"]);
        }

        // 3.28
        public int scan_stitch_op(string type, string op, bool report)
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            switch (type)
            {
                case "medium":
                    prms.Add("scan", type);
                    break;
                case "fine":
                    prms.Add("scan", type);
                    break;
                default:
                    throw new Exception("Cannot understand tera scan type");
            }
            switch (op)
            {
                case "start":
                    prms.Add("operation", op);
                    break;
                case "stop":
                    prms.Add("operation", op);
                    break;
                default:
                    throw new Exception("Cannot understand tera scan operation");
            }
            if (report) prms.Add("report", "finished");

            Dictionary<string, object> rslt = GenericCommand("scan_stitch_op", prms, report);
            if (rslt.Count == 0) return -1;
            if (report) return ((int)rslt["report"]);
            else return ((int)rslt["status"]);
        }

        // 3.29
        public Dictionary<string, object> scan_stitch_status(string type)
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            switch (type)
            {
                case "medium":
                    prms.Add("scan", type);
                    break;
                case "fine":
                    prms.Add("scan", type);
                    break;
                default:
                    throw new Exception("Cannot understand tera scan type");
            }
            return GenericCommand("scan_stitch_status", prms);
        }

        // 3.31
        public int terascan_output(string op, int delay, int update, string pause)
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            switch (op)
            {
                case "start":
                    prms.Add("operation", op);
                    break;
                case "stop":
                    prms.Add("operation", op);
                    break;
                default:
                    throw new Exception("Cannot understand automatic output operation");
            }
            if (delay < 0 || delay > 1000) throw new Exception("Automatic output delay out of bounds");
            else prms.Add("delay", delay);
            if (update < 0 || update > 50) throw new Exception("Automatic output update rate out of bounds");
            else prms.Add("update", update);
            switch (pause)
            {
                case "on":
                    prms.Add("pause", pause);
                    break;
                case "off":
                    prms.Add("pause", pause);
                    break;
                default:
                    throw new Exception("Cannot understand automatic output operation");
            }

            Dictionary<string, object> rslt = GenericCommand("terascan_output", prms);
            if (rslt.Count == 0) return -1;
            else return ((int)rslt["status"]);
        }

        // 3.32
        public int fast_scan_start(string scan, double width, double time, bool report)
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            prms.Add("scan", scan);
            prms.Add("width", width);
            prms.Add("time", time);
            if (report) prms.Add("report", "finished");

            Dictionary<string, object> rslt = GenericCommand("fast_scan_start", prms, report);
            if (rslt.Count == 0) return -1;
            if (report) return ((int)rslt["report"]);
            else return ((int)rslt["status"]);
        }

        // 3.34
        public int fast_scan_stop(string scan, bool report)
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            prms.Add("scan", scan);
            if (report) prms.Add("report", "finished");

            Dictionary<string, object> rslt = GenericCommand("fast_scan_stop", prms, report);
            if (rslt.Count == 0) return -1;
            if (report) return ((int)rslt["report"]);
            else return ((int)rslt["status"]);
        }

        // 3.39
        public int terascan_continue()
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            Dictionary<string, object> rslt = GenericCommand("terascan_continue", prms);
            if (rslt.Count == 0) return -1;
            else return ((int)rslt["status"]);
        }

        #endregion

    }
}
