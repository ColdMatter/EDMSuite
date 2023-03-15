using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NationalInstruments;

using DAQ.TransferCavityLock2012;

namespace TransferCavityLock2023
{
    class ScanData
    {
        public Dictionary<string, int> AIChannelsLookup { get; set; }
        public Dictionary<string, int> DIChannelsLookup { get; set; }
        public double[,] AIData { get; set; }
        public DigitalWaveform[] DIData { get; set; }
        private double[,] oldRampData;

        public ScanData(Dictionary<string, int> aiChannelsLookup, Dictionary<string, int> diChannelsLookup)
        {
            AIChannelsLookup = aiChannelsLookup;
            DIChannelsLookup = diChannelsLookup;
        }

        public void AddNewScan(TCLReadData data, bool shouldAverageRampData, int numAverages)
        {
            AIData = data.AnalogData;
            if (shouldAverageRampData)
            {
                averageRampData(numAverages);
            }
            else
            {
                oldRampData = null; // Reset averages
            }
            DIData = data.DigitalData;
        }

        private void averageRampData(int numAverages)
        {
            double[,] updatedOldRampData;
            if (oldRampData != null)
            {
                int oldRampDataNumRecords = oldRampData.GetLength(2);
                int updatedOldRampDataNumRecords = oldRampData.GetLength(2) >= numAverages ? numAverages : oldRampDataNumRecords + 1;
                updatedOldRampData = new double[updatedOldRampDataNumRecords, AIData.GetLength(1)];
                for (int record = 0; record < updatedOldRampDataNumRecords - 1; record++)
                {
                    for (int i = 0; i < updatedOldRampData.GetLength(1); i++)
                    {
                        updatedOldRampData[record, i] = oldRampData[record, i + 1];
                    }
                }
                for (int i = 0; i < updatedOldRampData.GetLength(1) - 1; i++)
                {
                    updatedOldRampData[updatedOldRampDataNumRecords - 1, i] = AIData[0, i];
                }

                double[] averageRampData = new double[updatedOldRampData.GetLength(1)];
                for (int i = 0; i < updatedOldRampData.GetLength(1) - 1; i++)
                {
                    double total = 0;
                    for (int record = 0; record < updatedOldRampDataNumRecords - 1; record++)
                    {
                        total += updatedOldRampData[record, i];
                    }
                    AIData[0, i] = total / updatedOldRampDataNumRecords;
                }
            }
            else
            {
                updatedOldRampData = new double[1, AIData.GetLength(1)];
                for (int i = 0; i < updatedOldRampData.GetLength(1) - 1; i++)
                {
                    updatedOldRampData[0, i] = AIData[0, i];
                }
            }
        }

        public double[] GetRampData()
        {
            double[] temp = new double[AIData.GetLength(1)];
            for (int i = 0; i < AIData.GetLength(1); i++)
            {
                temp[i] = AIData[0, i];
            }
            return temp;
        }

        public double[] GetLaserData(string laser)
        {
            double[] temp = new double[AIData.GetLength(1)];
            int channel = AIChannelsLookup[laser];
            for (int i = 0; i < AIData.GetLength(1); i++)
            {
                temp[i] = AIData[channel, i];
            }
            return temp;
        }

        public bool LaserLockBlocked(string laser)
        {
            bool lockBlocked = false;
            if (DIChannelsLookup.ContainsKey(laser))
            {
                int index = DIChannelsLookup[laser];
                DigitalWaveform waveform = DIData[index];
                lockBlocked = waveform.Signals[0].States.Any(x => x == DigitalState.ForceUp);
            }

            return lockBlocked;
        }
    }
}
