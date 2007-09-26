using System;
using System.Collections;
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

	}
}
