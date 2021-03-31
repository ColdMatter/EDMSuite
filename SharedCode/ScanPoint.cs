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
        public double gpibval;

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

        public double VarianceOn(int index, double startTime, double endTime)
        {
            return Variance(onShots, index, startTime, endTime);
        }

        public double FractionOfShotNoiseOn(int index, double startTime, double endTime)
        {
            return FractionOfShotNoise(onShots, index, startTime, endTime);
        }

        public double FractionOfShotNoiseNormedOn(int[] index, double startTime0, double endTime0, double startTime1, double endTime1)
        {
            return FractionOfShotNoiseNormed(onShots, index, startTime0, endTime0,startTime1,endTime1);
        }

		private double Integrate(ArrayList shots, int index, double startTime, double endTime)
		{
			double temp = 0;
			foreach (Shot s in shots) temp += s.Integrate(index, startTime, endTime);
			return temp/shots.Count;
		}

        private double IntegrateNormed(ArrayList shots, int[] index, double startTime0, double endTime0, double startTime1, double endTime1)
        {
            double temp = 0;
            foreach (Shot s in shots) temp += s.Integrate(index[0], startTime0, endTime0) / s.Integrate(index[1], startTime1, endTime1);
            return temp / shots.Count;
        }

        private double Variance(ArrayList shots, int index, double startTime, double endTime)
        {
            double tempMn = 0;
            double tempx2 = 0;
            double vari = 0;
            double n = shots.Count; 

            if (n==1)
            {
                return vari;
            }
            else
            {
            foreach (Shot s in shots) tempMn += s.Integrate(index, startTime, endTime);
            foreach (Shot s in shots) tempx2 += Math.Pow(s.Integrate(index, startTime, endTime),2);
            return vari = (n / (n - 1)) * ((tempx2 / n) - Math.Pow((tempMn / n), 2));
            }
        }

        private double VarianceNormed(ArrayList shots, int[] index, double startTime0, double endTime0, double startTime1, double endTime1)
        {
            double tempMn = 0;
            double tempx2 = 0;
            double vari = 0;
            double n = shots.Count;

            if (n == 1)
            {
                return vari;
            }
            else
            {
                foreach (Shot s in shots) tempMn += s.Integrate(index[0], startTime0, endTime0) / s.Integrate(index[1], startTime1, endTime1);
                foreach (Shot s in shots) tempx2 += Math.Pow(s.Integrate(index[0], startTime0, endTime0) / s.Integrate(index[1], startTime1, endTime1), 2);
                return vari = (n / (n - 1)) * ((tempx2 / n) - Math.Pow((tempMn / n), 2));
            }
        }

        private double Calibration(ArrayList shots, int index)
        {
            Shot s = (Shot)shots[0];
            return s.Calibration(index);
        }

        private double FractionOfShotNoise(ArrayList shots, int index, double startTime, double endTime)
        {
            return Math.Pow(Variance(shots, index, startTime, endTime)/(Integrate(shots, index, startTime, endTime)*Calibration(shots, index)),0.5);
        }

        private double FractionOfShotNoiseNormed(ArrayList shots, int[] index, double startTime0, double endTime0, double startTime1, double endTime1)
        {
            double fracNoiseTN = Math.Pow(VarianceNormed(shots, index, startTime0, endTime0, startTime1, endTime1),0.5) / IntegrateNormed(shots, index, startTime0, endTime0, startTime1, endTime1);
            double fracNoiseT2 = Calibration(shots, index[0]) / Integrate(shots, index[0], startTime0, endTime0);
            double fracNoiseN2 = Calibration(shots, index[1]) / Integrate(shots, index[1], startTime1, endTime1);
            return fracNoiseTN / Math.Pow(fracNoiseT2 + fracNoiseN2, 0.5);
 
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
        public double GPIBval
        {
            get { return gpibval; }
        }
        

	}
}
