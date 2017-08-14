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
using RemoteMessagingNS;
using ErrorManager;
using UtilsNS;


namespace MOTMaster2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Controller controller;
        private RemoteMessenger messenger;
         
        RemoteMessaging remoteMsg;

        public MainWindow()
        {
            controller = new Controller();
            controller.StartApplication();
            controller.LoadDefaultSequence();
            InitializeComponent();
            InitVisuals();
            ErrorMgr.Initialize(ref lbStatus, ref tbLogger, (string)Environs.FileSystem.Paths["configPath"]);
 
            this.sequenceControl.ChangedAnalogChannelCell += new SequenceDataGrid.ChangedAnalogChannelCellHandler(this.sequenceData_AnalogValuesChanged);
            this.sequenceControl.ChangedRS232Cell += new SequenceDataGrid.ChangedRS232CellHandler(this.sequenceData_RS232Changed);
       }

        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                  new Action(delegate { }));
        }

        public void InitVisuals()
        {
            btnRefresh_Click(null, null);
            tcMain.SelectedIndex = 0;
        }

        private string[] ParamsArray
        {
            get
            {
                string[] pa = { "param1", "param2", "param3" };
                if (Controller.sequenceData != null)
                    pa = Controller.sequenceData.Parameters.Where(t => !t.IsHidden).Select(t => t.Name).ToArray();
                return pa;
            }
        }
        
        private bool SingleShot(Dictionary<string,object> paramDict) // true if OK
        {
            //Would like to use RunStart as this Runs in a new thread
            if (controller.IsRunning())
            {
                controller.WaitForRunToFinish();
            }
            try
            {
                controller.RunStart(paramDict);
            }
            catch (WarningException w)
            {
                ErrorMgr.warningMsg(w.Message);
                return false;
            }
            catch (ErrorException er)
            {
                ErrorMgr.errorMsg(er.Message, 11);
                return false;
            }
            catch (Exception e)
            {
                ErrorMgr.errorMsg(e.Message, 11, true);
                return false;
            }
         
            return true;
        }
        private bool SingleShot() // true if OK
        {
            return SingleShot(null);
            
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
            if (Iters <= 0)
            {
                ErrorMgr.errorMsg("<Iteration Number> must be of positive value.",2,true);
                return;
            }
            progBar.Minimum = 0;
            progBar.Maximum = Iters;
            Controller.ExpData.ClearData();
            Controller.ExpData.ExperimentName = controller.ExperimentRunTag;
            controller.StartLogging();
            for (int i = 0; i < Iters; i++)
            {
                
                // single shot
                    ScanFlag = SingleShot();
                controller.SetBatchNumber(i);
                progBar.Value = i;
                DoEvents();
                if (!ScanFlag) break;
            }
            controller.StopLogging();
           
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

                int Iters = int.Parse(tbIterNumb.Text);
                // Start repeat
                    realRun(Iters);
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

        private void realScan(string prm, string fromScanS, string toScanS, string byScanS, string Hub = "none", int cmdId = -1)
        {
            string parameter = prm;
            Parameter param = Controller.sequenceData.Parameters.First(t => t.Name == parameter);
            Dictionary<string, object> scanDict = new Dictionary<string, object>();
            Controller.ExpData.ClearData();
            Controller.ExpData.SaveRawData = true;
            Controller.ExpData.ExperimentName = controller.ExperimentRunTag;
            controller.StartLogging();
            scanDict[parameter] = param.Value;
            object defaultValue = param.Value;
            int scanLength;
            object[] scanArray;
            if (defaultValue is int)
            {
                int fromScanI = int.Parse(fromScanS);
                int toScanI = int.Parse(toScanS);
                int byScanI = int.Parse(byScanS);
                progBar.Minimum = fromScanI;
                progBar.Maximum = toScanI;
                scanLength = (toScanI - fromScanI) / byScanI + 1;
                if (scanLength < 0)
                {
                    ErrorMgr.errorMsg("Incorrect looping parameters. <From> value must be smaller than <To> value if it increases per shot.",3,true);
                    return;
                }
                scanArray = new object[scanLength + 1];
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
                progBar.Minimum = fromScanD;
                progBar.Maximum = toScanD;
                scanLength = (int)((toScanD - fromScanD) / byScanD);
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
            foreach (object scanParam in scanArray)
            {
                controller.SetBatchNumber(c);
                param.Value = scanParam;
                scanDict[parameter] = scanParam;
                progBar.Value = (scanParam!=null && scanParam is double) ? (double)scanParam : Convert.ToDouble((int)scanParam);
                ScanFlag = SingleShot(scanDict);
                tbCurValue.Content = scanParam.ToString();
                DoEvents();
                if (!ScanFlag) break;
                c++;
            }
            param.Value = defaultValue;
            tbCurValue.Content = defaultValue.ToString();
            controller.StopLogging();
        }

        private void btnScan_Click(object sender, RoutedEventArgs e)
        {
            if (btnScan.Content.Equals("Scan"))
            {
                btnScan.Content = "Cancel";
                btnScan.Background = Brushes.LightYellow;
                ScanFlag = true;

                realScan(cbParamsScan.Text, tbFromScan.Text, tbToScan.Text, tbByScan.Text);

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

        private bool paramCheck = false;
        private void tcMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(TabControl))
            {
                if (paramCheck)
                    return;
                paramCheck = true;
                if (tcMain.SelectedIndex == 1)
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
                    cbParamsManual.Text = ParamsArray[0];
                    cbParamsManual.SelectedIndex = 0;

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
                    string[] text = File.ReadAllLines(filename);

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
                Nullable<bool> result = dlg.ShowDialog();

                // Process open file dialog box results
                if (result == true)
                {
                    string filename = dlg.FileName;
                    string json = JsonConvert.SerializeObject(controller.script.Parameters, Formatting.Indented);
                    File.WriteAllText(filename, json);
                }
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
                Nullable<bool> result = dlg.ShowDialog();

                // Process open file dialog box results
                if (result == true)
                {
                    string filename = dlg.FileName;
                    controller.SaveSequenceToPath(filename);
                }
            }
            else
                ErrorMgr.warningMsg("You have tried to save a Sequence before loading a script",-1,true);

        }
        private void LoadSequence_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".sm2"; // Default file extension
            dlg.Filter = "Sequence (.sm2)|*.sm2"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                string filename = dlg.FileName;
                controller.LoadSequenceFromPath(filename);
            }

        }
  
        private void LoadCicero_Click(object sender, RoutedEventArgs e)
        {
            
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Title = "Select Cicero Settings File";
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".set"; // Default file extension
            dlg.Filter = "Cicero Settings (.set,.json)|*.json;*.set"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                string filename = dlg.FileName;
                controller.LoadCiceroSettingsFromPath(filename);

                dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.Title = "Select Cicero Sequence File";
                dlg.FileName = ""; // Default file name
                dlg.DefaultExt = ".seq"; // Default file extension
                dlg.Filter = "Cicero Sequence (.seq,.json)|*.json;*.seq"; // Filter files by extension

                // Show open file dialog box
                result = dlg.ShowDialog();

                // Process open file dialog box results
                if (result == true)
                {
                    filename = dlg.FileName;
                    controller.LoadCiceroSequenceFromPath(filename);

                    controller.ConvertCiceroSequence();
                    Log("Loaded Cicero Sequence from " + filename);
                    UpdateSequenceControl();
                    
                }
            }


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
            Parameter param = Controller.sequenceData.Parameters.Where(t => t.Name == parameter).First();
            if (param.Value.GetType() == typeof(int))
            {
                param.Value = int.Parse(tbValue.Text);
            }
            else if (param.Value.GetType() == typeof(double))
            {
                param.Value = double.Parse(tbValue.Text);
            }
        }

        private void cbParamsManual_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(ComboBox) && controller.script != null)
                tbValue.Text = Controller.sequenceData.Parameters.Where(t => t.Name == cbParamsManual.SelectedItem.ToString()).Select(t => t.Value).First().ToString();
        }

        private void cbParamsScan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(ComboBox) && cbParamsScan.SelectedItem != null)
                tbCurValue.Content = Controller.sequenceData.Parameters.Where(t => t.Name == cbParamsScan.SelectedItem.ToString()).Select(t => t.Value).First().ToString();
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
        private void Log(string text)
        {
            tbLogger.AppendText("> " + text + "\n");

            //tbLogger.Focus();
            //tbLogger.CaretIndex = tbLogger.Text.Length;
            tbLogger.ScrollToEnd();
        }

        private void setProperty_Click(object sender, RoutedEventArgs e)
        {
            SequenceParser sqnParser = new SequenceParser();
            bool verified = false;
            //Checks the validity of all the values, but does not assign them until the sequence is built
            //TODO: Add a type check to make this work for AnalogItems or SerialItems
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
                    else ErrorMgr.errorMsg(string.Format("Incorrect format for {0} serial command", item.Name),4);
                }
                catch (Exception e)
                {
                    ErrorMgr.errorMsg("Couldn't parse serial commands. " + e.Message,4,false);
                    return false;
                }

            }
            return true;
        }
        private bool ParseAnalogItems(SequenceParser sqnParser)
        {
            foreach (AnalogArgItem analogItem in (List<AnalogArgItem>)propertyGrid.DataContext)
            {
                double analogRawValue;
                if (Double.TryParse(analogItem.Value, out analogRawValue)) continue;
                if (Controller.sequenceData != null && Controller.sequenceData.Parameters.Select(t => t.Name).Contains(analogItem.Value)) continue;
                //Tries to parse the function string
                if (analogItem.Name == "Function")
                {
                    if (sqnParser.CheckFunction(analogItem.Value)) continue;
                }
                ErrorMgr.errorMsg(string.Format("Incorrect Value given for {0}. Either choose a parameter name or enter a number.", analogItem.Name),5,true);
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
            List<SequenceStep> steps = sequenceControl.sequenceDataGrid.ItemsSource.Cast<SequenceStep>().ToList();
            controller.SaveSequenceAsDefault(steps);
        }

        private void EditParameters_Click(object sender, RoutedEventArgs e)
        {
            ParametersWindow paramWindow = new ParametersWindow();
            paramWindow.Show();
        }

        private void tbExperimentRun_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tbExperimentRun.Text != "---") controller.ExperimentRunTag = tbExperimentRun.Text;
        }

        public class MMexec
        {
            public string mmexec { get; set; }
            public string sender { get; set; }
            public string cmd { get; set; }
            public int id { get; set; }
            public Dictionary<string, object> prms = new Dictionary<string,object>();       
    
        }

        public bool Interpreter(string json)
        {
           
            messenger.Send("<" + json + ">");
            return true;
             //string js = File.ReadAllText(@"e:\VSprojects\set.mme");
            MMexec mme = JsonConvert.DeserializeObject<MMexec>(json);
            if (mme.sender.Equals("")) mme.sender = "none";
            if (mme.id == 0) mme.id = -1;
            switch (mme.cmd)
            {
                case("repeat"):
                    int Iters = (int)mme.prms["cycles"];
                    realRun(Iters, mme.sender, mme.id);
                    break;
                case("scan"):
                    realScan((string)mme.prms["param"], (string)mme.prms["from"], (string)mme.prms["to"], (string)mme.prms["by"], mme.sender, mme.id);
                    break;
                case ("set"):
                    foreach (var prm in mme.prms)
                    {
                        IEnumerable<Parameter> paramList  = Controller.sequenceData.Parameters.Where(t => t.Name == (string)prm.Key);
                        if (paramList.Count() != 1) continue;
                        else
                        {
                           Parameter p = paramList.First();
                           p.Value = prm.Value;
                    }
                    }
                    break;
                case ("load"):
                    controller.LoadSequenceFromPath((string)mme.prms["file"]);
                    break;
                case ("save"):
                    controller.SaveSequenceToPath((string)mme.prms["file"]);
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
            if(remoteMsg != null) remoteMsg.Enabled = (cbHub.SelectedIndex == 2);
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
                if (remoteMsg.CheckConnection()) ErrorMgr.simpleMsg("Connected to Axel-hub");
                else ErrorMgr.errorMsg("Connection to Axel-hub failed !", 666);
            }
            if (cbHub.SelectedIndex == 3) 
            {
                if (btnRemote.Content.Equals("Connect")) 
                {
                    messenger = new RemoteMessenger();
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
        }

        private void aiEnable_Checked(object sender, RoutedEventArgs e)
        {
            //TODO Fix this to bind to hardware class
            DAQ.HAL.NavigatorHardware hardware = (DAQ.HAL.NavigatorHardware)Environs.Hardware;
            hardware.config.UseAI = aiEnable.IsChecked.Value;
        }
    }
    
}
