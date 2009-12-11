using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

using EDMConfig;

namespace Data.EDM
{
	[Serializable]
	public class Block : MarshalByRefObject
	{
		public int Version = 2;
		private ArrayList points = new ArrayList();
		private DateTime timeStamp = DateTime.Now;
		private BlockConfig config = new BlockConfig();

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

	}
}
