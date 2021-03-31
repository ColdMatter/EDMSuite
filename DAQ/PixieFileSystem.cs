using System;
using System.Collections.Generic;
using System.Text;

namespace DAQ
{
    class PixieFileSystem : DAQ.Environment.FileSystem
	{
        public PixieFileSystem()
		{
			//Paths.Add("mathPath", "c:/program files/wolfram research/mathematica/10.4/mathkernel.exe");
			Paths.Add("settingsPath", "D:\\Data\\settings\\");
            Paths.Add("scanMasterDataPath", "D:\\Data\\general\\");
            Paths.Add("edmDataPath", "D:\\Data\\sedm\\v3\\");
            Paths.Add("fakeData", "D:\\Data\\examples\\");
            Paths.Add("transferCavityData", "D:\\Data\\transfer cavity\\");

			DataSearchPaths.Add(Paths["scanMasterDataPath"]);
			DataSearchPaths.Add(Paths["edmDataPath"]);
			
			SortDataByDate = true;
		}
	}
}
