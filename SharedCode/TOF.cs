using System;
using System.Xml.Serialization;

namespace Data
{
	/// <summary>
	/// A TOF is the result of a single detector looking at a single pulse from the source.
	/// </summary>
	[Serializable]
	public class TOF : MarshalByRefObject
	{
		private double[] tofData;
		private int length;
		private int gateStartTime;
		private int clockPeriod;
		public double calibration;

		public double Integrate() 
		{
			double sum = 0;
			foreach( double sample in tofData ) sum += sample;
			return (sum * clockPeriod);
		}

		public double Integrate(double startTime, double endTime) 
		{
			int low = (int)Math.Ceiling((startTime - gateStartTime) / clockPeriod);
			int high = (int)Math.Floor((endTime - gateStartTime) / clockPeriod);

			// check the range is sensible
			if (low < 0) low = 0;
			if (high > length - 1) high = length - 1;
			if (low > high) return 0;
			
			double sum = 0;
			for (int i = low ; i <= high ; i++) sum += tofData[i];
			return (sum * clockPeriod);
		}

		public static TOF
			operator +(TOF p1, TOF p2)
		{
			if (p1.ClockPeriod == p2.ClockPeriod && p1.GateStartTime == p2.GateStartTime
				&& p1.Length == p2.Length)
			{
				double[] tempData = new double[p1.Length];
				for (int i = 0 ; i < p1.Length ; i++)
				{
					tempData[i] = p1.Data[i] + p2.Data[i];
				}
				TOF temp = new TOF();
				temp.Data = tempData;
				temp.GateStartTime = p1.GateStartTime;
				temp.ClockPeriod = p1.ClockPeriod;
				return temp;
			} 
			else
			{
				if (p1.Length == 0) return p2;
				if (p2.Length == 0) return p1;
				return null;
			}
		}

		public static TOF operator /(TOF p, int n)
		{
			double[] tempData = new double[p.Length];
			for (int i = 0 ; i < p.Length ; i++ )
			{
				tempData[i] = p.Data[i] / n;
			}
			TOF temp = new TOF();
			temp.Data = tempData;
			temp.GateStartTime = p.GateStartTime;
			temp.ClockPeriod = p.ClockPeriod;
			return temp;
		}


		[XmlArrayItem("s")]
		public double[] Data 
		{
			get { return tofData; }
			set
			{
				tofData = value;
				length = value.Length;
			}
		}

		public int Length
		{
			get { return length; }
		}

		public int GateStartTime 
		{
			get { return gateStartTime; }
			set { gateStartTime = value; }
		}

		public int ClockPeriod
		{
			get { return clockPeriod; }
			set { clockPeriod = value; }
		}

		public double Calibration
		{
			get { return calibration; }
			set { calibration = value; }
		}

		public int[] Times
		{
			get
			{
				int[] times = new int[length];
				for (int i = 0 ; i < length ; i++) times[i] = gateStartTime + (i * clockPeriod);
				return times;
			}
		}


	}
}
