using System;

using NationalInstruments.Analysis.Math;

using Data;
using Data.Scans;
using ScanMaster.GUI;
using ScanMaster.Acquire.Plugin;

namespace ScanMaster.GUI
{
	/// <summary>
	/// Summary description for StatisticsViewer.
	/// </summary>
	public class StatisticsViewer : Viewer
	{
		// TODO: BIN is how often the stats viewer updates. It shouldn't really be
		// hard coded in like this.
		private static int BIN = 50;
		private int shotCounter = 0;
        private int gateStart;
        private int gateLength;

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
            PluginSettings shotSettings = Controller.GetController().ProfileManager.
                CurrentProfile.AcquisitorConfig.shotGathererPlugin.Settings;
            PluginSettings pgSettings = Controller.GetController().ProfileManager.CurrentProfile.AcquisitorConfig.pgPlugin.Settings;
            gateStart = (int)shotSettings["gateStartTime"];
            gateLength = (int)shotSettings["gateLength"];
           
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
				double[] values = pointsToAverage.GetTOFOnIntegralArray(0, gateStart, gateStart + gateLength);

				double mean = Statistics.Mean(values);
				double sd = Statistics.StandardDeviation(values);
				// TODO: this 7 is a cheesily coded magic number. If there's ever any demand it
				// could either be broken out in to the settings or the Environs.
				double noise = Math.Sqrt(7) * sd / Math.Sqrt(mean);

				window.AppendToSignalGraph(mean);
				window.AppendToSignalNoiseGraph(noise);

				window.SetMeanText(mean);
				window.SetSDText(sd);
				window.SetNoiseText(noise);

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
