using System;

using NationalInstruments.Analysis.Math;

using Data.Scans;
using ScanMaster.GUI;

namespace ScanMaster.GUI
{
	/// <summary>
	/// Summary description for StatisticsViewer.
	/// </summary>
	public class StatisticsViewer : Viewer
	{

		private static int BIN = 10;
		private int shotCounter = 0;

		private StatisticsViewerWindow window;
		bool visible = false;
		Scan pointsToAverage = new Scan();

		public String Name
		{
			get { return "Statistics"; }
		}

		public StatisticsViewer()
		{
			window = new StatisticsViewerWindow(this);
		}

		public void Show()
		{
			window.Show();
			visible = true;
		}

		public void Hide()
		{
			window.Hide();
			visible = false;
		}

		public void ToggleVisible()
		{
			if (visible) Hide();
			else Show();
		}

		public void AcquireStart()
		{
			pointsToAverage.Points.Clear();
			shotCounter = 0;
			window.ClearAll();
		}

		public void AcquireStop()
		{
			window.ClearAll();
		}

		public void HandleDataPoint(ScanMaster.Acquire.DataEventArgs e)
		{
			pointsToAverage.Points.Add(e.point);
			shotCounter++;
			if (shotCounter % BIN == 0)
			{
				int gateStart = (int)Controller.GetController().ProfileManager.CurrentProfile.
					AcquisitorConfig.shotGathererPlugin.Settings["gateStartTime"];
				int gateLength = (int)Controller.GetController().ProfileManager.CurrentProfile.
					AcquisitorConfig.shotGathererPlugin.Settings["gateLength"];
				double[] values = pointsToAverage.GetTOFOnIntegralArray(0, gateStart, gateStart + gateLength);

				double mean = Statistics.Mean(values);
				double sd = Statistics.StandardDeviation(values);

				window.AppendToSignalGraph(mean);
				window.AppendToSignalNoiseGraph(mean/sd);

				window.SetMeanText(mean);
				window.SetSDText(sd);

				pointsToAverage.Points.Clear();
			}
		}

		public void ScanFinished()
		{
//			pointsToAverage.Points.Clear();
//			shotCounter = 0;
//			window.ClearAll();
		}

		public void NewScanLoaded()
		{
		}

	}
}
