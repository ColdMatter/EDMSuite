using System;
using System.Collections.Generic;
using System.Text;

namespace DAQ
{
    class TCLEDMFileSystem : DAQ.Environment.FileSystem
	{
        public TCLEDMFileSystem()
		{
			//Install Mathematica and add this Paths.Add("mathPath", "c:/program files/wolfram research/mathematica/10.4/mathkernel.exe");
			Paths.Add("settingsPath","C:\\Users\\EDM\\Documents\\Data\\settings\\");
            Paths.Add("scanMasterDataPath", "C:\\Users\\EDM\\Documents\\Data\\general\\");
            Paths.Add("edmDataPath", "C:\\Users\\EDM\\Documents\\Data\\sedm\\v3\\");
            Paths.Add("fakeData", "C:\\Users\\EDM\\Documents\\Data\\examples\\");
            Paths.Add("transferCavityData", "C:\\Users\\EDM\\Documents\\Data\\transfer cavity\\");

			DataSearchPaths.Add(Paths["scanMasterDataPath"]);
			DataSearchPaths.Add(Paths["edmDataPath"]);
			
			SortDataByDate = true;
		}
	}
}
