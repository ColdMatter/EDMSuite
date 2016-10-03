using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ
{
    class TEW105FileSystem : DAQ.Environment.FileSystem
    {
        public TEW105FileSystem()
        {
            Paths.Add("MOTMasterDataPath", "c:\\Data\\MOTMasterData\\");
            Paths.Add("scriptListPath", "C:\\Users\\rfmot\\EDMSuite\\RFMOTMOTMasterScripts\\");
            Paths.Add("daqDLLPath", "C:\\Users\\rfmot\\EDMSuite\\DAQ\\bin\\RFMOT\\daq.dll");
            Paths.Add("MOTMasterExePath",
                "C:\\Users\\rfmot\\EDMSuite\\MOTMaster\\bin\\RFMOT\\");
            Paths.Add("UntriggeredCameraAttributesPath", "c:\\Data\\Settings\\CameraAttributes\\RFMOTCameraAttributes.txt");
            Paths.Add("CameraAttributesPath", "c:\\Data\\Settings\\CameraAttributes\\MOTMasterCameraAttributes.txt");
            Paths.Add("HardwareClassPath", "C:\\Users\\rfmot\\EDMSuite\\DAQ\\RFMOTHardware.cs");
            Paths.Add("settingsPath", "c:\\Data\\Settings\\");
            Paths.Add("DataPath", "c:\\Data\\");
        }
    }
}
