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
        public MMexec grpMME = new MMexec();
        public enum JumboModes { none, scan, repeat };
        public JumboModes jumboMode()
        {
             JumboModes jm = JumboModes.none;
            if (grpMME.sender.Equals("Axel-hub"))
            {
                if (grpMME.cmd.Equals("scan")) jm = JumboModes.scan;
                if (grpMME.cmd.Equals("repeat")) jm = JumboModes.repeat;
            }
            return jm;
        }
        //Collects time indices for each segment of analog data as a tuple tStart,tEnd
        public Dictionary<string, Tuple<int, int>> AnalogSegments { get; set; }

        public List<string> IgnoredSegments { get; set; }
        //Raw data recorded from each shot
        List<ExperimentShot> shotData = new List<ExperimentShot>();
        //List of sequence parameters for each shot
        List<Dictionary<string, object>> shotParams = new List<Dictionary<string, object>>();
        //Sampling rate for data
        private int sampleRate = 200000;
        public int SampleRate { get { return sampleRate; } set { sampleRate = value; } }
        //Number of acquired samples
        public int NSamples { get; set; }
        private Random random = new Random();
        public string InterferometerStepName { get; set; }
        //Rise time in seconds to be excluded from data
        public double RiseTime { get; set; }

        //This depends on the number of channels and sampling rate
        private int preTrigSamples = 32;
        public int PreTrigSamples { get { return preTrigSamples; } set { preTrigSamples = value; } }

        public static double[] TransferFunc { get; set; }

        public ExperimentData()
        {
           
           
        }

        public Dictionary<string, double[]> SegmentShot(double[,] rawData)
        {
            int riseSamples = (int)(RiseTime * SampleRate);
            int imin;
            int imax;
            Dictionary<string, double[]> segData = new Dictionary<string, double[]>();
            foreach (KeyValuePair<string, Tuple<int, int>> entry in AnalogSegments.OrderBy(t => t.Value.Item1))
            {
                if (!IgnoredSegments.Contains(entry.Key))
                {
                    imin = entry.Value.Item1 + riseSamples;
                    imax = entry.Value.Item2;
                    double[] data = new double[imax-imin];
                    for (int i = imin; i < imax; i++) data[i-imin] = rawData[1,i];
                    segData[entry.Key] = data;
                }
                else if (entry.Key == InterferometerStepName)
                {
                    imin = entry.Value.Item1;
                    imax = entry.Value.Item2;
                    double[] accelData = new double[imax-imin];
                    for (int i = imin; i < imax; i++) accelData[i - imin] = rawData[1, i];
                    segData["AccV"] = accelData;
                }
            }
            return segData;
        }

        /// <summary>
        /// Converts the accelerometer voltage into acceleration and integrates it using the interferometer response function
        /// </summary>
        /// <param name="segData"></param>
        /// <param name="accelData"></param>
        private Dictionary<string,double> ConvertAccelerometerVoltage(double[] accelData)
        {
            Dictionary<string,double> accDict = new Dictionary<string,double>();
            double keff = 4 * Math.PI / (780 * 1e-9);
            double accScale = 1.235976 * 1e-3 * 6 * 1e3 / 9.81;// V/ms^-2
            int nAccSamps = accelData.Length;
            double accSum = 0.0;
            //Uses the simple triangular form of the transfer function and trapezium rule to integrate
            for (int i = 1; i < nAccSamps / 2; i++) accSum += i* accelData[i];
            for (int i = nAccSamps / 2 + 1; i < nAccSamps; i++) accSum += (nAccSamps - i) * accelData[i];

            double accPhase = keff * accSum/(accScale * sampleRate * sampleRate); // 1/sampleRate comes from both transfer function and integration
            
            double accMean = accelData.Average();
            double accStd = 0.0;
            for (int i = 0; i < accelData.Length; i++) accStd += accelData[i]*accelData[i];
            accStd = accStd/(nAccSamps-1);
            accStd = Math.Sqrt(accStd - accMean*accMean);

            accDict["AccPhase"] = accPhase;
            accDict["AccV_mean"] = accMean;
            accDict["AccV_std"] = accStd;
            accDict["AccA_mean"] = accMean/accScale;
            accDict["AccA_std"] = accStd/accScale;
            return accDict;
        }

        public Dictionary<string,double> GetAverageValues(Dictionary<string,object> segData)
        {
            Dictionary<string,double> avgDict = new Dictionary<string, double>();
            double[] rawData;
            double mean = 0.0;
            double std =0.0;
            foreach (string name in segData.Keys)
            {
                if (name != "AccV")
                {
                rawData = (double[])segData[name];
                mean = rawData.Average();
                std = 0.0;
                for (int i = 0; i< rawData.Length; i++)
                {
                    std += rawData[i]*rawData[i];
                }
                std = Math.Sqrt((std-mean*mean)/(rawData.Length-1));
                avgDict[name+"_mean"] = mean;
                avgDict[name+"_std"] = std;
                }
                else
                {
                    Dictionary<string,double> accDict = ConvertAccelerometerVoltage((double[])segData[name]);
                    avgDict.Concat(accDict).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
            }
            return avgDict;
        }
        //Useful when starting a new scan
        public void ClearData()
        {
            shotData.Clear();
            shotParams.Clear();
            ExperimentName = null;
        }

        //Generates some fake data that is normally distributed about some mean value
        public double[,] GenerateFakeData()
        {
            double[,] fakeData = new double[2,NSamples];
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < NSamples; j++) { double g = Gauss(0, 1); fakeData[i,j] = g; }
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
        }

    /// <summary>
    /// Data from a single experiment shot
    /// </summary>
    [Serializable,JsonObject]
    public struct ExperimentShot
    {
        //Index of run. Might not be needed if adding each to a list
        public int runID;

        [JsonIgnore]
        internal double[,] analogInData;

        public Dictionary<string, double[]> analogSegments;

        public ExperimentShot(int id, double[,] data)
        {
            runID = id;
            analogInData = data;
            analogSegments = null;
        }
    }
}
