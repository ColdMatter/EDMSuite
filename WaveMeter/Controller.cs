using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;


using DAQ;
using DAQ.Environment; 
using DAQ.HAL;

using IMAQ;
using System.IO;





namespace WaveMeter
{
    public class Controller 
    {
        #region Constants
        //the default path doesn't work on this computer. For now, just use the defauly path given in MAX
        private static string cameraAttributesPath = "C:\\Users\\Public\\Documents\\National Instruments\\NI-IMAQdx\\Data\\Microsoft® LifeCam Studio(TM)  (#4A66879F76B0B7F6).icd";
        //private static string cameraAttributesPath = (string)Environs.FileSystem.Paths["CameraAttributesPath"];

        #endregion

        #region Setup
       
        public ControlWindow window;

        public CameraController ImageController;

        public ImageViewerWindow Imageviewer;
        private WavemeterFitterHelper Fitter;
        private NewWavemeterfitterhelper NewFitter;
        private class cameraNotFoundException : ArgumentException { };



        #endregion 




        public void Start()
        {





            // make the control window
            window = new ControlWindow();
            StartCameraControl();
            Imageviewer = new ImageViewerWindow();
            window.controller = this;

            Fitter = new WavemeterFitterHelper();
            NewFitter = new NewWavemeterfitterhelper();

      

            Application.Run(window);

        }


        #region camera control

        public void StartCameraControl()
        {
            try
            {
                ImageController = new CameraController("cam1");
                //This is used to analyse each image during a live stream
                ImageController.analyse = true;
                ImageController.Initialize();
                ImageController.SetCameraAttributes(cameraAttributesPath);
                ImageController.PrintCameraAttributesToConsole();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Camera Initialization Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();

            }
        }



      

        public void CameraSnapshot()
        {
            try
            {
 
              byte[,] imageData=ImageController.SingleSnapshot(cameraAttributesPath);
              Fitter.Datapoints = Getthearray.convertTO1D(imageData);
              Fitter.Datapoints = Getthearray.smooth(Fitter.Datapoints);
              
            // SaveImageWithDialog();
                

            }
            catch (Exception e){
            MessageBox.Show(e.Message);}
        }

        public void CameraStream()
        {
                ImageController.Stream(cameraAttributesPath);
        }
       
        public void StopCameraStream()
        {
            ImageController.StopStream();

        }


        #endregion


        #region Saving Images and Image Data

        public void SaveImageWithDialog()
        {
            ImageController.SaveImageWithDialog();
        }
        public void SaveImage(string path)
        {
            ImageController.SaveImage(path);
        }

        public void StoreImageDataWithDialog()
        {

            ImageController.StoreImageListWithDialog();
        }

        public void DisposeImages()
        {
            ImageController.DisposeImages();
        }
        #endregion

        #region Data Process and Plot

        double[] xdata, ydata;

        
        public void displyData()
        {   
            ydata = Fitter.Datapoints;

            
           
            Fitter.Position = Getthearray.CreateArray(0, ydata.GetLength(0));

            xdata = Fitter.Position;






            window.addDataToGraph(xdata,ydata);

        }
        
        public void Displyfit()
        { 
            
            double [] fittedvalue=new double[2000];


            ydata = Fitter.Datapoints;
            Fitter.Position = Getthearray.CreateArray(0, ydata.GetLength(0));
            xdata = Fitter.Position;
            fittedvalue = Fitter.Fittedvalues();
            xdata=Getthearray.CreateArray(0,fittedvalue.GetLength(0));
            window.addextradata(xdata,fittedvalue);
            double c1 = Fitter.center();


            Fitter.datamassage();
            fittedvalue = Fitter.Fittedvalues();
            window.addmoredata(xdata, fittedvalue);
            double c2 = Fitter.center();
            
            
            double tempdistance = c1 - c2;
            if (tempdistance < 0) tempdistance = tempdistance * -1;

            string text = tempdistance.ToString();

            string path = @"C:\Users\Andy\Desktop\Report.txt";

            // This text is added only once to the file.
            if (!File.Exists(path))
            {
                // Create a file to write to.
                string[] createText = { "Time and Fitting Parameters" };
                File.WriteAllLines(path, createText, Encoding.UTF8);
            }

            // This text is always added, making the file longer over time
            // if it is not deleted.
            string appendText = text + Environment.NewLine;
            File.AppendAllText(path, appendText, Encoding.UTF8);

            // Open the file to read from.
            // string[] readText = File.ReadAllLines(path, Encoding.UTF8);
            //foreach (string s in readText)
            //{
            //   Console.WriteLine(s);
            // }








        }

        public double Getthedistance()
        {

            double[] fittedvalue = new double[2000];


            ydata = Fitter.Datapoints;
            Fitter.Position = Getthearray.CreateArray(0, ydata.GetLength(0));
            xdata = Fitter.Position;
            fittedvalue = Fitter.Fittedvalues();
            xdata = Getthearray.CreateArray(0, fittedvalue.GetLength(0));
            window.addextradata(xdata, fittedvalue);
            double c1 = Fitter.center();


            Fitter.datamassage();
            fittedvalue = Fitter.Fittedvalues();
            window.addmoredata(xdata, fittedvalue);
            double c2 = Fitter.center();
            


            double tempdistance = c1 - c2;
            if (tempdistance < 0) tempdistance = tempdistance * -1;

            string text = tempdistance.ToString();

            string path = @"C:\Users\Andy\Desktop\Report.txt";
            
            // This text is added only once to the file.
            if (!File.Exists(path))
            {
                // Create a file to write to.
                string[] createText = { "Width" };
                File.WriteAllLines(path, createText, Encoding.UTF8);
            }

            // This text is always added, making the file longer over time
            // if it is not deleted.
            string appendText = text + Environment.NewLine;
            File.AppendAllText(path, appendText, Encoding.UTF8);

            // Open the file to read from.
            // string[] readText = File.ReadAllLines(path, Encoding.UTF8);
            //foreach (string s in readText)
            //{
            //   Console.WriteLine(s);
            // }


            return tempdistance;





        }        
      
        #endregion


        #region mean and standard devation

        public void calculation(double[] data)
        {
            double sum = 0;
            double n = 0;
            for (int i=0;i<data.GetLength(0);i++)
            {
                if (data[i]>0)
                {
                    sum = sum + data[i];
                    n = n + 1;

                }
               

            }

            double mean = sum / n;

             sum = 0;
            for (int i=0;i<data.GetLength(0);i++)
            {
                if (data[i] > 0)
                {

                    sum = sum + (data[i] - mean) * (data[i] - mean);

                }
            }

            double std = Math.Sqrt(sum /n);

            Console.WriteLine(mean);
            

            string text = "Mean: " + mean.ToString() +
                    " Standard Devation: " + std.ToString("G3");

            string path = @"C:\Users\Andy\Desktop\Report.txt";

            // This text is added only once to the file.
            if (!File.Exists(path))
            {
                // Create a file to write to.
                string[] createText = { "Width" };
                File.WriteAllLines(path, createText, Encoding.UTF8);
            }

            // This text is always added, making the file longer over time
            // if it is not deleted.
            string appendText = text + Environment.NewLine;
            File.AppendAllText(path, appendText, Encoding.UTF8);

            // Open the file to read from.
            // string[] readText = File.ReadAllLines(path, Encoding.UTF8);
            //foreach (string s in readText)
            //{
            //   Console.WriteLine(s);
            // }

        }

        #endregion


    }
}
