using System;
using System.Collections;
using System.Runtime.Remoting;
using NationalInstruments.DAQmx;

using DAQ.Pattern;
using System.Collections.Generic;
using DAQ.TransferCavityLock2012;
using DAQ.Remoting;

namespace DAQ.HAL
{
    /// <summary>
    /// This is the specific hardware that the edm test crate has. This class conforms
    /// to the Hardware interface.
    /// </summary>
    public class EDMTestCrateHardware : DAQ.HAL.Hardware
    {
                public override void ConnectApplications()
       {

       }


       public EDMTestCrateHardware()
        {

            // add the boards
            Boards.Add("rfPulseGenerator", "Dev2");
           
        }

    }
}