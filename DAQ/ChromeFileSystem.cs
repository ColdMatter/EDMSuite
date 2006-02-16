using System;

namespace DAQ.Environment
{
	/// <summary>
	/// 
	/// </summary>
	public class ChromeFileSystem : DAQ.Environment.FileSystem
	{
		public ChromeFileSystem()
		{
			Paths.Add("mathPath", "c:/program files/wolfram research/mathematica/5.2/mathkernel.exe");
			Paths.Add("settingsPath","d:\\data\\settings\\");
			Paths.Add("scanMasterDataPath", "d:\\data\\general\\");
			Paths.Add("edmDataPath", "d:\\data\\sedm\\v3\\");
			Paths.Add("fakeData","d:\\data\\examples\\");

			DataSearchPaths.Add(Paths["scanMasterDataPath"]);
			DataSearchPaths.Add(Paths["edmDataPath"]);
			
			SortDataByDate = true;
		}
	}
}
