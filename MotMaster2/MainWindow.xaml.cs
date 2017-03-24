using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DAQ.Environment;

namespace MOTMaster2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Controller controller;
        public MainWindow()
        {
            InitializeComponent();
            InitVisuals();           
        }

        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                  new Action(delegate { }));
        }

        public void InitVisuals()
        {
            controller = new Controller();
            controller.StartApplication();
            btnRefresh_Click(null,null);
            tcMain.SelectedIndex = 0;
        }
        
        private string[] ParamsArray
        {
            get
            {
                string[] pa = { "param1", "param2", "param3" };
                if (controller.script != null)
                     pa= controller.script.Parameters.Keys.ToArray(); 
                return pa;               
            }
        }
        //TODO fun one cycle with defined parameters
        private bool SingleShot() // true if OK
        {

            controller.RunStart();
            return true;
        }

        private static string
            scriptCsPath = (string)Environs.FileSystem.Paths["scriptListPath"];
        private static string
            scriptPyPath = (string)Environs.FileSystem.Paths["scriptListPath"]; 

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".cs"; // Default file extension
            dlg.Filter = "Pattern script (.cs)|*.cs"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                string filename = dlg.FileName;

                cbPatternScript.Items.Insert(0, filename); cbPatternScript.SelectedIndex = 0;
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            cbPatternScript.Items.Clear();

            string[] css = Directory.GetFiles(scriptCsPath, "*.cs");
            foreach (string cs in css)
            {
                cbPatternScript.Items.Add(cs);
            }

            string[] pys = Directory.GetFiles(scriptPyPath, "*.py");
            foreach (string py in pys)
            {
                cbPatternScript.Items.Add(py);
            }

            cbPatternScript.SelectedIndex = 0;
        }
        // RUNNING THINGS

        private bool ScanFlag = false;
        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            if (btnRun.Content.Equals("Run"))
            {
                btnRun.Content = "Stop";
                btnRun.Background = Brushes.LightYellow;
                ScanFlag = true;
                // Start repeat
                int n = int.Parse(tbIterNumb.Text);
                if (n <= 0)
                {
                    MessageBox.Show("<Iteration Number> must be of positive value.");
                    return;
                }

                for (int i = 0; i < n; i++)
                {
                    // single shot
                    SingleShot();

                    DoEvents();
                    if (!ScanFlag) break;
                }
                btnRun.Content = "Run";
                btnRun.Background = Brushes.LightGreen;
                ScanFlag = false;
                return;
            }

            if (btnRun.Content.Equals("Stop"))
            {
                btnRun.Content = "Run";
                btnRun.Background = Brushes.LightGreen;
                ScanFlag = false;
                // End repeat
            }
        }

        private void btnScan_Click(object sender, RoutedEventArgs e)
        {
            if (btnScan.Content.Equals("Scan"))
            {
                btnScan.Content = "Cancel";
                btnScan.Background = Brushes.LightYellow;
                ScanFlag = true;
                //TODO check type of parameter for looping parameter
                if (true) // type of Parameter is int
                {
                   // integer param
                   int fromScanI = int.Parse(tbFromScan.Text);
                   int toScanI = int.Parse(tbToScan.Text);
                   if (fromScanI >= toScanI)
                   {
                       MessageBox.Show("<From> value must be smaller than <To> value.");
                       return;
                   }

                   int byScanI = int.Parse(tbByScan.Text);
                   if (byScanI <= 0)
                   {
                       MessageBox.Show("<Step> must be of positive value.");
                       return;
                   }

                   for (int i = fromScanI; i < toScanI + 1; )
                   {
                      // update scan param

                      // single shot
                      SingleShot();
                      
                      tbCurValue.Content = i.ToString();
                      i += byScanI;
                      DoEvents();
                      if (!ScanFlag) break;
                    }
                }
                else
                {
                    // double param
                    double fromScanD = double.Parse(tbFromScan.Text);
                    double toScanD = double.Parse(tbToScan.Text);
                    if (fromScanD >= toScanD)
                    {
                        MessageBox.Show("<From> value must be smaller than <To> value.");
                        return;
                    }

                    double byScanD = double.Parse(tbByScan.Text);
                    if (byScanD <= 0)
                    {
                        MessageBox.Show("<Step> must be of positive value.");
                        return;
                    }

                    for (double d = fromScanD; d < toScanD + 1; )
                    {
                       // update scan param

                       // single shot
                       SingleShot();

                       tbCurValue.Content = d.ToString();
                       d += byScanD;
                       DoEvents();  
                       if (!ScanFlag) break;
                    }
                }
                btnScan.Content = "Scan";
                btnScan.Background = Brushes.LightGreen;
                ScanFlag = false;
                return;
            }

            if (btnScan.Content.Equals("Cancel"))
            {
                btnScan.Content = "Scan";
                btnScan.Background = Brushes.LightGreen;
                ScanFlag = false;
            }
        }

        private void cbPatternScript_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Load the new script
            if (cbPatternScript.Text != "")
            {
                controller.script = controller.prepareScript((string)cbPatternScript.SelectedItem, null);
                controller.SetScriptPath((string)cbPatternScript.SelectedItem);
            }
            //Change parameters
            tcMain.SelectedIndex = 0;           
        }

        private bool paramCheck=false;
        private void tcMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (paramCheck)
                return;
            paramCheck = true;
            if (tcMain.SelectedIndex==1)
            {
                cbParamsScan.Items.Clear();
                foreach (string param in ParamsArray)
                    cbParamsScan.Items.Add(param);
                cbParamsScan.SelectedIndex = 0;
            }
            if (tcMain.SelectedIndex == 2)
            {
                cbParamsManual.Items.Clear();
                foreach (string param in ParamsArray)
                    cbParamsManual.Items.Add(param);
                cbParamsManual.SelectedIndex = 0;
            }
            paramCheck = false;
        }

    }
}
