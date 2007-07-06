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
			Paths.Add("settingsPath","d:\\experiment control\\");
			Paths.Add("scanMasterDataPath", "d:\\data\\");
			Paths.Add("mathPath", "c:/program files/wolfram research/mathematica/5.2/mathkernel.exe");
			Paths.Add("fakeData","d:\\data\\examples\\");

			DataSearchPaths.Add(Paths["scanMasterDataPath"]);

			SortDataByDate = false;
		}
	}
}
