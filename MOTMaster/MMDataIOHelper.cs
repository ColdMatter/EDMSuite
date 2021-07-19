﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Runtime.Serialization.Json;

using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Forms;

using DAQ;
using DAQ.HAL;

namespace MOTMaster
{
    public class MMDataIOHelper
    {
        MMDataZipper zipper = new MMDataZipper();
        private string motMasterDataPath;
        private string element;
        string[] files;

        public MMDataIOHelper(string motMasterDataPath, string element)
        {
            this.motMasterDataPath = motMasterDataPath;
            this.element = element;
        }

        public void StoreRun(string saveFolder, int batchNumber, string pathToPattern, string pathToHardwareClass,
            MOTMasterSequence sequence, Dictionary<String, Object> dict, Dictionary<String, Object> report,
            string cameraAttributesPath, byte[][,] imageData, string externalFilesPath, string externalFilePattern)
        {
            string fileTag = getDataID(saveFolder, element, batchNumber);
            string ToFFilesPath = (string)DAQ.Environment.Environs.FileSystem.Paths["ToFFilesPath"];
            
            saveCameraData(fileTag, saveFolder, cameraAttributesPath, imageData);
            saveToFiles(fileTag, saveFolder, batchNumber, pathToPattern, pathToHardwareClass, dict, report, sequence);

            string[] datafiles = Directory.GetFiles(saveFolder, fileTag + "*");
            
            files = datafiles;
            AddFilesToZip(externalFilesPath, externalFilePattern);
            AddFilesToZip(ToFFilesPath, "*.txt");

            string[] filesCopied = putCopiesOfFilesToZip(saveFolder, fileTag, externalFilesPath, externalFilePattern);
            
            deleteFiles(files);
            

        }
        public void StoreRun(string saveFolder, int batchNumber, string pathToPattern, string pathToHardwareClass,
            MOTMasterSequence sequence, Dictionary<String, Object> dict, Dictionary<String, Object> report, string externalFilesPath, string externalFilePattern)
        {
            string fileTag = getDataID(saveFolder, element, batchNumber);
            string ToFFilesPath = (string)DAQ.Environment.Environs.FileSystem.Paths["ToFFilesPath"];
            
            saveToFiles(fileTag, saveFolder, batchNumber, pathToPattern, pathToHardwareClass, dict, report, sequence);

            string[] datafiles = Directory.GetFiles(saveFolder, fileTag + "*");
            files = datafiles;
            AddFilesToZip(externalFilesPath, externalFilePattern);
            AddFilesToZip(ToFFilesPath, "*.txt");
            
            string[] filesCopied = putCopiesOfFilesToZip(saveFolder, fileTag, externalFilesPath, externalFilePattern);
            
            deleteFiles(files);
            
        }
        private void deleteFiles(string[] files)
        {
            foreach (string s in files)
            {
                File.Delete(s);
            }
        }

        private void AddFilesToZip(string externalFilesPath, string externalFilePattern)
        {
            if (externalFilesPath != null && externalFilePattern != null)
            {
                string[] externalFiles = Directory.GetFiles(externalFilesPath, externalFilePattern);
                files = files.Concat(externalFiles).ToArray();
            }
        }

        // fileTag is the name tag of the files generated by MOTMaster. 
        // externalFilePattern is a filename pattern for files generated by external programs to be zipped up with all the other files (e.g. "*.tif" for image files generated by an external camera control program)
        private string[] putCopiesOfFilesToZip(string saveFolder, string fileTag, string externalFilesPath, string externalFilePattern)
        {
            
            System.IO.FileStream fs = new FileStream(saveFolder + fileTag + ".zip", FileMode.Create);
            zipper.PrepareZip(fs);
            foreach (string s in files)
            {
                zipper.AppendToZip(s);
            } 
            zipper.CloseZip();
            fs.Close();
            return files;
        }

        private void saveCameraData(string fileTag, string saveFolder, string cameraAttributesPath, byte[][,] imageData)
        {
            storeCameraAttributes(saveFolder + fileTag + "_cameraParameters.txt", cameraAttributesPath);
            storeImage(saveFolder + fileTag, imageData);
        }

        private void saveToFiles(string fileTag, string saveFolder, int batchNumber, string pathToPattern, string pathToHardwareClass,
            Dictionary<String, Object> dict, Dictionary<String, Object> report, MOTMasterSequence sequence)
        {
            storeDigitalPattern(saveFolder + fileTag + "_digitalPattern.json", sequence);
            storeAnalogPattern(saveFolder + fileTag + "_analogPattern.json", sequence);
            storeDictionary(saveFolder + fileTag + "_parameters.txt", dict);
            File.Copy(pathToPattern, saveFolder + fileTag + "_script.cs");
            File.Copy(pathToHardwareClass, saveFolder + fileTag + "_hardwareClass.cs");
            storeDictionary(saveFolder + fileTag + "_hardwareReport.txt", report);
        }

        public string SelectSavedScriptPathDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "DataSets|*.zip";
            dialog.Title = "Load previously saved pattern";
            dialog.Multiselect = false;
            dialog.InitialDirectory = motMasterDataPath;
            dialog.ShowDialog();
            return dialog.FileName;
        }

        public void UnzipFolder(string path)
        {
            zipper.Unzip(path);
        }

        public Dictionary<string, object> LoadDictionary(string dictionaryPath)
        {
            string[] parameterStrings = File.ReadAllLines(dictionaryPath);
            Dictionary<string, object> dict = new Dictionary<string, object>();
            char separator = '\t';
            foreach (string str in parameterStrings)
            {
                string[] keyValuePairs = str.Split(separator);
                Type t = System.Type.GetType(keyValuePairs[2]);
                dict.Add(keyValuePairs[0], Convert.ChangeType(keyValuePairs[1], t));
            }
            return dict;
        }

        public void DisposeReplicaScript(string folderPath)
        {
            Directory.Delete(folderPath, true);
        }


        private void storeCameraAttributes(string savePath, string attributesPath)
        {
            File.Copy(attributesPath, savePath);
        }

        private void storeImage(string savePath, byte[][,] imageData)
        {
            for (int i = 0; i < imageData.Length; i++)
            {
                storeImage(savePath + "_" + i.ToString(), imageData[i]);
            }
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

            FileStream stream = new FileStream(savePath + ".png", FileMode.Create);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Interlace = PngInterlaceOption.On;
            encoder.Frames.Add(BitmapFrame.Create(image));
            encoder.Save(stream);
            stream.Dispose();

        }

        private void storeDictionary(String dataStoreFilePath, Dictionary<string, object> dict)
        {
            if (dict != null)
            {
                TextWriter output = File.CreateText(dataStoreFilePath);
                foreach (KeyValuePair<string, object> pair in dict)
                {
                    output.Write(pair.Key);
                    output.Write('\t');
                    output.Write(pair.Value.ToString());
                    output.Write('\t');
                    output.WriteLine(pair.Value.GetType());
                }
                output.Close();
            }


        }

        private void storeDigitalPattern(string dataStoreFilePath, MOTMasterSequence sequence)
        {
            Hashtable digitalChannelsHash = DAQ.Environment.Environs.Hardware.DigitalOutputChannels;

            string patternGeneratorBoard = (string)DAQ.Environment.Environs.Hardware.GetInfo("PatternGeneratorBoard");
            Dictionary<string, string> additionalPGs = (Dictionary<string, string>)DAQ.Environment.Environs.Hardware.GetInfo("AdditionalPatternGeneratorBoards");
            Dictionary<string, int> digitalChannels = new Dictionary<string,int>();
            string digitalPatternString;
            Dictionary<string, int>[] additionaldigitalChannels = new Dictionary<string,int>[additionalPGs.Count];
            string[] additionaldigitalPatternString = new string[additionalPGs.Count];
            int pgIndex;

            for (int i = 0; i < additionalPGs.Count; i++)
                additionaldigitalChannels[i] = new Dictionary<string, int>();

            foreach (DictionaryEntry pair in digitalChannelsHash)
            {
                
                if (((DigitalOutputChannel)pair.Value).Device == patternGeneratorBoard)
                    digitalChannels.Add((string)pair.Key, ((DigitalOutputChannel)pair.Value).BitNumber);
                else
                {
                    pgIndex = 0;
                    foreach (string pg in additionalPGs.Values)
                    {
                        if (((DigitalOutputChannel)pair.Value).Device == pg && sequence.DigitalPattern.Boards.Keys.Contains<string>(pg))
                            additionaldigitalChannels[pgIndex].Add((string)pair.Key, ((DigitalOutputChannel)pair.Value).BitNumber);
                        pgIndex++;
                    }
                }
            }

            var settings = new DataContractJsonSerializerSettings();
            settings.UseSimpleDictionaryFormat = true; // Make format of json file {key : value} instead of {"Key": key, "Value": value}
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Dictionary<string, int>), settings);

            digitalPatternString = sequence.DigitalPattern.Boards[patternGeneratorBoard].Layout.ToString();
            
            TextWriter output = File.CreateText(dataStoreFilePath);
            output.Write("{\"" + patternGeneratorBoard + "\":");
            output.Write("{");
            output.Write("\"channels\":");
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, digitalChannels);
                output.Write(Encoding.Default.GetString(ms.ToArray()));
            }

            output.Write(",");
            output.Write("\"pattern\":");
            output.Write("\"");
            output.Write(System.Web.HttpUtility.JavaScriptStringEncode(digitalPatternString));
            output.Write("\"");

            output.Write("}");
            output.Write("}");

            output.Write("\n");

            pgIndex = 0;
            foreach (string pg in additionalPGs.Values)
            {
                if (sequence.DigitalPattern.Boards.Keys.Contains<string>(pg))
                {
                    additionaldigitalPatternString[pgIndex] = sequence.DigitalPattern.Boards[pg].Layout.ToString();
                    output.Write("{\"" + pg + "\":");
                    output.Write("{");
                    output.Write("\"channels\":");
                    using (MemoryStream ms = new MemoryStream())
                    {
                        serializer.WriteObject(ms, additionaldigitalChannels[pgIndex]);
                        output.Write(Encoding.Default.GetString(ms.ToArray()));
                    }

                    output.Write(",");
                    output.Write("\"pattern\":");
                    output.Write("\"");
                    output.Write(System.Web.HttpUtility.JavaScriptStringEncode(additionaldigitalPatternString[pgIndex]));
                    output.Write("\"");

                    output.Write("}");
                    output.Write("}");
                }
            }

            output.Close();
        }

        private void storeAnalogPattern(string dataStoreFilePath, MOTMasterSequence sequence)
        {
            Dictionary<String, Dictionary<Int32, Double>> analogPatterns = sequence.AnalogPattern.AnalogPatterns;

            var settings = new DataContractJsonSerializerSettings();
            settings.UseSimpleDictionaryFormat = true; // Make format of json file {key : value} instead of {"Key": key, "Value": value}

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Dictionary<String, Dictionary<Int32, Double>>), settings);
            TextWriter output = File.CreateText(dataStoreFilePath);
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, analogPatterns);
                output.Write(Encoding.Default.GetString(ms.ToArray()));
                output.Close();
            }
        }

        //private void storeAnalogPattern(string dataStoreFilePath, MOTMasterSequence sequence)
        //{
        //    TextWriter output = File.CreateText(dataStoreFilePath);
        //    string analogPattern = sequence.AnalogPattern.AnalogPatterns;
        //    output.Write(digitalPatternString);
        //}
        
        private string getDataID(string directory, string element, int batchNumber)
        {
            DateTime dt = DateTime.Now;
            string dateTag;
            string batchTag;
            int subTag = 0;

            dateTag = String.Format("{0:ddMMMyy}", dt);
            batchTag = batchNumber.ToString().PadLeft(2, '0');
            subTag = (Directory.GetFiles(directory, element +
                dateTag + batchTag + "*.zip")).Length;
            string id = element + dateTag + batchTag
                + "_" + subTag.ToString().PadLeft(3, '0');
            return id;
        }

        
    }
}
