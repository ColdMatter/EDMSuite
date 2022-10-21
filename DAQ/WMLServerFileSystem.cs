using System;

namespace DAQ.Environment
{
	/// <summary>
	/// 
	/// </summary>
	class WMLServerFileSystem : DAQ.Environment.FileSystem
	{
		public WMLServerFileSystem()
		{
			Paths.Add("fakeData", "C:\\Users\\cafmot\\Desktop\\Data\\examples\\");
			Paths.Add("scanMasterDataPath", "C:\\Users\\cafmot\\Desktop\\Data\\");

			DataSearchPaths.Add(Paths["scanMasterDataPath"]);
			DataSearchPaths.Add(Paths["fakeData"]);

			SortDataByDate = false;
		}
	}
}
