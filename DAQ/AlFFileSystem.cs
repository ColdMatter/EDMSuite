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
			Paths.Add("MOTMasterDataPath", "C:\\Users\\alfultra\\OneDrive - Imperial College London\\Desktop\\Data\\mot_master_data\\");
			Paths.Add("scriptListPath", "C:\\EDMSuite\\AlFMOTMasterScripts");
			Paths.Add("daqDLLPath", "C:\\EDMSuite\\DAQ\\bin\\AlF\\daq.dll");
			Paths.Add("MOTMasterExePath", "C:EDMSuite\\bin\\AlF\\");
			Paths.Add("fakeData", "C:\\Users\\alfultra\\OneDrive - Imperial College London\\Desktop\\Data\\Examples\\");
			Paths.Add("scanMasterDataPath", "C:\\Users\\alfultra\\OneDrive - Imperial College London\\Desktop\\Data\\");

			DataSearchPaths.Add(Paths["scanMasterDataPath"]);
			DataSearchPaths.Add(Paths["fakeData"]);

			SortDataByDate = false;
		}
	}
}
