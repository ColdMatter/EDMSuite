using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ
{
	class CaFBECFileSystem : DAQ.Environment.FileSystem
	{
		public CaFBECFileSystem()
		{
			//Install Mathematica and add this Paths.Add("mathPath", "c:/program files/wolfram research/mathematica/10.4/mathkernel.exe");
			//Paths.Add("settingsPath","C:\\Users\\EDM\\Documents\\Data\\settings\\");
			//Paths.Add("scanMasterDataPath", "C:\\Users\\EDM\\Documents\\Data\\general\\");
			//Paths.Add("edmDataPath", "C:\\Users\\EDM\\Documents\\Data\\sedm\\v3\\");
			//Paths.Add("fakeData", "C:\\Users\\EDM\\Documents\\Data\\examples\\");
			Paths.Add("CaFBECData", "C: \\ Data");
			Paths.Add("fakeData", "C: \\ ControlPrograms \\ Examples");

			DataSearchPaths.Add(Paths["CaFBECData"]);
			DataSearchPaths.Add(Paths["fakeData"]);

			SortDataByDate = true;
		}
	}
}
