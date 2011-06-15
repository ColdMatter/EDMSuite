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

using System.Windows.Media;
using System.Windows.Media.Imaging;

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
    /// - After everything's finished, MM releases the hardware.
    /// 
    /// - Camera control is run through the hardware controller. All MOTMaster knows about it a function called
    /// "GrabImage(string cameraSettings)". To take an image, MOTMaster has to call that function, then a TTL to 
    /// trigger the camera. It'll expect a short[,] as a return value.
    /// 
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
        private static string
            cameraAttributesPath = (string)Environs.FileSystem.Paths["cameraAttributesPath"];
        private const int
            pgClockFrequency = 10000;
        private const int
            apgClockFrequency = 10000;

        ControllerWindow controllerWindow;

        DAQMxPatternGenerator pg;
        DAQmxAnalogPatternGenerator apg;

        CameraControlable camera;

        public System.Boolean SaveEnable = false;
        public void SaveToggle(bool value)
        {
            SaveEnable = value;
        }
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

        #region RUN RUN RUN

        public void Run()
        {
            MOTMasterScript script = prepareScript(patternPath, null);
            MOTMasterSequence sequence = getSequenceFromScript(script);
            byte[,] imageData = GrabImage(cameraAttributesPath);
            buildPattern(sequence, (int)script.Parameters["PatternLength"]);
            runPattern(sequence);
            if (SaveEnable)
            {
                save(script, patternPath, imageData);
            }
        }
        public void Run(Dictionary<String,Object> dict)
        {

            MOTMasterScript script = prepareScript(patternPath, dict);
            MOTMasterSequence sequence = getSequenceFromScript(script);
            byte[,] imageData = GrabImage(cameraAttributesPath);
            buildPattern(sequence, (int)script.Parameters["PatternLength"]);
            runPattern(sequence);
            if (SaveEnable)
            {
                save(script, patternPath, imageData);
            }
        }
        private void save(MOTMasterScript script, string pathToPattern, byte[,] imageData)
        {
            string filePath = getDataID((string)Environs.Hardware.GetInfo("Element"),
                controllerWindow.GetSaveBatchNumber());
            storeRun(motMasterDataPath, filePath, pathToPattern, script.Parameters, 
                cameraAttributesPath, imageData);
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
                throw new NoFileSelectedException();
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

        #region CameraControl

        public byte[,] GrabImage(string cameraAttributes)
        {
            return camera.GrabImage(cameraAttributes);
        }

        private void storeImage(string savePath, byte[,] imageData)
        {
            int width = imageData.GetLength(1);
            int height = imageData.GetLength(0);
            byte[] pixels = new byte[width * height];
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    pixels[(width * j) + i] = imageData[j, i];
                }
            }
            // Define the image palette
            BitmapPalette myPalette = BitmapPalettes.Gray256Transparent;

            // Creates a new empty image with the pre-defined palette
            
            BitmapSource image = BitmapSource.Create(
                width,
                height,
                96,
                96,
                PixelFormats.Indexed8,
                myPalette,
                pixels,
                width);

            FileStream stream = new FileStream(savePath, FileMode.Create);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Interlace = PngInterlaceOption.On;
            encoder.Frames.Add(BitmapFrame.Create(image));
            encoder.Save(stream);
            stream.Dispose();

        }
        private void storeCameraAttributes(string savePath, string attributesPath)
        {
            File.Copy(attributesPath, savePath);
        }
        #endregion
        #region Saving, Loading and Modifying Experimental Parameters

        MOTMasterScriptSerializer serializer = new MOTMasterScriptSerializer();
        private void storeRun(string saveFolder, string fileTag, string pathToPattern, Dictionary<String, Object> dict,
            string cameraAttributesPath, byte[,] imageData)
        {
            System.IO.FileStream fs = new FileStream(saveFolder + fileTag + ".zip", FileMode.Create);
            storeDictionary(saveFolder + fileTag + "_parameters.txt", dict);
            storeMOTMasterScript(saveFolder + fileTag + ".cs", pathToPattern);

            storeCameraAttributes(saveFolder + fileTag + "_cameraParams.txt", cameraAttributesPath);
            storeImage(saveFolder + fileTag + ".png", imageData);
            serializer.PrepareZip(fs);
            serializer.AppendToZip(saveFolder, fileTag + "_parameters.txt");
            serializer.AppendToZip(saveFolder, fileTag + ".cs");
            serializer.AppendToZip(saveFolder, fileTag + ".png");
            serializer.AppendToZip(saveFolder, fileTag + "_cameraParams.txt");
            serializer.CloseZip();
            fs.Close();
            File.Delete(saveFolder + fileTag + "_parameters.txt");
            File.Delete(saveFolder + fileTag + ".cs");
            File.Delete(saveFolder + fileTag + ".png");
            File.Delete(saveFolder + fileTag + "_cameraParams.txt");
        }


        private void storeMOTMasterScript(String savePath, String pathToScript)
        {
            File.Copy(pathToScript, savePath);
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
                dateTag + batchTag + "*.zip")).Length;
            string id = element + dateTag + batchTag
                + "_" + subTag.ToString().PadLeft(3, '0');
            return id;
        }

        private string patternPath = "";
        public void SetPatternPath(string path)
        {
            patternPath = path;
            controllerWindow.WriteToPatternSourcePath(path);
        }
        public void SelectPatternPathDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Saved Patterns|*.cs";
            dialog.Title = "Load previously saved pattern";
            dialog.InitialDirectory = motMasterDataPath;
            dialog.ShowDialog();
            SetPatternPath(dialog.FileName);
        }
        #endregion
    }
}

