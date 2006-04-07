using System;
using System.Collections;

using Data;
using Data.Scans;
using ScanMaster;
using ScanMaster.Acquire;
using ScanMaster.Acquire.Plugin;

namespace ScanMaster.GUI
{
	/// <summary>
	/// 
	/// </summary>
	public class StandardViewer : Viewer
	{
		// viewer support
		private StandardViewerWindow window;
		private bool visible;

		// stuff used for displaying the scans
		Scan pointsToPlot = new Scan();
		double startSpectrumGate;
		double endSpectrumGate;
		double startTOFGate;
		double endTOFGate;
		private int shotCounter = 0;
		private int onAverages = 1;
		private int offAverages = 1;

		TOF avOnTof;
		TOF avOffTof;

		public String Name
		{
			get { return "Standard"; }
		}

		public StandardViewer()
		{
			window = new StandardViewerWindow(this);
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
			// clear the stored data
			pointsToPlot.Points.Clear();
			// grab the latest settings
			PluginSettings outputSettings = Controller.GetController().ProfileManager.
				CurrentProfile.AcquisitorConfig.outputPlugin.Settings;
			PluginSettings shotSettings = Controller.GetController().ProfileManager.
				CurrentProfile.AcquisitorConfig.shotGathererPlugin.Settings;

			// initially set the gates to full
			startSpectrumGate = (double)outputSettings["start"];
			endSpectrumGate = (double)outputSettings["end"];
			startTOFGate = (int)shotSettings["gateStartTime"];
			endTOFGate = (int)shotSettings["gateStartTime"] + (int)shotSettings["gateLength"];

			// prepare the front panel
			window.ClearAll();
			window.SpectrumAxes = new NationalInstruments.UI.Range(
				(double)outputSettings["start"], (double)outputSettings["end"]);
			window.SpectrumGate = new NationalInstruments.UI.Range(startSpectrumGate, endSpectrumGate);
			window.TOFGate = new NationalInstruments.UI.Range(startTOFGate, endTOFGate);

		}

		public void AcquireStop()
		{
			window.ClearAll();
			DataStore dataStore = Controller.GetController().DataStore;
			if (dataStore.NumberOfScans != 0)
			{
				// replot the averages
				UpdatePMTAveragePlots();
				UpdateTOFAveragePlots();
			
				// plot the average analog channels in the analog plots for convenience
				window.AppendToAnalog1(dataStore.AverageScan.ScanParameterArray,
					dataStore.AverageScan.GetAnalogArray(0));
				window.AppendToAnalog2(dataStore.AverageScan.ScanParameterArray,
					dataStore.AverageScan.GetAnalogArray(1));
			}
		}

		public void HandleDataPoint(DataEventArgs e)
		{
			Profile currentProfile = Controller.GetController().ProfileManager.CurrentProfile;
			// update the TOF graphs

			if (currentProfile.GUIConfig.average)
			{
				if (shotCounter % currentProfile.GUIConfig.updateTOFsEvery == 0) 
				{
					if (avOnTof != null)
					{
						window.PlotOnTOF(avOnTof/onAverages);
					}
					avOnTof = (TOF) e.point.AverageOnShot.TOFs[0];
					onAverages = 1;

					if ((bool)currentProfile.AcquisitorConfig.switchPlugin.Settings["switchActive"])
					{
						if (avOffTof != null)
						{
							window.PlotOffTOF(avOffTof/offAverages);
						}
						avOffTof = (TOF) e.point.AverageOffShot.TOFs[0];
						offAverages = 1;
					}
				}
				else // do the averaging
				{
					if (avOnTof != null)
					{
						avOnTof = avOnTof + ((TOF) e.point.AverageOnShot.TOFs[0]);
						onAverages++;
					}
					if ((bool)currentProfile.AcquisitorConfig.switchPlugin.Settings["switchActive"] && avOffTof != null)
					{
						avOffTof = avOffTof + ((TOF) e.point.AverageOffShot.TOFs[0]);
						offAverages++;
					}
				}
			}

			else  // if not averaging
			{
				if (shotCounter % currentProfile.GUIConfig.updateTOFsEvery == 0) 
				{
					window.PlotOnTOF((TOF)e.point.AverageOnShot.TOFs[0]);
					if ((bool)currentProfile.AcquisitorConfig.switchPlugin.Settings["switchActive"])
					{
						window.PlotOffTOF((TOF)e.point.AverageOffShot.TOFs[0]);
					}
				}
			}


			// update the spectra
			pointsToPlot.Points.Add(e.point);
			if (shotCounter % currentProfile.GUIConfig.updateSpectraEvery == 0)
			{
				window.AppendToAnalog1(pointsToPlot.ScanParameterArray, pointsToPlot.GetAnalogArray(0));
				window.AppendToAnalog2(pointsToPlot.ScanParameterArray, pointsToPlot.GetAnalogArray(1));
				window.AppendToPMTOn(pointsToPlot.ScanParameterArray,
					pointsToPlot.GetTOFOnIntegralArray(0,
					startTOFGate, endTOFGate));
				if ((bool)currentProfile.AcquisitorConfig.switchPlugin.Settings["switchActive"])
				{
					window.AppendToPMTOff(pointsToPlot.ScanParameterArray,
						pointsToPlot.GetTOFOffIntegralArray(0,
						startTOFGate, endTOFGate));
					window.AppendToDifference(pointsToPlot.ScanParameterArray,
						pointsToPlot.GetDifferenceIntegralArray(0,
						startTOFGate, endTOFGate));
				}
				pointsToPlot.Points.Clear();
			}
			shotCounter++;
		}

		public void ScanFinished()
		{
			// update the gate window
			startSpectrumGate = window.SpectrumGate.Minimum;
			endSpectrumGate = window.SpectrumGate.Maximum;
			startTOFGate = window.TOFGate.Minimum;
			endTOFGate = window.TOFGate.Maximum;
			
			UpdatePMTAveragePlots();
			UpdateTOFAveragePlots();
			
			// clear the realtime spectra
			pointsToPlot.Points.Clear();
			window.ClearRealtimeSpectra();
		}


		// The main window calls this when a TOF cursor is moved.
		public void TOFCursorMoved()
		{
			if (Controller.GetController().appState == Controller.AppState.stopped)
			{
				startTOFGate = window.TOFGate.Minimum;
				endTOFGate = window.TOFGate.Maximum;
				UpdatePMTAveragePlots();
			}
		}
		
		// The main window calls this when a PMT cursor is moved.
		public void PMTCursorMoved()
		{
			if (Controller.GetController().appState == Controller.AppState.stopped)
			{
				startSpectrumGate = window.SpectrumGate.Minimum;
				endSpectrumGate = window.SpectrumGate.Maximum;
				UpdateTOFAveragePlots();
			}
		}

		public void NewScanLoaded()
		{
			window.ClearAll();
			Scan averageScan = Controller.GetController().DataStore.AverageScan;
			if (averageScan.Points.Count == 0) return;
			startSpectrumGate = averageScan.ScanParameterArray[0];
			endSpectrumGate = averageScan.ScanParameterArray[averageScan.ScanParameterArray.Length - 1];
			TOF tof = ((TOF)((Shot)((ScanPoint)averageScan.Points[0]).OnShots[0]).TOFs[0]);
			startTOFGate = tof.GateStartTime;
			endTOFGate = tof.GateStartTime + tof.Length;
			UpdateTOFAveragePlots();
			UpdatePMTAveragePlots();
			window.SpectrumGate = new NationalInstruments.UI.Range(startSpectrumGate, endSpectrumGate);
			window.TOFGate = new NationalInstruments.UI.Range(startTOFGate, endTOFGate);
		}

		public double SpectrumGateLow
		{
			get { return window.SpectrumGate.Minimum; }
		}

		public double SpectrumGateHigh
		{
			get { return window.SpectrumGate.Maximum; }
		}
		
		public double TOFGateLow
		{
			get { return window.TOFGate.Minimum; }
		}

		public double TOFGateHigh
		{
			get { return window.TOFGate.Maximum; }
		}

		private void UpdateTOFAveragePlots()
		{
			Scan averageScan = Controller.GetController().DataStore.AverageScan;
			if (averageScan.Points.Count == 0) return;
			TOF tof =
				(TOF)((ArrayList)averageScan.GetGatedAverageOnShot(startSpectrumGate, endSpectrumGate).TOFs)[0];
			window.PlotAverageOnTOF(tof);
			Profile p = Controller.GetController().ProfileManager.CurrentProfile;
			if (p != null && (bool)p.AcquisitorConfig.switchPlugin.Settings["switchActive"]) 
			{
				window.PlotAverageOffTOF(
					(TOF)averageScan.GetGatedAverageOffShot(startSpectrumGate, endSpectrumGate).TOFs[0]);
			}
		}

		private void UpdatePMTAveragePlots()
		{
			Scan averageScan = Controller.GetController().DataStore.AverageScan;
			if (averageScan.Points.Count == 0) return;
			window.SpectrumAxes = new NationalInstruments.UI.Range(averageScan.ScanParameterArray[0],
										averageScan.ScanParameterArray[averageScan.ScanParameterArray.Length - 1]);
			window.PlotAveragePMTOn(averageScan.ScanParameterArray,
				averageScan.GetTOFOnIntegralArray(0,
				startTOFGate, endTOFGate));
			Profile p = Controller.GetController().ProfileManager.CurrentProfile;
			if (p != null && (bool)p.AcquisitorConfig.switchPlugin.Settings["switchActive"]) 
			{
				window.PlotAveragePMTOff(averageScan.ScanParameterArray,
					averageScan.GetTOFOffIntegralArray(0,
					startTOFGate, endTOFGate));
				window.PlotAverageDifference(averageScan.ScanParameterArray,
					averageScan.GetDifferenceIntegralArray(0,
					startTOFGate, endTOFGate));
			}
		}

		private void UpdateFitPlots()
		{
		}

	}
}
