using System;
using System.Collections;
using System.Xml.Serialization;

namespace Data.Scans
{
	/// <summary>
	/// A scan point is a single point in a scan. It might have
	/// more than one Shot (perhaps because several shots are being
	/// taken per shot to improve S:N. The shots are stored in two
	/// groups to facilitate switched scans. A scan point also has some
	/// analog channel readings associated with it. The scan point keeps
	/// a record of the scan parameter.
	/// </summary>
	[Serializable]
	public class ScanPoint : MarshalByRefObject
	{
		private double scanParameter;
		private ArrayList onShots = new ArrayList();
		private ArrayList offShots = new ArrayList();
		private ArrayList analogs = new ArrayList();

		public Shot AverageOnShot 
		{
			get { return AverageShots(onShots); }
		}

		public Shot AverageOffShot
		{
			get { return AverageShots(offShots); }
		}

		private Shot AverageShots(ArrayList shots) 
		{
			if (shots.Count == 1) return (Shot)shots[0];
			Shot temp = new Shot();
			foreach (Shot s in shots) temp += s;
			return temp/shots.Count;
		}

		public double IntegrateOn(int index, double startTime, double endTime)
		{
			return Integrate(onShots, index, startTime, endTime);
		}

		public double IntegrateOff(int index, double startTime, double endTime)
		{
			return Integrate(offShots, index, startTime, endTime);
		}

		private double Integrate(ArrayList shots, int index, double startTime, double endTime)
		{
			double temp = 0;
			foreach (Shot s in shots) temp += s.Integrate(index, startTime, endTime);
			return temp/shots.Count;
		}

        public double MeanOn(int index)
        {
            return Mean(onShots, index);
        }

        public double MeanOff(int index)
        {
            return Mean(offShots, index);
        }

        private double Mean(ArrayList shots, int index)
        {
            double temp = 0;
            foreach (Shot s in shots) temp += s.Mean(index);
            return temp / shots.Count;
        }


		public static ScanPoint operator +(ScanPoint p1, ScanPoint p2)
		{
			if (p1.OnShots.Count == p2.OnShots.Count
				&& p1.OffShots.Count == p2.OffShots.Count
				&& p1.Analogs.Count == p2.Analogs.Count)
			{
				ScanPoint temp = new ScanPoint();
				temp.ScanParameter = p1.ScanParameter;
				for (int i = 0 ; i < p1.OnShots.Count ; i++)
					temp.OnShots.Add((Shot)p1.OnShots[i] + (Shot)p2.OnShots[i]);
				for (int i = 0 ; i < p1.OffShots.Count ; i++)
					temp.OffShots.Add((Shot)p1.OffShots[i] + (Shot)p2.OffShots[i]);
				for (int i = 0 ; i < p1.Analogs.Count ; i++)
					temp.Analogs.Add((double)p1.Analogs[i] + (double)p2.Analogs[i]);
				return temp;
			}
			else
			{
				if (p1.OnShots.Count == 0) return p2;
				if (p2.OnShots.Count == 0) return p1;
				return null;
			}
		}

		public static ScanPoint operator /(ScanPoint p, int n)
		{
			ScanPoint temp = new ScanPoint();
			temp.ScanParameter = p.ScanParameter;
			foreach (Shot s in p.OnShots) temp.OnShots.Add(s/n);
			foreach (Shot s in p.OffShots) temp.OffShots.Add(s/n);
			foreach (double a in p.Analogs) temp.Analogs.Add(a/n);
			return temp;
		}

		public double ScanParameter
		{
			get { return scanParameter; }
			set { scanParameter = value; }
		}

		[XmlArray]
		[XmlArrayItem(Type = typeof(Shot))]
		public ArrayList OnShots
		{
			get { return onShots; }
		}
		[XmlArray]
		[XmlArrayItem(Type = typeof(Shot))]
		public ArrayList OffShots
		{
			get { return offShots; }
		}
		
		[XmlArray]
		[XmlArrayItem(Type = typeof(double))]
		public ArrayList Analogs
		{
			get { return analogs; }
		}

	}
}
