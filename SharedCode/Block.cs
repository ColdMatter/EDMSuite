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

        // convenience function to subtract background from TOFs, scale bottom TOF, 
        // and then calculate the asymmetry TOF
        public void ProcessBlock()
        {
            this.SubtractBackgroundFromProbeDetectorTOFs();
            this.CreateScaledBottomProbe();
            this.ConstructAsymmetryTOF();
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
                shot.TOFs.Add(asymmetry);
            }
            // give these data a name
            detectors.Add("asymmetry");
        }

        // this function scales up the bottom detector to match the top
        public void CreateScaledBottomProbe()
        {
            for (int i = 0; i < points.Count; i++)
            {
                Shot shot = ((EDMPoint)points[i]).Shot;
                int bottomIndex = detectors.IndexOf("bottomProbeNoBackground");
                int topIndex = detectors.IndexOf("topProbeNoBackground");
                TOF bottomScaled = TOF.ScaleTOFInTimeToMatchAnotherTOF((TOF)shot.TOFs[bottomIndex], (TOF)shot.TOFs[topIndex], kDetectorDistanceRatio);
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
                Shot shot = ((EDMPoint)points[i]).Shot;
                TOF t = (TOF)shot.TOFs[0];
                double bg = t.GatedMeanAndUncertainty(2800, 2900)[0];
                TOF bgSubtracted = t - bg;

                // if value if negative, set to zero
                for (int j = 0; j < bgSubtracted.Length; j++)
                {
                    if (bgSubtracted.Data[j] < 0) bgSubtracted.Data[j] = 0.0;
                }

                shot.TOFs.Add(bgSubtracted);
            }
            // give these data a name
            detectors.Add("bottomProbeNoBackground");

            for (int i = 0; i < points.Count; i++)
            {
                Shot shot = ((EDMPoint)points[i]).Shot;
                TOF t = (TOF)shot.TOFs[1];
                double bg = t.GatedMean(3200, 3300);
                TOF bgSubtracted = t - bg;

                // if value if negative, set to zero
                for (int j = 0; j < bgSubtracted.Length; j++)
                {
                    if (bgSubtracted.Data[j] < 0) bgSubtracted.Data[j] = 0.0;
                }

                shot.TOFs.Add(bgSubtracted);
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
