using DAQ;
using DAQ.Analog;
using DAQ.Environment;
using DAQ.HAL;
using Microsoft.CSharp;
using MOTMaster2.SequenceData;
using Newtonsoft.Json;
using System;
//using IMAQ;

using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
//using DataStructures;
using System.Runtime.Serialization.Formatters.Binary;
using UtilsNS;
using ErrorManager;

namespace MOTMaster2
{
    /// <summary>
    /// Here's MOTMaster's controller.
    /// 
    /// Gets a MOTMasterScript (a script contaning a series of commands like "addEdge" for both digital and analog)
    /// from user (either remotely or via UI), compiles it, builds a pattern and sends it
    /// to hardware.
    /// </summary>
    public class Controller : MarshalByRefObject
    {

        #region Class members

        private static string
            motMasterPath = (string)Environs.FileSystem.Paths["MOTMasterEXEPath"] + "MOTMaster2.exe";
        private static string
            daqPath = (string)Environs.FileSystem.Paths["daqDLLPath"];
        private static string
            scriptListPath = (string)Environs.FileSystem.Paths["scriptListPath"];
        private static string
            motMasterDataPath = (string)Environs.FileSystem.Paths["DataPath"];
        private static string
            saveToDirectory = (string)Environs.FileSystem.Paths["MOTMasterDataPath"];
        private static string
            cameraAttributesPath = (string)Environs.FileSystem.Paths["CameraAttributesPath"];
        private static string
            hardwareClassPath = (string)Environs.FileSystem.Paths["HardwareClassPath"];

        private static string defaultScriptPath = scriptListPath + "\\defaultScript.sm2";

        private static string digitalPGBoard = (string)Environs.Hardware.Boards["multiDAQ"];

        public static MMConfig config = (MMConfig)Environs.Hardware.GetInfo("MotMasterConfiguration");

        private Thread runThread;

        public enum RunningState { stopped, running };
        public RunningState status = RunningState.stopped;

        public List<string> analogChannels;
        public List<string> digitalChannels;
        public MOTMasterScript script;
        public static Sequence sequenceData;
        public static MOTMasterSequence sequence;
        public static ExperimentData ExpData { get; set; }
        public static AutoFileLogger dataLogger;
        public static AutoFileLogger paramLogger;
        public bool SendDataRemotely { get; set; }

        public event DataEventHandler MotMasterDataEvent;
        public delegate void DataEventHandler(object sender, DataEventArgs d);

        DAQMxPatternGenerator pg;
        HSDIOPatternGenerator hs;
        DAQMxPatternGenerator PCIpg;
        DAQMxAnalogPatternGenerator apg;
        MMAIWrapper aip;

        private static int aiSampleRate = 200000;
        private static double riseTime = 0.0001;
        public static bool StaticSequence { get; set; }

        CameraControllable camera = null;
        TranslationStageControllable tstage = null;
        ExperimentReportable experimentReporter = null;

        private WindfreakSynth microSynth;
        //public string ExperimentRunTag { get; set; }
        public MMscan? ScanParam { get; set; }
        public int numInterations;
        MuquansController muquans = null;
        public static ICEBlocDCS M2DCS;
        public static ICEBlocPLL M2PLL;

        MMDataIOHelper ioHelper;

        SequenceBuilder builder;

        DataStructures.SequenceData ciceroSequence;
        DataStructures.SettingsData ciceroSettings;
        #endregion

        #region Initialisation

        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        public void StartApplication()
        {
            if (Environs.Debug && config == null)
            {
                LoadEnvironment();
            }
            LoadDefaultSequence();
            if (!config.HSDIOCard) pg = new DAQMxPatternGenerator((string)Environs.Hardware.Boards["analog"]);
            else hs = new HSDIOPatternGenerator((string)Environs.Hardware.Boards["hsDigital"]);
            apg = new DAQMxAnalogPatternGenerator();
            PCIpg = new DAQMxPatternGenerator((string)Environs.Hardware.Boards["multiDAQPCI"]);
            aip = new MMAIWrapper((string)Environs.Hardware.Boards["analogIn"]);

            if (ExpData == null) { ExpData = new ExperimentData(); ExpData.SampleRate = aiSampleRate; ExpData.RiseTime = riseTime; }
            digitalChannels = Environs.Hardware.DigitalOutputChannels.Keys.Cast<string>().ToList();

            if (config.CameraUsed) camera = (CameraControllable)Activator.GetObject(typeof(CameraControllable),
                "tcp://localhost:1172/controller.rem");

            if (config.TranslationStageUsed) tstage = (TranslationStageControllable)Activator.GetObject(typeof(CameraControllable),
                "tcp://localhost:1172/controller.rem");

            if (config.ReporterUsed) experimentReporter = (ExperimentReportable)Activator.GetObject(typeof(ExperimentReportable),
                "tcp://localhost:1172/controller.rem");

            if (config.UseMuquans) { muquans = new MuquansController();  if (!config.Debug) { microSynth = (WindfreakSynth)Environs.Hardware.Instruments["microwaveSynth"]; microSynth.Connect(); /*microSynth.TriggerMode = WindfreakSynth.TriggerTypes.Pulse;*/ } }
            if (config.UseMSquared)
            {
            if (Environs.Hardware.Instruments.ContainsKey("MSquaredDCS")) M2DCS = (ICEBlocDCS)Environs.Hardware.Instruments["MSquaredDCS"];
                else throw new Exception("Cannot find DCS ICE-BLOC");
            if (Environs.Hardware.Instruments.ContainsKey("MSquaredPLL")) M2PLL = (ICEBlocPLL)Environs.Hardware.Instruments["MSquaredPLL"];
                else throw new Exception("Cannot find PLL ICE-BLOC");


                //Adds MSquared parameters if not already found
                if (!sequenceData.Parameters.ContainsKey("PLLFreq"))
                {
                    sequenceData.Parameters["PLLFreq"] = new Parameter("PLLFreq","",6834.689,true,false);
                    sequenceData.Parameters["ChirpRate"] = new Parameter("ChirpRate","",0.5,true,false);
                    sequenceData.Parameters["ChirpDuration"] = new Parameter("ChirpDuration","",0.5,true,false);

                    sequenceData.Parameters["Pulse1Power"] = new Parameter("Pulse1Power","",22.0,true,false);
                    sequenceData.Parameters["Pulse1Duration"] = new Parameter("Pulse1Duration","",10,true,false);
                    sequenceData.Parameters["Pulse1Phase"] = new Parameter("Pulse1Phase","",0.0,true,false);

                    sequenceData.Parameters["Pulse2Power"] = new Parameter("Pulse2Power","",22,true,false);
                    sequenceData.Parameters["Pulse2Duration"] = new Parameter("Pulse2Duration","",10,true,false);
                    sequenceData.Parameters["Pulse2Phase"] = new Parameter("Pulse2Phase","",0.0,true,false);

                    sequenceData.Parameters["Pulse3Power"] = new Parameter("Pulse3Power","",22.0,true,false);
                    sequenceData.Parameters["Pulse3Duration"] = new Parameter("Pulse3Duration","",10,true,false);
                    sequenceData.Parameters["Pulse3Phase"] = new Parameter("Pulse3Phase","",0.0,true,false);

                    sequenceData.Parameters["VelPulsePower"] = new Parameter("VelPulsePower","",22.0,true,false);
                    sequenceData.Parameters["VelPulseDuration"] = new Parameter("VelPulseDuration","",10,true,false);
                    sequenceData.Parameters["VelPulsePhase"] = new Parameter("VelPulsePhase","",0.0,true,false);

                    sequenceData.Parameters["IntTime1"] = new Parameter("IntTime1", "", 25.0, true, false);
                    sequenceData.Parameters["IntTime2"] = new Parameter("IntTime2", "", 25.0, true, false);
                }
                try
                {
                    M2DCS.Connect();
                    M2PLL.Connect();

                    M2PLL.StartLink();
                    M2DCS.StartLink();
                    SetMSquaredParameters();
                }
                catch
                {
                    //Set to popup to avoid Exception called when it can't write to a Log
                       ErrorMgr.warningMsg("Could not set MSquared Parameters",-1,true);
                }
            }

            ioHelper = new MMDataIOHelper(motMasterDataPath,
                    (string)Environs.Hardware.GetInfo("Element"));

            ScriptLookupAndDisplay();
        }

        #endregion

        #region Hardware control methods


        private void run(MOTMasterSequence sequence)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            if (config.UseMuquans)
            {
                muquans.StartOutput();
            }
            Console.WriteLine("Started muquans at {0}ms", watch.ElapsedMilliseconds);
            apg.OutputPatternAndWait(sequence.AnalogPattern.Pattern);
            Console.WriteLine("Started apg at {0}ms", watch.ElapsedMilliseconds);
            if (config.UseAI) aip.StartTask();
            if (!config.HSDIOCard) pg.OutputPattern(sequence.DigitalPattern.Pattern, true);
            else
            {
                int[] loopTimes = ((DAQ.Pattern.HSDIOPatternBuilder)sequence.DigitalPattern).LoopTimes;
                hs.OutputPattern(sequence.DigitalPattern.Pattern, loopTimes);
                Console.WriteLine("Started hs at {0}ms", watch.ElapsedMilliseconds);
            }

        }
        private void ContinueLoop()
        {
           
            //Just need to restart the cards
            apg.StartPattern();
            if (config.UseAI) aip.StartTask();
            if (config.HSDIOCard)
            {
                hs.StartPattern();
            }
            else
            {
                throw new NotImplementedException("DAQmx digital cards not currently supported");
            }
        }

        private void initializeHardware(MOTMasterSequence sequence)
        {
            if (!config.HSDIOCard) pg.Configure(config.DigitalPatternClockFrequency, StaticSequence, true, true, sequence.DigitalPattern.Pattern.Length, true, false);
            else hs.Configure(config.DigitalPatternClockFrequency, StaticSequence, true, false);
            if (config.UseMuquans) muquans.Configure(StaticSequence);
            apg.Configure(sequence.AnalogPattern, config.AnalogPatternClockFrequency, StaticSequence);
            if (config.UseAI) { 
                aip.Configure(sequence.AIConfiguration, StaticSequence);
                aip.AnalogDataReceived += OnAnalogDataReceived;
            }
        }


        private void releaseHardware()
        {
            if (!config.HSDIOCard) pg.StopPattern();
            else hs.StopPattern();
            apg.StopPattern();
            if (config.UseAI) { aip.StopPattern(); }
            if (config.UseMuquans) muquans.StopOutput();
        }

        private void pauseHardware()
        {
            apg.PauseLoop();
            if (config.UseAI) aip.PauseLoop();
            if (config.HSDIOCard) hs.PauseLoop();
            else throw new NotImplementedException("DAQmx digital cards not currently supported");
        }
        //private void releaseHardwareLoop()
        //{
        //    if (!config.HSDIOCard) pg.StopPattern();
        //    else hs.AbortRunning();
        //    apg.AbortRunning();
        //    if (config.UseAI) aip.AbortRunning();
        //    if (config.UseMuquans) muquans.StopOutput();
        //}
        private void clearDigitalPattern(MOTMasterSequence sequence)
        {
            sequence.DigitalPattern.Clear(); //No clearing required for analog (I think).
        }
        private void releaseHardwareAndClearDigitalPattern(MOTMasterSequence sequence)
        {
            clearDigitalPattern(sequence);
            releaseHardware();
        }

        //TODO Add this to the experiment-specific AccelSuite
        private void WriteToMicrowaveSynth(double value)
        {
            if (config.UseMuquans && !config.Debug) microSynth.ChannelA.Frequency = value;
        }


        protected void OnAnalogDataReceived(object sender, EventArgs e)
        {
            var rawData = config.Debug ? ExpData.GenerateFakeData() : aip.GetAnalogData();
            MMexec finalData = ConvertDataToAxelHub(rawData);
            if (ExpData.grpMME.cmd.Equals("repeat") && SendDataRemotely)
            {
                if (Convert.ToInt32(ExpData.grpMME.prms["cycles"]) == (Convert.ToInt32(finalData.prms["runID"]) + 1))
                {
                    finalData.prms["last"] = 1;
                }
            }
            if (ExpData.grpMME.cmd.Equals("scan") && SendDataRemotely)
            {
                MMscan mms = new MMscan();
                mms.FromDictionary(ExpData.grpMME.prms);
                int k = (int)((mms.sTo - mms.sFrom) / mms.sBy);
                if (k == (Convert.ToInt32(finalData.prms["runID"])))
                {
                    finalData.prms["last"] = 1;
                }
            }
            string dataJson = JsonConvert.SerializeObject(finalData, Formatting.Indented);
            dataLogger.log("{\"MMExec\":" + dataJson + "},");
            if (SendDataRemotely)
            {
                if (MotMasterDataEvent != null) MotMasterDataEvent(this, new DataEventArgs(dataJson));
            }
        }


        internal void StopRunning()
        {
            if (!config.Debug)
            {
                while (IsRunning() && !StaticSequence)
                {
                    WaitForRunToFinish();
                }
                releaseHardware();
            }
            StaticSequence = false; //Set this here in case we want to scan after
            status = RunningState.stopped;
        }

       
        #endregion

        #region Housekeeping on UI

        /// - MOTMaster looks in a folder ("scriptListPath") for all classes. 
        ///  Then displays the list in a combo box.
        /// 
        /// - These classes contain an implementation of a "MOTMasterScript". This contains the information 
        /// about the patterns.
        public void ScriptLookupAndDisplay()
        {
            string[] s = scriptLookup();
            displayScripts(s);
        }
        private string[] scriptLookup()
        {
            string[] scriptList = Directory.GetFiles(scriptListPath, "*.cs");
            return scriptList;
        }
        private void displayScripts(string[] s)
        {
            //controllerWindow.FillScriptComboBox(s);
        }

        #endregion

        #region RUN RUN RUN (public & remotable stuff)

        /// <summary>
        /// This is the guts of MOTMaster.
        /// 
        /// - MOTMaster initializes the hardware, faffs a little to prepare the patterns in the 
        /// builders (e.g. calls "BuildPattern"), and sends the pattern to Hardware.
        /// 
        /// -Note that the analog stuff needs a trigger to start!!!! Make sure one of your digital lines is reserved 
        /// for triggering the analog pattern.
        /// 
        /// - Once the experiment is finished, MM releases the hardware.
        /// 
        /// - MOTMaster also saves the data to a .zip. This includes: the original MOTMasterScript (.cs), a text file
        /// with the parameters in it (IF DIFFERENT FROM THE VALUES IN .cs, THE PARAMETERS IN THE TEXT FILE ARE THE
        /// CORRECT VALUES!), another text file with the camera attributes, yet another file (entitled hardware report)
        ///  which contains the values set by the Hardware controller at the start of the run, and a .png file(s) containing the final image(s).
        /// 
        /// -There are 2 ways of using "Run". Run(null) uses the parameters given in the script (.cs file).
        ///  Run(Dictionary<>) compiles the .cs file but then replaces values in the dictionary. This is to allow
        ///  the user to inject values after compilation but before sending to hardware. By doing this,
        ///  the user can scan parameters using a python script, for example.
        ///  If you call Run(), MOTMaster immediately checks to see if you're running a fresh script 
        ///  or whether you're re-running an old one. In the former case Run(null) is called. In the latter,
        ///  MOTMaster will fetch the dictionary used in the old experiment and use it as the
        ///  argument for Run(Dictionary<>).        ///  
        /// 

        /// </summary>

        private bool saveEnable = true;


        public void SaveToggle(System.Boolean value)
        {
            //saveEnable = value;
            //controllerWindow.SetSaveCheckBox(value);
        }
        private int batchNumber = 0;
        public void SetBatchNumber(Int32 number)
        {
            batchNumber = number;
            //controllerWindow.WriteToSaveBatchTextBox(number);
        }
        private string scriptPath = "";
        public void SetScriptPath(String path)
        {
            scriptPath = path;
            //controllerWindow.WriteToScriptPath(path);
        }
        private bool replicaRun = false;
        public void SetReplicaRunBool(System.Boolean value)
        {
            replicaRun = value;
        }
        private string dictionaryPath = "";
        public void SetDictionaryPath(String path)
        {
            dictionaryPath = path;
        }

        public bool IsRunning()
        {
            if (status == RunningState.running)
            {
                Console.WriteLine("Thread Running");
                return true;
            }
            else
                return false;
        }
        public void RunStart(Dictionary<string,object> paramDict, bool loop = false)
        {
            runThread = new Thread(new ParameterizedThreadStart(this.Run));
           // StaticSequence = loop;
            runThread.Name = "MOTMaster Controller";
            runThread.Priority = ThreadPriority.Highest;
            status = RunningState.running;
            runThread.Start(paramDict);
            Console.WriteLine("Thread Starting");
        }
        public void WaitForRunToFinish()
        {
            if (runThread != null) runThread.Join();
            Console.WriteLine("Thread Waiting");
        }

        public void Run()
        {
            status = RunningState.running;
            Run(replicaRun ? ioHelper.LoadDictionary(dictionaryPath) : null);
        }

        public void Run(Dictionary<String, Object> dict)
        {
            Run(dict, batchNumber);
        }

        public void Run(object dict)
        {
            Run((Dictionary<string, object>) dict, batchNumber);
        }
       
        public void Run(Dictionary<String, Object> dict, int myBatchNumber)
        {
            Stopwatch watch = new Stopwatch();
            if (config.UseMMScripts || sequenceData == null)
            {
                script = prepareScript(scriptPath, dict);
                sequence = getSequenceFromScript(script);
            }
            else
            {
                
                if (config.UseAI)
                {
                    CreateAcquisitionTimeSegments();
                   
                }
                    if(!StaticSequence)sequence = getSequenceFromSequenceData(dict);
                    //TODO Change where this is sent. Di we want to send this before each shot during a scan?
                    if (myBatchNumber == 0)
                    {
                        //Only intialise and build once
                        if (StaticSequence)
                        {
                            sequence = getSequenceFromSequenceData(dict);
                            if(!config.Debug) initializeHardware(sequence);
                            if (config.UseMMScripts) buildPattern(sequence, (int)script.Parameters["PatternLength"]);
                            else buildPattern(sequence, (int)builder.Parameters["PatternLength"]);
                        }
                        MMexec mme = InitialCommand(ScanParam);
                        string initJson = JsonConvert.SerializeObject(mme, Formatting.Indented);
                        paramLogger.log("{\"MMExec\":" + initJson + "},");
                        if (SendDataRemotely && (ExpData.jumboMode() == ExperimentData.JumboModes.none))
                        {
                            MotMasterDataEvent(this, new DataEventArgs(initJson));
                            ExpData.grpMME = mme.Clone();
                        }
                    }
                }
            
            if (sequence != null)
            {
                try
                {
                    if (config.CameraUsed) prepareCameraControl();

                    if (config.TranslationStageUsed) armTranslationStageForTimedMotion(script);

                    if (config.CameraUsed) GrabImage((int)script.Parameters["NumberOfFrames"]);

                    if (config.UseMuquans && !config.Debug)
                    {
                        microSynth.ChannelA.RFOn = true;
                        //microSynth.ChannelA.Amplitude = 6.0;
                        WriteToMicrowaveSynth((double)builder.Parameters["MWFreq"]);
                   
                        //microSynth.ReadSettingsFromDevice();
                    }
                    if (!StaticSequence)
                    {
                        if (config.UseMMScripts) buildPattern(sequence, (int)script.Parameters["PatternLength"]);
                        else buildPattern(sequence, (int)builder.Parameters["PatternLength"]);
                    }
                    if (config.CameraUsed) waitUntilCameraIsReadyForAcquisition();

                    watch.Start();
                    if (!config.Debug)
                    {
                        if (myBatchNumber == 0 || !StaticSequence) runPattern(sequence);
                        else ContinueLoop();
                    }
                    //if (!config.Debug || config.UseMMScripts)clearDigitalPattern(sequence);

                    watch.Stop();
                    if (saveEnable)
                    {

                        if (config.CameraUsed)
                        {

                            waitUntilCameraAquisitionIsDone();

                            try
                            {
                                checkDataArrived();
                            }
                            catch (DataNotArrivedFromHardwareControllerException)
                            {
                                return;
                            }

                            Dictionary<String, Object> report = new Dictionary<string, object>();
                            if (config.ReporterUsed)
                            {
                                report = GetExperimentReport();
                                //TODO Change save method
                                
                            }
                            save(script, scriptPath, imageData, report, myBatchNumber);
                        }
                        else
                        {
                            Dictionary<String, Object> report = new Dictionary<string, object>();
                            if (config.ReporterUsed)
                            {
                                report = GetExperimentReport();
                                //TODO Change save method
                               
                            }
                            if (config.UseMMScripts)
                            {
                                save(script, scriptPath, report, myBatchNumber);
                            }
                            else
                            {
                                save(builder, motMasterDataPath,report, ExpData.ExperimentName,myBatchNumber);
                            }
                        }
                    }
                    if (config.CameraUsed) finishCameraControl();
                    if (config.TranslationStageUsed) disarmAndReturnTranslationStage();
                    if (config.UseMuquans && !config.Debug) microSynth.ChannelA.RFOn = false;
                    if (config.UseAI) OnAnalogDataReceived(this, null);
                    if (StaticSequence && !config.Debug) pauseHardware();
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    MessageBox.Show("CameraControllable not found. \n Is there a hardware controller running? \n \n" + e.Message, "Remoting Error");
                }
            }
            else
            {
                throw new ErrorException("Sequence not found. \n Check that it has been built using the datagrid or loaded from a script.");

            }
            status = RunningState.stopped;
            //Dereferences the MMScan object
            ScanParam = null;
        }

        #endregion

        #region private stuff

        private void updateSaveDirectory(string newDirectory)
        {
            saveToDirectory = newDirectory;
            if (!Directory.Exists(newDirectory))
            {
                Directory.CreateDirectory(saveToDirectory);
            }

        }

        //TODO Change the way everything is saved
        private void save(MOTMasterScript script, string pathToPattern, byte[,] imageData, Dictionary<String, Object> report, double[,] aiData, int batchNumber)
        {
            ioHelper.StoreRun(motMasterDataPath, batchNumber, pathToPattern, hardwareClassPath,
                script.Parameters, report, cameraAttributesPath, imageData, config.ExternalFilePattern);
        }
        private void save(MOTMasterScript script, string pathToPattern, byte[][,] imageData, Dictionary<String, Object> report, double[,] aiData, int batchNumber)
        {
            ioHelper.StoreRun(motMasterDataPath, batchNumber, pathToPattern, hardwareClassPath,
                script.Parameters, report, cameraAttributesPath, imageData, config.ExternalFilePattern);
        }
        private void save(MOTMasterScript script, string pathToPattern, Dictionary<String, Object> report, int batchNumber)
        {
            ioHelper.StoreRun(motMasterDataPath, batchNumber, pathToPattern, hardwareClassPath,
                script.Parameters, report, config.ExternalFilePattern);
        }
        private void save(MOTMasterScript script, string pathToPattern, byte[][,] imageData, Dictionary<String, Object> report, int batchNumber)
        {
            ioHelper.StoreRun(motMasterDataPath, batchNumber, pathToPattern, hardwareClassPath,
                script.Parameters, report, cameraAttributesPath, imageData, config.ExternalFilePattern);
        }
        private void save(SequenceBuilder builder, string saveDirectory, Dictionary<string, object> report,string element,int batchNumber)
        {
            ioHelper.StoreRun(builder, saveDirectory, report,element,batchNumber);
        }

        
        private void runPattern(MOTMasterSequence sequence)
        {
            if (!StaticSequence)
            {
                try
                {
                    initializeHardware(sequence);
                }
                catch (Exception e)
                {
                    throw new ErrorManager.ErrorException("Could not initialise hardware:" + e.Message);
                }
            }
            run(sequence);
            if (!StaticSequence) { aip.ReadAnalogDataFromBuffer(); releaseHardware(); }
            //else pauseHardware();
        }

        private void debugRun(MOTMasterSequence sequence)
        {
                int[] loopTimes = ((DAQ.Pattern.HSDIOPatternBuilder)sequence.DigitalPattern).LoopTimes;
                hs.BuildScriptForDebug(sequence.DigitalPattern.Pattern, loopTimes);
        }
        public MOTMasterScript prepareScript(string pathToPattern, Dictionary<String, Object> dict)
        {
            MOTMasterScript script;
            CompilerResults results = compileFromFile(pathToPattern);
            if (results != null)
            {

                script = loadScriptFromDLL(results);
                if (dict != null)
                {
                    script.EditDictionary(dict);

                }
                return script;

            }
            return null;
        }

        private void buildPattern(MOTMasterSequence sequence, int patternLength)
        {
            sequence.DigitalPattern.BuildPattern(patternLength);
            sequence.AnalogPattern.BuildPattern();
            if (config.UseMuquans) muquans.BuildCommands(sequence.MuquansPattern.commands);
        }

        #endregion

        #region Compiler & Loading DLLs

        /// <summary>
        ///   /// - Once the user has selected a particular implementation of MOTMasterScript, 
        /// MOTMaster will compile it. Note: the dll is currently stored in a temp folder somewhere. 
        /// Its pathToPattern can be found in the CompilerResults.PathToAssembly). 
        /// This newly formed dll contain methods named GetDigitalPattern and GetAnalogPattern. 
        /// 
        /// - These are called by the script's "GetSequence". GetSequence always returns a 
        /// "MOTMasterSequence", which comprises a PatternBuilder32 and an AnalogPatternBuilder.
        /// </summary>

        private CompilerResults compileFromFile(string scriptPath)
        {
            CompilerParameters options = new CompilerParameters();

            options.ReferencedAssemblies.Add(motMasterPath);
            options.ReferencedAssemblies.Add(daqPath);
            options.ReferencedAssemblies.Add("System.Core.dll");

            TempFileCollection tempFiles = new TempFileCollection();
            tempFiles.KeepFiles = true;
            CompilerResults results = new CompilerResults(tempFiles);
            options.GenerateExecutable = false;                         //Creates .dll instead of .exe.
            CodeDomProvider codeProvider = new CSharpCodeProvider();
            options.TempFiles = tempFiles;
            //options.GenerateInMemory = false; // may not be necessary...haven't tried this in a while
            //options.OutputAssembly = string.Format(@"{0}\{1}", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "mydll.dll");
            try
            {
                results = codeProvider.CompileAssemblyFromFile(options, scriptPath);
                if (results.Errors.Count > 0)
                {
                    MessageBox.Show("Error in MOTMaster Script Compilation");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
            //controllerWindow.WriteToScriptPath(results.PathToAssembly);
            return results;
        }

        private MOTMasterScript loadScriptFromDLL(CompilerResults results)
        {
            object loadedInstance = new object();
            try
            {
                Assembly patternAssembly = Assembly.LoadFrom(results.PathToAssembly);
                foreach (Type type in patternAssembly.GetTypes())
                {
                    if (type.IsClass == true)
                    {
                        loadedInstance = Activator.CreateInstance(type);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + e.InnerException.Message, "Error in loading script DLL");
                return null;
            }

            return (MOTMasterScript)loadedInstance;
        }

        private MOTMasterSequence getSequenceFromScript(MOTMasterScript script)
        {
            MOTMasterSequence sequence = script.GetSequence(config.HSDIOCard, config.UseMuquans);

            return sequence;
        }

        private MOTMasterSequence getSequenceFromSequenceData(Dictionary<string,object> paramDict)
        {
            
            builder = new SequenceBuilder(sequenceData);
            builder.BuildSequence();
            if (paramDict != null) { builder.EditDictionary(paramDict); }
            MOTMasterSequence sequence = builder.GetSequence(config.HSDIOCard,config.UseMuquans);
            return sequence;
        }
        public void BuildMOTMasterSequence(List<SequenceStep> steps)
        {
            builder = new SequenceBuilder(sequenceData);

            builder.BuildSequence();


            sequence = builder.GetSequence(config.HSDIOCard, config.UseMuquans);

            if (sequenceData == null)
            {
                sequenceData = new Sequence();
                sequenceData.Steps = steps;
                sequenceData.CreateParameterList(script.Parameters);
            }
            if (config.Debug) MessageBox.Show("Successfully Built Sequence.");
        }
        #endregion

        #region CameraControl

        /// <summary>
        /// - Camera control is run through the hardware controller. All MOTMaster knows 
        /// about it a function called "GrabImage(string cameraSettings)". If the camera attributes are 
        /// set so that it needs a trigger, MOTMaster will have to deliver that too.
        /// It'll expect a byte[,] or byte[][,] (if there are several images) as a return value.
        /// 
        /// -At the moment MOTMaster won't run without a camera nor with 
        /// more than one. In the long term, we might 
        /// want to fix this.
        /// </summary>
        /// 
        int nof;
        public void GrabImage(int numberOfFrames)
        {
            nof = numberOfFrames;
            Thread LLEThread = new Thread(new ThreadStart(grabImage));
            LLEThread.Start();

        }

        bool imagesRecieved = false;
        /*private byte[,] imageData;
        private void grabImage()
        {
            imagesRecieved = false;
            imageData = (byte[,])camera.GrabSingleImage(cameraAttributesPath);
            imagesRecieved = true;
        }*/
        private byte[][,] imageData;
        private void grabImage()
        {
            imagesRecieved = false;
            imageData = camera.GrabMultipleImages(cameraAttributesPath, nof);
            imagesRecieved = true;
        }
        public class DataNotArrivedFromHardwareControllerException : Exception { };
        private bool waitUntilCameraAquisitionIsDone()
        {
            while (!imagesRecieved)
            { Thread.Sleep(10); }
            return true;
        }
        private bool waitUntilCameraIsReadyForAcquisition()
        {
            while (!camera.IsReadyForAcquisition())
            { Thread.Sleep(10); }
            return true;
        }
        private void prepareCameraControl()
        {
            camera.PrepareRemoteCameraControl();
        }
        private void finishCameraControl()
        {
            camera.FinishRemoteCameraControl();
        }
        private void checkDataArrived()
        {
            if (imageData == null)
            {
                MessageBox.Show("No data. Something's Wrong.");
                throw new DataNotArrivedFromHardwareControllerException();
            }
        }
        #endregion

        #region Getting an Experiment Report
        /// <summary>
        /// This is the mechanism for saving experimental parameters which MM doesn't control, but that the hardware controller can monitor
        /// (e.g. oven temperature, vacuum chamber pressure etc).
        /// </summary>

        public Dictionary<String, Object> GetExperimentReport()
        {
            return experimentReporter.GetExperimentReport();
        }

        #endregion

        #region Translation stage
        private void armTranslationStageForTimedMotion(MOTMasterScript script)
        {
            tstage.TSConnect();
            Thread.Sleep(50);
            tstage.TSInitialize((double)script.Parameters["TSAcceleration"], (double)script.Parameters["TSDeceleration"],
                (double)script.Parameters["TSDistance"], (double)script.Parameters["TSVelocity"]);
            Thread.Sleep(50);
            tstage.TSOn();
            Thread.Sleep(50);
            tstage.TSAutoTriggerDisable();
            Thread.Sleep(50);
            tstage.TSGo();
        }
        private void disarmAndReturnTranslationStage()
        {
            tstage.TSAutoTriggerEnable();
            Thread.Sleep(50);
            tstage.TSReturn(); // This is the hard coded return of the translation stage at the end of running a MM script
            Thread.Sleep(50);
            tstage.TSDisconnect();
        }
        #endregion

        #region Re-Running a script (intended for reloading old scripts)

        /// <summary>
        /// This section is meant to be for the situation when you want to re-run exactly the same pattern
        /// you ran sometime in the past.
        /// armReplicaRun prompts you for a zip file which contains the run you want to replicate. It unzipps the
        /// file into a folder of the same name, picks out the dictionary and the script.
        /// These then get loaded in the usual way through Run().
        /// disposeReplicaRun does some clean up after the experiment is finished.
        /// </summary>

        public void RunReplica()
        {
            armReplicaRun();
            Run();
            disposeReplicaRun();
        }

        private void armReplicaRun()
        {
            string zipPath = ioHelper.SelectSavedScriptPathDialog();
            string outputFolderPath = Path.GetDirectoryName(zipPath) + "\\" +
                Path.GetFileNameWithoutExtension(zipPath) + "\\";

            ioHelper.UnzipFolder(zipPath);
            SetScriptPath(outputFolderPath +
                Path.GetFileNameWithoutExtension(zipPath) + ".cs");

            SetDictionaryPath(outputFolderPath +
                Path.GetFileNameWithoutExtension(zipPath) + "_parameters.txt");

            SetReplicaRunBool(true);

        }

        private void disposeReplicaRun()
        {
            SetReplicaRunBool(false);
            ioHelper.DisposeReplicaScript(Path.GetDirectoryName(scriptPath));
        }
        #endregion

        #region Remotable Stuff from python

        public void RemoteRun(string scriptName, Dictionary<String, Object> parameters, bool save)
        {
            scriptPath = scriptName;
            saveEnable = save;
            status = RunningState.running;
            Run(parameters);
        }

        public void CloseIt()
        {
            //controllerWindow.Close();
        }

        public void SetSaveDirectory(string saveDirectory)
        {
            updateSaveDirectory(saveDirectory);
        }

        public string getSaveDirectory()
        {
            return saveToDirectory;
        }

        #endregion

        #region Environment Loading
        public void LoadEnvironment()
        {

            string fileJson = File.ReadAllText("filesystem.json");
            string hardwareJson = File.ReadAllText("hardware.json");
            string configJson = File.ReadAllText("config.json");

            LoadEnvironment(fileJson, hardwareJson, configJson);
        }

        public void LoadEnvironment(string fileJson, string hardwareJson, string configJson)
        {
            DAQ.Environment.Environs.FileSystem = JsonConvert.DeserializeObject<DAQ.Environment.FileSystem>(fileJson);
            DAQ.Environment.Environs.Hardware = JsonConvert.DeserializeObject<DAQ.HAL.NavigatorHardware>(hardwareJson);
            config = JsonConvert.DeserializeObject<MMConfig>(configJson);
        }
        public void SaveEnvironment()
        {
            string fileJson = JsonConvert.SerializeObject(DAQ.Environment.Environs.FileSystem, Formatting.Indented);
            string hardwareJson = JsonConvert.SerializeObject(DAQ.Environment.Environs.Hardware, Formatting.Indented);
            string configJson = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText("filesystem.json", fileJson);
            File.WriteAllText("hardware.json", hardwareJson);
            File.WriteAllText("config.json", configJson);
        }

        public void LoadDefaultSequence()
        {
            if (File.Exists(defaultScriptPath)) LoadSequenceFromPath(defaultScriptPath);
        }

        public void LoadSequenceFromPath(string path)
        {
            string sequenceJson = File.ReadAllText(path);
            sequenceData = JsonConvert.DeserializeObject<Sequence>(sequenceJson);
            //script.Parameters = sequenceData.CreateParameterDictionary();

        }
        public void SaveSequenceAsDefault(List<SequenceStep> steps)
        {
            SaveSequenceToPath(defaultScriptPath, steps);
        }

        public void SaveSequenceToPath(string path, List<SequenceStep> steps)
        {
           
            if (sequenceData == null)
            {
                sequenceData = new Sequence();
                sequenceData.CreateParameterList(script.Parameters);
                sequenceData.Steps = steps;
            }
            string sequenceJson = JsonConvert.SerializeObject(sequenceData, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            File.WriteAllText(path, sequenceJson);
        }

        public void SaveSequenceToPath(string path)
        {
            string sequenceJson = JsonConvert.SerializeObject(sequenceData, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            File.WriteAllText(path, sequenceJson);
        }
        #endregion

        #region Cicero Sequence Loading

        internal void LoadCiceroSequenceFromPath(string filename)
        {
            ciceroSequence = (DataStructures.SequenceData)DataStructures.Common.loadBinaryObjectFromFile(filename);

        }

        internal void LoadCiceroSettingsFromPath(string filename)
        {
            ciceroSettings = (DataStructures.SettingsData)DataStructures.Common.loadBinaryObjectFromFile(filename);
        }

        internal void ConvertCiceroSequence()
        {

            CiceroConverter ciceroConverter = new CiceroConverter();

            ciceroConverter.SetSettingsData(ciceroSettings);
            ciceroConverter.InitMMSequence(sequenceData);

            if (ciceroConverter.CheckValidHardwareChannels() && ciceroConverter.CanConvertFrom(ciceroSequence.GetType())) sequenceData = (Sequence)ciceroConverter.ConvertFrom(ciceroSequence);
        }

        #endregion

        #region Saving and Processing Experiment Data
        public void StartLogging()
        {
            string now = DateTime.Now.ToString("yyMMdd_hhmmss");
            string fileTag = motMasterDataPath + "/" + ExpData.ExperimentName + "_" + now;
            dataLogger = new AutoFileLogger(fileTag + "_data.ahf");
            paramLogger = new AutoFileLogger(fileTag + "_parameters.ahf");
            dataLogger.Enabled = true;
            paramLogger.Enabled = true;
            dataLogger.log("{\"MMbatch\":[");
            paramLogger.log("{\"MMbatch\":[");
        }
        public void StopLogging()
        {
            //Finishes writing the JSONs. Removes the last comma since Mathematica has issues with it
            dataLogger.DropLastChar();
            paramLogger.DropLastChar();
            dataLogger.log("]\n}");
            paramLogger.log("]\n}");
            dataLogger.Enabled= false;
            paramLogger.Enabled = false;
        }
        //This is very specific to the Navigator experiment. Assumes that the acqusition trigger channel is high during each segment that the data is recorded during 
        public void CreateAcquisitionTimeSegments()
        {
            if (!Environs.Hardware.DigitalOutputChannels.ContainsKey("acquisitionTrigger")) throw new WarningException("No channel named acquisitionTrigger found in Hardware");
            Dictionary<string, Tuple<int, int>> analogSegments = new Dictionary<string,Tuple<int,int>>();
            int sampleRate = ExpData.SampleRate;
            int sampleStartTime = ExpData.PreTrigSamples;
            List<string> ignoredSegments = new List<string>();
            ignoredSegments = sequenceData.Steps.Where(t => (t.Description.Contains("DNS") && t.GetDigitalData("acquisitionTrigger"))).Select(t => t.Name).ToList();
            ExpData.IgnoredSegments = ignoredSegments;
            try
            {
            ExpData.InterferometerStepName = sequenceData.Steps.Where(t => (t.Description.Contains("Interferometer") && t.GetDigitalData("acquisitionTrigger"))).Select(t => t.Name).First();
            }
            catch
            {
                ErrorMgr.errorMsg("No step named Interferometer.",-2);
            }
            foreach (SequenceStep step in sequenceData.Steps)
            {
                if (!step.GetDigitalData("acquisitionTrigger")) continue;
                if (ignoredSegments.Count == 0) throw new WarningException("All acquired data will be saved.");
                double timeMultiplier = 1.0;
                if (step.Timebase == TimebaseUnits.ms) timeMultiplier = 1e-3;
                else if (step.Timebase == TimebaseUnits.us) timeMultiplier = 1e-6;
                else if (step.Timebase == TimebaseUnits.s) timeMultiplier = 1.0;
                double duration = Convert.ToDouble(step.Duration);
                int sampleDuration = Convert.ToInt32(duration*timeMultiplier*sampleRate);
                string name = step.Name;
                Tuple<int, int> segmentTimes = Tuple.Create<int,int>(sampleStartTime, sampleStartTime + sampleDuration );
                analogSegments[name] = segmentTimes;
                sampleStartTime += sampleDuration;
            }
            ExpData.AnalogSegments = analogSegments;
            ExpData.NSamples = sampleStartTime;
        }

        public MMexec ConvertDataToAxelHub(double[,] aiData)
        {
            MMexec axelCommand = new MMexec();
            axelCommand.sender = "MOTMaster";
            axelCommand.cmd = "shotData";
            Dictionary<string,double[]> segData = ExpData.SegmentShot(aiData);
            foreach (KeyValuePair<string, double[]> item in segData) axelCommand.prms[item.Key] = item.Value;
            axelCommand.prms["runID"] = batchNumber;
            axelCommand.prms["groupID"] = ExpData.ExperimentName;
            return axelCommand;
        }

        public MMexec InitialCommand(MMscan? scan)
        {
            MMexec axelCommand = new MMexec();
            axelCommand.sender = "MOTMaster";
 
            axelCommand.mmexec = "";
            axelCommand.prms["params"] = sequenceData.CreateParameterDictionary();
            axelCommand.prms["sampleRate"] = ExpData.SampleRate;
            axelCommand.prms["runID"] = batchNumber;
            axelCommand.prms["groupID"] = ExpData.ExperimentName;
            if (scan != null)
            {
                MMscan s2 = (MMscan)scan;
                s2.ToDictionary(ref axelCommand.prms);
               // axelCommand.prms["scanParam"] = scanParam;
                axelCommand.cmd = "scan";
            }
            else
            {
                axelCommand.cmd = "repeat";
                axelCommand.prms["cycles"] = numInterations;
            }
            return axelCommand;
        }

        #endregion

        #region MSquared Control - Maybe move elswhere?
        public void GetMSquaredParameters()
        {
            var rawData = config.Debug ? ExpData.GenerateFakeData() : aip.GetAnalogData();
            MMexec finalData = ConvertDataToAxelHub(rawData);
            string dataJson = JsonConvert.SerializeObject(finalData, Formatting.Indented);
            dataLogger.log("{\"MMExec\":" + dataJson + "},");
            if (SendDataRemotely)
            {
                if (MotMasterDataEvent != null) MotMasterDataEvent(this, new DataEventArgs(dataJson));
            }
        }

        public static void SetMSquaredParameters(bool pulse1Enabled = true, bool pulse2Enabled = true, bool pulse3Enabled = true, bool velPulseEnabled = true, double intTime1 = 25.0, double intTime2 = 25.0)
            {
            if (!M2DCS.Connected || !M2PLL.Connected)
                {
                if(!config.Debug) ErrorMgr.warningMsg("Not connected to ICE-Blocs");
                return;
                }
            CheckPhaseLock();
            M2PLL.configure_lo_profile(true, false, "ecd", (double)sequenceData.Parameters["PLLFreq"].Value*1e6, 0.0, (double)sequenceData.Parameters["ChirpRate"].Value*1e6, (double)sequenceData.Parameters["ChirpDuration"].Value, true);
            //Checks the phase lock has not come out-of-loop
            CheckPhaseLock();

            M2DCS.ConfigurePulse("X", 0, sequenceData.Parameters["VelPulseDuration"].Value, sequenceData.Parameters["VelPulsePower"].Value, 1e-6, sequenceData.Parameters["VelPulsePhase"].Value,pulse1Enabled);
            M2DCS.ConfigurePulse("X", 1, sequenceData.Parameters["Pulse1Duration"].Value, sequenceData.Parameters["Pulse1Power"].Value, 1e-6, sequenceData.Parameters["Pulse1Phase"].Value,pulse2Enabled);
            M2DCS.ConfigureIntTime(1, intTime1);
            M2DCS.ConfigurePulse("X", 2, sequenceData.Parameters["Pulse2Duration"].Value, sequenceData.Parameters["Pulse2Power"].Value, 1e-6, sequenceData.Parameters["Pulse2Phase"].Value,pulse3Enabled);
            M2DCS.ConfigureIntTime(2, intTime2);
            M2DCS.ConfigurePulse("X", 3, sequenceData.Parameters["Pulse3Duration"].Value, sequenceData.Parameters["Pulse3Power"].Value, 1e-6, sequenceData.Parameters["Pulse3Phase"].Value,velPulseEnabled);

            M2DCS.UpdateSequenceParameters();
            }

        private static bool CheckPhaseLock()
        {
            DAQ.HAL.ICEBlocPLL.Lock_Status lockStatus = new DAQ.HAL.ICEBlocPLL.Lock_Status();
            bool locked = M2PLL.main_lock_status(out lockStatus);
            if (!locked) ErrorMgr.errorMsg("PLL lock is not engaged - currently " + lockStatus.ToString(),10,false);
            return locked;
        }
        #endregion



        internal static void SetParameter(string key, object p)
        {
            if (sequenceData.Parameters.ContainsKey(key)) sequenceData.Parameters[key].Value = p;
            else sequenceData.Parameters[key] = new Parameter(key, "", p, true, false);
        }
    }

    public class DataEventArgs : EventArgs
    {
        public object Data { get; set; }

        public DataEventArgs(object data) : base()
        {
            Data = data;
        }
    }
}
