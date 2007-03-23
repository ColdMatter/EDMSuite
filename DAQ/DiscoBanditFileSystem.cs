using System;

namespace DAQ.Environment
{
	/// <summary>
	/// 
	/// </summary>
	public class DiscoBanditFileSystem : DAQ.Environment.FileSystem
	{
		public DiscoBanditFileSystem()
		{
			Paths.Add("settingsPath","d:\\jony\\data\\settings\\");
			Paths.Add("scanMasterDataPath", "d:\\jony\\data\\general\\");
            Paths.Add("edmDataPath", "d:\\jony\\data\\sedm\\v3\\");
			Paths.Add("mathPath", "c:/program files/wolfram research/mathematica/5.2/mathkernel.exe");
            Paths.Add("fakeData", "d:\\jony\\data\\examples\\");

			DataSearchPaths.Add(Paths["scanMasterDataPath"]);
			DataSearchPaths.Add(Paths["edmDataPath"]);
				
			SortDataByDate = true;
		}
	}
}
