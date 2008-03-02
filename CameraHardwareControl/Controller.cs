using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;


using DAQ.Environment;
using DAQ.HAL;
using DAQ.Remoting;
using NationalInstruments.DAQmx;

namespace CameraHardwareControl
{
    public class Controller : MarshalByRefObject
    {
        ControlWindow window;

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

        public void primeAquisition(int numShots)
        {
            window.aquireSequence(numShots);
        }

        //public void haltAndDisplay()
        //{
        //    window.haltAndDisplay;
        //}

        public void scanFinished()
        {
            window.DumpAndDisplay();
        }
    }
}