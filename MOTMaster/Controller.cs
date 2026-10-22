using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using Microsoft.CSharp;

using DAQ;
using DAQ.Environment;
using DAQ.HAL;
using DAQ.Analog;
using Data;
using Data.Scans;


//using IMAQ;

using System.Runtime.InteropServices;
using System.CodeDom;
using System.CodeDom.Compiler;

using NationalInstruments;
//using NationalInstruments.DAQmx; TW: I have commented this out becaus e it's stopping the moleculemot system building and it doesn't seem to be used. Uncomment if it breaks your system
using System.Runtime.Serialization.Formatters.Binary;

namespace MOTMaster
{
    /// <summary>
    /// Here's MOTMaster's controller.
    /// 
    /// Gets a MOTMasterScript (a script contaning a series of commands like "addEdge" for both digital and analog)
    /// from user (either remotely or via UI), compiles it, builds a pattern and sends it
    /// to hardware.
    /// </summary>
    public class Controller : MarshalByRefObject, ILinkSecondary
    {

        #region Class members

        private static string motMasterPath = (string)Environs.FileSystem.Paths["MOTMasterEXEPath"] + "//MotMaster.exe";
        private static string daqPath = (string)Environs.FileSystem.Paths["daqDLLPath"];
        private static string scriptListPath = (string)Environs.FileSystem.Paths["scriptListPath"];
        private static string motMasterDataPath = (string)Environs.FileSystem.Paths["MOTMasterDataPath"];
        private static string cameraAttributesPath = (string)Environs.FileSystem.Paths["CameraAttributesPath"];
        private static string hardwareClassPath = (string)Environs.FileSystem.Paths["HardwareClassPath"];
        private static string externalFilesPath = (string)Environs.FileSystem.Paths["ExternalFilesPath"];



        private MMConfig config = (MMConfig)Environs.Hardware.GetInfo("MotMasterConfiguration");

        private Thread runThread;

        public enum RunningState { stopped, running };
        public RunningState status = RunningState.stopped;
        public bool triggered = false;
        string pgMasterName;

        ControllerWindow controllerWindow;

        DAQMxPatternGenerator pgMaster;
        Dictionary<string, DAQMxPatternGenerator> pgs;
        Dictionary<string, DAQMxAnalogPatternGenerator> analogs;
        Dictionary<string, DAQMxAnalogStaticGenerator> staticAnalogs;
        Dictionary<string, string> analogBoards;
        Dictionary<string, string> staticAnalogBoards;

        CameraControllable camera = null;
        TranslationStageControllable tstage = null;
        ExperimentReportable experimentReporter = null;

        public MMDataIOHelper ioHelper;

#if DDS
        /// <summary>
        /// Proxy to SpectrumDDSController, which owns the Spectrum card handle in
        /// its own process so that manual control works with MOTMaster closed.
        /// Only the CaF configuration defines DDS, so no other experiment sees any
        /// of this.
        /// </summary>
        private SpectrumDDSController.Controller ddsController;
#endif //DDS

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
            controllerWindow = new ControllerWindow();
            controllerWindow.controller = this;

            pgMasterName = (string)Environs.Hardware.GetInfo("PatternGeneratorBoard");
            pgMaster = new DAQMxPatternGenerator(pgMasterName);
            Dictionary<string, string> additionalPGs = (Dictionary<string, string>)Environs.Hardware.GetInfo("AdditionalPatternGeneratorBoards");
            pgs = new Dictionary<string, DAQMxPatternGenerator>();
            if (additionalPGs != null)
            {
                foreach (string address in additionalPGs.Keys)
                {
                    pgs[address] = new DAQMxPatternGenerator(address);
                }
            }

            analogBoards = (Dictionary<string, string>)Environs.Hardware.GetInfo("AnalogBoards");
            analogs = new Dictionary<string, DAQMxAnalogPatternGenerator>();
            if (analogBoards != null)
            {
                foreach (string address in analogBoards.Values)
                {
                    analogs[address] = new DAQMxAnalogPatternGenerator();
                }
            }

            staticAnalogBoards = (Dictionary<string, string>)Environs.Hardware.GetInfo("StaticAnalogBoards");
            staticAnalogs = new Dictionary<string, DAQMxAnalogStaticGenerator>();
            if (staticAnalogBoards != null)
            {
                foreach (string address in staticAnalogBoards.Values)
                {
                    staticAnalogs[address] = new DAQMxAnalogStaticGenerator();
                }
            }


            //if (config.CameraUsed) camera = (CameraControllable)Activator.GetObject(typeof(CameraControllable),
            //    "tcp://localhost:1172/controller.rem");

            //if (config.TranslationStageUsed) tstage = (TranslationStageControllable)Activator.GetObject(typeof(CameraControllable),
            //    "tcp://localhost:1172/controller.rem");

            //if (config.ReporterUsed) experimentReporter = (ExperimentReportable)Activator.GetObject(typeof(ExperimentReportable),
            //"tcp://172.22.116.195:1172/controller.rem");
            if (config.ReporterUsed) experimentReporter = (ExperimentReportable)Activator.GetObject(typeof(ExperimentReportable),
            "tcp://127.0.0.1:1172/controller.rem");

      
            ioHelper = new MMDataIOHelper(motMasterDataPath,
                    (string)Environs.Hardware.GetInfo("Element"));

#if DDS
            // Just a proxy: nothing connects until it is first used, so MOTMaster
            // still starts when SpectrumDDSController is not running.
            ddsController = (SpectrumDDSController.Controller)Activator.GetObject(
                typeof(SpectrumDDSController.Controller),
                "tcp://127.0.0.1:1818/controller.rem");
#endif //DDS

            // Experiments that don't declare safe outputs get none of this, as before.
            safeOutputs = DAQMxAnalogSafeState.FromHardware();
            List<string> safeOutputProblems = DAQMxAnalogSafeState.Problems(safeOutputs);
            if (safeOutputProblems.Count > 0)
                MessageBox.Show("The safe outputs declared in the Hardware class can't be used, so they will "
                    + "NOT be set after runs until this is fixed:\n\n" + string.Join("\n", safeOutputProblems),
                    "MOTMaster: safe outputs", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            // Closing the window mid-run: end the run so its finally sets the safe outputs.
            // The run thread is a foreground thread, so the process waits for it.
            Application.ApplicationExit += (s, e) => { status = RunningState.stopped; };

            // Some other thread's exception is about to take the process down. Best
            // effort: don't leave the coils wherever the pattern was.
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                SharedCode.AppLog.Error("MOTMaster unhandled exception", e.ExceptionObject as Exception);
                status = RunningState.stopped;
                setSafeOutputs();
            };

            ScriptLookupAndDisplay();

            Application.Run(controllerWindow);

        }

        #endregion

        #region Hardware control methods


        /// <param name="sleepAfterStart">
        /// True (every non-linked run) keeps pgMaster.OutputPattern's sleep of one pattern length after the
        /// task starts. The linked secondary passes false so it can report "armed" as soon as its task is
        /// waiting for the trigger.
        /// </param>
        private void run(MOTMasterSequence sequence, bool sleepAfterStart = true)
        {
            foreach (string address in analogs.Keys)
            {
                if (sequence.AnalogPattern.Boards.ContainsKey(address))
                {
                    analogs[address].OutputPatternAndWait(sequence.AnalogPattern.Boards[address].Pattern);
                }

            }

            foreach (string address in staticAnalogs.Keys)
            {
                if (sequence.AnalogStatic.Boards.ContainsKey(address))
                {
                    staticAnalogs[address].OutputStaticValueAndWait(sequence.AnalogStatic.Boards[address].StaticPattern);
                }

            }

            foreach (string address in pgs.Keys)
            {
                if (sequence.DigitalPattern.Boards.ContainsKey(address))
                    pgs[address].OutputPattern(sequence.DigitalPattern.Boards[address].Pattern, false);
            }
            pgMaster.OutputPattern(sequence.DigitalPattern.Boards[pgMasterName].Pattern, sleepAfterStart);

        }

        /// <param name="forceTriggered">True on the linked secondary, which always waits for the primary's trigger.</param>
        private void initializeHardware(MOTMasterSequence sequence, bool forceTriggered = false)
        {
            if (triggered == true || forceTriggered)
            {
                pgMaster.Configure(config.DigitalPatternClockFrequency, false, true, true, sequence.DigitalPattern.Boards[pgMasterName].Pattern.Length, true, true);
            }
            else
            {
                pgMaster.Configure(config.DigitalPatternClockFrequency, false, true, true, sequence.DigitalPattern.Boards[pgMasterName].Pattern.Length, true, false);
            }

            int i = 0;

            foreach (string address in pgs.Keys)
            {
                if (sequence.DigitalPattern.Boards.ContainsKey(address))
                    pgs[address].Configure("PGSlave" + i.ToString(), config.DigitalPatternClockFrequency, false, true, true, sequence.DigitalPattern.Boards[address].Pattern.Length, false, true);
                i++;
            }

            int j = 0;

            if (analogBoards != null)
            {
                foreach (KeyValuePair<string, string> kvp in analogBoards)
                {
                    if (sequence.AnalogPattern.Boards.ContainsKey(kvp.Value))
                    {
                        analogs[kvp.Value].Configure(
                                kvp.Key,
                                sequence.AnalogPattern.Boards[kvp.Value],
                                config.AnalogPatternClockFrequency, false, true);
                        j++;
                    }

                }
            }
            int k = 0;
            if (staticAnalogBoards != null)
            {
                foreach (KeyValuePair<string, string> kvp in staticAnalogBoards)
                {
                    if (sequence.AnalogStatic.Boards.ContainsKey(kvp.Value))
                    {
                        staticAnalogs[kvp.Value].Configure(
                                kvp.Key,
                                sequence.AnalogStatic.Boards[kvp.Value],
                                config.AnalogPatternClockFrequency, false, true);
                        k++;
                    }
                }
            }
        }

        private void releaseHardware()
        {
            pgMaster.StopPattern();
            foreach (DAQMxPatternGenerator pg in pgs.Values)
            {
                pg.StopPattern();
            }
            foreach (DAQMxAnalogPatternGenerator apg in analogs.Values)
            {
                apg.StopPattern();
            }
        }
        private void clearDigitalPattern(MOTMasterSequence sequence)
        {
            sequence.DigitalPattern.Boards[pgMasterName].Clear(); //No clearing required for analog (I think).
            foreach (string address in pgs.Keys)
            {
                if (sequence.DigitalPattern.Boards.ContainsKey(address))
                    sequence.DigitalPattern.Boards[address].Clear();
            }
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
            controllerWindow.FillScriptComboBox(s);
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
            saveEnable = value;
            controllerWindow.SetSaveCheckBox(value);
        }

        private int batchNumber = 0;
        public void SetBatchNumber(Int32 number)
        {
            batchNumber = number;
            controllerWindow.WriteToSaveBatchTextBox(number);
        }
        public void SetIterations(Int32 number)
        {
            controllerWindow.SetIterations(number);
        }
        public void SetRunUntilStopped(bool state)
        {
            controllerWindow.RunUntilStoppedState = state;
        }
        private string scriptPath = "";
        public void SetScriptPath(String path)
        {
            secondaryState.ThrowIfClaimed("SetScriptPath");
            scriptPath = path;
            controllerWindow.WriteToScriptPath(path);
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

        public void SetTriggered(System.Boolean value)
        {
            secondaryState.ThrowIfClaimed("SetTriggered");
            triggered = value;
            controllerWindow.TriggeredCheckBoxState = value;
        }

        public Dictionary<string, List<object>> GetSwitchConfiguration()
        {
            return prepareScript(scriptPath, new Dictionary<string, object> { }).switchConfiguration;
        }

        public Dictionary<string, object> GetParameters()
        {
            return prepareScript(scriptPath, new Dictionary<string, object> { }).Parameters;
        }

        public void ViewPattern()
        {
            MOTMasterScript script = prepareScript(scriptPath, null);
            MOTMasterSequence sequence = getSequenceFromScript(script);
            buildPattern(sequence, (int)script.Parameters["PatternLength"]);

            var digitalChannels = new List<(string, uint[])>
            {
                ("PG",  sequence.DigitalPattern.Boards[pgMasterName].Pattern),
            };

            var digitalNames = new List<(string, SortedList<int, string>)>
            {
                ("PG",  GenerateListOfDigitalChannelNames(pgMasterName)),
            };

            foreach (string address in pgs.Keys)
            {
                if (sequence.DigitalPattern.Boards.ContainsKey(address))
                {
                    digitalChannels.Add((address, sequence.DigitalPattern.Boards[address].Pattern));
                    digitalNames.Add((address, GenerateListOfDigitalChannelNames(address)));
                }
            }

            var analogChannels = new List<(string, double[,])>();
            var analogNames = new List<(string, SortedList<int, string>)>();

            foreach (string address in analogs.Keys)
            {
                if (sequence.AnalogPattern.Boards.ContainsKey(address))
                {
                    analogChannels.Add((address, sequence.AnalogPattern.Boards[address].Pattern));
                    var names = new SortedList<int, string>();
                    ICollection<string> keys = sequence.AnalogPattern.Boards[address].AnalogPatterns.Keys;
                    int i = 0;
                    foreach (string key in keys)
                    {
                        names.Add(i, key);
                        i++;
                    }
                    analogNames.Add((address, names));
                }
            }

            using (var viewer = new PatternViewer(digitalChannels, analogChannels, digitalNames, analogNames))
            {
                viewer.ShowDialog(controllerWindow);
            }
        }

        public SortedList<int, string> GenerateListOfDigitalChannelNames(string boardAddress)
        {
            // Some hardware configs define more than one named channel on the same
            // board/bit (e.g. an old channel left registered as "broken" alongside
            // its replacement) - combine the names rather than throwing on the
            // duplicate key.
            SortedList<int, string> digitalChannelNames = new SortedList<int, string>();
            foreach (DictionaryEntry channel in Environs.Hardware.DigitalOutputChannels)
            {
                DigitalOutputChannel digitalOutputChannel = (DigitalOutputChannel)channel.Value;
                if (digitalOutputChannel.Device == boardAddress)
                {
                    int bitNumber = digitalOutputChannel.BitNumber;
                    string name = (string)channel.Key;
                    if (digitalChannelNames.ContainsKey(bitNumber))
                    {
                        digitalChannelNames[bitNumber] = digitalChannelNames[bitNumber] + "/" + name;
                    }
                    else
                    {
                        digitalChannelNames.Add(bitNumber, name);
                    }
                }
            }
            return digitalChannelNames;
        }

        public void Run()
        {
            secondaryState.ThrowIfClaimed("Run");
            runThread = new Thread(() => goReportingErrors(() => Go()));
            runThread.Name = "MOTMaster Controller";
            runThread.Priority = ThreadPriority.Normal;

            runThread.Start();
        }

        public Thread Run(Dictionary<String, Object> dict)
        {
            secondaryState.ThrowIfClaimed("Run");
            var t = new Thread(() => goReportingErrors(() => Go(dict)));
            // status = RunningState.running;
            t.Start();
            //t.Join(); //Blocks calling thread until finished so that doesn't return until finished
            return null;
        }

        public void Stop()
        {
            status = RunningState.stopped;
        }

        public void Go()
        {
            if (replicaRun)
            {
                Go(ioHelper.LoadDictionary(dictionaryPath));
            }
            else
            {
                Go(null);
            }
        }


        /// <summary>
        /// True from the moment a run claims the hardware until it has fully finished,
        /// including saving and clearing the patterns. Deliberately separate from
        /// status: the Stop button sets status to stopped while the old run is still
        /// winding down, and a second run must not start in that gap.
        /// </summary>
        public bool IsRunning
        {
            get { lock (runLock) { return runInProgress; } }
        }
        private readonly object runLock = new object();
        private bool runInProgress = false;
        private Dictionary<string, double> safeOutputs;

        public void Go(Dictionary<String, Object> dict)
        {
            secondaryState.ThrowIfClaimed("Go");
            // Only one run may drive the pattern generators at a time. Go is reached
            // from the Run button (on its own thread) and directly over remoting from
            // Python, so the claim has to be atomic and live here, not in Run().
            lock (runLock)
            {
                if (runInProgress)
                {
                    SharedCode.AppLog.Write("Controller.Go", "A run is already in progress, request ignored.");
                    return;
                }
                runInProgress = true;
            }
            controllerWindow.RefreshRunControls();
            try
            {
                goClaimed(dict);
            }
            catch (Exception ex)
            {
                // Logged here, then rethrown so a remote (Python) caller still sees it.
                SharedCode.AppLog.Error("Controller.Go", ex);
                // MOTMaster now survives this, so free the tasks for every experiment:
                // left reserved, they would make the next run fail.
                stopPatternTasksQuietly();
                throw;
            }
            finally
            {
                // Before releasing the claim, so no new run can start in between.
                setSafeOutputs();
                status = RunningState.stopped;
                lock (runLock) { runInProgress = false; }
                controllerWindow.RefreshRunControls();
            }
        }

        /// <summary>
        /// Body of a thread started by Run(). An exception escaping a thread kills the
        /// whole process, so report it instead. Go's finally has already made the
        /// outputs safe by the time this sees it.
        /// </summary>
        private void goReportingErrors(Action go)
        {
            try
            {
                go();
            }
            catch (Exception ex)
            {
                MessageBox.Show("The run failed:\n\n" + ex.Message, "MOTMaster run error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Stop every pattern task and drive the experiment's safe outputs (if it declares
        /// any in its Hardware class) to their safe values. A task stopped part-way leaves
        /// its outputs at whatever value they had, so this runs however the run ended.
        /// </summary>
        private void setSafeOutputs()
        {
            if (safeOutputs == null || safeOutputs.Count == 0 || config.Debug) return;

            stopPatternTasksQuietly();

            try
            {
                DAQMxAnalogSafeState.SetSafeValues(safeOutputs);
            }
            catch (Exception ex)
            {
                SharedCode.AppLog.Error("Controller.setSafeOutputs", ex);
                MessageBox.Show("Could not set the safe outputs (" + DAQMxAnalogSafeState.Describe(safeOutputs)
                    + ") after the run. They may still be at their last value.\n\n" + ex.Message,
                    "MOTMaster: outputs NOT made safe", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// On an exception mid-pattern the tasks still hold their channels. After a
        /// normal shot they are already disposed and these just throw, harmlessly.
        /// </summary>
        private void stopPatternTasksQuietly()
        {
            if (config.Debug) return;
            try { pgMaster.StopPattern(); } catch { }
            foreach (DAQMxPatternGenerator pg in pgs.Values)
            {
                try { pg.StopPattern(); } catch { }
            }
            foreach (DAQMxAnalogPatternGenerator apg in analogs.Values)
            {
                try { apg.StopPattern(); } catch { }
            }
        }

        private void goClaimed(Dictionary<String, Object> dict)
        {
            {
                status = RunningState.running;
#if DDS
                // Declared out here so the finally can stop the shot loop however
                // the run ends.
                bool ddsInUse = false;
                int ddsTriggersSent = 0;
#endif //DDS
                try
                {
                Stopwatch watch = new Stopwatch();
                MOTMasterScript script = prepareScript(scriptPath, dict);
                if (script != null)
                {
                    MOTMasterSequence sequence = getSequenceFromScript(script);

#if DDS
                    // Hand the script's DDS pattern to the card and leave it armed.
                    // A script with no DDS pattern -- or one returning null -- just
                    // runs as it always did.
                    if (sequence.DDSPattern != null && sequence.DDSPattern.Count > 0)
                    {
                        // A script that wants the DDS, run without it, is not the
                        // experiment anyone asked for -- the AOMs would sit wherever
                        // they were last left. So a failure here stops the run
                        // instead of firing the sequence anyway. Nothing has been
                        // built or armed at this point, so there is nothing to undo.
                        if (!armDDS(sequence.DDSPattern))
                        {
                            status = RunningState.stopped;
                            return;
                        }
                        ddsInUse = true;
                    }
#endif //DDS

                    //try
                    //{
                    //if (config.CameraUsed) prepareCameraControl();

                    //if (config.TranslationStageUsed) armTranslationStageForTimedMotion(script);

                    //if (config.CameraUsed) GrabImage((int)script.Parameters["NumberOfFrames"]);

                    buildPattern(sequence, (int)script.Parameters["PatternLength"]);

                    //if (config.CameraUsed) waitUntilCameraIsReadyForAcquisition();

                    watch.Start();

                    if (controllerWindow.RunUntilStoppedState)
                    {
                        while (status == RunningState.running)
                        {
                            if (!config.Debug)
                            {
#if DDS
                                // Do not fire into the DDS re-queue deadtime. If it
                                // is still not armed after a second, go anyway and
                                // let the tally at the end report the miss.
                                if (ddsInUse) ddsController.WaitUntilArmed(1.0);
#endif //DDS
                                runPattern(sequence);
#if DDS
                                if (ddsInUse) ddsTriggersSent++;
#endif //DDS
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < controllerWindow.GetIterations() && status == RunningState.running; i++)
                        {
                            if (!config.Debug)
                            {
#if DDS
                                if (ddsInUse) ddsController.WaitUntilArmed(1.0);
#endif //DDS
                                runPattern(sequence);
#if DDS
                                if (ddsInUse) ddsTriggersSent++;
#endif //DDS
                            }
                        }
                    }


                    watch.Stop();
#if DDS
                    if (ddsInUse)
                    {
                        // sent   = triggers MOTMaster fired
                        // fired  = patterns the DDS ran to completion
                        // received = the card's own trigger counter, which only
                        //            means anything as a difference
                        int ddsFired = ddsController.PatternsFired;
                        int ddsReceived = ddsController.GetCardTriggerCount();
                        Console.WriteLine(
                            "DDS triggers -- sent: {0}, fired: {1}, missed: {2}, card count: {3}",
                            ddsTriggersSent, ddsFired, ddsTriggersSent - ddsFired, ddsReceived);
                        SpectrumDDS.DdsLog.Write("MOTMaster", string.Format(
                            "DDS triggers -- sent: {0}, fired: {1}, missed: {2}, card count: {3}",
                            ddsTriggersSent, ddsFired, ddsTriggersSent - ddsFired, ddsReceived));

                        // The console line above only reaches anyone who started
                        // MOTMaster from Visual Studio, which is rare, so hand the
                        // tally to the DDS window as well -- that is on screen anyway
                        // and is where these numbers actually get read. Reporting must
                        // never take the run down with it: saving and the report still
                        // have to happen.
                        try
                        {
                            ddsController.ReportRunTally(ddsTriggersSent);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Could not report the DDS tally: " + ex.Message);
                            SpectrumDDS.DdsLog.Error("MOTMaster.ReportRunTally", ex);
                        }
                    }
#endif //DDS

                    //MessageBox.Show(watch.ElapsedMilliseconds.ToString());
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
                            Dictionary<String, Object> report = null;
                            if (config.ReporterUsed)
                            {
                                report = GetExperimentReport();
                            }

                            save(sequence, script, scriptPath, imageData, report);

                        }
                        else
                        {
                            Dictionary<String, Object> report = null;
                            if (config.ReporterUsed)
                            {
                                report = GetExperimentReport();
                            }

                            save(sequence, script, scriptPath, report);

                        }


                    }
                    //if (config.CameraUsed) finishCameraControl();
                    //if (config.TranslationStageUsed) disarmAndReturnTranslationStage();
                    //if (config.CameraUsed) finishCameraControl();
                    //if (config.TranslationStageUsed) disarmAndReturnTranslationStage();

                    if (!config.Debug) clearDigitalPattern(sequence);
                    //}
                    //catch (System.Net.Sockets.SocketException e)
                    //{
                    //    MessageBox.Show("CameraControllable not found. \n Is there a hardware controller running? \n \n" + e.Message, "Remoting Error");
                    //}
                }
                else
                {
                    MessageBox.Show("Unable to load pattern. \n Check that the script file exists and that it compiled successfully");
                }
                }
                catch (AnalogPatternBuilderSingleBoard.InsufficientPatternLengthException ex)
                {
                    SharedCode.AppLog.Error("Controller.Go (analog pattern length)", ex);
                    MessageBox.Show("The pattern length is too short to fit all the requested analog events.\n\n"
                        + ex.Message + "\n\nIncrease PatternLength in the script and try again.",
                        "Insufficient Pattern Length", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (DAQ.Pattern.InsufficientPatternLengthException ex)
                {
                    SharedCode.AppLog.Error("Controller.Go (digital pattern length)", ex);
                    MessageBox.Show("The pattern length is too short to fit all the requested digital events.\n\n"
                        + ex.Message + "\n\nIncrease PatternLength in the script and try again.",
                        "Insufficient Pattern Length", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
#if DDS
                finally
                {
                    // Every way out of the run leads here, including the early return
                    // when the camera data does not arrive. A DDS still arming itself
                    // after the last trigger is what leaves stale shots queued on the
                    // card for the next run to play through.
                    if (ddsInUse) stopDDS();
                }
#endif //DDS
                status = RunningState.stopped;
            }
        }

        public void saveSequenceInfo(string dir)
            // like save, but callable outside of Go() with a custom path.
            // Intended use case is to give external libraries such as pycaf access to saving pattern info
        {
            MOTMasterScript script = prepareScript(scriptPath, null);
            MOTMasterSequence sequence = getSequenceFromScript(script);
            Dictionary <string, object> report = null;
            ioHelper.StoreRun(dir, controllerWindow.GetSaveBatchNumber(), scriptPath, hardwareClassPath, sequence, script.Parameters, report, externalFilesPath, config.ExternalFilePattern);
        }

#endregion

        #region Linked runs: secondary side (remotable)

        // This MOTMaster as the secondary of a linked pair: a primary claims it, sends it the script text and
        // full parameter set for each Go, and fires it with a TTL. While claimed, local Run/Go/SetScriptPath/
        // SetTriggered/RunReplica are refused. None of this is reached unless a primary calls ClaimLink.

        private readonly LinkedSecondaryState secondaryState = new LinkedSecondaryState();

        // True while a linked run's thread owns the run claim. Lets ReleaseLink end that run.
        private volatile bool linkedRunActive = false;

        public bool IsLockedByPrimary => secondaryState.IsClaimed;

        public string LockedBy => secondaryState.ClaimedBy;

        /// <summary>Raised (on whichever thread changed it) after every change of the link state, for the GUI.</summary>
        public event Action LinkStateChanged
        {
            add { secondaryState.Changed += value; }
            remove { secondaryState.Changed -= value; }
        }

        public string ClaimLink(string primaryName)
        {
            string token;
            try
            {
                token = secondaryState.Claim(primaryName, IsRunning);
            }
            catch (Exception ex)
            {
                SharedCode.AppLog.Write("Controller.ClaimLink", "Refused a link claim from " + primaryName + ": " + ex.Message);
                throw;
            }
            SharedCode.AppLog.Write("Controller.ClaimLink", "Linked: now controlled by " + primaryName);
            // Describe() runs git and can take seconds the first time; warm it now, off the caller's thread,
            // so PrepareLinkedRun does not pay for it.
            ThreadPool.QueueUserWorkItem(_ => BuildProvenance.Describe());
            return token;
        }

        public void ReleaseLink(string token)
        {
            bool valid = true;
            try { secondaryState.Validate(token); }
            catch (LinkedRunException) { valid = false; }
            if (!valid) return; // a wrong or stale token does nothing

            // A linked run nobody can trigger or end any more must not stay armed.
            if (linkedRunActive) status = RunningState.stopped;
            secondaryState.Release(token);
            SharedCode.AppLog.Write("Controller.ReleaseLink", "Link released by the primary");
        }

        /// <summary>The secondary's "Break link" button: ends any linked run and clears the claim whoever holds it.</summary>
        public void BreakLink()
        {
            status = RunningState.stopped;
            string by = secondaryState.ClaimedBy;
            secondaryState.Break();
            SharedCode.AppLog.Write("Controller.BreakLink", "Link broken by the user" + (by != null ? " (was controlled by " + by + ")" : ""));
        }

        public LinkReadyInfo PrepareLinkedRun(string token, LinkRunRequest request)
        {
            secondaryState.Validate(token);
            if (request == null) throw new LinkedRunException("The linked run request is empty.");

            // The same run claim Go takes, so a local run cannot start mid-linked-run and vice versa.
            lock (runLock)
            {
                if (runInProgress)
                    throw new LinkedRunException("This MOTMaster (" + System.Environment.MachineName
                        + ") is already running; the linked run was not started.");
                runInProgress = true;
            }
            controllerWindow.RefreshRunControls();

            bool threadStarted = false;
            bool runStartedSignalled = false;
            try
            {
                if (config.Debug)
                    throw new LinkedRunException("This MOTMaster (" + System.Environment.MachineName
                        + ") is in Debug mode, where it does not drive the pattern generators; linked runs are refused.");

                Hardware other = (Hardware)Activator.CreateInstance(Type.GetType(request.PrimaryHardwareType, true));
                LinkedBuild build = LinkedScriptBuilder.BuildSecondary(request, Environs.Hardware, other, compileAndLoadOrThrow);
                LinkedScriptBuilder.BuildPatterns(build);

                string[] clamped = new List<string>(build.Routing.ClampedEvents).ToArray();
                foreach (string line in clamped) SharedCode.AppLog.Write("Controller.PrepareLinkedRun", line);

                LinkReadyInfo info = new LinkReadyInfo
                {
                    // The text actually compiled: read back from the file BuildSecondary wrote.
                    ScriptHash = LinkFingerprint.OfText(File.ReadAllText(LinkedScriptBuilder.ScriptPathFor(request))),
                    ParameterHash = LinkFingerprint.OfParameters(build.Script.Parameters),
                    Provenance = BuildProvenance.Describe(),
                    DigitalClockFrequency = config.DigitalPatternClockFrequency,
                    AnalogClockFrequency = config.AnalogPatternClockFrequency,
                    ClampedEvents = clamped
                };

                // The shot loop runs while status == running; the primary may poll as soon as this returns, so
                // the state must already say "running" before the thread starts.
                status = RunningState.running;
                secondaryState.RunStarted();
                runStartedSignalled = true;
                linkedRunActive = true;
                Thread t = new Thread(() => linkedSecondaryLoop(request, build, info));
                t.Name = "MOTMaster linked secondary";
                t.Priority = ThreadPriority.Normal;
                t.Start();
                threadStarted = true;

                SharedCode.AppLog.Write("Controller.PrepareLinkedRun", "Linked run " + request.LinkRunId + " from " + request.PrimaryName
                    + " armed for triggering (" + request.ScriptFileName + ")");
                return info;
            }
            catch (Exception ex)
            {
                SharedCode.AppLog.Error("Controller.PrepareLinkedRun", ex);
                if (ex is LinkedRunException) throw;
                // Message only: the original may not be serializable, and it has to cross remoting to the primary.
                throw new LinkedRunException(System.Environment.MachineName + " could not prepare the linked run: "
                    + ex.GetType().Name + ": " + ex.Message);
            }
            finally
            {
                if (!threadStarted)
                {
                    MOTMasterScript.LinkedParameterOverride = null;
                    linkedRunActive = false;
                    status = RunningState.stopped;
                    lock (runLock) { runInProgress = false; }
                    controllerWindow.RefreshRunControls();
                    if (runStartedSignalled) secondaryState.RunEnded();
                    deleteLinkedScriptFolder(request);
                }
            }
        }

        public LinkShotStatus GetLinkShotStatus(string token)
        {
            return secondaryState.Status(token);
        }

        public void EndLinkedRun(string token)
        {
            secondaryState.Validate(token);
            status = RunningState.stopped;
        }

        /// <summary>
        /// The secondary's run thread: arm in triggered mode, report armed, wait for the primary's trigger to
        /// play the pattern, repeat until told to stop. Never touches the DDS.
        /// </summary>
        private void linkedSecondaryLoop(LinkRunRequest request, LinkedBuild build, LinkReadyInfo info)
        {
            MOTMasterSequence sequence = build.Sequence;
            try
            {
                while (status == RunningState.running)
                {
                    initializeHardware(sequence, forceTriggered: true);
                    // Without the one-pattern sleep, so "armed" is reported as soon as the task waits for its trigger.
                    run(sequence, sleepAfterStart: false);
                    secondaryState.MarkArmed();
                    // Ends when the triggered pattern has played, or at once when the run is ended (status stopped),
                    // abandoning a trigger that will not come; releaseHardware then stops the task.
                    while (pgMaster.TaskRunning && status == RunningState.running) ;
                    releaseLinkedHardware(sequence);
                }

                if (request.SaveEnable)
                {
                    Dictionary<String, Object> report = null;
                    if (config.ReporterUsed)
                    {
                        report = GetExperimentReport();
                    }

                    var linkInfo = new Dictionary<String, Object>
                    {
                        { "LinkRunId", request.LinkRunId ?? "" },
                        { "Role", "secondary" },
                        { "Partner", request.PrimaryName ?? "" },
                        { "ScriptHash", info.ScriptHash },
                        { "ParameterHash", info.ParameterHash },
                        { "SecondaryProvenance", info.Provenance ?? "" },
                        { "AnalogOrigin", build.AnalogOrigin },
                        { "SecondaryTriggerDelay", build.TriggerDelay },
                        { "ClampedEvents", string.Join("; ", info.ClampedEvents ?? new string[0]) }
                    };
                    ioHelper.LinkInfo = linkInfo;
                    try
                    {
                        // The script saved is the text this machine compiled (the temp copy), not a local file.
                        save(sequence, build.Script, LinkedScriptBuilder.ScriptPathFor(request), report, request.BatchNumber);
                    }
                    finally
                    {
                        ioHelper.LinkInfo = null;
                    }
                }
            }
            catch (Exception ex)
            {
                SharedCode.AppLog.Error("Controller.linkedSecondaryLoop", ex);
                // As in Go: free the tasks, which setSafeOutputs only does when safe outputs are declared.
                stopPatternTasksQuietly();
                secondaryState.Fail(ex.Message);
            }
            finally
            {
                // Safe outputs first (this also stops any pattern task still holding its channels), and only then
                // give up the run claim, so no other run can start in between.
                setSafeOutputs();
                try
                {
                    if (sequence != null && sequence.DigitalPattern != null && sequence.DigitalPattern.Boards.ContainsKey(pgMasterName))
                        clearDigitalPattern(sequence);
                }
                catch (Exception ex)
                {
                    SharedCode.AppLog.Error("Controller.linkedSecondaryLoop (clearDigitalPattern)", ex);
                }
                status = RunningState.stopped;
                linkedRunActive = false;
                lock (runLock) { runInProgress = false; }
                controllerWindow.RefreshRunControls();
                secondaryState.RunEnded();
                deleteLinkedScriptFolder(request);
                SharedCode.AppLog.Write("Controller.linkedSecondaryLoop", "Linked run " + request.LinkRunId + " ended");
            }
        }

        /// <summary>
        /// releaseHardware for the linked secondary: stops exactly the tasks this shot started (run() and
        /// initializeHardware only touch boards the sequence uses). releaseHardware stops every analog generator,
        /// which throws for a board the sequence doesn't use (never configured, or already disposed) - and a
        /// linked script may well have no analog events on this machine.
        /// </summary>
        private void releaseLinkedHardware(MOTMasterSequence sequence)
        {
            pgMaster.StopPattern();
            foreach (KeyValuePair<string, DAQMxPatternGenerator> pg in pgs)
            {
                if (sequence.DigitalPattern.Boards.ContainsKey(pg.Key))
                    pg.Value.StopPattern();
            }
            foreach (KeyValuePair<string, DAQMxAnalogPatternGenerator> apg in analogs)
            {
                if (sequence.AnalogPattern.Boards.ContainsKey(apg.Key))
                    apg.Value.StopPattern();
            }
        }

        /// <summary>Best effort: removes the temp folder holding a linked run's copy of the script.</summary>
        private void deleteLinkedScriptFolder(LinkRunRequest request)
        {
            try
            {
                string folder = Path.GetDirectoryName(LinkedScriptBuilder.ScriptPathFor(request));
                if (Directory.Exists(folder)) Directory.Delete(folder, true);
            }
            catch (Exception ex)
            {
                SharedCode.AppLog.Write("Controller.deleteLinkedScriptFolder", "Could not delete the linked script folder (ignored): " + ex.Message);
            }
        }

        #endregion

        #region private stuff

#if DDS
        /// <summary>
        /// Load the script's DDS pattern onto the card and leave it armed, saying
        /// what is wrong instead of letting the run die.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="Go(Dictionary{string, object})"/> runs on its own thread, so
        /// anything thrown out of the DDS calls used to take MOTMaster down with
        /// nothing on screen to say why. There are three ways they throw, and all
        /// three are things somebody will hit: the remoting call fails when
        /// SpectrumDDSController is not running, the driver refuses a pattern while
        /// the card is closed, and it rejects a pattern that asks for more amplitude
        /// than the clamp allows.
        /// </para>
        /// <para>
        /// Gated on the DDS compile symbol, which only the CaF configuration
        /// defines.
        /// </para>
        /// </remarks>
        /// <returns>True if the card is armed and the run may go ahead.</returns>
        private bool armDDS(Dictionary<string, List<List<double>>> ddsPattern)
        {
            // Any call would do to find out whether the controller is there at all;
            // IsOpen is the cheapest, and answers the next question too.
            bool cardOpen;
            try
            {
                cardOpen = ddsController.IsOpen;
            }
            catch (Exception ex)
            {
                SpectrumDDS.DdsLog.Error("MOTMaster.armDDS (controller unreachable)", ex);
                offerToLaunchDDSController(ex);
                return false;
            }

            if (!cardOpen)
            {
                // Opening the card enables no output stage and emits nothing, so
                // this is safe to offer from here.
                if (MessageBox.Show(
                        "This script has a DDS pattern, but the Spectrum DDS card is not open.\n\n" +
                        "Open it now?",
                        "Spectrum DDS not open",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return false;

                try
                {
                    ddsController.OpenCard();
                }
                catch (Exception ex)
                {
                    SpectrumDDS.DdsLog.Error("MOTMaster.armDDS (OpenCard)", ex);
                    MessageBox.Show(
                        "The Spectrum DDS card would not open, so the run has been stopped.\n\n" +
                        ex.Message,
                        "Spectrum DDS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            try
            {
                ddsController.PrepareForNewPattern();
                ddsController.patternList = ddsPattern;
                ddsController.StartRepetitivePattern();
                return true;
            }
            catch (Exception ex)
            {
                SpectrumDDS.DdsLog.Error("MOTMaster.armDDS (load pattern)", ex);
                MessageBox.Show(
                    "The Spectrum DDS would not take this script's pattern, so the run has been stopped.\n\n" +
                    ex.Message,
                    "Spectrum DDS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Stop the DDS re-arming once this run has fired its last trigger.
        /// </summary>
        /// <remarks>
        /// Only the loop stops. The card is left alone, so the four channels hold
        /// the last event's frequency and amplitude until the next pattern is
        /// loaded, and the one shot the loop had already armed stays queued until
        /// PrepareForNewPattern discards it. Failing here must not take the run's
        /// saving and reporting down with it, so it is reported and swallowed.
        /// </remarks>
        private void stopDDS()
        {
            try
            {
                ddsController.StopRepetitivePattern();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not stop the DDS shot loop: " + ex.Message);
                SpectrumDDS.DdsLog.Error("MOTMaster.stopDDS", ex);
            }
        }

        /// <summary>
        /// Say that the DDS controller is not running, and offer to start it.
        /// </summary>
        /// <remarks>
        /// SpectrumDDSController.exe is a ProjectReference of this project under the
        /// DDS symbol, so the build drops it beside MOTMaster.exe.
        /// </remarks>
        private void offerToLaunchDDSController(Exception ex)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                       "SpectrumDDSController.exe");

            if (!File.Exists(path))
            {
                MessageBox.Show(
                    "This script has a DDS pattern, but SpectrumDDSController is not running, " +
                    "so the run has been stopped.\n\n" + ex.Message +
                    "\n\nIt could not be started from here either: there is no " +
                    "SpectrumDDSController.exe at\n" + path,
                    "Spectrum DDS not running", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show(
                    "This script has a DDS pattern, but SpectrumDDSController is not running, " +
                    "so the run has been stopped.\n\n" + ex.Message + "\n\nLaunch Spectrum DDS?",
                    "Spectrum DDS not running",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                Process.Start(new ProcessStartInfo(path)
                {
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    UseShellExecute = true,
                });

                // It opens the card itself, but that takes a few seconds and this
                // thread should not sit and block the run button waiting for it.
                MessageBox.Show(
                    "Spectrum DDS is starting. It opens the card by itself; once the " +
                    "Status tab says so, press Go again.",
                    "Spectrum DDS", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception launchException)
            {
                MessageBox.Show("Spectrum DDS would not start:\n\n" + launchException.Message,
                    "Spectrum DDS", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
#endif //DDS

        private string constructSaveDirectory()
        {
            string dir = motMasterDataPath + DateTime.Now.ToString("yyyy/MM") + DateTime.Now.ToString("MMM/dd") + "\\";
            System.IO.Directory.CreateDirectory(dir);
            return dir;
        }

        // ONGOING: these save functions should probably take a sequence argument
        // batchNumberOverride: null (every existing caller) reads the batch number from the window, as before;
        // the linked secondary passes the primary's batch number.
        private void save(MOTMasterSequence sequence, MOTMasterScript script, string pathToPattern, byte[][,] imageData, Dictionary<String, Object> report,
            int? batchNumberOverride = null)
        {
            ioHelper.StoreRun(constructSaveDirectory(), batchNumberOverride ?? controllerWindow.GetSaveBatchNumber(), pathToPattern, hardwareClassPath,
                sequence, script.Parameters, report, cameraAttributesPath, imageData, externalFilesPath, config.ExternalFilePattern);
        }
        private void save(MOTMasterSequence sequence, MOTMasterScript script, string pathToPattern, Dictionary<String, Object> report,
            int? batchNumberOverride = null)
        {
            ioHelper.StoreRun(constructSaveDirectory(), batchNumberOverride ?? controllerWindow.GetSaveBatchNumber(), pathToPattern, hardwareClassPath,
                sequence, script.Parameters, report, externalFilesPath, config.ExternalFilePattern);
        }

        private void runPattern(MOTMasterSequence sequence)
        {
            initializeHardware(sequence);
            run(sequence);
            while (pgMaster.TaskRunning && status == RunningState.running) ;
            releaseHardware();
        }

        private MOTMasterScript prepareScript(string pathToPattern, Dictionary<String, Object> dict)
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
            sequence.AnalogStatic.BuildPattern();
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

        // compileFromFile and loadScriptFromDLL show a MessageBox and return null on failure, as they always
        // have. The linked secondary must never show a MessageBox (it would block the primary's remoting call
        // indefinitely), so it uses compileFromFileOrThrow / loadScriptOrThrow, which share the same cores.

        private CompilerResults compileFromFile(string scriptPath)
        {
            CompilerParameters options = scriptCompilerOptions();
            CodeDomProvider codeProvider = new CSharpCodeProvider();
            CompilerResults results;
            try
            {
                results = codeProvider.CompileAssemblyFromFile(options, scriptPath);
            }
            catch (Exception e)
            {
                SharedCode.AppLog.Error("Controller.compileFromFile " + scriptPath, e);
                MessageBox.Show(e.Message);
                return null;
            }
            //controllerWindow.WriteToScriptPath(results.PathToAssembly);
            return results;
        }

        private CompilerParameters scriptCompilerOptions()
        {
            CompilerParameters options = new CompilerParameters();

            options.ReferencedAssemblies.Add(motMasterPath);
            if (Environs.FileSystem.Paths.ContainsKey("AdditionalMOTMasterAssemblies"))
            {
                foreach (string path in (List<string>)Environs.FileSystem.Paths["AdditionalMOTMasterAssemblies"])
                {
                    options.ReferencedAssemblies.Add(path);
                }
            }

            options.ReferencedAssemblies.Add(daqPath);

            TempFileCollection tempFiles = new TempFileCollection();
            tempFiles.KeepFiles = true;
            options.GenerateExecutable = false;                         //Creates .dll instead of .exe.
            options.TempFiles = tempFiles;
            return options;
        }

        /// <summary>Linked path: compiles the script, throwing LinkedRunException (with the compiler errors) on any failure.</summary>
        private CompilerResults compileFromFileOrThrow(string scriptPath)
        {
            CompilerParameters options = scriptCompilerOptions();
            CodeDomProvider codeProvider = new CSharpCodeProvider();
            CompilerResults results;
            try
            {
                results = codeProvider.CompileAssemblyFromFile(options, scriptPath);
            }
            catch (Exception e)
            {
                SharedCode.AppLog.Error("Controller.compileFromFileOrThrow " + scriptPath, e);
                throw new LinkedRunException("The script could not be compiled on " + System.Environment.MachineName + ": " + e.Message);
            }
            if (results.Errors.HasErrors)
            {
                var errors = new StringBuilder();
                foreach (CompilerError error in results.Errors)
                {
                    if (error.IsWarning) continue;
                    errors.AppendLine("line " + error.Line + ": " + error.ErrorNumber + ": " + error.ErrorText);
                }
                string message = "The script did not compile on " + System.Environment.MachineName + ":\n" + errors.ToString();
                SharedCode.AppLog.Write("Controller.compileFromFileOrThrow", message);
                throw new LinkedRunException(message);
            }
            return results;
        }

        private MOTMasterScript loadScriptFromDLL(CompilerResults results)
        {
            object loadedInstance = new object();
            try
            {
                loadedInstance = createScriptInstance(results);
            }
            catch (Exception e)
            {
                SharedCode.AppLog.Error("Controller.loadScriptFromDLL", e);
                MessageBox.Show(e.Message);
                return null;
            }
            return (MOTMasterScript)loadedInstance;
        }

        /// <summary>Loads the compiled script and instantiates its classes; the last one wins, as it always has.</summary>
        private object createScriptInstance(CompilerResults results)
        {
            object loadedInstance = new object();
            Assembly patternAssembly = Assembly.LoadFrom(results.PathToAssembly);
            foreach (Type type in patternAssembly.GetTypes())
            {
                if (type.IsClass == true)
                {
                    loadedInstance = Activator.CreateInstance(type);
                }
            }
            return loadedInstance;
        }

        /// <summary>Linked path: like loadScriptFromDLL, but throws LinkedRunException instead of a MessageBox.</summary>
        private MOTMasterScript loadScriptOrThrow(CompilerResults results)
        {
            object loadedInstance;
            try
            {
                loadedInstance = createScriptInstance(results);
            }
            catch (Exception e)
            {
                SharedCode.AppLog.Error("Controller.loadScriptOrThrow", e);
                // A script constructor's own exception arrives wrapped in a TargetInvocationException.
                Exception cause = (e is TargetInvocationException && e.InnerException != null) ? e.InnerException : e;
                throw new LinkedRunException("The script could not be loaded on " + System.Environment.MachineName + ": " + cause.Message);
            }
            MOTMasterScript script = loadedInstance as MOTMasterScript;
            if (script == null)
                throw new LinkedRunException("The compiled script's last class is not a MOTMasterScript (on " + System.Environment.MachineName + ").");
            return script;
        }

        private MOTMasterScript compileAndLoadOrThrow(string scriptPath)
        {
            return loadScriptOrThrow(compileFromFileOrThrow(scriptPath));
        }

        private MOTMasterSequence getSequenceFromScript(MOTMasterScript script)
        {
            MOTMasterSequence sequence = script.GetSequence();
            return sequence;
        }

        #endregion

        public int GetIterations()
        {
            return controllerWindow.GetIterations();
        }

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
        /*

        */
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
            secondaryState.ThrowIfClaimed("RunReplica");
            // armReplicaRun repoints scriptPath, which a run in progress still uses
            // when it saves, so refuse before touching anything.
            if (IsRunning) return;
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
    }
}

