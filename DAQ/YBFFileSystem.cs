using System;

namespace DAQ.Environment
{
	/// <summary>
	/// 
	/// </summary>
	public class YBFFileSystem : DAQ.Environment.FileSystem
	{
		public YBFFileSystem()
		{
			Paths.Add("mathPath", "c:/program files/wolfram research/mathematica/5.1/mathkernel.exe");
			Paths.Add("settingsPath","c:\\EDMdata\\settings\\");
			Paths.Add("scanMasterDataPath", "c:\\EDMdata\\general\\");
			Paths.Add("edmDataPath", "c:\\data\\sedm\\v3\\");
			Paths.Add("fakeData","c:\\data\\examples\\");

			DataSearchPaths.Add(Paths["scanMasterDataPath"]);

			SortDataByDate = true;
		}
	}
}
