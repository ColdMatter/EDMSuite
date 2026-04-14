using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using NationalInstruments.DAQmx;
using System.Windows.Forms;
using DAQ.HAL;
using DAQ.Environment;
using NationalInstruments.UI.WindowsForms;
using System.Windows.Forms.DataVisualization.Charting;

namespace CaFBECHardwareController.Controls
{
    public class TTabController : GenericController
    {

        private TTabView castView;
        public LakeShore336TemperatureController lakeshore = (LakeShore336TemperatureController)Environs.Hardware.Instruments["Lakeshore"];
        private readonly object heater_lock = new object();


        //private System.Timers.Timer readTimer;
        public bool isCryoOn = false;
        public bool isHeaterOn = false;
        public bool isCycling = false;

        private static float maxTemperature = 290; 

        protected override GenericView CreateControl()
        {
            castView = new TTabView(this);
            return castView;
        }

        
        public TTabController()
        {
            //(new Thread(() => InitReadTimer())).Start();
        }

        //private void InitReadTimer() 
        //{
        //    readTimer = new System.Timers.Timer();
        //    readTimer.Interval = 100;
        //    readTimer.Elapsed += new ElapsedEventHandler(UpdateData);
        //}

        private Thread lakeshorePollThread;
        private int lakeshorePollPeriod = 1000;
        private Object lakeshoreMonitorLock;
        private bool lakeshoreMonitorFlag = true;
        public void ToggleReading()
        {
            if (lakeshoreMonitorFlag) StartReading();
            else StopReading();
            //if (!readTimer.Enabled)
            //{
            //    readTimer.Start();
            //    castView.UpdateRenderedObject(castView.StartReading, (Button but) => { but.Text = "Stop"; });
            //}
            //else
            //{
            //    readTimer.Stop();
            //    castView.UpdateRenderedObject(castView.StartReading, (Button but) => { but.Text = "Start"; });
            //}
        }

        public void StartReading()
        {
            lakeshorePollThread = new Thread(new ThreadStart(LakeShorePollWorker));
            castView.UpdateRenderedObject(castView.StartReading, (Button but) => { but.Text = "Stop"; });
            lakeshoreMonitorLock = new Object();
            lakeshoreMonitorFlag = false;
            lakeshorePollPeriod = System.Convert.ToInt32(castView.PollPeriod.Text);
            lakeshorePollThread.Start();
        }

        public void StopReading()
        {
            lakeshoreMonitorFlag = true;
            castView.UpdateRenderedObject(castView.StartReading, (Button but) => { but.Text = "Start"; });
        }

        private void LakeShorePollWorker()
        {
            for (; ; )
            {
                Thread.Sleep(lakeshorePollPeriod);
                lock (lakeshoreMonitorLock)
                {
                    UpdateData();
                    
                    if (lakeshoreMonitorFlag)
                    {
                        lakeshoreMonitorFlag = false;
                        break;
                    }
                }
            }
        }

        public string cryoRelayState;
        public string heaterRelayState;


        private string receivedData;
        public string[] TemperatureArray;

        private void UpdateLakeshoreTemperature()
        {
            DateTime localDate = DateTime.Now;
            
            lock (lakeshore)
            {
                try
                {
                    receivedData = lakeshore.GetTemperature(0, "K");
                    TemperatureArray = receivedData.Split(',');
                    cryoRelayState = lakeshore.QueryRelayStatus(1);
                    heaterRelayState = lakeshore.QueryRelayStatus(2);
                }
                catch (Exception)
                { 

                }
                

                if (cryoRelayState == "0")
                {
                    isCryoOn = true;
                }
                else
                {
                    isCryoOn = false;
                }
                if (heaterRelayState == "1")
                {
                    isHeaterOn = false;
                }
                else
                {
                    isHeaterOn = true;
                }
            }

            try
            {
                castView.Invoke((Action)(() =>
                {
                    castView.toggleCryoLED(isCryoOn);
                    castView.toggleHeaterLED(isHeaterOn);
                    castView.TempA.Text = TemperatureArray[0];
                    castView.TempB.Text = TemperatureArray[1];
                    castView.TempC.Text = TemperatureArray[2];
                    castView.TempD.Text = TemperatureArray[3];
                    //castView.TempE.Text = TemperatureArray[4];
                    castView.TempF.Text = TemperatureArray[5];
                    castView.TempG.Text = TemperatureArray[6];
                    castView.TempH.Text = TemperatureArray[7];

                    castView.UpdateRenderedObject<Chart>(castView.DataGraph, (Chart obj) => { obj.Series[castView.label1.Text].Points.AddXY(localDate, castView.TempA.Text); });
                    castView.UpdateRenderedObject<Chart>(castView.DataGraph, (Chart obj) => { obj.Series[castView.label2.Text].Points.AddXY(localDate, castView.TempB.Text); });
                    castView.UpdateRenderedObject<Chart>(castView.DataGraph, (Chart obj) => { obj.Series[castView.label3.Text].Points.AddXY(localDate, castView.TempC.Text); });
                    castView.UpdateRenderedObject<Chart>(castView.DataGraph, (Chart obj) => { obj.Series[castView.label4.Text].Points.AddXY(localDate, castView.TempD.Text); });
                    //castView.UpdateRenderedObject<Chart>(castView.DataGraph, (Chart obj) => { obj.Series[castView.label5.Text].Points.AddXY(localDate, castView.TempE.Text); });
                    castView.UpdateRenderedObject<Chart>(castView.DataGraph, (Chart obj) => { obj.Series[castView.label6.Text].Points.AddXY(localDate, castView.TempF.Text); });
                    castView.UpdateRenderedObject<Chart>(castView.DataGraph, (Chart obj) => { obj.Series[castView.label7.Text].Points.AddXY(localDate, castView.TempG.Text); });
                    castView.UpdateRenderedObject<Chart>(castView.DataGraph, (Chart obj) => { obj.Series[castView.label8.Text].Points.AddXY(localDate, castView.TempH.Text); });
                }));
            }

            catch
            {

            }
            
            
        }

        public void ToggleCryo()
        {
            if (isCryoOn == false)
            {
                isCryoOn = true;

                lakeshore.SetHeaterRange(1, 0);
                lakeshore.SetHeaterRange(2, 0);
                lakeshore.SetRelayParameters(2, 1);
                lakeshore.SetRelayParameters(1, 0);
                ToggleHeaterRelay(false);
            }
            else
            {
                isCryoOn = false;
                lakeshore.SetRelayParameters(1, 1);
            }
        }

        public void ToggleHeater()
        {
            if (isHeaterOn == false)
            {
                isHeaterOn = true;
                lakeshore.SetRelayParameters(1, 1);
                lakeshore.SetRelayParameters(2, 0);
                lakeshore.SetHeaterRange(1, 3);
                lakeshore.SetHeaterRange(2, 3);
                ToggleHeaterRelay(true);
            }
            else
            {
                isHeaterOn = false;
                lakeshore.SetHeaterRange(1, 0);
                lakeshore.SetHeaterRange(2, 0);
                lakeshore.SetRelayParameters(2, 1);
                ToggleHeaterRelay(false);
            }
        }

        public void ToggleCycleSource()
        {
            if (isCycling == true)
            {
                isCycling = false;
                castView.UpdateRenderedObject(castView.CycleButton, (Button but) => { but.Text = "Start Cycle"; });
            }
            else {
                isCycling = true;
                castView.UpdateRenderedObject(castView.CycleButton, (Button but) => { but.Text = "Stop Cycle"; });
            }
        }

        //protected void UpdateData(object anObject, EventArgs eventArgs)
        protected void UpdateData()
        {

            UpdateLakeshoreTemperature();

            if (isCycling == true)
            {
                if (Convert.ToDouble(TemperatureArray[0]) < maxTemperature || Convert.ToDouble(TemperatureArray[1]) < maxTemperature)
                {
                    lakeshore.SetRelayParameters(1, 1);
                    lakeshore.SetHeaterRange(1, 3);
                    lakeshore.SetHeaterRange(2, 3);
                    lakeshore.SetRelayParameters(2, 0);
                    ToggleHeaterRelay(true);
                    castView.UpdateRenderedObject(castView.CycleButton, (Button but) => { but.Text = "Stop Cycle"; });
                }
                else
                {
                    isCycling = false;
                    lakeshore.SetHeaterRange(1, 0);
                    lakeshore.SetHeaterRange(2, 0);
                    lakeshore.SetRelayParameters(2, 1);
                    lakeshore.SetRelayParameters(1, 0);
                    ToggleHeaterRelay(false);
                    castView.UpdateRenderedObject(castView.CycleButton, (Button but) => { but.Text = "Start Cycle"; });
                }
            }

        }

        private void ToggleHeaterRelay(bool heaterState)
        {
            lock (heater_lock)
            {
                Task analogOutTask = new Task();
                ((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["40KHeaterSwitch"]).AddToTask(analogOutTask, 0, 5);

                AnalogSingleChannelWriter analogwriter = new AnalogSingleChannelWriter(analogOutTask.Stream);
                if (heaterState == true)
                    analogwriter.WriteSingleSample(true, 5.0);
                else
                    analogwriter.WriteSingleSample(true, 0.0);

                analogOutTask.Stop();
                analogOutTask.Dispose();
            }
        }

    }
}
