using System;
using System.Collections;
using System.Xml.Serialization;

namespace Data.Scans
{
	/// <summary>
	/// A scan is a set of scan points.
	/// </summary>
	[Serializable]
	public class Scan : MarshalByRefObject
	{
		private ArrayList points = new ArrayList();

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

		public double[] GetTOFOnIntegralArray(int index, double startTime, double endTime)
		{
			double[] temp = new double[points.Count];
			for (int i = 0 ; i < points.Count ; i++) temp[i] = 
														 (double)((ScanPoint)points[i]).IntegrateOn(index, startTime, endTime);
			return temp;
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
			double scanParameterStart = ((ScanPoint)points[0]).ScanParameter;
			double scanParameterEnd = ((ScanPoint)points[points.Count -1]).ScanParameter;
			int low = (int)Math.Ceiling(points.Count * (lowGate - scanParameterStart) /
				(scanParameterEnd - scanParameterStart));
			int high = (int)Math.Floor(points.Count * (highGate - scanParameterStart) /
				(scanParameterEnd - scanParameterStart));
			if (low < 0) low = 0;
			if (high > points.Count) high = points.Count;

			ScanPoint temp = new ScanPoint();
			for (int i = low ; i < high ; i++) temp += (ScanPoint)points[i];

			return temp /(high-low);
		}

		public static Scan operator +(Scan s1, Scan s2)
		{
			if (s1.Points.Count == s2.Points.Count)
			{
				Scan temp = new Scan();
				for (int i = 0 ; i < s1.Points.Count ; i++)
					temp.Points.Add((ScanPoint)s1.Points[i] + (ScanPoint)s2.Points[i]);
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
			return temp;
		}


		[XmlArray]
		[XmlArrayItem(Type = typeof(ScanPoint))]
		public ArrayList Points
		{
			get { return points; }
		}

	}
}
