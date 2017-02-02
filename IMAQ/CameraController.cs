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
        private ImaqdxAttributeCollection attributes;
        public PointContour pointOfInterest;
        public Roi rectangleROI = new Roi();
        public Roi pointROI = new Roi();
        public bool roiSet = false;

        public CameraController(string cameraName)
        {
            this.cameraName = cameraName;
            windowShowing = false;
            image = new VisionImage();
            state = CameraState.FREE;
        }
        #endregion

        public void copyContoursToViewerROI()
        {
            try
            {
                Contour rectContour = rectangleROI.GetContour(0);
                rectContour.CopyTo(imageWindow.imageViewer.Roi);
            }
            catch
            {

            }

            try
            {
                Contour pointContour = pointROI.GetContour(0);
                pointContour.CopyTo(imageWindow.imageViewer.Roi);
            }
            catch
            {

            }
        }

        //public void addLineContoursToROI(LineContour line1, LineContour line2)
        //{
        //    Contour line1Contour = new Contour(line1);
        //    Contour line2Contour = new Contour(line2);
        //    line1Contour.CopyTo(imageWindow.imageViewer.Roi);
        //    line2Contour.CopyTo(imageWindow.imageViewer.Roi);
        //}

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
            if (state == CameraState.STREAMING) { StopStream(); }
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
                state = CameraState.READY_FOR_ACQUISITION;
                
                
                imaqdxSession.Sequence(images, numberOfShots);
                watch.Stop();
                if (windowShowing)
                {
                long interval = watch.ElapsedMilliseconds;
                imageWindow.WriteToConsole(interval.ToString());        
                }

                List<byte[,]> byteList = new List<byte[,]>();
                foreach (VisionImage i in images)
                {
                    byteList.Add((i.ImageToArray()).U8);

                   // if (windowShowing)
                    //{
                      //  imageWindow.AttachToViewer(i);

                   // }

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
        public void ShowSubtractedImage(byte[,] fgImage, byte[,] bgImage)
        {

            int height = fgImage.GetLength(0);
            int width = fgImage.GetLength(1);
            byte[,] subImage = new byte[height, width];
            if (fgImage.Length != bgImage.Length)
            {
                throw new Exception("Images not the same size");
            }
            for (int i=0;i<height;i++)
            {
                for (int j=0; j<width;j++)
                {
                    subImage[i, j] = (byte)(fgImage[i, j] - bgImage[i, j]); 
                }
                
            }
            image = new VisionImage();
            PixelValue2D array = new PixelValue2D(subImage);
            image.ArrayToImage(array);
            imageWindow.AttachToViewer(image);
            imageList.Add(image);
            imageWindow.WriteToConsole("Showing background subtracted image");
        }
        public void ClearImageList()
        {
            imageList.Clear();
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
            imageWindow.WriteToConsole("Attributes loaded in camera:");
            imageWindow.WriteToConsole(imaqdxSession.Attributes.WriteAttributesToString());
            foreach (string key in registers.Keys)
            {
                printRegisterInfo(registers[key]);
            }

        }

        public string SetCameraAttributes(string newPath)
        {
            lock (this)
            {
                if (attributes == null)
                {
                    imaqdxSession.Attributes.ReadAttributesFromFile(newPath);
                    attributes = imaqdxSession.Attributes;
                }
                else if (attributes != imaqdxSession.Attributes)
                {
                    attributes.WriteAttributesToFile(newPath);
                imaqdxSession.Attributes.ReadAttributesFromFile(newPath);
            }

            }
            return newPath;
        }

        public void SetROI()
        {
            RectangleContour rect = rectangleROI.GetBoundingRectangle();
            ImaqdxAttributeCollection attr = imaqdxSession.Attributes;
            attr[attr.IndexOf("OffsetX")].SetValue(rect.Left);
            attr[attr.IndexOf("OffsetY")].SetValue(rect.Top);
            attr[attr.IndexOf("Width")].SetValue(rect.Width);
            attr[attr.IndexOf("Height")].SetValue(rect.Height);
            roiSet = true;
        }

        public void ClearROI(int maxWidth,int maxHeight)
        {
            ImaqdxAttributeCollection attr = imaqdxSession.Attributes;
            attr[attr.IndexOf("OffsetX")].SetValue(0);
            attr[attr.IndexOf("OffsetY")].SetValue(0);
            attr[attr.IndexOf("Width")].SetValue(maxWidth);
            attr[attr.IndexOf("Height")].SetValue(maxHeight);
            attr[attr.IndexOf("PacketSize")].SetValue(8192);
            roiSet = false;
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
            catch (ImaqdxException e)
            {
                MessageBox.Show(e.Message+"Closing Camera...\n");
                imaqdxSession.Close();
                state = CameraState.FREE;
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
                    catch (Exception e)
                    {
                        MessageBox.Show("Something bad happened. Stopping the image stream.\n" + e.Message);
                        state = CameraState.FREE;
                        imaqdxSession.Acquisition.Stop();
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
                        if (!(e is ObjectDisposedException))
                        { imaqdxSession.Acquisition.Stop(); }
                        state = CameraState.FREE;
                        return;
                    }
                    if (state != CameraState.STREAMING)
                    {
                        state = CameraState.FREE;
                       
                        return;
                    }
                }
            }
        }

        #endregion

        #region Image Viewer (also private)

        private ImageViewerWindow imageWindow;
        public bool windowShowing;

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
                imageWindow.Close();
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
            String dataPath = (string)Environs.FileSystem.Paths["DataPath"];
            String dataStoreDir = dataPath + "SingleImages";
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
                saveFileDialog1.InitialDirectory = (string)Environs.FileSystem.Paths["DataPath"];

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

        public void StoreImageListWithDialog(bool rawData)
        {
            string filepath = GetSaveDialogFilename();
            if (!Directory.Exists(filepath))
            Directory.CreateDirectory(filepath);
            string filed = filepath + "\\image";
            StoreImageList(filed,rawData);
            imageWindow.WriteToConsole(filed);
        }

        public void StoreImageListWithDialog()
        {
            StoreImageListWithDialog(true);
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



        
        

        public void StoreImageList(string savePath, bool rawData)
        {
            
            for (int i = 0; i < imageList.Count; i++)
            {
                PixelValue2D pval = imageList[i].ImageToArray(); 
                if (rawData)
                    StoreImageData(savePath + "_" + i.ToString(), pval.U8);
                else
                    imageList[i].WritePngFile(savePath + "_" + i.ToString()+".png");
            }
            imageWindow.WriteToConsole("List of" + imageList.Count.ToString() + "images saved");
            }

        public void StoreImageList(string savePath)
        {
            StoreImageList(savePath, true);
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
            stream.Write(pixels, 0, width * height);
            stream.Dispose();

        }



       public void DisposeImages()
       {
           imageList.Clear();

       }


        #region Printing register values
     
        private void printRegisterInfo(string Adr)
        {
            imageWindow.WriteToConsole(Adr + " = " + Convert.ToString(imaqdxSession.ReadRegister((ulong)Convert.ToInt64(Adr, 16)), 2));
        }

        private void AddRegisters()
        {
            registers.Add("Shutter", "F0F0081C");
            registers.Add("Timebase", "F1000208");
            registers.Add("SEQUENCE_CTRL", "F1000220");
            registers.Add("DEFFERED", "F1000260");
            registers.Add("TRIG_CNTR", "F1000620");
            registers.Add("SEQUENCE_PARAM", "F1000224");
        }
        #endregion




        //Load image when opening the controller
        
        #endregion
    }
}
