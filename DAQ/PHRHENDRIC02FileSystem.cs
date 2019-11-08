using System;

namespace DAQ.Environment
{
    /// <summary>
    /// 
    /// </summary>
    public class PHRHENDRIC02FileSystem : DAQ.Environment.FileSystem
    {
        public PHRHENDRIC02FileSystem()
        {
            Paths.Add("settingsPath", "d:\\Settings\\");
            Paths.Add("scanMasterDataPath", "d:\\Data\\");
            Paths.Add("fakeData", "d:\\Examples\\");
            Paths.Add("UntriggeredCameraAttributesPath", "d:\\Settings\\CameraAttributes\\SHCCameraAttributes.txt");
            Paths.Add("CameraAttributesPath", "d:\\Settings\\CameraAttributes\\MOTMasterCameraAttributes.txt");
            Paths.Add("transferCavityData", "d:\\Data\\TCL\\");
            DataSearchPaths.Add(Paths["scanMasterDataPath"]);
            SortDataByDate = false;
        }
    }
}
