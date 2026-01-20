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
    public class PTabController : GenericController
    {

        private PTabView castView;
        public PfeifferPressureGauge pfeiffer = (PfeifferPressureGauge)Environs.Hardware.Instruments["Pfeiffer"];

        protected override GenericView CreateControl()
        {
            castView = new PTabView(this);
            return castView;
        }

        
        public PTabController()
        {
            
        }

        private Thread pollThread;
        private int pollPeriod = 1000;
        private Object monitorLock;
        private bool monitorFlag = true;
        public void ToggleReading()
        {
            if (monitorFlag) StartReading();
            else StopReading();
        }

        public void StartReading()
        {
            pollThread = new Thread(new ThreadStart(PollWorker));
            castView.UpdateRenderedObject(castView.StartReading, (Button but) => { but.Text = "Stop"; });
            monitorLock = new Object();
            monitorFlag = false;
            pollPeriod = System.Convert.ToInt32(castView.PollPeriod.Text);
            pollThread.Start();
        }

        public void StopReading()
        {
            monitorFlag = true;
            castView.UpdateRenderedObject(castView.StartReading, (Button but) => { but.Text = "Start"; });
        }

        private void PollWorker()
        {
            for (; ; )
            {
                Thread.Sleep(pollPeriod);
                lock (monitorLock)
                {
                    UpdatePressures();
                    
                    if (monitorFlag)
                    {
                        monitorFlag = false;
                        break;
                    }
                }
            }
        }

        private double[] LastData = new double[6];

        private void UpdatePressures()
        {
            DateTime localDate = DateTime.Now;

            double[] pressures = new double[6];
            double p;

            for (int i = 0; i < pressures.Length; ++i)
            {
                p = pfeiffer.ReadPressure(i+1);
                if (p > 0) pressures[i] = p;
                else pressures[i] = this.LastData[i];
            }

            this.LastData = pressures;

            castView.Invoke((Action)(()=>
            {
                castView.PressureA.Text = Convert.ToString(pressures[0]);
                castView.PressureB.Text = Convert.ToString(pressures[1]);
                castView.PressureC.Text = Convert.ToString(pressures[2]);
                castView.PressureD.Text = Convert.ToString(pressures[3]);
                castView.PressureE.Text = Convert.ToString(pressures[4]);
                castView.PressureF.Text = Convert.ToString(pressures[5]);
                castView.UpdateRenderedObject<Chart>(castView.DataGraph, (Chart obj) => { obj.Series[castView.label1.Text].Points.AddXY(localDate, castView.PressureA.Text); });
                castView.UpdateRenderedObject<Chart>(castView.DataGraph, (Chart obj) => { obj.Series[castView.label2.Text].Points.AddXY(localDate, castView.PressureB.Text); });
                //castView.UpdateRenderedObject<Chart>(castView.DataGraph, (Chart obj) => { obj.Series[castView.label3.Text].Points.AddXY(localDate, castView.PressureC.Text); });
                castView.UpdateRenderedObject<Chart>(castView.DataGraph, (Chart obj) => { obj.Series[castView.label4.Text].Points.AddXY(localDate, castView.PressureD.Text); });
                castView.UpdateRenderedObject<Chart>(castView.DataGraph, (Chart obj) => { obj.Series[castView.label5.Text].Points.AddXY(localDate, castView.PressureE.Text); });
                castView.UpdateRenderedObject<Chart>(castView.DataGraph, (Chart obj) => { obj.Series[castView.label6.Text].Points.AddXY(localDate, castView.PressureF.Text); });
            }));
            
        }
    }
}
