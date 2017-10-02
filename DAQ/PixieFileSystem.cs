using System;
using System.Collections.Generic;
using System.Text;

namespace DAQ
{
    class PixieFileSystem : DAQ.Environment.FileSystem
	{
        public PixieFileSystem()
		{
			Paths.Add("mathPath", "c:/program files/wolfram research/mathematica/10.4/mathkernel.exe");
			Paths.Add("settingsPath","d:\\Box Sync\\EDM\\Data\\settings\\");
            Paths.Add("scanMasterDataPath", "d:\\Box Sync\\EDM\\Data\\general\\");
            Paths.Add("edmDataPath", "d:\\Box Sync\\EDM\\Data\\sedm\\v3\\");
            Paths.Add("fakeData", "d:\\Box Sync\\EDM\\Data\\examples\\");
            Paths.Add("transferCavityData", "d:\\Box Sync\\EDM\\Data\\transfer cavity\\");

			DataSearchPaths.Add(Paths["scanMasterDataPath"]);
			DataSearchPaths.Add(Paths["edmDataPath"]);
			
			SortDataByDate = true;
		}
	}
}
