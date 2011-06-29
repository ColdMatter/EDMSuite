using System;
using System.Collections.Generic;
using System.Text;

namespace SympatheticHardwareControl.CameraControl
{
    public interface CameraControlable
    {
        bool PrepareRemoteCameraControl();
        bool FinishRemoteCameraControl();
        byte[,] GrabImage(string cameraSettings);
        bool IsDone();
    }
}
