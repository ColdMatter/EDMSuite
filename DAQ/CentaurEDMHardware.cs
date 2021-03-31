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
    /// This is the specific hardware that the edm machine has. This class conforms
    /// to the Hardware interface.
    /// </summary>
    public class CentaurEDMHardware : DAQ.HAL.Hardware
    {
       public override void ConnectApplications()
       {
       }
 

        public CentaurEDMHardware()
        {
        }

    }
}