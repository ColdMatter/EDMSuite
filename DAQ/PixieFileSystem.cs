using System;
using System.Collections.Generic;
using System.Text;

namespace DAQ
{
    class PixieFileSystem : DAQ.Environment.FileSystem
	{
        public PixieFileSystem()
		{
			//Paths.Add("mathPath", "c:/program files/wolfram research/mathematica/10.4/mathkernel.exe");
			Paths.Add("settingsPath", "D:\\OneDrive - Imperial College London\\Data\\settings\\");
            Paths.Add("scanMasterDataPath", "D:\\OneDrive - Imperial College London\\Data\\general\\");
            Paths.Add("fakeData", "D:\\OneDrive - Imperial College London\\Data\\examples\\");
            Paths.Add("transferCavityData", "D:\\OneDrive - Imperial College London\\Data\\transfer cavity\\");
            Paths.Add("MOTMasterDataPath", "D:\\OneDrive - Imperial College London\\Data\\mot_master_data\\");
            Paths.Add("scriptListPath", "D:\\EDMSuite\\TweezerMOTMasterScripts");
            Paths.Add("daqDLLPath", "D:\\EDMSuite\\DAQ\\bin\\EDM\\daq.dll");
            Paths.Add("MOTMasterExePath", "D:\\EDMSuite\\MOTMaster\\bin\\EDM\\");
            Paths.Add("ExternalFilesPath", "D:\\Temp_Camera_Images\\");
            Paths.Add("HardwareClassPath", "D:\\EDMSuite\\DAQ\\PXIEDMHardware.cs");

			DataSearchPaths.Add(Paths["MOTMasterDataPath"]);
			
			SortDataByDate = true;
		}
	}
}
