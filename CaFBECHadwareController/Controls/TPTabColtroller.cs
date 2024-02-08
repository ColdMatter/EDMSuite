using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NationalInstruments.DAQmx;
using System.Windows.Forms;
using DAQ.HAL;
using DAQ.Environment;
using NationalInstruments.UI.WindowsForms;

namespace CaFBECHadwareController.Controls
{
    public class TPTabController : GenericController
    {

        private TPTabView castView;
        public LakeShore336TemperatureController lakeshore = (LakeShore336TemperatureController)Environs.Hardware.Instruments["Lakeshore"];

        
        private System.Windows.Forms.Timer readTimer;
        public bool isCryoOn = false;
        public bool isHeaterOn = false;
        public bool isCycling = false;

        private static float maxTemperature = 290; 

        protected override GenericView CreateControl()
        {
            castView = new TPTabView(this);
            return castView;
        }


        public TPTabController()
        {
            (new Thread(() => InitReadTimer())).Start();
        }

        private void InitReadTimer() 
        {
            readTimer = new System.Windows.Forms.Timer();
            readTimer.Interval = 100;
            readTimer.Tick += new EventHandler(UpdateData);
        }


        public void ToggleReading()
        {
            if (!readTimer.Enabled)
            {
                readTimer.Start();
                castView.UpdateRenderedObject(castView.StartReading, (Button but) => { but.Text = "Stop"; });
            }
            else
            {
                readTimer.Stop();
                castView.UpdateRenderedObject(castView.StartReading, (Button but) => { but.Text = "Start"; });
            }
        }

        public void StopReading()
        {
            readTimer.Stop();
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
                if (heaterRelayState == "0")
                {
                    isHeaterOn = false;
                }
                else
                {
                    isHeaterOn = true;
                }
            }
            castView.Invoke((Action)(()=>
            {
                castView.led2.Value = isCryoOn;
                castView.led1.Value = isHeaterOn;
                castView.TempA.Text = TemperatureArray[0];
                castView.TempB.Text = TemperatureArray[1];
                castView.TempC.Text = TemperatureArray[2];
                castView.TempD.Text = TemperatureArray[3];
                castView.TempE.Text = TemperatureArray[4];
                castView.TempF.Text = TemperatureArray[5];
                castView.TempG.Text = TemperatureArray[6];
                castView.TempH.Text = TemperatureArray[7];
            }));
            
        }

        public void ToggleCryo()
        {
            if (isCryoOn == false)
            {
                isCryoOn = true;

                lakeshore.SetHeaterRange(1, 0);
                lakeshore.SetHeaterRange(2, 0);
                lakeshore.SetRelayParameters(2, 0);
                lakeshore.SetRelayParameters(1, 0);
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
                lakeshore.SetRelayParameters(2, 1);
                lakeshore.SetHeaterRange(1, 3);
                lakeshore.SetHeaterRange(2, 3);
            }
            else
            {
                isHeaterOn = false;
                lakeshore.SetHeaterRange(1, 0);
                lakeshore.SetHeaterRange(2, 0);
                lakeshore.SetRelayParameters(2, 0);
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

        protected void UpdateData(object anObject, EventArgs eventArgs)
        {

            UpdateLakeshoreTemperature();

            if (isCycling == true)
            {
                if (Convert.ToDouble(TemperatureArray[0]) < maxTemperature && Convert.ToDouble(TemperatureArray[1]) < maxTemperature && Convert.ToDouble(TemperatureArray[3]) < maxTemperature)
                {
                    lakeshore.SetRelayParameters(1, 1);
                    lakeshore.SetHeaterRange(1, 3);
                    lakeshore.SetHeaterRange(2, 3);
                    lakeshore.SetRelayParameters(2, 1);
                    castView.UpdateRenderedObject(castView.CycleButton, (Button but) => { but.Text = "Stop Cycle"; });
                }
                else
                {
                    isCycling = false;
                    lakeshore.SetHeaterRange(1, 0);
                    lakeshore.SetHeaterRange(2, 0);
                    lakeshore.SetRelayParameters(2, 0);
                    lakeshore.SetRelayParameters(1, 0);
                    castView.UpdateRenderedObject(castView.CycleButton, (Button but) => { but.Text = "Start Cycle"; });
                }
            }
        }

    }
}
