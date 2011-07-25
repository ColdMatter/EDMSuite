using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Runtime.Remoting.Lifetime;
using System.Windows.Forms;

using DAQ.HAL;
using DAQ.Environment;

using NationalInstruments;
using NationalInstruments.Vision;
using NationalInstruments.Vision.Acquisition.Imaqdx;
using NationalInstruments.Vision.Internal;
using NationalInstruments.Vision.WindowsForms.Internal;


namespace SympatheticHardwareControl.CameraControl
{
    /// <summary>
    /// This keeps track of everything to do with images. It has 3 sections:
    /// the form (to display the image)
    /// an IMAQdxSession (the class which controls the camera)
    /// a VisionImage (the class that deals with image which the camera spits out)
    /// Although this stuff could actually be part of the hardware controller, I just shoved it all into one class.
    /// In principle, the hardware controller doesn't really need to know anything about IMAQ anymore.
    /// </summary>
    public class ImageMaster
    {
        public Controller controller;
        public bool Streaming;
        public VisionImage Image;
        public enum CameraState { FREE, BUSY, READY_FOR_ACQUISITION, STREAMING };
        private CameraState state = new CameraState();


        public ImageMaster(string cameraName, string attributesFile)
        {
            cameraAttributesFilePath = attributesFile;
            this.cameraName = cameraName;
            Streaming = false;
            windowShowing = false;
            Image = new VisionImage();
            state = CameraState.FREE;
        }
        #region ImageMaster functions

        public void Initialize()
        {
            initializeCamera();
            openViewerWindow();
        }

        public void Dispose()
        {
            disposeCamera();
            closeViewerWindow();
        }

        private object streamStopLock = new object();
        public bool Stream()
        {
            Thread streamThread = new Thread(new ThreadStart(stream));
            streamThread.Start();
            return true;
        }
        
        public bool StopStream()
        {
            if (Streaming)
            {
                Streaming = false;
            }
            return true;
        }

        public byte[,] Snapshot()
        {

            Image = new VisionImage();
            state = CameraState.READY_FOR_ACQUISITION;
            try
            {
                ImaqdxSession.Snap(Image);
                if (windowShowing)
                {
                    imageWindow.AttachToViewer(Image);
                }
                PixelValue2D pval = Image.ImageToArray();
                state = CameraState.FREE;
                return pval.U8;
            }
            catch (ObjectDisposedException e)
            {
                MessageBox.Show(e.Message);
                throw new TimeoutException();
            }
            
        }

        public byte[][,] TriggeredSequence(int numberOfShots)
        {            
            state = CameraState.READY_FOR_ACQUISITION;
            VisionImage[] images = new VisionImage[numberOfShots];
            try
            {
                ImaqdxSession.Sequence(images, numberOfShots);
                List<byte[,]> byteList = new List<byte[,]>();
                foreach (VisionImage i in images)
                {
                    byteList.Add((i.ImageToArray()).U8);
                }
                state = CameraState.FREE;
                return byteList.ToArray();
            }
            catch (ImaqdxException e)
            {
                MessageBox.Show(e.Message);
                throw new TimeoutException();
            }

        }

        public CameraState State
        {
            get { return state; }
        }
        #endregion

        #region ImaqdxSession (Camera Control)

        private string cameraName;
        private string cameraAttributesFilePath;
        public ImaqdxSession ImaqdxSession;

        private void initializeCamera()
        {
            try
            {
                ImaqdxSession = new ImaqdxSession(cameraName);
            }
            catch (ImaqdxException e)
            {
                MessageBox.Show(e.Message);
            }
            ImaqdxSession.Attributes.ReadAttributesFromFile(cameraAttributesFilePath);
        }

        private void disposeCamera()
        {
            ImaqdxSession.Dispose();
        }

        public void SetCameraAttributes()
        {
            ImaqdxSession.Attributes.ReadAttributesFromFile(cameraAttributesFilePath);
        }

        public void SetCameraAttributes(string newPath)
        {
            ImaqdxSession.Attributes.ReadAttributesFromFile(newPath);
        }

        private void stream()
        {
            Streaming = true;
            Image = new VisionImage();
            try
            {
                ImaqdxSession.ConfigureGrab();
            }
            catch (ObjectDisposedException e)
            {
                MessageBox.Show(e.Message);
                return;
            }
            for (; ; )
            {
                lock (streamStopLock)
                {
                    try
                    {
                        ImaqdxSession.Grab(Image, true);
                    }
                    catch (ImaqdxException e)
                    {
                        MessageBox.Show("ImaqdxException. \n Did you try to control the camera while it was streaming...?\n Stopping camera now. \n" + e.Message);
                        Streaming = false;
                        return;
                    }
                    catch (InvalidOperationException e)
                    {
                        MessageBox.Show("Something bad happened. Stopping the image stream.\n" + e.Message);
                        Streaming = false;
                        return;
                    }
                    try
                    {
                        if (windowShowing)
                        {
                            imageWindow.AttachToViewer(Image);
                        }
                    }
                    catch (InvalidOperationException e)
                    {
                        MessageBox.Show("I have a leftover image without anywhere to display it. Dumping...\n\n" + e.Message);
                        Streaming = false;
                        return;
                    }
                    if (!Streaming)
                    {
                        ImaqdxSession.Acquisition.Stop();
                        return;
                    }
                }
            }
        }

        #endregion

        #region Image Viewer

        public ImageViewerWindow imageWindow;
        bool windowShowing;


        public void openViewerWindow()
        {
            if (!windowShowing)
            {
                imageWindow = new ImageViewerWindow();
                imageWindow.IM = this;
                imageWindow.Show();
                windowShowing = true;
            }
        }

        private void closeViewerWindow()
        {
            if (windowShowing)
            {
                windowShowing = false;
            }
        }

        #endregion

        #region Saving and loading images
        // Saving the image
        public void SaveImageWithDialog()
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "shc images|*.png";
            saveFileDialog1.Title = "Save Image";
            String dataPath = (string)Environs.FileSystem.Paths["dataPath"];
            String dataStoreDir = dataPath + "SHC Single Images";
            saveFileDialog1.InitialDirectory = dataStoreDir;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog1.FileName != "")
                {
                    StoreImage(saveFileDialog1.FileName);
                }
            }
        }

        // Quietly.
        public void StoreImage()
        {
            String dataPath = (string)Environs.FileSystem.Paths["dataPath"];
            String dataStoreFilePath = dataPath + "\\SHC Single Images\\tempImage.png";
            StoreImage(dataStoreFilePath);
        }



        public void StoreImage(String dataStoreFilePath)
        {
            Image.WritePngFile(dataStoreFilePath);
        }

        //Load image when opening the controller
        public VisionImage LoadImagesWithDialog()
        {
            VisionImage image = new VisionImage();
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "shc images|*.png";
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
            String dataStoreFilePath = dataPath + "\\SHC Single Images\\tempImage.png";
            Image = LoadImage(dataStoreFilePath);
            return Image;

        }

        public VisionImage LoadImage(String dataStoreFilePath)
        {
            Image.ReadFile(dataStoreFilePath);
            if (windowShowing)
            {
                imageWindow.AttachToViewer(Image);
            }
            return Image;

        }
        #endregion
    }
}
