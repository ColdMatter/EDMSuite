using System;

using NationalInstruments.Analysis.Math;

using Data.Scans;
using ScanMaster.GUI;

namespace ScanMaster.GUI
{
	/// <summary>
	/// Summary description for RollViewer.
	/// </summary>
	public class RollViewer : Viewer
	{

		private static int UPDATE_EVERY = 2;
		private int shotCounter = 0;

		private RollViewerWindow window;
		bool visible = false;
		Scan pointsToPlot = new Scan();

		public String Name
		{
			get { return "Roll"; }
		}

		public RollViewer()
		{
			window = new RollViewerWindow(this);
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
			pointsToPlot.Points.Clear();
			shotCounter = 0;
			window.ClearAll();
		}

		public void AcquireStop()
		{
			window.ClearAll();
		}

		public void HandleDataPoint(ScanMaster.Acquire.DataEventArgs e)
		{
			pointsToPlot.Points.Add(e.point);
			shotCounter++;
			if (shotCounter % UPDATE_EVERY == 0)
			{
				int gateStart = (int)Controller.GetController().ProfileManager.CurrentProfile.
					AcquisitorConfig.shotGathererPlugin.Settings["gateStartTime"];
				int gateLength = (int)Controller.GetController().ProfileManager.CurrentProfile.
					AcquisitorConfig.shotGathererPlugin.Settings["gateLength"];
				double[] values = pointsToPlot.GetTOFOnIntegralArray(0, gateStart, gateStart + gateLength);
				double[] iodine = pointsToPlot.GetAnalogArray(0);

				window.AppendToSignalGraph(values);
				window.AppendToIodineGraph(iodine);

				pointsToPlot.Points.Clear();
			}
		}

		public void ScanFinished()
		{
		}

		public void NewScanLoaded()
		{
		}

	}
}
