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
using EDMConfig;

namespace EDMBlockHead
{
    public partial class LiveViewer : Form
    {
        private Controller controller;

        int blockCount = 1;
        double clusterVariance = 0;
        double clusterVarianceNormed = 0;
        double blocksPerDay = 240;

        private double initPumpPD = 1;
        private double initProbePD = 1;
 

        public LiveViewer(Controller c)
        {
            InitializeComponent();
            controller = c;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            UpdateStatusText("EDMErr\t" + "normedErr\t" + "B\t" + "DB\t" + "DB/SIG" + "\t" + Environment.NewLine);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        //public void Clear()
        //{
        //}

        public void AddDBlock(DemodulatedBlock dblock)
        {
            QuickEDMAnalysis analysis = QuickEDMAnalysis.AnalyseDBlock(dblock);

            //Append LiveViewer text with edm errors, B, DB & DB/SIG
            AppendStatusText(
                (Math.Pow(10, 26) * analysis.RawEDMErr).ToString("G3")
                + "\t" + (Math.Pow(10, 26) * analysis.RawEDMErrNormed).ToString("G3")
                + "\t\t" + (analysis.BValAndErr[0]).ToString("N2")
                + "\t" + (analysis.DBValAndErr[0]).ToString("N2")
                + "\t" + (analysis.DBValAndErr[0] / analysis.SIGValAndErr[0]).ToString("N3")
                + Environment.NewLine);

            // Rollings values of edm error
            clusterVariance = 
                ((clusterVariance * (blockCount - 1)) + analysis.RawEDMErr * analysis.RawEDMErr) / blockCount;
            double edmPerDay = Math.Sqrt(clusterVariance / blocksPerDay);
            clusterVarianceNormed =
                ((clusterVarianceNormed * (blockCount - 1)) 
                + analysis.RawEDMErrNormed * analysis.RawEDMErrNormed) / blockCount;
            double edmPerDayNormed = Math.Sqrt(clusterVarianceNormed / blocksPerDay);

            UpdateClusterStatusText(
                "errorPerDay: " + edmPerDay.ToString("E3") 
                + "\terrorPerDayNormed: " + edmPerDayNormed.ToString("E3")
                + Environment.NewLine + "block count: " + blockCount);

            //Update Plots
            AppendToSigScatter(new double[] { blockCount }, new double[] { analysis.SIGValAndErr[0] });
            AppendToSigNoiseScatter(new double[] { blockCount }, new double[] { analysis.SIGValAndErr[1] });
            AppendToBScatter(new double[] { blockCount }, new double[] { analysis.BValAndErr[0] });
            AppendToDBScatter(new double[] { blockCount }, new double[] { analysis.DBValAndErr[0] });
            AppendToEDMScatter(new double[] { blockCount }, 
                new double[] { Math.Pow(10, 26) * analysis.RawEDMErr });
            AppendToEDMNormedScatter(new double[] { blockCount },
                new double[] { Math.Pow(10, 26) * analysis.RawEDMErrNormed });
            AppendSigmaToSIGScatter(new double[] { blockCount },
                new double[] { analysis.SIGValAndErr[0] + analysis.SIGValAndErr[1] },
                new double[] { analysis.SIGValAndErr[0] - analysis.SIGValAndErr[1] });
            AppendSigmaToBScatter(new double[] { blockCount },
                new double[] { analysis.BValAndErr[0] + analysis.BValAndErr[1] },
                new double[] { analysis.BValAndErr[0] - analysis.BValAndErr[1] });
            AppendSigmaToDBScatter(new double[] { blockCount },
                new double[] { analysis.DBValAndErr[0] + analysis.DBValAndErr[1] },
                new double[] { analysis.DBValAndErr[0] - analysis.DBValAndErr[1] });
            AppendToNorthLeakageScatter(new double[] { blockCount },
                new double[] { analysis.NorthCurrentValAndError[0] });
            AppendToSouthLeakageScatter(new double[] { blockCount },
                new double[] { analysis.SouthCurrentValAndError[0] });
            AppendToNorthLeakageErrorScatter(new double[] { blockCount },
                new double[] { analysis.NorthCurrentValAndError[1] });
            AppendToSouthLeakageErrorScatter(new double[] { blockCount },
                new double[] { analysis.SouthCurrentValAndError[1] });
            AppendToNorthECorrLeakageScatter(new double[] { blockCount },
                new double[] { analysis.NorthECorrCurrentValAndError[0] });
            AppendToSouthECorrLeakageScatter(new double[] { blockCount },
                new double[] { analysis.SouthECorrCurrentValAndError[0] });
            AppendToMagNoiseScatter(new double[] { blockCount },
                new double[] { analysis.MagValandErr[1] });
            AppendToRfCurrentScatter(new double[] {blockCount },
                new double[] {analysis.rfCurrent[0]});
            AppendToLF1Scatter(new double[] { blockCount }, new double[] { analysis.LFValandErr[0] });
            AppendToLF1NoiseScatter(new double[] { blockCount }, new double[] { analysis.LFValandErr[1] });
            AppendToRF1AScatter(new double[] { blockCount }, new double[] { analysis.rf1AmpAndErr[0] });
            AppendToRF2AScatter(new double[] { blockCount }, new double[] { analysis.rf2AmpAndErr[0] });
            AppendToRF1FScatter(new double[] { blockCount }, new double[] { analysis.rf1FreqAndErr[0] });
            AppendToRF2FScatter(new double[] { blockCount }, new double[] { analysis.rf2FreqAndErr[0] });

            if (blockCount == 1)
            {
                initProbePD = analysis.probePD[0];
                initPumpPD = analysis.pumpPD[0];
            }           
            AppendTopProbePDScatter(new double[] { blockCount }, new double[] { analysis.probePD[0] / initProbePD });
            AppendTopPumpPDScatter(new double[] { blockCount }, new double[] { analysis.pumpPD[0] / initPumpPD });

            AppendToLF1DBDBScatter(new double[] { blockCount }, new double[] { analysis.LF1DBDB[0] });
            AppendToLF2DBDBScatter(new double[] { blockCount }, new double[] { analysis.LF2DBDB[0] });
            AppendSigmaToLF1Scatter(new double[] { blockCount },
                new double[] { analysis.LF1DBDB[0] + analysis.LF1DBDB[1] },
                new double[] { analysis.LF1DBDB[0] - analysis.LF1DBDB[1] });
            AppendSigmaToLF2Scatter(new double[] { blockCount },
                new double[] { analysis.LF2DBDB[0] + analysis.LF2DBDB[1] },
                new double[] { analysis.LF2DBDB[0] - analysis.LF2DBDB[1] });

            blockCount = blockCount + 1;
        }

        private void resetEdmErrRunningMeans()
        {
            blockCount = 1;
            clusterVariance = 0;
            clusterVarianceNormed = 0;
            UpdateClusterStatusText("errorPerDay: " + 0 + "\terrorPerDayNormed: " + 0
                + Environment.NewLine + "block count: " + 0);
            UpdateStatusText("EDMErr\t" + "normedErr\t" + "B\t" + "DB\t" + "DB/SIG" + "\t" + Environment.NewLine);
            ClearSIGScatter();
            ClearSigNoiseScatterGraph();
            ClearBScatter();
            ClearDBScatter();
            ClearEDMErrScatter();
            ClearLeakageScatters();
            ClearMagNoiseScatterGraph();
            ClearRfCurrentScatterGraph();
            ClearLF1Graph();
            ClearLF1NoiseGraph();
            ClearRFxAGraph();
            ClearRFxFGraph();
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

        private void AppendToSigNoiseScatter(double[] x, double[] y)
        {
            PlotXYAppend(sigNoiseScatterGraph, sigNoisePlot, x, y);
        }

        private void AppendToLF1Scatter(double[] x, double[] y)
        {
            PlotXYAppend(lf1ScatterGraph, lf1Plot, x, y);
        }

        private void AppendToLF1DBDBScatter(double[] x, double[] y)
        {
            PlotXYAppend(lfxdbdbScatterGraph, lf1dbdbScatterPlot, x, y);
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

        private void AppendToLF1NoiseScatter(double[] x, double[] y)
        {
            PlotXYAppend(lf1NoiseScatterGraph, lf1NoisePlot, x, y);
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

        private void AppendTopProbePDScatter(double[] x, double[] y)
        {
            PlotXYAppend(photoDiodeScatterGraph, probePDScatterPlot, x, y);
        }

        private void AppendTopPumpPDScatter(double[] x, double[] y)
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

        private void ClearSigNoiseScatterGraph()
        {
            ClearNIGraph(sigNoiseScatterGraph);
        }

        private void ClearLF1Graph()
        {
            ClearNIGraph(lf1ScatterGraph);
        }

        private void ClearLF1DBDBGraph()
        {
            ClearNIGraph(lfxdbdbScatterGraph);
        }

        private void ClearLF1NoiseGraph()
        {
            ClearNIGraph(lf1NoiseScatterGraph);
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