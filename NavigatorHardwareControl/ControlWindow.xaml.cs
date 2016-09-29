using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Drawing;
using NationalInstruments.Controls;
using NationalInstruments.Controls.Primitives;
using System.Threading;
using DAQ.Environment;

namespace NavigatorHardwareControl
{
    /// <summary>
    /// Interaction logic for ControlWindow.xaml
    /// </summary>
    ///
    
    public partial class ControlWindow : Window
    {
       
        public Controller controller;
        public TextBoxStreamWriter console;
        private TextReader reader;
        //private FibreAligner fibreAlign;
        private Dictionary<string, TextBox> aoTextBoxes;
        private Dictionary<string, LED> doLEDs; 
        private bool isReading;
        public double intensityScale = 1.0;

        private static readonly GraphQueryArgs query = new GraphQueryArgs(
      PlotsToSearch.Any, SearchDimensions.HorizontalAndVertical,
      SearchDirections.ForwardAndReverse, isInclusive: true);

        public ControlWindow()
        {
            InitializeComponent();
            //I don't like initialising the controller here, but this seems to be the easiest way to deal with object references
            controller = App.controller;
            
            console = new TextBoxStreamWriter(consoleRichTextBox);
            //Sets the Console to stream to the consoleTextBox
            Console.SetOut(console);

            controller.Start();
            this.DataContext = controller.hardwareState;
            controller.controlWindow = this;
            
            //TODO fix this based on the UIData class
            //foreach (TextBox tb in FindVisualChildren<TextBox>(hardwareControl))
            //{
            //    aoTextBoxes[tb.Name] = tb;
            //}
            //foreach (LED led in FindVisualChildren<LED>(hardwareControl))
            //{
            //    doLEDs[led.Name] = led;
            //}

            //controller.Start();

            //Add an event to display the coordinates and intensity over the piezoMap
            piezoMap.PlotAreaMouseMove += this.OnPlotAreaMouseMove;
            piezoMap.PlotAreaMouseLeave += delegate { piezoMap.ToolTip = null; };
            ToolTipService.SetInitialShowDelay(piezoMap, 0);
            ToolTipService.SetShowDuration(piezoMap, int.MaxValue);

            Closing += OnWindowClosing; 
           
        }

        #region Accessing Digital and Analog values
        public double ReadAnalog(string name)
        {
            return 0.0;
            //return double.Parse(aoTextBoxes[name].Text);
        }
        public void SetAnalog(string channelName, double value)
        {
           aoTextBoxes[channelName].Text=Convert.ToString(value);
        }
        public bool ReadDigital(string channelName)
        {
            return doLEDs[channelName].Value;
        }
        public void SetDigital(string channelName, bool value)
        {
            doLEDs[channelName].Value=value;
        }

        #endregion

        #region Updating UI State
        public void UpdateUIState(Controller.NavHardwareState state)
        {
            switch (state)
            {
                case Controller.NavHardwareState.OFF:

                    remoteControlLED.Value = false;
                    hardwareControl.IsEnabled = true;

                    break;

                case Controller.NavHardwareState.LOCAL:

                    remoteControlLED.Value = false;
                    hardwareControl.IsEnabled=true;
                    break;

                case Controller.NavHardwareState.REMOTE:

                    remoteControlLED.Value = true;
                    hardwareControl.IsEnabled = false ;

                    break;
            }
        }

        public void WriteToConsole(string text)
        {
            console.WriteLine(text);
        }

        #endregion

        #region Accessor Methods
        public Controller.HardwareState NavHardware
        {
            get { return controller.hardwareState; }
            set { controller.hardwareState = value; }
        }
        public double[,] ScanData
        {
            get { return controller.fibreAlign.ScanData; }
        }
        public IEnumerable<double> FibrePowers
        {
            get { return controller.fibreAlign.fibrePowers; }
        }

        #endregion

        #region EventHandlers
        private void edfaLED_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as LED;
            string id = button.Name.Remove(5, 3);
            if (button.Value == true)
            {
                button.Value = false;
                controller.StopEDFA(id);
            }
            else
            {
                button.Value = true;
                controller.StartEDFA(id);
            }

            

        }
       
        private async void lockLaserButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as BooleanButton;
            button.Value = !button.Value;
            string laserID;
            string line;
            switch (button.Name)
            {
                case "lockMaster":
                    laserID = "master";
                    break;
                case "lockSlave0":
                    laserID = "slave0";
                    break;
                case "lockSlave1":
                    laserID = "slave1";
                    break;
                case "lockSlave2":
                    laserID = "slave2";
                    break;
                default:
                    laserID = "NoLaser";
                    break;
            }
            if (button.Value)
            {
                controller.LockLaser(laserID);
                WriteToConsole("Locked " + laserID);

            }
            else
            {
                controller.UnlockLaser(laserID);
                WriteToConsole("Unlocked " + laserID);
            }


        }

        private void piezoAlignButton_Click(object sender, RoutedEventArgs e)
        {
            double threshold = fibreThreshold.Value;
            if (piezoScan.IsChecked.Value || Environs.Debug)
            {
                //loads a test image and tries to maximaize that
                Console.WriteLine("Debug Enabled - Attempting to load previous scan data or create new scan");
                string fibrePath = controller.LoadFibreScanData();
                if (fibrePath != "")
                {
                    //
                }
                else
                {
                    Console.WriteLine("No Fibre Data Specified. Scanning Fibre...");
                    piezoMap.DataSource = controller.ScanFibre(100, 10000.0, 100);
                }
            }
            else
            {
                int[] coords = new int[2];
                coords = controller.AlignFibre(threshold, !piezoScan.IsChecked.Value);
                horizPiezo.Text = coords[0].ToString();
                vertPiezo.Text = coords[1].ToString();
                //Plots the scan data and the coupling efficiency
                piezoMap.DataSource = ScanData;
                iterationGraph.DataSource = FibrePowers;

            }
        }

        private void monitorButton_Click(object sender, RoutedEventArgs e)
        {
            console.WriteLine("This button can be used to monitor the error signal for the PID loop to lock this laser. This is not yet implemented");
        }

        private void edfaLockButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as BooleanButton;
            TextBox edfaText;
            LED edfaLED;
            Switch edfaLockType;
            string edfaID;

            switch (button.Name)
            {
                default:
                    edfaText = new TextBox();
                    edfaLED = new LED();
                    edfaLockType = new Switch();
                    edfaID = "None";
                    break;
                case "edfa0LockButton":
                    edfaText = edfa0Text;
                    edfaLED = edfa0LED;
                    edfaID = "edfa0";
                    edfaLockType = edfa0LockType;
                    break;
                case "edfa1LockButton":
                    edfaText = edfa1Text;
                    edfaLED = edfa1LED;
                    edfaID = "edfa1";
                    edfaLockType = edfa1LockType;
                    break;
                case "edfa2LockButton":
                    edfaText = edfa2Text;
                    edfaLED = edfa2LED;
                    edfaLockType = edfa2LockType;
                    edfaID = "edfa2";
                    break;
            }

            if (edfaText.Text == "")
            {
                MessageBox.Show("No value entered for PID. EDFA will not turn on without one.");
            }
            else
            {
                edfaLED.Value = !edfaLED.Value;
                string type = "";
                bool lockParam = edfaLockType.Value;
                double lockValue = Double.Parse(edfaText.Text);
                if (edfaLED.Value)
                    controller.EdfaLock(edfaID, lockParam, lockValue);
                if (lockParam)
                {
                    type = "Power";
                }
                else
                {
                    type = "Current";
                }
                console.WriteLine("Locking " + edfaID + ": " + type + " set to " + lockParam + "V");
            }
        }

        private void SaveParameters_Click(object sender, RoutedEventArgs e)
        {
            controller.SaveParametersWithDialog();
        }
        private void LoadParameters_Click(object sender, RoutedEventArgs e)
        {
            controller.LoadParametersWithDialog();
        }
        private void SaveDefault_Click(object sender, RoutedEventArgs e)
        {

        }

        private void consoleRichTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            //This is used to interrupt any text streams much like a console
            var textBox = sender as RichTextBox;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.C))
            {
                try
                {
                    isReading = false;
                    reader.Dispose();
                }
                catch (Exception s)
                {
                    console.WriteLine("Problem closing a text stream: " + s.Message);
                }
            }
        }

        private void OnPlotAreaMouseMove(object sender, MouseEventArgs e)
        {
            var graph = sender as IntensityGraph;
            IPlot plot = graph.AllPlots[0];
            Point screenPosition = graph.GetPlotAreaPosition(e);
            Point relativePosition = graph.ScreenToRelative(screenPosition);

            PlotValue nearestValue = graph.FindNearestValue(plot, relativePosition, query);
            if (nearestValue != null)
                graph.ToolTip = string.Format(
                    "Nearest intensity value is {2} at ({0},{1}).",
                    nearestValue.Value.Cast<object>().ToArray());
        }

       

        private void ddsupdateButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO Implement DDS update
            
            controller.UpdateDDS();
        }
        #endregion
        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            if (controller.hsdio != null)
                controller.hsdio.ReleaseHardware();

        }

        private void StopStream_Click(object sender, RoutedEventArgs e)
        {
            controller.CloseLaserThread();
        }

        private void cameraSnapshot_Click(object sender, RoutedEventArgs e)
        {
            bool background = BackgroundCheck.IsChecked.Value;
            controller.CameraSnapshot(background);
        }

        private void cameraSave_Click(object sender, RoutedEventArgs e)
        {
            bool background = BackgroundCheck.IsChecked.Value;
            controller.SaveImageWithDialog(background);
        }

        private void cameraStartStream_Click(object sender, RoutedEventArgs e)
        {
            controller.CameraStream();
        }

        private void cameraStopStream_Click(object sender, RoutedEventArgs e)
        {
            controller.StopCameraStream();
        }

        private void startImageAnalysis_Click(object sender, RoutedEventArgs e)
        {
            controller.OpenNewImageAnalysisWindow();
        }

        private void updateHardware_Click(object sender, RoutedEventArgs e)
        {
            controller.UpdateHardware();
        }

        private void startVoltageLogger_Click(object sender, RoutedEventArgs e)
        {
            controller.openNewVoltageLoggerWindow();

        }

    }

    #region Other ControlWindow classes
    public class TextBoxStreamWriter : TextWriter
    {
        RichTextBox output = null;
        /// <summary>
        /// This implements a TextWriter that can be used to stream text to a TextBox object. In particular, this is useful for streaming Console text to the textbox in the GUI. It also asynchronously writes so it will not block other code from executing.
        /// </summary>
        /// <param name="textBox"></param>
        public TextBoxStreamWriter(RichTextBox textBox)
        {
            output = textBox;
        }
        //public override void Write(char value)
        //{
        //    base.Write(value);
        //    output.AppendText(value.ToString());
        //}
       
        public override void WriteLine(string value)
        {
            base.WriteLine(value);
            output.AppendText(">>" + value.ToString()+"\n");
        }

        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }

    }
  
#endregion

}
