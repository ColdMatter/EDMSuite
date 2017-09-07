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

        public List<string> IgnoredSegments { get; set; }
        //Raw data recorded from each shot
        List<ExperimentShot> shotData = new List<ExperimentShot>();
        //List of sequence parameters for each shot
        List<Dictionary<string, object>> shotParams = new List<Dictionary<string, object>>();
        //Sampling rate for data
        private int sampleRate = 100000;
        public int SampleRate { get { return sampleRate; } set { sampleRate = value; } }
        //Number of acquired samples
        public int NSamples { get; set; }
        private Random random = new Random();

        //Rise time in seconds to be excluded from data
        public double RiseTime { get; set; }
        //public void AddExperimentShot(ExperimentShot shot,Dictionary<string,object> parameters)
        //{
        //    if (AnalogSegments != null) shot.analogSegments = SegmentShot(shot);
        //    shotData.Add(shot);
        //    shotParams.Add(parameters);
        //}

        //List<Dictionary<string,double>> AverageEachSegment()
        //{
        //    List<Dictionary<string, double>> segAvgs = new List<Dictionary<string,double>>();
        //    foreach (ExperimentShot shot in shotData)
        //    {
        //         segAvgs.Add(AverageShotSegments(shot));
        //    }
        //    return segAvgs;
        //}

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
            double[] rawData = shot.analogInData;
            return SegmentShot(rawData);
        }

        public Dictionary<string, double[]> SegmentShot(double[] rawData)
        {
            int riseSamples = (int)(RiseTime * SampleRate);
            Dictionary<string, double[]> segData = new Dictionary<string, double[]>();
            foreach (KeyValuePair<string, Tuple<int, int>> entry in AnalogSegments.OrderBy(t => t.Value.Item1))
            {
                double[] data = rawData.ToList().GetRange(entry.Value.Item1+riseSamples, entry.Value.Item2-entry.Value.Item1-1*riseSamples).ToArray();
                if(!IgnoredSegments.Contains(entry.Key))segData[entry.Key] = data;
            }
            return segData;
        }
        //Useful when starting a new scan
        public void ClearData()
        {
            shotData.Clear();
            shotParams.Clear();
            ExperimentName = "";
        }

        //Generates some fake data that is normally distributed about some mean value
        public double[] GenerateFakeData()
        {
            double[] fakeData = new double[NSamples];
            for (int i = 0; i < NSamples; i++) { double g = Gauss(0, 1); fakeData[i] = g;}
            return fakeData;
        }

        //Randomly generates normally distributed numbers using the BoxMuller transform
        public double Gauss(double mean, double std)
        {
            
            double u = 2 * random.NextDouble() - 1;
            double v = 2 * random.NextDouble() - 1;
            double w = u * u + v * v;
            if (w == 0 || w >= 1) return Gauss(mean, std);
            double c = Math.Sqrt(-2 * Math.Log(w) / w);
            return u * c * std + mean;
            
        }

        private int preTrigSamples = 64;

        public int PreTrigSamples { get { return preTrigSamples; } set { preTrigSamples = value; } }
    }

    /// <summary>
    /// Data from a single experiment shot
    /// </summary>
    [Serializable,JsonObject]
    public struct ExperimentShot
    {
        //Index of run. Might not be needed if adding each to a list
        public int runID;

        //Single channel analog input data -- Extend to multi-channel?
        [JsonIgnore]
        internal double[] analogInData;

        public Dictionary<string, double[]> analogSegments;

        public ExperimentShot(int id, double[] data)
        {
            runID = id;
            analogInData = data;
            analogSegments = null;
        }
    }
}
