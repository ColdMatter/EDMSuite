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

namespace WavemeterLock
{
    public class Controller : MarshalByRefObject
    {


        private string computer;
        private string name;
        private string hostName = (String)System.Environment.GetEnvironmentVariables()["COMPUTERNAME"];
        public int loopcount = 0;
        private WavemeterLockServer.Controller wavemeterContrller;

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
            computer = hostName;
            IPHostEntry hostInfo = Dns.GetHostEntry(computer);

            foreach (var addr in Dns.GetHostEntry(computer).AddressList)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                    name = addr.ToString();
            }

            EnvironsHelper eHelper = new EnvironsHelper(computer);

            wavemeterContrller = (WavemeterLockServer.Controller)(Activator.GetObject(typeof(WavemeterLockServer.Controller), "tcp://" + name + ":" + "1984" + "/controller.rem"));
            return wavemeterContrller.displayWavelength(channelNum);

        }

        public double getWavelength(int channelNum) //Return wavelength in nm
        {
            computer = hostName;
            IPHostEntry hostInfo = Dns.GetHostEntry(computer);

            foreach (var addr in Dns.GetHostEntry(computer).AddressList)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                    name = addr.ToString();
            }

            EnvironsHelper eHelper = new EnvironsHelper(computer);

            return wavemeterContrller.getWavelength(channelNum);

        }

        public string acquireFrequency(int channelNum) //Display frequency
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
            return wavemeterContrller.displayFrequency(channelNum);

        }

        public double getFrequency(int channelNum) //Return frequency in THz
        {
            computer = hostName;
            IPHostEntry hostInfo = Dns.GetHostEntry(computer);

            foreach (var addr in Dns.GetHostEntry(computer).AddressList)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                    name = addr.ToString();
            }

            EnvironsHelper eHelper = new EnvironsHelper(computer);

            return wavemeterContrller.getFrequency(channelNum);

        }
        #endregion

        public Dictionary<string, Laser> lasers;
        public Dictionary<string, DAQMxWavemeterLockLaserControlHelper> helper = new Dictionary<string, DAQMxWavemeterLockLaserControlHelper>();
        private LockForm ui;
        public WavemeterLockConfig config;

        public Controller(string configName)
        {
            config = (WavemeterLockConfig)Environs.Hardware.GetInfo(configName);

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
                ui.AddLaserControlPanel(slaveLaser, config.slaveLasers[slaveLaser]);
                helper.Add(slaveLaser, new DAQMxWavemeterLockLaserControlHelper(config.slaveLasers[slaveLaser]));
            }

            foreach (KeyValuePair<string, string> entry in config.slaveLasers)//Enter value in dictionary according to config
            {
                string laser = entry.Key;
                Laser slave = new Laser(laser, entry.Value, helper[laser]);
                lasers.Add(laser, slave);

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

        public void updateFrequency(Laser laser)
        {
            laser.currentFrequency = getFrequency(laser.WLMChannel);
        }
        private void endLoop()
        {
            foreach (string laser in lasers.Keys)
            {
                if (lasers[laser].lState != Laser.LaserState.FREE)
                {
                    lasers[laser].DisengageLock();
                    lasers[laser].DisposeLaserControl();
                }
            }
            
        }

        
        private void mainLoop()
        {
            
            while (WMLState!= ControllerState.STOPPED)
            {
                foreach(string slave in lasers.Keys)
                {
                    Laser laser = lasers[slave];
                    updateFrequency(laser);
                    loopcount++;
                    if(laser.lState == Laser.LaserState.LOCKED)
                    {
                        laser.UpdateLock();
                    }
                }
                
            }

            endLoop();
            loopcount = 0;
        }

        



    }
}
