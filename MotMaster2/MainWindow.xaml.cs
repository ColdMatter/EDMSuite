using DAQ.Environment;
using MOTMaster2.SequenceData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ErrorManager;
using UtilsNS;
using System.ComponentModel;


namespace MOTMaster2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Controller controller;
        private RemoteMessenger messenger;
        DispatcherTimer dispatcherTimer;

        //TODO change this so Controller can access it properly
        RemoteMessaging remoteMsg;

        public MainWindow()
        {
            controller = new Controller();
            controller.StartApplication();
            InitializeComponent();
            InitVisuals();
            ErrorMgr.Initialize(ref lbStatus, ref tbLogger, (string)Environs.FileSystem.Paths["configPath"]);

            dispatcherTimer = new DispatcherTimer(DispatcherPriority.Send);
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);

            this.sequenceControl.ChangedAnalogChannelCell += new SequenceDataGrid.ChangedAnalogChannelCellHandler(this.sequenceData_AnalogValuesChanged);
            this.sequenceControl.ChangedRS232Cell += new SequenceDataGrid.ChangedRS232CellHandler(this.sequenceData_RS232Changed);
            controller.MotMasterDataEvent += OnDataCreated;

            //   ((INotifyPropertyChanged)Controller.sequenceData.Parameters).PropertyChanged += this.InterferometerParams_Changed;
        }

        private void OnDataCreated(object sender, DataEventArgs e)
        {
            if (sender is Controller)
            {
                string data = (string)e.Data;
                remoteMsg.sendCommand(data);
                if (messenger != null) messenger.Send(data.Replace("\r\n",String.Empty)+"\n");
            }
        }

        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                  new Action(delegate { }));
                controller.WaitForRunToFinish();
        }

        public void InitVisuals()
        {
            btnRefresh_Click(null, null);
            tcMain.SelectedIndex = 0;
            SetInterferometerParams(Controller.sequenceData.Parameters);
        }

        private string[] ParamsArray
        {
            get
            {
                string[] pa = { "param1", "param2", "param3" };
                if (Controller.sequenceData != null)
                    pa = Controller.sequenceData.Parameters.Keys.ToArray();
                return pa;
            }
        }

        //TODO Rename to reflect loop runs
        private bool SingleShot(Dictionary<string, object> paramDict, bool loop = false) // true if OK
        {
            //Would like to use RunStart as this Runs in a new thread
            if (controller.IsRunning())
            {
                controller.WaitForRunToFinish();
            }

            controller.RunStart(paramDict, loop);
            return true;
        }
        private bool SingleShot(bool loop = false) // true if OK
        {
            return SingleShot(null, loop);
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
            bool? result = dlg.ShowDialog();

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
            cbPatternScript.Items.Add("- - -");

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

        #region RUNNING THINGS

        private void realRun(int Iters, string Hub = "none", int cmdId = -1)
        {
            if ((Iters == 0) || (Iters < -1))
            {
                ErrorMgr.errorMsg("Invalid <Iteration Number> value.", 2, true);
                if (!btnRun.Content.Equals("Run")) btnRun_Click(null, null);
                return;
            }
            progBar.Minimum = 0;
            progBar.Maximum = Iters - 1;
            int numInterations = Iters;
            if (Iters == -1)
            {
                numInterations = Int32.MaxValue;
                progBar.Maximum = 100;
            }
            Controller.ExpData.ClearData();
            controller.numInterations = numInterations;
            Controller.ExpData.ExperimentName = tbExperimentRun.Text;
            Controller.StaticSequence = true;
            if ((Controller.ExpData.ExperimentName.Equals("---") || String.IsNullOrEmpty(Controller.ExpData.ExperimentName)))
            {
                Controller.ExpData.ExperimentName = DateTime.Now.ToString("yy-MM-dd_H-mm-ss");
                tbExperimentRun.Text = Controller.ExpData.ExperimentName;
            }
            controller.StartLogging();

            for (int i = 0; i < numInterations; i++)
            {
                controller.SetBatchNumber(i);
                ScanFlag = SingleShot(true);
                if (Iters == -1) progBar.Value = i % 100;
                else progBar.Value = i;
                lbCurNumb.Content = i.ToString();
                DoEvents();
                if (!ScanFlag) break;
            }
            controller.StopLogging();
            Controller.StaticSequence = false;
            if (!btnRun.Content.Equals("Run")) btnRun_Click(null, null);
        }


        private bool ScanFlag = false;
        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            if (btnRun.Content.Equals("Run"))
            {
                Controller.ExpData.SaveRawData = true;
                btnRun.Content = "Stop";
                btnRun.Background = Brushes.LightYellow;
                ScanFlag = true;
                Controller.ExpData.grpMME.Clear();
                int Iters = (int)ntbIterNumb.Value;
                // Start repeat
                try
                {
                    realRun(Iters);
                }
                catch (Exception ex)
                {
                    ErrorMgr.errorMsg(ex.Message, -5);
                    btnRun_Click(null, null);
                    return;
                }
                return;
            }

            if (btnRun.Content.Equals("Stop"))
            {
                tbExperimentRun.Text = "---";
                btnRun.Content = "Run";
                btnRun.Background = Brushes.LightGreen;
                ScanFlag = false;
                controller.StopRunning();
                lbCurNumb.Content = "";
                // End repeat
            }

            if (btnRun.Content.Equals("Abort Remote"))
            {
                tbExperimentRun.Text = "---";
                btnRun.Content = "Run";
                btnRun.Background = Brushes.LightGreen;
                ScanFlag = false;
                //Send Remote Message to AxelHub
                controller.StopRunning();
                lbCurNumb.Content = "";
                if (!Utils.isNull(sender))
                {
                    MMexec mme = new MMexec("Axel-hub");
                    remoteMsg.sendCommand(mme.Abort("MOTMaster"));
                }
            }
        }

        private void realScan(string prm, string fromScanS, string toScanS, string byScanS, string Hub = "none", int cmdId = -1)
        {
            string parameter = prm;
            if (!Controller.sequenceData.Parameters.ContainsKey(prm)) { ErrorMgr.errorMsg(string.Format("Parameter {0} not found in sequence", prm), 100, true); return; }
            Parameter param = Controller.sequenceData.Parameters[prm];
            //Sets the sequence to static if we know the scan parameter does not modify the sequence
            Controller.StaticSequence = !param.SequenceVariable;
            Dictionary<string, object> scanDict = new Dictionary<string, object>();
            Controller.ExpData.ClearData();
            Controller.ExpData.SaveRawData = true;
            Controller.ExpData.ExperimentName = tbExperimentRun.Text;
            if (Controller.ExpData.ExperimentName.Equals("---") || String.IsNullOrEmpty(Controller.ExpData.ExperimentName))
            {
                Controller.ExpData.ExperimentName = DateTime.Now.ToString("yy-MM-dd_H-mm-ss");
                tbExperimentRun.Text = Controller.ExpData.ExperimentName;
            }
            controller.StartLogging();
            scanDict[parameter] = param.Value;
            object defaultValue = param.Value;
            MMscan scanParam = new MMscan();
            scanParam.sParam = prm;
            scanParam.groupID = Controller.ExpData.ExperimentName;

            int scanLength;
            object[] scanArray;
            if (defaultValue is int && Controller.sequenceData.Parameters.ContainsKey(prm))
            {
                int fromScanI = int.Parse(fromScanS);
                int toScanI = int.Parse(toScanS);
                int byScanI = int.Parse(byScanS);
                scanParam.sFrom = fromScanI;
                scanParam.sTo = toScanI;
                scanParam.sBy = byScanI;
                progBar.Minimum = fromScanI;
                progBar.Maximum = toScanI;
                scanLength = (toScanI - fromScanI) / byScanI + 1;
                if (scanLength < 0)
                {
                    ErrorMgr.errorMsg("Incorrect looping parameters. <From> value must be smaller than <To> value if it increases per shot.",3,true);
                    return;
                }
                scanArray = new object[scanLength];
                for (int i = 0; i < scanLength; i++)
                {
                    scanArray[i] = fromScanI;
                    fromScanI += byScanI;
                }
            }
            else
            {
                double fromScanD = double.Parse(fromScanS);
                double toScanD = double.Parse(toScanS);
                double byScanD = double.Parse(byScanS);
                scanParam.sFrom = fromScanD;
                scanParam.sTo = toScanD;
                scanParam.sBy = byScanD;
                progBar.Minimum = fromScanD;
                progBar.Maximum = toScanD;
                scanLength = (int)((toScanD - fromScanD) / byScanD) + 1;
                if (scanLength < 0)
                {
                    ErrorMgr.errorMsg("Incorrect looping parameters. <From> value must be smaller than <To> value if it increases per shot.",3,true);
                    return;
                }
                scanArray = new object[scanLength];
           
                for (int i = 0; i < scanLength; i++)
                {
                    scanArray[i] = fromScanD;
                    fromScanD += byScanD;
                }
            }
            int c = 0;
            controller.ScanParam = scanParam;
            foreach (object scanItem in scanArray)
            {
                controller.SetBatchNumber(c);
                param.Value = scanItem;
                scanDict[parameter] = scanItem;
                progBar.Value = (scanItem != null && scanItem is double) ? (double)scanItem : Convert.ToDouble((int)scanItem);
                SetInterferometerParams(scanDict);
                try
                {
                    ScanFlag = SingleShot(scanDict);
                }
                catch (Exception e)
                {
                    ErrorMgr.errorMsg("Error running scan: " + e.Message, -2);
                    break;
                }
                lbCurValue.Content = scanItem.ToString();
                DoEvents();
                if (!ScanFlag) break;
                c++;
            }
            if (!btnScan.Content.Equals("Scan")) btnScan_Click(null, null);
            param.Value = defaultValue;
            lbCurValue.Content = defaultValue.ToString();
            controller.StopLogging();
        }



        private void btnScan_Click(object sender, RoutedEventArgs e)
        {
            var converter = new System.Windows.Media.BrushConverter();
            var brush = (Brush)converter.ConvertFromString("#FFF9E76B");
            if (btnScan.Content.Equals("Scan"))
            {
                btnScan.Content = "Cancel";
                btnScan.Background = Brushes.LightYellow;
                ScanFlag = true;
                Controller.ExpData.grpMME.Clear();
                try
                {
                    realScan(cbParamsScan.Text, tbFromScan.Text, tbToScan.Text, tbByScan.Text);
                }
                catch (Exception ex)
                {
                    ErrorMgr.errorMsg(ex.Message, -5);
                    btnScan_Click(null, null);
                    return;
                }
                controller.StopRunning();
                btnScan.Content = "Scan";
                btnScan.Background = brush;
                ScanFlag = false;
                return;
            }

            if (btnScan.Content.Equals("Cancel"))
            {
                tbExperimentRun.Text = "---";
                btnScan.Content = "Scan";
                btnScan.Background = brush;
                ScanFlag = false;
                controller.StopRunning();
            }

            if (btnScan.Content.Equals("Abort Remote"))
            {
                tbExperimentRun.Text = "---";
                btnScan.Content = "Scan";
                btnScan.Background = brush;
                ScanFlag = false;
                controller.StopRunning();
                //Send Remote Message to AxelHub
                if (!Utils.isNull(sender))
                {
                    MMexec mme = new MMexec("Axel-hub");
                    remoteMsg.sendCommand(mme.Abort("MOTMaster"));
                }
            }
        }
        #endregion
        private void cbPatternScript_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Load the new script
            if ((!String.IsNullOrEmpty(cbPatternScript.Text)) && (!cbPatternScript.Text.Equals("- - -")))
            {
                controller.script = controller.prepareScript((string)cbPatternScript.SelectedItem, null);
                controller.SetScriptPath((string)cbPatternScript.SelectedItem);
            }
            //Change parameters
            tcMain.SelectedIndex = 0;
        }

        private bool paramCheck;
        private void tcMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(TabControl))
            {
                if (paramCheck)
                    return;
                paramCheck = true;
                if (tcMain.SelectedIndex == 1)
                {
                    int selIdx = cbParamsScan.SelectedIndex;
                    if (selIdx == -1) selIdx = 0;
                    cbParamsScan.Items.Clear();
                    foreach (string param in ParamsArray)
                        cbParamsScan.Items.Add(param);
                    cbParamsScan.SelectedIndex = selIdx;
                }
                if (tcMain.SelectedIndex == 2)
                {
                    int selIdx = cbParamsManual.SelectedIndex;
                    if (selIdx == -1) selIdx = 0;
                    cbParamsManual.Items.Clear();
                    foreach (string param in ParamsArray)
                        cbParamsManual.Items.Add(param);
                    //cbParamsManual.Text = ParamsArray[0];
                    cbParamsManual.SelectedIndex = selIdx;

                }
                paramCheck = false;
            }
        }

        private void LoadParameters_Click(object sender, RoutedEventArgs e)
        {
            if (controller.script != null)
            { // Configure open file dialog box
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.FileName = ""; // Default file name
                dlg.DefaultExt = ".csv"; // Default file extension
                dlg.Filter = "Parameters (.csv)|*.csv,*.txt"; // Filter files by extension

                // Show open file dialog box
                Nullable<bool> result = dlg.ShowDialog();

                // Process open file dialog box results
                if (result == true)
                {
                    string filename = dlg.FileName;

                    Dictionary<String, Object> LoadedParameters = new Dictionary<string, object>();
                    string json = File.ReadAllText(filename);
                    LoadedParameters = (Dictionary<String, Object>)JsonConvert.DeserializeObject(json, typeof(Dictionary<String, Object>));
                    if (controller.script != null)
                        foreach (string key in LoadedParameters.Keys)
                            controller.script.Parameters[key] = LoadedParameters[key];
                    else
                        ErrorMgr.warningMsg("You have tried to load parameters without loading a script");
                }
            }
        }


        private void SaveParameters_Click(object sender, RoutedEventArgs e)
        {
            if (controller.script != null)
            { // Configure open file dialog box
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = ""; // Default file name
                dlg.DefaultExt = ".csv"; // Default file extension
                dlg.Filter = "Parameters (.csv)|*.csv,*.txt"; // Filter files by extension

                // Show open file dialog box
                bool? result = dlg.ShowDialog();

                // Process open file dialog box results
                if (result != true) return;
                string filename = dlg.FileName;
                string json = JsonConvert.SerializeObject(controller.script.Parameters, Formatting.Indented);
                File.WriteAllText(filename, json);
            }
            else
                ErrorMgr.warningMsg("You have tried to save parmaters before loading a script");
        }

        private void SaveSequence_Click(object sender, RoutedEventArgs e)
        {
            if (Controller.sequenceData != null)
            { // Configure open file dialog box
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = ""; // Default file name
                dlg.DefaultExt = ".sm2"; // Default file extension
                dlg.Filter = "Sequence (.sm2)|*.sm2,*.txt"; // Filter files by extension
                //dlg.InitialDirectory = Controller.scriptListPath;

                // Show open file dialog box
                bool? result = dlg.ShowDialog();

                // Process open file dialog box results
                if (result != true) return;
                string filename = dlg.FileName;
                controller.SaveSequenceToPath(filename);
            }
            else
                ErrorMgr.warningMsg("You have tried to save a Sequence before loading a script", -1, true);

        }
        private void LoadSequence_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".sm2"; // Default file extension
            dlg.Filter = "Sequence (.sm2)|*.sm2"; // Filter files by extension

            // Show open file dialog box
            bool? result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result != true) return;
            string filename = dlg.FileName;
            controller.LoadSequenceFromPath(filename);
        }

        private void LoadCicero_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Title = "Select Cicero Settings File";
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".set"; // Default file extension
            dlg.Filter = "Cicero Settings (.set,.json)|*.json;*.set"; // Filter files by extension

            // Show open file dialog box
            bool? result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result != true) return;
            string filename = dlg.FileName;
            controller.LoadCiceroSettingsFromPath(filename);

            dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select Cicero Sequence File",
                FileName = "",
                DefaultExt = ".seq",
                Filter = "Cicero Sequence (.seq,.json)|*.json;*.seq"
            };
            // Show open file dialog box
            result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result != true) return;
            filename = dlg.FileName;
            controller.LoadCiceroSequenceFromPath(filename);

            controller.ConvertCiceroSequence();
            Log("Loaded Cicero Sequence from " + filename);
            UpdateSequenceControl();
        }

        private void UpdateSequenceControl()
        {
            //Simplest way is to recreate the ViewModel. This should be called when a new sequence is loaded.
            sequenceControl.UpdateSequenceData();
        }
        private void SaveEnvironment_Click(object sender, RoutedEventArgs e)
        {
            controller.SaveEnvironment();
        }
        private void LoadEnvironment_Click(object sender, RoutedEventArgs e)
        {
            controller.LoadEnvironment();
        }
        private void EditHardware_Click(object sender, RoutedEventArgs e)
        {
            HardwareOptions optionsWindow = new HardwareOptions();
            optionsWindow.Show();
        }
        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("MOTMaster v1.2\n by Jimmy Stammers and Teodor Krastev\n for Imperial College, London, UK");
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string parameter = cbParamsManual.Text;
            Parameter param = Controller.sequenceData.Parameters[parameter];
            if (param.Value is int)
            {
                param.Value = int.Parse(tbValue.Text);
            }
            else if (param.Value is double)
            {
                param.Value = double.Parse(tbValue.Text);
            }
        }

        private void cbParamsManual_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(ComboBox) && controller.script != null)
                tbValue.Text = Controller.sequenceData.Parameters[cbParamsManual.SelectedItem.ToString()].Value.ToString();
        }

        private void cbParamsScan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(ComboBox) && cbParamsScan.SelectedItem != null)
                lbCurValue.Content = Controller.sequenceData.Parameters[cbParamsScan.SelectedItem.ToString()].Value.ToString();
        }

        //Creates a table of values for the selected analog parameters
        private void CreateAnalogPropertyTable(SequenceStep selectedStep, string channelName, AnalogChannelSelector analogType)
        {
            // SequenceData.sequenceDataGrid.IsReadOnly = true;
            setPropertyBtn.Visibility = System.Windows.Visibility.Visible;
            tcLog.SelectedIndex = 1;
            //if (noPropLabel.Visibility == System.Windows.Visibility.Visible) { noPropLabel.Visibility = Visibility.Hidden; propertyGrid.Visibility = System.Windows.Visibility.Visible; }
            propLabel.Content = string.Format("{0}: {1} with {2}", selectedStep.Name, channelName, analogType.ToString());
            List<AnalogArgItem> data = selectedStep.GetAnalogData(channelName, analogType);
            propertyGrid.DataContext = data;
        }

        //TODO fix this so that it correctly creates a new table if the serial data is null
        //Creates a table of values for the selected analog parameters
        private void CreateSerialPropertyTable(SequenceStep selectedStep)
        {
            // SequenceData.sequenceDataGrid.IsReadOnly = true;
            setPropertyBtn.Visibility = System.Windows.Visibility.Visible;
            tcLog.SelectedIndex = 1;
            //if (noPropLabel.Visibility == System.Windows.Visibility.Visible) { noPropLabel.Visibility = Visibility.Hidden; propertyGrid.Visibility = System.Windows.Visibility.Visible; }
            propLabel.Content = string.Format("Edit Serial Commands for {0}", selectedStep.Name);
            List<SerialItem> data = selectedStep.GetSerialData();
            propertyGrid.DataContext = data;
            ToolTip tool = new ToolTip();
            tool.Content = "Enter commands separated by a space or comma. Frequencies in MHz, time in ms";
            propertyGrid.ToolTip = tool;
        }

        private void sequenceData_AnalogValuesChanged(object sender, SelectionChangedEventArgs e)
        {
            SequenceStepViewModel model = (SequenceStepViewModel)sequenceControl.sequenceDataGrid.DataContext;
            KeyValuePair<string, AnalogChannelSelector> analogChannel = model.SelectedAnalogChannel;
            SequenceStep step = model.SelectedSequenceStep;
            CreateAnalogPropertyTable(step, analogChannel.Key, analogChannel.Value);
        }
        private void sequenceData_RS232Changed(object sender, DataGridBeginningEditEventArgs e)
        {
            SequenceStepViewModel model = (SequenceStepViewModel)sequenceControl.sequenceDataGrid.DataContext;
            SequenceStep step = model.SelectedSequenceStep;
            CreateSerialPropertyTable(step);
        }
        private void Log(string txt, Color? clr = null)
        {
            if (!chkLog.IsChecked.Value) return;
            string printOut;
            if ((chkVerbatim.IsChecked.Value) || (txt.Length < 81)) printOut = txt;
            else printOut = txt.Substring(0, 80) + "...";

            Color ForeColor = clr.GetValueOrDefault(Brushes.Black.Color);
            TextRange rangeOfText1 = new TextRange(tbLogger.Document.ContentEnd, tbLogger.Document.ContentEnd);
            rangeOfText1.Text = printOut + "\r";
            rangeOfText1.ApplyPropertyValue(TextElement.ForegroundProperty, new System.Windows.Media.SolidColorBrush(ForeColor));
            tbLogger.ScrollToEnd();
        }

        private void setProperty_Click(object sender, RoutedEventArgs e)
        {
            SequenceParser sqnParser = new SequenceParser();
            bool verified = false;
            //Checks the validity of all the values, but does not assign them until the sequence is built
            //TODO: Add a type check to make this work for AnalogItems or SerialItems
            if (propertyGrid.DataContext == null) return;
            if (propertyGrid.DataContext.GetType() == typeof(List<AnalogArgItem>)) verified = ParseAnalogItems(sqnParser);
            else if (propertyGrid.DataContext.GetType() == typeof(List<SerialItem>)) verified = ParseSerialItems(sqnParser);
            if (verified)
            {
                SequenceStepViewModel model = (SequenceStepViewModel)sequenceControl.sequenceDataGrid.DataContext;
                object newArgs = propertyGrid.ItemsSource;
                model.UpdateChannelValues(newArgs);
                sequenceControl.sequenceDataGrid.IsReadOnly = false;
            }
        }

        private bool ParseSerialItems(SequenceParser sqnParser)
        {
            foreach (SerialItem item in (List<SerialItem>)propertyGrid.DataContext)
            {
                string value = item.Value;
                try
                {
                    if (SequenceParser.CheckMuquans(value)) continue;
                    else ErrorMgr.errorMsg(string.Format("Incorrect format for {0} serial command", item.Name), 4);
                }
                catch (Exception e)
                {
                    ErrorMgr.errorMsg("Couldn't parse serial commands. " + e.Message, 4, false);
                    return false;
                }

            }
            return true;
        }
        private bool ParseAnalogItems(SequenceParser sqnParser)
        {
            //TODO Fix this to ignore no start time value
            foreach (AnalogArgItem analogItem in (List<AnalogArgItem>)propertyGrid.DataContext)
            {
                double analogRawValue;
                if (Double.TryParse(analogItem.Value, out analogRawValue)) continue;
                if (Controller.sequenceData != null && Controller.sequenceData.Parameters.ContainsKey(analogItem.Value)) continue;
                //Tries to parse the function string
                if (analogItem.Name == "Function")
                {
                    if (sqnParser.CheckFunction(analogItem.Value)) continue;
                }
                ErrorMgr.errorMsg(string.Format("Incorrect Value given for {0}. Either choose a parameter name or enter a number.", analogItem.Name), 5, true);
                return false;

            }
            return true;
        }

        private void buildBtn_Click(object sender, RoutedEventArgs e)
        {
            ErrorMgr.warningMsg(Utils.dataPath, 123); return;
            // if (controller.script == null || Controller.sequenceData == null) { MessageBox.Show("No script loaded!"); return; }
            Button btn = sender as Button;
            switch (btn.Name)
            {
                case ("btnUp"):
                    break;
                case ("btnDown"):
                    break;
                case ("btnBuild"):
                    List<SequenceStep> steps = this.sequenceControl.sequenceDataGrid.ItemsSource.Cast<SequenceStep>().ToList();
                    controller.BuildMOTMasterSequence(steps);
                    break;
            }
        }

        private void frmMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Save the currently open sequence to a default location
            MessageBoxResult result = MessageBox.Show("MOTMaster is closing. Do you want to save the sequence?","Save Sequence to File/Default" ,MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                SaveSequence_Click(sender, null);
            }
            else if (result == MessageBoxResult.No)
            {
                List<SequenceStep> steps = sequenceControl.sequenceDataGrid.ItemsSource.Cast<SequenceStep>().ToList();
                controller.SaveSequenceAsDefault(steps);
            }
        }

        private void EditParameters_Click(object sender, RoutedEventArgs e)
        {
            ParametersWindow paramWindow = new ParametersWindow();
            paramWindow.Show();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            dispatcherTimer.Stop();
            if (Controller.ExpData.jumboMode() == ExperimentData.JumboModes.scan)
            {
                MMscan mms = new MMscan();
                mms.FromDictionary(Controller.ExpData.grpMME.prms);
                realScan(mms.sParam, mms.sFrom.ToString(), mms.sTo.ToString(), mms.sBy.ToString(), Controller.ExpData.grpMME.sender, Controller.ExpData.grpMME.id);
            }
            if (Controller.ExpData.jumboMode() == ExperimentData.JumboModes.repeat)
            {
                string jumboGroupID = (string)Controller.ExpData.grpMME.prms["groupID"];
                int jumboCycles = Convert.ToInt32(Controller.ExpData.grpMME.prms["cycles"]);
                realRun(jumboCycles, Controller.ExpData.grpMME.sender, Controller.ExpData.grpMME.id);
            }
        }

        public bool Interpreter(string json)
        {
            //if (messenger != null) messenger.Send("<" + json + ">");
            //return true;
            //string js = File.ReadAllText(@"e:\VSprojects\set.mme");
            MMexec mme = JsonConvert.DeserializeObject<MMexec>(json);

            if (mme.sender.Equals("")) mme.sender = "none";
            if (mme.id == 0) mme.id = -1;
            switch (mme.cmd)
            {
                case ("phaseAdjust"):
                    {
                        if (Controller.ExpData.jumboMode() != ExperimentData.JumboModes.repeat) throw new Exception("Not active Jumbo repeat group command!");
                        double corr = 0;
                        if (mme.prms.ContainsKey("phaseCorrection")) corr = Convert.ToDouble(mme.prms["phaseCorrection"]);
                        Log("<< phaseAdjust to " + corr.ToString("G6"));
                        controller.phaseStrobes.Correction(corr);
                    }
                    break;
                case ("repeat"):
                    Controller.ExpData.grpMME = mme.Clone();
                    btnRun.Content = "Abort Remote";
                    btnRun.Background = Brushes.LightCoral;
                    tcMain.SelectedIndex = 0; DoEvents();
                    tbExperimentRun.Text = (string)mme.prms["groupID"];
                    int cycles = -1; // default to infinity
                    if (mme.prms.ContainsKey("cycles")) cycles = Convert.ToInt32(mme.prms["cycles"]);
                    if (mme.sender.Equals("Axel-hub"))
                    {
                        if (mme.prms.ContainsKey("strobes")) controller.phaseStrobes.DoubleStrobe = Convert.ToInt32(mme.prms["strobes"]) == 2;
                        if (mme.prms.ContainsKey("strobe1")) controller.phaseStrobes.Strobe1 = Convert.ToDouble(mme.prms["strobe1"]);
                        if (mme.prms.ContainsKey("strobe2") && controller.phaseStrobes.DoubleStrobe) controller.phaseStrobes.Strobe2 = Convert.ToDouble(mme.prms["strobe2"]);
                        controller.phaseStrobes.Count = 0;
                    }
                    else if (mme.sender.Equals("Python"))
                    {
                        realRun(cycles);
                    }
                    ntbIterNumb.Value = cycles;
                    dispatcherTimer.Start();
                    break;
                case ("scan"):
                    Controller.ExpData.grpMME = mme.Clone();
                    btnScan.Content = "Abort Remote";
                    btnScan.Background = Brushes.LightCoral;
                    tcMain.SelectedIndex = 1; DoEvents();
                    tbExperimentRun.Text = (string)mme.prms["groupID"];
                    cbParamsScan.Text = (string)mme.prms["param"];
                    tbFromScan.Text = Convert.ToDouble(mme.prms["from"]).ToString();
                    tbToScan.Text = Convert.ToDouble(mme.prms["to"]).ToString();
                    tbByScan.Text = Convert.ToDouble(mme.prms["by"]).ToString();
                    dispatcherTimer.Start();
                    break;
                case ("set"):
                    foreach (var prm in mme.prms)
                    {
                        Controller.sequenceData.Parameters[prm.Key].Value = prm.Value;
                    }
                    break;
                case ("load"):
                    controller.LoadSequenceFromPath((string)mme.prms["file"]);
                    break;
                case ("save"):
                    controller.SaveSequenceToPath((string)mme.prms["file"]);
                    break;
                case ("abort"):
                    //Stop running
                    if (btnRun.Content == "Abort Remote") btnRun_Click(this, null);
                    else if (btnScan.Content == "Abort Remote") btnScan_Click(this, null);
                    break;
            }
            return true;
        }

        private void cancelPropertyBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cbHub_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            controller.SaveToggle(cbHub.SelectedIndex == 1);
            if (remoteMsg != null)
            {
                remoteMsg.Enabled = (cbHub.SelectedIndex == 2);
                controller.SendDataRemotely = (cbHub.SelectedIndex == 2 || cbHub.SelectedIndex == 3);
            }
            if (btnRemote == null) return;
            if (cbHub.SelectedIndex == 2) btnRemote.Content = "Check comm.";
            if (cbHub.SelectedIndex == 3) btnRemote.Content = "Connect";
            if (cbHub.SelectedIndex > 1)
            {
                btnRemote.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                btnRemote.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private async void btnRemote_Click(object sender, RoutedEventArgs e)
        {
            if (cbHub.SelectedIndex == 2)
            {
                if (remoteMsg.CheckConnection()) { ErrorMgr.simpleMsg("Connected to Axel-hub"); }
                else ErrorMgr.errorMsg("Connection to Axel-hub failed !", 666);
            }
            if (cbHub.SelectedIndex == 3)
            {
                if (btnRemote.Content.Equals("Connect"))
                {
                    messenger = new RemoteMessenger("127.0.0.1","127.0.0.1");
                    messenger.Remote += Interpreter;
                    Log("Awaiting remote requests");
                    btnRemote.Content = "Disconnect";
                    btnRemote.Background = Brushes.LightGreen;
                    try
                    {
                        await messenger.Run();
                    }
                    catch (Exception ex)
                    {
                        Log("Error with remote command: " + ex.Message);
                    }
                }
                else
                {
                    Log("Closing remote connection");
                    if (messenger != null) messenger.Close();
                    btnRemote.Content = "Connect";
                    btnRemote.Background = Brushes.LightBlue;
                }
            }
        }
        private void OnActiveComm(bool active)
        {
            if (active) ErrorMgr.simpleMsg("Connected to Axel Hub");
            else ErrorMgr.errorMsg("Commun. problem with Axel Hub", 666);
        }
        private void frmMain_SourceInitialized(object sender, EventArgs e)
        {
            remoteMsg = new RemoteMessaging("Axel Hub");
            remoteMsg.Enabled = false;
            remoteMsg.ActiveComm += OnActiveComm;
            remoteMsg.OnReceive += Interpreter;
        }

        private void aiEnable_Checked(object sender, RoutedEventArgs e)
        {
            //TODO Fix this to bind to hardware class
            DAQ.HAL.NavigatorHardware hardware = (DAQ.HAL.NavigatorHardware)Environs.Hardware;
            hardware.config.UseAI = aiEnable.IsChecked.Value;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            tbLogger.Document.Blocks.Clear();
        }

        private void chkVerbatim_Checked(object sender, RoutedEventArgs e)
        {
            ErrorMgr.Verbatim = chkVerbatim.IsChecked.Value;
        }

        private void nbPower1_ValueChanged(object sender, NationalInstruments.Controls.ValueChangedEventArgs<double> e)
        {
            Type type = typeof(NationalInstruments.Controls.NumericTextBoxDouble);
            string laserKey = (string)type.GetProperty("Name").GetValue(sender);
            Controller.sequenceData.Parameters[laserKey].Value = type.GetProperty("Value").GetValue(sender);


        }

        private void SetInterferometerParams(Dictionary<string, object> scanDict)
        {
            string key = scanDict.Keys.ToArray()[0];
            object control = MSquaredTab.FindName(key);
            if (control == null) return;
            else
            {
                ((NationalInstruments.Controls.NumericTextBoxDouble)control).Value = (double)scanDict[key];
                //Only set them if one is changed
                //TODO fix handling of warnings if ICE-BLocs are not connected
                Controller.SetMSquaredParameters();
            }
        }

        private void SetInterferometerParams(ObservableDictionary<string, Parameter> observableDictionary)
        {
            foreach (KeyValuePair<string, Parameter> entry in observableDictionary)
            {
                if (entry.Value.SequenceVariable) continue;
                else
                {
                    object control = MSquaredTab.FindName(entry.Key);
                    if (control == null) continue;
                    else ((NationalInstruments.Controls.NumericTextBoxDouble)control).Value = Convert.ToDouble(entry.Value.Value);
                }
            }
        }

        private void m2updateBtn_Click(object sender, RoutedEventArgs e)
        {
            Controller.SetMSquaredParameters();
            Log("Updated MSquared laser parameters");
        }


    }

}