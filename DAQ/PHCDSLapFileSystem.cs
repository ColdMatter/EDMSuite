using System;

namespace DAQ.Environment
{
    /// <summary>
    /// 
    /// </summary>
    public class PHCDSLapFileSystem : DAQ.Environment.FileSystem
    {
        public PHCDSLapFileSystem()
        {
            Paths.Add("settingsPath", "d:\\files\\data\\settings");
            Paths.Add("scanMasterDataPath", "d:\\files\\data\\Scan master data");
            Paths.Add("mathPath", "d:/program files/wolfram research/mathematica/5.2/mathkernel.exe");
            Paths.Add("fakeData", "d:\\files\\data\\examples");

            DataSearchPaths.Add(Paths["scanMasterDataPath"]);

            SortDataByDate = false;
        }
    }
}
