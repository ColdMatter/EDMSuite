using System;
using System.Collections;
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

namespace MOTMaster
{
    /// <summary>
    /// Here's MOTMaster's controller. It loads some instructions from the window, converts it to a PatternList
    /// which it then sends to the hardware.
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
        private const int
            pgClockFrequency = 1000;
        private const int
            apgClockFrequency = 10000;
        private const int
            patternLength = 100;

        ControllerWindow controllerWindow;

        DAQMxPatternGenerator pg;
        DAQmxAnalogPatternGenerator apg;

        #endregion

        #region Initialisation

        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        // This function is called at the very start of application execution.
        public void StartApplication()
        {

            controllerWindow = new ControllerWindow();
            controllerWindow.controller = this;

            pg = new DAQMxPatternGenerator((string)Environs.Hardware.Boards["multiDAQ"]);
            apg = new DAQmxAnalogPatternGenerator();
            // run the main event loop
            
            Application.Run(controllerWindow);

        }

        #endregion
        #region Public hardware control methods

        public void SendSequenceToHardware(MOTMasterSequence s)
        {
            InitializeHardware(s);

            s.DigitalPattern.BuildPattern(patternLength);
            s.AnalogPattern.BuildPattern();

            apg.OutputPatternAndWait(s.AnalogPattern.Pattern);
            pg.OutputPattern(s.DigitalPattern.Pattern);

            s.DigitalPattern.Clear(); //No clearing required for analog (I think).

            ReleaseHardware();
            // Add something about analog stuff here.
        }
        public void InitializeHardware(MOTMasterSequence sequence)
        {
            pg.Configure(pgClockFrequency, false, true, true, patternLength, true);
            apg.Configure(sequence, apgClockFrequency);
        }
        public void ReleaseHardware()
        {
            pg.StopPattern();
            apg.StopPattern();
        }

        #endregion

        #region Script Housekeeping

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

        /*#region Compiler

        CompilerResults CompiledPattern;
        public void Compile(string scriptPath)
        {
            CompiledPattern = compileCodeOnUI(scriptPath);
        }

        public void LoadAndRunPattern()
        {
            MOTMasterSequence sequence = 
                (MOTMasterSequence)LoadAndRunMethodFromDll(CompiledPattern, "GetSequence");

            SendSequenceToHardware(sequence);
        }

        private CompilerResults compileCodeOnUI(string scriptPath)
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
                results = codeProvider.CompileAssemblyFromSource(options, scriptPath);
            }
            catch (Exception e)
            {
                controllerWindow.WriteToConsole(e.Message);
            }
            controllerWindow.WriteToConsole(results.PathToAssembly);
            return results;
        }

        private object LoadAndRunMethodFromDll(CompilerResults results, string methodName)
        {
            object result = new object();
            try
            {
                Assembly patternAssembly = Assembly.LoadFrom(results.PathToAssembly);
                foreach (Type type in patternAssembly.GetTypes())
                {
                    if (type.IsClass == true)
                    {
                        //controllerWindow.WriteToConsole(type.FullName);
                        object obj = Activator.CreateInstance(type);
                        result = type.InvokeMember(methodName, BindingFlags.Default | BindingFlags.InvokeMethod,
                                 null,
                                 obj,
                                 null);
                        //controllerWindow.WriteToConsole((string)result);
                        controllerWindow.WriteToConsole("done");
                    }
                }
            }
            catch (Exception e)
            {
                controllerWindow.WriteToConsole(e.Message);
            }
            return result;
        }

        #endregion*/

        #region Compiler

        CompilerResults CompiledPattern;
        public void CompileAndRun(string scriptPath)
        {
            CompiledPattern = compileFromFile(scriptPath);

            MOTMasterSequence sequence =
                (MOTMasterSequence)LoadAndRunMethodFromDll(CompiledPattern, "GetSequence");

            SendSequenceToHardware(sequence);
        }
        private CompilerResults compileFromFile(string filePath)
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
                results = codeProvider.CompileAssemblyFromFile(options, filePath);
            }
            catch (Exception e)
            {
                controllerWindow.WriteToConsole(e.Message);
            }
            controllerWindow.WriteToConsole(results.PathToAssembly);
            return results;
        }

        private object LoadAndRunMethodFromDll(CompilerResults results, string methodName)
        {
            object result = new object();
            try
            {
                Assembly patternAssembly = Assembly.LoadFrom(results.PathToAssembly);
                foreach (Type type in patternAssembly.GetTypes())
                {
                    if (type.IsClass == true)
                    {
                        //controllerWindow.WriteToConsole(type.FullName);
                        object obj = Activator.CreateInstance(type);
                        result = type.InvokeMember(methodName, BindingFlags.Default | BindingFlags.InvokeMethod,
                                 null,
                                 obj,
                                 null);
                        //controllerWindow.WriteToConsole((string)result);
                        controllerWindow.WriteToConsole("done");
                    }
                }
            }
            catch (Exception e)
            {
                controllerWindow.WriteToConsole(e.Message);
            }
            return result;
        }
        #endregion
    }
}

