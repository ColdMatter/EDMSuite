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
    /// - After everything's finished, MM releases the hardware.
    /// 
    /// IMPORTANT NOTE ABOUT WRITING SCRIPTS: At the moment, THERE IS NO AUTOMATIC TIME ORDERING FOR ANALOG
    /// CHANNELS. IT WILL BUILD A PATTERN FOLLOWING THE ORDER IN WHICH YOU CALL AddAnalogValue / AddLinearRamp!!
    /// ---> Stick to writing out the pattern in the correct time order to avoid weirdo behaviour.
    /// 
    /// </summary>
    public class Controller : MarshalByRefObject
    {

        #region Class members

        private static string
            motMasterPath = (string)Environs.FileSystem.Paths["MOTMasterEXEPath"] + "//MotMaster.exe";
        private static string
            daqPath = (string)Environs.FileSystem.Paths["daqDLLPath"] + "//daq.dll";
        private static string
            scriptListPath = (string)Environs.FileSystem.Paths["scriptListPath"];
        private static string
            motMasterDataPath = (string)Environs.FileSystem.Paths["MOTMasterDataPath"];
        private const int
            pgClockFrequency = 1000;
        private const int
            apgClockFrequency = 10000;
        private const int
            patternLength = 100;

        ControllerWindow controllerWindow;

        DAQMxPatternGenerator pg;
        DAQmxAnalogPatternGenerator apg;

        public bool SaveEnable = false;
        public enum GetPattern { FROM_MOTMASTERSCRIPT, FROM_MOTMASTERSCRIPT_W_CHANGES, FROM_BINARY}
        public GetPattern PatternSource = new GetPattern();

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

            PatternSource = GetPattern.FROM_BINARY;

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
            pg.Configure(pgClockFrequency, false, true, true, patternLength, true);
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

        #region RUN RUN RUN
        
        public void Run()
        {
            
            MOTMasterSequence sequence = preparePattern(patternPath, null);
            runPattern(sequence); 
        }
        public void Run(Dictionary<String,Object> dict)
        {

            MOTMasterSequence sequence = preparePattern(patternPath, dict);
            runPattern(sequence);
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
                controllerWindow.WriteToPatternSourcePath(e.Message);
            }
        }

        private MOTMasterSequence preparePattern(string pathToPattern, Dictionary<String,Object> dict)
        {
            MOTMasterSequence sequence = new MOTMasterSequence();
            if (pathToPattern.Length != 0)
            {

                if (getFileType(pathToPattern) == "bin")
                {
                    sequence = loadSequenceFromBinaryFile(pathToPattern);
                }

                else if (getFileType(pathToPattern) == ".cs")
                {
                    CompilerResults results = compileFromFile(pathToPattern);
                    MOTMasterScript script = loadScriptFromDLL(results);

                    if (dict != null)
                    {
                        swapDictionary(script, dict);
                    }
                    sequence = getSequenceFromScript(script);
                    buildPattern(sequence);

                    if (SaveEnable)
                    {
                        string filePath = getDataID((string)Environs.Hardware.GetInfo("Element"),
                            controllerWindow.GetSaveBatchNumber());

                        storeDictionary(motMasterDataPath + filePath + ".txt", script.Parameters);
                        storeMOTMasterSequence(motMasterDataPath + filePath + ".bin", sequence);
                    }
                }
                else
                {
                    controllerWindow.WriteToPatternSourcePath("File not recognized");
                }
            }
            return sequence;
        }


        private string getFileType(string path)
        {
            return path.Substring(path.Length - 3, 3);
        }
        private void buildPattern(MOTMasterSequence sequence)
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
            catch (Exception e)
            {
                controllerWindow.WriteToPatternSourcePath(e.Message);
            }
            //controllerWindow.WriteToPatternSourcePath(results.PathToAssembly);
            return results;
        }

        private MOTMasterSequence loadSequenceFromBinaryFile(String dataStoreFilePath)
        {
            // deserialize
            BinaryFormatter s = new BinaryFormatter();
            FileStream fs = new FileStream(dataStoreFilePath, FileMode.Open);
            MOTMasterSequence sequence = new MOTMasterSequence();
            // eat any errors in the following, as it's just a convenience function
            try
            {
                sequence = (MOTMasterSequence)s.Deserialize(fs);

            }
            catch (Exception)
            { Console.Out.WriteLine("Unable to load settings"); }
            finally
            {
                fs.Close();
            }
            return sequence;
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
                controllerWindow.WriteToPatternSourcePath(e.Message);
            }
            return (MOTMasterScript)loadedInstance;
        }

        private MOTMasterSequence getSequenceFromScript(MOTMasterScript script)
        {
            MOTMasterSequence sequence = script.GetSequence(); 
            return sequence;
        }

        #endregion

        #region Saving, Loading and Modifying Experimental Parameters
        
        private void swapDictionary(MOTMasterScript script, Dictionary<String,Object> dictionary)
        {
            script.Parameters = dictionary;
        }
       
        private void storeMOTMasterSequence(String dataStoreFilePath, MOTMasterSequence sequence)
        {
            BinaryFormatter s = new BinaryFormatter();
            FileStream fs = new FileStream(dataStoreFilePath, FileMode.Create);
            try
            {
                s.Serialize(fs, sequence);
            }
            catch (Exception)
            {
                Console.Out.WriteLine("Saving failed");
            }
            finally
            {
                fs.Close();
            }

        }
        private void storeDictionary(String dataStoreFilePath, Dictionary<string, object> dict)
        {
            TextWriter output = File.CreateText(dataStoreFilePath);
            foreach (KeyValuePair<string, object> pair in dict)
            {
                output.Write((string)pair.Key);
                output.Write('\t');
                output.WriteLine(pair.Value.ToString());
            }
            output.Close();


        }

        private string getDataID(string element, int batchNumber)
        {
            DateTime dt = DateTime.Now;
            string dateTag;
            string batchTag;
            int subTag = 0;

            dateTag = String.Format("{0:ddMMMyy}", dt);
            batchTag = batchNumber.ToString().PadLeft(2, '0');
            subTag = (Directory.GetFiles(motMasterDataPath, element +
                dateTag + batchTag + "*.txt")).Length;
            string id = element + dateTag + batchTag
                + "_" + subTag.ToString().PadLeft(3, '0');
            return id;
        }

        private string patternPath;
        public void SetPatternPath(string path)
        {
            patternPath = path;
            controllerWindow.WriteToPatternSourcePath(path);
        }
        public void SelectPatternPathDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Saved Patterns|*.bin";
            dialog.Title = "Load previously saved pattern";
            dialog.InitialDirectory = motMasterDataPath;
            dialog.ShowDialog();
            SetPatternPath(dialog.FileName);
        }
        #endregion
    }
}

