using System;

namespace DAQ.Environment
{
	/// <summary>
	/// 
	/// </summary>
	public class SealClubberFileSystem : DAQ.Environment.FileSystem
	{
        public SealClubberFileSystem()
		{
			Paths.Add("settingsPath","c:\\Users\\jony\\Files\\data\\settings\\");
            Paths.Add("scanMasterDataPath", "c:\\Users\\jony\\Files\\data\\general\\");
            Paths.Add("edmDataPath", "c:\\Users\\jony\\Files\\data\\sedm\\v3\\");
			Paths.Add("mathPath", "c:/program files/wolfram research/mathematica/8.0/mathkernel.exe");
            Paths.Add("fakeData", "c:\\Users\\jony\\Files\\data\\examples\\");

			DataSearchPaths.Add(Paths["scanMasterDataPath"]);
			DataSearchPaths.Add(Paths["edmDataPath"]);
				
			SortDataByDate = true;
		}
	}
}
