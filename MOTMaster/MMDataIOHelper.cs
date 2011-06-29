using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Forms;

namespace MOTMaster
{
    public class MMDataIOHelper
    {
        MMDataZipper zipper = new MMDataZipper();
        private string motMasterDataPath;
        private string element;

        public MMDataIOHelper(string motMasterDataPath, string element)
        {
            this.motMasterDataPath = motMasterDataPath;
            this.element = element;
        }



        public void StoreRun(string saveFolder, int batchNumber, string pathToPattern, string pathToHardwareClass,
            Dictionary<String, Object> dict, Dictionary<String, Object> report,
            string cameraAttributesPath, byte[,] imageData)
        {
            string fileTag = getDataID(element, batchNumber);

            saveToFiles(fileTag, saveFolder, batchNumber, pathToPattern, pathToHardwareClass, dict, report, cameraAttributesPath, imageData);

            putCopiesOfFilesToZip(saveFolder, fileTag);

            deleteFiles(saveFolder, fileTag);
        }

        private void deleteFiles(string saveFolder, string fileTag)
        {
            File.Delete(saveFolder + fileTag + "_parameters.txt");
            File.Delete(saveFolder + fileTag + "_script.cs");
            File.Delete(saveFolder + fileTag + ".png");
            File.Delete(saveFolder + fileTag + "_cameraParameters.txt");
            File.Delete(saveFolder + fileTag + "_hardwareClass.cs");
            File.Delete(saveFolder + fileTag + "_hardwareReport.txt");
        }
        private void putCopiesOfFilesToZip(string saveFolder, string fileTag)
        {
            System.IO.FileStream fs = new FileStream(saveFolder + fileTag + ".zip", FileMode.Create);
            zipper.PrepareZip(fs);
            zipper.AppendToZip(saveFolder, fileTag + "_parameters.txt");
            zipper.AppendToZip(saveFolder, fileTag + "_script.cs");
            zipper.AppendToZip(saveFolder, fileTag + ".png");
            zipper.AppendToZip(saveFolder, fileTag + "_cameraParameters.txt");
            zipper.AppendToZip(saveFolder, fileTag + "_hardwareClass.cs");
            zipper.AppendToZip(saveFolder, fileTag + "_hardwareReport.txt");
            zipper.CloseZip();
            fs.Close();
        }
        private void saveToFiles(string fileTag, string saveFolder, int batchNumber, string pathToPattern, string pathToHardwareClass,
            Dictionary<String, Object> dict, Dictionary<String, Object> report,
            string cameraAttributesPath, byte[,] imageData)
        {
            storeDictionary(saveFolder + fileTag + "_parameters.txt", dict);
            File.Copy(pathToPattern, saveFolder + fileTag + "_script.cs");
            File.Copy(pathToHardwareClass, saveFolder + fileTag + "_hardwareClass.cs");
            storeCameraAttributes(saveFolder + fileTag + "_cameraParameters.txt", cameraAttributesPath);
            storeImage(saveFolder + fileTag + ".png", imageData);
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

        private void storeDictionary(String dataStoreFilePath, Dictionary<string, object> dict)
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

        
    }
}
