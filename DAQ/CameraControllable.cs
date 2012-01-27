using System;
using System.Collections.Generic;
using System.Text;

namespace DAQ
{
    /// <summary>
    /// An interface for something that can control a camera.
    /// 
    /// </summary>
    public interface CameraControllable
    {
        void PrepareRemoteCameraControl();
        void FinishRemoteCameraControl();
        byte[,] GrabSingleImage(string cameraSettings);
        byte[][,] GrabMultipleImages(string cameraSettings, int numberOfShots);
        bool IsReadyForAcquisition();
        void SaveImage(String dataStoreFilePath);
        void SaveImageWithDialog();
    }
}
