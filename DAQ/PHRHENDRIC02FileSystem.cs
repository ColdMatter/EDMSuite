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
            Paths.Add("settingsPath", "d:\\scanmaster\\settings\\");
            Paths.Add("scanMasterDataPath", "d:\\data\\");
            Paths.Add("fakeData", "d:\\scanmaster\\examples\\");

            DataSearchPaths.Add(Paths["scanMasterDataPath"]);

            SortDataByDate = false;
        }
    }
}
