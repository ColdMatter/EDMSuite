//using System;

//using NationalInstruments.Analysis.Math;

//using Data;
//using Data.Scans;
//using ScanMaster.GUI;
//using ScanMaster.Acquire.Plugin;

//namespace ScanMaster.GUI
//{
//    /// <summary>
//    /// Summary description for RollViewer.
//    /// </summary>
//    public class RollViewer : Viewer
//    {

//        private static int UPDATE_EVERY = 2;
//        private int shotCounter = 0;
//        private int gateStart;
//        private int gateLength;

//        private RollViewerWindow window;
//        public string dataSelection = "On";
//        bool visible = false;
//        Scan pointsToPlot = new Scan();

//        public String Name
//        {
//            get { return "Roll"; }
//        }

//        public RollViewer()
//        {
//            window = new RollViewerWindow(this);
//            window.dataSelectorCombo.SelectedIndex = 0;
//        }

//        public void Show()
//        {
//            window.Show();
//            visible = true;
//        }

//        public void Hide()
//        {
//            window.Hide();
//            visible = false;
//        }

//        public void ToggleVisible()
//        {
//            if (visible) Hide();
//            else Show();
//        }

//        public void AcquireStart()
//        {
//            PluginSettings shotSettings = Controller.GetController().ProfileManager.
//                CurrentProfile.AcquisitorConfig.shotGathererPlugin.Settings;
//            PluginSettings pgSettings = Controller.GetController().ProfileManager.CurrentProfile.AcquisitorConfig.pgPlugin.Settings;
//            pointsToPlot.Points.Clear();
//            gateStart = (int)shotSettings["gateStartTime"];
//            gateLength = (int)shotSettings["gateLength"];
//            shotCounter = 0;
//            window.ClearAll();
//        }

//        public void AcquireStop()
//        {
//            window.ClearAll();
//        }

//        public void updateDataSelection(string selection)
//        {
//            dataSelection = selection;
//        }

//        public void HandleDataPoint(ScanMaster.Acquire.DataEventArgs e)
//        {
//            pointsToPlot.Points.Add(e.point);
//            shotCounter++;
//            if (shotCounter % UPDATE_EVERY == 0)
//            {
//                double[] values = pointsToPlot.GetTOFOnIntegralArray(0, gateStart, gateStart + gateLength);
//                if (dataSelection == "Off" && (bool)Controller.GetController().ProfileManager.CurrentProfile.AcquisitorConfig.switchPlugin.Settings["switchActive"])
//                {
//                    values = pointsToPlot.GetTOFOffIntegralArray(0, gateStart, gateStart + gateLength);
//                }
//                double[] iodine;
//                if (pointsToPlot.AnalogChannelCount >= 1)
//                {
//                    iodine = pointsToPlot.GetAnalogArray(0);
//                    window.AppendToIodineGraph(iodine);
//                }

//                window.AppendToSignalGraph(values);

//                pointsToPlot.Points.Clear();
//            }
//        }

//        public void ScanFinished()
//        {
//        }

//        public void NewScanLoaded()
//        {
//        }

//    }
//}
