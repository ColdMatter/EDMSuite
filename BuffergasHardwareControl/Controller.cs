using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using DAQ;
using DAQ.Environment;
using DAQ.HAL;
using NationalInstruments.DAQmx;

using IMAQ;



namespace BuffergasHardwareControl
{


    public class Controller : MarshalByRefObject, CameraControllable
    {

        #region Constants

        private static string notriggerCameraAttributesPath = (string)Environs.FileSystem.Paths["UntriggeredCameraAttributesPath"];
        private static string cameraAttributesPath = (string)Environs.FileSystem.Paths["CameraAttributesPath"];
        private static string profilesPath = (string)Environs.FileSystem.Paths["settingsPath"]
           + "BuffergasHardwareController\\";
        #endregion

        #region Setup
        public double flowControlVoltage;
        public double flowInputVoltage;
        public byte[][,] imageData;
        public int frameCount;
        public int scanCount;




        //delare that there will be a control window
       public ControlWindow window;

        //Camera
        public CameraController ImageController;

        //private bool sHCUIControl// public enum SHCUIControlState { OFF, LOCAL, REMOTE };
       // public SHCUIControlState HCState = new SHCUIControlState();

        private class cameraNotFoundException : ArgumentException { };

        //set up a task for an analog output and analog input channel
      
        private Task outputTask = new Task("FlowControllerOutput");
        private AnalogOutputChannel flowChannel = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["laser"];
        public AnalogSingleChannelWriter flowWriter;

        
        private Task inputTask = new Task("FlowMeterInput");
        private AnalogInputChannel flowmeterChannel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["pressure1"];
        public AnalogSingleChannelReader flowReader;

       
       



        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.

        public override Object InitializeLifetimeService()
        {
            return null;
        }

        public void Start()
        {


            flowChannel.AddToTask(outputTask, 0, 5);
            outputTask.Control(TaskAction.Verify);
            flowWriter = new AnalogSingleChannelWriter(outputTask.Stream);

            flowmeterChannel.AddToTask(inputTask, 0, 5);
            inputTask.Control(TaskAction.Verify);
            flowReader = new AnalogSingleChannelReader(inputTask.Stream);





           // make the control window
           window = new ControlWindow();
           window.controller = this;



            Application.Run(window);

        }

      #endregion

      //  #region Saving and loading experimental parameters
      //   //Saving the parameters when closing the controller
      //  public void SaveParametersWithDialog()
      //  {
      //      SaveFileDialog saveFileDialog1 = new SaveFileDialog();
      //      saveFileDialog1.Filter = "shc parameters|*.bin";
      //      saveFileDialog1.Title = "Save parameters";
      //      saveFileDialog1.InitialDirectory = profilesPath;
      //      if (saveFileDialog1.ShowDialog() == DialogResult.OK)
      //      {
      //          if (saveFileDialog1.FileName != "")
      //          {
      //              StoreParameters(saveFileDialog1.FileName);
      //          }
      //      }
      //  }

      //private void StoreParameters(String dataStoreFilePath)
      //  {
      //      stateRecord = readValuesOnUI();
      //      BinaryFormatter s = new BinaryFormatter();
      //      FileStream fs = new FileStream(dataStoreFilePath, FileMode.Create);
      //     try
      //     {
      //          s.Serialize(fs, dataStore);
      //          s.Serialize(fs, stateRecord);
      //      }
      //      catch (Exception)
      //      {
      //          Console.Out.WriteLine("Saving failed");
      //      }
      //      finally
      //      {
      //          fs.Close();
      //          window.WriteToConsole("Saved parameters to " + dataStoreFilePath);
      //      }

      //  }

      //  Load parameters when opening the controller
      //  public void LoadParametersWithDialog()
      //  {
      //      OpenFileDialog dialog = new OpenFileDialog();
      //      dialog.Filter = "shc parameters|*.bin";
      //      dialog.Title = "Load parameters";
      //      dialog.InitialDirectory = profilesPath;
      //      dialog.ShowDialog();
      //      if (dialog.FileName != "") stateRecord = loadParameters(dialog.FileName);
      //      setValuesDisplayedOnUI(stateRecord);
      //  }

       
      //  #endregion

        #region Remoting stuff

        /// <summary>
        /// This is used when you want another program to take control of some/all of the hardware. The hc then just saves the
        /// last hardware state, then prevents you from making any changes to the UI. Use this if your other program wants direct control of hardware. In the buffer gas case this is only used for the camera at the moment. 
        /// </summary>
        public void StartRemoteControl()
        {
           // if (HCState == SHCUIControlState.OFF)
            //{
                if (!ImageController.IsCameraFree())
                {
                    StopCameraStream();
                    window.WriteToConsole("Remoting Started!");
                }
               // StoreParameters(profilesPath + "tempParameters.bin");
               // HCState = SHCUIControlState.REMOTE;
              // window.UpdateUIState(HCState);
               
            //}
            else
            {
                MessageBox.Show("Controller is busy");
            }

        }
        public void StopRemoteControl()
        {
          //  try
          //  {
                window.WriteToConsole("Remoting Stopped!");
                //setValuesDisplayedOnUI(loadParameters(profilesPath + "tempParameters.bin"));

              //  if (System.IO.File.Exists(profilesPath + "tempParameters.bin"))
              //  {
               //     System.IO.File.Delete(profilesPath + "tempParameters.bin");
               // }
           // }
           // catch (Exception)
           // {
           //     window.WriteToConsole("Unable to load Parameters.");
           // }
            //HCState = SHCUIControlState.OFF;
           // window.UpdateUIState(HCState);
            //ApplyRecordedStateToHardware();
        }

        #endregion

        #region Flow
        public double FlowControlVoltage
        {
            get { return flowControlVoltage; }
            set
            {
                flowControlVoltage = value;
                flowWriter.WriteSingleSample(true, value);
                outputTask.Control(TaskAction.Unreserve);
            }
        }


        public double FlowInputVoltage
        {
            get
            {
                flowInputVoltage = flowReader.ReadSingleSample();
                inputTask.Control(TaskAction.Unreserve);
                return flowInputVoltage;
            }

        }

        #endregion

        #region Local camera control

        public void StartCameraControl()
        {
            try
            {
                ImageController = new CameraController("cam1");
                ImageController.Initialize();
                ImageController.PrintCameraAttributesToConsole();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Camera Initialization Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();

            }
        }



        public void CameraStream()
        {
            try
            {
                ImageController.Stream(notriggerCameraAttributesPath);
            }
            catch { }
        }

        public void StopCameraStream()
        {
            try
            {
                ImageController.StopStream();
            }
            catch { }
        }

        public void CameraSnapshot()
        {
            try
            {
                ImageController.SingleSnapshot(notriggerCameraAttributesPath, true);
            }
            catch { }
        }

        #endregion

        #region Remote Camera Control
        //Written for taking images triggered by TTL. This "Arm" sets the camera so it's expecting a TTL by using the camera attributes camerAttributesPath.

        public void CameraPrimeAcquisition()
        {
        }

        public void CloseCamera()
        {
        }

        /*Gets and returns a single image as a byte array. Settings don't do anything at the moment*/
        public byte[,] GrabSingleImage(string settings)
        {
            return ImageController.SingleSnapshot(cameraAttributesPath,true);
           
        }

        /*Takes several images and returns them as a list of byte arrays. Settings don't do anything at the moment*/
        public byte[][,] GrabMultipleImages(string settings, int numberOfShots)
        {
            try
            {
                byte[][,] images = ImageController.MultipleSnapshot(cameraAttributesPath, numberOfShots);
               
                return images;
            }

            catch (TimeoutException)
            {
                FinishRemoteCameraControl();
                return null;
            }

        }

        public bool IsReadyForAcquisition()
        {
            return ImageController.IsReadyForAcqisition();
        }

        public void PrepareRemoteCameraControl()
        {
            StartRemoteControl();
        }
        public void FinishRemoteCameraControl()
        {
            StopRemoteControl();
        }
        
      
        
        #endregion

        #region CameraControl

        ///// <summary>
        ///// -Camera control functions copied from Sean's MOTMaster Controller, not needed for Buffergas Hardware Controller (Camera control is run through the hardware controller. All MOTMaster knows 
        ///// about it a function called "GrabImage(string cameraSettings)". If the camera attributes are 
        ///// set so that it needs a trigger, MOTMaster will have to deliver that too.
        ///// It'll expect a byte[,] or byte[][,] (if there are several images) as a return value.
        ///// 
        ///// -At the moment MOTMaster won't run without a camera nor with 
        ///// more than one. In the long term, we might 
        ///// want to fix this.)
        ///// </summary>
        ///// 
        //int nof;
        
        //CameraControllable camera;

        //public void GrabImage(int numberOfFrames)
        //{
        //    nof = numberOfFrames;
        //    Thread LLEThread = new Thread(new ThreadStart(grabImage));
        //    LLEThread.Start();

        //}

        //bool imagesRecieved = false;
        ///*private byte[,] imageData;
        //private void grabImage()
        //{
        //    imagesRecieved = false;
        //    imageData = (byte[,])camera.GrabSingleImage(cameraAttributesPath);
        //    imagesRecieved = true;
        //}*/
      
        //private void grabImage()
        //{
        //    imagesRecieved = false;
        //    imageData = camera.GrabMultipleImages(cameraAttributesPath, nof);
        //    imagesRecieved = true;
        //}
        //public class DataNotArrivedFromHardwareControllerException : Exception { };
        //private bool waitUntilCameraAquisitionIsDone()
        //{
        //    while (!imagesRecieved)
        //    { Thread.Sleep(10); }
        //    return true;
        //}
        //private bool waitUntilCameraIsReadyForAcquisition()
        //{
        //    while (!camera.IsReadyForAcquisition())
        //    { Thread.Sleep(10); }
        //    return true;
        //}
        //private void prepareCameraControl()
        //{
        //    camera.PrepareRemoteCameraControl();
        //}
        //private void finishCameraControl()
        //{
        //    camera.FinishRemoteCameraControl();
        //}
        //private void checkDataArrived()
        //{
        //    if (imageData == null)
        //    {
        //        MessageBox.Show("No data. Something's Wrong.");
        //        throw new DataNotArrivedFromHardwareControllerException();
        //    }
        //}
        #endregion

        #region  Run Camera 


        //public void RunCamera(int nos)
        //{
           
        //        prepareCameraControl();

        //        GrabImage(nos);

        //        waitUntilCameraIsReadyForAcquisition();

        //}



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
    }
}
