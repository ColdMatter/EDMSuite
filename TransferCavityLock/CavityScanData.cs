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

namespace TransferCavityLock
{

    //I tried to bundle everything to do with a single scan (scan parameters as well as data) into the same class.
    //I might have overdone the inheritance thing, but I was practising. The base class keeps parameters 
    //concerning the cavity ramp. Then a few other parameters about the scan get added to the 
    //scan parameters, then this bit holds the data.

    class CavityScanData
    {
        public double[,] PhotodiodeData;
        public ScanParameters parameters = new ScanParameters();

        public CavityScanData(int Steps, int numberOfDatasets)
        {
            PhotodiodeData = new double[numberOfDatasets, Steps];
            parameters.Steps = Steps;
       }

        public double[] MasterPhotodiodeData
        {
            get
            {
                double[] temp = new double[PhotodiodeData.GetLength(1)];
                for (int i = 0; i < PhotodiodeData.GetLength(1); i++)
                {
                    temp[i] = PhotodiodeData[0, i];
                }
                return temp;
            }
            set
            {
                double[] temp = new double[PhotodiodeData.GetLength(1)];
                for (int i = 0; i < PhotodiodeData.GetLength(1); i++)
                {
                    PhotodiodeData[0, i] = value[i];
                }
            }
        }
        public double[] SlavePhotodiodeData
        {
            get
            {
                double[] temp = new double[PhotodiodeData.GetLength(1)];
                for (int i = 0; i < PhotodiodeData.GetLength(1); i++)
                {
                    temp[i] = PhotodiodeData[1, i];
                }
                return temp;
            }
            set
            {
                double[] temp = new double[PhotodiodeData.GetLength(1)];
                for (int i = 0; i < PhotodiodeData.GetLength(1); i++)
                {
                    PhotodiodeData[1, i] = value[i];
                }
            }
        }
        public double[] CavityData
        {
            get
            {
                double[] temp = new double[PhotodiodeData.GetLength(1)];
                for (int i = 0; i < PhotodiodeData.GetLength(1); i++)
                {
                    temp[i] = PhotodiodeData[2, i];
                }
                return temp;
            }
            set
            {
                double[] temp = new double[PhotodiodeData.GetLength(1)];
                for (int i = 0; i < PhotodiodeData.GetLength(1); i++)
                {
                    PhotodiodeData[2, i] = value[i];
                }
            }
        }

 

    }
}
