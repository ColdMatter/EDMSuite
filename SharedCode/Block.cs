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

        //// Old functions
        //// NOTE: this function is rendered somewhat obsolete by the BlockTOFDemodulator.
        //// This function takes a list of switches, defining an analysis channel, and gives the
        //// average TOF for that analysis channel's positively contributing TOFs and the same for
        //// the negative contributors. Note that this definition may or may not line up with how
        //// the analysis channels are defined (they may differ by a sign, which might depend on
        //// the number of switches in the channel).
        //public TOF[] GetSwitchTOFs(string[] switches, int index)
        //{
        //    TOF[] tofs = new TOF[2];
        //    // calculate the state of the channel for each point in the block
        //    int numSwitches = switches.Length;
        //    int waveformLength = config.GetModulationByName(switches[0]).Waveform.Length;
        //    List<bool[]> switchBits = new List<bool[]>();
        //    foreach (string s in switches)
        //        switchBits.Add(config.GetModulationByName(s).Waveform.Bits);
        //    List<bool> channelStates = new List<bool>();
        //    for (int point = 0; point < waveformLength; point++)
        //    {
        //        bool channelState = false;
        //        for (int i = 0; i < numSwitches; i++)
        //        {
        //            channelState = channelState ^ switchBits[i][point];
        //        }
        //        channelStates.Add(channelState);
        //    }
        //    // build the "on" and "off" average TOFs
        //    TOF tOn = new TOF();
        //    TOF tOff = new TOF();
        //    for (int i = 0; i < waveformLength; i++)
        //    {
        //        if (channelStates[i]) tOn += ((TOF)((EDMPoint)Points[i]).Shot.TOFs[index]);
        //        else tOff += ((TOF)((EDMPoint)Points[i]).Shot.TOFs[index]);
        //    }
        //    tOn /= (waveformLength / 2);
        //    tOff /= (waveformLength / 2);
        //    tofs[0] = tOn;
        //    tofs[1] = tOff;
        //    return tofs;
        //}


        //// NOTE: this function is rendered somewhat obsolete by the BlockTOFDemodulator.
        //// This function takes a list of switches, defining an analysis channel, and gives the
        //// average TOF for that analysis channel's positively contributing TOFs and the same for
        //// the negative contributors. Note that this definition may or may not line up with how
        //// the analysis channels are defined (they may differ by a sign, which might depend on
        //// the number of switches in the channel).
        //public TOF[] GetSwitchTOFs(string[] switches, int index, bool normed)
        //{
        //    TOF[] tofs = new TOF[2];
        //    // calculate the state of the channel for each point in the block
        //    int numSwitches = switches.Length;
        //    int waveformLength = config.GetModulationByName(switches[0]).Waveform.Length;
        //    List<bool[]> switchBits = new List<bool[]>();
        //    foreach (string s in switches)
        //        switchBits.Add(config.GetModulationByName(s).Waveform.Bits);
        //    List<bool> channelStates = new List<bool>();
        //    for (int point = 0; point < waveformLength; point++)
        //    {
        //        bool channelState = false;
        //        for (int i = 0; i < numSwitches; i++)
        //        {
        //            channelState = channelState ^ switchBits[i][point];
        //        }
        //        channelStates.Add(channelState);
        //    }
        //    // build the "on" and "off" average TOFs
        //    TOF tOn = new TOF();
        //    TOF tOff = new TOF();
        //    if (!normed)
        //    {
        //        // if no normalisation is requested then the TOFs are added directly
        //        for (int i = 0; i < waveformLength; i++)
        //        {
        //            if (channelStates[i]) tOn += ((TOF)((EDMPoint)Points[i]).Shot.TOFs[index]);
        //            else tOff += ((TOF)((EDMPoint)Points[i]).Shot.TOFs[index]);
        //        }
        //    }
        //    else
        //    {
        //        // otherwise each point's TOF is normalised to the ratio of that points norm
        //        // signal (integrated over the cgate11) to the average norm signal for the block
        //        double[] normIntegrals = GetTOFIntegralArray(1, 0, 3000);
        //        double normMean = 0;
        //        for (int j = 0; j < normIntegrals.Length; j++) normMean += normIntegrals[j];
        //        normMean /= normIntegrals.Length;
        //        for (int j = 0; j < normIntegrals.Length; j++) normIntegrals[j] /= normMean;
        //        for (int i = 0; i < waveformLength; i++)
        //        {
        //            if (channelStates[i]) tOn += ((TOF)((EDMPoint)Points[i]).Shot.TOFs[index]) / normIntegrals[i];
        //            else tOff += ((TOF)((EDMPoint)Points[i]).Shot.TOFs[index]) / normIntegrals[i];
        //        }
        //    }
        //    tOn /= (waveformLength / 2);
        //    tOff /= (waveformLength / 2);
        //    tofs[0] = tOn;
        //    tofs[1] = tOff;
        //    return tofs;
        //}

        // Old function - we don't normalise by a "norm" detector anymore - CH 12/09
        // this function adds a new set of detector data to the block, constructed
        // by normalising the PMT data to the norm data. The normalisation is done
        // by dividing the PMT tofs through by the integrated norm data. The integration
        // is done according to the provided GatedDetectorExtractSpec.

        // This is kept for backwards-compatibility with old code, but we don't normalise data with a "norm" detector anymore
        public void Normalise(GatedDetectorExtractSpec normGate)
        {
            GatedDetectorData normData = GatedDetectorData.ExtractFromBlock(this, normGate);
            double averageNorm = 0;
            foreach (double val in normData.PointValues) averageNorm += val;
            averageNorm /= normData.PointValues.Count;

            for (int i = 0; i < points.Count; i++)
            {
                Shot shot = ((EDMPoint)points[i]).Shot;
                TOF normedTOF = ((TOF)shot.TOFs[0]) / (normData.PointValues[i] * (1 / averageNorm));
                shot.TOFs.Add(normedTOF);
            }
            // give these data a name
            detectors.Add("topNormed");
        }

        // this function adds a new set of detector data to the block, constructed
        // by calculating the asymmetry of the top and bottom detectors, after scaling
        // the bottom detector in time so that it matches the top detector

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
