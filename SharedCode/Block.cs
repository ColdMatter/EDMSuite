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
		public int Version = 3;
		private ArrayList points = new ArrayList();
		private DateTime timeStamp = DateTime.Now;
		private BlockConfig config = new BlockConfig();

        public List<string> detectors = new List<string>() {"top", "norm", "magnetometer", "gnd", "battery"};

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

        // NOTE: this function is rendered somewhat obsolete by the BlockTOFDemodulator.
        // This function takes a list of switches, defining an analysis channel, and gives the 
        // average TOF for that analysis channel's positively contributing TOFs and the same for
        // the negative contributors. Note that this definition may or may not line up with how 
        // the analysis channels are defined (they may differ by a sign, which might depend on
        // the number of switches in the channel).
        public TOF[] GetSwitchTOFs(string[] switches, int index)
        {
            TOF[] tofs = new TOF[2];
            // calculate the state of the channel for each point in the block
            int numSwitches = switches.Length;
            int waveformLength = config.GetModulationByName(switches[0]).Waveform.Length;
            List<bool[]> switchBits = new List<bool[]>();
            foreach (string s in switches)
                switchBits.Add(config.GetModulationByName(s).Waveform.Bits);
            List<bool> channelStates = new List<bool>();
            for (int point = 0; point < waveformLength; point++)
            {
                bool channelState = false;
                for (int i = 0; i < numSwitches; i++)
                {
                    channelState = channelState ^ switchBits[i][point];
                }
                channelStates.Add(channelState);
            }
            // build the "on" and "off" average TOFs
            TOF tOn = new TOF();
            TOF tOff = new TOF();
            for (int i = 0; i < waveformLength; i++)
            {
                if (channelStates[i]) tOn += ((TOF)((EDMPoint)Points[i]).Shot.TOFs[index]);
                else tOff += ((TOF)((EDMPoint)Points[i]).Shot.TOFs[index]);
            }
            tOn /= (waveformLength / 2);
            tOff /= (waveformLength / 2);
            tofs[0] = tOn;
            tofs[1] = tOff;
            return tofs;
        }


        // NOTE: this function is rendered somewhat obsolete by the BlockTOFDemodulator.
        // This function takes a list of switches, defining an analysis channel, and gives the 
        // average TOF for that analysis channel's positively contributing TOFs and the same for
        // the negative contributors. Note that this definition may or may not line up with how 
        // the analysis channels are defined (they may differ by a sign, which might depend on
        // the number of switches in the channel).
        public TOF[] GetSwitchTOFs(string[] switches, int index, bool normed)
        {
            TOF[] tofs = new TOF[2];
            // calculate the state of the channel for each point in the block
            int numSwitches = switches.Length;
            int waveformLength = config.GetModulationByName(switches[0]).Waveform.Length;
            List<bool[]> switchBits = new List<bool[]>();
            foreach (string s in switches)
                switchBits.Add(config.GetModulationByName(s).Waveform.Bits);
            List<bool> channelStates = new List<bool>();
            for (int point = 0; point < waveformLength; point++)
            {
                bool channelState = false;
                for (int i = 0; i < numSwitches; i++)
                {
                    channelState = channelState ^ switchBits[i][point];
                }
                channelStates.Add(channelState);
            }
            // build the "on" and "off" average TOFs
            TOF tOn = new TOF();
            TOF tOff = new TOF();
            if (!normed)
            {
                // if no normalisation is requested then the TOFs are added directly
                for (int i = 0; i < waveformLength; i++)
                {
                    if (channelStates[i]) tOn += ((TOF)((EDMPoint)Points[i]).Shot.TOFs[index]);
                    else tOff += ((TOF)((EDMPoint)Points[i]).Shot.TOFs[index]);
                }
            }
            else
            {
                // otherwise each point's TOF is normalised to the ratio of that points norm
                // signal (integrated over the cgate11) to the average norm signal for the block
                double[] normIntegrals = GetTOFIntegralArray(1, 0, 3000);
                double normMean = 0;
                for (int j = 0; j < normIntegrals.Length; j++) normMean += normIntegrals[j];
                normMean /= normIntegrals.Length;
                for (int j = 0; j < normIntegrals.Length; j++) normIntegrals[j] /= normMean;
                for (int i = 0; i < waveformLength; i++)
                {
                    if (channelStates[i]) tOn += ((TOF)((EDMPoint)Points[i]).Shot.TOFs[index]) / normIntegrals[i];
                    else tOff += ((TOF)((EDMPoint)Points[i]).Shot.TOFs[index]) / normIntegrals[i];
                }
            }
            tOn /= (waveformLength / 2);
            tOff /= (waveformLength / 2);
            tofs[0] = tOn;
            tofs[1] = tOff;
            return tofs;
        }

        // this function adds a new set of detector data to the block, constructed
        // by normalising the PMT data to the norm data. The normalisation is done
        // by dividing the PMT tofs through by the integrated norm data. The integration
        // is done according to the provided GatedDetectorExtractSpec.
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
    }
}