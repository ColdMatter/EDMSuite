using System;

namespace DAQ.Environment
{
	/// <summary>
	/// 
	/// </summary>
	public class SchnapsFileSystem : DAQ.Environment.FileSystem
	{
		public SchnapsFileSystem()
		{
			Paths.Add("settingsPath","d:\\experiment control\\");
			Paths.Add("scanMasterDataPath", "d:\\data\\supersonic");
			Paths.Add("mathPath", "c:/program files/wolfram research/mathematica/5.1/mathkernel.exe");
			Paths.Add("fakeData","d:\\data\\examples\\");
            Paths.Add("decelerationUtilitiesPath", "d:\\Tools\\");

			DataSearchPaths.Add(Paths["scanMasterDataPath"]);

			SortDataByDate = false;
		}
	}
}
