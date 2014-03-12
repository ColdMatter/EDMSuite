using System;

namespace DAQ.Environment
{
    public class SunshineFileSystem : DAQ.Environment.FileSystem
    {
        public SunshineFileSystem()
        {
            Paths.Add("settingsPath", "c:\\Control Programs\\");
            Paths.Add("scanMasterDataPath", "c:\\Data\\LCMCaF");
            Paths.Add("mathPath", "c:/program files/wolfram research/mathematica/7.0/mathkernel.exe");
            Paths.Add("fakeData", "c:\\Data\\examples\\");
          //  Paths.Add("decelerationUtilitiesPath", "d:\\Tools\\");
            Paths.Add("vcoLockData", "c:\\Data\\VCO Lock\\");

            DataSearchPaths.Add(Paths["scanMasterDataPath"]);

            SortDataByDate = false;
        }
    }
}
