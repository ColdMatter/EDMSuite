using System;
using System.Collections.Generic;
using System.Text;

namespace SympatheticHardwareControl.CameraControl
{
    public interface CameraControlable
    {
        byte[,] GrabImage(string cameraSettings);
    }
}
