using System;

namespace DAQ.Environment
{
	/// <summary>
	/// 
	/// </summary>
	public class GanymedeFileSystem : DAQ.Environment.FileSystem
	{
		public GanymedeFileSystem()
		{
			Paths.Add("settingsPath","d:\\Data\\Settings\\");
			Paths.Add("scanMasterDataPath", "d:\\Data\\LiH\\");
			Paths.Add("mathPath", "c:/program files/wolfram research/mathematica/5.2/mathkernel.exe");
			Paths.Add("fakeData","d:\\data\\examples\\");

			DataSearchPaths.Add(Paths["scanMasterDataPath"]);

			SortDataByDate = false;
		}
	}
}