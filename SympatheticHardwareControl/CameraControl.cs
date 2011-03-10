using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Runtime.Remoting.Lifetime;
using System.Windows.Forms;
using NationalInstruments;
using NationalInstruments.DAQmx;
using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;
using NationalInstruments.VisaNS;

using DAQ.HAL;
using DAQ.Environment;

using NationalInstruments.Vision;
using NationalInstruments.Vision.Acquisition.Imaqdx;
using NationalInstruments.Vision.Internal;
using NationalInstruments.Vision.WindowsForms.Internal;

namespace SympatheticHardwareControl
{
    class CameraControl
    {
        public CameraControl(string CN, string CAFP)
        {
            CameraAttributesFilePath = CAFP;
            CameraName = CN;
        }


        private string cameraName;
        private string cameraAttributesFilePath;
        private ImaqdxSession session;
        private bool busy;
        

        public string CameraName
        {
            get
            {
                return cameraName;
            }
            set
            {
                cameraName = value;
            }
        }
        public string CameraAttributesFilePath
        {
            get
            {
                return cameraAttributesFilePath;
            }
            set
            {
                cameraAttributesFilePath = value;
            }
        }
        public ImaqdxSession Session
        {
            get
            {
                return session;
            }
            set
            {
                
                session = value;
            }
        }

        public bool Busy
        {
            get
            {
                return busy;
            }
            set
            {
                busy = value;
            }
        }

        public void InitializeCameraControl()
        {
            Session = new ImaqdxSession(CameraName);
            Session.Attributes.ReadAttributesFromFile(CameraAttributesFilePath);
            
        }

        public void CloseCameraControl()
        {
            Session.Acquisition.Unconfigure();
            Session.Acquisition.Dispose();
            Session.Close();
        }

        public void UpdateCameraAttributes()
        {
            Session.Attributes.ReadAttributesFromFile(CameraAttributesFilePath);
        }
        public void UpdateCameraAttributes(string newPath)
        {
            CameraAttributesFilePath = newPath;
            Session.Attributes.ReadAttributesFromFile(CameraAttributesFilePath);
        }

    }
}
