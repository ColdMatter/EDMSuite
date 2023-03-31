using DAQ.Environment;
using DAQ.HAL;
using DAQ;
using System.Windows.Forms;
using System.Threading;
using System;
using System.Windows.Forms.DataVisualization.Charting;
using Data;

namespace AlFHardwareControl
{
    public class AlFController
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

        public void Start()
        {

            window = new AlFControlWindow(this);
            window.controller = this;
            Application.Run(window);
        }

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

        public void WindowLoaded()
        {
            UpdateLakeshoreTemperature();
            (new Thread(() => UpdateData())).Start();
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
                loop1Off = eurotherm.GetAMSwitch(0);
                loop2Off = eurotherm.GetAMSwitch(1);

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

        }

        private void UpdateData()
        {

            ThreadSync DAQ_sync = new ThreadSync();
            DAQ_sync.CreateDelegateThread(() => {
                UpdateLakeshoreTemperature();
                UpdateCryoState();
            });

            DAQ_sync.CreateDelegateThread(() => {
                UpdatePressure();
            });

            DAQ_sync.CreateDelegateThread(() => {
                UpdateTypeK();
            });

            DAQ_sync.SwitchToData();
            for (; ; )
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
        }

    }
}
