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
			Paths.Add("fakeData", "C:\\Users\\alfultra\\Desktop\\Data\\Examples\\");
			Paths.Add("scanMasterDataPath", "C:\\Users\\alfultra\\Desktop\\Data\\");

			DataSearchPaths.Add(Paths["scanMasterDataPath"]);
			DataSearchPaths.Add(Paths["fakeData"]);

			SortDataByDate = false;
		}
	}
}
