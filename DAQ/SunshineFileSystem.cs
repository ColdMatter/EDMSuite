using System;

namespace DAQ.Environment
{
    public class SunshineFileSystem : DAQ.Environment.FileSystem
    {
        public SunshineFileSystem()
        {
            Paths.Add("settingsPath", "c:\\Control Programs\\Settings\\");
            Paths.Add("scanMasterDataPath", "c:\\Data\\LCMCaF\\");
            Paths.Add("mathPath", "c:/program files/wolfram research/mathematica/7.0/mathkernel.exe");
            Paths.Add("fakeData", "c:\\Data\\examples\\");
          //  Paths.Add("decelerationUtilitiesPath", "d:\\Tools\\");
            Paths.Add("vcoLockData", "c:\\Data\\VCO Lock\\");
            Paths.Add("transferCavityData", "c:\\Data\\LCMCaF\\TCL\\");

            Paths.Add("MOTMasterDataPath", "c:\\Data\\MOTMasterData\\");
            Paths.Add("scriptListPath", "C:\\Control Programs\\EDMSuite\\MoleculeMOTMasterScripts");
            Paths.Add("daqDLLPath", "C:\\Control Programs\\EDMSuite\\DAQ\\bin\\CaF\\daq.dll");
            Paths.Add("MOTMasterExePath", "C:\\Control Programs\\EDMSuite\\MOTMaster\\bin\\CaF\\");
            Paths.Add("UntriggeredCameraAttributesPath", "c:\\Data\\Settings\\CameraAttributes\\SHCCameraAttributes.txt");
            Paths.Add("CameraAttributesPath", "c:\\Data\\Settings\\CameraAttributes\\MOTMasterCameraAttributes.txt");
            Paths.Add("ExternalFilesPath", "\\\\PH-RAINBOW1\\CameraImages\\");
            Paths.Add("HardwareClassPath", "C:\\Control Programs\\EDMSuite\\DAQ\\MoleculeMOTHardware.cs");

            DataSearchPaths.Add(Paths["scanMasterDataPath"]);

            SortDataByDate = false;
        }
    }
}
