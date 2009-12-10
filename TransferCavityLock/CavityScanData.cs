using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using Data;
using Data.Scans;
using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;
using DAQ.Remoting;
using System.Windows.Forms;
using NationalInstruments.Analysis.Math;

namespace TransferCavityLock
{
    class CavityScanData
    {
        private double[] voltages, p1Data, p2Data;
        
        public CavityScanData(int steps)
        {
            this.voltages = new double[steps];
            this.p1Data = new double[steps];
            this.p2Data = new double[steps];
        }

        public double[] Voltages
        {
            set { voltages = value; }
            get { return voltages; }
        }
        public double[] P1Data
        {
            set { p1Data = value; }
            get { return p1Data; }
        }
        public double[] P2Data
        {
            set { p2Data = value; }
            get { return p2Data; }
        }

        public void PrepareData(ScanParameters parameters)
        {
            parameters.AdjustStepSize();
            for (int i = 0; i < parameters.Steps; i++)
            {
                this.voltages[i] = parameters.Low + i * parameters.StepSize;
                this.p1Data[i] = 0;
                this.p2Data[i] = 0;
            }
        }
        
        public double[,] ConvertCavityDataToDoublesArray()
        {
           double[,] rd = new double[3, this.voltages.GetLength(0)];
           for (int i = 0; i < this.voltages.GetLength(0); i++)
           {
               rd[0, i] = this.voltages[i];
               rd[1, i] = this.p1Data[i];
               rd[2, i] = this.p2Data[i];
           }
           return rd;
        }

    }
}
