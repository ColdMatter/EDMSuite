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
            Dictionary<string, object> rslt = GenericCommandNoReportReply("status", prms);
            return rslt;
        }

        // 3.27
        public int scan_stitch_initialise(string type, double start, double stop, int rate, string units)
        {
            if (start < 1100 || start > 1680) throw new Exception("wavelength out of range");
            if (stop < 1100 || stop > 1680) throw new Exception("wavelength out of range");

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
        public int scan_stitch_op(string type, string op, bool report, bool wait)
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

            if (report)
            {
                prms.Add("report", "finished");

                if (wait)
                {
                    Dictionary<string, object> rslt = GenericCommand("scan_stitch_op", prms, report);
                    if (rslt.Count == 0) return -1;
                    if (report) return ((int)rslt["report"]);
                    else return ((int)rslt["status"]);
                }
                else
                {
                    Dictionary<string, object> rslt = GenericCommandNoReportReply("scan_stitch_op", prms);
                    if (rslt.Count == 0) return -1;
                    else return ((int)rslt["status"]);
                }
            }

            else
            {
                if (wait)
                {
                    Dictionary<string, object> rslt = GenericCommand("scan_stitch_op", prms, report);
                    if (rslt.Count == 0) return -1;
                    if (report) return ((int)rslt["report"]);
                    else return ((int)rslt["status"]);
                }
                else
                {
                    GenericCommandNoReply("scan_stitch_op", prms);
                    return 0;
                }
            }
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
