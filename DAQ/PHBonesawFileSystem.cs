using System;

namespace DAQ.Environment
{
    public class PHBonesawFileSystem : DAQ.Environment.FileSystem
    {
        public PHBonesawFileSystem()
        {
            Paths.Add("MOTMasterDataPath", "E:\\mot_master_data\\");
            Paths.Add("scriptListPath", "C:\\ControlPrograms\\EDMSuite\\MoleculeMOTMasterScripts");
            Paths.Add("daqDLLPath", "C:\\ControlPrograms\\EDMSuite\\DAQ\\bin\\CaF\\daq.dll");
            Paths.Add("MOTMasterExePath", "C:\\ControlPrograms\\EDMSuite\\MOTMaster\\bin\\CaF\\");
            Paths.Add("ExternalFilesPath", "C:\\Users\\cafmot\\Documents\\TempCameraImages");
            Paths.Add("HardwareClassPath", "C:\\ControlPrograms\\EDMSuite\\DAQ\\MoleculeMOTHardware.cs");
            Paths.Add("settingsPath", "C:\\ControlPrograms\\Settings\\ScanMaster\\");
            Paths.Add("scanMasterDataPath", "C:\\Users\\cafmot\\Box\\CaF MOT\\MOTData\\MOTScanMasterData\\");//where scan master will save data
            Paths.Add("fakeData", "C:\\ControlPrograms\\Settings\\Examples\\");
            Paths.Add("SourceLogPath", "E:\\Source_Log\\");
            Paths.Add("ToFFilesPath", "E:\\Source_Log\\ToF_Data\\");
            Paths.Add("transferCavityData", "E:\\TCL_DataLog\\");
            DataSearchPaths.Add(Paths["scanMasterDataPath"]);

            SortDataByDate = false;
        }
    }
}
