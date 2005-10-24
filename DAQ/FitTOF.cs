using System;
using Wolfram.NETLink;

using DAQ.Mathematica;
using Data;
using Data.Scans;


namespace DAQ.Analyze
{
	/// <summary>
	/// 
	/// </summary>
	public class FitTOF : Analysis
	{
		GaussianFitResult result;

		public FitTOF()
		{
			TitleText = "Time of flight fit";
		}

		// Fits a gaussian to the gated time-of-flight profile.
		public override void DoAnalysis(AnalysisData data)
		{
			initialiseKernel();

			ScanAnalysisData scanData = (ScanAnalysisData)data;

			if (kernel != null)
			{
				// get the gates from the StandardViewer window
				double lowgate = (scanData.SpectrumGateStart);
				double highgate = (scanData.SpectrumGateEnd);
				// extract the data to be fitted
				Scan averageScan = scanData.Scan;
				TOF tof = (TOF)averageScan.GetGatedAverageOnShot(lowgate, highgate).TOFs[0];
				object[] tofData = {tof.Times, tof.Data};

				// send data to mathematica for fitting
				MathematicaService.LoadPackage("FittingTools`",false);
				kernel.PutFunction("EvaluatePacket",1);
				kernel.PutFunction("gaussFit", 1);
				kernel.PutFunction("Transpose",1);
				kernel.Put(tofData);
				kernel.EndPacket();
				kernel.WaitAndDiscardAnswer();
				
				// get back the results and construct the GaussianFitResult object to be returned
				GaussianFitResult result = new GaussianFitResult();
				kernel.Evaluate("getBase[]");
				kernel.WaitForAnswer();
				result.BestFitParameters.Offset = kernel.GetDouble();
				kernel.Evaluate("getAmplitude[]");
				kernel.WaitForAnswer();
				result.BestFitParameters.Amplitude = kernel.GetDouble();
				kernel.Evaluate("getCentroid[]");
				kernel.WaitForAnswer();
				result.BestFitParameters.Centroid = kernel.GetDouble();
				kernel.Evaluate("getWidth[]");
				kernel.WaitForAnswer();
				result.BestFitParameters.Width = kernel.GetDouble();

				this.result = result;										
			}
			else // no mathematica service
			{
				Console.WriteLine("Unable to perform the fitting procedure because the Mathematica service is unavailable");
				return;
			}
		}

		public override AnalysisResult GetResult()
		{
			return result;
		}

		public override String GetText()
		{
			return "Offset = " + result.BestFitParameters.Offset
							+ System.Environment.NewLine +
						"Amplitude = " + result.BestFitParameters.Amplitude
							+ System.Environment.NewLine +
						"Centroid = " + result.BestFitParameters.Centroid
							+ System.Environment.NewLine +
						"Width = " + result.BestFitParameters.Width
							+ System.Environment.NewLine;
		}


	}
}
