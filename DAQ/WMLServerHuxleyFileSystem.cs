using System;

namespace DAQ.Environment
{
	/// <summary>
	/// 
	/// </summary>
	class WMLServerHuxleyFileSystem : DAQ.Environment.FileSystem
	{
		public WMLServerHuxleyFileSystem()
		{
			Paths.Add("fakeData", "C:\\examples\\");
			Paths.Add("scanMasterDataPath", "C:\\Data\\");

			DataSearchPaths.Add(Paths["scanMasterDataPath"]);
			DataSearchPaths.Add(Paths["fakeData"]);

			SortDataByDate = false;
		}
	}
}
