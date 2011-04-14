using System;
using System.Collections;

using Data;
using Data.Scans;
using ScanMaster;
using ScanMaster.Acquire;
using ScanMaster.Acquire.Plugin;
using ScanMaster.Analyze;

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

		// fitting config
		enum FitMode {None, Shot, Average};
		FitMode tofFitMode = FitMode.None;
		FitMode spectrumFitMode = FitMode.None;
		Hashtable fitters = new Hashtable();
		Fitter tofFitter;
		Fitter spectrumFitter;

		public String Name
		{
			get { return "Standard"; }
		}

		public StandardViewer()
		{
			window = new StandardViewerWindow(this);
            AddFitter(new TofFitter());
			AddFitter( new LorentzianFitter() );
            AddFitter(new GaussianFitter());
			AddFitter(new SincFitter());
			AddFitter(new InterferenceFitter());
			window.tofFitModeCombo.SelectedIndex = 0;
            window.spectrumFitModeCombo.SelectedIndex = 0;
            window.tofFitFunctionCombo.SelectedIndex = 0;
			window.spectrumFitFunctionCombo.SelectedIndex = 0;
            window.tofFitDataSelectCombo.SelectedIndex = 0;
		}

		private void AddFitter(Fitter f)
		{
			fitters.Add(f.Name, f);
			window.tofFitFunctionCombo.Items.Add(f);
			window.spectrumFitFunctionCombo.Items.Add(f);
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
            PluginSettings pgSettings = Controller.GetController().ProfileManager.CurrentProfile.AcquisitorConfig.pgPlugin.Settings;

			// initially set the gates to full
			startSpectrumGate = (double)outputSettings["start"];
			endSpectrumGate = (double)outputSettings["end"];

			// prepare the front panel
			window.ClearAll();
			window.SpectrumAxes = new NationalInstruments.UI.Range(
				(double)outputSettings["start"], (double)outputSettings["end"]);
			window.SpectrumGate = new NationalInstruments.UI.Range(startSpectrumGate, endSpectrumGate);
            startTOFGate = (int)shotSettings["gateStartTime"];
            endTOFGate = startTOFGate + (int)shotSettings["gateLength"] * (int)shotSettings["clockPeriod"];
            window.TOFGate = new NationalInstruments.UI.Range(startTOFGate, endTOFGate);

			// disable the fit function selectors
            window.SetTofFitFunctionComboState(false);
            window.SetSpectrumFitFunctionComboState(false);
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
				if (dataStore.AverageScan.AnalogChannelCount >= 1)
                    window.AppendToAnalog1(dataStore.AverageScan.ScanParameterArray,
					    dataStore.AverageScan.GetAnalogArray(0));
                if (dataStore.AverageScan.AnalogChannelCount >= 2) 
                    window.AppendToAnalog2(dataStore.AverageScan.ScanParameterArray,
					    dataStore.AverageScan.GetAnalogArray(1));

                // replot the fits
                if (spectrumFitMode != FitMode.None && spectrumFitter.FittedValues.Length != 0)
                {
                    // watch out for the case where the scan is stopped when there are some points
                    // taken which have yet to be fitted
                    double[] xValues = new double[spectrumFitter.FittedValues.Length];
                    Array.Copy(dataStore.AverageScan.ScanParameterArray, xValues, xValues.Length);
                    window.PlotSpectrumFit(xValues, spectrumFitter.FittedValues);
                }
			}
            // enable the fit function selectors
            window.SetTofFitFunctionComboState(true);
            window.SetSpectrumFitFunctionComboState(true);
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
                if (pointsToPlot.AnalogChannelCount >= 1)
				    window.AppendToAnalog1(pointsToPlot.ScanParameterArray, pointsToPlot.GetAnalogArray(0));
                if (pointsToPlot.AnalogChannelCount >= 2) 
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
                // update the spectrum fit if in shot mode.
                if (spectrumFitMode == FitMode.Shot)
                {
                    Scan currentScan = Controller.GetController().DataStore.CurrentScan;
                    if (currentScan.Points.Count > 10)
                    {
						FitAndPlotSpectrum(currentScan);
                   }
                }
				pointsToPlot.Points.Clear();
			}
			shotCounter++;
		}

		private void FitAndPlotSpectrum(Scan scan)
		{
			FitScan(scan, startTOFGate, endTOFGate);
			// plot the fit
			window.ClearSpectrumFit();
			window.PlotSpectrumFit(scan.ScanParameterArray, spectrumFitter.FittedValues);
			// update the parameter report
			window.SetLabel(window.spectrumFitResultsLabel, spectrumFitter.ParameterReport);
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

			// update the fits
			if (spectrumFitMode == FitMode.Average)
			{
				Scan averageScan = Controller.GetController().DataStore.AverageScan;
				FitAndPlotSpectrum(averageScan);
			}

			if (tofFitMode == FitMode.Average)
			{
				FitAverageTOF();
			}
			

			// clear the realtime spectra
			pointsToPlot.Points.Clear();
			window.ClearRealtimeSpectra();
		}

		private void FitAverageTOF()
		{
			Scan averageScan = Controller.GetController().DataStore.AverageScan;
			TOF tof;
            if (window.GetTofFitDataSelection() == 0)
            {
                tof = (TOF)((ArrayList)averageScan.GetGatedAverageOnShot(
                                                    startSpectrumGate,
                                                    endSpectrumGate).TOFs)[0];
            }
            else
            {
                if (((ScanPoint)(averageScan.Points[0])).OffShots.Count == 0) return; //make sure there are some offshots
                tof = (TOF)((ArrayList)averageScan.GetGatedAverageOffShot(
                                                    startSpectrumGate,
                                                    endSpectrumGate).TOFs)[0];
            }
			double[] doubleTimes = new double[tof.Times.Length];
			for (int i = 0; i < tof.Times.Length; i++) doubleTimes[i] = (double)tof.Times[i];
			tofFitter.Fit(
				doubleTimes,
				tof.Data,
				tofFitter.SuggestParameters(
					doubleTimes,
					tof.Data,
					tof.Times[0],
					tof.Times[tof.Times.Length - 1])
			);
			window.ClearTOFFit();
			window.PlotTOFFit(tof.GateStartTime, tof.ClockPeriod, tofFitter.FittedValues);
			// update the parameter report
			window.SetLabel(window.tofFitResultsLabel, tofFitter.ParameterReport);
		}

        private void FitScan(Scan s, double gateStart, double gateEnd)
        {
            double scanStart = SpectrumGateLow;
            double scanEnd = SpectrumGateHigh;
            double[] xDat = s.ScanParameterArray;
            double[] yDat = s.GetTOFOnIntegralArray(0, gateStart, gateEnd);
            spectrumFitter.Fit(
                xDat,
                yDat,
                spectrumFitter.SuggestParameters(xDat, yDat, scanStart, scanEnd)
                );
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
                window.SetStatus("S: " + string.Format("{0:N4}", startSpectrumGate) + " E: " + string.Format("{0:N4}", endSpectrumGate));
				UpdateTOFAveragePlots();
			}
		}

		public void NewScanLoaded()
		{
			window.ClearAll();
			Scan averageScan = Controller.GetController().DataStore.AverageScan;
			if (averageScan.Points.Count == 0) return;
            startSpectrumGate = averageScan.MinimumScanParameter;
			endSpectrumGate = averageScan.MaximumScanParameter;
			TOF tof = ((TOF)((Shot)((ScanPoint)averageScan.Points[0]).OnShots[0]).TOFs[0]);
			startTOFGate = tof.GateStartTime;
			endTOFGate = tof.GateStartTime + tof.Length;
			UpdateTOFAveragePlots();
			UpdatePMTAveragePlots();
			window.SpectrumGate = new NationalInstruments.UI.Range(startSpectrumGate, endSpectrumGate);
			window.TOFGate = new NationalInstruments.UI.Range(startTOFGate, endTOFGate);
			if (spectrumFitMode == FitMode.Average) FitAndPlotSpectrum(averageScan);
			if (tofFitMode == FitMode.Average) FitAverageTOF();
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
            window.SpectrumAxes = new NationalInstruments.UI.Range(averageScan.MinimumScanParameter,
                 averageScan.MaximumScanParameter);
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

		public void TOFFitModeChanged(int index)
		{
			UpdateFitMode(ref tofFitMode, index);
			window.ClearTOFFit();
        }

		public void SpectrumFitModeChanged(int index)
		{
            UpdateFitMode(ref spectrumFitMode, index);
            window.ClearSpectrumFit();
        }

        private void UpdateFitMode(ref FitMode f, int index)
        {
            switch (index)
            {
                case 0:
                    f = FitMode.None;
                    break;
                case 1:
					f = FitMode.Average;
					break;
                case 2:
					f = FitMode.Shot;
                    break;
            }
        }

        public void TOFFitFunctionChanged(object item)
		{
			tofFitter = (Fitter)item;
			window.ClearTOFFit();
		}

		public void SpectrumFitFunctionChanged(object item)
		{
			spectrumFitter = (Fitter)item;
            window.ClearSpectrumFit();
		}


		internal void UpdateTOFFit()
		{
			Scan averageScan = Controller.GetController().DataStore.AverageScan;
			if (averageScan.Points.Count != 0) FitAverageTOF();
		}

		internal void UpdateSpectrumFit()
		{
			Scan averageScan = Controller.GetController().DataStore.AverageScan;
			if (averageScan.Points.Count != 0) FitAndPlotSpectrum(averageScan);
		}
	}
}
