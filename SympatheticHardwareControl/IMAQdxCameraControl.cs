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
    public class IMAQdxCameraControl
    {
        public IMAQdxCameraControl(string cameraName, string attributesFile)
        {
            cameraAttributesFilePath = attributesFile;
            this.cameraName = cameraName;
        }


        private string cameraName;
        private string cameraAttributesFilePath;
        public ImaqdxSession Session;


        #region CameraControllable Members


        public void InitializeCamera()
        {
            Session = new ImaqdxSession(cameraName);
            Session.Attributes.ReadAttributesFromFile(cameraAttributesFilePath);
        }

        public void CloseCamera()
        {
            Session.Acquisition.Unconfigure();
            Session.Acquisition.Dispose();
            Session.Close();
        }

        public void SetCameraAttributes()
        {
            Session.Attributes.ReadAttributesFromFile(cameraAttributesFilePath);
        }

        public void SetCameraAttributes(string newPath)
        {
            Session.Attributes.ReadAttributesFromFile(newPath);
        }

        #endregion
    }
}
