using System;

namespace DAQ.Environment
{
    /// <summary>
    /// 
    /// </summary>
    public class RbCaFFileSystem : DAQ.Environment.FileSystem
    {
        public RbCaFFileSystem()
        {
            Paths.Add("settingsPath", "C:\\Data\\Settings\\");
            Paths.Add("HardwareControllerCameraAttributesPath", "C:\\Data\\Settings\\CameraAttributes\\RbCaFHCCameraAttributes.txt");
            Paths.Add("CameraAttributesPath", "C:\\Data\\Settings\\CameraAttributes\\MOTMasterCameraAttributes.txt");
            Paths.Add("scriptListPath", "C:\\EDMSuite\\RbCaFScripts");
            Paths.Add("daqDLLPath","C:\\EDMSuite\\DAQ\\bin\\RbCaF\\DAQ.dll");
            Paths.Add("MOTMasterDataPath", "C:\\Data\\MOTMasterData\\");
            Paths.Add("HardwareClassPath", "C:\\EDMSuite\\DAQ\\RbCaFHardware.cs");
           // Paths.Add("mathPath", "c:/program files/wolfram research/mathematica/6.0/mathkernel.exe");
           // Paths.Add("fakeData", "d:\\mike\\data\\examples\\");
          //  Paths.Add("decelerationUtilitiesPath", "d:\\Mike\\Work\\");

          //  DataSearchPaths.Add(Paths["scanMasterDataPath"]);

          //  SortDataByDate = false;
        }
    }
}
