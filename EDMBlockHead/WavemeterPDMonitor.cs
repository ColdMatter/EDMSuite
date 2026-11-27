using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;

using DAQ.Environment;
using EDMConfig;

namespace EDMBlockHead.Acquire
{
    /// <summary>
    /// Reads wavemeter frequencies (from the WavemeterLockServer) and laser photodiode voltages
    /// (from UEDMHardwareControl.AcquirePDSnapshot) once per point, the same readings that
    /// ScanMaster's UEDMWavemeterPDPlugin takes. The values go into each point's SinglePointData,
    /// so they end up as detector channels in the saved block.
    ///
    /// This is OFF unless the block config contains the setting "wavemeterChannels" and/or
    /// "pdChannels" (comma separated channel numbers). Without them BlockHead behaves exactly as before.
    ///
    /// Block config settings read here:
    ///   wavemeterChannels   e.g. "1,7"      wavemeter channels to read. Stored as WS{n}Frequency, in THz minus the offset.
    ///   wavemeterComputer   e.g. "WS8SERVERHUXLEY"   computer running the wavemeter lock server (default WS8SERVERHUXLEY)
    ///   wavemeterOffset     e.g. 0.0        subtracted from every wavemeter frequency, THz (default 0)
    ///   pdChannels          e.g. "1,2,3"    photodiodes 1-8 of the hardware controller. Stored as PD{n}Voltage, in V.
    ///   wavemeterPDTimeoutMs e.g. 200       how long to wait for the readings once the PMT gate has been read (default 200)
    ///
    /// If pdChannels is set, the voltage-to-power calibration of each recorded PD is also written
    /// into the block config at the start of the block (PD{n}CalSlope, PD{n}CalIntercept, PD{n}Gain),
    /// together with the name of the laser it monitors (PD{n}Laser, from the hardware controller UI).
    ///
    /// Every point gets the same set of keys. A reading that fails or times out is stored as NaN
    /// (Block.TOFuliseSinglePointData needs every point to have every key).
    /// </summary>
    public class WavemeterPDMonitor
    {
        public const string WavemeterChannelsSetting = "wavemeterChannels";
        public const string WavemeterComputerSetting = "wavemeterComputer";
        public const string WavemeterOffsetSetting = "wavemeterOffset";
        public const string PDChannelsSetting = "pdChannels";
        public const string TimeoutSetting = "wavemeterPDTimeoutMs";

        private const string DefaultWavemeterComputer = "WS8SERVERHUXLEY";
        private const int DefaultTimeoutMs = 200;
        private const int MaxErrorMessagesPerBlock = 5;

        private readonly int[] wavemeterChannels;
        private readonly int[] pdChannels;          // 1-8, as written in the config
        private readonly string wavemeterComputer;
        private readonly double wavemeterOffset;
        private readonly int timeoutMs;
        private readonly UEDMHardwareControl.UEDMController hardwareController;
        private readonly List<string> keys = new List<string>();

        private WavemeterLockServer.Controller wavemeter;
        private System.Threading.Tasks.Task<Dictionary<string, double>> pending;
        private readonly Random fakeRandom = new Random();

        // counters for the block, written into the block config at the end
        private int timeouts;
        private int failedReads;
        private int errorMessages;
        private long slowestReadMs;
        private int readsFinishedAfterPMT;

        /// <summary>
        /// Returns a monitor if the block config asks for wavemeter and/or photodiode readings,
        /// and null otherwise (in which case BlockHead does nothing new).
        /// </summary>
        public static WavemeterPDMonitor FromConfig(BlockConfig config, UEDMHardwareControl.UEDMController hardwareController)
        {
            int[] wm = ParseChannels(GetString(config, WavemeterChannelsSetting), 1, 64, WavemeterChannelsSetting);
            int[] pd = ParseChannels(GetString(config, PDChannelsSetting), 1, 8, PDChannelsSetting);
            if (wm.Length == 0 && pd.Length == 0) return null;
            return new WavemeterPDMonitor(config, hardwareController, wm, pd);
        }

        private WavemeterPDMonitor(BlockConfig config, UEDMHardwareControl.UEDMController hardwareController, int[] wm, int[] pd)
        {
            this.hardwareController = hardwareController;
            wavemeterChannels = wm;
            pdChannels = pd;

            string computer = GetString(config, WavemeterComputerSetting);
            wavemeterComputer = computer == "" ? DefaultWavemeterComputer : computer;
            wavemeterOffset = GetDouble(config, WavemeterOffsetSetting, 0.0);
            timeoutMs = (int)GetDouble(config, TimeoutSetting, DefaultTimeoutMs);

            foreach (int ch in wavemeterChannels) keys.Add(WavemeterKey(ch));
            foreach (int ch in pdChannels) keys.Add(PDKey(ch));

            if (pdChannels.Length > 0) RecordPDCalibration(config);
        }

        /// <summary>
        /// Stores the voltage-to-power calibration of each recorded photodiode in the block config,
        /// as it is in the hardware controller when the block starts (including any edits made on its UI):
        ///   PD{n}CalSlope (mW/V), PD{n}CalIntercept (mW), with Power (mW) = slope * PD{n}Voltage + intercept,
        ///   PD{n}Gain (the amplifier gain the calibration was taken at) and PD{n}Laser (the monitored laser).
        /// NaN = that PD is not calibrated. Never throws: if the hardware controller can't be reached,
        /// the hardcoded default calibrations are stored and PDCalibrationSource says so.
        /// </summary>
        private void RecordPDCalibration(BlockConfig config)
        {
            double[] slopes = null;
            double[] intercepts = null;
            string[] laserNames = null;
            string source = "UEDMHardwareControl (live)";
            if (!Environs.Debug)
            {
                try
                {
                    slopes = hardwareController.GetPDCalibrationSlopes();
                    intercepts = hardwareController.GetPDCalibrationIntercepts();
                }
                catch (Exception e)
                {
                    slopes = null;
                    intercepts = null;
                    Console.WriteLine("Wavemeter/PD monitor: could not get the PD calibrations from the hardware controller, storing the defaults. " + e.Message);
                }
                // separate try, so an older hardware controller without the names still gives the calibrations
                try
                {
                    laserNames = hardwareController.GetPDLaserNames();
                }
                catch (Exception e)
                {
                    laserNames = null;
                    Console.WriteLine("Wavemeter/PD monitor: could not get the PD laser names from the hardware controller, storing 'unknown'. " + e.Message);
                }
            }
            if (slopes == null || intercepts == null)
            {
                slopes = UEDMHardwareControl.UEDMController.PDDefaultCalSlope;
                intercepts = UEDMHardwareControl.UEDMController.PDDefaultCalIntercept;
                source = "UEDMHardwareControl defaults";
            }
            string[] gains = UEDMHardwareControl.UEDMController.PDGainSettings;

            config.Settings["PDCalibrationSource"] = source;
            foreach (int ch in pdChannels)
            {
                int i = ch - 1;
                config.Settings["PD" + ch + "CalSlope"] = i < slopes.Length ? slopes[i] : double.NaN;
                config.Settings["PD" + ch + "CalIntercept"] = i < intercepts.Length ? intercepts[i] : double.NaN;
                if (i < gains.Length) config.Settings["PD" + ch + "Gain"] = gains[i];
                config.Settings["PD" + ch + "Laser"] = (laserNames != null && i < laserNames.Length && !string.IsNullOrEmpty(laserNames[i])) ? laserNames[i] : "unknown";
            }
        }

        public static string WavemeterKey(int channel) { return "WS" + channel + "Frequency"; }
        public static string PDKey(int channel) { return "PD" + channel + "Voltage"; }

        /// <summary>
        /// Connects to the wavemeter lock server. Call once at the start of the block.
        /// Never throws: if the server can't be reached the wavemeter values are stored as NaN.
        /// </summary>
        public void Connect()
        {
            Console.WriteLine("Wavemeter/PD monitor ON. Recording: " + string.Join(", ", keys)
                + ". Timeout " + timeoutMs + " ms after the PMT gate.");
            if (Environs.Debug || wavemeterChannels.Length == 0) return;
            try
            {
                // same connection as ScanMaster's UEDMWavemeterPDPlugin
                EnvironsHelper eHelper = new EnvironsHelper(wavemeterComputer);
                string address = null;
                foreach (IPAddress a in Dns.GetHostEntry(wavemeterComputer).AddressList)
                {
                    if (a.AddressFamily == AddressFamily.InterNetwork) { address = a.ToString(); break; }
                }
                if (address == null) throw new Exception("no IPv4 address found for " + wavemeterComputer);
                string url = "tcp://" + address + ":" + eHelper.serverTCPChannel + "/controller.rem";
                wavemeter = (WavemeterLockServer.Controller)Activator.GetObject(typeof(WavemeterLockServer.Controller), url);
                Console.WriteLine("Wavemeter/PD monitor: wavemeter server at " + url);
            }
            catch (Exception e)
            {
                wavemeter = null;
                Console.WriteLine("Wavemeter/PD monitor: could not connect to the wavemeter server on "
                    + wavemeterComputer + ". Wavemeter values will be NaN for this block. " + e.Message);
            }
        }

        /// <summary>
        /// Starts a reading in the background and returns straight away. Call it when the PMT read starts.
        /// If the previous reading is still running (e.g. a hung remoting call, or a retaken shot)
        /// it is reused rather than starting another one, so calls never pile up.
        /// </summary>
        public System.Threading.Tasks.Task<Dictionary<string, double>> StartRead()
        {
            if (pending != null && !pending.IsCompleted) return pending;
            pending = System.Threading.Tasks.Task.Run(() => ReadAll());
            return pending;
        }

        /// <summary>
        /// Waits (at most the timeout) for a reading started with StartRead and returns its values.
        /// Values that are missing, failed or timed out are NaN. Never throws.
        /// </summary>
        public Dictionary<string, double> Collect(System.Threading.Tasks.Task<Dictionary<string, double>> read, bool finishedBeforePMT)
        {
            if (!finishedBeforePMT) readsFinishedAfterPMT++;
            if (read == null) return NaNValues();
            try
            {
                if (read.Wait(timeoutMs)) return read.Result;
            }
            catch (Exception e)
            {
                Report("Wavemeter/PD monitor: read failed. " + e.Message);
                failedReads++;
                return NaNValues();
            }
            timeouts++;
            Report("Wavemeter/PD monitor: reading took longer than the timeout (" + timeoutMs + " ms after the PMT gate). Storing NaN for this point.");
            return NaNValues();
        }

        /// <summary>
        /// Made-up values for Debug mode (no hardware calls).
        /// </summary>
        public Dictionary<string, double> FakeValues()
        {
            Dictionary<string, double> values = new Dictionary<string, double>();
            foreach (int ch in wavemeterChannels) values[WavemeterKey(ch)] = 500.0 + 0.001 * fakeRandom.NextDouble();
            foreach (int ch in pdChannels) values[PDKey(ch)] = fakeRandom.NextDouble();
            return values;
        }

        /// <summary>
        /// Prints a summary and stores it in the block config (saved inside the block zip).
        /// </summary>
        public void BlockFinished(BlockConfig config)
        {
            config.Settings["wavemeterPDTimeouts"] = timeouts;
            config.Settings["wavemeterPDFailedReads"] = failedReads;
            config.Settings["wavemeterPDSlowestReadMs"] = (int)slowestReadMs;
            config.Settings["wavemeterPDReadsAfterPMT"] = readsFinishedAfterPMT;
            Console.WriteLine("Wavemeter/PD monitor: block done. Slowest read " + slowestReadMs + " ms, "
                + readsFinishedAfterPMT + " reads finished after the PMT gate, "
                + timeouts + " timeouts, " + failedReads + " failed reads.");
        }

        // runs on a thread-pool thread
        private Dictionary<string, double> ReadAll()
        {
            Stopwatch sw = Stopwatch.StartNew();
            Dictionary<string, double> values = NaNValues();

            if (wavemeterChannels.Length > 0 && wavemeter != null)
            {
                foreach (int ch in wavemeterChannels)
                {
                    try
                    {
                        double f = wavemeter.getFrequency(ch);
                        // the wavemeter returns zero or a negative error code when it has no good reading
                        if (f > 0) values[WavemeterKey(ch)] = f - wavemeterOffset;
                        else
                        {
                            failedReads++;
                            Report("Wavemeter/PD monitor: wavemeter channel " + ch + " returned error code " + f + ". Storing NaN.");
                        }
                    }
                    catch (Exception e)
                    {
                        failedReads++;
                        Report("Wavemeter/PD monitor: wavemeter channel " + ch + " read failed. " + e.Message);
                    }
                }
            }

            if (pdChannels.Length > 0)
            {
                try
                {
                    // note: the hardware controller returns 1.0 for a PD whose DAQ read failed (see UEDMController.ReadAnalogInput)
                    double[] v = hardwareController.AcquirePDSnapshot(null, null).Voltages;
                    foreach (int ch in pdChannels)
                    {
                        if (v != null && ch - 1 < v.Length) values[PDKey(ch)] = v[ch - 1];
                    }
                }
                catch (Exception e)
                {
                    failedReads++;
                    Report("Wavemeter/PD monitor: photodiode read failed. " + e.Message);
                }
            }

            sw.Stop();
            if (sw.ElapsedMilliseconds > slowestReadMs) slowestReadMs = sw.ElapsedMilliseconds;
            Console.WriteLine("Wavemeter/PD read took " + sw.ElapsedMilliseconds + " ms.");
            return values;
        }

        private Dictionary<string, double> NaNValues()
        {
            Dictionary<string, double> values = new Dictionary<string, double>();
            foreach (string k in keys) values[k] = double.NaN;
            return values;
        }

        // limit the console output if something keeps failing for the whole block
        private void Report(string message)
        {
            errorMessages++;
            if (errorMessages <= MaxErrorMessagesPerBlock) Console.WriteLine(message);
            else if (errorMessages == MaxErrorMessagesPerBlock + 1)
                Console.WriteLine("Wavemeter/PD monitor: more errors, not printing them. The count is in the summary at the end of the block.");
        }

        private static string GetString(BlockConfig config, string key)
        {
            if (!config.Settings.Contains(key) || config.Settings[key] == null) return "";
            return config.Settings[key].ToString().Trim();
        }

        private static double GetDouble(BlockConfig config, string key, double defaultValue)
        {
            string s = GetString(config, key);
            if (s == "") return defaultValue;
            double d;
            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out d)) return d;
            if (double.TryParse(s, out d)) return d;
            Console.WriteLine("Wavemeter/PD monitor: could not read setting " + key + " = '" + s + "'. Using " + defaultValue + ".");
            return defaultValue;
        }

        // "1, 7" -> {1, 7}. Ignores blanks, duplicates and numbers outside [min, max] (with a warning).
        private static int[] ParseChannels(string s, int min, int max, string settingName)
        {
            List<int> channels = new List<int>();
            if (s == "") return channels.ToArray();
            foreach (string part in s.Split(','))
            {
                string p = part.Trim();
                if (p == "") continue;
                int ch;
                if (int.TryParse(p, out ch) && ch >= min && ch <= max)
                {
                    if (!channels.Contains(ch)) channels.Add(ch);
                }
                else Console.WriteLine("Wavemeter/PD monitor: ignoring '" + p + "' in " + settingName + " (must be a whole number " + min + "-" + max + ").");
            }
            return channels.ToArray();
        }
    }
}
