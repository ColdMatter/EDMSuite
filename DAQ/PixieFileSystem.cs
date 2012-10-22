using System;
using System.Collections.Generic;
using System.Text;

namespace DAQ
{
    class PixieFileSystem : DAQ.Environment.FileSystem
	{
        public PixieFileSystem()
		{
			Paths.Add("mathPath", "c:/program files/wolfram research/mathematica/5.2/mathkernel.exe");
			Paths.Add("settingsPath","f:\\data\\settings\\");
			Paths.Add("scanMasterDataPath", "f:\\data\\general\\");
			Paths.Add("edmDataPath", "f:\\data\\sedm\\v3\\");
			Paths.Add("fakeData","f:\\data\\examples\\");
            Paths.Add("transferCavityData", "f:\\data\\transfer cavity\\");

			DataSearchPaths.Add(Paths["scanMasterDataPath"]);
			DataSearchPaths.Add(Paths["edmDataPath"]);
			
			SortDataByDate = true;
		}
	}
}
