using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;


using DAQ.Environment;
using DAQ.HAL;
using DAQ.Remoting;
using NationalInstruments.DAQmx;
using System.Collections;

namespace CameraHardwareControl
{
    public class Controller : MarshalByRefObject
    {
        ControlWindow window;

        ArrayList parameters = new ArrayList();

        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        public void Start()
        {
            // get access to ScanMaster and the DecelerationHardwareController
          
            window = new ControlWindow();
            window.controller = this;
            Application.Run(window);
        }


        public void primeScan()
        {
            window.clearSequences();
           
        }
        public void primeAquisition(int numShots)
        {
            window.OpenCameraLink(numShots);
        }

        public void CameraArmAndWait(double scanParameter) {

            window.AquireSingleImage(scanParameter);
        }

        public void CloseCamera()
        {
            window.CloseCameraLink();
        }

        public void scanFinished()
        {
            window.DumpAndDisplay();
        }
    }
}