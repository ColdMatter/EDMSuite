using System;

namespace DAQ.Environment
{
	/// <summary>
	/// 
	/// </summary>
	public class PhkaraFileSystem : DAQ.Environment.FileSystem
	{
		public PhkaraFileSystem()
		{
            Paths.Add("settingsPath", "d:\\data\\settings\\");
			Paths.Add("scanMasterDataPath", "d:\\data\\general\\");
			Paths.Add("mathPath", "c:/program files/wolfram research/mathematica/7.0/mathkernel.exe");
			Paths.Add("fakeData","d:\\data\\examples\\");
            Paths.Add("edmDataPath", "d:\\data\\sedm\\v3\\");
			DataSearchPaths.Add(Paths["scanMasterDataPath"]);
            DataSearchPaths.Add(Paths["edmDataPath"]);
			SortDataByDate = false;
		}
	}
}
