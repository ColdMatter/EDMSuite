using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Runtime.Remoting.Lifetime;
using System.Windows.Forms;
using System.Diagnostics;

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

        public VisionImage Image;
        public enum CameraState { FREE, BUSY, READY_FOR_ACQUISITION, STREAMING, ACQUISITION_TERMINATED };
        private CameraState state = new CameraState();
        


        public ImageMaster(string cameraName, string attributesFile)
        {
            cameraAttributesFilePath = attributesFile;
            this.cameraName = cameraName;
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
            ImaqdxSession.Dispose();
            closeViewerWindow();
        }

        private object streamStopLock = new object();
        public bool Stream()
        {
            if (state == CameraState.FREE)
            {
                state = CameraState.STREAMING;
                Thread streamThread = new Thread(new ThreadStart(stream));
                streamThread.Start();
                return true;
            }
            else
            {
                return false;
            }
        }
        
        /// <summary>
        /// This is to stop the Grab loop. It sets the camera state from "Streaming" to "Busy". In the loop, there is an 
        /// if-statement to check for this.
        /// The thread then waits until the camera is free again.
        /// </summary>
        /// <returns></returns>
        public void StopStream()
        {
            if (state == CameraState.STREAMING)
            {
                state = CameraState.BUSY;
            }
        }

        public byte[,] Snapshot()
        {
            if (state == CameraState.FREE)
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
                    throw new ObjectDisposedException("");
                }
                catch (ImaqdxException e)
                {
                    MessageBox.Show(e.Message);
                    throw new ImaqdxException();
                }
            }
            else return null;
        }

        public byte[][,] TriggeredSequence(int numberOfShots)
        {

            VisionImage[] images = new VisionImage[numberOfShots];
            Stopwatch watch = new Stopwatch();
            try
            {

                watch.Start();
                state = CameraState.READY_FOR_ACQUISITION;
                
                ImaqdxSession.Sequence(images, numberOfShots);
                watch.Stop();
                long interval = watch.ElapsedMilliseconds;
                controller.PrintIntervalInConsole(interval);
                

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
                state = CameraState.FREE;
                throw new TimeoutException();
            }

        }

        public bool IsReadyForAcqisition()
        {
            if (state == CameraState.READY_FOR_ACQUISITION)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool IsCameraFree()
        {
            if (state == CameraState.FREE)
            {
                return true;
            }
            else
            {
                return false;
            }
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

        }

        public string SetCameraAttributes(string newPath)
        {
            lock (this)
            {
                cameraAttributesFilePath = newPath;
                
                ImaqdxSession.Attributes.ReadAttributesFromFile(newPath);

            }
            return newPath;
        }

        private void stream()
        {
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
                    catch (InvalidOperationException e)
                    {
                        MessageBox.Show("Something bad happened. Stopping the image stream.\n" + e.Message);
                        state = CameraState.FREE;
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
                        ImaqdxSession.Acquisition.Stop();
                        state = CameraState.FREE;
                        return;
                    }
                    if (state != CameraState.STREAMING)
                    {
                        ImaqdxSession.Acquisition.Stop();
                        state = CameraState.FREE;
                        return;
                    }
                }
            }
        }

        #endregion

        #region Image Viewer (private)

        private ImageViewerWindow imageWindow;
        bool windowShowing;

        private void openViewerWindow()
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

        #region Saving and loading images (Public functions)
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
        public void LoadImagesWithDialog()
        {
            VisionImage image = new VisionImage();
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "shc images|*.png";
            dialog.Title = "Load Image";
            String dataPath = (string)Environs.FileSystem.Paths["dataPath"];
            String dataStoreDir = dataPath + "SHC Single Images";
            dialog.InitialDirectory = dataStoreDir;
            dialog.ShowDialog();
            if (dialog.FileName != "") LoadImage(dialog.FileName);

        }

        public void LoadImage()
        {
            String dataPath = (string)Environs.FileSystem.Paths["dataPath"];
            String dataStoreFilePath = dataPath + "\\SHC Single Images\\tempImage.png";
            LoadImage(dataStoreFilePath);
            //return Image;

        }

        public void LoadImage(String dataStoreFilePath)
        {
            Image.ReadFile(dataStoreFilePath);
            if (windowShowing)
            {
                imageWindow.AttachToViewer(Image);
            }
            //return Image;

        }
        #endregion
    }
}
