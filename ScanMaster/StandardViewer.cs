using System;
using System.Collections;

using Data;
using Data.Scans;
using ScanMaster;
using ScanMaster.Acquire;
using ScanMaster.Acquire.Plugin;
using ScanMaster.Analyze;

using DAQ.Analyze;
using DAQ.Environment;

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
		double bgStartTime;
		double bgEndTime;
		private int shotCounter = 0;
		private int onAverages = 1;
		private int offAverages = 1;

		//TOF avOnTof;
		//TOF avOffTof;
		ArrayList avOnTofs;
		ArrayList avOffTofs;

		// fitting config
		enum FitMode { None, Shot, Average };
		FitMode tofFitMode = FitMode.None;
		FitMode spectrumFitMode = FitMode.None;
		Hashtable fitters = new Hashtable();
		Fitter tofFitter;
		//Fitter spectrumFitter;//14Sept2025: Guanchen is going to get two fitters
		Fitter spectrumFitterOffShots;
		Fitter spectrumFitterOnShots;
		Fitter spectrumFitterRatio = new ExponentialFitter();
		double OverShotNoise0;
		double OverShotNoise1;
		double OverShotNoise01;


		public String Name
		{
			get { return "Standard"; }
		}

		public StandardViewer()
		{
			window = new StandardViewerWindow(this);
			AddFitter(new TofFitter());
			AddFitter(new LorentzianFitter());
			AddFitter(new GaussianFitter());
			AddFitter(new SincFitter());
			AddFitter(new InterferenceFitter());
			// AddFitter(new ExponentialFitter());
			window.tofFitModeCombo.SelectedIndex = 0;
			window.spectrumFitModeCombo.SelectedIndex = 0;
			window.tofFitFunctionCombo.SelectedIndex = 0;
			window.spectrumFitFunctionCombo.SelectedIndex = 2;//by default, let's fit it to the Gaussian function
			window.tofFitDataSelectCombo.SelectedIndex = 0;
			SetTOFAxesRangeToDefault();
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
			if (avOnTofs != null) avOnTofs = null;
			if (avOffTofs != null) avOffTofs = null;
			shotCounter = 0;
			onAverages = 1;
			offAverages = 1;
			// grab the latest settings
			PluginSettings outputSettings = Controller.GetController().ProfileManager.
				CurrentProfile.AcquisitorConfig.outputPlugin.Settings;
			PluginSettings shotSettings = Controller.GetController().ProfileManager.
				CurrentProfile.AcquisitorConfig.shotGathererPlugin.Settings;
			PluginSettings pgSettings = Controller.GetController().ProfileManager.CurrentProfile.AcquisitorConfig.pgPlugin.Settings;

			// initially set the gates to full
			startSpectrumGate = (double)outputSettings["start"] + 0.1 * ((double)outputSettings["end"] - (double)outputSettings["start"]);
			endSpectrumGate = (double)outputSettings["end"] - 0.1 * ((double)outputSettings["end"] - (double)outputSettings["start"]);

			// prepare the front panel
			window.ClearAll();
			window.SpectrumAxes = new NationalInstruments.UI.Range(
				(double)outputSettings["start"], (double)outputSettings["end"]);
			window.SpectrumGate = new NationalInstruments.UI.Range(startSpectrumGate, endSpectrumGate);
			window.ratioPlotCursor = new NationalInstruments.UI.Range(startSpectrumGate, endSpectrumGate);
			window.differencePlotCursor = new NationalInstruments.UI.Range(startSpectrumGate, endSpectrumGate);
			//startTOFGate = (int)shotSettings["gateStartTime"];
			startTOFGate = 100 * (int)shotSettings["TOFgateSelectionStartInMs"];// 100 is the convertion factor for the unit of ms to the unit of clock period. 
																				// Example:TOFgateSelectionStartInMs=14 means we want to select gate from 14 ms. Then the startTOFGate=1400, which is 1400 numbers of the clock period. 
																				// The clock period is set to be 10 us, so 1400 clock period is 14000 us, which is 14 ms. 
																				//endTOFGate = startTOFGate + (int)shotSettings["gateLength"] * (int)shotSettings["clockPeriod"];
			endTOFGate = 100 * (int)shotSettings["TOFgateSelectionEndInMs"];// This is modified by Guanchen on 01Oct2024
			bgStartTime = 100 * (int)shotSettings["TOFgateBgStartInMs"];
			bgEndTime = 100 * (int)shotSettings["TOFgateBgEndInMs"];
			window.TOFGate = new NationalInstruments.UI.Range(startTOFGate, endTOFGate);



			// disable the fit function selectors
			window.SetTofFitFunctionComboState(false);
			window.SetSpectrumFitFunctionComboState(false);


			//update the displayed current scan number
			int currentScanNum = Controller.GetController().DataStore.NumberOfScans + 1;
			window.label7.Text = "Final scan num: " + currentScanNum.ToString();
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
				if (spectrumFitMode != FitMode.None && spectrumFitterOffShots.FittedValues.Length != 0)
				{
					// watch out for the case where the scan is stopped when there are some points
					// taken which have yet to be fitted
					double[] xValues = new double[spectrumFitterOffShots.FittedValues.Length];
					Array.Copy(dataStore.AverageScan.ScanParameterArray, xValues, xValues.Length);
					window.PlotSpectrumFitOffAvg(xValues, spectrumFitterOffShots.FittedValues);
				}
				if (spectrumFitMode != FitMode.None && spectrumFitterOnShots.FittedValues.Length != 0)
				{
					// watch out for the case where the scan is stopped when there are some points
					// taken which have yet to be fitted
					double[] xValues = new double[spectrumFitterOnShots.FittedValues.Length];
					Array.Copy(dataStore.AverageScan.ScanParameterArray, xValues, xValues.Length);
					window.PlotSpectrumFitOnAvg(xValues, spectrumFitterOnShots.FittedValues);
				}
			}
			// enable the fit function selectors
			window.SetTofFitFunctionComboState(true);
			window.SetSpectrumFitFunctionComboState(true);
			//update the current scan number dispaled at the bottom right corner
			int currentScanNum = Controller.GetController().DataStore.NumberOfScans;
			window.label7.Text = "Current scan num: " + currentScanNum.ToString();
		}

		public void HandleDataPoint(DataEventArgs e)
		{
			Profile currentProfile = Controller.GetController().ProfileManager.CurrentProfile;


			// update the TOF graphs

			if (currentProfile.GUIConfig.average)
			{
				if (shotCounter % currentProfile.GUIConfig.updateTOFsEvery == 0)
				{
					if (avOnTofs != null)
					{
						for (int i = 0; i < avOnTofs.Count; i++) { avOnTofs[i] = ((TOF)avOnTofs[i]) / onAverages; }
						window.PlotOnTOF(avOnTofs);
						if (avOnTofs.Count > 1)
						{
							// integral between the signal gates minus the integral between the background gates (adjusted for the relative width of the two gates)
							//double normVal = (((TOF)avOnTofs[1]).Integrate(NormSigGateLow, NormSigGateHigh)) - (((TOF)avOnTofs[1]).Integrate(NormBgGateLow, NormBgGateHigh)) * (NormSigGateHigh - NormSigGateLow) / (NormBgGateHigh - NormBgGateLow);
							//window.PlotNormedOnTOF(((TOF)avOnTofs[0]) / normVal);
						}
					}
					avOnTofs = e.point.AverageOnShot.TOFs;
					onAverages = 1;

					if ((bool)currentProfile.AcquisitorConfig.switchPlugin.Settings["switchActive"])
					{
						if (avOffTofs != null)
						{
							for (int i = 0; i < avOffTofs.Count; i++) { avOffTofs[i] = ((TOF)avOffTofs[i]) / offAverages; }
							window.PlotOffTOF(avOffTofs);
							if (avOffTofs.Count > 1)
							{
								//double normVal = (((TOF)avOffTofs[1]).Integrate(NormSigGateLow, NormSigGateHigh)) - (((TOF)avOffTofs[1]).Integrate(NormBgGateLow, NormBgGateHigh)) * (NormSigGateHigh - NormSigGateLow) / (NormBgGateHigh - NormBgGateLow);
								//window.PlotNormedOffTOF(((TOF)avOffTofs[0]) / normVal);
							}
						}
						avOffTofs = e.point.AverageOffShot.TOFs;
						offAverages = 1;
					}
				}
				else // do the averaging
				{
					if (avOnTofs != null)
					{
						//avOnTof = avOnTof + ((TOF) e.point.AverageOnShot.TOFs[0]);
						for (int i = 0; i < avOnTofs.Count; i++) { avOnTofs[i] = (TOF)avOnTofs[i] + ((TOF)e.point.AverageOnShot.TOFs[i]); }
						onAverages++;
					}
					if ((bool)currentProfile.AcquisitorConfig.switchPlugin.Settings["switchActive"] && avOffTofs != null)
					{
						//avOffTof = avOffTof + ((TOF) e.point.AverageOffShot.TOFs[0]);
						for (int i = 0; i < avOffTofs.Count; i++) { avOffTofs[i] = (TOF)avOffTofs[i] + ((TOF)e.point.AverageOffShot.TOFs[i]); }
						offAverages++;
					}
				}
			}

			else  // if not averaging
			{
				if (shotCounter % currentProfile.GUIConfig.updateTOFsEvery == 0)
				{
					ArrayList currentTofs = e.point.AverageOnShot.TOFs;
					window.PlotOnTOF(currentTofs);
					if (currentTofs.Count >1)//currentTOFs are aquired as Ch1_YgOn, Ch1_YgOff, Ch2_YgOn, Ch2_YgOff
					{
						//double normVal = (((TOF)currentTofs[1]).Integrate(NormSigGateLow, NormSigGateHigh)) - (((TOF)currentTofs[1]).Integrate(NormBgGateLow, NormBgGateHigh)) * (NormSigGateHigh - NormSigGateLow) / (NormBgGateHigh - NormBgGateLow);
						TOF ch1TOFbgRemovedOnShots = (TOF)currentTofs[0] - (TOF)currentTofs[1];

						window.PlotNormedOnTOF(ch1TOFbgRemovedOnShots);

					}
					if ((bool)currentProfile.AcquisitorConfig.switchPlugin.Settings["switchActive"])
					{
						currentTofs = e.point.AverageOffShot.TOFs;
						window.PlotOffTOF(currentTofs);
						if (currentTofs.Count >1)
						{
							//double normVal = (((TOF)currentTofs[1]).Integrate(NormSigGateLow, NormSigGateHigh)) - (((TOF)currentTofs[1]).Integrate(NormBgGateLow, NormBgGateHigh)) * (NormSigGateHigh - NormSigGateLow) / (NormBgGateHigh - NormBgGateLow);
							//window.PlotNormedOffTOF(((TOF)currentTofs[0]) PlotY(tofGraphNormed, tofOnAverageNormedPlot, ((TOF)t[0]).GateStartTime, ((TOF)t[0]).ClockPeriod, ((TOF)t[0]-(TOF)t[1]).Data););

							//double normVal = (((TOF)currentTofs[1]).Integrate(NormSigGateLow, NormSigGateHigh)) - (((TOF)currentTofs[1]).Integrate(NormBgGateLow, NormBgGateHigh)) * (NormSigGateHigh - NormSigGateLow) / (NormBgGateHigh - NormBgGateLow);
							TOF ch1TOFbgRemovedOffShots = (TOF)currentTofs[0] - (TOF)currentTofs[1];

							window.PlotNormedOffTOF(ch1TOFbgRemovedOffShots);

						}
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
					//pointsToPlot.GetTOFOnIntegralArray(0,startTOFGate, endTOFGate)//Without background subtraction
					pointsToPlot.GetBackgroundSubstractedTOFOnIntegralArray(0, startTOFGate, endTOFGate, bgStartTime, bgEndTime)//with backgroun subtraction
					//pointsToPlot.TOFOnIntegralArrayYgOnMinusOff(startTOFGate, endTOFGate)//ygOn-ygOff
					//with bg subtraction
					);


				if ((bool)currentProfile.AcquisitorConfig.switchPlugin.Settings["switchActive"])
				{
					window.AppendToPMTOff(pointsToPlot.ScanParameterArray,
						//pointsToPlot.GetTOFOffIntegralArray(0,startTOFGate, endTOFGate)//Without background subtraction
						pointsToPlot.GetBackgroundSubstractedTOFOffIntegralArray(0, startTOFGate, endTOFGate, bgStartTime, bgEndTime)//with bg subtraction
						);
					window.AppendToDifference(pointsToPlot.ScanParameterArray,
						//pointsToPlot.GetDifferenceIntegralArray(0,startTOFGate, endTOFGate)//Without background subtraction
						pointsToPlot.GetBgSubtractedDifferenceIntegralArray(0, startTOFGate, endTOFGate, bgStartTime, bgEndTime)//with bg subtraction
						);
					window.AppendToRatio(pointsToPlot.ScanParameterArray,
						pointsToPlot.GetBgSubtractedRatioIntegralArray(0, startTOFGate, endTOFGate, bgStartTime, bgEndTime)//with bg subtraction
						);
				}
				// update the spectrum fit if in shot mode.
				if (spectrumFitMode == FitMode.Shot)
				{
					Scan currentScan = Controller.GetController().DataStore.CurrentScan;
					if (currentScan.Points.Count > 10)
					{
						FitAndPlotSpectrumOffShots(currentScan);
					}
				}
				pointsToPlot.Points.Clear();
			}
			shotCounter++;


		}

		private void FitAndPlotSpectrumOffShots(Scan scan)
		{
			//FitScan(scan, startTOFGate, endTOFGate);
			FitScanOffShots(scan);
			// plot the fit
			window.ClearSpectrumFitOffShots();
			window.PlotSpectrumFitOffAvg(scan.ScanParameterArray, spectrumFitterOffShots.FittedValues);
			// update the parameter report
			window.SetLabel(window.spectrumFitResultsLabelOffShots, spectrumFitterOffShots.ParameterReport);
		}

		private void FitAndPlotSpectrumOnShots(Scan scan)
		{
			//FitScan(scan, startTOFGate, endTOFGate);
			FitScanOnShots(scan);
			// plot the fit
			window.ClearSpectrumFitOnShots();
			window.PlotSpectrumFitOnAvg(scan.ScanParameterArray, spectrumFitterOnShots.FittedValues);
			// update the parameter report
			window.SetLabel(window.spectrumFitResultsLabelOnShots, spectrumFitterOnShots.ParameterReport);
		}

		private void FitAndPlotRatio(Scan scan)
		{
			//FitScan(scan, startTOFGate, endTOFGate);
			FitScanRatio(scan);
			// plot the fit
			window.ClearSpectrumFitRatio();
			window.PlotSpectrumFitRatio(scan.ScanParameterArray, spectrumFitterRatio.FittedValues);
			// update the parameter report
			window.SetLabel(window.ratioFitResults, spectrumFitterRatio.ParameterReport);
		}

		private void AnalyseNoise(Scan scan)
		{
			AnalyseScanNoise(scan, startTOFGate, endTOFGate, startSpectrumGate, endSpectrumGate);
			// update the noise report
			string[] info = { "top:", OverShotNoise0.ToString("G3"), "norm:", OverShotNoise1.ToString("G3"), "Normed:", OverShotNoise01.ToString("G3") };
			window.SetLabel(window.noiseResultsLabel, String.Join(" ", info));
		}


		public void ScanFinished()
		{
			// update the gate window
			startSpectrumGate = window.SpectrumGate.Minimum + 0.1 * (window.SpectrumGate.Maximum - window.SpectrumGate.Minimum);
			endSpectrumGate = window.SpectrumGate.Maximum - 0.1 * (window.SpectrumGate.Maximum - window.SpectrumGate.Minimum);
			startTOFGate = window.TOFGate.Minimum;
			endTOFGate = window.TOFGate.Maximum;

			UpdatePMTAveragePlots();
			UpdateTOFAveragePlots();

			// update the fits
			if (spectrumFitMode == FitMode.Average)
			{
				Scan averageScan = Controller.GetController().DataStore.AverageScan;
				FitAndPlotSpectrumOffShots(averageScan);
			}

			if (tofFitMode == FitMode.Average)
			{
				FitAverageTOF();
			}


			// clear the realtime spectra
			pointsToPlot.Points.Clear();
			window.ClearRealtimeSpectra();
			//update the current scan number dispaled at the bottom right corner
			//int currentScanNum = Controller.GetController().DataStore.NumberOfScans;
			//window.label7.Text = "Current scan num: " + currentScanNum.ToString();
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

		//private void FitScan(Scan s, double gateStart, double gateEnd)
		private void FitScanOnShots(Scan s)
		{
			double scanStart = SpectrumGateLow;//Guanchen13Sept2025:seems not useful 
			double scanEnd = SpectrumGateHigh;//Guanchen13Sept2025:seems not useful 
			double[] xDat = s.ScanParameterArray;
			//double[] yDat = s.GetTOFOnIntegralArray(0, gateStart, gateEnd); //the orignal code
			double[] yDat = s.GetBackgroundSubstractedTOFOnIntegralArray(0, startTOFGate, endTOFGate, bgStartTime, bgEndTime);//Guanchen13Sept2025:updated to use background substraction
			spectrumFitterOnShots.Fit(
				xDat,
				yDat,
				spectrumFitterOnShots.SuggestParameters(xDat, yDat, scanStart, scanEnd)
				);
		}

		private void FitScanOffShots(Scan s)
		{
			double scanStart = SpectrumGateLow;//Guanchen13Sept2025:seems not useful 
			double scanEnd = SpectrumGateHigh;//Guanchen13Sept2025:seems not useful 
			double[] xDat = s.ScanParameterArray;
			//double[] yDat = s.GetTOFOnIntegralArray(0, gateStart, gateEnd); //the orignal code
			double[] yDat = s.GetBackgroundSubstractedTOFOffIntegralArray(0, startTOFGate, endTOFGate, bgStartTime, bgEndTime);//Guanchen13Sept2025:updated to use background substraction
			spectrumFitterOffShots.Fit(
				xDat,
				yDat,
				spectrumFitterOffShots.SuggestParameters(xDat, yDat, scanStart, scanEnd)
				);
		}

		private void FitScanRatio(Scan s)
		{
			double scanStart = SpectrumGateLow;//Guanchen13Sept2025:seems not useful 
			double scanEnd = SpectrumGateHigh;//Guanchen13Sept2025:seems not useful 
			double[] xDat = s.ScanParameterArray;
			//double[] yDat = s.GetTOFOnIntegralArray(0, gateStart, gateEnd); //the orignal code
			double[] yDat = s.GetBgSubtractedRatioIntegralArray(0, startTOFGate, endTOFGate, bgStartTime, bgEndTime);//Guanchen13Sept2025:updated to use background substraction
			spectrumFitterRatio.Fit(
				xDat,
				yDat,
				spectrumFitterRatio.SuggestParameters(xDat, yDat, scanStart, scanEnd)
				);
		}

		private void AnalyseScanNoise(Scan s, double gateStart, double gateEnd, double spectrumGateStart, double spectrumGateEnd)
		{
			double detectorRatio = (double)Environs.Hardware.GetInfo("machineLengthRatio");
			string channelList = (string)s.GetSetting("shot", "channel");
			string[] channels = channelList.Split(new char[] { ',' });
			double[] probedat;
			double[] pumpdat;
			double[] normedDat;

			if (channels.Length == 2)
			{
				int[] detectors = { 0, 1 };

				probedat = s.GetTOFOnOverShotNoiseArray(0, gateStart, gateEnd);
				pumpdat = s.GetTOFOnOverShotNoiseArray(1, gateStart / detectorRatio, gateEnd / detectorRatio);
				normedDat = s.GetTOFOnOverShotNoiseNormedArray(detectors, gateStart, gateEnd, gateStart / detectorRatio, gateEnd / detectorRatio);
			}
			else
			{
				probedat = s.GetTOFOnOverShotNoiseArray(0, gateStart, gateEnd);
				pumpdat = probedat;
				normedDat = probedat;
			}

			double tempP = 0;
			double tempN = 0;
			double tempTN = 0;

			double scanStart = s.ScanParameterArray[0];
			double scanEnd = s.ScanParameterArray[s.ScanParameterArray.Length - 1];

			int pointStart = (int)Math.Round((spectrumGateStart * (probedat.Length - 1)) / (scanEnd - scanStart));
			int pointEnd = (int)Math.Round((spectrumGateEnd * (probedat.Length - 1)) / (scanEnd - scanStart));
			int pointLength = pointEnd - pointStart;

			for (int i = pointStart; i < pointEnd; i++)
			{
				tempP += probedat[i];
				tempN += pumpdat[i];
				tempTN += normedDat[i];
			}

			OverShotNoise0 = tempP / pointLength;
			OverShotNoise1 = tempN / pointLength;
			OverShotNoise01 = tempTN / pointLength;
		}

		// The main window calls this when a TOF cursor is moved.
		public void TOFCursorMoved()
		{
			if (Controller.GetController().appState == Controller.AppState.stopped)
			{
				startTOFGate = window.TOFGate.Minimum;
				endTOFGate = window.TOFGate.Maximum;
				window.SetTOFStatus("S: " + string.Format("{0:N1}", startTOFGate) + " E: " + string.Format("{0:N1}", endTOFGate) + " C: " + string.Format("{0:N1}", (endTOFGate + startTOFGate) / 2) + " L: " + string.Format("{0:N1}", endTOFGate - startTOFGate));
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
			endTOFGate = tof.GateStartTime + tof.ClockPeriod * tof.Length;
			UpdateTOFAveragePlots();
			UpdatePMTAveragePlots();
			window.SpectrumGate = new NationalInstruments.UI.Range(startSpectrumGate, endSpectrumGate);
			window.TOFGate = new NationalInstruments.UI.Range(startTOFGate, endTOFGate);
			if (spectrumFitMode == FitMode.Average) FitAndPlotSpectrumOffShots(averageScan);
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

		public double NormSigGateLow
		{
			get { return window.NormSigGate.Minimum; }
		}

		public double NormSigGateHigh
		{
			get { return window.NormSigGate.Maximum; }
		}

		public double NormBgGateLow
		{
			get { return window.NormBgGate.Minimum; }
		}

		public double NormBgGateHigh
		{
			get { return window.NormBgGate.Maximum; }
		}

		private void UpdateTOFAveragePlots()
		{
			Scan averageScan = Controller.GetController().DataStore.AverageScan;
			if (averageScan.Points.Count == 0) return;

			ArrayList currentTOFs = averageScan.GetGatedAverageOnShot(startSpectrumGate, endSpectrumGate).TOFs;
			window.PlotAverageOnTOF(currentTOFs);
			//if (currentTOFs.Count > 1)
			//{
			//	double normVal = (((TOF)currentTOFs[1]).Integrate(NormSigGateLow, NormSigGateHigh)) - (((TOF)currentTOFs[1]).Integrate(NormBgGateLow, NormBgGateHigh)) * (NormSigGateHigh - NormSigGateLow) / (NormBgGateHigh - NormBgGateLow);
			//	window.PlotAverageNormedOnTOF(((TOF)currentTOFs[0]) / normVal);
			//}
			Profile p = Controller.GetController().ProfileManager.CurrentProfile;
			if (p != null && (bool)p.AcquisitorConfig.switchPlugin.Settings["switchActive"])
			{
				currentTOFs = averageScan.GetGatedAverageOffShot(startSpectrumGate, endSpectrumGate).TOFs;
				window.PlotAverageOffTOF(currentTOFs);
				if (currentTOFs.Count > 1)
				{
					//double normVal = (((TOF)currentTOFs[1]).Integrate(NormSigGateLow, NormSigGateHigh)) - (((TOF)currentTOFs[1]).Integrate(NormBgGateLow, NormBgGateHigh)) * (NormSigGateHigh - NormSigGateLow) / (NormBgGateHigh - NormBgGateLow);
					//window.PlotAverageNormedOffTOF(((TOF)currentTOFs[0]) / normVal);
				}
			}
		}

		private void UpdatePMTAveragePlots()
		{
			Scan averageScan = Controller.GetController().DataStore.AverageScan;
			if (averageScan.Points.Count == 0) return;
			window.SpectrumAxes = new NationalInstruments.UI.Range(averageScan.MinimumScanParameter,
				 averageScan.MaximumScanParameter);
			window.PlotAveragePMTOn(
				averageScan.ScanParameterArray,
				//averageScan.GetTOFOnIntegralArray(0,startTOFGate, endTOFGate)//this is without background substraction
				averageScan.GetBackgroundSubstractedTOFOnIntegralArray(0, startTOFGate, endTOFGate, bgStartTime, bgEndTime)//this is using background substraction
				);
			Profile p = Controller.GetController().ProfileManager.CurrentProfile;
			if (p != null && (bool)p.AcquisitorConfig.switchPlugin.Settings["switchActive"])
			{
				window.PlotAveragePMTOff(averageScan.ScanParameterArray,
					//averageScan.GetTOFOffIntegralArray(0,startTOFGate, endTOFGate)// this is without background substraction
					averageScan.GetBackgroundSubstractedTOFOffIntegralArray(0, startTOFGate, endTOFGate, bgStartTime, bgEndTime)//this is using background substraction
					);
				window.PlotAverageDifference(averageScan.ScanParameterArray,
					//averageScan.GetDifferenceIntegralArray(0,startTOFGate, endTOFGate)// this is not bg substracted
					averageScan.GetBgSubtractedDifferenceIntegralArray(0, startTOFGate, endTOFGate, bgStartTime, bgEndTime)//this is using background substraction
					);
				window.PlotAverageRatio(averageScan.ScanParameterArray,
					//averageScan.GetDifferenceIntegralArray(0,startTOFGate, endTOFGate)// this is not bg substracted
					averageScan.GetBgSubtractedRatioIntegralArray(0, startTOFGate, endTOFGate, bgStartTime, bgEndTime)//this is using background substraction
					);

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
			window.ClearSpectrumFitOnShots();
			window.ClearSpectrumFitOffShots();
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
			spectrumFitterOffShots = (Fitter)item;
			spectrumFitterOnShots = (Fitter)item;
			window.ClearSpectrumFitOnShots();
			window.ClearSpectrumFitOffShots();
		}


		internal void UpdateTOFFit()
		{
			Scan averageScan = Controller.GetController().DataStore.AverageScan;
			if (averageScan.Points.Count != 0) FitAverageTOF();
		}

		internal void UpdateSpectrumFitOffShots()
		{
			Scan averageScan = Controller.GetController().DataStore.AverageScan;
			if (averageScan.Points.Count != 0) FitAndPlotSpectrumOffShots(averageScan);
		}
		internal void UpdateSpectrumFitOnShots()
		{
			Scan averageScan = Controller.GetController().DataStore.AverageScan;
			if (averageScan.Points.Count != 0) FitAndPlotSpectrumOnShots(averageScan);
		}
		internal void UpdateRatioFit()
		{
			Scan averageScan = Controller.GetController().DataStore.AverageScan;
			if (averageScan.Points.Count != 0) FitAndPlotRatio(averageScan);
		}

		internal void UpdateNoiseResults()
		{
			string tempPath = Environment.GetEnvironmentVariable("TEMP") + "\\ScanMasterTemp";
			int k = Controller.GetController().DataStore.NumberOfScans;
			Scan currentScan = Controller.GetController().serializer.DeserializeScanAsBinary(tempPath + "\\scan_" + k);
			if (currentScan.Points.Count != 0) AnalyseNoise(currentScan);
		}

		internal void SetGatesToDefault()
		{
			if ((double[])Environs.Hardware.GetInfo("defaultGate") != null)
			{
				double[] defaultGate = (double[])Environs.Hardware.GetInfo("defaultGate");
				window.TOFGate = new NationalInstruments.UI.Range(defaultGate[0] - defaultGate[1], defaultGate[0] + defaultGate[1]);
				TOFCursorMoved();
			}

		}

		internal void SetTOFAxesRangeToDefault()
		{
			if ((double[])Environs.Hardware.GetInfo("defaultTOFRange") != null)
			{
				double[] defaultRange = (double[])Environs.Hardware.GetInfo("defaultTOFRange");
				window.TOFAxes = new NationalInstruments.UI.Range(defaultRange[0], defaultRange[1]);
			}
			else
			{
				window.TOFAxes = null;
			}

			if ((double[])Environs.Hardware.GetInfo("defaultTOF2Range") != null)
			{
				double[] defaultRange = (double[])Environs.Hardware.GetInfo("defaultTOF2Range");
				window.TOF2Axes = new NationalInstruments.UI.Range(defaultRange[0], defaultRange[1]);
			}
			else
			{
				window.TOF2Axes = null;
			}
		}


	}
}
