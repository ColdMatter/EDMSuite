using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IMAQ;

using NationalInstruments.Vision;
using NationalInstruments;
using NationalInstruments.UI;

namespace RFMOTHardwareControl
{
    public partial class ImageAnalysisWindow : Form
    {
        public CameraController controller;
        private VisionImage currentImage;
        private PixelValue1D verticalPixels;
        private PixelValue1D horizontalPixels;
        private LineContour verticalLine;
        private LineContour horizontalLine;
        private AnalogWaveform<byte> verticalPixelData;
        private AnalogWaveform<byte> horizontalPixelData;
        private AnalogWaveform<double> verticalBinsWaveform;
        private AnalogWaveform<double> horizontalBinsWaveform;
        private PixelValue2D roiArray;
        private int roiWidth;
        private int roiHeight;
        private waveformGraphCollection waveformGraphs;
        private double average;

        public ImageAnalysisWindow()
        {
            InitializeComponent();
            waveformGraphs = new waveformGraphCollection();
            waveformGraphs.addGraphToCollection("verticalLinePixelGraph", waveformGraph1);
            waveformGraphs.addGraphToCollection("horizontalLinePixelGraph", waveformGraph2);
            waveformGraphs.addGraphToCollection("verticalLinePixelHistogram", waveformGraph3);
            waveformGraphs.addGraphToCollection("horizontalLinePixelHistogram", waveformGraph4);

        }

        public void updateImage()
        {
            currentImage = controller.image;
            roiArray = currentImage.ImageToArray(controller.rectangleROI);
        }

        private void getPixelLines(PointContour poi)
        {
            PointContour verticalLineStartPoint = new PointContour((double)poi.X, (double)0);
            PointContour verticalLineEndPoint = new PointContour((double)poi.X, currentImage.Height);

            verticalLine = new LineContour(verticalLineStartPoint, verticalLineEndPoint);
            verticalPixels = currentImage.GetLinePixels(verticalLine);

            PointContour horizontalLineStartPoint = new PointContour((double)0, (double)poi.Y);
            PointContour horizontalLineEndPoint = new PointContour(currentImage.Width, (double)poi.Y);

            horizontalLine = new LineContour(horizontalLineStartPoint, horizontalLineEndPoint);
            horizontalPixels = currentImage.GetLinePixels(horizontalLine);

           // controller.ImageController.addLineContoursToROI(verticalLine,horizontalLine);
        }

        private void getROIHistogramAndAverage()
        {
            byte[,] roiData = roiArray.U8;
            roiHeight = roiData.GetLength(0);
            roiWidth = roiData.GetLength(1);
            double[] verticalBins = new double[roiHeight];
            double[] horizontalBins = new double[roiWidth];
            int noOfPixels = roiData.Length;
            int totPixelValue = 0;
            for (int w = 0; w < roiWidth; w++)
            {
                for (int h = 0; h < roiHeight; h++)
                {
                    int thisVal = roiData[h,w];
                    horizontalBins[w] += (double)thisVal/roiHeight;
                    verticalBins[h] += (double)thisVal/roiWidth;
                    totPixelValue += thisVal;
                }
            }
            average = (float)totPixelValue / noOfPixels;
            verticalBinsWaveform = AnalogWaveform<double>.FromArray1D(verticalBins);
            horizontalBinsWaveform = AnalogWaveform<double>.FromArray1D(horizontalBins);
        }


        private void setTextBox(TextBox box, string text)
        {
            box.Invoke(new setTextDelegate(setTextHelper), new object[] { box, text });
        }
        private delegate void setTextDelegate(TextBox box, string text);

        private void setTextHelper(TextBox box, string text)
        {
            box.Text = text;
        }

        private void getPixelLineData()
        {
            byte[] pixelByteData = verticalPixels.U8;
            verticalPixelData = AnalogWaveform<byte>.FromArray1D(pixelByteData);    
            pixelByteData = horizontalPixels.U8;
            horizontalPixelData = AnalogWaveform<byte>.FromArray1D(pixelByteData);
            
        }

        private void plotAllGraphs()
        {
            waveformGraph1.PlotWaveform(verticalPixelData);
            waveformGraph2.PlotWaveform(horizontalPixelData);
            waveformGraph3.PlotWaveform(verticalBinsWaveform);
            waveformGraph4.PlotWaveform(horizontalBinsWaveform);
        }

        public void updateImageAndAnalyse()
        {
            updateImage();
            getPixelLines(controller.pointOfInterest);
            getPixelLineData();
            getROIHistogramAndAverage();
            plotAllGraphs();
            setTextBox(average_pixelTextBox, average.ToString("F4")); 
        }
        #region Interacting with the graphs
        private void autoScaleToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (autoScaleToolStripMenuItem.Checked)
            {
                waveformGraphs.turnOnAllAutoScale();
            }
            else
            {
                waveformGraphs.turnOffAllAutoScale();
            }
        }
        #endregion
        private void ImageAnalysisWindow_Load(object sender, EventArgs e)
        {

        }

        private void ImageAnalysisWindow_FormClosing(object sender, EventArgs e)
        {
            //TODO correctly stop analysing once the window closes. 
            //controller.stopImageAnalysis();
        }

        private void waveformGraph1_PlotDataChanged(object sender, NationalInstruments.UI.XYPlotDataChangedEventArgs e)
        {

        }

    }
}
