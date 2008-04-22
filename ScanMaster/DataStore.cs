using System;

using Data;
using Data.Scans;

namespace ScanMaster
{
	/// <summary>
	/// The controller has an instance of this class. It contains the current scan, and
	/// a running total scan which is used to derive the average.
	/// </summary>
	public class DataStore : MarshalByRefObject
	{
		private Scan currentScan = new Scan();
		public Scan CurrentScan
		{
			get { return currentScan; }
		}
		private Scan totalScan = new Scan();
		public Scan TotalScan 
		{
			get { return totalScan; }
		}
		public Scan AverageScan
		{
			get	{ return totalScan / scansInTotal; }
			set
			{
				totalScan = value;
				scansInTotal = 1;
			}
		}

		private int scansInTotal = 0;
		public int NumberOfScans
		{
			get { return scansInTotal; }
		}

		public void AddScanPoint(ScanPoint sp) 
		{
			currentScan.Points.Add(sp);
		}

		public void UpdateTotal() 
		{
			totalScan += currentScan.GetSortedScan();
			scansInTotal++;
		}

		public void ClearCurrentScan()
		{
			currentScan = new Scan();
		}

		public void ClearAll()
		{
			currentScan = new Scan();
			totalScan = new Scan();
			scansInTotal = 0;
		}

	}
}
