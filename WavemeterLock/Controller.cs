using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.HAL;
using DAQ.Environment;
using DAQ.WavemeterLock;
using System.Threading;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;

namespace WavemeterLock
{
    public class Controller : MarshalByRefObject
    {

        private string computer;
        private string name;
        string thisComputerName;
        private int hostTCPChannel;
        public int loopcount = 0;
        private WavemeterLockServer.Controller wavemeterContrller;
        public int colorParameter = 0;
        private double freqTolerance = 0.5;//Frequency jump tolerance in THz
        string faultyLaser;
        public double updateRate = 100;
        Stopwatch stopWatch = new Stopwatch();
        Stopwatch stopWatchGraph = new Stopwatch();
        private List<double> scanTimes;
        public int numScanAverages = 5;


        #region Set up TCP channel and remote methods

        public void initializeTCPChannel()
        {
            //computer = "IC-CZC136CFDJ"; //Computer name of the server
            EnvironsHelper eHelper = new EnvironsHelper(computer);
            //hostTCPChannel = eHelper.serverTCPChannel;
            foreach (var addr in Dns.GetHostEntry(computer).AddressList)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    name = addr.ToString();
                }
            }

            try
            {
                wavemeterContrller = (WavemeterLockServer.Controller)(Activator.GetObject(typeof(WavemeterLockServer.Controller), "tcp://" + name + ":" + hostTCPChannel.ToString() + "/controller.rem"));
            }

            catch (Exception e)
            {
                connectionError(e);
            }
            //Register in remote server
            thisComputerName = Environment.MachineName.ToString();

            try
            {
                wavemeterContrller.registerWavemeterLock(thisComputerName);
            }
            catch (Exception e)
            {
                connectionError(e);
            }
            //This throws a security exception
            //wavemeterContrller.measurementAcquired += () => { updateLockMaster(); };
            ui.Log("TCP channel initialised: server=" + computer + " (" + name + "), port=" + hostTCPChannel.ToString());
        }

        public string acquireWavelength(int channelNum) //Display wavelength
        {
            string display = null;

            display = wavemeterContrller.displayWavelength(channelNum);

            return display;

        }

        public double getWavelength(int channelNum) //Return wavelength in nm
        {


            return wavemeterContrller.getWavelength(channelNum);

        }

        public string acquireFrequency(int channelNum) //Display frequency
        {

            return wavemeterContrller.displayFrequency(channelNum);

        }

        public double getFrequency(int channelNum) //Return frequency in THz
        {


            return wavemeterContrller.getFrequency(channelNum);

        }

        public double getSetFrequency(string laser)
        {
            return lasers[laser].setFrequency;
        }

        //Remote Methods
        public void setSlaveFrequency(string name, double freq)
        {
            lasers[name].setFrequency = freq;
        }

        public double getSlaveFrequency(string name)
        {
            return lasers[name].currentFrequency;
        }

        public void setSlaveVoltage(string name, double v)
        {
            lasers[name].CurrentVoltage = v;
        }

        public double getSlaveVoltage(string name)
        {
            return lasers[name].CurrentVoltage;
        }

        public void updateSetpoint(string name)
        {
        }

        #endregion

        public Dictionary<string, Laser> lasers;
        public Dictionary<string, int> wmChannels;
        public Dictionary<string, bool> lockBlocked;
        public Dictionary<string, bool> lastMeasurementLockBlocked;
        public Dictionary<string, DAQMxWavemeterLockLaserControlHelper> helper = new Dictionary<string, DAQMxWavemeterLockLaserControlHelper>();
        public Dictionary<string, DAQMxWavemeterLockBlockHelper> blockHelper = new Dictionary<string, DAQMxWavemeterLockBlockHelper>();
        public Dictionary<string, LockControlPanel> panelList = new Dictionary<string, LockControlPanel>();
        public Dictionary<string, double> timeList = new Dictionary<string, double>();
        private LockForm ui;
        public WavemeterLockConfig config;

        private static string logfilePath = (string)Environs.FileSystem.Paths["wavemeterLockData"];

        //Constructor
        public Controller(string configName, string hostName, int channelNumber)
        {
            config = (WavemeterLockConfig)Environs.Hardware.GetInfo(configName);
            computer = hostName;
            hostTCPChannel = channelNumber;
        }

        public Color selectColor(int par)//Assign a plot line color for each laser
        {
            switch (par)
            {
                case 0:
                    return Color.FromName("Yellow");

                case 1:
                    return Color.FromName("Lime");

                case 2:
                    return Color.FromName("Red");

                case 3:
                    return Color.FromName("Blue");

                case 4:
                    return Color.FromName("Pink");

                case 5:
                    return Color.FromName("Cyan");

                case 6:
                    return Color.FromName("Aqua");

                default:
                    return Color.FromName("White");


            }

        }


        public void start()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ui = new LockForm();
            ui.controller = this;
            initializeTCPChannel();
            initializeLasers();
            Application.Run(ui);
        }

        public void startWML()
        {
            Thread mainThread = new Thread(new ThreadStart(mainLoop));
            mainThread.Start(); 
            Thread blockThread = new Thread(new ThreadStart(checkBlockLoop));
            blockThread.Start();
        }


        public void indicateRemoteConnection(int channelNum, bool status)
        {
            wavemeterContrller.changeConnectionStatus(channelNum, status);
        }

        public void initializeLasers()
        {
            lasers = new Dictionary<string, Laser>();
            lockBlocked = new Dictionary<string, bool>();
            lastMeasurementLockBlocked = new Dictionary<string, bool>();
            wmChannels = new Dictionary<string, int>();

            //Config hardware channel and time stamp
            foreach (string slaveLaser in config.slaveLasers.Keys)
            {
                helper.Add(slaveLaser, new DAQMxWavemeterLockLaserControlHelper(config.slaveLasers[slaveLaser]));
                timeList.Add(slaveLaser, 0);
            }

            //Create instances of class Laser, register lasers in controller's dictionary
            foreach (KeyValuePair<string, string> entry in config.slaveLasers)
            {
                string laser = entry.Key;
                Laser slave = new Laser(laser, entry.Value, helper[laser])
                {
                    OnLog = ui.Log,
                    WLMChannel = config.channelNumbers[laser]
                };
                lasers.Add(laser, slave);
                wmChannels.Add(laser, slave.WLMChannel);
            }

            //Config lock block
            foreach (KeyValuePair<string, string> entry in config.lockBlockFlag)
            {
                string laser = entry.Key;
                blockHelper.Add(laser, new DAQMxWavemeterLockBlockHelper(laser, config.lockBlockFlag[laser]));
                lockBlocked.Add(laser, false);
                lastMeasurementLockBlocked.Add(laser, false);
            }

            //Config initial set points and gains
            foreach (KeyValuePair<string, double> entry in config.setPoints)
            {
                string laser = entry.Key;
                lasers[laser].setFrequency = config.setPoints[laser];
            }

            foreach (KeyValuePair<string, double> entry in config.pGains)
            {
                string laser = entry.Key;
                lasers[laser].PGain = config.pGains[laser];
            }

            foreach (KeyValuePair<string, double> entry in config.IGains)
            {
                string laser = entry.Key;
                lasers[laser].IGain = config.IGains[laser];
            }

            foreach (string slaveLaser in config.slaveLasers.Keys)
            {
                ui.AddLaserControlPanel(slaveLaser, config.slaveLasers[slaveLaser], config.channelNumbers[slaveLaser]);
            }

            //Asign an LED to each laser
            foreach (int n in wmChannels.Values)
            {
                ui.enable_LED(n);
            }

            //Restore last output voltages if available
            string voltageLogPath = System.IO.Path.Combine(logfilePath, "last_voltage_log.txt");
            if (System.IO.File.Exists(voltageLogPath))
            {
                foreach (string line in System.IO.File.ReadLines(voltageLogPath))
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("Timestamp"))
                        continue;
                    string[] parts = line.Split('\t');
                    if (parts.Length < 3)
                        continue;
                    string laserName = parts[1];
                    if (lasers.ContainsKey(laserName) && double.TryParse(parts[2], out double voltage))
                    {
                        lasers[laserName].CurrentVoltage = voltage;
                        lasers[laserName].offsetVoltage = voltage;
                        ui.Log("Restored voltage for " + laserName + ": " + voltage.ToString("F4") + " V");
                    }
                }
            }

        }

        public enum ControllerState
        {
            STOPPED, RUNNING
        };

        public ControllerState WMLState = ControllerState.STOPPED;


        #region methods


        //Displace the wavelength of the target laser.
        public string displayWL(int channelnum)
        {

            if (channelnum > 0 & channelnum < 9)
                return acquireWavelength(channelnum);

            else if (channelnum == 0)
                return "Off";

            else
                return "Error";
        }

        public string displayFreq(int channelnum)
        {

            if (channelnum > 0 & channelnum < 9)
                return acquireFrequency(channelnum);

            else if (channelnum == 0)
                return "Off";

            else
                return "Error";
        }



        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }


        //For each method, first look up target (Laser) according to input name in (Dictionary) lasers
        //Then implement method on the target (Laser)
        public double getOutputvoltage(string slavename)
        {
            Laser laser = lasers[slavename];
            return laser.CurrentVoltage;
        }


        internal void setChannel(string slavename, int c)
        {
            Laser laser = lasers[slavename];
            laser.WLMChannel = c;
        }
        internal void setPGain(string slavename, double g)
        {
            Laser laser = lasers[slavename];
            laser.PGain = g;

        }

        internal void setIGain(string slavename, double g)
        {
            Laser laser = lasers[slavename];
            double oldIGain = laser.IGain;
            if (g != 0)
            {
                laser.summedWavelengthDifference = laser.summedWavelengthDifference * oldIGain / g; //Scale summed error to prevent overshoot
            }
            laser.IGain = g;
        }

        internal void setFrequency(string slavename, double s)
        {
            Laser laser = lasers[slavename];
            laser.setFrequency = s;
        }


        public double gerFrequencyError(string slavename)
        {
            Laser laser = lasers[slavename];
            return laser.FrequencyError;
        }

        public string getLaserState(string slavename)
        {
            Laser laser = lasers[slavename];
            if (laser.lState == Laser.LaserState.LOCKED)
                return "Locked";
            else return "Unlocked";
        }



        public bool returnLaserState(string slavename)
        {
            Laser laser = lasers[slavename];
            if (laser.lState == Laser.LaserState.LOCKED)
                return true;
            else
                return false;

        }

        public string getChannelNum(string slavename)
        {
            Laser laser = lasers[slavename];
            return Convert.ToString(laser.WLMChannel);
        }

        public void resetOutput(string slavename)
        {
            Laser laser = lasers[slavename];
            laser.ResetOutput();
        }



        public void EngageLock(string slavename)
        {
            Laser laser = lasers[slavename];
            //laser.ResetOutput();
            laser.Lock();
        }

        public void DisengageLock(string slavename)
        {
            Laser laser = lasers[slavename];
            laser.DisengageLock();
            wavemeterContrller.changeConnectionStatus(laser.WLMChannel, false);
        }

        public void toggle_led_state(int n, bool val)
        {
            ui.toggle_led(n, val);
        }

        #endregion

        private void updateLockRate(Stopwatch stopWatch)
        {
            double elapsedTime = stopWatch.Elapsed.TotalSeconds;
            scanTimes.Add(elapsedTime);
            if (scanTimes.Count > numScanAverages)
            {
                double averageScanTime = scanTimes.Sum() / scanTimes.Count;
                double averageUpdateRate = 1 / averageScanTime;
                ui.UpdateLockRate(averageUpdateRate);
                scanTimes = new List<double>();
            }

        }

        public void updateFrequency(Laser laser)
        {
            laser.currentFrequency = getFrequency(laser.WLMChannel);
        }

        private void polling()
        {
            if (WMLState != ControllerState.STOPPED)
            {
                if (wavemeterContrller.getMeasurementStatus(thisComputerName))//SocketException thrown here when server turned off while running
                {
                    updateLockMaster();
                    wavemeterContrller.resetMeasurementStatus(thisComputerName);
                }
            }

            else endLoop();
        }

        private void mainLoop()
        {
            scanTimes = new List<double>();
            stopWatch.Restart();
            stopWatchGraph.Restart();
            loopcount = 0;

            while (true)
            {
                polling();
            }
        }

        private void endLoop()
        {
            loopcount = 0;
            foreach (string laser in lasers.Keys)
            {
                if (lasers[laser].lState != Laser.LaserState.FREE)
                {
                    lasers[laser].DisengageLock();
                    //lasers[laser].DisposeLaserControl();
                }
            }

        }

        private void checkBlockLoop()
        {
            while (true)
            {
                if (WMLState != ControllerState.STOPPED) { 
                    foreach (string slave in lasers.Keys){
                    
                        if (lockBlocked.ContainsKey(slave)){
                        
                            checkBlockStatus(slave);
                        }
                    }
                }
            }
        }

        private const int MaxGlitches = 3;

        private string WlmFaultDescription(string laserName, double measuredFreq)
        {
            switch ((int)measuredFreq)
            {
                case  0: return "no WLM value yet (ErrNoValue)";
                case -1: return "no signal (ErrNoSignal)";
                case -2: return "bad signal (ErrBadSignal)";
                case -3: return "signal too low / underexposed (ErrLowSignal)";
                case -4: return "signal too high / overexposed (ErrBigSignal)";
                case -5: return "wavemeter not connected (ErrWlmMissing)";
                case -6: return "channel not available (ErrNotAvailable)";
                case -8: return "no pulse (ErrNoPulse)";
                case -10: return "channel not available (ErrChannelNotAvailable)";
                case -13: return "WLM division by zero (ErrDiv0)";
                case -14: return "WLM out of range (ErrOutOfRange)";
                case -15: return "unit not available (ErrUnitNotAvailable)";
                case -26: return "TCP error (ErrTCPErr)";
                default:
                    if (measuredFreq <= 0)
                        return "unknown WLM error code " + ((int)measuredFreq).ToString();
                    double errorMHz = 1e6 * (measuredFreq - lasers[laserName].setFrequency);
                    return "frequency jump " + errorMHz.ToString("F2") +
                           " MHz (measured " + measuredFreq.ToString("F6") +
                           " THz, set " + lasers[laserName].setFrequency.ToString("F6") + " THz)";
            }
        }

        private void HandleLockFault(string slave)
        {
            lasers[slave].glitchCount++;
            string description = WlmFaultDescription(slave, lasers[slave].currentFrequency);
            if (lasers[slave].glitchCount > MaxGlitches)
            {
                faultyLaser = slave;
                lasers[slave].glitchCount = 0;
                lasers[slave].DisengageLock();
                indicateRemoteConnection(lasers[slave].WLMChannel, false);
                ui.Log(slave + " lock disengaged after " + MaxGlitches + " glitches: " + description);
            }
            else
            {
                ui.Log(slave + " glitch " + lasers[slave].glitchCount + "/" + MaxGlitches + ": " + description);
            }
        }

        private void updateLockMaster()
        {
            if (WMLState != ControllerState.STOPPED)
            {

                foreach (string slave in lasers.Keys)
                {
                    if (lockBlocked.ContainsKey(slave))
                    {
                        //checkBlockStatus(slave);
                        panelList[slave].updateLockBlockStatus(lockBlocked[slave]);

                        if (lasers[slave].lState == Laser.LaserState.LOCKED && !lasers[slave].isBlocked )
                        {

                            updateFrequency(lasers[slave]);

                            if (Math.Abs(getFrequency(lasers[slave].WLMChannel) - lasers[slave].setFrequency) > freqTolerance)//In the case of over/underexpose or big mode-hop, disengage lock

                            {
                                HandleLockFault(slave);
                            }
                            else
                            {
                                lasers[slave].glitchCount = 0;
                                lasers[slave].UpdateLock();
                            }
                        }

                        

                        else
                        lasers[slave].UpdateBlockedLock();

                    }

                    else
                    {

                        updateFrequency(lasers[slave]);

                        if (lasers[slave].lState == Laser.LaserState.LOCKED)
                        {


                            if (Math.Abs(getFrequency(lasers[slave].WLMChannel) - lasers[slave].setFrequency) > freqTolerance)//In the case of over/underexpose or big mode-hop, disengage lock

                            {
                                HandleLockFault(slave);
                            }
                            else
                            {
                                lasers[slave].glitchCount = 0;
                                lasers[slave].UpdateLock();
                            }
                        }

                        else
                            lasers[slave].UpdateBlockedLock();
                    }

                    if (lasers[slave].logData)
                    {
                        logSlaveData(slave);
                    }
                }

                
            }

            loopcount++;

            //Calculate the time
            foreach (string slave in lasers.Keys)
            {
                if (lasers[slave].lState == Laser.LaserState.LOCKED)
                {
                    timeList[slave] += stopWatchGraph.Elapsed.TotalSeconds;
                }
            }

            stopWatchGraph.Restart();

            foreach (string slave in lasers.Keys)
            {
                panelList[slave].AppendToErrorGraph(timeList[slave], 1000000 * lasers[slave].FrequencyError);
            }

            updateLockRate(stopWatch);
            stopWatch.Restart();


        }

        public void removeWavemeterLock()
        {
            wavemeterContrller.removeWavemeterLock(thisComputerName);
        }

        void checkBlockStatus(string laser)
        {
            blockHelper[laser].checkLockBlockStatus();
            lockBlocked[laser] = blockHelper[laser].isBlocked;
            lasers[laser].isBlocked = blockHelper[laser].isBlocked;
        }

        public void connectionError(Exception e)
        {
            MessageBox.Show($"Connection failed: {e.Message}");
            Environment.Exit(1);
        }

        public void updateLaserRMSNoise(Laser laser)
        {
            if (laser.lState == Laser.LaserState.LOCKED)
            {
                laser.sumedNoise += Math.Pow((laser.FrequencyError * Math.Pow(10, 6)), 2);
                laser.loopCount++;
                laser.RMSNoise = Math.Sqrt(laser.sumedNoise / laser.loopCount);
            }

        }

        public void logSlaveData(string slave)
        {
            DateTime dt = DateTime.Now;
            String filename = logfilePath + slave + "-" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";

            if (!System.IO.File.Exists(filename))
            {
                string header = "Time \t Set_Frequency(THz) \t Current_Frequency(THz) \t " +
                   "Frequency_Error(MHz) \t Lock_Status \t Lock_Block_Status \t "+
                   "Current Voltage \t";
                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(filename, false))
                    file.WriteLine(header);
            }

            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(filename, true))
            {
                file.WriteLine(dt.TimeOfDay.ToString() + "\t" + lasers[slave].setFrequency + "\t" + lasers[slave].currentFrequency + "\t" +
                    lasers[slave].FrequencyError * 1000000.0 + "\t" + lasers[slave].lState.ToString() + "\t" + lasers[slave].isBlocked.ToString()+"\t"+
                    lasers[slave].CurrentVoltage
                    );
                file.Flush();
            }
        }

        public void logSetPoints()
        {
            string defaultDirectory = System.IO.Path.Combine(logfilePath, "LaserSetPoints");
            System.IO.Directory.CreateDirectory(defaultDirectory);

            string defaultFileName = "WavemeterLockLog_" + DateTime.Now.ToString("yyyy_MM_dd") + ".txt";
            string fullPath = System.IO.Path.Combine(defaultDirectory, defaultFileName);

            WriteSetPointsLog(fullPath, "Auto log");
        }

        public void logSetPoints(string fullPath, string note = "Manual log")
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                return;

            string directory = System.IO.Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            WriteSetPointsLog(fullPath, note);
        }

        private void WriteSetPointsLog(string filename, string note)
        {
            bool fileExists = System.IO.File.Exists(filename);
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename, true))
            {
                if (!fileExists)
                {
                    string header = "Time\tLaser\tSet Frequency (THz)\tCurrent Frequency (THz)\tFrequency Error (MHz)\tPGain\tIGain\tOffset(V)\tLock Status\tLock Block Status\tNote";
                    file.WriteLine(header);
                }

                foreach (string slave in lasers.Keys)
                {
                    double frequencyErrorMHz = lasers[slave].FrequencyError * 1000000.0;

                    string content =
                        timestamp + "\t" +
                        slave + "\t" +
                        lasers[slave].setFrequency + "\t" +
                        lasers[slave].currentFrequency + "\t" +
                        frequencyErrorMHz + "\t" +
                        lasers[slave].PGain + "\t" +
                        lasers[slave].IGain + "\t" +
                        lasers[slave].offsetVoltage + "\t" +
                        lasers[slave].lState.ToString() + "\t" +
                        lasers[slave].isBlocked.ToString() + "\t" +
                        note;

                    file.WriteLine(content);
                }

                file.Flush();
            }
        }

        public void loadSetPoints(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                return;

            ReadSetPointsLog(fullPath);
        }

        private void ReadSetPointsLog(string filename)
        {
            if (!System.IO.File.Exists(filename))
                return;

            Dictionary<string, List<string>> latestLaserEntries = new Dictionary<string, List<string>>();

            foreach (string line in System.IO.File.ReadLines(filename))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("Time\t"))
                    continue;

                List<string> resultList = line.Split('\t').ToList();

                if (resultList.Count < 11)
                    continue;

                string slave = resultList[1];

                if (lasers.ContainsKey(slave))
                {
                    latestLaserEntries[slave] = resultList;
                }
            }

            foreach (string slave in latestLaserEntries.Keys)
            {
                List<string> resultList = latestLaserEntries[slave];

                lasers[slave].setFrequency = Convert.ToDouble(resultList[2]);
                lasers[slave].currentFrequency = Convert.ToDouble(resultList[3]);
                lasers[slave].PGain = Convert.ToDouble(resultList[5]);
                setIGain(slave, Convert.ToDouble(resultList[6]));
                lasers[slave].offsetVoltage = Convert.ToDouble(resultList[7]);
            }

            foreach (LockControlPanel panel in panelList.Values)
            {
                panel.updateParameters();
            }
        }

        /// <summary>
        /// Log laser setpoint and frequency for each iteration of experiment
        /// </summary>
        public void logCurrentSetpoints()
        {
            string filePath = (string)DAQ.Environment.Environs.FileSystem.Paths["ToFFilesPath"];
            string filename = filePath + "LaserSetPoints\\" + "WavemeterLockLog" + ".txt";

            foreach (string slave in lasers.Keys)
            {
                string header = "Laser:" + "\t" + slave + "\t" + "Lock Status" + "\t" + lasers[slave].lState.ToString();

                string content = "Set frequency (THz):" + "\t" + lasers[slave].setFrequency + "\t" + 
                    "Current frequency (THz):" + "\t" + lasers[slave].currentFrequency + "\t" +
                    "Frequency error (MHz):" + "\t" + (1e6 * lasers[slave].currentFrequency - lasers[slave].setFrequency).ToString() + "\t";


                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(filename, true))
                {
                    file.WriteLine(header);
                    file.WriteLine(content);
                    file.Flush();
                }
            }
        }
        
    }
}
