using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

using Analysis.EDM;
using EDMConfig;

namespace Data.EDM
{
	[Serializable]
	public class Block : MarshalByRefObject
	{
		public int Version = 4;
		private ArrayList points = new ArrayList();
		private DateTime timeStamp = DateTime.Now;
		private BlockConfig config = new BlockConfig();

        // ratio of distance from source for the two detectors
        private static double kDetectorDistanceRatioClassicEDM = 1715.0 / 1500.0; // For ClassicEDM
        private static double kDetectorDistanceRatioUEDM = 3903.0 / 3513.0; // For UEDM

        public List<string> detectors = new List<string>();

        public List<string> GetPointDetectors()
        {
            EDMPoint point = (EDMPoint)points[0];
            List<string> pointDetectorList = new List<string>();
            foreach (string key in point.SinglePointData.Keys)
            {
                //XmlSerialisableHashtables have "dummy" as a default entry.
                if (key != "dummy") pointDetectorList.Add(key);
            }
            return pointDetectorList;
        }

		public void SetTimeStamp()
		{
			timeStamp = DateTime.Now;
		}

		public DateTime TimeStamp
		{
			get { return timeStamp; }
			set { timeStamp = value; }
		}

		/// <summary>
		/// The shots that make up the block, in order.
		/// </summary>
		[XmlArray]
		[XmlArrayItem(Type = typeof(EDMPoint))]
		public ArrayList Points
		{
			get { return points; }
		}

		public BlockConfig Config
		{
			get { return config; }
			set { config = value; }
		}

		public double[] GetTOFIntegralArray(int index, double startTime, double endTime)
		{
			double[] temp = new double[points.Count];
			for (int i = 0 ; i < points.Count ; i++) temp[i] =
								(double)((EDMPoint)points[i]).Shot.Integrate(index, startTime, endTime);
			return temp;
		}

        public double[] GetTOFMeanArray(int index, double startTime, double endTime)
        {
            double[] temp = new double[points.Count];
            for (int i = 0; i < points.Count; i++) temp[i] =
                              (double)((EDMPoint)points[i]).Shot.GatedMean(index, startTime, endTime);
            return temp;
        }

        public TOF GetAverageTOF(int index)
		{
			TOF temp = new TOF();
			for (int i = 0 ; i < points.Count ; i++) temp += (TOF)((EDMPoint)points[i]).Shot.TOFs[index];

			return temp /(points.Count);
		}

        public List<double> GetSPData(string channel)
        {
            List<double> d = new List<double>();
            foreach (EDMPoint p in this.Points) d.Add((double)p.SinglePointData[channel]);
            return d;
        }

        // This function adds background-subtracted TOFs, scaled bottom probe TOF, and asymmetry TOF to the block
        // Also has the option of converting single point data to TOFs
        public void AddDetectorsToBlock()
        {
            SubtractBackgroundFromProbeDetectorTOFs(50000, 114590, 50000, 114590);    //Jan 2024 temporarily changed this for UEDM

            CreateScaledDetA();

            ConstructAsymmetryTOF();

            TOFuliseSinglePointData();
        }

        public void AddDetectorsToMagBlock()
        {
            TOFuliseSinglePointData();
        }

        // this function adds a new set of detector data to the block, constructed
        // by calculating the asymmetry of the top and bottom detectors (which must
        // be scaled first)
        public void ConstructAsymmetryTOF()
        {
            for (int i = 0; i < points.Count; i++)
            {
                Shot shot = ((EDMPoint)points[i]).Shot;
                //int bottomScaledIndex = detectors.IndexOf("bottomProbeScaled");               //ClassicEDM
                //int topIndex = detectors.IndexOf("topProbeNoBackground");
                //TOF asymmetry = ((TOF)shot.TOFs[bottomScaledIndex] - (TOF)shot.TOFs[topIndex]) / ((TOF)shot.TOFs[bottomScaledIndex] + (TOF)shot.TOFs[topIndex]);
                int AScaledIndex = detectors.IndexOf("detAScaled");               //UEDM
                int BIndex = detectors.IndexOf("detBNoBackground");
                TOF asymmetry = ((TOF)shot.TOFs[AScaledIndex] - (TOF)shot.TOFs[BIndex]) / ((TOF)shot.TOFs[AScaledIndex] + (TOF)shot.TOFs[BIndex]);
                asymmetry.Calibration = 1;
                shot.TOFs.Add(asymmetry);
            }
            // give these data a name
            detectors.Add("asymmetry");
        }

        public void ConstructAsymmetryShotNoiseTOF()
        {
            for (int i = 0; i < points.Count; i++)
            {
                EDMPoint point = (EDMPoint)points[i];
                Shot shot = point.Shot;

                //TOF bottomScaled = (TOF)shot.TOFs[detectors.IndexOf("bottomProbeScaled")];
                //TOF top = (TOF)shot.TOFs[detectors.IndexOf("topProbeNoBackground")];
                TOF detAScaled = (TOF)shot.TOFs[detectors.IndexOf("detAScaled")];
                TOF detB = (TOF)shot.TOFs[detectors.IndexOf("detBNoBackground")];

                // Multiply TOFs by their calibrations
                //bottomScaled *= bottomScaled.Calibration;
                //top *= top.Calibration;
                detAScaled *= detAScaled.Calibration;
                detB *= detB.Calibration;

                // Need the total signal TOF for later calculations
                //TOF total = bottomScaled + top;
                TOF total = detAScaled + detB;

                // Get background counts
                //double topLaserBackground = (double)point.SinglePointData["TopDetectorBackground"] * bottomScaled.Calibration;
                //double bottomLaserBackground = (double)point.SinglePointData["BottomDetectorBackground"] * top.Calibration;
                double detBLaserBackground = (double)point.SinglePointData["DetectorBBackground"] * detB.Calibration;
                double detALaserBackground = (double)point.SinglePointData["DetectorABackground"] * detAScaled.Calibration;

                // Calculate the shot noise variance in the asymmetry detector
                //TOF asymmetryVariance =
                //    bottomScaled * bottomScaled * top * 4.0
                //    + bottomScaled * top * top * 4.0
                //    + top * top * bottomLaserBackground * 8.0
                //    + bottomScaled * bottomScaled * topLaserBackground * 8.0;
                TOF asymmetryVariance =
                    detAScaled * detAScaled * detB * 4.0
                    + detAScaled * detB * detB * 4.0
                    + detB * detB * detALaserBackground * 8.0
                    + detAScaled * detAScaled * detBLaserBackground * 8.0;
                asymmetryVariance /= total * total * total * total;
                shot.TOFs.Add(asymmetryVariance);
            }

            detectors.Add("asymmetryShotNoiseVariance");
        }

        // this function scales up the bottom detector to match the top
        public void CreateScaledBottomProbe()
        {
            for (int i = 0; i < points.Count; i++)
            {
                Shot shot = ((EDMPoint)points[i]).Shot;
                int bottomIndex = detectors.IndexOf("bottomProbeNoBackground");
                int topIndex = detectors.IndexOf("topProbeNoBackground");
                TOF bottomTOF = (TOF)shot.TOFs[bottomIndex];
                TOF topTOF = (TOF)shot.TOFs[topIndex];
                TOF bottomScaled = TOF.ScaleTOFInTimeToMatchAnotherTOF(bottomTOF, topTOF, kDetectorDistanceRatioClassicEDM);
                bottomScaled.Calibration = bottomTOF.Calibration;
                shot.TOFs.Add(bottomScaled);
            }
            // give these data a name
            detectors.Add("bottomProbeScaled");
        }

        public void CreateScaledDetA()
        {
            for (int i = 0; i < points.Count; i++)
            {
                Shot shot = ((EDMPoint)points[i]).Shot;
                int AIndex = detectors.IndexOf("detANoBackground");
                int BIndex = detectors.IndexOf("detBNoBackground");
                TOF ATOF = (TOF)shot.TOFs[AIndex];
                TOF BTOF = (TOF)shot.TOFs[BIndex];
                TOF bottomScaled = TOF.ScaleTOFInTimeToMatchAnotherTOF(ATOF, BTOF, kDetectorDistanceRatioUEDM);
                bottomScaled.Calibration = ATOF.Calibration;
                shot.TOFs.Add(bottomScaled);
            }
            // give these data a name
            detectors.Add("detAScaled");
        }

        // this function subtracts the background off a TOF signal
        // the background is taken as the mean of a background array of points
        public void SubtractBackgroundFromProbeDetectorTOFs()
        {
            for (int i = 0; i < points.Count; i++)
            {
                EDMPoint point = (EDMPoint)points[i];
                Shot shot = point.Shot;
                TOF t = (TOF)shot.TOFs[0];
                double bg = t.GatedMeanAndUncertainty(2800, 2900)[0];
                TOF bgSubtracted = t - bg;

                // if value if negative, set to zero
                for (int j = 0; j < bgSubtracted.Length; j++)
                {
                    if (bgSubtracted.Data[j] < 0) bgSubtracted.Data[j] = 0.0;
                }

                bgSubtracted.Calibration = t.Calibration;
                shot.TOFs.Add(bgSubtracted);
                point.SinglePointData.Add("BottomDetectorBackground", bg);
            }
            // give these data a name
            detectors.Add("bottomProbeNoBackground");

            for (int i = 0; i < points.Count; i++)
            {
                EDMPoint point = (EDMPoint)points[i];
                Shot shot = point.Shot;
                TOF t = (TOF)shot.TOFs[1];
                double bg = t.GatedMeanAndUncertainty(3200, 3300)[0];
                TOF bgSubtracted = t - bg;

                // if value if negative, set to zero
                for (int j = 0; j < bgSubtracted.Length; j++)
                {
                    if (bgSubtracted.Data[j] < 0) bgSubtracted.Data[j] = 0.0;
                }

                bgSubtracted.Calibration = t.Calibration;
                shot.TOFs.Add(bgSubtracted);
                point.SinglePointData.Add("TopDetectorBackground", bg);
            }
            // give these data a name
            detectors.Add("topProbeNoBackground");
        }

        public void SubtractBackgroundFromProbeDetectorTOFs(double gateAst, double gateAend, double gateBst, double gateBend)

        {
            for (int i = 0; i < points.Count; i++)
            {
                EDMPoint point = (EDMPoint)points[i];
                Shot shot = point.Shot;
                TOF t = (TOF)shot.TOFs[0];
                double bg = t.GatedMeanAndUncertainty(gateAst, gateAend)[0];
                TOF bgSubtracted = t - bg;

                // if value if negative, set to zero
                for (int j = 0; j < bgSubtracted.Length; j++)
                {
                    if (bgSubtracted.Data[j] < 0) bgSubtracted.Data[j] = 0.0;
                }

                bgSubtracted.Calibration = t.Calibration;
                shot.TOFs.Add(bgSubtracted);
                //point.SinglePointData.Add("BottomDetectorBackground", bg);    // ClassicEDM
                point.SinglePointData.Add("DetectorABackground", bg);           // UEDM
            }
            // give these data a name
            //detectors.Add("bottomProbeNoBackground");     // ClassicEDM
            detectors.Add("detANoBackground");              // UEDM

            for (int i = 0; i < points.Count; i++)
            {
                EDMPoint point = (EDMPoint)points[i];
                Shot shot = point.Shot;
                TOF t = (TOF)shot.TOFs[1];
                double bg = t.GatedMeanAndUncertainty(gateBst, gateBend)[0];
                TOF bgSubtracted = t - bg;

                // if value if negative, set to zero
                for (int j = 0; j < bgSubtracted.Length; j++)
                {
                    if (bgSubtracted.Data[j] < 0) bgSubtracted.Data[j] = 0.0;
                }

                bgSubtracted.Calibration = t.Calibration;
                shot.TOFs.Add(bgSubtracted);
                //point.SinglePointData.Add("TopDetectorBackground", bg);       // ClassicEDM
                point.SinglePointData.Add("DetectorBBackground", bg);         // UEDM
            }
            // give these data a name
            //detectors.Add("topProbeNoBackground");        // ClassicEDM
            detectors.Add("detBNoBackground");          // UEDM
        }
        // this function takes some of the single point data and adds it to the block shots as TOFs
        // with one data point in them. This allows us to use the same code to break all of the data
        // into channels.
        public void TOFuliseSinglePointData(string[] channelsToTOFulise)
        {
            foreach (string spv in channelsToTOFulise)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    EDMPoint point = (EDMPoint)points[i];
                    Shot shot = point.Shot;
                    TOF spvTOF = new TOF((double)point.SinglePointData[spv]);
                    shot.TOFs.Add(spvTOF);
                }
                // give these data a name
                detectors.Add(spv);
            }
        }

        // TOF-ulise all the single point detectors
        public void TOFuliseSinglePointData()
        {
            EDMPoint point = (EDMPoint)points[0];
            string[] pointDetectors = new string[point.SinglePointData.Keys.Count];
            point.SinglePointData.Keys.CopyTo(pointDetectors, 0);

            foreach (string spv in pointDetectors)
            {
                if (spv != "dummy") // the hashtable has "dummy" as the default entry, but we don't want it
                {
                    for (int i = 0; i < points.Count; i++)
                    {
                        EDMPoint pt = (EDMPoint)points[i];
                        Shot shot = pt.Shot;
                        TOF spvTOF = new TOF((double)pt.SinglePointData[spv]);
                        shot.TOFs.Add(spvTOF);
                    }
                    // give these data a name
                    detectors.Add(spv);
                }
            }
        }
    }

}
