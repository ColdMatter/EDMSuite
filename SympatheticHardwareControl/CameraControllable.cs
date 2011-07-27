using System;
using System.Collections.Generic;
using System.Text;

namespace SympatheticHardwareControl.CameraControl
{
    public interface CameraControllable
    {
        void PrepareRemoteCameraControl();
        void FinishRemoteCameraControl();
        byte[,] GrabSingleImage(string cameraSettings);
        byte[][,] GrabMultipleImages(string cameraSettings, int numberOfShots);
        bool IsReadyForAcquisition();
    }
}
