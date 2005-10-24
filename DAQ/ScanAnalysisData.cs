using System;

using Data.Scans;

namespace DAQ.Analyze
{
	/// <summary>
	/// 
	/// </summary>
	public class ScanAnalysisData : AnalysisData
	{
		public Scan Scan;
		
		public double TOFGateStart;
		public double TOFGateEnd;
		public double SpectrumGateStart;
		public double SpectrumGateEnd;
	}

}
