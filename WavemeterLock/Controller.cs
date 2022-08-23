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
        private string hostName = "IC-CZC136CFDJ";
        public int loopcount = 0;
        private WavemeterLockServer.Controller wavemeterContrller;
        public int colorParameter = 0;
        private double freqTolerance = 0.5;//THz

        private List<double> scanTimes;
        public int numScanAverages = 1000;


        #region Set up TCP channel

        public void initializeTCPChannel()
        {
            computer = hostName;
            IPHostEntry hostInfo = Dns.GetHostEntry(computer);

            foreach (var addr in Dns.GetHostEntry(computer).AddressList)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                    name = addr.ToString();
            }

            EnvironsHelper eHelper = new EnvironsHelper(computer);

            wavemeterContrller = (WavemeterLockServer.Controller)(Activator.GetObject(typeof(WavemeterLockServer.Controller), "tcp://" + name + ":" + "1984" + "/controller.rem"));
        }

        public string acquireWavelength(int channelNum) //Display wavelength
        {
            
            return wavemeterContrller.displayWavelength(channelNum);

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
        #endregion

        public Dictionary<string, Laser> lasers;
        public Dictionary<string, DAQMxWavemeterLockLaserControlHelper> helper = new Dictionary<string, DAQMxWavemeterLockLaserControlHelper>();
        public Dictionary<string, LockControlPanel> panelList = new Dictionary<string, LockControlPanel>();
        private LockForm ui;
        public WavemeterLockConfig config;

        public Controller(string configName)
        {
            config = (WavemeterLockConfig)Environs.Hardware.GetInfo(configName);

        }
        
        public Color selectColor(int par)//Asign a plot line color for each laser
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
            initializeTCPChannel();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
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

        public void initializeLasers()
        {
            lasers = new Dictionary<string, Laser>(); //A dictionary to store slave lasers

            foreach (string slaveLaser in config.slaveLasers.Keys)
            {
                ui.AddLaserControlPanel(slaveLaser, config.slaveLasers[slaveLaser], config.channelNumbers[slaveLaser]);
                helper.Add(slaveLaser, new DAQMxWavemeterLockLaserControlHelper(config.slaveLasers[slaveLaser]));
            }

            foreach (KeyValuePair<string, string> entry in config.slaveLasers)//Enter value in dictionary according to config
            {
                string laser = entry.Key;
                Laser slave = new Laser(laser, entry.Value, helper[laser]);
                lasers.Add(laser, slave);
                slave.WLMChannel = config.channelNumbers[laser];

            }

            Dictionary<string, string> analogs = new Dictionary<string, string>();

            
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
            else return false;
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
            laser.ResetOutput();
            laser.Lock();
        }

        public void DisengageLock(string slavename)
        {
            Laser laser = lasers[slavename];
            laser.DisengageLock();
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

        
        private void mainLoop()
        {
            scanTimes = new List<double>();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            loopcount = 0;
            int miniLoopcount = 0;

            while (WMLState!= ControllerState.STOPPED)
            {
                foreach(string slave in lasers.Keys)
                {
                    Laser laser = lasers[slave];
                    updateFrequency(laser);
                    loopcount++;
                    miniLoopcount++;
                    if(laser.lState == Laser.LaserState.LOCKED)
                    {
                        if (Math.Abs(getFrequency(laser.WLMChannel) - laser.setFrequency)>freqTolerance)//In the case of over/underexpose or big mode-hop, disengage lock
                        {
                            laser.DisengageLock();
                            MessageBox.Show("You messed up! Laser " + slave + " lock disengaged due to wavemeter reading error or large frequency jump.");
                        }
                        laser.UpdateLock();
                        if (miniLoopcount > 30)
                        {
                            panelList[slave].AppendToErrorGraph(loopcount, 1000000 * laser.FrequencyError);
                            miniLoopcount = 0;
                        }
                    }
                }
                updateLockRate(stopWatch);

                stopWatch.Reset();
                stopWatch.Start();
            }

            endLoop();
        }

        



    }
}
