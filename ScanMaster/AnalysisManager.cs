using System;
using System.Collections;

using DAQ.Analyze;
using DAQ.Environment;

namespace ScanMaster.Analyze
{
	/// <summary>
	/// 
	/// </summary>
	public class AnalysisManager
	{
		private Hashtable analyses = new Hashtable();
		public Hashtable Analyses
		{
			get { return analyses; }
		}

		public AnalysisManager()
		{
			switch (Environs.ExperimentType)
			{
				case "decel":
					analyses.Add("Fit TOF", new FitTOF());
					break;
				case "edm":
				default:
					//analyses.Add("Fit TOF", new FitTOF());
					//analyses.Add("Fit interference", new InterferenceFit());
					break;
			}
		}

		public Analysis GetAnalysis(String name)
		{
			if (analyses.Contains(name)) return (Analysis)analyses[name];
			else return null;
		}
	}
}
