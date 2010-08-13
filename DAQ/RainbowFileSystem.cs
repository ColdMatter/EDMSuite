using System;
using System.Collections.Generic;
using System.Text;

namespace DAQ
{
    class RainbowFileSystem : DAQ.Environment.FileSystem
	{
        public RainbowFileSystem()
		{
			Paths.Add("mathPath", "c:/program files/wolfram research/mathematica/7.0/mathkernel.exe");
			Paths.Add("settingsPath", "C:\\Users\\SrF\\Files\\Data\\Settings\\");
            Paths.Add("scanMasterDataPath", "C:\\Users\\SrF\\Files\\Data\\General\\");
            Paths.Add("fakeData", "C:\\Users\\SrF\\Files\\Data\\Examples\\");

			DataSearchPaths.Add(Paths["scanMasterDataPath"]);
			
			SortDataByDate = true;
		}
	}
}
