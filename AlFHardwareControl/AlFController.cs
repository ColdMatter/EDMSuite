using DAQ.Environment;
using DAQ.HAL;
using DAQ;
using System.Windows.Forms;
using System.Threading;
using System;
using System.Windows.Forms.DataVisualization.Charting;
using Data;
using NewFocus.PicomotorApp;
using Newport.DeviceIOLib;
using System.Collections.Generic;

namespace AlFHardwareControl
{
    public class AlFController : MarshalByRefObject, ExperimentReportable
    {

        private AlFControlWindow window;

        public LakeShore336TemperatureController lakeshore = (LakeShore336TemperatureController)Environs.Hardware.Instruments["Lakeshore"];
        public LeyboldGraphixController leybold = (LeyboldGraphixController)Environs.Hardware.Instruments["LeyboldGraphix"];
        public Eurotherm3504Instrument eurotherm = (Eurotherm3504Instrument)Environs.Hardware.Instruments["Eurotherm"];

        public bool interlocksActive = true;
        public bool InterlocksActive
        {
            get
            {
                return interlocksActive;
            }
        }

        public AlFController()
        {
            Application.SetCompatibleTextRenderingDefault(false);
        }

        public void ExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Uncaught exception! Take care program might be unstable\n\n" +
                    "Exception:\n" + ((Exception)e.ExceptionObject).Message + "\n\n" + ((Exception)e.ExceptionObject), "Uncaught Exception",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public Dictionary<string, object> GetExperimentReport()
        {
            Dictionary<string,object> dict = new Dictionary<string, object>();
            dict["scanActive"] = window.mmStuff.ScanRunning;
            if (window.mmStuff.ScanRunning)
            {
                dict["scanParam"] = window.mmStuff.ScanRunning;
                dict["scanName"] = window.mmStuff.ScanPlugin;
                dict["scanSettings"] = window.mmStuff.ScanSettings;
            }

            while (!window.mmStuff.isDataSaved && window.mmStuff.ToFArmed) ;
            window.mmStuff.resetDataSaveStatus();
            return dict;
        }

        public override Object InitializeLifetimeService()
        {
            return null;
        }

        public void Start()
        {

            //DeviceIOLib deviceIOLib = new DeviceIOLib();
            //CmdLib8742 cmdlib = new CmdLib8742(deviceIOLib);
            //deviceIOLib.DiscoverDevices(0b0001, 5000);



            window = new AlFControlWindow(this);
            window.controller = this;
            Application.Run(window);
        }


        #region Resource related functions
        public string nameA;
        public string nameB;
        public string nameC;
        public string nameD;


        public void UpdateLakeshoreNames()
        {
            lock (lakeshore)
            {
                nameA = lakeshore.GetChannelName("A");
                nameB = lakeshore.GetChannelName("B");
                nameC = lakeshore.GetChannelName("C");
                nameD = lakeshore.GetChannelName("D");
            }

        }

        public Thread UpdateThread; 
        public void WindowLoaded()
        {
            UpdateLakeshoreTemperature();
            UpdateThread = new Thread(() => UpdateData());
            UpdateThread.Start();
        }

        private void UpdateLakeshoreTemperature()
        {
            DateTime localDate = DateTime.Now;
            string tempA;
            string tempB;
            string tempC;
            string tempD;
            lock (lakeshore)
            {
                tempA = lakeshore.GetTemperature(1, "K").Trim(new char[] { '\n', '\r' });
                tempB = lakeshore.GetTemperature(2, "K").Trim(new char[] { '\n', '\r' });
                tempC = lakeshore.GetTemperature(3, "K").Trim(new char[] { '\n', '\r' });
                tempD = lakeshore.GetTemperature(4, "K").Trim(new char[] { '\n', '\r' });
            }
            window.SetTextField(window.TempA, tempA + " K");
            window.SetTextField(window.TempB, tempB + " K");
            window.SetTextField(window.TempC, tempC + " K");
            window.SetTextField(window.TempD, tempD + " K");

        }

        public string pressure1;
        public string pressure2;
        public string pressure3;
        public string pressure1Name;
        public string pressure2Name;
        public string pressure3Name;

        public void UpdateLeyboldNames()
        {
            lock (leybold)
            {
                pressure1Name = leybold.ReadValue("1;5");
                pressure2Name = leybold.ReadValue("2;5");
                pressure3Name = leybold.ReadValue("3;5");
            }
        }

        private void UpdatePressure()
        {

            lock (leybold)
            {
                pressure1 = leybold.ReadValue("1;29");
                pressure2 = leybold.ReadValue("2;29");
                pressure3 = leybold.ReadValue("3;29");
            }

            window.SetTextField(window.P1, pressure1 + " mbar");
            window.SetTextField(window.P2, pressure2 + " mbar");
            window.SetTextField(window.P3, pressure3 + " mbar");

        }

        public double loop1PV;
        public double loop2PV;

        private void UpdateTypeK()
        {
            lock (eurotherm)
            {
                loop1PV = eurotherm.GetPV(0);
                loop2PV = eurotherm.GetPV(1);
            }

            window.SetTextField(window.Loop1Temp, loop1PV + " C");
            window.SetTextField(window.Loop2Temp, loop2PV + " C");
            UpdateTypeKAMState();
        }

        public bool loop1Off;
        public bool loop2Off;
        public double loop1Out;
        public double loop2Out;
        private void UpdateTypeKAMState()
        {
            lock (eurotherm)
            {
                //loop1Off = eurotherm.GetAMSwitch(0);
                //loop2Off = eurotherm.GetAMSwitch(1);

                loop1Off = eurotherm.GetHeaterShutoff(0);
                loop2Off = eurotherm.GetHeaterShutoff(1);

                loop1Out = eurotherm.GetActiveOut(0);
                loop2Out = eurotherm.GetActiveOut(1);
            }
            bool OK_Cryo = window.CryoStatus.Text == "OFF" || !InterlocksActive;
            if (loop1Off)
            {
                window.SetTextField(window.Loop1Status, "OFF");
                if (OK_Cryo)
                    window.UpdateRenderedObject(window.Loop1Engage, (Button but) => { but.Enabled = true; });
                else
                    window.UpdateRenderedObject(window.Loop1Engage, (Button but) => { but.Enabled = false; });
                window.UpdateRenderedObject(window.Loop1Disengage, (Button but) => { but.Enabled = false; });
            }
            else
            {
                window.SetTextField(window.Loop1Status, "ON");
                window.UpdateRenderedObject(window.Loop1Engage, (Button but) => { but.Enabled = false; });
                window.UpdateRenderedObject(window.Loop1Disengage, (Button but) => { but.Enabled = true; });
            }
            if (loop2Off)
            {
                window.SetTextField(window.Loop2Status, "OFF");
                if (OK_Cryo)
                    window.UpdateRenderedObject(window.Loop2Engage, (Button but) => { but.Enabled = true; });
                else
                    window.UpdateRenderedObject(window.Loop2Engage, (Button but) => { but.Enabled = false; });
                window.UpdateRenderedObject(window.Loop2Disengage, (Button but) => { but.Enabled = false; });
            }
            else
            {
                window.SetTextField(window.Loop2Status, "ON");
                window.UpdateRenderedObject(window.Loop2Engage, (Button but) => { but.Enabled = false; });
                window.UpdateRenderedObject(window.Loop2Disengage, (Button but) => { but.Enabled = true; });
            }


            window.UpdateRenderedObject(window.Loop1Out, (ProgressBar bar) => { bar.Value = (int)loop1Out; });
            window.UpdateRenderedObject(window.Loop2Out, (ProgressBar bar) => { bar.Value = (int)loop2Out; });


            // Send data to ccmmonitoring
            InfluxDBDataLogger data = InfluxDBDataLogger.Measurement("TEC Output").Tag("name", "Eurotherm 1");
            if (loop1Off)
            {
                data.Field("Loop 1", 0d);
            }
            else
            {
                data.Field("Loop 1", loop1Out);
            }
            if (loop2Off)
            {
                data.Field("Loop 2", 0d);
            }
            else
            {
                data.Field("Loop 2", loop2Out);
            }

            data.Write("https://ccmmonitoring.ph.ic.ac.uk:8086", Environment.GetEnvironmentVariable("INFLUX_BUCKET"), "CentreForColdMatter");
        }

        private void UpdateCryoState()
        {

            
            bool OK_pressure = Convert.ToDouble(pressure1) < 1e-4 || !InterlocksActive;
            bool OK_heaters = !(window.Loop1Status.Text == "ON" || window.Loop2Status.Text == "ON") || !InterlocksActive;
            bool CRYO_off = false;
            lock (lakeshore)
            {
                CRYO_off = (System.Convert.ToInt32(lakeshore.QueryRelayStatus(1)) == 0);
            }
            if (CRYO_off)
            {
                window.SetTextField(window.CryoStatus, "OFF");
                if (OK_pressure && OK_heaters)
                    window.UpdateRenderedObject(window.EngageCryo, (Button but) => { but.Enabled = true; });
                else
                    window.UpdateRenderedObject(window.EngageCryo, (Button but) => { but.Enabled = false; });
                window.UpdateRenderedObject(window.DisengageCryo, (Button but) => { but.Enabled = false; });

            }
            else
            {
                window.SetTextField(window.CryoStatus, "ON");
                window.UpdateRenderedObject(window.EngageCryo, (Button but) => { but.Enabled = false; });
                window.UpdateRenderedObject(window.DisengageCryo, (Button but) => { but.Enabled = true; });
            }


            // Send data to ccmmonitoring
            InfluxDBDataLogger data = InfluxDBDataLogger.Measurement("Cryo state").Tag("name", "Cryo 1");
            data.Field("Cryo state", !CRYO_off);
            data.TimestampMS(DateTime.UtcNow);

            data.Write("https://ccmmonitoring.ph.ic.ac.uk:8086", Environment.GetEnvironmentVariable("INFLUX_BUCKET"), "CentreForColdMatter");

        }

        public event EventHandler MiscDataUpdate;

        public bool exiting = false;
        public ThreadSync DAQ_sync = new ThreadSync();
        private void UpdateData()
        {

            DAQ_sync.CreateDelegateThread(() => {
                try
                {
                    UpdateLakeshoreTemperature();
                    UpdateCryoState();
                }
                catch (Exception e) when (e is Ivi.Visa.NativeVisaException || e is Ivi.Visa.IOTimeoutException || e is AccessViolationException)
                {
                    lakeshore.Disconnect();
                    window.tSched.UpdateEventLog("Error in communicating with LakeShore:" + e.ToString());
                }
            });

            DAQ_sync.CreateDelegateThread(() => {
                try
                {
                    UpdatePressure();
                }
                catch (Exception e) when (e is Ivi.Visa.NativeVisaException || e is Ivi.Visa.IOTimeoutException || e is AccessViolationException)
                {
                    leybold.Disconnect();
                    window.tSched.UpdateEventLog("Error in communicating with Leybold pressure controller:" + e.ToString());
                }
            });

            DAQ_sync.CreateDelegateThread(() => {
                try
                {
                    UpdateTypeK();
                }
                catch (Exception e) when (e is Ivi.Visa.NativeVisaException || e is Ivi.Visa.IOTimeoutException || e is AccessViolationException)
                {
                    eurotherm.Disconnect();
                    window.tSched.UpdateEventLog("Error in communicating with EuroTherm:" + e.ToString());
                }
            });

            DAQ_sync.CreateDelegateThread(() => {
                MiscDataUpdate?.Invoke(this, null);
            });

            DAQ_sync.SwitchToData();
            while (!exiting)
            {
                System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
                DAQ_sync.SwitchToControl();
                
                DAQ_sync.SwitchToData();
                foreach (DataGrapher dg in window.graphers.Values)
                {
                    dg.UpdatePlot();
                }
                watch.Stop();
                if (watch.ElapsedMilliseconds < 500)
                    Thread.Sleep((int)(500 - watch.ElapsedMilliseconds));
            }
            DAQ_sync.JoinThreads();
        }

        #endregion

        #region External Control

        #endregion
    }
}
