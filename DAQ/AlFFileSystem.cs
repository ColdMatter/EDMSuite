using System;

namespace DAQ.Environment
{
	/// <summary>
	/// 
	/// </summary>
	class AlFFileSystem : DAQ.Environment.FileSystem
	{
		public AlFFileSystem()
		{
			Paths.Add("fakeData", "C:\\Users\\AlF\\Desktop\\Data\\examples\\");
			Paths.Add("scanMasterDataPath", "C:\\Users\\AlF\\Desktop\\Data\\");

			DataSearchPaths.Add(Paths["scanMasterDataPath"]);
			DataSearchPaths.Add(Paths["fakeData"]);

			SortDataByDate = false;
		}
	}
}
