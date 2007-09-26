using System;
using System.Collections;
using System.Xml.Serialization;

namespace Data
{
	/// <summary>
	/// A shot is the set of measurements associated with a single pulse
	/// from the source. It can have any number of time of flight profiles
	/// (for instance, there might be more than one detector).
	/// </summary>
	[Serializable]
	public class Shot : MarshalByRefObject
	{
		private ArrayList tofs = new ArrayList();

		[XmlArray]
		[XmlArrayItem(Type = typeof(TOF))]
		public ArrayList TOFs
		{
			get { return tofs; }
		}

		public double Integrate( int index, double startTime, double endTime ) 
		{
			return ((TOF)tofs[index]).Integrate(startTime, endTime);
		}

        public double Mean(int index)
        {
            return ((TOF)tofs[index]).Mean;
        }

        public double GatedMean(int index, double startTime, double endTime)
        {
            return ((TOF)tofs[index]).GatedMean(startTime, endTime);
        }
        
        public static Shot operator +(Shot s1, Shot s2)
		{
			if (s1.TOFs.Count == s2.TOFs.Count) 
			{
				Shot temp = new Shot();
				for (int i = 0 ; i < s1.TOFs.Count ; i++) 
				{
					temp.TOFs.Add((TOF)s1.TOFs[i] + (TOF)s2.TOFs[i]);
				}
				return temp;
			} 
			else 
			{
				if (s1.TOFs.Count == 0) return s2;
				if (s2.TOFs.Count == 0) return s1;
				return null;
			}
		}

		public static Shot operator /(Shot s, int n) 
		{
			Shot temp = new Shot();
			foreach (TOF t in s.TOFs) temp.TOFs.Add(t/n);
			return temp;
		}

	}
}
