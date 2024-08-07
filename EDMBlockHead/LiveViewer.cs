using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;

using Analysis.EDM;
using Analysis;
using EDMConfig;

namespace EDMBlockHead
{
    public partial class LiveViewer : Form
    {
        private Controller controller;

        private const int kNumReplicates = 5000;
        private const double trimLevel = 0.05;
        private Random r = new Random();
        List<double> edms;

        int blockCount = 1;
        int blocksPerDay = 220;

        public LiveViewer(Controller c)
        {
            InitializeComponent();
            controller = c;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            UpdateStatusText("C\t SN\t EDM err \t {SIG}_A\t {SIG}_B\t {B}\t {RF1A}\t {RF2A}\t {RF1F}\t {RF2F}\t {LF1DBDB} \t Error" + Environment.NewLine);

            edms = new List<double>();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        //public void Clear()
        //{
        //}

        public void AddAnalysedDBlock(QuickEDMAnalysis analysis)
        {
            edms.Add(analysis.RawEDMValAndErr[0]);

            //Append LiveViewer text with edm errors, B, DB & DB/SIG
            AppendStatusText(
                (analysis.Contrast).ToString("N3")
                + "\t" + (analysis.ShotNoise).ToString("N4")
                + "\t" + (analysis.RawEDMValAndErr[1]*Math.Pow(10, 26) ).ToString("N4")
                //+ "\t" + (analysis.SIGValAndErrbp[0]).ToString("N1")
                //+ "\t" + (analysis.SIGValAndErrtp[0]).ToString("N1")
                + "\t" + (analysis.SIGValAndErrdA[0]).ToString("N1")
                + "\t" + (analysis.SIGValAndErrdB[0]).ToString("N1")
                + "\t" + (analysis.BValAndErr[0]).ToString("N4")
                //+ "\t" + (analysis.rf1AmpAndErr[0]).ToString("N4")
                //+ "\t" + (analysis.rf2AmpAndErr[0]).ToString("N4")
                //+ "\t" + (analysis.rf1FreqAndErr[0]).ToString("N4")
                //+ "\t" + (analysis.rf2FreqAndErr[0]).ToString("N4")
                //+ "\t" + (analysis.LF1DBDB[0]).ToString("N4")
                //+ "\t" + (analysis.LF1DBDB[1]).ToString("N4")
                + Environment.NewLine);

            // edm error
            double edmErrOverCluster = Statistics.BootstrappedTrimmedMeanAndError(edms.ToArray(), trimLevel, kNumReplicates)[1];
            double edmErrPerDay = edmErrOverCluster * Math.Sqrt((double)edms.Count / (double)blocksPerDay);

            UpdateClusterStatusText(
                "Error per day: " + edmErrPerDay.ToString("E3")
                + Environment.NewLine + "Block count: " + blockCount);

            //Update Plots
            AppendToSigScatter(new double[] { blockCount }, new double[] { analysis.SIGValAndErr[0] });
            AppendToBScatter(new double[] { blockCount }, new double[] { analysis.BValAndErr[0] });
            AppendToDBScatter(new double[] { blockCount }, new double[] { analysis.DBValAndErr[0] });
            AppendToEDMScatter(new double[] { blockCount }, 
                new double[] { Math.Pow(10, 26) * analysis.RawEDMValAndErr[1] });
            AppendSigmaToSIGScatter(new double[] { blockCount },
                new double[] { analysis.SIGValAndErr[0] + analysis.SIGValAndErr[1] },
                new double[] { analysis.SIGValAndErr[0] - analysis.SIGValAndErr[1] });
            AppendSigmaToBScatter(new double[] { blockCount },
                new double[] { analysis.BValAndErr[0] + analysis.BValAndErr[1] },
                new double[] { analysis.BValAndErr[0] - analysis.BValAndErr[1] });
            AppendSigmaToDBScatter(new double[] { blockCount },
                new double[] { analysis.DBValAndErr[0] + analysis.DBValAndErr[1] },
                new double[] { analysis.DBValAndErr[0] - analysis.DBValAndErr[1] });
            //AppendToNorthLeakageScatter(new double[] { blockCount },
            //    new double[] { analysis.NorthCurrentValAndError[0] });
            //AppendToSouthLeakageScatter(new double[] { blockCount },
            //    new double[] { analysis.SouthCurrentValAndError[0] });
            //AppendToNorthLeakageErrorScatter(new double[] { blockCount },
            //    new double[] { analysis.NorthCurrentValAndError[1] });
            //AppendToSouthLeakageErrorScatter(new double[] { blockCount },
            //    new double[] { analysis.SouthCurrentValAndError[1] });
            //AppendToNorthECorrLeakageScatter(new double[] { blockCount },
            //    new double[] { analysis.NorthECorrCurrentValAndError[0] });
            //AppendToSouthECorrLeakageScatter(new double[] { blockCount },
            //    new double[] { analysis.SouthECorrCurrentValAndError[0] });
            AppendToNorthLeakageScatter(new double[] { blockCount },
                new double[] { analysis.WestCurrentValAndError[0] });
            AppendToSouthLeakageScatter(new double[] { blockCount },
                new double[] { analysis.EastCurrentValAndError[0] });
            AppendToNorthLeakageErrorScatter(new double[] { blockCount },
                new double[] { analysis.WestCurrentValAndError[1] });
            AppendToSouthLeakageErrorScatter(new double[] { blockCount },
                new double[] { analysis.EastCurrentValAndError[1] });
            AppendToNorthECorrLeakageScatter(new double[] { blockCount },
                new double[] { analysis.WestECorrCurrentValAndError[0] });
            AppendToSouthECorrLeakageScatter(new double[] { blockCount },
                new double[] { analysis.EastECorrCurrentValAndError[0] });
            AppendToMagNoiseScatter(new double[] { blockCount },
                new double[] { analysis.MagValandErr[1] });
            //AppendToRF1AScatter(new double[] { blockCount }, new double[] { analysis.rf1AmpAndErr[0] });
            //AppendToRF2AScatter(new double[] { blockCount }, new double[] { analysis.rf2AmpAndErr[0] });
            //AppendToRF1FScatter(new double[] { blockCount }, new double[] { analysis.rf1FreqAndErr[0] });
            //AppendToRF2FScatter(new double[] { blockCount }, new double[] { analysis.rf2FreqAndErr[0] });
            //AppendToRF1ADBDBScatter(new double[] { blockCount }, new double[] { analysis.RF1ADBDB[0] });
            //AppendToRF2ADBDBScatter(new double[] { blockCount }, new double[] { analysis.RF2ADBDB[0] });
            //AppendToRF1FDBDBScatter(new double[] { blockCount }, new double[] { analysis.RF1FDBDB[0] });
            //AppendToRF2FDBDBScatter(new double[] { blockCount }, new double[] { analysis.RF2FDBDB[0] });
            //AppendToLF1Scatter(new double[] { blockCount }, new double[] { analysis.LF1ValAndErr[0] });
            //AppendToLF1DBDBScatter(new double[] { blockCount }, new double[] { analysis.LF1DBDB[0] });
            //AppendToTopPDScatter(new double[] { blockCount }, new double[] { analysis.TopPDSIG[0]});
            //AppendToBottomPDScatter(new double[] { blockCount }, new double[] { analysis.BottomPDSIG[0]});

            blockCount = blockCount + 1;
        }

        private void resetEdmErrRunningMeans()
        {
            blockCount = 1;
            edms.Clear();
            UpdateClusterStatusText(
                "Error per day: " + 0
                + Environment.NewLine + "Block count: " + 0);
            UpdateStatusText("C\t SN\t {SIG}_A\t {SIG}_B\t {B}\t {RF1A}\t {RF2A}\t {RF1F}\t {RF2F}\t {LF1}\t Error \t {LF1DB} \t Error \t {LF1DBDB} \t Error" + Environment.NewLine); ClearSIGScatter();
            ClearBScatter();
            ClearDBScatter();
            ClearEDMErrScatter();
            ClearLeakageScatters();
            ClearMagNoiseScatterGraph();
            ClearRfCurrentScatterGraph();
            ClearLF1Graph();
            ClearRFxAGraph();
            ClearRFxFGraph();
            ClearRFxFDBDBGraph();
            ClearRFxADBDBGraph();
            ClearPDScatter();
            ClearLF1DBDBGraph();
        }

        #region UI methods

        private void UpdateStatusText(string newText)
        {
            SetTextBox(statusText, newText);
        }

        private void AppendStatusText(string newText)
        {
            SetTextBox(statusText, statusText.Text + newText);
        }

        private void UpdateClusterStatusText(string newText)
        {
            SetTextBox(clusterStatusText, newText);
        }

        private void AppendToSigScatter(double[] x, double[] y)
        {
            PlotXYAppend(sigScatterGraph, sigPlot, x, y);
        }

        private void AppendToBScatter(double[] x, double[] y)
        {
            PlotXYAppend(bScatterGraph, bPlot, x, y);
        }

        private void AppendToBNormedScatter(double[] x, double[] y)
        {
            PlotXYAppend(bScatterGraph, bDBNLPlot, x, y);
        }

        private void AppendToDBScatter(double[] x, double[] y)
        {
            PlotXYAppend(dbScatterGraph, dbPlot, x, y);
        }

        private void AppendToNorthLeakageScatter(double[] x, double[] y)
        {
            PlotXYAppend(leakageGraph, northLeakagePlot, x, y);
        }

        private void AppendToSouthLeakageScatter(double[] x, double[] y)
        {
            PlotXYAppend(leakageGraph, southLeakagePlot, x, y);
        }

        private void AppendToNorthECorrLeakageScatter(double[] x, double[] y)
        {
            PlotXYAppend(eCorrLeakageScatterGraph, nECorrLeakagePlot, x, y);
        }

        private void AppendToSouthECorrLeakageScatter(double[] x, double[] y)
        {
            PlotXYAppend(eCorrLeakageScatterGraph, sECorrLeakagePlot, x, y);
        }
 
        private void AppendToEDMScatter(double[] x, double[] y)
        {
            PlotXYAppend(edmErrorScatterGraph, edmErrorPlot, x, y);
        }

        private void AppendToEDMNormedScatter(double[] x, double[] y)
        {
            PlotXYAppend(edmErrorScatterGraph, edmNormedErrorPlot, x, y);
        }

        private void AppendSigmaToSIGScatter(double[] x, double[] yPlusSigma, double[] yMinusSigma)
        {
            PlotXYAppend(sigScatterGraph, sigSigmaHi, x, yPlusSigma);
            PlotXYAppend(sigScatterGraph, sigSigmaLo, x, yMinusSigma);
        }

        private void AppendSigmaToBScatter(double[] x, double[] yPlusSigma, double[] yMinusSigma)
        {
            PlotXYAppend(bScatterGraph, bSigmaHi, x, yPlusSigma);
            PlotXYAppend(bScatterGraph, bSigmaLo, x, yMinusSigma);
        }

        private void AppendSigmaToDBScatter(double[] x, double[] yPlusSigma, double[] yMinusSigma)
        {
            PlotXYAppend(dbScatterGraph, dbSigmaHi, x, yPlusSigma);
            PlotXYAppend(dbScatterGraph, dbSigmaLo, x, yMinusSigma);
        }

        private void AppendToNorthLeakageErrorScatter(double[] x, double[] y)
        {
            PlotXYAppend(leakageErrorGraph, northLeakageVariancePlot, x, y);
        }

        private void AppendToSouthLeakageErrorScatter(double[] x, double[] y)
        {
            PlotXYAppend(leakageErrorGraph, southLeakageVariancePlot, x, y);
        }

        private void AppendToMagNoiseScatter(double[] x, double[] y)
        {
            PlotXYAppend(magNoiseGraph, magNoisePlot, x, y);
        }

        private void AppendToRfCurrentScatter(double[] x, double[] y)
        {
            PlotXYAppend(rfCurrentGraph, rfCurrentPlot, x, y);
        }

        private void AppendToLF1Scatter(double[] x, double[] y)
        {
            PlotXYAppend(lf1ScatterGraph, lf1Plot, x, y);
        }

        private void AppendToLF1DBDBScatter(double[] x, double[] y)
        {
            PlotXYAppend(lfxdbdbScatterGraph, lf1dbdbScatterPlot, x, y);
        }

        private void AppendToRF1FDBDBScatter(double[] x, double[] y)
        {
            PlotXYAppend(rfxfdbdbScatterGraph, rf1fdbdbScatterPlot, x, y);
        }

        private void AppendToRF2FDBDBScatter(double[] x, double[] y)
        {
            PlotXYAppend(rfxfdbdbScatterGraph, rf2fdbdbScatterPlot, x, y);
        }

        private void AppendToRF1ADBDBScatter(double[] x, double[] y)
        {
            PlotXYAppend(rfxadbdbScatterGraph, rf1adbdbScatterPlot, x, y);
        }

        private void AppendToRF2ADBDBScatter(double[] x, double[] y)
        {
            PlotXYAppend(rfxadbdbScatterGraph, rf2adbdbScatterPlot, x, y);
        }

        private void AppendToLF2DBDBScatter(double[] x, double[] y)
        {
            PlotXYAppend(lfxdbdbScatterGraph, lf2dbdbScatterPlot, x, y);
        }

        private void AppendSigmaToLF1Scatter(double[] x, double[] yPlusSigma, double[] yMinusSigma)
        {
            PlotXYAppend(lfxdbdbScatterGraph, lf1SigmaHi, x, yPlusSigma);
            PlotXYAppend(lfxdbdbScatterGraph, lf1SigmaLo, x, yMinusSigma);
        }

        private void AppendSigmaToLF2Scatter(double[] x, double[] yPlusSigma, double[] yMinusSigma)
        {
            PlotXYAppend(lfxdbdbScatterGraph, lf2SigmaHi, x, yPlusSigma);
            PlotXYAppend(lfxdbdbScatterGraph, lf2SigmaLo, x, yMinusSigma);
        }

        private void AppendToRF1AScatter(double[] x, double[] y)
        {
            PlotXYAppend(rfAmpScatterGraph, rf1aScatterPlot, x, y);
        }

        private void AppendToRF2AScatter(double[] x, double[] y)
        {
            PlotXYAppend(rfAmpScatterGraph, rf2aScatterPlot, x, y);
        }

        private void AppendToRF1FScatter(double[] x, double[] y)
        {
            PlotXYAppend(rfFreqScatterGraph, rf1fScatterPlot, x, y);
        }

        private void AppendToRF2FScatter(double[] x, double[] y)
        {
            PlotXYAppend(rfFreqScatterGraph, rf2fScatterPlot, x, y);
        }

        private void AppendToTopPDScatter(double[] x, double[] y)
        {
            PlotXYAppend(photoDiodeScatterGraph, probePDScatterPlot, x, y);
        }

        private void AppendToBottomPDScatter(double[] x, double[] y)
        {
            PlotXYAppend(photoDiodeScatterGraph, pumpPDScatterPlot, x, y);
        }

        private void ClearPDScatter()
        {
            ClearNIGraph(photoDiodeScatterGraph);
        }

        private void ClearSIGScatter()
        {
            ClearNIGraph(sigScatterGraph);
        }

        private void ClearBScatter()
        {
            ClearNIGraph(bScatterGraph);
        }

        private void ClearDBScatter()
        {
            ClearNIGraph(dbScatterGraph);
        }

        private void ClearEDMErrScatter()
        {
            ClearNIGraph(edmErrorScatterGraph);
        }

        private void ClearLeakageScatters()
        {
            ClearNIGraph(leakageGraph);
            ClearNIGraph(leakageErrorGraph);
            ClearNIGraph(eCorrLeakageScatterGraph);
        }

        private void ClearMagNoiseScatterGraph()
        {
            ClearNIGraph(magNoiseGraph);
        }

        private void ClearRfCurrentScatterGraph()
        {
            ClearNIGraph(rfCurrentGraph);
        }

        private void ClearLF1Graph()
        {
            ClearNIGraph(lf1ScatterGraph);
        }

        private void ClearLF1DBDBGraph()
        {
            ClearNIGraph(lfxdbdbScatterGraph);
        }

        private void ClearRFxFDBDBGraph()
        {
            ClearNIGraph(rfxfdbdbScatterGraph);
        }

        private void ClearRFxADBDBGraph()
        {
            ClearNIGraph(rfxadbdbScatterGraph);
        }

        private void ClearRFxAGraph()
        {
            ClearNIGraph(rfAmpScatterGraph);
        }

        private void ClearRFxFGraph()
        {
            ClearNIGraph(rfFreqScatterGraph);
        }

        #endregion


        #region Click Handler

        private void resetRunningMeans_Click(object sender, EventArgs e)
        {
            resetEdmErrRunningMeans();
        }

        #endregion

        #region Thread safe handlers

        private void SetTextBox(TextBox textBox, string text)
        {
            textBox.Invoke(new SetTextDelegate(SetTextHelper), new object[] { textBox, text });
        }

        private delegate void SetTextDelegate(TextBox textBox, string text);

        private void SetTextHelper(TextBox textBox, string text)
        {
            textBox.Text = text;
        }

        private delegate void PlotXYDelegate(double[] x, double[] y);

        private void PlotXYAppend(Graph graph, ScatterPlot plot, double[] x, double[] y)
        {
            graph.Invoke(new PlotXYDelegate(plot.PlotXYAppend), new Object[] { x, y });
        }

        private void PlotXY(Graph graph, ScatterPlot plot, double[] x, double[] y)
        {
            graph.Invoke(new PlotXYDelegate(plot.PlotXY), new Object[] { x, y });
        }

        private delegate void ClearDataDelegate();
        private void ClearNIGraph(Graph graph)
        {
            graph.Invoke(new ClearDataDelegate(graph.ClearData));
        }

        #endregion    
    }

}