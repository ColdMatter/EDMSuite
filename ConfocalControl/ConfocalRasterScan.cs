using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using Microsoft.Win32;

using NationalInstruments.DAQmx;
using DAQ.Environment;

using Data;
using Data.Scans;
using System.Diagnostics;

namespace ConfocalControl
{
    ///<summary>
    /// Plug-in for confocal raster scans. No fancy DAQ pattern builder is necessary here. 
    /// Borrows heavily from Kyle's Microcavity Scanitor.
    /// </summary>
    /// 

    // Uses delegate multicasting to compose and invoke event manager methods in series 
    public delegate void DataEventHandler(Point3D[] ps);
    public delegate void ScanFinishedEventHandler();
    public delegate void LineFinishedEventHandler(Point[] ps);

    public class ConfocalRasterScan
    {
        #region Class members

        // Dependencies should refer to this instance only 
        private static ConfocalRasterScan controllerInstance;
        public static ConfocalRasterScan GetController()
        {
            if (controllerInstance == null)
            {
                controllerInstance = new ConfocalRasterScan();
            }
            return controllerInstance;
        }

        private PluginSettings _rasterScanSettings = new PluginSettings("confocalScan");
        public PluginSettings scanSettings
        {
            get { return _rasterScanSettings; }
            set { _rasterScanSettings = value; }
        }

        // Bound event managers to class
        public event DataEventHandler Data;
        public event LineFinishedEventHandler LineFinished;
        public event ScanFinishedEventHandler ScanFinished;
        public event DaqExceptionEventHandler DaqProblem;

        // Define RasterScan state
        enum RasterScanState { stopped, running, stopping };
        private RasterScanState backendState = RasterScanState.stopped;

        // Keeping track of data
        private List<double> dataX;
        private List<double> dataY;
        private List<double>[] dataInputs;
        private List<Point3D> dataOutputs;

        #endregion

        #region Init

        private void InitialiseSettings()
        {
            _rasterScanSettings.LoadSettings();
        }

        public ConfocalRasterScan() 
        {
            InitialiseSettings();
            dataX = new List<double>();
            dataY = new List<double>();
            dataInputs = new List<double>[1];
        }

        #endregion

        #region Methods

        public void StartScan()
        {
            try
            {
                dataInputs[0] = new List<double>();
                dataOutputs = new List<Point3D>();

                SingleCounterPlugin.GetController().AcquisitionStarting();
                ThreadStart acquirethreadStart = new ThreadStart(SingleCounterPlugin.GetController().PreArm);
                Thread acquireThread = new Thread(acquirethreadStart);
                acquireThread.IsBackground = true;
                acquireThread.Start();

                backendState = RasterScanState.running;

                Acquire();
            }
            catch (DaqException e)
            {
                if (DaqProblem != null) DaqProblem(e);
            }

        }

        public void StopScan()
        {
            backendState = RasterScanState.stopping;
        }

        private void Acquire()
            // Main method for looping over scan parameters, aquiring scan outputs and connecting to controller for display
        {
            GalvoPairPlugin.GetController().AcquisitionStarting();

            // Move to the start of the scan.
            GalvoPairPlugin.GetController().SetGalvoXSetpoint(
                         (double)scanSettings["GalvoXStart"]);

            GalvoPairPlugin.GetController().SetGalvoYSetpoint(
                         (double)scanSettings["GalvoYStart"]);

            // Loop for X axis
            for (double YNumber = 0;
                    YNumber < (double)scanSettings["GalvoYRes"];
                    YNumber++)
            {
                List<Point> linePnts = new List<Point>();
                // Reset Y axis for new line
                GalvoPairPlugin.GetController().SetGalvoYSetpoint(
                             (double)scanSettings["GalvoXStart"]);

                // Calculate new X galvo point from current scan point 
                double currentGalvoYpoint = (double)scanSettings["GalvoYStart"] + YNumber *
                        ((double)scanSettings["GalvoYEnd"] -
                        (double)scanSettings["GalvoYStart"]) /
                        (double)scanSettings["GalvoYRes"];

                dataY.Add(currentGalvoYpoint);

                // Move Y galvo to new scan point
                GalvoPairPlugin.GetController().SetGalvoYSetpoint(currentGalvoYpoint);

                // Loop for Y axis
                for (double XNumber = 0;
                        XNumber < (double)scanSettings["GalvoXRes"];
                        XNumber++)
                {
                    // Calculate new Y galvo point from current scan point 
                    double currentGalvoXpoint = (double)scanSettings["GalvoXStart"] + XNumber *
                    ((double)scanSettings["GalvoXEnd"] -
                    (double)scanSettings["GalvoXStart"]) /
                    (double)scanSettings["GalvoXRes"];

                    dataX.Add(currentGalvoXpoint);

                    // Move X galvo to new scan point 
                    GalvoPairPlugin.GetController().SetGalvoXSetpoint(currentGalvoXpoint);

                    // Take datapoint
                    int latestData = SingleCounterPlugin.GetController().ArmAndWait();
                    dataInputs[0].Add(latestData);

                    // Send up the data bundle
                    Point linePoint = new Point(XNumber + 1, latestData);
                    linePnts.Add(linePoint);

                    Point3D point = new Point3D(XNumber + 1, YNumber + 1, latestData);
                    dataOutputs.Add(point);
                    OnData(dataOutputs.ToArray());

                    // Check if scan exit.
                    if (CheckIfStopping())
                    {
                        // Quit plugins
                        AcquisitionFinishing();
                        return;
                    }
                }
                OnLineFinished(linePnts.ToArray());
            }

            OnScanFinished();
            AcquisitionFinishing();
        }

        private void AcquisitionFinishing()
        {
            SingleCounterPlugin.GetController().AcquisitionFinished();
            GalvoPairPlugin.GetController().AcquisitionFinished();
            dataX = new List<double>();
            dataY = new List<double>();
            dataInputs = new List<double>[1];
            backendState = RasterScanState.stopped;
        }

        private bool CheckIfStopping()
        {
            lock (this)
            {
                if (backendState == RasterScanState.stopping) return true;
                else return false;
            }
        }

        private void OnData(Point3D[] ps)
        {
            if (Data != null) Data(ps);
        }

        private void OnLineFinished(Point[] ps)
        {
            if (LineFinished != null) LineFinished(ps);
        }

        private void OnScanFinished()
        {
            if (ScanFinished != null) ScanFinished();
        }

        public void SaveData()
        {
            string directory = Environs.FileSystem.GetDataDirectory((string)Environs.FileSystem.Paths["scanMasterDataPath"]);
            string fileName = "data_try.txt";

            List<string> lines = new List<string>();
            lines.Add("X, Y, Z"); lines.Add("");

            foreach (Point3D pnt in dataOutputs)
            {
                string line = pnt.X.ToString() + ", " + pnt.Y.ToString() + ", " + pnt.Z.ToString();
                lines.Add(line);
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = directory;
            saveFileDialog.FileName = fileName;

            if (saveFileDialog.ShowDialog() == true)
            {
                System.IO.File.WriteAllLines(saveFileDialog.FileName, lines.ToArray());
            }
        }

        public void SaveDataAutomatic()
        {
            string directory = Environs.FileSystem.GetDataDirectory((string)Environs.FileSystem.Paths["scanMasterDataPath"]);
            string fileName = directory + "data_try.txt";

            List<string> lines = new List<string>();
            lines.Add("X, Y, Z"); lines.Add("");

            foreach (Point3D pnt in dataOutputs)
            {
                string line = pnt.X.ToString() + ", " + pnt.Y.ToString() + ", " + pnt.Z.ToString();
                lines.Add(line);
            }

            System.IO.File.WriteAllLines(fileName, lines.ToArray());
        }

        #endregion 
    }

    public class DataEventArgs : EventArgs
    {
        public ScanPoint point;
    }
}
