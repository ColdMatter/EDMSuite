using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

using DAQ.HAL;
using DAQ.Environment;

using NationalInstruments.Vision;
using NationalInstruments.Vision.Acquisition.Imaqdx;
using NationalInstruments.Vision.Internal;
using NationalInstruments.Vision.WindowsForms.Internal;

namespace SympatheticHardwareControl.CameraControl
{
    public class ImageFileIOManager
    {
        #region Saving and loading images
        // Saving the image
        public void SaveImageWithDialog(VisionImage image)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "shc images|*.jpg";
            saveFileDialog1.Title = "Save Image";
            String dataPath = (string)Environs.FileSystem.Paths["dataPath"];
            String dataStoreDir = dataPath + "SHC Single Images";
            saveFileDialog1.InitialDirectory = dataStoreDir;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog1.FileName != "")
                {
                    StoreImage(saveFileDialog1.FileName, image);
                }
            }
        }

        // Quietly.
        public void StoreImage(VisionImage image)
        {
            String dataPath = (string)Environs.FileSystem.Paths["dataPath"];
            String dataStoreFilePath = dataPath + "\\SHC Single Images\\tempImage.jpg";
            StoreImage(dataStoreFilePath, image);
        }



        public void StoreImage(String dataStoreFilePath, VisionImage image)
        {
            image.WriteJpegFile(dataStoreFilePath);
        }

        //Load image when opening the controller
        public VisionImage LoadImagesWithDialog()
        {
            VisionImage image = new VisionImage();
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "shc images|*.jpg";
            dialog.Title = "Load Image";
            String dataPath = (string)Environs.FileSystem.Paths["dataPath"];
            String dataStoreDir = dataPath + "SHC Single Images";
            dialog.InitialDirectory = dataStoreDir;
            dialog.ShowDialog();
            if (dialog.FileName != "") image = LoadImage(dialog.FileName);
            return image;
        }

        public VisionImage LoadImage()
        {
            String dataPath = (string)Environs.FileSystem.Paths["dataPath"];
            String dataStoreFilePath = dataPath + "\\SHC Single Images\\tempImage.jpg";
            VisionImage image = LoadImage(dataStoreFilePath);
            return image;

        }

        public VisionImage LoadImage(String dataStoreFilePath)
        {
            VisionImage image = new VisionImage();
            image.ReadFile(dataStoreFilePath);
            return image;

        }



        #endregion
    }
}
