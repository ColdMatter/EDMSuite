using DAQ;
using DAQ.Analog;
using DAQ.Environment;
using DAQ.HAL;
using Microsoft.CSharp;
using MOTMaster2.SequenceData;
//using NationalInstruments.UI.WindowsForms;
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
using DataStructures;
using System.Runtime.Serialization.Formatters.Binary;

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

        DAQMxPatternGenerator pg;
        HSDIOPatternGenerator hs;
        DAQMxPatternGenerator PCIpg;
        DAQMxAnalogPatternGenerator apg;
        MMAIWrapper aip;


        CameraControllable camera = null;
        TranslationStageControllable tstage = null;
        ExperimentReportable experimentReporter = null;

        MuquansController muquans = null;

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
            if (!config.HSDIOCard) pg = new DAQMxPatternGenerator((string)Environs.Hardware.Boards["analog"]);
            else hs = new HSDIOPatternGenerator((string)Environs.Hardware.Boards["hsDigital"]);
            apg = new DAQMxAnalogPatternGenerator();
            PCIpg = new DAQMxPatternGenerator((string)Environs.Hardware.Boards["multiDAQPCI"]);
            aip = new MMAIWrapper((string)Environs.Hardware.Boards["multiDAQPCI"]);
            analogChannels =
            digitalChannels = Environs.Hardware.DigitalOutputChannels.Keys.Cast<string>().ToList();

            if (config.CameraUsed) camera = (CameraControllable)Activator.GetObject(typeof(CameraControllable),
                "tcp://localhost:1172/controller.rem");

            if (config.TranslationStageUsed) tstage = (TranslationStageControllable)Activator.GetObject(typeof(CameraControllable),
                "tcp://localhost:1172/controller.rem");

            if (config.ReporterUsed) experimentReporter = (ExperimentReportable)Activator.GetObject(typeof(ExperimentReportable),
                "tcp://localhost:1172/controller.rem");

            if (config.UseMuquans) muquans = new MuquansController();

            ioHelper = new MMDataIOHelper(motMasterDataPath,
                    (string)Environs.Hardware.GetInfo("Element"));

            ScriptLookupAndDisplay();
        }

        #endregion

        #region Hardware control methods


        private void run(MOTMasterSequence sequence)
        {
            if (config.UseMuquans)
                muquans.StartOutput();
            apg.OutputPatternAndWait(sequence.AnalogPattern.Pattern);
            if (config.UseAI) aip.StartTask();
            if (!config.HSDIOCard) pg.OutputPattern(sequence.DigitalPattern.Pattern, true);
            else
            {
                int[] loopTimes = ((DAQ.Pattern.HSDIOPatternBuilder)sequence.DigitalPattern).LoopTimes;
                hs.OutputPattern(sequence.DigitalPattern.Pattern, loopTimes);
            }

        }
        private void initializeHardware(MOTMasterSequence sequence)
        {
            if (!config.HSDIOCard) pg.Configure(config.DigitalPatternClockFrequency, false, true, true, sequence.DigitalPattern.Pattern.Length, true, false);
            else hs.Configure(config.DigitalPatternClockFrequency, false, true, false);
            if (config.UseMuquans) muquans.Configure();
            apg.Configure(sequence.AnalogPattern, config.AnalogPatternClockFrequency, false);
        }


        private void releaseHardware()
        {
            if (!config.HSDIOCard) pg.StopPattern();
            else hs.StopPattern();
            apg.StopPattern();
            if (config.UseAI) aip.StopPattern();
            if (config.UseMuquans) muquans.StopOutput();
        }

        private void clearDigitalPattern(MOTMasterSequence sequence)
        {
            sequence.DigitalPattern.Clear(); //No clearing required for analog (I think).
        }
        private void releaseHardwareAndClearDigitalPattern(MOTMasterSequence sequence)
        {
            clearDigitalPattern(sequence);
            releaseHardware();
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
            //batchNumber = number;
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
        public void RunStart(Dictionary<string,object> paramDict)
        {
            runThread = new Thread(new ParameterizedThreadStart(this.Run));
            
            runThread.Name = "MOTMaster Controller";
            runThread.Priority = ThreadPriority.Highest;
            status = RunningState.running;
            runThread.Start(paramDict);
            Console.WriteLine("Thread Starting");

        }
        public void WaitForRunToFinish()
        {
            runThread.Join();
            Console.WriteLine("Thread Waiting");
        }

        public void Run()
        {
            status = RunningState.running;
            if (replicaRun)
            {
                Run(ioHelper.LoadDictionary(dictionaryPath));
            }
            else
            {
                Run(null);
            }
        }

        public void Run(Dictionary<String, Object> dict)
        {
            Run(dict, 1, 0);
        }

        public void Run(object dict)
        {
            Run((Dictionary<string,object>)dict, 1, 0);
        }
        //TODO Change this to handle Sequences and Scripts built using SequenceData

        public void Run(Dictionary<String, Object> dict, int numInterations, int batchNumber)
        {
            Stopwatch watch = new Stopwatch();
            if (config.UseMMScripts || sequenceData == null)
            {
                script = prepareScript(scriptPath, dict);
                sequence = getSequenceFromScript(script);
            }
            else
            {
                sequence = getSequenceFromSequenceData(dict);
            }

            if (sequence != null)
            {

                try
                {
                    if (config.CameraUsed) prepareCameraControl();

                    if (config.TranslationStageUsed) armTranslationStageForTimedMotion(script);

                    if (config.CameraUsed) GrabImage((int)script.Parameters["NumberOfFrames"]);

                    
                    if (config.UseMMScripts) buildPattern(sequence, (int)script.Parameters["PatternLength"]);
                    else buildPattern(sequence, (int)builder.Parameters["PatternLength"]);

                    if (config.CameraUsed) waitUntilCameraIsReadyForAcquisition();

                    watch.Start();

                    for (int i = 0; i < numInterations && status == RunningState.running; i++)
                    {
                        if (!config.Debug) runPattern(sequence);
                        if (i == 0)
                        {
                            if (Environs.FileSystem.Paths.Contains("DataPath"))
                            {
                                Console.WriteLine("yes");
                            }
                            string filepath = (string)(Environs.FileSystem.Paths["DataPath"]);
                            ioHelper.SaveRawSequence(filepath, i, sequence);
                        }
                    }
                    if (!config.Debug || config.UseMMScripts)clearDigitalPattern(sequence);

                    watch.Stop();
                    //    MessageBox.Show(watch.ElapsedMilliseconds.ToString());
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
                                save(script, scriptPath, imageData, report, batchNumber);
                            }

                        }
                        else
                        {
                            Dictionary<String, Object> report = new Dictionary<string, object>();
                            if (config.ReporterUsed)
                            {
                                report = GetExperimentReport();
                                //TODO Change save method
                                save(script, scriptPath, report, batchNumber);
                            }
                        }


                    }
                    if (config.CameraUsed) finishCameraControl();
                    if (config.TranslationStageUsed) disarmAndReturnTranslationStage();


                }
                catch (System.Net.Sockets.SocketException e)
                {
                    MessageBox.Show("CameraControllable not found. \n Is there a hardware controller running? \n \n" + e.Message, "Remoting Error");
                }
            }
            else
            {
                MessageBox.Show("Sequence not found. \n Check that it has been built using the datagrid or loaded from a script.");
            }

            status = RunningState.stopped;


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
        private void runPattern(MOTMasterSequence sequence)
        {

            initializeHardware(sequence);
            try
            {
                run(sequence);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error when running sequence. Continuing and releasing hardware...");
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
            }
            releaseHardware();
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
            if (paramDict!=null)builder.EditDictionary(paramDict);
            builder.BuildSequence();
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
            // Just for testing purposes!!!
            config.UseMuquans = false;
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
    }
}
