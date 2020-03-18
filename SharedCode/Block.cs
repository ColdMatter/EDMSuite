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
        private static double kDetectorDistanceRatio = 1715.0 / 1500.0;

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
            SubtractBackgroundFromProbeDetectorTOFs();

            CreateScaledBottomProbe();

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
                int bottomScaledIndex = detectors.IndexOf("bottomProbeScaled");
                int topIndex = detectors.IndexOf("topProbeNoBackground");
                TOF asymmetry = ((TOF)shot.TOFs[bottomScaledIndex] - (TOF)shot.TOFs[topIndex]) / ((TOF)shot.TOFs[bottomScaledIndex] + (TOF)shot.TOFs[topIndex]);
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

                TOF bottomScaled = (TOF)shot.TOFs[detectors.IndexOf("bottomProbeScaled")];
                TOF top = (TOF)shot.TOFs[detectors.IndexOf("topProbeNoBackground")];

                // Multiply TOFs by their calibrations
                bottomScaled *= bottomScaled.Calibration;
                top *= top.Calibration;

                // Need the total signal TOF for later calculations
                TOF total = bottomScaled + top;

                // Get background counts
                double topLaserBackground = (double)point.SinglePointData["TopDetectorBackground"] * bottomScaled.Calibration;
                double bottomLaserBackground = (double)point.SinglePointData["BottomDetectorBackground"] * top.Calibration;

                // Calculate the shot noise variance in the asymmetry detector
                TOF asymmetryVariance =
                    bottomScaled * bottomScaled * top * 4.0
                    + bottomScaled * top * top * 4.0
                    + top * top * bottomLaserBackground * 8.0
                    + bottomScaled * bottomScaled * topLaserBackground * 8.0;
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
                TOF bottomScaled = TOF.ScaleTOFInTimeToMatchAnotherTOF(bottomTOF, topTOF, kDetectorDistanceRatio);
                bottomScaled.Calibration = bottomTOF.Calibration;
                shot.TOFs.Add(bottomScaled);
            }
            // give these data a name
            detectors.Add("bottomProbeScaled");
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
