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

namespace TransferCavityLock2012
{

    //I tried to bundle everything to do with a single scan (scan parameters as well as data) into the same class.
    //I might have overdone the inheritance thing, but I was practising. The base class keeps parameters 
    //concerning the cavity ramp. Then a few other parameters about the scan get added to the 
    //scan parameters, then this bit holds the data.

    class CavityScanData
    {
        public double[,] AIData;
        public ScanParameters parameters = new ScanParameters();

        public CavityScanData(int steps, int numberOfChannels)
        {
            AIData = new double[numberOfChannels, steps];
            parameters.Steps = steps;
            parameters.NumberOfAIChannels = numberOfChannels;
       }

        public double[] MasterPhotodiodeData
        {
            get
            {
                double[] temp = new double[AIData.GetLength(1)];
                for (int i = 0; i < AIData.GetLength(1); i++)
                {
                    temp[i] = AIData[0, i];
                }
                return temp;
            }
            set
            {
                double[] temp = new double[AIData.GetLength(1)];
                for (int i = 0; i < AIData.GetLength(1); i++)
                {
                    AIData[0, i] = value[i];
                }
            }
        }
        public double[] SlavePhotodiodeData
        {
            get
            {
                double[] temp = new double[AIData.GetLength(1)];
                for (int i = 0; i < AIData.GetLength(1); i++)
                {
                    temp[i] = AIData[1, i];
                }
                return temp;
            }
            set
            {
                double[] temp = new double[AIData.GetLength(1)];
                for (int i = 0; i < AIData.GetLength(1); i++)
                {
                    AIData[1, i] = value[i];
                }
            }
        }
        public double[] CavityData
        {
            get
            {
                double[] temp = new double[AIData.GetLength(1)];
                for (int i = 0; i < AIData.GetLength(1); i++)
                {
                    temp[i] = AIData[2, i];
                }
                return temp;
            }
            set
            {
                double[] temp = new double[AIData.GetLength(1)];
                for (int i = 0; i < AIData.GetLength(1); i++)
                {
                    AIData[2, i] = value[i];
                }
            }
        }

 

    }
}
