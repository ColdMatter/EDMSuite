using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;

namespace DAQ.Remoting
{

    public class RemotingHelper
    {

        public static void ConnectScanMaster()
        {
            RemotingConfiguration.RegisterWellKnownClientType(
                    Type.GetType("ScanMaster.Controller, ScanMaster"),
                    "tcp://localhost:1170/controller.rem"
                    );
        }

        public static void ConnectBlockHead()
        {
            RemotingConfiguration.RegisterWellKnownClientType(
                    Type.GetType("BlockHead.Controller, BlockHead"),
                    "tcp://localhost:1171/controller.rem"
                    );
        }

        public static void ConnectEDMHardwareControl()
        {
            RemotingConfiguration.RegisterWellKnownClientType(
                    Type.GetType("EDMHardwareControl.Controller, EDMHardwareControl"),
                    "tcp://localhost:1172/controller.rem"
                    );
        }

        public static void ConnectBufferGasHardwareControl()
        {
            RemotingConfiguration.RegisterWellKnownClientType(
                    Type.GetType("BuffergasHardwareControl.Controller, BuffergasHardwareControl"),
                    "tcp://localhost:1178/controller.rem"
                    );
        }

        public static void ConnectPhaseLock()
        {
            RemotingConfiguration.RegisterWellKnownClientType(
                    Type.GetType("PhaseLock.MainForm, PhaseLock"),
                    "tcp://localhost:1175/controller.rem"
                    );
        }

        public static void ConnectLaserLock()
        {
            RemotingConfiguration.RegisterWellKnownClientType(
                Type.GetType("LaserLock.LaserController, LaserLock"),
                "tcp://localhost:1176/controller.rem"
                );
        }

        public static void ConnectMoleculeMOTHardwareControl()
        {
            RemotingConfiguration.RegisterWellKnownClientType(
                    Type.GetType("MoleculeMOTHardwareControl.Controller, MoleculeMOTHardwareControl"),
                    "tcp://localhost:1172/controller.rem"
                    );
        }

        public static void ConnectSympatheticHardwareControl()
        {
            RemotingConfiguration.RegisterWellKnownClientType(
                    Type.GetType("SympatheticHardwareControl.Controller, SympatheticHardwareControl"),
                    "tcp://localhost:1180/controller.rem"
                    );
        }

        public static void ConnectMOTMaster()
        {
            RemotingConfiguration.RegisterWellKnownClientType(
                    Type.GetType("MOTMaster.Controller, MOTMaster"),
                    "tcp://localhost:1187/controller.rem"
                    );
        }

    }
}
