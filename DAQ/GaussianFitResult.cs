using System;

namespace DAQ.Analyze
{
	/// <summary>
	/// Represents the result of a Gaussian fit.
	/// </summary>
	public class GaussianFitResult : AnalysisResult
	{
		public GaussianFitResult()
		{
		}

		private FitParameters fitParams = new FitParameters();

		public FitParameters BestFitParameters
		{
			get { return fitParams; }
		}

		
		public class FitParameters
		{
			private double offset = 0.0;
			private double amplitude = 0.0;
			private double centroid = 0.0;
			private double width = 0.0;

			public double Offset
			{
				get { return offset; }
				set { offset = value; }
			}

			public double Amplitude
			{
				get { return amplitude; }
				set { amplitude = value; }
			}

			public double Centroid
			{
				get { return centroid; }
				set { centroid = value; }
			}

			public double Width
			{
				get { return width; }
				set { width = value; }
			}
		}

	}
}
