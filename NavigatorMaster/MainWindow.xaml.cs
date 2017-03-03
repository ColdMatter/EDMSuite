using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NavigatorMaster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitVisuals();
        }
        
        public void InitVisuals()
        {
            cbParamsScan.Items.Clear(); 
            foreach (string pa in ParamsArray) 
            {
               cbParamsScan.Items.Add(pa);
            }
            cbParamsScan.SelectedIndex = 0;

            cbParamsManual.Items.Clear();
            foreach (string pa in ParamsArray)
            {
               cbParamsManual.Items.Add(pa);
            }
            cbParamsManual.SelectedIndex = 0;

            btnRefresh_Click(null,null);
        }
        //TODO Get Parameter Names from loaded script
        private string[] ParamsArray
        {
            get
            {
                string[] pa = { "param1", "param2", "param3" };
                return pa;
            }
        }
        //TODO fun one cycle with defined parameters
        private bool SingleShot() // true if OK
        {
            return true;
        }
        //TODO assign these based on the computer hardware
        private static string
            scriptCsPath = "e:\\VSprojects\\EDMSuite\\scanmaster\\";  //(string)Environs.FileSystem.Paths["scriptListPath"];
        private static string
            scriptPyPath = "e:\\VSprojects\\EDMSuite\\EDMScripts\\";  //(string)Environs.FileSystem.Paths["scriptListPath"]; 

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

            //string[] css = Directory.GetFiles(scriptCsPath, "*.cs");
            //foreach (string cs in css) 
            //{   cbPatternScript.Items.Add(cs);
            //}

            //string[] pys = Directory.GetFiles(scriptPyPath, "*.py");
            //foreach (string py in pys) 
            //{   cbPatternScript.Items.Add(py);
            //}

            //cbPatternScript.SelectedIndex = 0;
        }
        // RUNNING THINGS

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            if (btnRun.Content.Equals("Run"))
            {
                btnRun.Content = "Stop";
                btnRun.Background = Brushes.LightYellow;
                // Start repeat
                int n = int.Parse(tbIterNumb.Text);
                for (int i = 0; i < n; i++)
                {
                    // single shot
                    SingleShot();
                }
                    return;
            }

            if (btnRun.Content.Equals("Stop"))
            {
                btnRun.Content = "Run";
                btnRun.Background = Brushes.LightGreen;
                // End repeat
            }
        }

        private bool ScanFlag = false;
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
                   int byScanI = int.Parse(tbByScan.Text);
                   for (int i = fromScanI; i < toScanI + 1; )
                   {
                      // update scan param

                      // single shot
                      SingleShot();
                      
                      tbCurValue.Content = i.ToString();
                      i += byScanI;
                      if (!ScanFlag) break;
                    }
                }
                else
                {
                    // double param
                    double fromScanD = double.Parse(tbFromScan.Text);
                    double toScanD = double.Parse(tbToScan.Text);
                    double byScanD = double.Parse(tbByScan.Text);
                    for (double d = fromScanD; d < toScanD + 1; )
                    {
                       // update scan param

                       // single shot
                       SingleShot();

                       tbCurValue.Content = d.ToString();
                       d += byScanD;
                       if (!ScanFlag) break;
                    }
                }
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

    }
}
