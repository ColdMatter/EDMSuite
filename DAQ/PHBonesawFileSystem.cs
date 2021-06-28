using System;

namespace DAQ.Environment
{
    public class PHBonesawFileSystem : DAQ.Environment.FileSystem
    {
        public PHBonesawFileSystem()
        {
            Paths.Add("MOTMasterDataPath", "D:\\mot_master_data\\");
            Paths.Add("scriptListPath", "C:\\ControlPrograms\\EDMSuite\\MoleculeMOTMasterScripts");
            Paths.Add("daqDLLPath", "C:\\ControlPrograms\\EDMSuite\\DAQ\\bin\\CaF\\daq.dll");
            Paths.Add("MOTMasterExePath", "C:\\ControlPrograms\\EDMSuite\\MOTMaster\\bin\\CaF\\");
            Paths.Add("ExternalFilesPath", "C:\\Users\\cafmot\\Documents\\Temp Camera Images\\");
            Paths.Add("HardwareClassPath", "C:\\ControlPrograms\\EDMSuite\\DAQ\\MoleculeMOTHardware.cs");
            Paths.Add("settingsPath", "C:\\ControlPrograms\\Settings\\ScanMaster\\");
            Paths.Add("scanMasterDataPath", "C:\\Users\\cafmot\\Box\\CaF MOT\\MOTData\\MOTScanMasterData\\");//where scan master will save data
            Paths.Add("fakeData", "C:\\ControlPrograms\\Settings\\Examples\\");
            Paths.Add("SourceLogPath", "D:\\Source_Log\\");
            Paths.Add("ToFFilesPath", "D:\\Source_Log\\ToF_Data\\");

            DataSearchPaths.Add(Paths["scanMasterDataPath"]);

            SortDataByDate = false;
        }
    }
}
