using System;

namespace DAQ.Environment
{
	/// <summary>
	/// 
	/// </summary>
	public class CrashFileSystem : DAQ.Environment.FileSystem
	{
		public CrashFileSystem()
		{
			Paths.Add("settingsPath","d:\\experiment control");
			Paths.Add("scanMasterDataPath", "d:\\data\\supersonic");
			Paths.Add("mathPath", "c:/program files/wolfram research/mathematica/5.1/mathkernel.exe");
			Paths.Add("fakeData","d:\\data\\examples\\");

			SortDataByDate = false;
		}
	}
}
