using System;
using System.Threading;
using wlmData;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;

namespace WavemeterLockServer
{
    
    public delegate void measurementHandler();

    public class Controller : MarshalByRefObject
    {
        public Boolean bAvail;
        public Boolean bMeas;
        public int mode;
        public int timeVal;
        public int val;
        public int switchTime;
        private ServerForm ui;
        public bool[] remoteConnection = new bool[8];//an array of boolean to show if the channel is being used by wavemeterLock remotely
        
        public event measurementHandler measurementAcquired;
        public Dictionary<string, bool> measurementStatus = new Dictionary<string, bool>();
        public int test = 0;

        public Controller()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ui = new ServerForm(this);
            

        }

        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        private WLM.CallbackProcEx callbackObj;
        public void start()
        {
            
            callbackObj = new WLM.CallbackProcEx(callback);
            WLM.Instantiate(WLM.cInstNotification, WLM.cNotifyInstallCallback, callbackObj, 0);
            measurementAcquired += () => { indicateNewMeasurement(); };
            Application.Run(ui);
            for (int i = 0; i < 8; i++)
                remoteConnection[i] = false;
            
        }


        private void callback(int Ver, int Mode, int IntVal, double DblVal, int Res1)
        {

            switch (Mode)
            {
                case WLM.cmiResultMode:
                    //measurementAcquired.Invoke(Ver, Mode, IntVal, DblVal, Res1);
                    measurementAcquired?.Invoke();
                    break;

            }

        }

       
        public void changeConnectionStatus(int channelNum, bool status)
        {
            remoteConnection[channelNum - 1] = status;
        }

        public void registerWavemeterLock(string computerName)
        {
            try
            {
                measurementStatus.Add(computerName, false);
            }

            catch (ArgumentException)
            {
                measurementStatus.Remove(computerName);
                measurementStatus.Add(computerName, false);
            }
        }

        public void indicateNewMeasurement()
        {
            
            for (int index = 0; index < measurementStatus.Count(); index++)
            {
                string dummy;
                dummy = measurementStatus.ElementAt(index).Key;
                measurementStatus[dummy] = true;
            }
        }

        public bool getMeasurementStatus(string computerName)
        {
            return measurementStatus[computerName];
        }

        public void resetMeasurementStatus(string computerName)
        {
            measurementStatus[computerName] = false;
        }

        public double getWavelength(int channelNum)//Returns wavelength (nm)
        {
            return WLM.GetWavelengthNum(channelNum, 0);
        }

        public double getFrequency(int channelNum)//Returns frequency (THz)
        {
            return WLM.GetFrequencyNum(channelNum, 0);
        }


        // check result for error and display message or wavelength
        public string displayWavelength(int n)
        {
            double w = 0;
            int i = Convert.ToInt32(getWavelength(n));
            switch (i)
            {
                case WLM.ErrNoValue:
                    return "No value measured";

                case WLM.ErrNoSignal:
                    return "No signal";

                case WLM.ErrBadSignal:
                    return "Bad signal";

                case WLM.ErrLowSignal:
                    return "Underexposed";

                case WLM.ErrBigSignal:
                    return "Overexposed";

                case WLM.ErrWlmMissing:
                    bAvail = false;
                    return "WLM Server not available";

                case WLM.ErrOutOfRange:
                    return "Out of range";

                case WLM.ErrUnitNotAvailable:
                    return "Requested unit not available";

                default: // no error
                    w = Math.Round(w, (int)WLM.GetWLMVersion(0) - 2);
                    return Convert.ToString(Math.Round(WLM.GetWavelengthNum(n, 0), (int)WLM.GetWLMVersion(0) - 2)) + "  nm";
            }

        }

        public string displayFrequency(int n)
        {
            double w = 0;
            int i = Convert.ToInt32(getFrequency(n));
            switch (i)
            {
                case WLM.ErrNoValue:
                    return "No value measured";

                case WLM.ErrNoSignal:
                    return "No signal";

                case WLM.ErrBadSignal:
                    return "Bad signal";

                case WLM.ErrLowSignal:
                    return "Underexposed";

                case WLM.ErrBigSignal:
                    return "Overexposed";

                case WLM.ErrWlmMissing:
                    bAvail = false;
                    return "WLM Server not available";

                case WLM.ErrOutOfRange:
                    return "Out of range";

                case WLM.ErrUnitNotAvailable:
                    return "Requested unit not available";

                default: // no error
                    w = Math.Round(w, (int)WLM.GetWLMVersion(0) - 2);
                    return Convert.ToString(Math.Round(WLM.GetFrequencyNum(n, w), (int)WLM.GetWLMVersion(0) - 2)) + "  THz";
            }

        }

        
       


        public void startServer()
        {
            if (bAvail)
            {
                WLM.ControlWLM(WLM.cCtrlWLMExit, 0, 0);
                bAvail = false;

            }
            else
            {
                WLM.ControlWLM(WLM.cCtrlWLMShow, 0, 0);
                bAvail = true;

            }
        }

        public void startMeasure()
        {
            if (bMeas)
            {
                WLM.Operation(0);
                bMeas = false;
            }
            else
            {
                WLM.Operation(2);
                bMeas = true;
            }
        }

    }

}
