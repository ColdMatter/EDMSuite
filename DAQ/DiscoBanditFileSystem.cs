using System;

namespace DAQ.Environment
{
	/// <summary>
	/// 
	/// </summary>
	public class DiscoBanditFileSystem : DAQ.Environment.FileSystem
	{
		public DiscoBanditFileSystem()
		{
			Paths.Add("settingsPath","c:\\Files\\data\\settings\\");
			Paths.Add("scanMasterDataPath", "c:\\Files\\data\\general\\");
			Paths.Add("edmDataPath", "c:\\Files\\data\\sedm\\v3\\");
			Paths.Add("mathPath", "c:/program files/wolfram research/mathematica/5.2/mathkernel.exe");
			Paths.Add("fakeData","c:\\Files\\data\\examples\\");
				
			SortDataByDate = true;
		}
	}
}
