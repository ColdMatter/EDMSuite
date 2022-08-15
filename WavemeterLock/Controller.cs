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
        #region Set up TCP channel

        private string computer;
        private string name;
        private string hostName = (String)System.Environment.GetEnvironmentVariables()["COMPUTERNAME"];

        private WavemeterLockServer.Controller wavemeterContrller;




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

        public double getWavelength(int channelNum) //Return wavelength
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

        public double getFrequency(int channelNum) //Return frequency
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
        WavemeterLockLaserControllable wml;
        private LockForm ui;
        public void start()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ui = new LockForm();
            ui.controller = this;
            Application.Run(ui);

            
        }

        public enum ControllerState
        {
            STOPPED, RUNNING
        };

        public ControllerState WMLState = ControllerState.STOPPED;

        Laser laser = new Laser("WavemeterLock1");

        //Displace the wavelength of the target laser.
        public string displayWL(int num)
        {

            if (num > 0 & num < 9)
                return acquireWavelength(num);

            else if (num == 0)
                return "Off";

            else
                return "Error";
        }

        public string displayFreq(int num)
        {

            if (num > 0 & num < 9)
                return acquireFrequency(num);

            else if (num == 0)
                return "Off";

            else
                return "Error";
        }

        public void updateFrequency()
        {
            laser.currentFrequency = getFrequency(laser.WLMChannel);
        }

        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        public double getOutputvoltage()
        {
            return laser.CurrentVoltage;
        }


        internal void setChannel(int c)
        {
            laser.WLMChannel = c;
        }
        internal void setPGain(double g)
        {
            laser.PGain = g;
        }

        internal void setIGain (double g)
        {
            laser.IGain = g;
        }

        internal void setFrequency (double s)
        {
            laser.setFrequency = s;
        }


        public double gerFrequencyError()
        {
            return laser.FrequencyError;
        }

        public string getLaserState()
        {
            if (laser.lState == Laser.LaserState.LOCKED)
                return "Locked";
            else return "Unlocked";
        }

        public string getChannelNum()
        {
            return Convert.ToString(laser.WLMChannel);
        }

        public void resetOutput()
        {
            laser.ResetOutput();
        }

     
        public void EngageLock()
        {
            WMLState = ControllerState.RUNNING;
            laser.Lock();
            Thread.Sleep(0);
            Thread mainThread = new Thread(new ThreadStart(mainLoop));
            mainThread.Start();
        }

        public void DisengageLock()
        {
            WMLState = ControllerState.STOPPED;
            laser.DisengageLock();
        }

        private int loopcount = 0;

        private void endLoop()
        {
            loopcount = 0;
            
            if (laser.lState != Laser.LaserState.FREE)
                {
                    laser.DisengageLock();
                    laser.DisposeLaserControl();
                }
            
        }

        
        private void mainLoop()
        {


            while(WMLState!= ControllerState.STOPPED)
            {
                updateFrequency();
                laser.UpdateLock();
                loopcount++;
            }

            endLoop();
        }

        public int getLoopCount()
        {
            return loopcount;
        }



    }
}
