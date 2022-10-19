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
			Paths.Add("settingsPath", "D:\\OneDrive - Imperial College London\\Data\\settings\\");
            Paths.Add("scanMasterDataPath", "D:\\OneDrive - Imperial College London\\Data\\general\\");
            Paths.Add("edmDataPath", "D:\\OneDrive - Imperial College London\\Data\\sedm\\v3\\");
            Paths.Add("fakeData", "D:\\OneDrive - Imperial College London\\Data\\examples\\");
            Paths.Add("transferCavityData", "D:\\OneDrive - Imperial College London\\Data\\transfer cavity\\");

			DataSearchPaths.Add(Paths["scanMasterDataPath"]);
			DataSearchPaths.Add(Paths["edmDataPath"]);
			
			SortDataByDate = true;
		}
	}
}
