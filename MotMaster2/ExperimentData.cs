using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MOTMaster2.SequenceData;
using Newtonsoft.Json;

namespace MOTMaster2
{
    [Serializable,JsonObject]
    public class ExperimentData
    {
        //Flag to save raw data or average from each segment
        public bool SaveRawData { get; set; }
        //Name to identify each experiment
        public string ExperimentName {get; set;}
        //Collects time indices for each segment of analog data as a tuple tStart,tEnd
        public Dictionary<string, Tuple<int, int>> AnalogSegments { get; set; }
        //Raw data recorded from each shot
        List<ExperimentShot> shotData = new List<ExperimentShot>();
        //List of sequence parameters for each shot
        List<Dictionary<string, object>> shotParams = new List<Dictionary<string, object>>();
        //Sampling rate for data
        public int SampleRate { get; set; }
        //Number of acquired samples
        public int NSamples { get; set; }
        public void AddExperimentShot(ExperimentShot shot,Dictionary<string,object> parameters)
        {
            shotData.Add(shot);
            shotParams.Add(parameters);
        }

        List<Dictionary<string,double>> AverageEachSegment()
        {
            List<Dictionary<string, double>> segAvgs = new List<Dictionary<string,double>>();
            foreach (ExperimentShot shot in shotData)
            {
                 segAvgs.Add(AverageShotSegments(shot));
            }
            return segAvgs;
        }

        /// <summary>
        /// Gets the average value of the analog input data for each time segment
        /// </summary>
        /// <param name="shot"></param>
        /// <returns></returns>
        private Dictionary<string,double> AverageShotSegments(ExperimentShot shot)
        {
            Dictionary<string, double> segmentAvg = new Dictionary<string, double>();
            Dictionary<string, double[]> segData = SegmentShot(shot);
            foreach (KeyValuePair<string,double[]> entry in segData)
            {
                segmentAvg[entry.Key] = entry.Value.Average();
            }
            return segmentAvg;
        }

        /// <summary>
        /// Segments a shot using the speicified analog time segments
        /// </summary>
        /// <param name="shot"></param>
        /// <returns></returns>
        public Dictionary<string,double[]> SegmentShot(ExperimentShot shot)
        {
            double[] rawData = new double[shot.analogInData.Length];
            for (int i = 0; i < rawData.Length; i++) rawData[i] = shot.analogInData[0, i];
            return SegmentShot(rawData);
        }

        public Dictionary<string, double[]> SegmentShot(double[] rawData)
        {
            Dictionary<string, double[]> segData = new Dictionary<string, double[]>();
            foreach (KeyValuePair<string, Tuple<int, int>> entry in AnalogSegments.OrderBy(t => t.Value.Item1))
            {
                double[] data = rawData.ToList().GetRange(entry.Value.Item1, entry.Value.Item2).ToArray();
                segData[entry.Key] = data;
            }
            return segData;
        }
        public void SaveData(string filePath)
        {
            //Save Parameters to a file
            string paramJson = JsonConvert.SerializeObject(shotParams,Formatting.Indented);
            //Saves raw data if flag set. Otherwise averages each segment and serialises that
            string dataJson = SaveRawData ? JsonConvert.SerializeObject(shotData, Formatting.Indented) : JsonConvert.SerializeObject(AverageEachSegment());
            File.WriteAllText(filePath + "_" + ExperimentName + "_params.sm3",paramJson);
            //Maybe save both raw data and averaged?
            File.WriteAllText(filePath + "_" + ExperimentName + "_data.sm3", dataJson);
        }

        //Useful when starting a new scan
        public void ClearData()
        {
            shotData.Clear();
            shotParams.Clear();
            ExperimentName = "";
        }

        //Generates some fake data that is normally distributed about some mean value
        public double[,] GenerateFakeData()
        {
            double[,] fakeData = new double[1, NSamples];
            for (int i = 0; i < NSamples; i++) { double g = Gauss(0, 1); fakeData[0, i] = g; Console.WriteLine(g); }
            return fakeData;
        }

        //Randomly generates normally distributed numbers using the BoxMuller transform
        public double Gauss(double mean, double std)
        {
            Random r = new Random();
            double u = 2 * r.NextDouble() - 1;
            double v = 2 * r.NextDouble() - 1;
            double w = u * u + v * v;
            if (w == 0 || w >= 1) return Gauss(mean, std);
            double c = Math.Sqrt(-2 * Math.Log(w) / w);
            return u * c * std + mean;
            
        }
    }

    /// <summary>
    /// Data from a single experiment shot
    /// </summary>
    [Serializable]
    public struct ExperimentShot
    {
        //Index of run. Might not be needed if adding each to a list
        internal int runID;
        //Single channel analog input data -- Extend to multi-channel?
        internal double[,] analogInData;

        public ExperimentShot(int id, double[,] data)
        {
            runID = id;
            analogInData = data;
        }
    }
}
