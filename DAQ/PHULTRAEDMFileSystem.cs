using System;

namespace DAQ.Environment
{
    /// <summary>
    /// 
    /// </summary>
    public class PHULTRAEDMFileSystem : DAQ.Environment.FileSystem
    {
        public PHULTRAEDMFileSystem()
        {
            Paths.Add("settingsPath", "C:\\Users\\ultraedm\\Documents\\EDM Suite Files\\Settings\\");
            Paths.Add("scanMasterDataPath", "D:\\OneDriveData\\OneDrive - Imperial College London\\LatticeEDM\\data"); //This defines where the data is saved
            Paths.Add("fakeData", "C:\\Users\\ultraedm\\Documents\\EDM Suite Files\\Examples\\");
            //Paths.Add("UntriggeredCameraAttributesPath", "C:\\Users\\ultraedm\\Documents\\EDM Suite Files\\Settings\\CameraAttributes\\SHCCameraAttributes.txt");
            //Paths.Add("CameraAttributesPath", "C:\\Users\\ultraedm\\Documents\\EDM Suite Files\\Settings\\CameraAttributes\\MOTMasterCameraAttributes.txt");
            //Paths.Add("transferCavityData", "C:\\Users\\ultraedm\\Documents\\EDM Suite Files\\Data\\TCL\\");
            DataSearchPaths.Add(Paths["scanMasterDataPath"]);
            //Paths.Add("HardwareControllerDataPath", "C:\\Users\\ultraedm\\Box\\UltracoldEDM\\data\\");
            SortDataByDate = false;
        }
    }
}