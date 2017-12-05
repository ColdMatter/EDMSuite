using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using MOTMaster2.SequenceData;
using Newtonsoft.Json;
using DAQ.Analyze;
using UtilsNS;
using ErrorManager;

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
            if (UtilsNS.Utils.isNull(grpMME)) return jm;
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

        public static AutoFileLogger multiScanParamLogger;
        public Stopwatch logWatch;
        private int batchNumber;
        public static Dictionary<string, object> lastData;
        public static AutoFileLogger multiScanDataLogger;

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
                    imin = entry.Value.Item1 + riseSamples/2;
                    imax = entry.Value.Item2 - riseSamples/2;
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

        public Dictionary<string,double> GetAverageValues(Dictionary<string,object> segData, bool fitN2 = false)
        {
            Dictionary<string,double> avgDict = new Dictionary<string, double>();
            double[] rawData;
            double mean = 0.0;
            double std =0.0;
            foreach (string name in segData.Keys)
            {
                rawData = (double[])segData[name];
                if (name != "AccV")
                {
                mean = rawData.Average();
                std = 0.0;
                for (int i = 0; i< rawData.Length; i++)
                {
                    std += (rawData[i]-mean)*(rawData[i]-mean);
                }
                std = Math.Sqrt(std/(rawData.Length-1));
                avgDict[name+"_mean"] = mean;
                avgDict[name+"_std"] = std;
                }
                else
                {
                    Dictionary<string,double> accDict = ConvertAccelerometerVoltage((double[])segData[name]);
                    if (avgDict.Count == 0) avgDict = accDict;
                    else avgDict.Concat(accDict).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
                if (name == "N2" && fitN2)
                {
                    double[] linearParams = LinearFit(rawData, 2);
                    avgDict[name+"_x0"] = linearParams[0];
                    avgDict[name+"_x1"] = linearParams[1];
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

        //Performs a Least-Sqaures optimisation to fit a linear model. Assumes a polynomial model with the highest degree given by paramNo
        public double[] LinearFit(double[] yData, int paramNo)
        {
            double[,] fMatrix = new double [yData.Length,paramNo];
            double[] xData = new double[yData.Length];
            int info;
            double[] c = new double[paramNo];
            alglib.lsfitreport report = new alglib.lsfitreport();
            for (int i = 0; i < fMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < fMatrix.GetLength(1); j++){
                    fMatrix[i,j] = Math.Pow(i * 1 / SampleRate,j);
                }
            }
            alglib.lsfitlinear(yData, fMatrix, out info, out c, out report);

            return c;
        }
        #region MultiScan
        /// <summary>
        /// MScan subroutines
        /// </summary>
        public bool CreateMScanLogger(string dir, Sequence segData, List<MMscan> mms)
        {
            if (Directory.Exists(dir))
            {
                MessageBox.Show(" Error message: Directory ("+dir+") already exists");
                return false;
            }
            Directory.CreateDirectory(dir);
            multiScanParamLogger = new AutoFileLogger(dir + "\\header.msn");
            multiScanParamLogger.Enabled = true; 
            foreach (string item in segData.Parameters.Keys)
            {
                multiScanParamLogger.log(item + '=' + segData.Parameters[item].Value.ToString());
            }
            string ss = "\n#.dta\t";
            for (int i = 0; i < mms.Count-1; i++)
            {
                ss += mms[i].sParam + "\t";
            }
            multiScanParamLogger.log(ss);
            batchNumber = 1;
            multiScanDataLogger = new AutoFileLogger(dir + "\\1.dta");
            if (Utils.isNull(logWatch)) logWatch = new Stopwatch();
            else logWatch.Reset();
            logWatch.Start();
            return true;
        }

        public void LogNextShot(List<MMscan> mms)
        {
            bool writeData = lastData != null;
            if (writeData)
            {
                foreach (var item in lastData.Where(kvp => kvp.Value.GetType() != typeof(double[])).ToList())
                {
                    lastData.Remove(item.Key);
                }
            }
            Dictionary<string, double> avgDict = (lastData == null) ? null : GetAverageValues(lastData, true);
            if (Utils.isNull(avgDict)) return;
            string ss;
            if (mms[mms.Count-1].isFirstValue())
            {
                ss = batchNumber.ToString("000") + "\t"; 
                for (int i = 0; i < mms.Count-1; i++)
                {
                    ss += mms[i].Value.ToString("G7") + '\t';
                }
                multiScanParamLogger.log(ss.TrimEnd('\t'));
                multiScanDataLogger.Enabled = false; // flush
                string dir = Path.GetDirectoryName(multiScanParamLogger.AutoSaveFileName);
                multiScanDataLogger.AutoSaveFileName = dir + "\\" + batchNumber.ToString("000") + ".dta";                
                multiScanDataLogger.Enabled = true;
                ss = mms[mms.Count - 1].sParam + "\tTime[ms]\t" ;
                foreach (var item in avgDict)
                {
                    ss += item.Key + '\t';
                }
                multiScanDataLogger.log(ss.TrimEnd('\t'));
                batchNumber += 1;
            }
            ss = mms[mms.Count - 1].Value.ToString("G7") + "\t"+ logWatch.ElapsedMilliseconds.ToString() + '\t';

            foreach (var item in avgDict)
            {
                ss += item.Value.ToString("G7") + '\t';
            }
            multiScanDataLogger.log(ss.TrimEnd('\t'));
        }

        public void StopMScanLogger() 
        {
            if (!Utils.isNull(multiScanDataLogger)) multiScanDataLogger.Enabled = false;
            if (!Utils.isNull(multiScanParamLogger)) multiScanParamLogger.Enabled = false;
        }

        public bool ImportMScanFile(string fn, out Dictionary<string, double> prms, out Dictionary<string, List<double>> scans)
        {
            bool rslt = true;
            prms = new Dictionary<string, double>();
            scans = new Dictionary<string, List<double>>();
            if (!File.Exists(fn))
            {
                ErrorMgr.errorMsg("File <" + fn + "> does not exist.", 1102);
                return false;
            }
            bool paramMode = true;
            List<string> sn = new List<string>();
            foreach (string line in File.ReadLines(fn)) 
            {
                if (line == "") continue;
                if (line[0] == '#')
                {
                    paramMode = false;
                    string[] ss = line.Split('\t');
                    for(int i=1; i<ss.Length; i++) 
                    {
                        if (ss[i].Equals("")) continue;
                        sn.Add(ss[i]);
                        scans[ss[i]] = new List<double>();
                    }
                    continue;
                }
                if (paramMode)
                {
                    string[] ss = line.Split('=');
                    prms[ss[0]] = Convert.ToDouble(ss[1]);
                }
                else
                {
                    string[] ss = line.Split('\t');
                    if (ss.Length != (sn.Count + 1))
                    {
                        rslt = false;
                        continue;
                    }
                    for (int i = 1; i < ss.Length; i++)
                    {
                        if (ss[i].Equals("")) continue;
                        scans[sn[i-1]].Add(Convert.ToDouble(ss[i]));
                    }
                }                   
            }
            return rslt;
        }
        #endregion
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
