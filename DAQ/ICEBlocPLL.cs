using System;
using System.Collections.Generic;
using UtilsNS;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ.HAL
{
    public class ICEBlocPLL : ICEBlocRemote
    {
        string my_ip_address = "192.168.1.100";
        byte[] my_byte_ip_address = { 192, 168, 1, 100 };
        string M2_ip_address = "192.168.1.228"; //  "echo.websocket.org/"

        // Following the manual ....
        // 3.1. Tune Resonator
        public int tune_resonator(double setting, bool report)
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            if (!Utils.InRange(setting, 0, 100)) new Exception("setting - out of range");
            prms.Add("setting", setting);
            if (report) prms.Add("report", "finished");

            Dictionary<string, object> rslt = GenericCommand("tune_resonator", prms, report);
            if (report) AdjustReport(ref rslt);
            if (rslt.Count == 0) return 2;
            if (report) return ((int)rslt["report"]);
            else return ((int)rslt["status"]);
        }

        // 3.2. Main Lock
        public int main_lock(bool locked, bool report)
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            if (locked) prms.Add("operation", "on");
            else prms.Add("operation", "off");
            if (report) prms.Add("report", "finished");

            Dictionary<string, object> rslt = GenericCommand("main_lock", prms, report);

            if (report) AdjustReport(ref rslt);
            if (rslt.Count == 0) return 2;
            if (report) return ((int)rslt["report"]);
            else return ((int)rslt["status"]);
        }

        // 3.3. Main Lock Status
        public enum Lock_Status
        {
            off, // the lock is off
            on, // the lock is on
            debug, // the lock is in a debug condition
            error, // the lock operation is in error
            search, // the lock search algorithm is active
            low // the lock is off due to low output.
        }

        public bool main_lock_status(out Lock_Status mls)
        {
            Dictionary<string, object> rslt = GenericCommand("main_lock_status", new Dictionary<string, object>());
            
            mls = (Lock_Status)Enum.Parse(typeof(Lock_Status), (string)rslt["condition"], true);
            return (mls == Lock_Status.on);
        }

        // 3.4. Aux Lock
        public int aux_lock(bool locked, bool report)
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            if (locked) prms.Add("operation", "on");
            else prms.Add("operation", "off");
            if (report) prms.Add("report", "finished");

            Dictionary<string, object> rslt = GenericCommand("aux_lock", prms, report);

            if (report) AdjustReport(ref rslt);
            if (rslt.Count == 0) return 2;
            if (report) return ((int)rslt["report"]);
            else return ((int)rslt["status"]);
        }
        // 3.5. Aux Lock Status
        public bool aux_lock_status(out Lock_Status als)
        {
            Dictionary<string, object> rslt = GenericCommand("aux_lock_status", new Dictionary<string, object>());

            als = (Lock_Status)Enum.Parse(typeof(Lock_Status), (string)rslt["condition"], true);
            return ((int)rslt["status"] == 1);
        }

        // 3.6. ECD Lock
        public int ecd_lock(bool locked, bool report)
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            if (locked) prms.Add("operation", "on");
            else prms.Add("operation", "off");
            if (report) prms.Add("report", "finished");

            Dictionary<string, object> rslt = GenericCommand("ecd_lock", prms, report);

            if (report) AdjustReport(ref rslt);
            if (rslt.Count == 0) return 2;
            if (report) return ((int)rslt["report"]);
            else return ((int)rslt["status"]);
        }
        // 3.7. ECD Lock Status
        public bool ecd_lock_status(out Lock_Status els)
        {
            Dictionary<string, object> rslt = GenericCommand("ecd_lock_status", new Dictionary<string, object>());

            els = (Lock_Status)Enum.Parse(typeof(Lock_Status), (string)rslt["condition"], true);
            return ((int)rslt["status"] == 1);
        }

        // 3.8. Select LO Profile
        public int select_lo_profile(int profile, bool report)
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            if (!Utils.InRange(profile, 0, 7)) new Exception("profile out of range");
            prms.Add("profile", profile);
            if (report) prms.Add("report", "finished");

            Dictionary<string, object> rslt = GenericCommand("select_lo_profile", prms, report);

            if (report) AdjustReport(ref rslt);
            if (rslt.Count == 0) return 2;
            if (report) return ((int)rslt["report"]);
            else return ((int)rslt["status"]);
        }

        // 3.9. Configure LO Profile
        public bool configure_lo_profile(bool main_synth, bool aux_synth, string aux_detector_mode, double input_frequency,
            double beat_frequency_trim, double chirp_rate, double chirp_duration, bool report)
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            if (!(main_synth ^ aux_synth)) throw new Exception("Either main_synth or aux_synth must be enabled");
            if (main_synth) prms.Add("main_synth", "enable");
            else prms.Add("main_synth", "disable");
            if (aux_synth) prms.Add("aux_synth", "enable");
            else prms.Add("aux_synth", "disable");
            prms.Add("aux_detector_mode", aux_detector_mode);
            prms.Add("input_frequency", input_frequency);
            prms.Add("beat_frequency_trim", beat_frequency_trim);
            prms.Add("chirp_rate", chirp_rate);
            prms.Add("chirp_duration", chirp_duration);
            if (report) prms.Add("report", "finished");

            Dictionary<string, object> rslt = GenericCommand("configure_lo_profile", prms, report);

            if (report) AdjustReport(ref rslt);
            if (rslt.Count == 0) return false;
            if (report) return ((int)rslt["report"] == 0);
            else return ((int)rslt["status"] == 0);
        }

        // 3.10. Configure AOM
        public bool configure_aom(bool enable, int freq, bool report)
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            if (enable) prms.Add("aom_synth", "enable");
            else prms.Add("aom_synth", "disable");
            prms.Add("frequency", freq);
            if (report) prms.Add("report", "finished");

            Dictionary<string, object> rslt = GenericCommand("configure_aom", prms, report);

            if (report) AdjustReport(ref rslt);
            if (rslt.Count == 0) return false;
            if (report) return ((int)rslt["report"] == 1);
            else return ((int)rslt["status"] == 1);
        }

        public enum MonitorSignal
        {
            AuxLockOutput = 1,
            MainPhaseError = 2,
            IFPhaseError = 3,
            AuxPhaseError = 4,
            EOMOutput = 5,
            M3FastOutput = 6,
            MainInputPower = 7,
            AuxInputPower = 8
        };

        // 3.11. Apply monitor A
        public bool monitor_a(MonitorSignal monitor, bool report)
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            prms.Add("signal", (int)monitor);
            if (report) prms.Add("report", "finished");

            Dictionary<string, object> rslt = GenericCommand("monitor_a", prms, report);

            if (report) AdjustReport(ref rslt);
            if (rslt.Count == 0) return false;
            if (report) return ((int)rslt["report"] == 1);
            else return ((int)rslt["status"] == 1);
        }
        // 3.12. Apply monitor B
        public bool monitor_b(MonitorSignal monitor, bool report)
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            prms.Add("signal", (int)monitor);
            if (report) prms.Add("report", "finished");

            Dictionary<string, object> rslt = GenericCommand("monitor_b", prms, report);

            if (report) AdjustReport(ref rslt);
            if (rslt.Count == 0) return false;
            if (report) return ((int)rslt["report"] == 1);
            else return ((int)rslt["status"] == 1);
        }
        // 3.13. Frequency reference selection
        public bool select_freq_reference(bool external, bool report)
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            if (!external) prms.Add("setting", "internal");
            else prms.Add("setting", "external");
            if (report) prms.Add("report", "finished");

            Dictionary<string, object> rslt = GenericCommand("select_freq_reference", prms, report);

            if (report) AdjustReport(ref rslt);
            if (rslt.Count == 0) return false;
            if (report) return ((int)rslt["report"] == 1);
            else return ((int)rslt["status"] == 1);
        }
        // 3.14. Frequency reference trim
        public bool trim_freq_reference(double value, bool report)
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            prms.Add("setting", value);
            if (report) prms.Add("report", "finished");

            Dictionary<string, object> rslt = GenericCommand("trim_freq_reference", prms, report);

            if (report) AdjustReport(ref rslt);
            if (rslt.Count == 0) return false;
            if (report) return ((int)rslt["report"] == 1);
            else return ((int)rslt["status"] == 1);
        }
        // 3.15. Main LO selection
        public bool select_main_lo(bool external, bool report)
        {
            Dictionary<string, object> prms = new Dictionary<string, object>();
            if (!external) prms.Add("setting", "internal");
            else prms.Add("setting", "external");
            if (report) prms.Add("report", "finished");

            Dictionary<string, object> rslt = GenericCommand("select_main_lo", prms, report);

            if (report) AdjustReport(ref rslt);
            if (rslt.Count == 0) return false;
            if (report) return ((int)rslt["report"] == 1);
            else return ((int)rslt["status"] == 1);
        }
        // 3.16. System Status
        public Dictionary<string, object> get_status()
        {
            Dictionary<string, object> rslt = GenericCommand("get_status", null);
            return rslt;
        }

        public object CommandTest(int index)
        {
            object result = null;
            Lock_Status mls = new Lock_Status();
            bool success = false;
            switch (index)
            {

                case 0:
                    double res_value = 50.0;
                    result = tune_resonator(res_value, false);
                    break;
                case 1:
                    result = main_lock(false, true);
                    break;
                case 2:
                    success = main_lock_status(out mls);
                    if (success) result = "Main Lock - " + mls.ToString();
                    else result = false;
                    break;
                case 3:
                    result = aux_lock(false, true);
                    break;
                case 4:
                    success = aux_lock_status(out mls);
                    if (success) result = "Aux Lock - " + mls.ToString();
                    else result = false;
                    break;
                case 5:
                    result = ecd_lock(false, true);
                    break;
                case 6:
                    success = ecd_lock_status(out mls);
                    if (success) result = "Ecd Lock - " + mls.ToString();
                    else success = false;
                    break;
                case 7:
                    result = select_lo_profile(0, true);
                    break;
                case 8:
                    result = configure_lo_profile(true, false, "ecd", 6834648621.0, 0.0, 500000.0, 0.5, true);
                    break;
                case 9:
                    result = configure_aom(true, 100000000, true);
                    break;
                case 10:
                    MonitorSignal mon_a = MonitorSignal.MainInputPower;
                    result = monitor_a(mon_a, true);
                    break;
                case 11:
                    MonitorSignal mon_b = MonitorSignal.MainPhaseError;
                    result = monitor_b(mon_b, true);
                    break;
                case 12:
                    result = select_freq_reference(false, true);
                    break;
                case 13:
                    result = trim_freq_reference(1.0, true);
                    break;
                case 14:
                    result = select_main_lo(false, true);
                    break;
                case 15:
                    result = get_status();
                    break;
                default:
                    result = null;
                    break;
            }
            return result;
        }
    }
    

}
