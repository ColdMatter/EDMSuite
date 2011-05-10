using System;

namespace DAQ.Environment
{
	/// <summary>
	/// 
	/// </summary>
	public class PHRHENDRIC0FileSystem : DAQ.Environment.FileSystem
	{
        public PHRHENDRIC0FileSystem()
		{
			Paths.Add("settingsPath","e:\\data\\settings\\");
			Paths.Add("scanMasterDataPath", "e:\\data\\");
			Paths.Add("mathPath", "c:/program files/wolfram research/mathematica/7.0/mathkernel.exe");
			Paths.Add("fakeData","e:\\data\\examples\\");

			DataSearchPaths.Add(Paths["scanMasterDataPath"]);

			SortDataByDate = false;
		}
	}
}
