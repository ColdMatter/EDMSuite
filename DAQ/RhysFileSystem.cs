using System;

namespace DAQ.Environment
{
	/// <summary>
	/// 
	/// </summary>
	public class RhysFileSystem : DAQ.Environment.FileSystem
	{
		public RhysFileSystem()
		{
			Paths.Add("settingsPath","c:\\experiment control\\");
			Paths.Add("scanMasterDataPath", "c:\\data\\");
			Paths.Add("mathPath", "c:/program files/wolfram research/mathematica/5.1/mathkernel.exe");
			Paths.Add("fakeData","c:\\data\\examples\\");

			DataSearchPaths.Add(Paths["scanMasterDataPath"]);

			SortDataByDate = false;
		}
	}
}
