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
			Paths.Add("CaFBECData", "E: \\Data");
			Paths.Add("fakeData", "C: \\ControlPrograms\\Examples");

<<<<<<< Updated upstream
			Paths.Add("MOTMasterDataPath", "E:\\Data\\mot_master_data\\");
			Paths.Add("daqDLLPath", "C:\\ControlPrograms\\EDMSuite\\DAQ\\bin\\CaFBEC\\daq.dll");
=======
			Paths.Add("MOTMasterDataPath", "C:\\Users\\cafmot\\OneDrive - Imperial College London\\cafbec\\mot_master_data\\");
			Paths.Add("daqDLLPath", "C:\\ControlPrograms\\EDMSuite\\DAQ\\bin\\CaF\\daq.dll");
>>>>>>> Stashed changes
			Paths.Add("MOTMasterExePath", "C:\\ControlPrograms\\EDMSuite\\MOTMaster\\bin\\CaFBEC\\");
			Paths.Add("ExternalFilesPath", "C:\\Users\\cafmot\\Documents\\Temp_camera_images\\");
			Paths.Add("HardwareClassPath", "C:\\ControlPrograms\\EDMSuite\\DAQ\\CaFBECHardware.cs");
			Paths.Add("scriptListPath", "C:\\ControlPrograms\\EDMSuite\\MoleculeMOTMasterScripts");

			DataSearchPaths.Add(Paths["CaFBECData"]);
			DataSearchPaths.Add(Paths["fakeData"]);

			SortDataByDate = true;
		}
	}
}
