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
			Paths.Add("settingsPath","d:\\data\\settings\\");
			Paths.Add("scanMasterDataPath", "d:\\data\\general\\");
			Paths.Add("edmDataPath", "d:\\data\\sedm\\v3\\");
			Paths.Add("fakeData","d:\\data\\examples\\");
            Paths.Add("transferCavityData", "d:\\data\\transfer cavity\\");

			DataSearchPaths.Add(Paths["scanMasterDataPath"]);
			DataSearchPaths.Add(Paths["edmDataPath"]);
			
			SortDataByDate = true;
		}
	}
}
