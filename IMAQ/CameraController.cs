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
using System.Windows.Media;
using System.Windows.Media.Imaging;


using DAQ.HAL;
using DAQ.Environment;

using NationalInstruments;
using NationalInstruments.Vision;
using NationalInstruments.Vision.Acquisition.Imaqdx;
using NationalInstruments.Vision.Internal;
using NationalInstruments.Vision.WindowsForms.Internal;
//using NationalInstruments.Vision.Common;


namespace IMAQ
{
    /// <summary>
    /// This keeps track of everything to do with images. It has 3 sections:
    /// -the form (to display the image)
    /// -an IMAQdxSession (the class which controls the camera)
    /// -a VisionImage (the class that deals with image data)
    /// You can now build this into a hardware controller and it knows how to use a camera!
    /// The hardware controller doesn't really need to know anything about IMAQ anymore.
    /// </summary>
    public class CameraController
    {
        #region Setup
        public VisionImage image;
        public enum CameraState { FREE, BUSY, READY_FOR_ACQUISITION, STREAMING, ACQUISITION_TERMINATED };
        private CameraState state = new CameraState();
        private object streamStopLock = new object();
        public List<VisionImage> imageList = new List<VisionImage>();
        public bool analyse = false;
        private double max = 0;


        public CameraController(string cameraName)
        {
            this.cameraName = cameraName;
            windowShowing = false;
            image = new VisionImage();
            state = CameraState.FREE;
        }
        #endregion

        #region ImageController functions (Public stuff)

        public void Initialize()
        {
            try
            {
                initializeCamera();
                openViewerWindow();
            }
            catch { }
            
        }

        public void Dispose()
        {
            imaqdxSession.Dispose();
            closeViewerWindow();
        }

        public bool Stream(string cameraAttributesFilePath)
        {
            SetCameraAttributes(cameraAttributesFilePath);
            imageWindow.WriteToConsole("Applied camera attributes from " + cameraAttributesFilePath);
            imageWindow.WriteToConsole("Streaming from camera");
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
        
        public void StopStream()
        {
            if (state == CameraState.STREAMING)
            {
                state = CameraState.BUSY;
            }
            imageWindow.WriteToConsole("Streaming stopped");
        }

        public byte[,] SingleSnapshot(string attributesPath)
        {
            return SingleSnapshot(attributesPath, false);
        }

        public byte[,] SingleSnapshot(string attributesPath, bool addToImageList)
        {
            imageWindow.WriteToConsole("Taking snapshot");
            imageWindow.WriteToConsole("Applied camera attributes from " + attributesPath);
            SetCameraAttributes(attributesPath);
            try
            {

                if (state == CameraState.FREE || state == CameraState.READY_FOR_ACQUISITION)
                {
                    image = new VisionImage();
                    
                    state = CameraState.READY_FOR_ACQUISITION;
                    try
                    {
                        imaqdxSession.Snap(image);
                       
                        if (windowShowing)
                        {
                            imageWindow.AttachToViewer(image);
                     
                        }
                        if (addToImageList)
                        {
                            imageList.Add(image);
                        }
                        image.WriteFile("test.bmp");
                        
                        PixelValue2D pval = image.ImageToArray();
                        byte[,] u8array = Getthearray.convertToU8(pval.Rgb32);
                        double max = Getthearray.Findthemaximum(u8array);
                        imageWindow.WriteToConsole(max.ToString("F6"));
                        state = CameraState.FREE;
                        return u8array;
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
                    catch (VisionException e)
                    {
                        MessageBox.Show(e.VisionErrorText);
                        throw e;
                    }
                }
                else return null;

            }
            catch (TimeoutException)
            {
                return null;
            }
        }

      
        
        public byte[][,] MultipleSnapshot(string attributesPath, int numberOfShots)
        {
            SetCameraAttributes(attributesPath);
            VisionImage[] images = new VisionImage[numberOfShots];
            Stopwatch watch = new Stopwatch();
            try
            {

                watch.Start();
                
                //state = CameraState.READY_FOR_ACQUISITION;              
                //imaqdxSession.Sequence(images, numberOfShots);
                imaqdxSession.Acquisition.Configure(ImaqdxAcquisitionType.SingleShot, numberOfShots);
                imaqdxSession.Acquisition.Start();
                state = CameraState.READY_FOR_ACQUISITION;
                
                watch.Stop();
                if (windowShowing)
                {
                long interval = watch.ElapsedMilliseconds;
                imageWindow.WriteToConsole(interval.ToString());        
                }

                List<byte[,]> byteList = new List<byte[,]>();
                for (uint i = 0; i < images.Length; ++i)
                {
                    //byteList.Add((i.ImageToArray()).U8);
                    imaqdxSession.Acquisition.GetImageAt(images[i], i);
                   // if (windowShowing)
                    //{
                      //  imageWindow.AttachToViewer(i);

                   // }

                }
                imaqdxSession.Acquisition.Stop();
                imaqdxSession.Acquisition.Unconfigure();
                imaqdxSession.Acquisition.Dispose();
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

        public void PrintCameraAttributesToConsole()
        {
            imageWindow.WriteToConsole(imaqdxSession.Attributes.WriteAttributesToString());
            imageWindow.WriteToConsole("Attributes loaded in camera:");
        }

        public string SetCameraAttributes(string newPath)
        {
            lock (this)
            {
                imaqdxSession.Attributes.ReadAttributesFromFile(newPath);
            }
            return newPath;
        }

        #endregion

        #region imaqdxSession (Camera Control. Should be all private)

        private string cameraName;
        private ImaqdxSession imaqdxSession;

        private void initializeCamera()
        {
            try
            {
                imaqdxSession = new ImaqdxSession(cameraName);
            }
            catch (ImaqdxException e)
            {
                MessageBox.Show(e.Message);
                throw new ImaqdxException();
            }

        }

        private void Getthemaximum()
        {
            PixelValue2D pval = image.ImageToArray();
            byte[,] u8array = Getthearray.convertToU8(pval.Rgb32);
                       
        }

        private void stream()
        {
            image = new VisionImage();
            try
            {
                imaqdxSession.ConfigureGrab();
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
                        imaqdxSession.Grab(image, true);
                        if (analyse)
                        {
                            PixelValue2D pval = image.ImageToArray();
                            byte[,] u8array = Getthearray.convertToU8(pval.Rgb32);
                            max = Getthearray.Findthemaximum(u8array);
                            imageWindow.WriteToConsole(max.ToString("F6"));

                        }
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
                            imageWindow.AttachToViewer(image);
                        }
                    }
                    catch (InvalidOperationException e)
                    {
                        MessageBox.Show("I have a leftover image without anywhere to display it. Dumping...\n\n" + e.Message);
                        imaqdxSession.Acquisition.Stop();
                        state = CameraState.FREE;
                        return;
                    }
                    if (state != CameraState.STREAMING)
                    {
                        imaqdxSession.Acquisition.Stop();
                        state = CameraState.FREE;
                        return;
                    }
                }
            }
        }

        #endregion

        #region Image Viewer (also private)

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

        #region Saving images (Public functions)
        // Saving the image
        public void SaveImageWithDialog()
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "shc images|*.png";
            saveFileDialog1.Title = "Save Image";
            String dataPath = (string)Environs.FileSystem.Paths["dataPath"];
            String dataStoreDir = dataPath + "Single Images";
            saveFileDialog1.InitialDirectory = dataStoreDir;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog1.FileName != "")
                {
                    SaveImage(saveFileDialog1.FileName);
                }
            }
        }

      

        public string GetSaveDialogFilename()
        {
            string file = "";
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Title = "Save image data";

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    if (saveFileDialog1.FileName != "")
                    {
                        file = saveFileDialog1.FileName;
                    }
                }
            }
            return file;
        }

        public void StoreImageListWithDialog()
        {
            string filepath = GetSaveDialogFilename();
            string filetext = Path.GetFileName(filepath);
            Directory.CreateDirectory(filepath);
            string filed = filepath+"\\"+filetext;
            StoreImageList(filed);
            imageWindow.WriteToConsole(filed);
        }



        // Quietly.
        public void SaveImage()
        {
            String dataPath = (string)Environs.FileSystem.Paths["dataPath"];
            String dataStoreFilePath = dataPath + "\\Single Images\\tempImage.png";
            SaveImage(dataStoreFilePath);
        }



        public void SaveImage(String dataStoreFilePath)
        {
            image.WritePngFile(dataStoreFilePath);
            imageWindow.WriteToConsole("Image saved");
        }

        //public void storeImage(string savePath, byte[][,] imageData)
        //{
        //    for (int i = 0; i < imageData.Length; i++)
        //    {
        //        storeImage(savePath + "_" + i.ToString(), imageData[i]);
        //    }
        //    imageWindow.WriteToConsole("Imagedata saved");
        //}

        //public void storeImage(string savePath, byte[,] imageData)
        //{
        //    int width = imageData.GetLength(1);
        //    int height = imageData.GetLength(0);
        //    byte[] pixels = new byte[width * height];
        //    for (int j = 0; j < height; j++)
        //    {
        //        for (int i = 0; i < width; i++)
        //        {
        //            pixels[(width * j) + i] = imageData[j, i];
        //        }
        //    }
        //    // Define the image palette
        //    BitmapPalette myPalette = BitmapPalettes.Gray256Transparent;

        //    // Creates a new empty image with the pre-defined palette

        //    BitmapSource image = BitmapSource.Create(
        //      width,
        //     height,
        //     96,
        //     96,
        //     PixelFormats.Indexed8,
        //     myPalette,
        //    pixels,
        //     width);

        //    FileStream stream = new FileStream(savePath + ".dat", FileMode.Create);
        //    stream.Write(pixels, 0, width * height);

        //    PngBitmapEncoder encoder = new PngBitmapEncoder();
        //    encoder.Interlace = PngInterlaceOption.On;
        //    encoder.Frames.Add(BitmapFrame.Create(image));
        //    encoder.Save(stream);

        //    stream.Dispose();

        //}



        
        

        public void StoreImageList(string savePath)
        {
            
            for (int i = 0; i < imageList.Count; i++)
            {
                PixelValue2D pval = imageList[i].ImageToArray(); 
                StoreImageData(savePath+"_" + i.ToString(), pval.U8);
            }
            imageWindow.WriteToConsole("List of"+ imageList.Count.ToString() +"images saved");
        }

       public void StoreImageData(string savePath, byte[,] imageData)
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
         

            FileStream stream = new FileStream(savePath + ".dat", FileMode.Create);
            stream.Write(pixels,0,width*height);
            stream.Dispose();

        }



       public void DisposeImages()
       {
           imageList.Clear();

       }

     





        //Load image when opening the controller
        
        #endregion
    }
}
