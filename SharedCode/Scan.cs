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

		public double[] ScanParameterArray
		{
			get 
			{
				double[] temp = new double[points.Count];
				for (int i = 0 ; i < points.Count ; i++) temp[i] = ((ScanPoint)points[i]).ScanParameter;
				return temp;
			}
		}
        
         /* Returns the index in the array of ScanPoints where the ScanPoint with the given scanParameter 
          * can be found */ 
        public int GetIndex(double scanParameter)
        {
            double[] spa = ScanParameterArray;
            for (int i = 0; i < spa.Length; i++)
            {
                if (spa[i] == scanParameter) return i;
            }
            return -1;
        }

        /* Finds the ScanPoint in the array of ScanPoints whose ScanParameter most nearly matches the argument.
         * Returns the index in the array of this ScanPoint */
        public int GetNearestIndex(double scanParameter)
        {
            double[] spa = ScanParameterArray;
            int bestIndex = 0;
            double temp;
            double diff = (double)Math.Abs(spa[0] - scanParameter);
            for (int i = 0; i < spa.Length; i++)
            {
                temp = (double)Math.Abs(spa[i] - scanParameter);
                if (temp < diff) 
                {
                    diff = temp;
                    bestIndex = i;
                }
            }
            return bestIndex;
        }

        /* Returns the ScanPoint that has the given scanParameter, or null if there isn't one */
        public ScanPoint GetScanPointAt(double scanParameter)
        {
            int ind = GetIndex(scanParameter);
            if (ind != -1) return (ScanPoint)points[ind];
            else return null;
        }

        /* Returns the ScanPoint in the array of ScanPoints whose ScanParameter most nearly matches the argument */
        public ScanPoint GetNearestScanPoint(double scanParameter)
        {
            return (ScanPoint)points[GetNearestIndex(scanParameter)];
        }

        public double MinimumScanParameter
        {
            get
            {
                double[] spa = ScanParameterArray;
                double temp = spa[0];

                foreach (double d in spa)
                {
                    if (d < temp) temp = d;
                }
                return temp;
            }
        }

        public double MaximumScanParameter
        {
            get
            {
                double[] spa = ScanParameterArray;
                double temp = spa[0];

                foreach (double d in spa)
                {
                    if (d > temp) temp = d;
                }
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
            double scanResolution = (MaximumScanParameter - MinimumScanParameter) / (points.Count - 1);
            ScanPoint temp = new ScanPoint();
            double paramVal = lowGate;
            int numberOfPoints = 0;
            if (highGate < lowGate) highGate = lowGate + scanResolution;
            while (paramVal < highGate)
            {
                temp += GetNearestScanPoint(paramVal);
                paramVal += scanResolution;
                numberOfPoints++;
            }
            return temp / numberOfPoints;
		}

		public static Scan operator +(Scan s1, Scan s2)
		{
			if (s1.Points.Count == s2.Points.Count)
			{
				Scan temp = new Scan();
                ScanPoint sp1;
                ScanPoint sp2;
                for (int i = 0; i < s1.Points.Count; i++)
                {
                    sp1 = (ScanPoint)s1.Points[i];
                    sp2 = s2.GetNearestScanPoint(sp1.ScanParameter);
                    temp.Points.Add(sp1 + sp2);
                }
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
