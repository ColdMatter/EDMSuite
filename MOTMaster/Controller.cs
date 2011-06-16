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

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Analog;
using Data;
using Data.Scans;

using SympatheticHardwareControl;
using SympatheticHardwareControl.CameraControl;

using System.Runtime.InteropServices;
using System.CodeDom;
using System.CodeDom.Compiler;

using NationalInstruments;
using NationalInstruments.DAQmx;
using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;
using System.Runtime.Serialization.Formatters.Binary;

namespace MOTMaster
{
    /// <summary>
    /// Here's MOTMaster's controller.
    /// 
    /// Here's what it does:
    /// 
    /// - MOTMaster looks in a folder ("scriptListPath") for all classes. Then displays the list in a combo box.
    /// 
    /// - These classes contain an implementation of a "MOTMasterScript". This contains the information 
    /// about the patterns.
    /// 
    /// - Once the user has selected a particular implementation of MOTMasterScript, 
    /// MOTMaster will compile it. Note: the dll is currently stored in a temp folder somewhere. 
    /// Its pathToPattern can be found in the CompilerResults.PathToAssembly). 
    /// This newly formed dll contain methods named GetDigitalPattern and GetAnalogPattern. 
    /// 
    /// - These are called by the script's "GetSequence". GetSequence always returns a 
    /// "MOTMasterSequence", which comprises a PatternBuilder32 and an AnalogPatternBuilder.
    /// 
    /// - MOTMaster then initializes the hardware, faffs a little to prepare the patterns in the 
    /// builders (e.g. calls "BuildPattern"), and sends the pattern to Hardware.
    /// 
    /// -Note that the analog stuff needs a trigger to start!!!! Make sure one of your digital lines is reserved 
    /// for triggering the analog pattern.
    /// 
    /// - Once the experiment is finished, MM releases the hardware.
    /// 
    /// - MOTMaster also saves the data to a .zip. This includes: the original MOTMasterScript (.cs), a text file
    /// with the parameters in it (IF DIFFERENT FROM THE VALUES IN .cs, THE PARAMETERS IN THE TEXT FILE ARE THE
    /// CORRECT VALUES!), another text file with the camera attributes and a .png file containing the final image.
    /// 
    /// 
    /// 
    /// </summary>
    public class Controller : MarshalByRefObject
    {

        #region Class members

        private static string
            motMasterPath = (string)Environs.FileSystem.Paths["MOTMasterEXEPath"] + "//MotMaster.exe";
        private static string
            daqPath = (string)Environs.FileSystem.Paths["daqDLLPath"];
        private static string
            scriptListPath = (string)Environs.FileSystem.Paths["scriptListPath"];
        private static string
            motMasterDataPath = (string)Environs.FileSystem.Paths["MOTMasterDataPath"];
        private static string
            cameraAttributesPath = (string)Environs.FileSystem.Paths["CameraAttributesPath"];
        private const int
            pgClockFrequency = 10000;
        private const int
            apgClockFrequency = 10000;

        ControllerWindow controllerWindow;

        DAQMxPatternGenerator pg;
        DAQmxAnalogPatternGenerator apg;

        CameraControlable camera;

        MOTMasterDataIOHelper ioHelper;

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

            pg = new DAQMxPatternGenerator((string)Environs.Hardware.Boards["multiDAQ"]);
            apg = new DAQmxAnalogPatternGenerator();

            camera = (CameraControlable)Activator.GetObject(typeof(CameraControlable),
            "tcp://localhost:1180/controller.rem");

            ioHelper = new MOTMasterDataIOHelper(motMasterDataPath, 
                    (string)Environs.Hardware.GetInfo("Element"));

            ScriptLookupAndDisplay();

            Application.Run(controllerWindow);

        }

        #endregion

        #region Hardware control methods

        private void run(MOTMasterSequence sequence)
        {
            apg.OutputPatternAndWait(sequence.AnalogPattern.Pattern);
            pg.OutputPattern(sequence.DigitalPattern.Pattern);
        }

        private void initializeHardware(MOTMasterSequence sequence)
        {
            pg.Configure(pgClockFrequency, false, true, true, sequence.DigitalPattern.Pattern.Length, true);
            apg.Configure(sequence.AnalogPattern, apgClockFrequency);
        }

        private void releaseHardwareAndClearDigitalPattern(MOTMasterSequence sequence)
        {
            sequence.DigitalPattern.Clear(); //No clearing required for analog (I think).
            pg.StopPattern();
            apg.StopPattern();
        }

        #endregion

        #region Script Housekeeping on UI

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
        private string scriptPath = "";
        public void SetScriptPath(string path)
        {
            scriptPath = path;
            controllerWindow.WriteToScriptPath(path);
        }
        private bool replicaRun = false;
        public void SetReplicaRunBool(System.Boolean value)
        {
            replicaRun = value;
        }
        private string dictionaryPath = "";
        public void SetDictionaryPath(string path)
        {
            dictionaryPath = path;
        }


        public void Run()
        {
            if (replicaRun)
            {
                Run(ioHelper.LoadDictionary(dictionaryPath));
            }
            else
            {
                Run(null);
            }
        }
        public void Run(Dictionary<String,Object> dict)
        {
            MOTMasterScript script = prepareScript(scriptPath, dict);
            MOTMasterSequence sequence = getSequenceFromScript(script);
            byte[,] imageData = GrabImage(cameraAttributesPath);
            buildPattern(sequence, (int)script.Parameters["PatternLength"]);
            runPattern(sequence);
            if (saveEnable)
            {
                save(script, scriptPath, imageData);
            }
        }

        #endregion

        #region private stuff

        private void save(MOTMasterScript script, string pathToPattern, byte[,] imageData)
        {
            ioHelper.StoreRun(motMasterDataPath, controllerWindow.GetSaveBatchNumber(), pathToPattern, 
                script.Parameters, cameraAttributesPath, imageData);
        }
        private void runPattern(MOTMasterSequence sequence)
        {
            try
            {
                initializeHardware(sequence);
                run(sequence);
                releaseHardwareAndClearDigitalPattern(sequence);
            }
            catch (Exception e)
            {
                controllerWindow.WriteToScriptPath(e.Message);
            }
        }

        private MOTMasterScript prepareScript(string pathToPattern, Dictionary<String, Object> dict)
        {
            
            MOTMasterScript script;
            if (pathToPattern.Length != 0 && Path.GetExtension(pathToPattern) == ".cs")
            {

                CompilerResults results = compileFromFile(pathToPattern);
                script = loadScriptFromDLL(results);

                if (dict != null)
                {
                    script.EditDictionary(dict);
                }
                
            }
            else
            {
                throw new FileNotRecognizedException();
            }
            
            return script;
        }
        public class FileNotRecognizedException : ApplicationException { }
        public class NoFileSelectedException : ApplicationException { }

        private void buildPattern(MOTMasterSequence sequence, int patternLength)
        {
            sequence.DigitalPattern.BuildPattern(patternLength);
            sequence.AnalogPattern.BuildPattern();
        }
        #endregion

        #region Compiler & Loading DLLs

        private CompilerResults compileFromFile(string scriptPath)
        {
            CompilerParameters options = new CompilerParameters();

            options.ReferencedAssemblies.Add(motMasterPath);
            options.ReferencedAssemblies.Add(daqPath);

            TempFileCollection tempFiles = new TempFileCollection();
            tempFiles.KeepFiles = true;
            CompilerResults results = new CompilerResults(tempFiles);
            options.GenerateExecutable = false;                         //Creates .dll instead of .exe.
            CodeDomProvider codeProvider = new CSharpCodeProvider();
            options.TempFiles = tempFiles;
            try
            {
                results = codeProvider.CompileAssemblyFromFile(options, scriptPath);
            }
            catch
            {
                throw new FileNotRecognizedException();
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
                controllerWindow.WriteToScriptPath(e.Message);
            }
            return (MOTMasterScript)loadedInstance;
        }

        private MOTMasterSequence getSequenceFromScript(MOTMasterScript script)
        {
            MOTMasterSequence sequence = script.GetSequence();
            return sequence;
        }

        #endregion


        /// <summary>
        /// - Camera control is run through the hardware controller. All MOTMaster knows 
        /// about it a function called "GrabImage(string cameraSettings)". If the camera attributes are 
        /// set so that it needs a trigger, MOTMaster will have to deliver that too.
        /// It'll expect a short[,] as a return value.
        /// 
        /// -At the moment MOTMaster won't run without a camera nor with 
        /// more than one, and it can only take one photograph per run. In the long term, we might 
        /// want to fix this.
        /// </summary>

        #region CameraControl

        public byte[,] GrabImage(string cameraAttributes)
        {
            return camera.GrabImage(cameraAttributes);
        }

        #endregion

        #region Re-Running a script (intended for reloading old scripts)

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
    }
}

