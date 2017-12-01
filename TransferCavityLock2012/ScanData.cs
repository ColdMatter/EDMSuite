using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferCavityLock2012
{
    class ScanData
    {
        public Dictionary<string, int> ChannelsLookup { get; set;}
        public double[,] Data { get; set; }
        private double[,] oldRampData;

        public ScanData(Dictionary<string, int> channelsLookup)
        {
            ChannelsLookup = channelsLookup;
        }

        public void AddNewScan(double[,] data, bool shouldAverageRampData, int numAverages)
        {
            Data = data;
            if (shouldAverageRampData)
            {
                averageRampData(numAverages);
            }
            else
            {
                oldRampData = null; // Reset averages
            }
        }

        private void averageRampData(int numAverages)
        {
            double[,] updatedOldRampData;
            if (oldRampData != null)
            {
                int oldRampDataNumRecords = oldRampData.GetLength(2);
                int updatedOldRampDataNumRecords = oldRampData.GetLength(2) >= numAverages ? numAverages : oldRampDataNumRecords + 1;
                updatedOldRampData = new double[updatedOldRampDataNumRecords, Data.GetLength(1)];
                for (int record = 0; record < updatedOldRampDataNumRecords - 1; record++)
                {
                    for (int i = 0; i < updatedOldRampData.GetLength(1); i++)
                    {
                        updatedOldRampData[record, i] = oldRampData[record, i + 1];
                    }
                }
                for (int i = 0; i < updatedOldRampData.GetLength(1) - 1; i++)
                {
                    updatedOldRampData[updatedOldRampDataNumRecords - 1, i] = Data[0, i];
                }

                double[] averageRampData = new double[updatedOldRampData.GetLength(1)];
                for (int i = 0; i < updatedOldRampData.GetLength(1) - 1; i++)
                {
                    double total = 0;
                    for (int record = 0; record < updatedOldRampDataNumRecords - 1; record++)
                    {
                        total += updatedOldRampData[record, i];
                    }
                    Data[0, i] = total / updatedOldRampDataNumRecords;
                }
            }
            else
            {
                updatedOldRampData = new double[1, Data.GetLength(1)];
                for (int i = 0; i < updatedOldRampData.GetLength(1) - 1; i++)
                {
                    updatedOldRampData[0, i] = Data[0, i];
                }
            }
        }

        public double[] GetRampData()
        {
            double[] temp = new double[Data.GetLength(1)];
            for (int i = 0; i < Data.GetLength(1); i++)
            {
                temp[i] = Data[0, i];
            }
            return temp;
        }

        public double[] GetLaserData(string laser)
        {
            double[] temp = new double[Data.GetLength(1)];
            int channel = ChannelsLookup[laser];
            for (int i = 0; i < Data.GetLength(1); i++)
            {
                temp[i] = Data[channel, i];
            }
            return temp;
        }
    }
}
