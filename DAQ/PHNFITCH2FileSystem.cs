using System;

namespace DAQ.Environment
{
	/// <summary>
	/// 
	/// </summary>
    public class PHNFITCH2FileSystem : DAQ.Environment.FileSystem
	{
		public PHNFITCH2FileSystem()
		{
			Paths.Add("mathPath", "c:/program files/wolfram research/mathematica/5.2/mathkernel.exe");
            Paths.Add("settingsPath", "d:\\Box Sync\\CaF MOT\\ZeemanSisyphus\\settings\\");
			Paths.Add("scanMasterDataPath", "d:\\Box Sync\\CaF MOT\\ZeemanSisyphus\\data\\");//where scan master will save data
			//Paths.Add("edmDataPath", "d:\\data\\sedm\\v3\\");
            Paths.Add("fakeData", "d:\\Box Sync\\CaF MOT\\ZeemanSisyphus\\examples\\");

			DataSearchPaths.Add(Paths["scanMasterDataPath"]);
			//DataSearchPaths.Add(Paths["edmDataPath"]);
			
			SortDataByDate = true;
		}
	}
}
