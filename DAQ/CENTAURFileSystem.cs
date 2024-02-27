using System;

namespace DAQ.Environment
{
    /// <summary>
    /// 
    /// </summary>
    public class CENTAURFileSystem : DAQ.Environment.FileSystem
    {
        public CENTAURFileSystem()
        {
            Paths.Add("settingsPath", "C:\\Users\\UEDM\\Documents\\EDM Suite Files\\Settings\\");
            Paths.Add("scanMasterDataPath", "C:\\Users\\UEDM\\Imperial College London\\Team ultracold - PH - Documents\\Data\\ScriptData\\");
            Paths.Add("fakeData", "C:\\Users\\UEDM\\Documents\\EDM Suite Files\\Examples\\");
            Paths.Add("UntriggeredCameraAttributesPath", "C:\\Users\\UEDM\\Documents\\EDM Suite Files\\Settings\\CameraAttributes\\SHCCameraAttributes.txt");
            Paths.Add("CameraAttributesPath", "C:\\Users\\UEDM\\Documents\\EDM Suite Files\\Settings\\CameraAttributes\\MOTMasterCameraAttributes.txt");
            Paths.Add("transferCavityData", "C:\\Users\\UEDM\\Documents\\EDM Suite Files\\Data\\TCL\\");
            Paths.Add("HardwareControllerDataPath", "C:\\Users\\UEDM\\Documents\\EDM Suite Files\\Data\\HardwareController\\");
            Paths.Add("edmDataPath", "C:\\Users\\UEDM\\Imperial College London\\Team ultracold - PH - Documents\\Data\\BlockData\\");
            DataSearchPaths.Add(Paths["scanMasterDataPath"]);
            DataSearchPaths.Add(Paths["edmDataPath"]);

            SortDataByDate = true;
        }
    }
}