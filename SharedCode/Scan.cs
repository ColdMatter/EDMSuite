using System;
using System.Collections;
using System.Xml.Serialization;
using Utility;

namespace Data.Scans
{
	/// <summary>
	/// A scan is a set of scan points. Also holds a list of the settings used during its acquisition.
	/// </summary>
	[Serializable]
	public class Scan : MarshalByRefObject
	{
		private ArrayList points = new ArrayList();

        // this is a hashtable of the acquisitor settings used for this scan
        public XmlSerializableHashtable ScanSettings = new XmlSerializableHashtable();

        public Scan GetSortedScan()
        {
            double[] spa = ScanParameterArray;
            ScanPoint[] pts = (ScanPoint[])points.ToArray(typeof(ScanPoint));
            Array.Sort(spa, pts);
            Scan ss = new Scan();
            ss.points.AddRange(pts);
            ss.ScanSettings = ScanSettings;
            return ss;
        }

        public double MinimumScanParameter
        {
            get
            {
                return GetSortedScan().ScanParameterArray[0];
            }
        }
        public double MaximumScanParameter
        {
            get
            {
                double[] spa = GetSortedScan().ScanParameterArray;
                return spa[spa.Length - 1];
            }
        }


		public double[] ScanParameterArray
		{
			get 
			{
				double[] temp = new double[points.Count];
				for (int i = 0 ; i < points.Count ; i++) temp[i] = ((ScanPoint)points[i]).ScanParameter;
				return temp;
			}
		}

		public double[] GetAnalogArray(int index)
		{
			double[] temp = new double[points.Count];
			for (int i = 0 ; i < points.Count ; i++) temp[i] = (double)((ScanPoint)points[i]).Analogs[index];
			return temp;
		}

        public double[] GetGPIBArray
        {
            get
            {
                double[] temp = new double[points.Count];
                for (int i = 0; i < points.Count; i++) temp[i] = ((ScanPoint)points[i]).ScanParameter;
                return temp;
            }
        }
        public int AnalogChannelCount
        {
            get
            {
                return ((ScanPoint)points[0]).Analogs.Count;
            }
        }

		public double[] GetTOFOnIntegralArray(int index, double startTime, double endTime)
		{
			double[] temp = new double[points.Count];
			for (int i = 0 ; i < points.Count ; i++) temp[i] = 
														 (double)((ScanPoint)points[i]).IntegrateOn(index, startTime, endTime);
			return temp;
		}

        public double[] GetTOFOnOverShotNoiseArray(int index, double startTime, double endTime)
        {
            double[] tempShot = new double[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                tempShot[i] = (double)((ScanPoint)points[i]).FractionOfShotNoiseOn(index, startTime, endTime);
            }
                return tempShot;
        }

        public double[] GetTOFOnOverShotNoiseNormedArray(int[] index, double startTime0, double endTime0,double startTime1,double endTime1)
        {
            double[] tempShot = new double[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                tempShot[i] = (double)((ScanPoint)points[i]).FractionOfShotNoiseNormedOn(index, startTime0, endTime0,startTime1,endTime1);
            }
            return tempShot;
        }


		public double[] GetTOFOffIntegralArray(int index, double startTime, double endTime)
		{
			double[] temp = new double[points.Count];
			for (int i = 0 ; i < points.Count ; i++) temp[i] = 
														 (double)((ScanPoint)points[i]).IntegrateOff(index, startTime, endTime);
			return temp;
		}

		public double[] GetDifferenceIntegralArray(int index, double startTime, double endTime)
		{
			double[] temp = new double[points.Count];
			double[] on = GetTOFOnIntegralArray(index, startTime, endTime);
			double[] off = GetTOFOffIntegralArray(index, startTime, endTime);
			for (int i = 0 ; i < points.Count ; i++) temp[i] = on[i] - off[i];
			return temp;
		}

		public double[] GetBgSubtractedDifferenceIntegralArray(int index, double signalStartTime, double signalEndTime, double bgStartTime, double bgEndTime)
		{
			double[] temp = new double[points.Count];
			double[] on = GetBackgroundSubstractedTOFOnIntegralArray(index, signalStartTime, signalEndTime, bgStartTime, bgEndTime);
			double[] off = GetBackgroundSubstractedTOFOffIntegralArray(index, signalStartTime, signalEndTime, bgStartTime, bgEndTime);
			for (int i = 0; i < points.Count; i++) temp[i] = on[i] - off[i];
			return temp;
		}

		public double[] GetBgSubtractedRatioIntegralArray(int index, double signalStartTime, double signalEndTime, double bgStartTime, double bgEndTime)
		{
			double[] temp = new double[points.Count];
			double[] on = GetBackgroundSubstractedTOFOnIntegralArray(index, signalStartTime, signalEndTime, bgStartTime, bgEndTime);
			double[] off = GetBackgroundSubstractedTOFOffIntegralArray(index, signalStartTime, signalEndTime, bgStartTime, bgEndTime);
			for (int i = 0; i < points.Count; i++) temp[i] = on[i] / off[i];
			return temp;
		}

		//define a new function to do background substraction
		public double[] GetBackgroundSubstractedTOFOnIntegralArray(int index, double signalStartTime, double signalEndTime, double bgStartTime, double bgEndTime)
		{
			double[] temp = new double[points.Count];
			double[] signalArray = GetTOFOnIntegralArray(index, signalStartTime, signalEndTime);
			double[] backgroundArray = GetTOFOnIntegralArray(index, bgStartTime, bgEndTime);
			for (int i = 0; i < points.Count; i++) temp[i] = signalArray[i] - backgroundArray[i]*(signalEndTime- signalStartTime)/(bgEndTime- bgStartTime);
			//for each background term, we multiply a complicated correction factor in case bgEndTime- bgStartTime is not the same as the signalEndTime- signalStartTime
			return temp;
		}

		public double[] GetBackgroundSubstractedTOFOffIntegralArray(int index, double signalStartTime, double signalEndTime, double bgStartTime, double bgEndTime)
		{
			double[] temp = new double[points.Count];
			double[] signalArray = GetTOFOffIntegralArray(index, signalStartTime, signalEndTime);
			double[] backgroundArray = GetTOFOffIntegralArray(index, bgStartTime, bgEndTime);
			for (int i = 0; i < points.Count; i++) temp[i] = signalArray[i] - backgroundArray[i] * (signalEndTime - signalStartTime) / (bgEndTime - bgStartTime);
			//for each background term, we multiply a complicated correction factor in case bgEndTime- bgStartTime is not the same as the signalEndTime- signalStartTime
			return temp;
		}
		//define a new function to do yg on-yg off

		public double[] TOFOnIntegralArrayYgOnMinusOff(double signalStartTime, double signalEndTime)
		{
			double[] temp = new double[points.Count];
			double[] signalArrayYgOn = GetTOFOnIntegralArray(0, signalStartTime, signalEndTime);
			double[] signalArrayYgOff = GetTOFOnIntegralArray(1, signalStartTime, signalEndTime);
			for (int i = 0; i < points.Count; i++) temp[i] = signalArrayYgOn[i] - signalArrayYgOff[i];
			//for each background term, we multiply a complicated correction factor in case bgEndTime- bgStartTime is not the same as the signalEndTime- signalStartTime
			return temp;
		}
		public double[] TOFOffIntegralArrayYgOnMinusOff(double signalStartTime, double signalEndTime)
		{
			double[] temp = new double[points.Count];
			double[] signalArrayYgOn = GetTOFOffIntegralArray(0, signalStartTime, signalEndTime);
			double[] signalArrayYgOff = GetTOFOffIntegralArray(1, signalStartTime, signalEndTime);
			for (int i = 0; i < points.Count; i++) temp[i] = signalArrayYgOn[i] - signalArrayYgOff[i];
			//for each background term, we multiply a complicated correction factor in case bgEndTime- bgStartTime is not the same as the signalEndTime- signalStartTime
			return temp;
		}

		public double[] GetMeanOnArray(int index)
        {
            double[] temp = new double[points.Count];
            for (int i = 0; i < points.Count; i++) temp[i] =
                                                       (double)((ScanPoint)points[i]).MeanOn(index);
            return temp;
        }

        public double[] GetMeanOffArray(int index)
        {
            double[] temp = new double[points.Count];
            for (int i = 0; i < points.Count; i++) temp[i] =
                                                       (double)((ScanPoint)points[i]).MeanOff(index);
            return temp;
        }

        public Shot GetGatedAverageOnShot(double lowGate, double highGate)
		{
			return GetAverageScanPoint(lowGate, highGate).AverageOnShot;
		}
		public Shot GetGatedAverageOffShot(double lowGate, double highGate)
		{
			return GetAverageScanPoint(lowGate, highGate).AverageOffShot;
		}

		private ScanPoint GetAverageScanPoint(double lowGate, double highGate)
		{
            Scan ss = GetSortedScan();
			double scanParameterStart = ((ScanPoint)ss.points[0]).ScanParameter;
			double scanParameterEnd = ((ScanPoint)ss.points[points.Count -1]).ScanParameter;
			int low = (int)Math.Ceiling(ss.points.Count * (lowGate - scanParameterStart) /
				(scanParameterEnd - scanParameterStart));
			int high = (int)Math.Floor(ss.points.Count * (highGate - scanParameterStart) /
				(scanParameterEnd - scanParameterStart));
			if (low < 0) low = 0;
			if (low >= ss.points.Count) low = ss.points.Count - 2;
			if (high < low) high = low + 1;
			if (high >= ss.points.Count) high = ss.points.Count -1;

			ScanPoint temp = new ScanPoint();
			for (int i = low ; i < high ; i++) temp += (ScanPoint)ss.points[i];

			return temp /(high-low);
		}

        // Note: this only really makes sense for sorted scans!
		public static Scan operator +(Scan s1, Scan s2)
		{
			if (s1.Points.Count == s2.Points.Count)
			{
				Scan temp = new Scan();
				for (int i = 0 ; i < s1.Points.Count ; i++)
					temp.Points.Add((ScanPoint)s1.Points[i] + (ScanPoint)s2.Points[i]);
                temp.ScanSettings = s1.ScanSettings;
				return temp;
			}
			else
			{
				if (s1.Points.Count == 0) return s2;
				if (s2.Points.Count == 0) return s1;
				return null;
			}
		}

		public static Scan operator /(Scan s, int n)
		{
			Scan temp = new Scan();
			foreach (ScanPoint sp in s.Points) temp.Points.Add(sp/n);
            temp.ScanSettings = s.ScanSettings;
			return temp;
		}

        public object GetSetting(string pluginType, string parameter)
        {
            return ScanSettings[pluginType + ":" + parameter];
        }

		[XmlArray]
		[XmlArrayItem(Type = typeof(ScanPoint))]
		public ArrayList Points
		{
			get { return points; }
		}

	}
}
