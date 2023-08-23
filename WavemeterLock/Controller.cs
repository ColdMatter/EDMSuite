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

            catch(Exception e)
            {
                connectionError(e);
            }
            //Register in remote server
            thisComputerName = Environment.MachineName.ToString();

            try
            {
                wavemeterContrller.registerWavemeterLock(thisComputerName);
            }
            catch(Exception e)
            {
                connectionError(e);
            }
            //This throws a security exception
            //wavemeterContrller.measurementAcquired += () => { updateLockMaster(); };
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
        public Dictionary<string, DAQMxWavemeterLockLaserControlHelper> helper = new Dictionary<string, DAQMxWavemeterLockLaserControlHelper>();
        public Dictionary<string, DAQMxWavemeterLockBlockHelper> blockHelper = new Dictionary<string, DAQMxWavemeterLockBlockHelper>();
        public Dictionary<string, LockControlPanel> panelList = new Dictionary<string, LockControlPanel>();
        public Dictionary<string, double> timeList = new Dictionary<string,double>();
        private LockForm ui;
        public WavemeterLockConfig config;

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
            initializeTCPChannel();
            ui = new LockForm();
            ui.controller = this;
            initializeLasers();
            Application.Run(ui);
        }

        public void startWML()
        {
            Thread mainThread = new Thread(new ThreadStart(mainLoop));
            mainThread.Start();
        }


        public void indicateRemoteConnection(int channelNum, bool status)
        {
            wavemeterContrller.changeConnectionStatus(channelNum,status);
        }

        public void initializeLasers()
        {
            lasers = new Dictionary<string, Laser>();
            lockBlocked = new Dictionary<string, bool>();
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
                Laser slave = new Laser(laser, entry.Value, helper[laser]);
                lasers.Add(laser, slave);
                slave.WLMChannel = config.channelNumbers[laser];
                wmChannels.Add(laser, slave.WLMChannel);
            }

            //Config lock block
            foreach (KeyValuePair<string, string> entry in config.lockBlockFlag)
            {
                string laser = entry.Key;
                blockHelper.Add(laser, new DAQMxWavemeterLockBlockHelper(laser, config.lockBlockFlag[laser]));
                lockBlocked.Add(laser, false);
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

        

        public bool returnLaserState(string slavename){
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

        private void updateLockMaster()
        {
            if (WMLState != ControllerState.STOPPED)
            {

                foreach (string slave in lasers.Keys)
                {
                    if (lockBlocked.ContainsKey(slave))
                    {
                        checkBlockStatus(slave);
                        updateFrequency(lasers[slave]);
                        panelList[slave].updateLockBlockStatus(lockBlocked[slave]);

                        if (lasers[slave].lState == Laser.LaserState.LOCKED && !lockBlocked[slave])
                        {


                            if (Math.Abs(getFrequency(lasers[slave].WLMChannel) - lasers[slave].setFrequency) > freqTolerance)//In the case of over/underexpose or big mode-hop, disengage lock

                            {
                                faultyLaser = slave;
                                lasers[slave].DisengageLock();
                                //Thread msgThread = new Thread(errorMsg);
                                indicateRemoteConnection(lasers[slave].WLMChannel, false);
                                //msgThread.Start();
                            }
                            else
                            {
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
                                faultyLaser = slave;
                                lasers[slave].DisengageLock();
                                //Thread msgThread = new Thread(errorMsg);
                                indicateRemoteConnection(lasers[slave].WLMChannel, false);
                                //msgThread.Start();
                            }
                            else
                            {
                                lasers[slave].UpdateLock();
                            }
                        }

                        else
                            lasers[slave].UpdateBlockedLock();
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
        }

        public void errorMsg()
        {
            MessageBox.Show("You messed up! Laser " + faultyLaser + " lock disengaged due to wavemeter reading error or large frequency jump.");
        }

        public void connectionError(Exception e)
        {
            MessageBox.Show($"Connection failed: {e.Message}");
            if (Application.MessageLoop)
            {
                // WinForms app
                Application.Exit();
            }
            else
            {
                // Console app
                Environment.Exit(1);
            }
        }



    }
}
